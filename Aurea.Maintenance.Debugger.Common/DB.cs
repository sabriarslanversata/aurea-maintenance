using System.Text.RegularExpressions;
using Aurea.Logging;

namespace Aurea.Maintenance.Debugger.Common
{
    using System;
    using System.Collections.Generic;
    using System.Data;
    using System.Data.SqlClient;
    using System.IO;
    using System.Linq;
    using System.Text;
    using System.Xml;
    using System.Xml.Linq;
    using CIS.Framework.Data;
    using Aurea.Db;

    public static class DB
    {
        private static readonly Dictionary<string, string> InsertSqlCache = new Dictionary<string, string>();
        private static readonly Dictionary<string, string> UpdateSqlCache = new Dictionary<string, string>();
        private static readonly Dictionary<string, DataSet> MetaDataCache = new Dictionary<string, DataSet>();
        private static readonly List<string> ConstrainCheckDisabledTables = new List<string>();
        private static readonly List<string> IdentityInsertCheckClosedTables = new List<string>();

        public delegate void BeforeImportDelegate(string xmlPath, string connectionString);
        public delegate void AfterImportDelegate(string connectionString);

        public static T ReadSingleValue<T>(string commandText, string connectionString, int columnNumber = 0)
        {
            var reader = SqlHelper.ExecuteReader(connectionString, CommandType.Text, commandText);

            if (reader.HasRows && reader.Read())
            {
                return reader.GetFieldValue<T>(columnNumber);
            }

            return default(T);
        }

        public static DataRow ReadSingleRow(string commandText, string connectionString)
        {
            var dataSet = SqlHelper.ExecuteDataset(connectionString, CommandType.Text, commandText);

            if (dataSet.Tables.Count > 0 && dataSet.Tables[0].Rows.Count > 0)
            {
                return dataSet.Tables[0].Rows[0];
            }

            return null;
        }

        public static DataRowCollection ReadRows(string commandText, string connectionString, int tableNumber = 0)
        {
            var dataSet = SqlHelper.ExecuteDataset(connectionString, CommandType.Text, commandText);

            if (dataSet.Tables.Count >= tableNumber + 1 && dataSet.Tables[tableNumber].Rows.Count > 0)
            {
                return dataSet.Tables[tableNumber].Rows;
            }

            return null;
        }

        public static void ExecuteQuery(string commandText, string connectionString)
        {
            SqlHelper.ExecuteNonQuery(connectionString, CommandType.Text, commandText);
        }

        public static DataSet GetDataSets(string commandText, string connectionString)
        {
            return SqlHelper.ExecuteDataset(connectionString, CommandType.Text, commandText);
        }

        /// <summary>
        /// check if sql does not contaion any DML senctences, current limitation is insert into able check on same line, if there is line break it can't detect
        /// </summary>
        /// <param name="sql"></param>
        /// <returns>true if sql is safe</returns>
        public static bool IsSqlSafe(string sql)
        {
            var rm = new Regex(@"(?:\bupdate\b|\bdelete\b|\btruncate\b|\balter\b).+", RegexOptions.IgnoreCase);
            var rm2 = new Regex(@"(?:\b(insert)\b(.*?)\b(into)\b([^#@][a-zA-Z0-9])).+", RegexOptions.IgnoreCase);

            //private string _strRegex = @"(?i)(?s)\b(select)\b(.*?)\b(from)\b|\b(insert)\b(.*?)\b(into)\b|\b(update)\b(.*?)\b(set)\b|\b(delete)(.*?)\b(from)\b";
            var m1 = rm.Matches(sql).Count;
            var m2 = rm2.Matches(sql).Count;
            return m1 == 0 && m2 == 0;
        }

        public static void ExportResultsToFile(string commandText, string connectionString, string path, bool isXML)
        {
            if (!IsSqlSafe(commandText))
            {
                return;
            }

            var results = GetDataSets(commandText, connectionString);
            var sb = new StringBuilder();
            if (isXML)
            {
                sb.AppendLine("<?xml version=\"1.0\" encoding=\"utf-8\" ?>");
                sb.AppendLine("<root>");
            }
            foreach (DataTable table in results.Tables)
            {
                foreach (DataRow row in table.Rows)
                {
                    var res = row[0];
                    if (res is string s)
                    {
                        sb.AppendLine(s.Replace("paes_", "daes_").Replace("saes_", "daes_")); //always insert to daes
                    }
                }
            }
            if (isXML)
            {
                sb.AppendLine("</root>");
            }
            var xmlDoc = new XmlDocument();
            xmlDoc.LoadXml(sb.ToString());

            File.WriteAllText(path, XmlBeautify(xmlDoc));
        }

        private static string XmlBeautify(XmlDocument doc)
        {
            string strRetValue;
            Encoding enc = Encoding.UTF8;

            var xmlWriterSettings = new XmlWriterSettings
            {
                Encoding = enc,
                Indent = true,
                IndentChars = "    ",
                NewLineChars = "\r\n",
                NewLineHandling = NewLineHandling.Replace,
                ConformanceLevel = ConformanceLevel.Document
            };
            using (MemoryStream ms = new MemoryStream())
            {
                using (XmlWriter writer = XmlWriter.Create(ms, xmlWriterSettings))
                {
                    doc.Save(writer);
                    writer.Flush();
                    ms.Flush();
                    writer.Close();
                }
                ms.Position = 0;
                using (var sr = new StreamReader(ms, enc))
                {
                    strRetValue = sr.ReadToEnd();
                    sr.Close();
                }

                ms.Close();
            }
            return strRetValue;
        }

        public static void ImportQueryResultsFromProduction(string sql, string connectionString, string tempPath,
            BeforeImportDelegate beforeImport = null, AfterImportDelegate afterImport = null)
        {
            ImportRecordsFromQuery(
                sql,
                connectionString
                    .Replace("daes_", "paes_")
                    .Replace("SGISUSEUAV01.aesua.local", "SGISUSEPRV01.aesprod.local"),
                connectionString,
                tempPath,
                beforeImport,
                afterImport);
        }

        public static void ImportQueryResultsFromUa(string sql, string connectionString, string tempPath,
            BeforeImportDelegate beforeImport = null, AfterImportDelegate afterImport = null)
        {
            ImportRecordsFromQuery(
                sql,
                connectionString.Replace("daes_", "saes_"),
                connectionString,
                tempPath,
                beforeImport,
                afterImport);
        }

        public static void ImportRecordsFromQuery(string commandText, string sourceConnectionString,
            string destConnectionString, string tempPath, BeforeImportDelegate beforeImport = null,
            AfterImportDelegate afterImport = null)
        {
            var fileName = Path.ChangeExtension(Path.GetRandomFileName(), "xml");
            try
            {
                var xmlPath = Path.Combine(tempPath, fileName);
                ExportResultsToFile(commandText, sourceConnectionString, xmlPath, true);

                beforeImport?.Invoke(xmlPath, destConnectionString);

                ImportFiles(tempPath, Path.GetFileNameWithoutExtension(fileName), destConnectionString);

                afterImport?.Invoke(destConnectionString);
            }
            finally
            {
                if (File.Exists(fileName))
                {
                    try{File.Delete(fileName);}catch{}
                }
            }

        }

        public static IEnumerable<string> ReadAsLines(string filename)
        {
            using (var reader = new StreamReader(filename))
            {
                while (!reader.EndOfStream)
                {
                    yield return reader.ReadLine();
                }
            }
        }

        /// <summary>
        /// Imports Text and XML files found with given filter to DB
        /// <para>Supports updates only if there is primary key exists on Destination Table, otherwise it would update row via first column's value</para>
        /// <para>For Text Files, file content should be: first line TableName, second line ColumnNames, then data</para>
        /// </summary>
        /// <param name="path">root path of the start searching for files</param>
        /// <param name="filter">file name filter, ie. Ticket Number</param>
        /// <param name="connectionString">destination DB connection string</param>
        public static void ImportFiles(string path, string filter, string connectionString)
        {
            //import xml files, one xml file can contain multiple datasets, all dataset must be child of root element,
            // a xml result of a query can be obtained by adding 'FOR XML AUTO' end of select query
            Directory.EnumerateFiles(path, $"*{filter}*.xml", SearchOption.AllDirectories).ForEach(
                filename =>
                {
                    Import2DatabaseFromXMLFile(filename, connectionString);
                }
            );

            //import text files, one text file can contain only one dataset which written at first line of file
            Directory.EnumerateFiles(path, $"*{filter}*.txt", SearchOption.AllDirectories).ForEach(
                filename =>
                {
                    Import2DatabaseFromTextFile(filename, connectionString);
                }
            );

            if (ConstrainCheckDisabledTables.Count > 0)
            {
                var sqlBatch = new StringBuilder();
                ConstrainCheckDisabledTables.ForEach(tableName =>
                {
                    sqlBatch.AppendLine($"ALTER TABLE {tableName} WITH CHECK CHECK CONSTRAINT ALL");
                });

                SqlHelper.ExecuteNonQuery(connectionString, CommandType.Text, sqlBatch.ToString());
                ConstrainCheckDisabledTables.Clear();
            }
        }

        private static void Import2DatabaseFromTextFile(string fileName, string connectionString)
        {
            if (!File.Exists(fileName))
            {
                return;
            }

            var reader = ReadAsLines(fileName);
            if (reader.Count() > 2)//first line table name, second line headers
            {
                var sqlBatch = new StringBuilder();

                var tableName = reader.First();
                if (ConstrainCheckDisabledTables.IndexOf(tableName) < 0)
                {
                    sqlBatch.AppendLine($"ALTER TABLE {tableName} NOCHECK CONSTRAINT ALL");
                    ConstrainCheckDisabledTables.Add(tableName);
                }

                var headers = reader
                    .Skip(1)
                    .Take(1)
                    .SingleOrDefault()
                    .Split('\t')
                    .ToList();

                var records = reader.Skip(2).Where(line => !line.StartsWith("#"));
                if(!records.Any())
                    return;

                DataSet ds;
                if (!MetaDataCache.Any(x => x.Key.Equals(tableName, StringComparison.InvariantCultureIgnoreCase))) 
                {
                    var sql = $"SELECT * FROM {tableName} WHERE 1 = 0";//used for getting meta data only

                    SqlDataAdapter dataAdapter = new SqlDataAdapter(sql, connectionString);
                    ds = new DataSet(tableName);
                    dataAdapter.FillSchema(ds, SchemaType.Source, tableName);
                    dataAdapter.Fill(ds, tableName);
                    MetaDataCache.Add(tableName, ds);
                }
                else
                {
                    ds = MetaDataCache.SingleOrDefault(x => x.Key.Equals(tableName, StringComparison.InvariantCultureIgnoreCase)).Value;
                }

                var hasPrimaryKey = ds.Tables[0].PrimaryKey.Any();

                DataColumn primaryKeyColumn;

                if (ds.Tables[0].PrimaryKey.Any())
                {
                    primaryKeyColumn = ds.Tables[0].PrimaryKey[0];
                }
                else
                {
                    string primaryKeyName = headers.First();
                    primaryKeyColumn = (DataColumn)
                        from DataColumn c in ds.Tables[0].Columns
                        where c.ColumnName.Equals(primaryKeyName, StringComparison.InvariantCultureIgnoreCase)
                        select c;
                }
                        
                var isPrimaryKeysHasValue = hasPrimaryKey && headers.Any(x => x.Equals(primaryKeyColumn.ColumnName, StringComparison.InvariantCultureIgnoreCase));

                foreach (var record in records)
                {

                    var values = record.Split('\t').ToArray();
                    int primaryKeyIndex = headers.FindIndex(x => x.Equals(primaryKeyColumn.ColumnName, StringComparison.InvariantCultureIgnoreCase));
                    var primaryKeyValue = CreateFieldValueSql(primaryKeyColumn, values[primaryKeyIndex]);

                    //searching from firstColumn to check if record already present in DB
                    var checkSql = $"IF EXISTS(SELECT 1 FROM {tableName} WHERE {primaryKeyColumn.ColumnName} = {primaryKeyValue}) SELECT 1 ELSE SELECT 0";
                    var isExists = ReadSingleValue<int>(checkSql, connectionString) == 1;
                    if (!isExists)
                    {
                        sqlBatch.AppendLine(CreateInsertSql(tableName, ds.Tables[0].Columns, headers, values, isPrimaryKeysHasValue, hasPrimaryKey));
                    }
                    else
                    {
                        sqlBatch.AppendLine(CreateUpdateSql(tableName, ds.Tables[0].Columns, headers, values, primaryKeyColumn.ColumnName, primaryKeyValue));
                    }
                }

                if (IdentityInsertCheckClosedTables.Any(x => x.Equals(tableName, StringComparison.InvariantCultureIgnoreCase))) 
                {
                    sqlBatch.AppendLine($"SET IDENTITY_INSERT {tableName} OFF ");
                    IdentityInsertCheckClosedTables.RemoveAll(x => x.Equals(tableName, StringComparison.InvariantCultureIgnoreCase));
                }

                SqlHelper.ExecuteNonQuery(connectionString, CommandType.Text, sqlBatch.ToString());
            }
        }

        private static string parseXmlEncodedString(string xmlTableName)
        {
            if (string.IsNullOrEmpty(xmlTableName))
            {
                return string.Empty;
            }

            if (!xmlTableName.StartsWith("_"))
            {
                return xmlTableName;
            }

            string plainString = xmlTableName.Remove(0, 1).GetAfter("_");
            string encodedString = xmlTableName.Split('_')[1].Remove(0, 1);//xml encoded strings starts with _ and end with _
            
            string decodedChar = ((char)int.Parse(encodedString, System.Globalization.NumberStyles.HexNumber)).ToString();

            return $"{decodedChar}{plainString}";
        }

        private static readonly List<string> recordsExistsOnDatabase= new List<string>();

        private static void fetchRecordsFromDB(string fileName, string connectionString)
        {
            if (!File.Exists(fileName))
            {
                return;
            }
            var reader = XmlReader.Create(fileName);
            var dataHeaders = new List<string>();
            var dataValues = new List<string>();
            var sqlBatch = new StringBuilder();

            sqlBatch.AppendLine($"DECLARE @RecordStatus TABLE (TableName VARCHAR(255), RecordID INT, Status INT)");

            while (reader.Read())
            {
                if (reader.NodeType == XmlNodeType.Element)
                {
                    var tableName = parseXmlEncodedString(reader.Name);
                    if (tableName == "root")
                    {
                        continue;
                    }
                    if (XNode.ReadFrom(reader) is XElement el)
                    {
                        try
                        {
                            foreach (var xAttribute in el.Attributes())
                            {
                                dataHeaders.Add(parseXmlEncodedString(xAttribute.Name.LocalName));
                                dataValues.Add(xAttribute.Value);
                            }

                            DataSet ds;
                            if (!MetaDataCache.Any(x => x.Key.Equals(tableName, StringComparison.InvariantCultureIgnoreCase)))
                            {
                                var sql = $"SELECT * FROM {tableName} WHERE 1 = 0";//used for getting meta data only

                                SqlDataAdapter dataAdapter = new SqlDataAdapter(sql, connectionString);
                                ds = new DataSet(tableName);
                                dataAdapter.FillSchema(ds, SchemaType.Source, tableName);
                                dataAdapter.Fill(ds, tableName);
                                MetaDataCache.Add(tableName, ds);
                            }
                            else
                            {
                                ds = MetaDataCache.SingleOrDefault(x => x.Key.Equals(tableName, StringComparison.InvariantCultureIgnoreCase)).Value;
                            }

                            DataColumn primaryKeyColumn;

                            if (ds.Tables[0].PrimaryKey.Any())
                            {
                                primaryKeyColumn = ds.Tables[0].PrimaryKey[0];
                            }
                            else
                            {
                                string primaryKeyName = dataHeaders.First();
                                primaryKeyColumn = (DataColumn)(
                                    from DataColumn c in ds.Tables[0].Columns
                                    where c.ColumnName.Equals(primaryKeyName, StringComparison.InvariantCultureIgnoreCase)
                                    select c).SingleOrDefault();
                            }
                            
                            var primaryKeyIndex = dataHeaders.FindIndex(x => x.Equals(primaryKeyColumn.ColumnName, StringComparison.InvariantCultureIgnoreCase));
                            var primaryKeyValue = CreateFieldValueSql(primaryKeyColumn, dataValues[primaryKeyIndex]);
                            sqlBatch.AppendLine($"INSERT INTO @RecordStatus (TableName, RecordID, Status) SELECT '{tableName}', {primaryKeyValue}, CASE WHEN EXISTS(SELECT 1 FROM {tableName} WHERE [{primaryKeyColumn.ColumnName}] = {primaryKeyValue} ) THEN 1 ELSE  0 END");

                        }
                        finally
                        {
                            dataHeaders.Clear();
                            dataValues.Clear();
                        }
                    }
                }
            }
            sqlBatch.AppendLine("DELETE FROM @RecordStatus WHERE Status = 0");
            sqlBatch.AppendLine("SELECT '[' + TableName + ']_[' + CAST(RecordId as VARCHAR(16)) + ']' tableAndId FROM @RecordStatus");
            recordsExistsOnDatabase.Clear();
            var rows = ReadRows(sqlBatch.ToString(), connectionString);
            if (rows == null)
                return;
            foreach (DataRow row in rows)
            {
                recordsExistsOnDatabase.Add(row[0].ToString());
            }
        }

        private static void Import2DatabaseFromXMLFile(string fileName, string connectionString)
        {
            if (!File.Exists(fileName))
            {
                return;
            }
            fetchRecordsFromDB(fileName, connectionString);
            var reader = XmlReader.Create(fileName);
            var dataHeaders = new List<string>();
            var dataValues = new List<string>();
            var dbHeaders = new List<string>();
            var dbValues = new List<string>();
            var sqlBatch = new StringBuilder();
            while (reader.Read())
            {
                if (reader.NodeType == XmlNodeType.Element)
                {
                    var tableName = parseXmlEncodedString(reader.Name);
                    if (tableName == "root")
                    {
                        continue;
                    }

                    if (!ConstrainCheckDisabledTables.Any(x => x.Equals(tableName, StringComparison.InvariantCultureIgnoreCase))) 
                    {
                        sqlBatch.AppendLine($"ALTER TABLE {tableName} NOCHECK CONSTRAINT ALL");
                        ConstrainCheckDisabledTables.Add(tableName);
                    }

                    if (XNode.ReadFrom(reader) is XElement el)
                    {
                        try
                        {
                            foreach (var xAttribute in el.Attributes())
                            {
                                dataHeaders.Add(parseXmlEncodedString(xAttribute.Name.LocalName));
                                dataValues.Add(xAttribute.Value);
                            }

                            DataSet ds;
                            if (!MetaDataCache.Any(x=>x.Key.Equals(tableName, StringComparison.InvariantCultureIgnoreCase)))
                            {
                                var sql = $"SELECT * FROM {tableName} WHERE 1 = 0";//used for getting meta data only

                                SqlDataAdapter dataAdapter = new SqlDataAdapter(sql, connectionString);
                                ds = new DataSet(tableName);
                                dataAdapter.FillSchema(ds, SchemaType.Source, tableName);
                                dataAdapter.Fill(ds, tableName);
                                MetaDataCache.Add(tableName, ds);
                            }
                            else
                            {
                                ds = MetaDataCache.SingleOrDefault(x => x.Key.Equals(tableName, StringComparison.InvariantCultureIgnoreCase)).Value;
                            }

                            var hasPrimaryKey = ds.Tables[0].PrimaryKey.Any() && ds.Tables[0].PrimaryKey[0].AutoIncrement;

                            DataColumn primaryKeyColumn;

                            if (ds.Tables[0].PrimaryKey.Any())
                            {
                                primaryKeyColumn = ds.Tables[0].PrimaryKey[0];
                            }
                            else
                            {
                                string primaryKeyName = dataHeaders.First();
                                primaryKeyColumn = (DataColumn)
                                    from DataColumn c in ds.Tables[0].Columns
                                    where c.ColumnName.Equals(primaryKeyName, StringComparison.InvariantCultureIgnoreCase)
                                    select c;
                            }

                            var isPrimaryKeysHasValue = hasPrimaryKey && dataHeaders.Any(x => x.Equals(primaryKeyColumn.ColumnName, StringComparison.InvariantCultureIgnoreCase));
                            var primaryKeyIndex = dataHeaders.FindIndex(x =>x.Equals(primaryKeyColumn.ColumnName, StringComparison.InvariantCultureIgnoreCase));
                            var primaryKeyValue = CreateFieldValueSql(primaryKeyColumn, dataValues[primaryKeyIndex]);

                            foreach (DataColumn column in ds.Tables[0].Columns)
                            {
                                dbHeaders.Add(column.ColumnName);
                                if (dataHeaders.Any(x => x.Equals(column.ColumnName, StringComparison.InvariantCultureIgnoreCase))) 
                                {
                                    var dataColumnIndex = dataHeaders.FindIndex(x => x.Equals(column.ColumnName, StringComparison.InvariantCultureIgnoreCase));
                                    dbValues.Add(dataValues[dataColumnIndex]);
                                }
                                else
                                {
                                    dbValues.Add("NULL");
                                }
                            }


                            //searching from firstColumn to check if record already present in DB
                            //var checkSql = $"IF EXISTS(SELECT 1 FROM {tableName} WHERE [{primaryKeyColumn.ColumnName}] = {primaryKeyValue} ) SELECT 1 ELSE SELECT 0";
                            //var isExists = ReadSingleValue<int>(checkSql, connectionString) == 1;
                            var isExists = recordsExistsOnDatabase.FindIndex(x => x.Equals($"[{tableName}]_[{primaryKeyValue}]", StringComparison.InvariantCultureIgnoreCase)) >= 0;
                            if (!isExists) 
                            {
                                sqlBatch.AppendLine(CreateInsertSql(tableName, ds.Tables[0].Columns, dbHeaders, dbValues.ToArray(), isPrimaryKeysHasValue, hasPrimaryKey));
                            }
                            else
                            {
                                sqlBatch.AppendLine(CreateUpdateSql(tableName, ds.Tables[0].Columns, dbHeaders, dbValues.ToArray(), primaryKeyColumn.ColumnName, primaryKeyValue));
                            }

                            if (IdentityInsertCheckClosedTables.Any(x=>x.Equals(tableName, StringComparison.InvariantCultureIgnoreCase)))
                            {
                                sqlBatch.AppendLine($"SET IDENTITY_INSERT {tableName} OFF ");
                                IdentityInsertCheckClosedTables.RemoveAll(x => x.Equals(tableName, StringComparison.InvariantCultureIgnoreCase));
                            }
                        }
                        finally
                        {
                            dataHeaders.Clear();
                            dataValues.Clear();
                            dbHeaders.Clear();
                            dbValues.Clear();
                        }
                    }
                }
            }

            if (sqlBatch.Length > 0)
            {
                sqlBatch.Insert(0, "SET XACT_ABORT ON \r\n begin transaction\r\n ");
                sqlBatch.AppendLine("commit transaction");
                SqlHelper.ExecuteNonQuery(connectionString, CommandType.Text, sqlBatch.ToString());
            }
            recordsExistsOnDatabase.Clear();
        }

        private static string CreateFieldValueSql(DataColumn field, string value)
        {
            string toReturn = "NULL";
            if (value != "NULL")
            {
                switch (Type.GetTypeCode(field.DataType))
                {
                    case TypeCode.Char:
                    case TypeCode.String:
                    case TypeCode.DateTime:
                        toReturn = $"'{value.EscapeString()}'";
                        break;
                    case TypeCode.Single:
                    case TypeCode.Int16:
                    case TypeCode.Int32:
                    case TypeCode.Int64:
                    case TypeCode.UInt16:
                    case TypeCode.UInt32:
                    case TypeCode.UInt64:
                    case TypeCode.Decimal:
                    case TypeCode.Double:
                    case TypeCode.Boolean:
                        toReturn = value;
                        break;
                }
            }

            if (toReturn == "NULL" && !field.AllowDBNull)
            {
                switch (Type.GetTypeCode(field.DataType))
                {
                    case TypeCode.Char:
                    case TypeCode.String:
                        toReturn = "''";
                        break;
                    case TypeCode.Single:
                    case TypeCode.Int16:
                    case TypeCode.Int32:
                    case TypeCode.Int64:
                    case TypeCode.UInt16:
                    case TypeCode.UInt32:
                    case TypeCode.UInt64:
                    case TypeCode.Decimal:
                    case TypeCode.Double:
                    case TypeCode.Boolean:
                        toReturn = "0";
                        break;
                    case TypeCode.DateTime:
                        toReturn = $"'{DateTime.MinValue}'";
                        break;
                    case TypeCode.Byte:
                    case TypeCode.SByte:
                        toReturn = "0x";
                        break;
                }
            }
            return toReturn;
        }
        
        private static string CreateInsertSql(string tableName, DataColumnCollection columns, List<string> columnNames, string[] values, bool isPrimaryKeysHasValue, bool hasPrimaryKey)
        {
            var sql = new StringBuilder();
            if (hasPrimaryKey && isPrimaryKeysHasValue && !IdentityInsertCheckClosedTables.Any(x => x.Equals(tableName, StringComparison.InvariantCultureIgnoreCase))) 
            {
                sql.AppendLine($"SET IDENTITY_INSERT {tableName} ON ");
                IdentityInsertCheckClosedTables.Add(tableName);
            }

            string insertSqlHeader;
            if (!InsertSqlCache.Any(x => x.Key.Equals(tableName, StringComparison.InvariantCultureIgnoreCase)))
            {
                insertSqlHeader = $"INSERT INTO {tableName} ( [{string.Join("],[", columnNames)}] ) SELECT ";
                InsertSqlCache.Add(tableName, insertSqlHeader);
            }
            else
            {
                insertSqlHeader = InsertSqlCache.SingleOrDefault(x => x.Key.Equals(tableName, StringComparison.InvariantCultureIgnoreCase)).Value;
            }

            sql.AppendLine(insertSqlHeader);
            var valuesLine = new StringBuilder();
            foreach (DataColumn column in columns)
            {
                if (columnNames.Any(x => x.Equals(column.ColumnName, StringComparison.InvariantCultureIgnoreCase)))
                {
                    var colIndex = columnNames.FindIndex(x => x.Equals(column.ColumnName, StringComparison.InvariantCultureIgnoreCase));
                    valuesLine.Append($"{CreateFieldValueSql(column, values[colIndex])}, ");
                }
                else
                {
                    valuesLine.Append($"{CreateFieldValueSql(column, "NULL")}, ");
                }
            }
            sql.AppendLine(valuesLine.ToString().Remove(valuesLine.Length - 2, 2));


            return sql.ToString();
        }

        private static string CreateUpdateSql(string tableName, DataColumnCollection columns, List<string> columnNames, string[] values, string primaryKeyName, string primaryKeyValue)
        {
            var sql = new StringBuilder();
            string updateSqlHeader;
            if (!UpdateSqlCache.Any(x => x.Key.Equals(tableName, StringComparison.InvariantCultureIgnoreCase)))
            {
                updateSqlHeader = $"UPDATE {tableName} SET ";
                UpdateSqlCache.Add(tableName, updateSqlHeader);
            }
            else
            {
                updateSqlHeader = UpdateSqlCache.SingleOrDefault(x => x.Key.Equals(tableName, StringComparison.InvariantCultureIgnoreCase)).Value;
            }
            sql.AppendLine(updateSqlHeader);

            var updateSentence = new StringBuilder();
            foreach (DataColumn column in columns)
            {
                if (column.ColumnName != primaryKeyName && columnNames.Any(x => x.Equals(column.ColumnName, StringComparison.InvariantCultureIgnoreCase))) 
                {
                    var colIndex = columnNames.FindIndex(x => x.Equals(column.ColumnName, StringComparison.InvariantCultureIgnoreCase));
                    updateSentence.Append($"[{column.ColumnName}] = {CreateFieldValueSql(column, values[colIndex])}, ");
                }
                else if (column.ColumnName != primaryKeyName)
                {
                    updateSentence.Append($"[{column.ColumnName}] = {CreateFieldValueSql(column, "NULL")}, ");
                }
            }
            sql.AppendLine(updateSentence.ToString().Remove(updateSentence.Length - 2, 2));

            sql.AppendLine($"WHERE [{primaryKeyName}] = {primaryKeyValue} ");
            return sql.ToString();
        }

    }
}

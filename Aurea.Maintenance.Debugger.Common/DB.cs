using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using System.Xml.Linq;
using CIS.Framework.Data;

namespace Aurea.Maintenance.Debugger.Common
{
    public static class DB
    {
        public static T ReadSingleValue<T>(string sql, string connectionString, int columnNumber = 0)
        {
            var reader = SqlHelper.ExecuteReader(connectionString, CommandType.Text, sql);
            if (reader.HasRows && reader.Read())
            {
                return reader.GetFieldValue<T>(columnNumber);
            }
            return default(T);
        }

        public static DataRow ReadSingleRow(string sql, string connectionString)
        {
            var ds = SqlHelper.ExecuteDataset(connectionString, CommandType.Text, sql);
            if (ds.Tables.Count > 0 && ds.Tables[0].Rows.Count > 0)
            {
                return ds.Tables[0].Rows[0];
            }
            return null;
        }

        public static DataRowCollection ReadRows(string sql, string connectionString, int tableNumber = 0)
        {
            var ds = SqlHelper.ExecuteDataset(connectionString, CommandType.Text, sql);
            if (ds.Tables.Count >= tableNumber + 1 && ds.Tables[tableNumber].Rows.Count > 0)
            {
                return ds.Tables[tableNumber].Rows;
            }
            return null;
        }

        public static void ExecuteQuery(string sql, string connectionString)
        {
            SqlHelper.ExecuteNonQuery(connectionString, CommandType.Text, sql);
        }

        static IEnumerable<string> ReadAsLines(string filename)
        {
            using (var reader = new StreamReader(filename))
                while (!reader.EndOfStream)
                    yield return reader.ReadLine();
        }

        private static readonly Dictionary<string, string> insertSqlCache = new Dictionary<string, string>();
        private static readonly Dictionary<string, string> updateSqlCache = new Dictionary<string, string>();
        private static readonly Dictionary<string, DataSet> metaDataCache = new Dictionary<string, DataSet>();
        private static readonly List<string> constrainCheckDisabledTables = new List<string>();
        private static readonly List<string> identityInsertCheckClosedTables = new List<string>();

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

            if (constrainCheckDisabledTables.Count > 0)
            {
                var sqlBatch = new StringBuilder();
                constrainCheckDisabledTables.ForEach(tableName =>
                {
                    sqlBatch.AppendLine($"ALTER TABLE {tableName} WITH CHECK CHECK CONSTRAINT ALL");
                });

                SqlHelper.ExecuteNonQuery(connectionString, CommandType.Text, sqlBatch.ToString());
                constrainCheckDisabledTables.Clear();
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
                if (constrainCheckDisabledTables.IndexOf(tableName) < 0)
                {
                    sqlBatch.AppendLine($"ALTER TABLE {tableName} NOCHECK CONSTRAINT ALL");
                    constrainCheckDisabledTables.Add(tableName);
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
                if (!metaDataCache.Any(x => x.Key.Equals(tableName, StringComparison.InvariantCultureIgnoreCase))) 
                {
                    var sql = $"SELECT * FROM {tableName} WHERE 1 = 0";//used for getting meta data only

                    SqlDataAdapter dataAdapter = new SqlDataAdapter(sql, connectionString);
                    ds = new DataSet(tableName);
                    dataAdapter.FillSchema(ds, SchemaType.Source, tableName);
                    dataAdapter.Fill(ds, tableName);
                    metaDataCache.Add(tableName, ds);
                }
                else
                {
                    ds = metaDataCache.SingleOrDefault(x => x.Key.Equals(tableName, StringComparison.InvariantCultureIgnoreCase)).Value;
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

                if (identityInsertCheckClosedTables.Any(x => x.Equals(tableName, StringComparison.InvariantCultureIgnoreCase))) 
                {
                    sqlBatch.AppendLine($"SET IDENTITY_INSERT {tableName} OFF ");
                    identityInsertCheckClosedTables.RemoveAll(x => x.Equals(tableName, StringComparison.InvariantCultureIgnoreCase));
                }

                SqlHelper.ExecuteNonQuery(connectionString, CommandType.Text, sqlBatch.ToString());
            }
        }

        private static void Import2DatabaseFromXMLFile(string fileName, string connectionString)
        {
            if (!File.Exists(fileName))
            {
                return;
            }
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
                    var tableName = reader.Name;
                    if (tableName == "root")
                    {
                        continue;
                    }

                    if (!constrainCheckDisabledTables.Any(x => x.Equals(tableName, StringComparison.InvariantCultureIgnoreCase))) 
                    {
                        sqlBatch.AppendLine($"ALTER TABLE {tableName} NOCHECK CONSTRAINT ALL");
                        constrainCheckDisabledTables.Add(tableName);
                    }

                    if (XNode.ReadFrom(reader) is XElement el)
                    {
                        try
                        {
                            foreach (var xAttribute in el.Attributes())
                            {
                                dataHeaders.Add(xAttribute.Name.LocalName);
                                dataValues.Add(xAttribute.Value);
                            }

                            DataSet ds;
                            if (!metaDataCache.Any(x=>x.Key.Equals(tableName, StringComparison.InvariantCultureIgnoreCase)))
                            {
                                var sql = $"SELECT * FROM {tableName} WHERE 1 = 0";//used for getting meta data only

                                SqlDataAdapter dataAdapter = new SqlDataAdapter(sql, connectionString);
                                ds = new DataSet(tableName);
                                dataAdapter.FillSchema(ds, SchemaType.Source, tableName);
                                dataAdapter.Fill(ds, tableName);
                                metaDataCache.Add(tableName, ds);
                            }
                            else
                            {
                                ds = metaDataCache.SingleOrDefault(x => x.Key.Equals(tableName, StringComparison.InvariantCultureIgnoreCase)).Value;
                            }

                            var hasPrimaryKey = ds.Tables[0].PrimaryKey.Any();

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
                            var checkSql = $"IF EXISTS(SELECT 1 FROM {tableName} WHERE {primaryKeyColumn.ColumnName} = {primaryKeyValue}) SELECT 1 ELSE SELECT 0";
                            var isExists = ReadSingleValue<int>(checkSql, connectionString) == 1;
                            if (!isExists)
                            {
                                sqlBatch.AppendLine(CreateInsertSql(tableName, ds.Tables[0].Columns, dbHeaders, dbValues.ToArray(), isPrimaryKeysHasValue, hasPrimaryKey));
                            }
                            else
                            {
                                sqlBatch.AppendLine(CreateUpdateSql(tableName, ds.Tables[0].Columns, dbHeaders, dbValues.ToArray(), primaryKeyColumn.ColumnName, primaryKeyValue));
                            }

                            if (identityInsertCheckClosedTables.Any(x=>x.Equals(tableName, StringComparison.InvariantCultureIgnoreCase)))
                            {
                                sqlBatch.AppendLine($"SET IDENTITY_INSERT {tableName} OFF ");
                                identityInsertCheckClosedTables.RemoveAll(x => x.Equals(tableName, StringComparison.InvariantCultureIgnoreCase));
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
                SqlHelper.ExecuteNonQuery(connectionString, CommandType.Text, sqlBatch.ToString());
            }
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
                        toReturn = $"'{value}'";
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
            if (hasPrimaryKey && isPrimaryKeysHasValue && !identityInsertCheckClosedTables.Any(x => x.Equals(tableName, StringComparison.InvariantCultureIgnoreCase))) 
            {
                sql.AppendLine($"SET IDENTITY_INSERT {tableName} ON ");
                identityInsertCheckClosedTables.Add(tableName);
            }

            string insertSqlHeader;
            if (!insertSqlCache.Any(x => x.Key.Equals(tableName, StringComparison.InvariantCultureIgnoreCase)))
            {
                insertSqlHeader = $"INSERT INTO [{tableName}] ( [{string.Join("],[", columnNames)}] ) SELECT ";
                insertSqlCache.Add(tableName, insertSqlHeader);
            }
            else
            {
                insertSqlHeader = insertSqlCache.SingleOrDefault(x => x.Key.Equals(tableName, StringComparison.InvariantCultureIgnoreCase)).Value;
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
            if (!updateSqlCache.Any(x => x.Key.Equals(tableName, StringComparison.InvariantCultureIgnoreCase)))
            {
                updateSqlHeader = $"UPDATE [{tableName}] SET ";
                updateSqlCache.Add(tableName, updateSqlHeader);
            }
            else
            {
                updateSqlHeader = updateSqlCache.SingleOrDefault(x => x.Key.Equals(tableName, StringComparison.InvariantCultureIgnoreCase)).Value;
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

            sql.AppendLine($"WHERE {primaryKeyName} = {primaryKeyValue} ");
            return sql.ToString();
        }

    }
}

using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
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

        /// <summary>
        /// Imports data from File
        /// </summary>
        /// <param name="fileName"></param>
        /// <param name="connectionString"></param>
        public static void Import2DatabaseFromTextFile(string fileName, string connectionString, bool hasPrimaryKey = true, bool primaryKeysHasValue = true)
        {
            //TODO: create insert or update sql regarding datatype and existence
            if (File.Exists(fileName))
            {
                var reader = ReadAsLines(fileName);
                if (reader.Count() > 2)//first line table name, second line headers
                {
                    var tableName = reader.First();
                    var headerRow = reader
                        .Skip(1)
                        .Take(1);
                    var headers = new List<string>();
                    foreach (string r in headerRow)
                    {
                        headers = r.Split('\t').ToList();
                    }

                    var data = reader.Skip(2);
                    var primaryKeys = string.Join(",", data.Select((row) => row.Split('\t')[0]).ToArray());
                    var sql = $"SELECT * FROM {tableName} WHERE {headers.First().ToString()} IN ({primaryKeys})";

                    using (var dbConnection = new SqlConnection(connectionString))
                    {
                        dbConnection.Open();
                        //using (var dbTransaction = dbConnection.BeginTransaction())
                        {
                            try
                            {
                                SqlDataAdapter dataAdapter = new SqlDataAdapter(sql, connectionString);   
                                var ds = new DataSet(tableName);
                                dataAdapter.FillSchema(ds, SchemaType.Source, tableName);
                                dataAdapter.Fill(ds, tableName);
                                foreach (var record in data)
                                {
                                    var values = record.Split('\t').ToArray();

                                    //var checkSql = $"IF EXISTS(SELECT 1 FROM {tableName} WHERE {headers.First().ToString()}={values[0]}) select 1 else select 0";
                                    //var isExists = ReadSingleValue<int>(checkSql, connectionString) == 1;
                                    //if (!isExists)
                                    {
                                        var row = ds.Tables[0].Rows.Find(int.Parse(values[0]));
                                        var isNewRow = false;
                                        if (row == null)
                                        {
                                            row = ds.Tables[0].NewRow();
                                            isNewRow = true;
                                        }
                                        foreach (DataColumn col in ds.Tables[0].Columns)
                                        {
                                            var colIndex = headers.IndexOf(col.ColumnName);
                                            if ((isNewRow && colIndex >= 0) || (!isNewRow && colIndex >= 1)) 
                                            {
                                                var value = values[colIndex];
                                                if (value != "NULL")
                                                {
                                                    switch (Type.GetTypeCode(col.DataType))
                                                    {
                                                        case TypeCode.Boolean:
                                                            if (Boolean.TryParse(value, out var boolres))
                                                                row.SetField<Boolean>(col, boolres);
                                                            else row.SetField<Boolean>(col, false);
                                                            break;
                                                        case TypeCode.Char:
                                                        case TypeCode.String:
                                                            row.SetField<string>(col, value);
                                                            break;
                                                        case TypeCode.Single:
                                                            if (Single.TryParse(value, out var singleres))
                                                                row.SetField(col, singleres);
                                                            else row.SetField<Single>(col, 0);
                                                            break;
                                                        case TypeCode.Int16:
                                                            if (Int16.TryParse(value, out var int16res))
                                                                row.SetField(col, int16res);
                                                            else row.SetField<Int16>(col, 0);
                                                            break;
                                                        case TypeCode.Int32:
                                                            if (Int32.TryParse(value, out var int32res))
                                                                row.SetField(col, int32res);
                                                            else row.SetField<Int32>(col, 0);
                                                            break;
                                                        case TypeCode.Int64:
                                                            if (Int64.TryParse(value, out var int64res))
                                                                row.SetField(col, int64res);
                                                            else row.SetField<Int64>(col, 0);
                                                            break;
                                                        case TypeCode.UInt16:
                                                            if (Int16.TryParse(value, out var uint16res))
                                                                row.SetField(col, uint16res);
                                                            else row.SetField<UInt16>(col, 0);
                                                            break;
                                                        case TypeCode.UInt32:
                                                            if (Int32.TryParse(value, out var uint32res))
                                                                row.SetField(col, uint32res);
                                                            else row.SetField<UInt32>(col, 0);
                                                            break;
                                                        case TypeCode.UInt64:
                                                            if (Int64.TryParse(value, out var uint64res))
                                                                row.SetField(col, uint64res);
                                                            else row.SetField<UInt64>(col, 0);
                                                            break;
                                                        case TypeCode.Decimal:
                                                            if (Decimal.TryParse(value, out var decimalres))
                                                                row.SetField(col, decimalres);
                                                            else row.SetField<Decimal>(col, 0);
                                                            break;
                                                        case TypeCode.Double:
                                                            if (Double.TryParse(value, out var doubleres))
                                                                row.SetField(col, doubleres);
                                                            else row.SetField<Double>(col, 0);
                                                            break;
                                                        case TypeCode.DateTime:
                                                            if (DateTime.TryParse(value, out var dateTimeres))
                                                                row.SetField(col, dateTimeres);
                                                            else row.SetField<DateTime>(col, DateTime.MinValue);
                                                            break;
                                                    }
                                                }
                                            }
                                        }
                                        if (isNewRow)
                                            ds.Tables[0].Rows.Add(row);
                                    }
                                }
                                //if (ds.HasChanges())
                                //{
                                //    ds.AcceptChanges();
                                //}

                                var cmdBuilder = new SqlCommandBuilder(dataAdapter);
                                cmdBuilder.GetInsertCommand();
                                cmdBuilder.GetUpdateCommand();
                                
                                dataAdapter.InsertCommand = GenerateInsertCommand(tableName, ds.Tables[0].Columns, headers.ToArray(), primaryKeysHasValue);
                                dataAdapter.Update(ds, ds.Tables[0].TableName);
                            }
                            catch
                            {
                                //dbTransaction.Rollback();
                                throw;
                            }
                            //dbTransaction.Commit();
                        }
                        dbConnection.Close();
                    }
                }
            }
            //string[] lines = dataWithColHeaders.Replace("\r\n","\n").Replace("\r","\n").Split('\n');

        }

        private static SqlCommand CreateTextCommandWithParameters(DataColumnCollection columns)
        {
            var cmd = new SqlCommand
            {
                CommandType = CommandType.Text
            };
            foreach (DataColumn column in columns)
            {
                var parameter = cmd.CreateParameter();
                parameter.Direction = ParameterDirection.Input;
                parameter.DbType = GetDBType(column.DataType);
                parameter.ParameterName = $"@p{column.Ordinal}";
            }
            return cmd;
        }

        private static SqlCommand GenerateUpdateCommandSql(string tableName, DataColumnCollection columns, string[] columnNames, bool hasPrimaryKey)
        {
            var cmd = CreateTextCommandWithParameters(columns);


            int[] columnIndexes = columnNames.Select((v, i) => i).ToArray();
            string[] columnUpdateValues = columnNames.Select((v, i) => $"{i}").ToArray();

            var sql = new StringBuilder();
            
            sql.AppendLine($"UPDATE [{tableName}] SET ( [{string.Join("],[", columnNames)}] ) ");

            sql.AppendLine($"UPDATE [{tableName}] SET ( [{string.Join("],[", columnNames)}] ) ");

            sql.AppendLine($"SELECT @p{string.Join(",@p", columnIndexes)} ");

            if (primaryKeysHasValue)
                sql.AppendLine($"SET IDENTITY_INSERT {tableName} OFF ");

            cmd.CommandText = sql.ToString();
            return cmd;
        }

        private static SqlCommand GenerateInsertCommand(string tableName, DataColumnCollection columns, string[] columnNames, bool primaryKeysHasValue)
        {
            var cmd = CreateTextCommandWithParameters(columns);


            int[] columnIndexes;
            if(primaryKeysHasValue)
                columnIndexes = columnNames.Select((v, i) => i).ToArray();
            else
                columnIndexes = columnNames.Select((v, i) => i).Skip(1).ToArray();

            var sql = new StringBuilder();
            if (primaryKeysHasValue)
                sql.AppendLine($"SET IDENTITY_INSERT {tableName} ON ");

            sql.AppendLine($"INSERT INTO [{tableName}] ( [{string.Join("],[", columnNames)}] ) ");
            sql.AppendLine($"SELECT @p{string.Join(",@p", columnIndexes)} ");

            if (primaryKeysHasValue)
                sql.AppendLine($"SET IDENTITY_INSERT {tableName} OFF ");

            cmd.CommandText = sql.ToString();
            return cmd;
        }

        private static DbType GetDBType(System.Type theType)
        {
            var p1 = new SqlParameter();
            var tc = System.ComponentModel.TypeDescriptor.GetConverter(p1.DbType);
            if (tc.CanConvertFrom(theType))
            {
                p1.DbType = (DbType)tc.ConvertFrom(theType.Name);
            }
            else
            {
                //Try brute force
                try
                {
                    p1.DbType = (DbType)tc.ConvertFrom(theType.Name);
                }
                catch (Exception)
                {
                    //Do Nothing; will return NVarChar as default
                }
            }
            return p1.DbType;
        }

    }
}

using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CIS.BusinessEntity;
using CIS.Framework.Data;
using System.Diagnostics;

namespace Aurea.Maintenance.Debugger.Spark.Domain.Report
{
    public static class AgedARDetailReportDebugger
    {
        public static void ReportLoadStatistics(GlobalApplicationConfigurationDS.GlobalApplicationConfiguration applicationConfig, int timeOutMinutes, int times)
        {
            Stopwatch timePerQuery = new Stopwatch();

            SqlParameter[] arrParms = new SqlParameter[5];

            arrParms[0] = new SqlParameter("@Type", SqlDbType.Int);
            arrParms[0].Value = 1;
            arrParms[1] = new SqlParameter("@ExtendedCustomerInfoFlag", SqlDbType.Char, 1);
            arrParms[1].Value = "N";
            arrParms[2] = new SqlParameter("@DivisionCode", SqlDbType.VarChar);
            arrParms[2].Value = DBNull.Value;
            arrParms[3] = new SqlParameter("@CSPDUNSID", SqlDbType.Int);
            arrParms[3].Value = DBNull.Value;
            arrParms[4] = new SqlParameter("@LDCID", SqlDbType.Int);
            arrParms[4].Value = DBNull.Value;

            
            var dataSet = new DataSet();

            try
            {
                Console.Write($"Round-{timeOutMinutes}: ");
                timePerQuery.Start();
                    
                DataSet ds = SqlHelper.ExecuteDataset(
                    applicationConfig.ConnectionCsr,
                    CommandType.StoredProcedure,
                    "cspReportAgedARDetail",
                    60 * timeOutMinutes,
                    arrParms);

                timePerQuery.Stop();
                Console.Write($"Loaded {ds.Tables[0].Rows.Count} lines in approx. {timePerQuery.Elapsed.TotalSeconds} seconds.{Environment.NewLine}");
            }
            catch
            {
                timePerQuery.Stop();
                Console.Write($"--> Timeout in approx. {timePerQuery.Elapsed.TotalSeconds} seconds.{Environment.NewLine}");
            }
            finally
            {
                timePerQuery.Reset();
                dataSet.Reset();
                dataSet.Dispose();
                Console.WriteLine(new string('-', 20));
            }
            
        }
    }
}

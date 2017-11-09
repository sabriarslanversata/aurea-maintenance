using System;
using System.Collections;
using Aurea.Maintenance.Debugger.Common;
using System.Data.SqlClient;
using System.Data;

namespace Aurea.Maintenance.Debugger.Spark
{
    public class Program
    {
        public class MyMaintenance : CIS.Clients.Spark.Maintenance
        {
            public MyMaintenance(string connectionCsr, string connectionMarket, string connectionAdmin)
                : base(connectionCsr, connectionMarket, connectionAdmin)
            {
                //
            }

            public override void InitializeVariables(string maintenanceFunction)
            {
                _runHour = "*";
                _runDay = "*";
                _runDayOfWeek = "*";
                _isEnabled = true;
                SkipIsValidRuntimeVerification = true;
                _lastRunTime = DateTime.Now.AddYears(-1);
            }
        }

        private static CIS.BusinessEntity.GlobalApplicationConfigurationDS.GlobalApplicationConfiguration _clientConfiguration;

        public static void Main(string[] args)
        {
            _clientConfiguration = Utility.SetSecurity(Utility.BillingAdminDEV, Utility.Clients["SPK"]);
            // Set culture to en-EN to prevent string manipulation issues in base code
            Utility.SetThreadCulture("en-US");

            PrepareMockData();

            GenerateEvents();

            RestoreData2Original();
        }

        private static void GenerateEvents()
        {
            var maintenance = new MyMaintenance(_clientConfiguration.ConnectionCsr, _clientConfiguration.ConnectionMarket, Utility.BillingAdminDEV);
            //will create events for all configured eventtype on client
            //maintenance.GenerateEvents();

            //will create renewal letters
            maintenance.GenerateContractRenewalNoticeLetters();
            
            
            //will create email queue
            maintenance.QueueLettersForEmail();
        }

        private static void ProcessEvents()
        {
            var engine = new CIS.Engine.Event.Queue(Utility.BillingAdminDEV);
            engine.ProcessEventQueue(_clientConfiguration.ClientID, _clientConfiguration.ConnectionCsr, _clientConfiguration.ConnectionMarket, _clientConfiguration.ClientAbbreviation);
        }

        private static void PrepareMockData()
        {
            DB.ExecuteQuery("UPDATE config.LDCConfiguration SET SendRenewalNoticeOne = 0 WHERE LDCId IN (SELECT LDCId FROM LDC WHERE MarketID = 1)");
            DB.ExecuteQuery("DELETE FROM Letter WHERE CreateDate > DateAdd(dd,-2,GETDATE()) ");
            DB.ExecuteQuery("UPDATE Premise SET LDCId = 11 WHERE CustID = 1865754");
            DB.ExecuteQuery("UPDATE config.LDCConfiguration SET QueueLettersForEmail = 1 WHERE LDCId = 11");
            DB.ExecuteQuery("UPDATE Customer SET ContractEndDate = '2016-12-30' WHERE CustID = 1865754");
            DB.ExecuteQuery("DELETE FROM Letter WHERE LetterTypeId = 701 AND CustID = 1865754");
            DB.ExecuteQuery("UPDATE Contract SET EndDate = '2016-12-31' WHERE CustID = 1865754");
            DB.ExecuteQuery("UPDATE CustomerAdditionalInfo SET BillingTypeId = 2 WHERE CustID = 1865754");
            DB.ExecuteQuery("UPDATE Address SET Email = 'test@example.com' WHERE AddrID IN (SELECT MailAddrId FROM Customer WHERE CustID = 1865754)");
        }

        private static void RestoreData2Original()
        {
            DB.ExecuteQuery("UPDATE Address SET Email = NULL WHERE AddrID IN (SELECT MailAddrId FROM Customer WHERE CustID = 1865754)");
            DB.ExecuteQuery("UPDATE CustomerAdditionalInfo SET BillingTypeId = 1 WHERE CustID = 1865754");
            DB.ExecuteQuery("UPDATE Contract SET EndDate = '2017-11-30' WHERE CustID = 1865754");
            DB.ExecuteQuery("UPDATE Customer SET ContractEndDate = '2017-11-29' WHERE CustID = 1865754");
            DB.ExecuteQuery("UPDATE config.LDCConfiguration SET QueueLettersForEmail = 0 WHERE LDCId = 11");
            DB.ExecuteQuery("UPDATE Premise SET LDCId = 3 WHERE CustID = 1865754");
            DB.ExecuteQuery("UPDATE config.LDCConfiguration SET SendRenewalNoticeOne = 1 WHERE LDCId IN (SELECT LDCId FROM LDC WHERE MarketID = 1)");
        }

        public sealed class DB
        {
            public static void ExecuteQuery(string sql)
            {
                using (IDbConnection connection = new SqlConnection(_clientConfiguration.ConnectionCsr))
                {
                    connection.Open();
                    var cmd = connection.CreateCommand();
                    cmd.CommandType = CommandType.Text;
                    cmd.CommandText = sql;
                    cmd.CommandTimeout = 1000 * 60 * 5;
                    cmd.ExecuteNonQuery();
                    connection.Close();
                }
            }
        }
    }
}

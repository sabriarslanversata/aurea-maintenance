using System;
using System.Collections;
using Aurea.Maintenance.Debugger.Common;

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
            
            //generateEvents by DB
            GenerateEvents();

            ProcessEvents();
        }

        private static void GenerateEvents()
        {
            var maintenance = new MyMaintenance(_clientConfiguration.ConnectionCsr, _clientConfiguration.ConnectionMarket, Utility.BillingAdminDEV);
            //maintenance.GenerateEvents();
            //executin renewal letter event generation only 
            maintenance.GenerateContractRenewalNoticeLetters();
        }

        private static void ProcessEvents()
        {
            var engine = new CIS.Engine.Event.Queue(Utility.BillingAdminDEV);
            engine.ProcessEventQueue(_clientConfiguration.ClientID, _clientConfiguration.ConnectionCsr, _clientConfiguration.ConnectionMarket, _clientConfiguration.ClientAbbreviation);
        }
    }
}

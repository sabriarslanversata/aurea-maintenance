using System;
using System.Collections;
using System.Runtime.Remoting.Contexts;
using System.Threading;
using Aurea.Logging;
using Aurea.Maintenance.Debugger.Common;
using Aurea.TaskToaster;
using CIS.BusinessEntity;

namespace Aurea.Maintenance.Debugger.Texpo
{
    public class Program
    {
        private static GlobalApplicationConfigurationDS.GlobalApplicationConfiguration _clientConfiguration;

        private static TaskContext CreateContext()
        {
            return new TaskContext
            {
                EnvironmentId = _clientConfiguration.EnvironmentID, // Dev = 1, Qc = 2, Ua = 12, Production = 13
                BillingAdminConnection = Utility.BillingAdminDEV,
                Client = "Texpo",
                ClientName = "Texpo",
                Namespace = "Texpo",
                Abbreviation = "TXP",
                ClientId = 22,
                ClientConnection = _clientConfiguration.ConnectionCsr,
                MarketConnection = _clientConfiguration.ConnectionMarket,
                TDSPConnection = _clientConfiguration.ConnectionTdsp,
                TaxConnection = "",
                Logger = new ConsoleLogger()
            };
        }

        public static void Main(string[] args)
        {
            _clientConfiguration = Utility.SetSecurity(Utility.BillingAdminDEV, Utility.Clients["TXP"]);
            
            // Set culture to en-EN to prevent string manipulation issues in base code
            SetThreadCulture("en-US");

            GenerateSimpleMarketTransactionEvaluationEvents();
            ProcessEvents();
            /*
			CalculateConsumptionDueDatesTask myTask = new CalculateConsumptionDueDatesTask();

            myTask.Initialize(CreateContext());
            
            //execute Maintenance.CalculateConsumptionDueDates 
            myTask.Execute();
			*/
            var waitForLine = Console.ReadLine();
            Console.WriteLine($"readed {waitForLine}");
        }

        private static void GenerateSimpleMarketTransactionEvaluationEvents()
        {
            var gen = new CIS.Framework.Event.EventGenerator.SimpleMarketTransactionEvaluation(_clientConfiguration.ConnectionCsr, Utility.BillingAdminDEV);
            var hashTable = new Hashtable();

            if (gen.Generate(Utility.Clients["TXP"], hashTable))
            {
                Console.WriteLine("Events Generated");
            }
            else
            {
                Console.WriteLine("Events could not generated");
            }
        }

        private static void ProcessEvents()
        {
            var engine = new CIS.Engine.Event.Queue(Utility.BillingAdminDEV);
            engine.ProcessEventQueue(_clientConfiguration.ClientID, _clientConfiguration.ConnectionCsr, _clientConfiguration.ConnectionMarket, _clientConfiguration.ClientAbbreviation);
        }

        private static void SetThreadCulture(string culture)
        {
            Thread.CurrentThread.CurrentCulture = System.Globalization.CultureInfo.GetCultureInfo(culture);
            Thread.CurrentThread.CurrentUICulture = System.Globalization.CultureInfo.GetCultureInfo(culture);
        }
    }
}
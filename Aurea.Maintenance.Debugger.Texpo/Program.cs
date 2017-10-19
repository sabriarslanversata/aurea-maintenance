using System;
using System.Runtime.Remoting.Contexts;
using Aurea.Logging;
using Aurea.Maintenance.Debugger.Common;
using Aurea.TaskToaster;
using CIS.BusinessEntity;
using Aurea.TaskToaster.ProductTasks;


namespace Aurea.Maintenance.Debugger.Texpo
{
    public class Program
    {

        private static TaskContext CreateContext(
            GlobalApplicationConfigurationDS.GlobalApplicationConfiguration configuration)
        {
            return new TaskContext
            {
                EnvironmentId = configuration.EnvironmentID, // Dev = 1, Qc = 2, Ua = 12, Production = 13
                BillingAdminConnection = Utility.BillingAdminDEV,
                Client = "Texpo",
                ClientName = "Texpo",
                Namespace = "Texpo",
                Abbreviation = "TXP",
                ClientId = 22,
                ClientConnection = configuration.ConnectionCsr,
                MarketConnection = configuration.ConnectionMarket,
                TDSPConnection = configuration.ConnectionTdsp,
                TaxConnection = "",
                Logger = new ConsoleLogger()
            };
        }

        public static void Main(string[] args)
        {
            var clientConfiguration = Utility.SetSecurity(Utility.BillingAdminDEV, Utility.Clients["TXP"]);
            CalculateConsumptionDueDatesTask myTask = new CalculateConsumptionDueDatesTask();

            myTask.Initialize(CreateContext(clientConfiguration));
            
            //execute Maintenance.CalculateConsumptionDueDates 
            myTask.Execute();
            var waitForLine = Console.ReadLine();
            Console.WriteLine($"readed {waitForLine}");

        }
    }
}

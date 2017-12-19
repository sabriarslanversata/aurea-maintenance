

namespace Aurea.Maintenance.Debugger.Accent
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using System.Threading.Tasks;
    using Common;
    using Common.Models;
    using System.Security.Policy;
    using Aurea.IO;
    using Aurea.Logging;
    using CIS.BusinessEntity;
    using System.Transactions;

    public class Program
    {
        private static ClientEnvironmentConfiguration _clientConfig;
        private static GlobalApplicationConfigurationDS.GlobalApplicationConfiguration _appConfig;
        private static ILogger _logger = new Logger();

        static void Main(string[] args)
        {
            // Set client configuration and then the application configuration context.            
            _clientConfig = ClientConfiguration.GetClientConfiguration(Clients.Accent, Stages.Development, TransactionMode.Enlist);
            _appConfig = ClientConfiguration.SetConfigurationContext(_clientConfig);

            TransactionManager.DistributedTransactionStarted += delegate
                (object sender, TransactionEventArgs e)
            {
                _logger.Info("Distributed Transaction Started");
            };

            simulateEnrollmentViaWS();

            _logger.Info("Debug session has ended");
            Console.ReadLine();
        }

        private static void simulateEnrollmentViaWS()
        {
            CopyProductAndRate();
            CopyEnrollCustomer();
            Execute814Import();
            GenerateEvents();
            ProcessEvents();
            MakeCTRAccepted();
            GenerateEvents();
            ProcessEvents();
        }

        private static void CopyEnrollCustomer()
        {
            
        }

        private static void CopyProductAndRate()
        {
            
        }

        private static void Execute814Import()
        {
            ExecuteTaskToasterTask();
        }

        private static void MakeCTRAccepted()
        {
            
        }

        private static void GenerateEvents()
        {
            
        }

        private static void ProcessEvents()
        {
            
        }

        private static void ExecuteTaskToasterTask()
        {
            
        }


    }
}

namespace Aurea.Maintenance.Debugger.Spark
{
    using System;
    using Aurea.Maintenance.Debugger.Common;
    using Aurea.Maintenance.Debugger.Common.Models;

    public class Program
    {
        public static void Main(string[] args)
        {
            // Set client configuration and then the application configuration context.            
            var clientConfig = ClientConfiguration.GetClientConfiguration(Clients.Spark, Stages.UserAcceptance);
            var applicationConfig = ClientConfiguration.SetConfigurationContext(clientConfig);

            // Call debugger method

            // Await exiting the application for debugging purposes.
            Console.ReadLine();
        }
    }
}

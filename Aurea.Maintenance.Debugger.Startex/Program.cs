namespace Aurea.Maintenance.Debugger.Spark
{
    using System;
    using System.Diagnostics;
    using Aurea.Maintenance.Debugger.Common;
    using Aurea.Maintenance.Debugger.Common.Models;

    public class Program
    {
        public static void Main(string[] args)
        {
            // Set client configuration and then the application configuration context.            
            var clientConfig = ClientConfiguration.GetClientConfiguration(Clients.StarTex, Stages.Development);
            var applicationConfig = ClientConfiguration.SetConfigurationContext(clientConfig);

            // Call debugger method
            InvoiceDebugger.InvoiceGeneration(applicationConfig, clientConfig);

            // Await exiting the application for debugging purposes. Set break and await exit to alert debugger!
#if DEBUG
            if (Debugger.IsAttached)
            {
                Debugger.Break();
            }
#endif
        }
    }
}

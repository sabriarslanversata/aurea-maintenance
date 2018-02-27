namespace Aurea.Maintenance.Debugger.Spark
{
    using System.Diagnostics;
    using Aurea.Maintenance.Debugger.Common;
    using Aurea.Maintenance.Debugger.Common.Models;

    public static class Program
    {
        public static void Main()
        {
            var clientConfig = ClientConfiguration.GetClientConfiguration(Clients.StarTex, Stages.Development);
            var applicationConfig = ClientConfiguration.SetConfigurationContext(clientConfig);

            InvoiceDebugger.InvoiceXmlGeneration(applicationConfig, clientConfig, 11885141);

#if DEBUG
            if (Debugger.IsAttached)
            {
                Debugger.Break();
            }
#endif
        }
    }
}

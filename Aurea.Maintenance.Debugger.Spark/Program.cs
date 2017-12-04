namespace Aurea.Maintenance.Debugger.Spark
{
    using System;
    using Aurea.Maintenance.Debugger.Common;
    using Aurea.Maintenance.Debugger.Common.Models;
    using Aurea.Maintenance.Debugger.Spark.Domain.Report;

    public class Program
    {
        public static void Main(string[] args)
        {
            // Set client configuration and then the application configuration context.            
            var clientConfig = ClientConfiguration.GetClientConfiguration(Clients.Spark, Stages.Production);
            var applicationConfig = ClientConfiguration.SetConfigurationContext(clientConfig);

            // Call debugger method
            Console.WriteLine($"===============================");
            Console.WriteLine($"= Report Test Started");
            Console.WriteLine($"= Timeout: 1 to 22 mins");
            Console.WriteLine($"= Rounds: 22 times");
            Console.WriteLine($"===============================");
            
                AgedARDetailReportDebugger.ReportLoadStatistics(applicationConfig, 1, 1);
            AgedARDetailReportDebugger.ReportLoadStatistics(applicationConfig, 2, 1);
            AgedARDetailReportDebugger.ReportLoadStatistics(applicationConfig, 3, 1);
            AgedARDetailReportDebugger.ReportLoadStatistics(applicationConfig, 4, 1);
            AgedARDetailReportDebugger.ReportLoadStatistics(applicationConfig, 5, 1);
            AgedARDetailReportDebugger.ReportLoadStatistics(applicationConfig, 6, 1);
            AgedARDetailReportDebugger.ReportLoadStatistics(applicationConfig, 7, 1);
            AgedARDetailReportDebugger.ReportLoadStatistics(applicationConfig, 8, 1);
            AgedARDetailReportDebugger.ReportLoadStatistics(applicationConfig, 9, 1);
            AgedARDetailReportDebugger.ReportLoadStatistics(applicationConfig, 10, 1);
            AgedARDetailReportDebugger.ReportLoadStatistics(applicationConfig, 11, 1);
            AgedARDetailReportDebugger.ReportLoadStatistics(applicationConfig, 12, 1);
            AgedARDetailReportDebugger.ReportLoadStatistics(applicationConfig, 13, 1);
            AgedARDetailReportDebugger.ReportLoadStatistics(applicationConfig, 14, 1);
            AgedARDetailReportDebugger.ReportLoadStatistics(applicationConfig, 15, 1);
            AgedARDetailReportDebugger.ReportLoadStatistics(applicationConfig, 16, 1);
            AgedARDetailReportDebugger.ReportLoadStatistics(applicationConfig, 17, 1);
            AgedARDetailReportDebugger.ReportLoadStatistics(applicationConfig, 18, 1);
            AgedARDetailReportDebugger.ReportLoadStatistics(applicationConfig, 19, 1);
            AgedARDetailReportDebugger.ReportLoadStatistics(applicationConfig, 20, 1);
            AgedARDetailReportDebugger.ReportLoadStatistics(applicationConfig, 21, 1);
            AgedARDetailReportDebugger.ReportLoadStatistics(applicationConfig, 22, 1);
            AgedARDetailReportDebugger.ReportLoadStatistics(applicationConfig, 23, 1);
            AgedARDetailReportDebugger.ReportLoadStatistics(applicationConfig, 24, 1);
            AgedARDetailReportDebugger.ReportLoadStatistics(applicationConfig, 25, 1);
            AgedARDetailReportDebugger.ReportLoadStatistics(applicationConfig, 26, 1);
            AgedARDetailReportDebugger.ReportLoadStatistics(applicationConfig, 27, 1);
            AgedARDetailReportDebugger.ReportLoadStatistics(applicationConfig, 28, 1);
            AgedARDetailReportDebugger.ReportLoadStatistics(applicationConfig, 29, 1);
            AgedARDetailReportDebugger.ReportLoadStatistics(applicationConfig, 30, 1);
            AgedARDetailReportDebugger.ReportLoadStatistics(applicationConfig, 31, 1);
            AgedARDetailReportDebugger.ReportLoadStatistics(applicationConfig, 32, 1);
            AgedARDetailReportDebugger.ReportLoadStatistics(applicationConfig, 33, 1);
            AgedARDetailReportDebugger.ReportLoadStatistics(applicationConfig, 34, 1);
            AgedARDetailReportDebugger.ReportLoadStatistics(applicationConfig, 35, 1);
            AgedARDetailReportDebugger.ReportLoadStatistics(applicationConfig, 36, 1);
            AgedARDetailReportDebugger.ReportLoadStatistics(applicationConfig, 37, 1);
            AgedARDetailReportDebugger.ReportLoadStatistics(applicationConfig, 38, 1);
            AgedARDetailReportDebugger.ReportLoadStatistics(applicationConfig, 39, 1);
            AgedARDetailReportDebugger.ReportLoadStatistics(applicationConfig, 40, 1);



            // Await exiting the application for debugging purposes.
            Console.ReadLine();
        }
    }
}

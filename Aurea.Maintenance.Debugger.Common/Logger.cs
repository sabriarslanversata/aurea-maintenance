namespace Aurea.Maintenance.Debugger.Common
{
    using System;
    using Aurea.Logging;

    public class Logger : ILogger
    {
        public void Log(LogLevel type, string message, long? messageId = null)
        {
            Console.WriteLine(message);
        }

        internal static void Error(Exception ex, string v)
        {
            Console.WriteLine($"{v} : \r\n{ex.Message}");
        }
    }
}

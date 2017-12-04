

namespace Aurea.Maintenance.Debugger.Common
{
    using System;
    using Aurea.Logging;
    using System.Diagnostics;

    public class Logger : ILogger
    {
        public void Log(LogLevel type, string message, long? messageId = null)
        {
            string logMessage = $"{DateTime.Now.ToString("s")} {type} - {message}";
            Console.WriteLine(logMessage);
            Debug.WriteLine(logMessage);
            
        }

        internal static void Error(Exception ex, string v)
        {
            string logMessage = $"{DateTime.Now.ToString("s")} Error {v} : \r\n{ex.Message}";
            Console.WriteLine(logMessage);
            Debug.WriteLine(logMessage);
        }
    }
}

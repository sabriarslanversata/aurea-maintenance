namespace Aurea.Maintenance.Debugger.Common
{
    using System;
    using Aurea.Logging;
    using System.Diagnostics;

    public class Logger : ILogger
    {
        public void Log(LogLevel type, string message, long? messageId = null)
        {
            Console.WriteLine($"{DateTime.Now.ToString("s")} {type} - {message}");
            Debug.WriteLine($"{type} - {message}");            
        }

        internal static void Error(Exception ex, string v)
        {
            Console.WriteLine($"{DateTime.Now.ToString("s")} Error {v} : \r\n{ex.Message}");
            Debug.WriteLine($"Error {v} : \r\n{ex.Message}");
        }
    }
}

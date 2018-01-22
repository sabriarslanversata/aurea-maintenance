using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using Aurea.Maintenance.Debugger.Common;
using System.Net.Http;
using Aurea.Logging;

namespace Aurea.Maintenance.CashPCI
{
    class Program
    {
        private static readonly string cashDBConnectionString = @"Server=.\SqlExpress;Trusted_Connection=True;initial catalog=CASH";
        
        private static readonly ILogger _logger = new Logger();
        private static Dictionary<string, string> _payload = new Dictionary<string, string>();
        static void Main(string[] args)
        {
            CreatePCIInvoker();

            Console.WriteLine("press return to exit");
            Console.ReadLine();
        }

        private static void CreatePCIInvoker()
        {
            var resp = "Y";
            while (resp.ToUpper() == "Y")
            {
                testPCIConcurrency();
                Console.WriteLine("do you want to try again? (Y/N)");
                resp = Console.ReadLine().Trim();
            }
        }

        private static void testPCIConcurrency()
        {
            _logger.Info("deleting customers");
            DB.ExecuteQuery("delete from cash.customer", cashDBConnectionString);
            _logger.Info("invoking HttpPost actions in parallel");
            if (_payload.Count == 0)
            {
                _payload.Add("application", "Maintenance.Test");
                _payload.Add("clientId", "");
                _payload.Add("customerId", "");
                _payload.Add("originalUrl", "");
                _payload.Add("username", "");
                _payload.Add("token", "");
                _payload.Add("tokenDetail", "");
            }
            Parallel.Invoke(InvokePaymetAction, InvokePaymetAction, InvokePaymetAction, InvokePaymetAction, InvokePaymetAction);
        }

        private static async void InvokePaymetAction()
        {
            _logger.Info($"invoke started {System.Threading.Thread.CurrentThread.ManagedThreadId}");
            
            var client = new HttpClient();
            var content = new FormUrlEncodedContent(_payload);
            
            var response = await client.PostAsync("url", content);
            var responseString = await response.Content.ReadAsStringAsync();
            _logger.Info($"response returned of {responseString.Length} bytes from {System.Threading.Thread.CurrentThread.ManagedThreadId}");
        }

        private static void InvokePaymentActionPure()
        {
            _logger.Info($"InvokePaymentActionPure started {System.Threading.Thread.CurrentThread.ManagedThreadId}");
            var request = (HttpWebRequest)WebRequest.Create("url");

            var postData = string.Empty;
            _payload.ForEach((p) =>
            {
                postData += $"{p.Key}={p.Value}&";
            });
            postData = postData.Remove(postData.Length - 1, 1);
            var data = Encoding.ASCII.GetBytes(postData);

            request.Method = "POST";
            request.ContentType = "application/x-www-form-urlencoded";
            request.ContentLength = data.Length;

            using (var stream = request.GetRequestStream())
            {
                stream.Write(data, 0, data.Length);
            }

            var response = (HttpWebResponse)request.GetResponse();

            var responseString = new StreamReader(response.GetResponseStream()).ReadToEnd();
            _logger.Info($"response returned of {responseString.Length} bytes from {System.Threading.Thread.CurrentThread.ManagedThreadId}");
        }
    }
}

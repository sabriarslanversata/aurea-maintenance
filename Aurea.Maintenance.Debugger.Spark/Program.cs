using Aurea.Maintenance.Debugger.Common;

namespace Aurea.Maintenance.Debugger.Spark
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var clientConfiguration = Utility.SetSecurity(Utility.BillingAdminDEV, Utility.Clients["SPK"]);
        }
    }
}

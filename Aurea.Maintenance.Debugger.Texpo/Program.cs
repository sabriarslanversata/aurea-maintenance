using Aurea.Maintenance.Debugger.Common;

namespace Aurea.Maintenance.Debugger.Texpo
{
    class Program
    {

        static void Main(string[] args)
        {
            var clientConfiguration = Utility.SetSecurity(Utility.BillingAdminDEV, Utility.Clients["TXP"]);
        }
    }
}

using CIS.BusinessComponent;
using CIS.BusinessEntity;
using CIS.Framework.Security;
using CIS.Import.Billing;
using System.Collections.Generic;

namespace Aurea.Maintenance.Debugger.Texpo
{
    class Program
    {
        private const string BillingAdminUA = "Server=SGISUSEUAV01.aesua.local,24955;Initial Catalog=saes_BillingAdmin;Trusted_Connection=Yes";
        private const string BillingAdminDEV = "Server=SGISUSEUAV01.aesua.local,24955;Initial Catalog=daes_BillingAdmin;Trusted_Connection=Yes";
        private static readonly Dictionary<string, int> Clients = new Dictionary<string, int>
        {
            { "TXP", 22 },
            { "SPK", 48 }
        };

        static void Main(string[] args)
        {
            var clientConfiguration = SetSecurity(BillingAdminDEV, Clients["TXP"]);

            Tdsp tdspQueue = new Tdsp(clientConfiguration.ConnectionMarket, clientConfiguration.ConnectionCsr, BillingAdminDEV, clientConfiguration.ClientAbbreviation);
            tdspQueue.Import();
            
            

        }

        private static GlobalApplicationConfigurationDS.GlobalApplicationConfiguration SetSecurity(string billingConection, int clientId)
        {
            var clientConfiguration = GlobalApplicationConfigurationBC.Load(billingConection, clientId);
            SecurityManager.SetSecurityContext(clientConfiguration, clientId);

            return clientConfiguration;
        }
    }
}

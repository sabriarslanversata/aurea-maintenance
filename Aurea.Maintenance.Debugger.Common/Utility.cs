using CIS.BusinessComponent;
using CIS.BusinessEntity;
using CIS.Framework.Security;
using System.Collections.Generic;
using System.Configuration;

namespace Aurea.Maintenance.Debugger.Common
{
    public class Utility
    {
        // ReSharper disable once InconsistentNaming
        public const string BillingAdminUA = "Server=SGISUSEUAV01.aesua.local,24955;Initial Catalog=saes_BillingAdmin;Trusted_Connection=Yes";
        // ReSharper disable once InconsistentNaming
        public const string BillingAdminDEV = "Server=SGISUSEUAV01.aesua.local,24955;Initial Catalog=daes_BillingAdmin;Trusted_Connection=Yes";

        public static readonly Dictionary<string, int> Clients = new Dictionary<string, int>
        {
            {"TXP", 22},
            {"SPK", 48},
            {"SGE", 45}
        };

        public static GlobalApplicationConfigurationDS.GlobalApplicationConfiguration SetSecurity(string billingConection, int clientId)
        {
            var clientConfiguration = GlobalApplicationConfigurationBC.Load(billingConection, clientId);

            ConfigurationManager.AppSettings["Connection.BillingAdministration"] = billingConection;
            ConfigurationManager.AppSettings["Connection.Tdsp"] = clientConfiguration.ConnectionTdsp;

            SecurityManager.SetSecurityContext(clientConfiguration, clientId);
            return clientConfiguration;
        }
    }
}

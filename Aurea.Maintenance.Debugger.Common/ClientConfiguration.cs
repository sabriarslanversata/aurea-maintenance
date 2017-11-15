namespace Aurea.Maintenance.Debugger.Common
{
    using System.IO;
    using System.Collections.Generic;
    using System.Configuration;
    using System.Globalization;
    using System.Reflection;
    using System.Threading;
    using CIS.BusinessComponent;
    using CIS.BusinessEntity;
    using CIS.Framework.Security;
    using Aurea.Maintenance.Debugger.Common.Models;

    public static class ClientConfiguration
    {
        public static ClientEnvironmentConfiguration GetClientConfiguration(Clients client, Stages stage)
        {
            return new ClientEnvironmentConfiguration(client, stage);
        }

        public static GlobalApplicationConfigurationDS.GlobalApplicationConfiguration SetConfigurationContext(ClientEnvironmentConfiguration config)
        {
            var applicationConfiguration = GlobalApplicationConfigurationBC.Load(config.ConnectionBillingAdmin, config.ClientId);
            SecurityManager.SetSecurityContext(applicationConfiguration, 0, config.ConnectionBillingAdmin, string.Empty, string.Empty, string.Empty);

            RewriteAppSettings(new Dictionary<string, string>
            {
                {"AdminConnectionString", config.ConnectionBillingAdmin},
                {"Connection.BillingAdministration", config.ConnectionBillingAdmin},
                {"Connection.BillingAdministrationMaster", config.ConnectionBillingAdmin},
                {"Connection.BillingAdmin", config.ConnectionBillingAdmin},
                {"BillingAdminDbConnection", config.ConnectionBillingAdmin},
                {"conBillingAdminString", config.ConnectionBillingAdmin},
                {"BillingAdmin", config.ConnectionBillingAdmin}
            });

            // Set culture to en-EN to prevent string manipulation issues in base code
            var culture = "en-US";
            var cultureInfo = CultureInfo.GetCultureInfo(culture);
            Thread.CurrentThread.CurrentCulture = cultureInfo;
            Thread.CurrentThread.CurrentUICulture = cultureInfo;

            return applicationConfiguration;
        }

        private static void RewriteAppSettings(Dictionary<string, string> configDictionary)
        {
            var executingAssembly = Assembly.GetExecutingAssembly();
            var appPath = Path.GetDirectoryName(executingAssembly.Location);
            var appConfigFile = Path.Combine(appPath, executingAssembly.ManifestModule.Name + ".config");
            var configFileMap = new ExeConfigurationFileMap { ExeConfigFilename = appConfigFile };
            var config = ConfigurationManager.OpenMappedExeConfiguration(configFileMap, ConfigurationUserLevel.None);

            foreach (var configLine in configDictionary)
            {
                if (config.AppSettings.Settings[configLine.Key] != null)
                {
                    config.AppSettings.Settings[configLine.Key].Value = configLine.Value;
                }
                else
                {
                    config.AppSettings.Settings.Add(configLine.Key, configLine.Value);
                }
            }
            config.Save();
            ConfigurationManager.RefreshSection("appSettings");
        }
    }
}

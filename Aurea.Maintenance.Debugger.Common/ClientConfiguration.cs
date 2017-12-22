namespace Aurea.Maintenance.Debugger.Common
{
    using System;
    using CIS.Framework.ExceptionManagement;
    using System.IO;
    using System.Collections.Generic;
    using System.Configuration;
    using System.Globalization;
    using System.Reflection;
    using System.Threading;
    using CIS.BusinessComponent;
    using CIS.BusinessEntity;
    using CIS.Framework.Security;
    using Common.Models;

    public static class ClientConfiguration
    {
        public static ClientEnvironmentConfiguration GetClientConfiguration(Clients client, Stages stage, TransactionMode transactionMode = TransactionMode.Normal)
        {
            return new ClientEnvironmentConfiguration(client, stage, transactionMode);
        }

        public static GlobalApplicationConfigurationDS.GlobalApplicationConfiguration SetConfigurationContext(ClientEnvironmentConfiguration config)
        {
            var applicationConfiguration = GlobalApplicationConfigurationBC.Load(config.ConnectionBillingAdmin, config.ClientId);

            if (config.TransactionMode == TransactionMode.Enlist)
            {
                applicationConfiguration.ConnectionCsr += ";Enlist=false";
                applicationConfiguration.ConnectionMarket += ";Enlist=false";
                applicationConfiguration.ConnectionTdsp += ";Enlist=false";
            }

            var settings = new Dictionary<string, string>
            {
                {"AdminConnectionString", config.ConnectionBillingAdmin},
                {"Connection.BillingAdministration", config.ConnectionBillingAdmin},
                {"Connection.BillingAdministrationMaster", config.ConnectionBillingAdmin},
                {"Connection.BillingAdmin", config.ConnectionBillingAdmin},
                {"BillingAdminDbConnection", config.ConnectionBillingAdmin},
                {"conBillingAdminString", config.ConnectionBillingAdmin},
                {"BillingAdmin", config.ConnectionBillingAdmin},
                {"Connection.Tdsp", applicationConfiguration.ConnectionTdsp},
                {"Connection.Csr", applicationConfiguration.ConnectionCsr },
                {"ClientId", applicationConfiguration.ClientID.ToString() }
            };

            RewriteAppSettings(Assembly.GetExecutingAssembly(), settings);
            RewriteAppSettings(Assembly.GetEntryAssembly(), settings);

            SecurityManager.SetSecurityContext(applicationConfiguration, 0, 
                config.ConnectionBillingAdmin, 
                applicationConfiguration.ConnectionTdsp, 
                string.Empty, 
                string.Empty);

            // Set culture to en-EN to prevent string manipulation issues in base code
            var culture = "en-US";
            var cultureInfo = CultureInfo.GetCultureInfo(culture);
            Thread.CurrentThread.CurrentCulture = cultureInfo;
            Thread.CurrentThread.CurrentUICulture = cultureInfo;

            return applicationConfiguration;
        }

        private static void RewriteAppSettings(Assembly assembly, Dictionary<string, string> configDictionary)
        {
            var appPath = Path.GetDirectoryName(assembly.Location);
            var appConfigFile = Path.Combine(appPath ?? throw new InvalidArgumentException("AppPath"), assembly.ManifestModule.Name + ".config");
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

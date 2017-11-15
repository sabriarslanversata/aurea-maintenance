using System;
using CIS.BusinessComponent;
using CIS.BusinessEntity;
using CIS.Framework.Security;
using System.Collections.Generic;
using System.Configuration;
using System.Runtime.InteropServices;
using System.Threading;

namespace Aurea.Maintenance.Debugger.Common
{

    public class Utility
    {
        [StructLayout(LayoutKind.Sequential)]
        private struct SYSTEMTIME
        {
            public short wYear;
            public short wMonth;
            public short wDayOfWeek;
            public short wDay;
            public short wHour;
            public short wMinute;
            public short wSecond;
            public short wMilliseconds;
        }

        [DllImport("kernel32.dll", SetLastError = true)]
        private static extern bool SetSystemTime(ref SYSTEMTIME st);

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

        public static void SetThreadCulture(string culture)
        {
            Thread.CurrentThread.CurrentCulture = System.Globalization.CultureInfo.GetCultureInfo(culture);
            Thread.CurrentThread.CurrentUICulture = System.Globalization.CultureInfo.GetCultureInfo(culture);
        }

        public static void ChangeSystemDateTime(DateTime value)
        {
            var st = new SYSTEMTIME();
            st.wYear = (short)value.Year;
            st.wMonth = (short)value.Month;
            st.wDay = (short)value.Day;
            st.wHour = (short)value.Hour;
            st.wMinute = (short)value.Minute;
            st.wSecond = (short)value.Second;
            SetSystemTime(ref st);
        }

    }
}

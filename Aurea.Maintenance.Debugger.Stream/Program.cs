using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

using Aurea.Maintenance.Debugger.Common;

using CIS.BusinessEntity;
//using CIS.Element.Billing;
//using Csla.Validation;

namespace Aurea.Maintenance.Debugger.Stream
{
    class Program
    {
        static void Main(string[] args)
        {
            var clientConfiguration = Utility.SetSecurity(Utility.BillingAdminDEV, Utility.Clients["SGE"]);

            //SimulatePostEnrollmentEvent(clientConfiguration);

            //SimulateInbound814E(clientConfiguration);
        }

        private static void SimulateInbound814E(GlobalApplicationConfigurationDS.GlobalApplicationConfiguration clientConfiguration)
        {
            // Set culture to en-EN to prevent string manipulation issues in base code
            SetThreadCulture("en-EN");

            var engine = new CIS.Engine.Event.Queue(Utility.BillingAdminDEV);
            engine.ProcessEventQueue(clientConfiguration.ClientID, clientConfiguration.ConnectionCsr, clientConfiguration.ConnectionMarket, clientConfiguration.ClientAbbreviation);
        }

        private static void SimulatePostEnrollmentEvent(GlobalApplicationConfigurationDS.GlobalApplicationConfiguration clientConfiguration)
        {
            // Set culture to en-EN to prevent string manipulation issues in base code
            SetThreadCulture("en-EN");

            var pe = new CIS.Clients.Stream.PostEnrollment
            {
                ConnectionString = clientConfiguration.ConnectionCsr
            };
            pe.Process(30396997);
        }

        private static SetThreadCulture(string culture)
        {
            Thread.CurrentThread.CurrentCulture = System.Globalization.CultureInfo.GetCultureInfo(culture);
            Thread.CurrentThread.CurrentUICulture = System.Globalization.CultureInfo.GetCultureInfo(culture);
        }
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Aurea.Maintenance.Debugger.Common;

namespace Aurea.Maintenance.Debugger.Stream
{
    class Program
    {
        static void Main(string[] args)
        {
            var clientConfiguration = Utility.SetSecurity(Utility.BillingAdminDEV, Utility.Clients["SGE"]);
            
        }
    }
}

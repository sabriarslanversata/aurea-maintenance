using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Aurea.Maintenance.Debugger.Spark.Models;

namespace Aurea.Maintenance.Debugger.Spark.Extensions
{
    public static class CustomerExtension
    {
        public static string GetTableName(this Customer cust)
        {
            return "Customer";
        }

        public static string GetPrimaryKeyFieldName(this Customer cust)
        {
            return "CustId";
        }

        public static List<Type> GetRequiredEntiyList(this Customer cust)
        {
            var requiredEntityList = new List<Type>
            {
                typeof(Premise),
                typeof(Address),
                typeof(Rate),
                typeof(RateDetail),
                typeof(RateTransition),
                typeof(Meter),
                typeof(Product),
                typeof(CustomerAdditionalInfo),
                typeof(AccountsReceivable),
                typeof(ClientCustomerContract)
            };


            return requiredEntityList;
        }
            
        public static Customer CopyCustomer(this Customer cust, int custId, string connectionString)
        {
            throw new NotImplementedException();
        }
    }
}

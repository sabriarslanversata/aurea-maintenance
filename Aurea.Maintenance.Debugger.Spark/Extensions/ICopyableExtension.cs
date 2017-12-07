using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Aurea.Maintenance.Debugger.Spark.Models;

namespace Aurea.Maintenance.Debugger.Spark.Extensions
{
    public static class ICopyableExtension
    {
        public static Boolean CopyEntity<T>(this T entity, string connectionString) where T :  ICopyableEntity
        {
            var result = false;
            var tableAttribute = entity.GetType().GetCustomAttributesIncludingBaseInterfaces<TableAttribute>();
            var requiredAttributes = entity.GetType().GetCustomAttributesIncludingBaseInterfaces<RequiredEntityAttribute>();
            
            return result;
        }
    }
}

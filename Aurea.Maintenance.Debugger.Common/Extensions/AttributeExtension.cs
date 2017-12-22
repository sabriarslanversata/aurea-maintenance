namespace Aurea.Maintenance.Debugger.Common.Extensions
{
    using System;
    using System.Collections.Generic;
    using System.Linq;

    public static class AttributeExtension
    {
        public static IEnumerable<T> GetCustomAttributesIncludingBaseInterfaces<T>(this Type type)
        {
            var attributeType = typeof(T);
            return type.GetCustomAttributes(attributeType, true)
                .Union(type.GetInterfaces()
                    .SelectMany(interfaceType => interfaceType.GetCustomAttributes(attributeType, true)))
                .Distinct()
                .Cast<T>();
        }
    }
}

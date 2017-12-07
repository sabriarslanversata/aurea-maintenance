using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Aurea.Maintenance.Debugger.Common.Extensions;

namespace Aurea.Maintenance.Debugger.Common.Models
{
    [System.AttributeUsage(System.AttributeTargets.Class, AllowMultiple = true)]
    public class RelatedEntityAttribute : System.Attribute
    {
        public Type RelatedEntity;
        public string RelatedField;
        public string EntityField;
        public bool IsRequiredBeforeCopy;

        public RelatedEntityAttribute(Type relatedEntity)
        {
            this.RelatedEntity = relatedEntity;
            this.IsRequiredBeforeCopy = false;
            var tableAttr = relatedEntity.GetCustomAttributesIncludingBaseInterfaces<TableAttribute>().First();
            if (tableAttr.HasIdentity)
            {
                this.EntityField = tableAttr.PrimaryKey;
            }

        }
    }
}

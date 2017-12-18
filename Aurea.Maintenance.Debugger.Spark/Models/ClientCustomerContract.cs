using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Aurea.Maintenance.Debugger.Common.Models;

// ReSharper disable InconsistentNaming

namespace Aurea.Maintenance.Debugger.Spark.Models
{
    [Table("Contract", PrimaryKey = "ContractID", TableSchema = "ClientCustomer", HasIdentity = false)]
    [RelatedEntity(typeof(Contract)/*, IsRequiredBeforeCopy = true*/)]
    public class ClientCustomerContract:ICopyableEntity
    {
        public int ContractID { get; set; }
        public decimal SegmentationAdjustmentToEnergyRate { get; set; }
        public string SegmentationLabel { get; set; }
    }
}

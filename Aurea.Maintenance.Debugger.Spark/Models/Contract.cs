using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
// ReSharper disable InconsistentNaming

namespace Aurea.Maintenance.Debugger.Spark.Models
{
    [Table("Contract")]
    [RelatedEntity(typeof(Product), IsRequired = true, RelatedField = "ProductID")]
    [RelatedEntity(typeof(Customer), IsRequired = true, RelatedField = "CustID")]
    [RelatedEntity(typeof(Rate), IsRequired = true, RelatedField = "RateID")]
    [RelatedEntity(typeof(RateDetail), IsRequired = true, RelatedField = "RateDetID")]
    public class Contract : ICopyableEntity
    {
        public int ContractID { get; set; }
        public DateTime SignedDate { get; set; }
        public DateTime BeginDate { get; set; }
        public DateTime EndDate { get; set; }
        public int TermLength { get; set; }
        public int ContractTypeID { get; set; }
        public string ContactName { get; set; }
        public string ContactPhone { get; set; }
        public string ContactFax { get; set; }
        public int ProductID { get; set; }
        public int CreatedByID { get; set; }
        public DateTime CreateDate { get; set; }
        public int CustID { get; set; }
        public short AutoRenewFlag { get; set; }
        public string Service { get; set; }
        public short ActiveFlag { get; set; }
        public string RateCode { get; set; }
        public int TDSPTemplateID { get; set; }
        public int ContractTerm { get; set; }
        public int RateDetID { get; set; }
        public int RateID { get; set; }
        public int Terms { get; set; }
        public string ContractName { get; set; }
        public int ContractLength { get; set; }
        public int AccountManagerID { get; set; }
        public string MeterChargeCode { get; set; }
        public short AggregatorFee { get; set; }
        public DateTime TermDate { get; set; }
        public int Bandwidth { get; set; }
        public decimal FinanceCharge { get; set; }
        public string ContractNumber { get; set; }
        public int PremID { get; set; }
        public int AnnualUsage { get; set; }
        public int CurePeriod { get; set; }
        public int ContractStatusID { get; set; }
        public string RenewalRate { get; set; }
        public DateTime RenewalStartDate { get; set; }
        public int RenewalTerm { get; set; }
        public int ChangeReason { get; set; }
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Aurea.Maintenance.Debugger.Common.Models;

// ReSharper disable InconsistentNaming

namespace Aurea.Maintenance.Debugger.Spark.Models
{
    [Table("RateDetail", PrimaryKey = "RateDetID")]
    //[RelatedEntity(typeof(Rate), RelatedField = "RateID"/*, IsRequiredBeforeCopy = true*/)]
    public class RateDetail : ICopyableEntity
    {
        public int RateDetID { get; set; }
        public int RateID { get; set; }
        public int CategoryID { get; set; }
        public int RateTypeID { get; set; }
        public int ConsUnitID { get; set; }
        public int RateDescID { get; set; }
        public DateTime EffectiveDate { get; set; }
        public DateTime ExpirationDate { get; set; }
        public decimal RateAmt { get; set; }
        public decimal RateAmt2 { get; set; }
        public decimal RateAmt3 { get; set; }
        public decimal FixedAdder { get; set; }
        public decimal MinDetAmt { get; set; }
        public decimal MaxDetAmt { get; set; }
        public string GLAcct { get; set; }
        public decimal RangeLower { get; set; }
        public decimal RangeUpper { get; set; }
        public string CustType { get; set; }
        public string Graduated { get; set; }
        public string Progressive { get; set; }
        public string AmountCap { get; set; }
        public string MaxRateAmt { get; set; }
        public string MinRateAmt { get; set; }
        public string CategoryRollup { get; set; }
        public string Taxable { get; set; }
        public string ChargeType { get; set; }
        public string MiscData1 { get; set; }
        public decimal FixedCapRate { get; set; }
        public decimal ScaleFactor1 { get; set; }
        public decimal ScaleFactor2 { get; set; }
        public int TemplateRateDetID { get; set; }
        public float Margin { get; set; }
        public int HALRateDetailId { get; set; }
        public int UsageClassId { get; set; }
        public int LegacyRateDetailId { get; set; }
        public string Building { get; set; }
        public int ServiceTypeID { get; set; }
        public int TaxCategoryID { get; set; }
        public int UtilityID { get; set; }
        public int UtilityInvoiceTemplateDetailID { get; set; }
        public short Active { get; set; }
        public int StatusID { get; set; }
        public int RateVariableTypeId { get; set; }
        public int MinDays { get; set; }
        public int MaxDays { get; set; }
        public int BlockPriceIndicator { get; set; }
        public int RateTransitionId { get; set; }
        public DateTime CreateDate { get; set; }
        public short MeterMultiplierFlag { get; set; }
        public decimal BlendRatio { get; set; }
        public int ContractVolumeID { get; set; }
        public int CreatedByUserId { get; set; }
        public int ModifiedByUserId { get; set; }
        public DateTime ModifiedDate { get; set; }
        public int TOUTemplateID { get; set; }
        public int TOUTemplateRegisterID { get; set; }
        public string TOUTemplateRegisterName { get; set; }
    }
}

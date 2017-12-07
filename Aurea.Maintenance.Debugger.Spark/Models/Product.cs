using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
// ReSharper disable InconsistentNaming

namespace Aurea.Maintenance.Debugger.Spark.Models
{
    [Table("Product")]
    [RelatedEntity(typeof(Rate), RelatedField = "RateID", IsRequiredBeforeCopy = true)]
    public class Product:ICopyableEntity
    {
        public int ProductID { get; set; }
        public int RateID { get; set; }
        public int LDCCode { get; set; }
        public int PlanType { get; set; }
        public int TDSPTemplateID { get; set; }
        public string Description { get; set; }
        public DateTime BeginDate { get; set; }
        public DateTime EndDate { get; set; }
        public string CustType { get; set; }
        public string Graduated { get; set; }
        public int RangeTier1 { get; set; }
        public int RangeTier2 { get; set; }
        public int SortOrder { get; set; }
        public short ActiveFlag { get; set; }
        public decimal Uplift { get; set; }
        public int CSATDSPTemplateID { get; set; }
        public int CAATDSPTemplateID { get; set; }
        public string PriceDescription { get; set; }
        public string MarketingCode { get; set; }
        public int RateTypeID { get; set; }
        public int ConsUnitID { get; set; }
        public short Default { get; set; }
        public string DivisionCode { get; set; }
        public string RateDescription { get; set; }
        public string ServiceType { get; set; }
        public int CSPId { get; set; }
        public int TermsId { get; set; }
        public int RolloverProductId { get; set; }
        public int CommissionId { get; set; }
        public decimal CommissionAmt { get; set; }
        public int CancelFeeId { get; set; }
        public int MonthlyChargeId { get; set; }
        public string ProductCode { get; set; }
        public int RatePackageId { get; set; }
        public string ProductName { get; set; }
        public DateTime TermDate { get; set; }
        public int DiscountTypeId { get; set; }
        public decimal DiscountAmount { get; set; }
        public int ProductZoneID { get; set; }
        public short IsGreen { get; set; }
        public short IsBestChoice { get; set; }
        public short ActiveEnrollmentFlag { get; set; }
        public int CreditScoreThreshold { get; set; }
        public decimal DepositAmount { get; set; }
        public string Incentives { get; set; }
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Aurea.Maintenance.Debugger.Common.Models;

// ReSharper disable InconsistentNaming

namespace Aurea.Maintenance.Debugger.Spark.Models
{
    [Table("Rate")]
    [RelatedEntity(typeof(RateDetail), RelatedField = "RateID")]
    public class Rate : ICopyableEntity
    {
        public int RateID { get; set; }
        public int CSPID { get; set; }
        public string RateCode { get; set; }
        public string RateDesc { get; set; }
        public DateTime EffectiveDate { get; set; }
        public DateTime ExpirationDate { get; set; }
        public string RateType { get; set; }
        public int PlanType { get; set; }
        public short IsMajority { get; set; }
        public short TemplateFlag { get; set; }
        public int LDCCode { get; set; }
        public DateTime CreateDate { get; set; }
        public int UserID { get; set; }
        public string RatePackageName { get; set; }
        public string CustType { get; set; }
        public string ServiceType { get; set; }
        public string DivisionCode { get; set; }
        public int ConsUnitId { get; set; }
        public short ActiveFlag { get; set; }
        public string LDCRateCode { get; set; }
        public int migr_plan_id { get; set; }
        public int migr_custid { get; set; }
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Aurea.Maintenance.Debugger.Common.Models;

// ReSharper disable InconsistentNaming

namespace Aurea.Maintenance.Debugger.Spark.Models
{
    [Table("Premise", PrimaryKey = "PremID")]
    //[RelatedEntity(typeof(Customer), RelatedField = "CustID"/*, IsRequiredBeforeCopy = true*/)]
    [RelatedEntity(typeof(Address), RelatedField = "AddrID"/*, IsRequiredBeforeCopy = true*/)]
    public class Premise: ICopyableEntity
    {
        public int PremID { get; set; }
        public int CustID { get; set; }
        public int CSPID { get; set; }
        public int AddrID { get; set; }
        public int TDSPTemplateID { get; set; }
        public int ServiceCycle { get; set; }
        public int TDSP { get; set; }
        public int TaxAssessment { get; set; }
        public string PremNo { get; set; }
        public string PremDesc { get; set; }
        public string PremStatus { get; set; }
        public string PremType { get; set; }
        public string LocationCode { get; set; }
        public short SpecialNeedsFlag { get; set; }
        public int SpecialNeedsStatus { get; set; }
        public DateTime SpecialNeedsDate { get; set; }
        public int ReadingIncrement { get; set; }
        public string Metered { get; set; }
        public string Taxable { get; set; }
        public DateTime BeginServiceDate { get; set; }
        public DateTime EndServiceDate { get; set; }
        public int SourceLevel { get; set; }
        public int StatusID { get; set; }
        public DateTime StatusDate { get; set; }
        public DateTime CreateDate { get; set; }
        public int UnitID { get; set; }
        public int PropertyCommonID { get; set; }
        public int RateID { get; set; }
        public short DeleteFlag { get; set; }
        public int LBMPId { get; set; }
        public int PipelineId { get; set; }
        public int GasLossId { get; set; }
        public int LDCID { get; set; }
        public string GasPoolID { get; set; }
        public string DeliveryPoint { get; set; }
        public string ConsumptionBandIndex { get; set; }
        public DateTime LastModifiedDate { get; set; }
        public int CreatedByID { get; set; }
        public int ModifiedByID { get; set; }
        public string BillingAccountNumber { get; set; }
        public string NameKey { get; set; }
        public string GasSupplyServiceOption { get; set; }
        public int IntervalUsageTypeId { get; set; }
        public short LDC_UnMeteredAcct { get; set; }
        public string AltPremNo { get; set; }
        public short OnSwitchHold { get; set; }
        public DateTime SwitchHoldStartDate { get; set; }
        public int ConsumptionImportTypeId { get; set; }
        public DateTime TDSPTemplateEffectiveDate { get; set; }
        public string ServiceDeliveryPoint { get; set; }
        public string UtilityContractID { get; set; }
        public short LidaDiscount { get; set; }
        public string GasCapacityAssignment { get; set; }
        public string CPAEnrollmentTypes { get; set; }
        public short IsTOU { get; set; }
        public string SupplierPricingStructureNr { get; set; }
        public string SupplierGroupNumber { get; set; }
    }
}

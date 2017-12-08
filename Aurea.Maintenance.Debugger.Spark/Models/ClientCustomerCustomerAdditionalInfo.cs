using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Aurea.Maintenance.Debugger.Common.Models;

// ReSharper disable InconsistentNaming

namespace Aurea.Maintenance.Debugger.Spark.Models
{
    [Table("CustomerAdditionalInfo", PrimaryKey = "CustID", TableSchema = "ClientCustomer", HasIdentity = false)]
    [RelatedEntity(typeof(Customer)/*, IsRequiredBeforeCopy = true*/)]
    public class ClientCustomerCustomerAdditionalInfo:ICopyableEntity
    {

        public int CustID { get; set; }
        public int InvoiceDueDays { get; set; }
        public int RolloverProductId { get; set; }
        public string ContractPath { get; set; }
        public string MarketerCode { get; set; }
        public string PromoCode { get; set; }
        public string SalesAgent { get; set; }
        public string SegmentationLabel { get; set; }
        public decimal SegmentationAdjustmentToEnergyRate { get; set; }
        public DateTime ReleaseDate { get; set; }
        public DateTime EstimatedFlowDate { get; set; }
        public string ReferAFriendToken { get; set; }
        public short DoesOnFlowDateResetContractDates { get; set; }
        public short IsEmployee { get; set; }
        public decimal EarlyTerminationFee { get; set; }
        public short CsrAutoPayFlag { get; set; }
        public string OCCNumber { get; set; }
        public string DealType { get; set; }
        public string DCQ { get; set; }
        public string SICCode { get; set; }
        public string EnrollmentRateClass { get; set; }
        public short IsRenewalEligible { get; set; }
        public string ExternalSalesId { get; set; }
        public string EnrollmentCategory { get; set; }
        public string ReferralID { get; set; }
        public string SalesChannel { get; set; }
        public string AutoPaySource { get; set; }
        public DateTime AutoPayLastUpdated { get; set; }
        public short AutoPayFlag { get; set; }
        public DateTime SoldDate { get; set; }
    }
}

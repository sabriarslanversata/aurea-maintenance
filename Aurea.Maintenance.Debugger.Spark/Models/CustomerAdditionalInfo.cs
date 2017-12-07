using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
// ReSharper disable InconsistentNaming

namespace Aurea.Maintenance.Debugger.Spark.Models
{
    [Table("CustomerAdditionalInfo", PrimaryKey = "CustID", HasIdentity = false)]
    [RelatedEntity(typeof(Customer), RelatedField = "CustID", IsRequiredBeforeCopy = true)]
    public class CustomerAdditionalInfo : ICopyableEntity
    {
        public int CustID { get; set; }
        public int CSPDUNSID { get; set; }
        public int BillingTypeID { get; set; }
        public int BillingDayOfMonth { get; set; }
        public int MasterCustID_2 { get; set; }
        public int MasterCustID_3 { get; set; }
        public int MasterCustID_4 { get; set; }
        public int TaxAssessment { get; set; }
        public int ContractPeriod { get; set; }
        public DateTime ContractDate { get; set; }
        public int AccessVerificationType { get; set; }
        public string AccessVerificationData { get; set; }
        public string ClientAccountNo { get; set; }
        public string InstitutionID { get; set; }
        public string TransitNum { get; set; }
        public string AccountNum { get; set; }
        public string MigrationAccountNo { get; set; }
        public DateTime MigrationFirstServed { get; set; }
        public int MigrationKwh { get; set; }
        public int CollectionsStageID { get; set; }
        public int CollectionsStatus { get; set; }
        public DateTime CollectionsDate { get; set; }
        public string KeyAccount { get; set; }
        public string DisconnectLtr { get; set; }
        public string AuthorizedReleaseName { get; set; }
        public DateTime AuthorizedReleaseDOB { get; set; }
        public string AuthorizedReleaseFederalTaxID { get; set; }
        public short EFTFlag { get; set; }
        public short PromiseToPayFlag { get; set; }
        public short DisconnectFlag { get; set; }
        public short CreditHoldFlag { get; set; }
        public short RawConsumptionImportFlag { get; set; }
        public short CustomerProtectionStatus { get; set; }
        public short MCPEFlag { get; set; }
        public short HasLocationMasterFlag { get; set; }
        public int DivisionID { get; set; }
        public string DivisionCode { get; set; }
        public string DriverLicenseNo { get; set; }
        public int PromotionCodeID { get; set; }
        public string CustomerDUNS { get; set; }
        public int CustomerGroupID { get; set; }
        public decimal EarlyTermFee { get; set; }
        public DateTime EarlyTermFeeUpdateDate { get; set; }
        public short DeceasedFlag { get; set; }
        public short BankruptFlag { get; set; }
        public int CollectionsAgencyID { get; set; }
        public short DoNotCall { get; set; }
        public string CustomerSecretWord { get; set; }
        public int PrintGroupID { get; set; }
        public string CurrentCustNo { get; set; }
        public short IsDPP { get; set; }
        public string UnitNumber { get; set; }
        public int CustomerCategoryID { get; set; }
        public short SubsequentDepositExempt { get; set; }
        public short AutoPayFlag { get; set; }
        public short SpecialNeedsFlag { get; set; }
        public DateTime SpecialNeedsEndDate { get; set; }
        public int SpecialNeedsQualifierTypeID { get; set; }
        public DateTime CsrImportDate { get; set; }
        public short OnSwitchHold { get; set; }
        public DateTime SwitchHoldStartDate { get; set; }
        public int DPPStatusID { get; set; }
        public short UpdateContactInfoFlag { get; set; }
        public short IsFriendlyLatePaymentReminderSent { get; set; }
        public short IsLowIncome { get; set; }
        public int ExtendedCustTypeId { get; set; }
        public DateTime AutoPayLastUpdated { get; set; }
        public int GreenEnergyOptIn { get; set; }
        public int SocialCauseID { get; set; }
        public string SocialCauseCode { get; set; }
        public string SecondaryContactFirstName { get; set; }
        public string SecondaryContactLastName { get; set; }
        public string SecondaryContactPhone { get; set; }
        public int SecondaryContactRelationId { get; set; }
        public string SSN { get; set; }
        public string ServiceAccount { get; set; }
        public short IsPUCComplaint { get; set; }
        public int EncryptionPasswordTypeId { get; set; }
        public string EncryptionPasswordCustomValue { get; set; }
        public string CustInfo1 { get; set; }
        public string CustInfo2 { get; set; }
        public string CustInfo3 { get; set; }
        public DateTime CustInfo4 { get; set; }
        public decimal CustInfo5 { get; set; }
        public string SalesAgent { get; set; }
        public string Broker { get; set; }
        public string PromoCode { get; set; }
        public string CommissionType { get; set; }
        public decimal CommissionAmount { get; set; }
        public string ReferralID { get; set; }
        public string CampaignName { get; set; }
        public string AccessDBID { get; set; }
        public string SalesChannel { get; set; }
        public string TCPAAuthorization { get; set; }
        public string CancellationFee { get; set; }
        public string MunicipalAggregation { get; set; }
    }
}

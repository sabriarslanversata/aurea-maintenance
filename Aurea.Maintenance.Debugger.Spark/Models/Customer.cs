using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

// ReSharper disable InconsistentNaming

namespace Aurea.Maintenance.Debugger.Spark.Models
{
    [Table("Customer", PrimaryKey = "CustID")]
    [RequiredEntity(typeof(Address), SourceField = "SiteAddrId")]
    [RequiredEntity(typeof(Address), SourceField = "MailAddrId")]
    [RequiredEntity(typeof(Address), SourceField = "CrrAddrId")]
    [RequiredEntity(typeof(Rate), SourceField = "RateId")]
    public class Customer : ICopyableEntity
    {
        public int CustID { get; set; }
        public int CSPID { get; set; }
        public int CSPCustID { get; set; }
        public int PropertyID { get; set; }
        public int PropertyCustID { get; set; }
        public int CustomerTypeID { get; set; }
        public string CustNo { get; set; }
        public string CustName { get; set; }
        public string LastName { get; set; }
        public string FirstName { get; set; }
        public string MidName { get; set; }
        public string CompanyName { get; set; }
        public string DBA { get; set; }
        public string FederalTaxID { get; set; }
        public int AcctsRecID { get; set; }
        public string DistributedAR { get; set; }
        public int ProductionCycle { get; set; }
        public int BillCycle { get; set; }
        public int RateID { get; set; }
        public int SiteAddrID { get; set; }
        public int MailAddrId { get; set; }
        public int CorrAddrID { get; set; }
        public short MailToSiteAddress { get; set; }
        public int BillCustID { get; set; }
        public int MasterCustID { get; set; }
        public string Master { get; set; }
        public string CustStatus { get; set; }
        public string BilledThru { get; set; }
        public string CSRStatus { get; set; }
        public string CustType { get; set; }
        public string Services { get; set; }
        public string FEIN { get; set; }
        public DateTime DOB { get; set; }
        public string Taxable { get; set; }
        public string LateFees { get; set; }
        public int NoOfAccts { get; set; }
        public string ConsolidatedInv { get; set; }
        public string SummaryInv { get; set; }
        public int MsgID { get; set; }
        public int TDSPTemplateID { get; set; }
        public int TDSPGroupID { get; set; }
        public string LifeSupportIndictor { get; set; }
        public string LifeSupportStatus { get; set; }
        public DateTime LifeSupportDate { get; set; }
        public string SpecialBenefitsPlan { get; set; }
        public int BillFormat { get; set; }
        public int CreditScore { get; set; }
        public string HitIndicator { get; set; }
        public string RequiredDeposit { get; set; }
        public string AccountManager { get; set; }
        public string EnrollmentAlias { get; set; }
        public int ContractID { get; set; }
        public DateTime ContractStartDate { get; set; }
        public DateTime ContractEndDate { get; set; }
        public string UserDefined1 { get; set; }
        public DateTime CreateDate { get; set; }
        public DateTime RateChangeDate { get; set; }
        public string ConversionAccountNo { get; set; }
        public string PermitContactName { get; set; }
        public short CustomerPrivacy { get; set; }
        public short UsagePrivacy { get; set; }
        public string CompanyRegistrationNumber { get; set; }
        public string VATNumber { get; set; }
        public string AccountStatus { get; set; }
        public short AutoCreditAfterInvoiceFlag { get; set; }
        public short LidaDiscount { get; set; }
        public short DoNotDisconnect { get; set; }
        public string DDPlus1 { get; set; }
        public DateTime CsrImportDate { get; set; }
        public int DeliveryTypeID { get; set; }
        public int SpecialNeedsAddrID { get; set; }
        public int PaymentModelId { get; set; }
        public string PORFlag { get; set; }
        public int PowerOutageAddrId { get; set; }
    }
}

using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
// ReSharper disable InconsistentNaming

namespace Aurea.Maintenance.Debugger.Spark.Models
{
    [Table("AccountsReceivable", PrimaryKey = "AcctsRecID")]
    [RelatedEntity(typeof(Customer), IsRequired = true)]
    public class AccountsReceivable:ICopyableEntity
    {
        public int AcctsRecID { get; set; }
        public DateTime ResetDate { get; set; }
        public DateTime ARDate { get; set; }
        public decimal PrevBal { get; set; }
        public decimal CurrInvs { get; set; }
        public decimal CurrPmts { get; set; }
        public decimal CurrAdjs { get; set; }
        public decimal BalDue { get; set; }
        public decimal LateFee { get; set; }
        public decimal LateFeeRate { get; set; }
        public decimal LateFeeMaxAmount { get; set; }
        public int LateFeeTypeID { get; set; }
        public decimal AuthorizedPymt { get; set; }
        public string PastDue { get; set; }
        public decimal BalAge0 { get; set; }
        public decimal BalAge1 { get; set; }
        public decimal BalAge2 { get; set; }
        public decimal BalAge3 { get; set; }
        public decimal BalAge4 { get; set; }
        public decimal BalAge5 { get; set; }
        public decimal BalAge6 { get; set; }
        public decimal Deposit { get; set; }
        public string DepositBeginDate { get; set; }
        public short PaymentPlanFlag { get; set; }
        public short PaymentPlanTrueUpFlag { get; set; }
        public decimal PaymentPlanAmount { get; set; }
        public int PaymentPlanTrueUpPeriod { get; set; }
        public decimal PaymentPlanTrueUpThresholdAmount { get; set; }
        public int PaymentPlanTrueUpType { get; set; }
        public DateTime PaymentPlanEffectiveDate { get; set; }
        public short PrePaymentFlag { get; set; }
        public decimal PrePaymentDailyAmount { get; set; }
        public decimal CapitalCredit { get; set; }
        public int Terms { get; set; }
        public int StatusID { get; set; }
        public int GracePeriod { get; set; }
        public decimal PaymentPlanTotalVariance { get; set; }
        public decimal PaymentPlanVarianceUnit { get; set; }
        public int LateFeeGracePeriod { get; set; }
        public int CancelFeeTypeId { get; set; }
        public decimal CancelFeeAmount { get; set; }
        public string Migr_acct_no { get; set; }
        public decimal InvoiceMinimumAmount { get; set; }
        public decimal LateFeeThresholdAmt { get; set; }
        public int LastInvoiceAcctsRecHistID { get; set; }
        public int LastPaymentAcctsRecHistId { get; set; }
        public int LastAdjustmentAcctsRecHistId { get; set; }
        public decimal DeferredBalance { get; set; }
    }
}

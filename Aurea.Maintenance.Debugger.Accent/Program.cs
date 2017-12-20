

using System.Data;
using System.Data.SqlClient;
using CIS.Clients.Accent.EnrollmentManager;
using CIS.Clients.Accent.Import;
using CIS.Framework.Data;
using CIS.Web.Services.Clients.Accent.Helpers;

namespace Aurea.Maintenance.Debugger.Accent
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using System.Threading.Tasks;
    using Common;
    using Common.Models;
    using System.Security.Policy;
    using Aurea.IO;
    using Aurea.Logging;
    using CIS.BusinessEntity;
    using System.Transactions;

    public class Program
    {
        public class MyMaintenance : CIS.Clients.Accent.Maintenance
        {
            public MyMaintenance(string connectionCsr, string connectionMarket, string connectionAdmin)
                : base(connectionCsr, connectionMarket, connectionAdmin)
            {
                //
            }

            public override void InitializeVariables(string maintenanceFunction)
            {
                _runHour = "*";
                _runDay = "*";
                _runDayOfWeek = "*";
                _isEnabled = true;
                SkipIsValidRuntimeVerification = true;
                _lastRunTime = DateTime.Now.AddYears(-1);
            }
        }

        private static ClientEnvironmentConfiguration _clientConfig;
        private static GlobalApplicationConfigurationDS.GlobalApplicationConfiguration _appConfig;
        private static ILogger _logger = new Logger();

        static void Main(string[] args)
        {
            // Set client configuration and then the application configuration context.            
            _clientConfig = ClientConfiguration.GetClientConfiguration(Clients.Accent, Stages.Development, TransactionMode.Enlist);
            _appConfig = ClientConfiguration.SetConfigurationContext(_clientConfig);

            TransactionManager.DistributedTransactionStarted += delegate
                (object sender, TransactionEventArgs e)
            {
                _logger.Info("Distributed Transaction Started");
            };

            simulateEnrollmentViaWS();

            _logger.Info("Debug session has ended");
            Console.ReadLine();
        }

        private static void simulateEnrollmentViaWS()
        {
            string productCode = "ETTXU24FIXED00ZZ00RPROMO1TRC199NESTTHERMOSTAT";
            long accountNo = 10443720004064914;

            CopyProductAndRate(productCode: productCode, maxDepth: 5);
            (string message, int enrollCustId) = EnrollCustomerWithPaymentInfo(CreateEnrollmentDataWithPaymentInfoData(productCode, accountNo));

            Execute814Import();
            GenerateEvents();
            ProcessEvents();
            MakeCTRAccepted();
            GenerateEvents();
            ProcessEvents();
        }

        private static EnrollmentDataWithPaymentInfo CreateEnrollmentDataWithPaymentInfoData(string productCode, long accountNo)
        {
            string firstName = "Sabri"
                ,lastName = "Lion"
                ,address1 = "7440 La Vista Dr"
                ,address2 = "Apt 269"
                ,city = "Dallas"
                ,state = "TX"
                ,email = "lionsabri@lions.com"
                ,phone = "515-551-5511"
                //,productCode = "ETTXU24FIXED00ZZ00RPROMO1TRC199NESTTHERMOSTAT"
                ;
            int zip = 75214;
            //long accountNo = 10443720004064914;
            DateTime endDate = DateTime.Parse("2019-08-07T00:00:00"),
                signedDate = DateTime.Parse("2017-08-05T05:46:17"),
                specialReadDate = DateTime.Parse("2017-08-17T01:00:00"),
                switchDate = DateTime.Parse("2017-08-17T01:00:00");
            return new CIS.Clients.Accent.EnrollmentManager.EnrollmentDataWithPaymentInfo()
            {
                AccountManager = "IGS",
                ACHAccountType = ACHAccountTypes.CHECKING,
                AnnualTolerance = 0,
                AutoRenewFlag = true,
                BeginDate = DateTime.Parse("2017-08-07T00:00:00"),
                BillingAddress1 = address1,
                BillingAddress2 = address2,
                BillingCity = city,
                BillingContact = $"{firstName} {lastName}",
                BillingState = state,
                BillingType = BillingType.SupplierConsolidated,
                BillingZip = zip.ToString(),
                BillPrintType = "DYNOWATT",
                CardType = CreditCardTypes.AMEX,
                ContactEmail = email,
                ContactPhone = phone,
                County = city,
                CustomerCharge = 0,
                CustomerName = $"{firstName} {lastName}",
                CustomerType = CustomerType.Residential,
                Dba = $"{firstName} {lastName}",
                DepositAddress1 = address1,
                DepositAddress2 = address2,
                DepositAmount = 0m,
                DepositCity = city,
                DepositEmail = email,
                DepositFirstName = firstName,
                DepositLastName = lastName,
                DepositPhone = phone,
                DepositState = state,
                DepositZipCode = zip.ToString(),
                DivisionCode = "DYNOWATT",
                DoNotDisconnect = false,
                Email = email,
                EndDate = endDate,
                EnergyCharge = 0.09m,
                EnrollType = EnrollTypeEnum.MoveIn,
                EspDuns = "133305370",
                FirstName = firstName,
                IndexPriceAdder = 0,
                LastName = lastName,
                LateFeeGracePeriod = "16",
                LateFeePercentage = 5,
                LateFees = true,
                LifeSupport = false,
                LocalTaxExempt = false,
                NotificationWaiver = false,
                OfferCode = "EXACC04",
                OvertakeAdder = 0,
                PaymentPlan = false,
                PaymentPlanAmount = 0,
                Phone = phone,
                PORFlag = false,
                ProductCode = productCode,
                PTCValidationBypass = false,
                SalesChannel = "IGS",
                ServiceAddress1 = address1,
                ServiceAddress2 = address2,
                ServiceCity = city,
                ServiceState = state,
                ServiceType = ServiceType.Elec,
                ServiceZip = zip.ToString(),
                SignedDate = signedDate,
                SpanishBill = false,
                SpecialReadDate = specialReadDate,
                SpotPrice = 0,
                StateTaxExempt = false,
                SwitchDate = switchDate,
                Taxable = true,
                TdspCode = "TXU",
                Terms = 24,
                TolerancePct = 0,
                TXUtilityTaxExempt = false,
                UndertakeAdder = 0,
                UnitOfMeasure = UOM.KWH,
                UtilityAccountNumber = accountNo.ToString(),
                UtilityTaxExempt = false,
                AutoPayDebitType = 0,
                AutoPayCreditCardType = 0,
                AutoPayACHType = 0,
            };

        }

        private static void CheckForLDCConfigAndTrueUps(string ldcShortName, ref string trueUpType, ref string trueUpPeriod, ref string ldcValidationErrorMessage, ref bool ldcValidationStatus)
        {

            if ((!string.IsNullOrEmpty(trueUpType)) && (!string.IsNullOrEmpty(trueUpPeriod)))
            {
                if (!((CIS.Web.Services.Clients.Accent.Constants.Definitions.TrueUpType)Convert.ToInt16(trueUpType)).In(CIS.Web.Services.Clients.Accent.Constants.Definitions.TrueUpType.AdjustBudgetBillingAmount,
                    CIS.Web.Services.Clients.Accent.Constants.Definitions.TrueUpType.BillTrueUpAmount, CIS.Web.Services.Clients.Accent.Constants.Definitions.TrueUpType.BillTrueUpAmountAndAdjustBudgetBillingAmount))
                {
                    ldcValidationErrorMessage += "True up type is not supported; value must be 0, 1, or 3! <br />";
                    ldcValidationStatus = false;
                }

                if (!((CIS.Web.Services.Clients.Accent.Constants.Definitions.TrueUpPeriod)Convert.ToInt16(trueUpPeriod)).In(CIS.Web.Services.Clients.Accent.Constants.Definitions.TrueUpPeriod.Annual, CIS.Web.Services.Clients.Accent.Constants.Definitions.TrueUpPeriod.Quarterly,
                    CIS.Web.Services.Clients.Accent.Constants.Definitions.TrueUpPeriod.SemiAnnual))
                {
                    ldcValidationErrorMessage += "True up period is not supported; value must be 12, 6, or 3! <br />";
                    ldcValidationStatus = false;
                }
            }
            else
            {
                //Set the Default values for TrueUpType and TrueUpPeriod
                trueUpType = "0";//TrueUpType.BillTrueUpAmount;
                trueUpPeriod = "12"; //TrueUpPeriod.Annual;
            }
            DataTable marketdt = DB.GetMarketBasedonLDC(_appConfig.ConnectionCsr, ldcShortName);

            if (marketdt.Rows.Count == 0)
            {
                ldcValidationErrorMessage += $"LDC {ldcShortName} is not setup/configured for Budget Billing!";
                ldcValidationStatus = false;
            }
            else
            {
                DataRow marketdr = marketdt.Rows[0];

                int marketID =  CIS.Framework.Data.Utility.GetInt32(marketdr, "MarketID");

                if (marketID != 1) // Avoid the following validation for Texas Market
                {
                    DataTable dt = DB.GetBudgetBillingEnabledLDC(_appConfig.ConnectionCsr, ldcShortName);

                    if (dt.Rows.Count == 0)
                    {
                        ldcValidationErrorMessage += $"LDC {ldcShortName} is not setup/configured for Budget Billing!";
                        ldcValidationStatus = false;
                    }
                    else if (dt.Rows.Count == 1)
                    {
                        DataRow dr = dt.Rows[0];

                        int budgetBillingAllowed = CIS.Framework.Data.Utility.GetInt32(dr, "IsBudgetBillingInvoicingAllowed");
                        if (budgetBillingAllowed != 1)
                        {
                            ldcValidationErrorMessage += $"LDC {ldcShortName} is not setup/configured for Budget Billing!";
                            ldcValidationStatus = false;
                        }
                    }
                }
            }
        }

        private static (string errorMessage, int enrollCustId) EnrollCustomerWithPaymentInfo(EnrollmentDataWithPaymentInfo enrollmentData)
        {
            var toReturn = (message:"", custId:0);
            string errorMessage = "";
            int enrollCustId = 0;
            
            //For Logging
            const string methodName = "Service.EnrollCustomerWithPaymentInfo";
            const string paymentInfoErrorMessage = "Payment information was not inserted!";

            try
            {

                int taxAssessment = 255;
                if (enrollmentData.StateTaxExempt)
                    taxAssessment = taxAssessment - 1;

                if (enrollmentData.LocalTaxExempt)
                    taxAssessment = taxAssessment - 4;

                if (enrollmentData.UtilityTaxExempt)
                    taxAssessment = taxAssessment - 8;

                if (enrollmentData.TXUtilityTaxExempt)
                    taxAssessment = taxAssessment - 16;

                var enrollment = new CIS.Clients.Accent.Import.Enrollment(_clientConfig.ConnectionBillingAdmin, _appConfig.ConnectionCsr, _appConfig.ConnectionTdsp);

                var enrollManagerDataContract =
                    new EnrollmentDataContract().
                        WebDataContractToAccentEnrollContract(enrollmentData);

                IEnrollOutputContract enrollOutput = new EnrollOutputContract();
                enrollOutput.IsSuccess = true;

                string ldcValidationErrorMessage = string.Empty;
                bool ldcValidationStatus = enrollOutput.IsSuccess;
                string trueUpType = string.Empty;
                string trueUpPeriod = string.Empty;

                if (enrollManagerDataContract.PaymentPlanType == 1)
                {
                    if (enrollManagerDataContract.PaymentPlanAmount <= 0)
                    {
                        enrollOutput.ErrorMessage = "Budget billing amount must be greater than 0! <br />";
                        enrollOutput.IsSuccess = false;
                    }

                    trueUpType = enrollManagerDataContract.TrueUpType;
                    trueUpPeriod = enrollManagerDataContract.TrueUpPeriod;

                    // TrueUpType, TrueUpPeriod and LDC Configuration Validation for PaymentPlanType  == 1 and PaymentPlanAmount > 0
                    CheckForLDCConfigAndTrueUps(enrollManagerDataContract.TdspCode, ref trueUpType, ref trueUpPeriod, ref ldcValidationErrorMessage, ref ldcValidationStatus);

                    enrollOutput.ErrorMessage += ldcValidationErrorMessage;
                    enrollOutput.IsSuccess = ldcValidationStatus;

                    enrollManagerDataContract.TrueUpType = trueUpType;
                    enrollManagerDataContract.TrueUpPeriod = trueUpPeriod;
                }
                else if (enrollManagerDataContract.PaymentPlanType == 0)
                {
                    if (enrollManagerDataContract.PaymentPlanAmount > 0)
                    {
                        // TrueUpType, TrueUpPeriod and LDC Configuration Validation for PaymentPlanType  == 0 and PaymentPlanAmount > 0

                        trueUpType = enrollManagerDataContract.TrueUpType;
                        trueUpPeriod = enrollManagerDataContract.TrueUpPeriod;

                        CheckForLDCConfigAndTrueUps(enrollManagerDataContract.TdspCode, ref trueUpType, ref trueUpPeriod, ref ldcValidationErrorMessage, ref ldcValidationStatus);

                        enrollOutput.ErrorMessage += ldcValidationErrorMessage;
                        enrollOutput.IsSuccess = ldcValidationStatus;

                        enrollManagerDataContract.TrueUpType = trueUpType;
                        enrollManagerDataContract.TrueUpPeriod = trueUpPeriod;
                    }
                }

                if (enrollOutput.IsSuccess)
                {
                    enrollOutput = enrollment.Enroll(enrollManagerDataContract);
                }

                if (enrollOutput.IsSuccess)
                {
                    if (enrollmentData.AutoPayDebitType.Equals(1) || enrollmentData.AutoPayDebitType.Equals(2))
                    {
                        try
                        {
                            {
                                int rowCount = DB.EnrollCustomerPaymentInfo(enrollOutput.EnrollCustId, enrollmentData, _appConfig.ConnectionCsr);
                                enrollOutput.ErrorMessage = (
                                                                 rowCount > 0
                                                                 ? enrollOutput.ErrorMessage
                                                                 : $"{enrollOutput.ErrorMessage} {paymentInfoErrorMessage}, Zero records inserted."
                                                             );
                            }
                        }
                        catch (Exception ex)
                        {
                            enrollOutput.ErrorMessage = $"{enrollOutput.ErrorMessage} {paymentInfoErrorMessage}, {ex.Message}";
                        }
                    }
                }

                //Remove CC And ACH #'s brfore writing log for security
                enrollmentData.AutoPayCreditCardNumber = string.IsNullOrEmpty(enrollmentData.AutoPayCreditCardNumber) ? "" : "XXX-Removed for Security-XXX";
                enrollmentData.AutoPayACHAccountNumber = string.IsNullOrEmpty(enrollmentData.AutoPayACHAccountNumber) ? "" : "XXX-Removed for Security-XXX";

                toReturn.message = enrollOutput.ErrorMessage;
                toReturn.custId = enrollOutput.EnrollCustId;
                _logger.Info(methodName);
            }
            catch (Exception ex)
            {
                
                _logger.Error(ex, methodName);
            }

            return toReturn;
        }

        private static void CopyProductAndRate(string productCode, int maxDepth = 5)
        {
            //copy +RateDetail, +Rate, +Product, PTC?
            string sql = $@"
USE daes_Accent
DECLARE @ProductCode VARCHAR(50) = '{productCode}';
DECLARE @MAX_DEPTH INT = {maxDepth};
DECLARE @CURR_PRODUCT_ID INT = 0;
DECLARE @PREV_PRODUCT_ID INT = (SELECT ProductId FROM saes_Accent..Product WHERE ProductCode = @ProductCode);
DECLARE @RATE_ID INT = 0;
DECLARE @CTR INT = 0;

DECLARE @Products TABLE (cpProductId INT, cpRollOverProductID INT);
DECLARE @Rates TABLE (RateId INT);

WHILE @CTR < @MAX_DEPTH
BEGIN
	SELECT @RATE_ID = RateId, @CURR_PRODUCT_ID = RollOverProductId FROM Product WHERE ProductId = @PREV_PRODUCT_ID;
	INSERT INTO @Rates VALUES (@RATE_ID)
	
	IF @CURR_PRODUCT_ID = @PREV_PRODUCT_ID --if there
	BEGIN
	 SET @CTR = @MAX_DEPTH;
	 INSERT INTO @Products VALUES (@PREV_PRODUCT_ID, @CURR_PRODUCT_ID);
	END
	ELSE
	IF @CTR = @MAX_DEPTH - 1
	BEGIN
	 SET @CTR = @CTR + 1;
	 INSERT INTO @Products VALUES (@CURR_PRODUCT_ID, @CURR_PRODUCT_ID);
	END
	ELSE
	BEGIN
	 SET @CTR = @CTR + 1;
	 INSERT INTO @Products VALUES (@PREV_PRODUCT_ID, @CURR_PRODUCT_ID);
	 SET @PREV_PRODUCT_ID = @CURR_PRODUCT_ID;
	END
END

PRINT 'Copy Rate'
SET IDENTITY_INSERT daes_Accent..Rate ON
INSERT INTO daes_Accent..Rate
( [RateID], [CSPID], [RateCode], [RateDesc], [EffectiveDate], [ExpirationDate], [RateType], [PlanType], [IsMajority], [TemplateFlag], [LDCCode], [CreateDate], [UserID], [RatePackageName], [CustType], [ServiceType], [DivisionCode], [ConsUnitId], [ActiveFlag], [LDCRateCode], [migr_plan_id], [migr_custid])
SELECT
 [RateID], [CSPID], [RateCode], [RateDesc], [EffectiveDate], [ExpirationDate], [RateType], [PlanType], [IsMajority], [TemplateFlag], [LDCCode], [CreateDate], [UserID], [RatePackageName], [CustType], [ServiceType], [DivisionCode], [ConsUnitId], [ActiveFlag], [LDCRateCode], [migr_plan_id], [migr_custid]
FROM saes_Accent..Rate src
WHERE
  src.RateId IN (SELECT RateId FROM @Rates)
  AND NOT EXISTS(SELECT 1 FROM daes_Accent..Rate dst WHERE src.RateId = dst.RateId )
SET IDENTITY_INSERT daes_Accent..Rate OFF


PRINT 'Copy RateDetail'
SET IDENTITY_INSERT daes_Accent..RateDetail ON
INSERT INTO daes_Accent..RateDetail
( [RateDetID], [RateID], [CategoryID], [RateTypeID], [ConsUnitID], [RateDescID], [EffectiveDate], [ExpirationDate], [RateAmt], [RateAmt2], [RateAmt3], [FixedAdder], [MinDetAmt], [MaxDetAmt], [GLAcct], [RangeLower], [RangeUpper], [CustType], [Graduated], [Progressive], [AmountCap], [MaxRateAmt], [MinRateAmt], [CategoryRollup], [Taxable], [ChargeType], [MiscData1], [FixedCapRate], [ScaleFactor1], [ScaleFactor2], [TemplateRateDetID], [Margin], [HALRateDetailId], [UsageClassId], [LegacyRateDetailId], [Building], [ServiceTypeID], [TaxCategoryID], [UtilityID], [UtilityInvoiceTemplateDetailID], [Active], [StatusID], [RateVariableTypeId], [MinDays], [MaxDays], [BlockPriceIndicator], [RateTransitionId], [CreateDate], [MeterMultiplierFlag], [BlendRatio], [ContractVolumeID], [CreatedByUserId], [ModifiedByUserId], [ModifiedDate], [TOUTemplateID], [TOUTemplateRegisterID], [TOUTemplateRegisterName])
SELECT
 [RateDetID], [RateID], [CategoryID], [RateTypeID], [ConsUnitID], [RateDescID], [EffectiveDate], [ExpirationDate], [RateAmt], [RateAmt2], [RateAmt3], [FixedAdder], [MinDetAmt], [MaxDetAmt], [GLAcct], [RangeLower], [RangeUpper], [CustType], [Graduated], [Progressive], [AmountCap], [MaxRateAmt], [MinRateAmt], [CategoryRollup], [Taxable], [ChargeType], [MiscData1], [FixedCapRate], [ScaleFactor1], [ScaleFactor2], [TemplateRateDetID], [Margin], [HALRateDetailId], [UsageClassId], [LegacyRateDetailId], [Building], [ServiceTypeID], [TaxCategoryID], [UtilityID], [UtilityInvoiceTemplateDetailID], [Active], [StatusID], [RateVariableTypeId], [MinDays], [MaxDays], [BlockPriceIndicator], [RateTransitionId], [CreateDate], [MeterMultiplierFlag], [BlendRatio], [ContractVolumeID], [CreatedByUserId], [ModifiedByUserId], [ModifiedDate], [TOUTemplateID], [TOUTemplateRegisterID], [TOUTemplateRegisterName]
FROM saes_Accent..RateDetail src
WHERE
  src.RateId IN (SELECT RateId FROM @Rates)
  AND NOT EXISTS(SELECT 1 FROM daes_Accent..RateDetail dst WHERE src.RateDetId = dst.RateDetId )
SET IDENTITY_INSERT daes_Accent..RateDetail OFF


PRINT 'Copy Product'
SET IDENTITY_INSERT daes_Accent..Product ON
INSERT INTO daes_Accent..Product
( [ProductID], [RateID], [LDCCode], [PlanType], [TDSPTemplateID], [Description], [BeginDate], [EndDate], [CustType], [Graduated], [RangeTier1], [RangeTier2], [SortOrder], [ActiveFlag], [Uplift], [CSATDSPTemplateID], [CAATDSPTemplateID], [PriceDescription], [MarketingCode], [RateTypeID], [ConsUnitID], [Default], [DivisionCode], [RateDescription], [ServiceType], [CSPId], [TermsId], [RolloverProductId], [CommissionId], [CommissionAmt], [CancelFeeId], [MonthlyChargeId], [ProductCode], [RatePackageId], [ProductName], [TermDate], [DiscountTypeId], [DiscountAmount], [ProductZoneID], [IsGreen], [IsBestChoice], [ActiveEnrollmentFlag], [DepositAmount], [CreditScoreThreshold], [Incentives])
SELECT
 [ProductID], [RateID], [LDCCode], [PlanType], [TDSPTemplateID], [Description], [BeginDate], [EndDate], [CustType], [Graduated], [RangeTier1], [RangeTier2], [SortOrder], [ActiveFlag], [Uplift], [CSATDSPTemplateID], [CAATDSPTemplateID], [PriceDescription], [MarketingCode], [RateTypeID], [ConsUnitID], [Default], [DivisionCode], [RateDescription], [ServiceType], [CSPId], [TermsId], [cpRolloverProductId], [CommissionId], [CommissionAmt], [CancelFeeId], [MonthlyChargeId], [ProductCode], [RatePackageId], [ProductName], [TermDate], [DiscountTypeId], [DiscountAmount], [ProductZoneID], [IsGreen], [IsBestChoice], [ActiveEnrollmentFlag], [DepositAmount], [CreditScoreThreshold], [Incentives]
FROM @Products cp
INNER JOIN saes_Accent..Product src ON cp.cpProductId = src.ProductId
WHERE
  src.ProductID IN (SELECT ProductID FROM @Products)
  AND NOT EXISTS(SELECT 1 FROM daes_Accent..Product dst WHERE src.ProductId = dst.ProductId )
SET IDENTITY_INSERT daes_Accent..Product OFF


PRINT 'Copy PriceToLDC'
SET IDENTITY_INSERT daes_Accent..PriceToLDC ON
INSERT INTO daes_Accent..PriceToLDC
( [PriceToLDCID], [ActiveDate], [LDCID], [DUNS], [EdcCode], [EDICode], [ProductCode], [MegaPrice], [UnitPrice], [ServiceType], [Description], [UtilityAccountNumber])
SELECT
 [PriceToLDCID], [ActiveDate], [LDCID], [DUNS], [EdcCode], [EDICode], [ProductCode], [MegaPrice], [UnitPrice], [ServiceType], [Description], [UtilityAccountNumber]
FROM saes_Accent..PriceToLDC src
WHERE
  src.ProductCode IN (SELECT ProductCode FROM saes_Accent..Product WHERE ProductId IN (SELECT ProductID FROM @Products))
  AND NOT EXISTS(SELECT 1 FROM daes_Accent..PriceToLDC dst WHERE src.PriceToLDCID = dst.PriceToLDCID )
SET IDENTITY_INSERT daes_Accent..PriceToLDC OFF

";
            SqlHelper.ExecuteNonQuery(_appConfig.ConnectionCsr, CommandType.Text, sql);
        }

        private static void Execute814Import()
        {
            ExecuteTaskToasterTask();
        }

        private static void MakeCTRAccepted()
        {
            
        }

        private static void GenerateEvents()
        {
            var maintenance = new MyMaintenance(_appConfig.ConnectionCsr, _appConfig.ConnectionMarket, _clientConfig.ConnectionBillingAdmin);
            maintenance.GenerateEvents();
        }

        private static void ProcessEvents()
        {
            var engine = new CIS.Engine.Event.Queue(_clientConfig.ConnectionBillingAdmin);
            engine.ProcessEventQueue(_appConfig.ClientID, _appConfig.ConnectionCsr, _appConfig.ConnectionMarket, _appConfig.ClientAbbreviation);
        }

        private static void ExecuteTaskToasterTask()
        {
            
        }

        private sealed class DB
        {
            #region EnrollCustomerWithPaymentInfo
            public static DataTable GetBudgetBillingEnabledLDC(string connectionString, string LDCShortName)
            {
                SqlParameter[] arrParms = new SqlParameter[1];

                arrParms[0] = new SqlParameter("@LDCShortName", SqlDbType.VarChar);
                arrParms[0].Value = LDCShortName;

                return SqlHelper.ExecuteDataset(connectionString
                    , CommandType.StoredProcedure
                    , "cspIsBudgetBillingConfiguredLDC"
                    , arrParms
                ).Tables[0];
            }

            public static DataTable GetMarketBasedonLDC(string connectionString, string LDCShortName)
            {
                SqlParameter[] arrParms = new SqlParameter[1];

                arrParms[0] = new SqlParameter("@LDCShortName", SqlDbType.VarChar);
                arrParms[0].Value = LDCShortName;

                return SqlHelper.ExecuteDataset(connectionString
                    , CommandType.StoredProcedure
                    , "cspGetMarketIDByLDC"
                    , arrParms
                ).Tables[0];
            }


            public static int EnrollCustomerPaymentInfo(int enrollCustId, EnrollmentDataWithPaymentInfo enrollmentData, string connectionStr)
            {
                string encryptedNumber = "";
                int parsedMonth = default(int);
                int parsedYear = default(int);

                var crypto = new Viterra.Common.Crypto(Viterra.Common.Crypto.SymmetricProvider.DES);
                string salt = crypto.CreateSalt(5);

                var enrollDataWPayInfo = enrollmentData;

                if (enrollDataWPayInfo.AutoPayDebitType == 1) //Credit Card
                    encryptedNumber = crypto.Encrypt(CIS.Framework.Data.Utility.DigitsOnly(enrollDataWPayInfo.AutoPayCreditCardNumber), salt);

                if (enrollDataWPayInfo.AutoPayDebitType == 2) //ACH
                    encryptedNumber = crypto.Encrypt(CIS.Framework.Data.Utility.DigitsOnly(enrollDataWPayInfo.AutoPayACHAccountNumber), salt);

                if (!string.IsNullOrEmpty(enrollDataWPayInfo.AutoPayCreditCardExpirationMMYYYY))
                {
                    if (enrollDataWPayInfo.AutoPayCreditCardExpirationMMYYYY.Length == 6)
                    {
                        int.TryParse(enrollDataWPayInfo.AutoPayCreditCardExpirationMMYYYY.Substring(0, 2), out parsedMonth);
                        int.TryParse(enrollDataWPayInfo.AutoPayCreditCardExpirationMMYYYY.Substring(2, 4), out parsedYear);
                    }

                    if (enrollDataWPayInfo.AutoPayCreditCardExpirationMMYYYY.Length == 5)
                    {
                        int.TryParse(enrollDataWPayInfo.AutoPayCreditCardExpirationMMYYYY.Substring(0, 1), out parsedMonth);
                        int.TryParse(enrollDataWPayInfo.AutoPayCreditCardExpirationMMYYYY.Substring(1, 4), out parsedYear);
                    }
                    else
                        throw new ApplicationException($"Invalid credit card expiration, the format should be MMYYYY. Provided value: {enrollDataWPayInfo.AutoPayCreditCardExpirationMMYYYY}");
                }

                var list = new List<SqlParameter>
                               {
                                   new SqlParameter("EnrollCustId", enrollCustId),
                                   new SqlParameter("DebitType", enrollDataWPayInfo.AutoPayDebitType),
                                   new SqlParameter("ACHType", enrollDataWPayInfo.AutoPayACHType),
                                   new SqlParameter("ACHAccountName",  !String.IsNullOrEmpty(enrollDataWPayInfo.AutoPayACHAccountName) ? enrollDataWPayInfo.AutoPayACHAccountName : string.Empty),
                                   new SqlParameter("ACHAccountNumber", enrollDataWPayInfo.AutoPayDebitType == 2 ? encryptedNumber : string.Empty),
                                   new SqlParameter("ACHRoutingNumber", !String.IsNullOrEmpty(enrollDataWPayInfo.AutoPayACHRoutingNumber) ?  enrollDataWPayInfo.AutoPayACHRoutingNumber : string.Empty),
                                   new SqlParameter("CreditCardName", !String.IsNullOrEmpty(enrollDataWPayInfo.AutoPayCrediCardName) ? enrollDataWPayInfo.AutoPayCrediCardName : string.Empty),
                                   new SqlParameter("CreditCardNumber",  enrollDataWPayInfo.AutoPayDebitType == 1 ? encryptedNumber : string.Empty),
                                   new SqlParameter("CreditCardType", enrollDataWPayInfo.AutoPayCreditCardType),
                                   new SqlParameter("CreditCardCID", !String.IsNullOrEmpty(enrollDataWPayInfo.AutoPayCreditCardCID) ? enrollDataWPayInfo.AutoPayCreditCardCID : string.Empty),
                                   new SqlParameter("CreditCardExpirationMonth", parsedMonth),
                                   new SqlParameter("CreditCardExpirationYear", parsedYear),
                                   new SqlParameter("Salt", salt)
                               };

                return SqlHelper.ExecuteNonQuery(connectionStr, CommandType.StoredProcedure, "cspEnrollPaymentInfoInsert", list.ToArray());
            }
            #endregion
        }
        
    }
}

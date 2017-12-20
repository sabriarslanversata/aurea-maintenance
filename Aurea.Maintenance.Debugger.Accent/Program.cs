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
    using Aurea.Logging;
    using CIS.BusinessEntity;
    using System.Transactions;
    using System.Collections;
    using System.Data;
    using System.Data.SqlClient;
    using System.IO;
    using System.Reflection;
    using CIS.Clients.Accent.EnrollmentManager;
    using CIS.Clients.Accent.Import;
    using CIS.Framework.Common;
    using CIS.Framework.Data;
    using CIS.Web.Services.Clients.Accent.Helpers;

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

        public class MyImport : CIS.Import.BaseImport
        {
            public MyImport() : base()
            {
                Client = ClientConfigurationFactory.ClientCode;
                ServiceName = "Import";
            }

            public MyImport(string connectionMarket, string connectionCsr, string connectionTdsp,
                string connectionAdmin) : base(connectionMarket, connectionCsr, connectionTdsp, connectionAdmin)
            {
                Client = ClientConfigurationFactory.ClientCode;
                ServiceName = "Import";
            }

            public void myImportTransaction()
            {
                this.ImportTransaction();

            }
        }

        public class MyExport : CIS.Clients.Accent.Export.MainProcess//CIS.Export.BaseExport
        {
            private static readonly string _uaaDir = Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location),  "uua\\");
            private static readonly string _uqcDir = Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location) , "uqc\\");

            public MyExport(string connectionMarket, string connectionCsr, string connectionAdmin)
            {
                _connectionMarket = connectionMarket;
                _connectionCsr = connectionCsr;
                _connectionAdmin = connectionAdmin;
                _serviceExecuteDate = DateTime.Now;
                LoadConfiguration(string.Empty);
            }

            public void MyExportTransactions()
            {
                ExportTransactions();
            }

            public void MyTransmitFiles()
            {
                TransmitFiles();
            }

            public void MyExportLoadForecasting()
            {
                //ExportLoadForecasting();//private and exists on Texpo only, so if we need to debug this we need to implement logic here again
            }

            public void MyProcessEFTPayments()
            {
                ProcessEFTPayments();//protected virtual
            }

            public void MyRunAutoDNPProcess()
            {
                //RunAutoDNPProcess();//private and exists on Texpo only, so if we need to debug this we need to implement logic here again
            }

            public void MyLogService()
            {
                LogService();//protected derived from BaseService
            }

            public override void InitializeVariables(string exportFunction)
            {
                _directoryFtpOut = Path.Combine(_uaaDir, @"Data\ClientData\ACC\Services\Transport\Ftp\Out\");
                _directoryEncrypted = Path.Combine(_uaaDir, @"Data\Clientdata\ACC\Services\Transport\Ftp\Out\Market\Encrypted\");
                _directoryDecrypted = Path.Combine(_uaaDir, @"Data\Clientdata\ACC\Services\Transport\Ftp\Out\Market\Decrypted\");
                _directoryArchive = Path.Combine(_uaaDir, @"Data\Clientdata\ACC\Services\TransportArchive\Ftp\Out\");
                _directoryException = Path.Combine(_uaaDir, @"Data\Clientdata\ACC\Services\Transport\Ftp\Out\Market\Exception\");
                _pgpPassPhrase = "hokonxoyg";
                _pgpEncryptionKey = Path.Combine(_uqcDir, @"Data\PGPKeys\ista-na.asc");
                _pgpSignatureKey = Path.Combine(_uaaDir, @"Data\PGPKeys\ista-na.asc");
                _ftpRemoteServer = "localhost";
                _ftpRemoteDirectory = string.Empty;
                _ftpUserName = string.Empty;
                _ftpUserPassword = string.Empty;
                _runHour = "6";
                _runDay = "*";
                _runDayOfWeek = "*";
                _marketFileVersion = "3.0";
                _serviceInterval = 5;
                _historicalUsageRequestType = "HU";
                _clientID = _clientConfig.ClientId;
                if (!Directory.Exists(_directoryDecrypted))
                    Directory.CreateDirectory(_directoryDecrypted);

                if (!Directory.Exists(_directoryEncrypted))
                    Directory.CreateDirectory(_directoryEncrypted);

                if (!Directory.Exists(_directoryException))
                    Directory.CreateDirectory(_directoryException);


                if (!File.Exists(_pgpEncryptionKey))
                {
                    Directory.CreateDirectory(_pgpEncryptionKey.Replace(Path.GetFileName(_pgpEncryptionKey), ""));
                    File.WriteAllText(_pgpEncryptionKey, @"-----BEGIN PGP PRIVATE KEY BLOCK-----
Version: BCPG C# v1.6.1.0

lQOsBFn53zQBCADMlxdkpJRay9J1lA2Xc6fsyM0A7CRzcY5TCuU31ZROV0s8Z+97
hpwAdRzRXPfwh7465505tW2q3hqpOOyMmXXAbua8fVn/hklXxVnCB0WQFLHIL/Dm
UyJnRy9vyKkt5QGzAhLMDWtRn/bejIJ8jzB8WFjf+luRpR/GdvPjy8Q7/0tY6By4
Ua67nlHTFkHJ7iytE1ShbNHoe1BtsuEdwu5BmNHDkixpUAFvm5MoC0Xvib4yvAVj
1+r/U9w5AFGMoNZrYO4Ckg3ZKCHwNMvI0sD0zHnW36jGV4yoNBQ+Uc3bBaxZKoy9
DxLIWT+066UqD2V3pIcObQT8xVQybxIHzoxFABEBAAH/AwMCB5I8JHaajnJgZ10f
dxEBzvlTmxBq2x4WGVvfhV6vks1CAu+KO6pGxaskJG/rNIXBrDv5g4dO3hIjaDQX
tOuyAp3WB+b06EnCRuXsUBQcDPoRZ8SfNlUkyNCEh9M3sDHng9nfVbGd5jjpFHiU
1Uw1OOGNd6TZLrmTzwqTpMoKs/cGw5GnT1LOsAy57oG0UuF9KVeknkOApN6K3Mez
VX6LiheaS8izV4Vfgr4pZn+GrL97iV41CITxPDOqL0AJvXmzAzT95324Bjo6+WAW
q3geDzMgd0udO4n/DQ1NSpBz1Wjvd9f60RDxfavpYKntEyy074ZbUAEGFE5CQhOy
r4JQQcXGzkkRK65/lkVbDsUAe2ICH2WQ5KEHe1gNEJyBQZr/s1XK0xO7B3EwPzDT
qdgN47H0DA7pXAbb3QxVxNTVx8HZkzxySJjmjrNvOC3VqKtDkQwY4dIDQ+R2ArrQ
7h9F/XdUAAFcw8XdtISl5XHWH13l3ZQb5DxpMOtjazLnFu5P00Drf9NyftO6eAY8
Xph+T1Oq24HXIO91zKsjpaV4Ru3U/5cxP8jp3rBLHkhWFnQrV6mVNuXeXPHYzMhj
LVxeTIoY/ELuM17fouyOEMhNLtDX3JZ5AXLctP9BEr5mwnjIJMmmB2yOOXMKZYeL
UO0pTBzgTI4BzlEpCcUQ2G8GAbpSN2f6R9trVgWNaX+hnC5GBv2yMIFfeKMIasS1
soGP5C6NPCBJDfd5iztfz8YLYwoaIb9d6HsQ06Xeylb/5cwpjehC4iL9OFI4emaJ
Ghrf/MeFfQVDdhM2MULaiJGMrWCvJNy+VMQfiX11/5iFtWgM6FK9D8yEdj2L7PEH
t/WzujPxWTS/AtnI7xnNhc7sb82LRsV7R2US7XTLDbQYc2FicmkuYXJzbGFuQHZl
cnNhdGEuY29tiQEcBBABAgAGBQJZ+d80AAoJEGHYYRGYWV281UkH/2wW6vqlqRzT
G2WFGAJEf3SoqYiEb5rJvA0aI10izRla2hzWV45F8xafmQEQN6ncU8k3cFIVdXw8
4Jq4NBlkIVngpbE7I1WqhiP5snFmJhdlpGjTdUvtKnSZNT/qMrdsyBwrOJ9SkmTO
NMG76HYXOGYJV0wxMh9PyABx+IzIHR/jdZ/5wDxhv76O/cV5oLcX/TK6UAjuQchO
drGAQFcbiwOlXv1wz8x4LrchcPgd2c5l9elozFLlDSKtukAnRIpgcNr71mv36/xF
bIICDu7Y9DBejbH0JPwumR3M6L4tVPAvgH1jcVzW28yF/qHrtfIoY+o1H/e7PF1v
XHfN4TUbhDg=
=sYfZ
-----END PGP PRIVATE KEY BLOCK-----
", Encoding.UTF8);
                }

                if (!File.Exists(_pgpSignatureKey))
                {
                    Directory.CreateDirectory(_pgpSignatureKey.Replace(Path.GetFileName(_pgpSignatureKey), ""));
                    File.WriteAllText(_pgpSignatureKey, @"-----BEGIN PGP PUBLIC KEY BLOCK-----
Version: BCPG C# v1.6.1.0

mQENBFn53zQBCADMlxdkpJRay9J1lA2Xc6fsyM0A7CRzcY5TCuU31ZROV0s8Z+97
hpwAdRzRXPfwh7465505tW2q3hqpOOyMmXXAbua8fVn/hklXxVnCB0WQFLHIL/Dm
UyJnRy9vyKkt5QGzAhLMDWtRn/bejIJ8jzB8WFjf+luRpR/GdvPjy8Q7/0tY6By4
Ua67nlHTFkHJ7iytE1ShbNHoe1BtsuEdwu5BmNHDkixpUAFvm5MoC0Xvib4yvAVj
1+r/U9w5AFGMoNZrYO4Ckg3ZKCHwNMvI0sD0zHnW36jGV4yoNBQ+Uc3bBaxZKoy9
DxLIWT+066UqD2V3pIcObQT8xVQybxIHzoxFABEBAAG0GHNhYnJpLmFyc2xhbkB2
ZXJzYXRhLmNvbYkBHAQQAQIABgUCWfnfNAAKCRBh2GERmFldvNVJB/9sFur6pakc
0xtlhRgCRH90qKmIhG+aybwNGiNdIs0ZWtoc1leORfMWn5kBEDep3FPJN3BSFXV8
POCauDQZZCFZ4KWxOyNVqoYj+bJxZiYXZaRo03VL7Sp0mTU/6jK3bMgcKzifUpJk
zjTBu+h2FzhmCVdMMTIfT8gAcfiMyB0f43Wf+cA8Yb++jv3FeaC3F/0yulAI7kHI
TnaxgEBXG4sDpV79cM/MeC63IXD4HdnOZfXpaMxS5Q0irbpAJ0SKYHDa+9Zr9+v8
RWyCAg7u2PQwXo2x9CT8LpkdzOi+LVTwL4B9Y3Fc1tvMhf6h67XyKGPqNR/3uzxd
b1x3zeE1G4Q4
=Yf0P
-----END PGP PUBLIC KEY BLOCK-----
", Encoding.UTF8);
                }

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

            System.Transactions.TransactionManager.DistributedTransactionStarted += delegate
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
            GenerateEvents(new List<int> { 10, 18});
            ProcessEvents();
            Execute814Export();
            GenerateEvents(new List<int> { 10, 18 });
            ProcessEvents();

            //MakeCTRAccepted(enrollCustId);
            Execute814Import();
            GenerateEvents(new List<int> { 10, 18 });
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

        private static void Execute814Export()
        {
            //exec csp_GetServiceMethods 2 (Export = ExportTransactions, EncryptFiles, TransmitFiles, ExportLoadForecasting, ProcessEFTPayments, RunAutoDNPProcess, LogService)
            var myExport = new MyExport(_appConfig.ConnectionMarket, _appConfig.ConnectionCsr, _clientConfig.ConnectionBillingAdmin);
            myExport.MyExportTransactions();
            //myExport.EncryptFiles();
            //myExport.MyTransmitFiles();
            //myExport.MyExportLoadForecasting();
            //myExport.MyProcessEFTPayments();
            //myExport.MyRunAutoDNPProcess();
            //myExport.MyLogService();

            /*
            var exporter = new CIS.Export.Billing.Market814(_clientConfiguration.ConnectionMarket, _clientConfiguration.ConnectionCsr)
            {
                HistoricalUsageRequestType = _clientConfiguration.ExportHistoricalUsageRequestType,
                ConnectionAdmin = Utility.BillingAdminDEV,
                Client = _clientConfiguration.ConnectionCsr,
                ClientID = Utility.Clients["TXP"]
            };
            exporter.Export();
            */
        }

        private static void Execute814Import()
        {
            var baseImport = new MyImport()
            {
                ConnectionAdmin = _clientConfig.ConnectionBillingAdmin,
                ConnectionMarket = _appConfig.ConnectionMarket,
                ConnectionCsr = _appConfig.ConnectionCsr,
                ClientID = _clientConfig.ClientId,
                Client = _clientConfig.Client

            };
            baseImport.myImportTransaction();
        }

        private static void clearOldCtr(int enrollCustId)
        {
            var sql = $@"
USE daes_Accent;
DECLARE @EnrollCustId INT = {enrollCustId};
DECLARE @CustId INT = (SELECT CsrCustId FROM daes_Accent..EnrollCustomer WHERE EnrollCustId = @EnrollCustId);
DECLARE @SourceId INT = (SELECT RequestId FROM daes_Accent..CustomerTranscationRequest WHERE CustId = @CustId AND TransactionType = '814' AND ActionCode = '05' AND Direction = 1)
DECLARE @EightFourteenKey = (SELECT SourceId FROM daes_Accent..CustomerTranscationRequest WHERE CustId = @CustId AND TransactionType = '814' AND ActionCode = '05' AND Direction = 1)
DECLARE @ServiceKey 
DECLARE @MarketFileId INT 
DECLARE @ServiceKey INT
DECLARE @MeterKey INT

SELECT @ServiceKey = Service_Key, @MarketFileId = MarketFileId FROM daes_AccentMarket..tbl_814_Service WHERE [814_Key] = @EightFourteenKey
SELECT @MeterKey = Meter_Key FROM daes_AccentMarket..tbl_814_Service_Meter WHERE Service_Key = @ServiceKey

DELETE FROM daes_AccentMarket..tblMarketFile WHERE [MarketFileId] = @MarketFileId
DELETE FROM daes_AccentMarket..tbl_814_Service_Meter_Type WHERE Meter_Key = @MeterKey
DELETE FROM daes_AccentMarket..tbl_814_Service_Meter WHERE Service_Key = @ServiceKey
DELETE FROM daes_AccentMarket..tbl_814_Name WHERE 814_Key = @EightFourteenKey
DELETE FROM daes_AccentMarket..tbl_814_Service WHERE Service_Key = @ServiceKey
DELETE FROM daes_AccentMarket..tbl_814_Header WHERE [814_Key] = @EightFourteenKey
DELETE FROM daes_Accent..CustomerTransactionRequest WHERE RequestId = @SourceId

";
            SqlHelper.ExecuteNonQuery(_appConfig.ConnectionCsr, CommandType.Text, sql);
        }

        private static void MakeCTRAccepted(int enrollCustId)
        {
            clearOldCtr(enrollCustId);
            var sql = $@"
USE daes_Accent;
DECLARE @EnrollCustId INT = {enrollCustId};
DECLARE @CustId INT = (SELECT CsrCustId FROM daes_Accent..EnrollCustomer WHERE EnrollCustId = @EnrollCustId);
DECLARE @PremId INT = (SELECT PremId FROM daes_Accent..Premise WHERE CustId = @CustId )
DECLARE @MarketFileId INT 
DECLARE @SourceId INT
DECLARE @RefSourceId INT
DECLARE @RefNumber INT
DECLARE @ServiceKey INT
DECLARE @MeterKey INT
SELECT @RefSourceId = SourceId, @RefNumber = TransactionNumber FROM daes_Accent..CustomerTranscationRequest WHERE CustId = @CustId AND TransactionType = '814' AND ActionCode = '16' AND Direction = 0
SELECT @MarketFileId = MarketFileID FROM saes_AccentMarket..tbl_814_Header WHERE [814_key] = (SELECT SourceID FROM saes_Accent..CustomerTranscationRequest WHERE CustId = @CustId AND TransactionType = '814' AND ActionCode = '05' AND Direction = 1)

SET IDENTITY_INSERT daes_AccentMarket..tblMarketFile ON
INSERT INTO tblMarketFile ( [MarketFileId], [FileName], [FileType], [ProcessStatus], [ProcessDate], [ProcessError], [SenderTranNum], [DirectionFlag], [Status], [LDCID], [CSPDUNSID], [RefMarketFileId], [CreateDate], [CspDunsTradingPartnerID], [TransactionCount])
VALUES (
 @MarketFileId, 'ACT_TX_183529049_814.20170807094914.201843962.txt'/*[FileName]*/, NULL/*[FileType]*/, 'N'/*[ProcessStatus]*/, '2017-08-07 10:01:19.673'/*[ProcessDate]*/, NULL/*[ProcessError]*/, NULL/*[SenderTranNum]*/, 1/*[DirectionFlag]*/, 3/*[Status]*/, 0/*[LDCID]*/, 0/*[CSPDUNSID]*/, NULL/*[RefMarketFileId]*/, '2017-08-07 10:01:41.563'/*[CreateDate]*/, NULL/*[CspDunsTradingPartnerID]*/, NULL/*[TransactionCount]*/
)
SET IDENTITY_INSERT daes_AccentMarket..tblMarketFile OFF

INSERT INTO tbl_814_Header ( [MarketFileId], [TransactionSetId], [TransactionSetControlNbr], [TransactionSetPurposeCode], [TransactionNbr], [TransactionDate], [ReferenceNbr], [ActionCode], [TdspDuns], [TdspName], [CrDuns], [CrName], [ProcessFlag], [ProcessDate], [Direction], [TransactionTypeID], [MarketID], [ProviderID], [POLRClass], [TransactionTime], [TransactionTimeCode], [TransactionQualifier], [TransactionQueueTypeID], [CreateDate])
VALUES (
 @MarketFileId, 5/*[TransactionSetId]*/, NULL/*[TransactionSetControlNbr]*/, '5'/*[TransactionSetPurposeCode]*/, '167528458620170807094645133305'/*[TransactionNbr]*/, '20170807'/*[TransactionDate]*/, @RefNumber/*[ReferenceNbr]*/, 'S'/*[ActionCode]*/, '1039940674000'/*[TdspDuns]*/, 'ONCOR'/*[TdspName]*/, '133305370'/*[CrDuns]*/, 'ACCENT ENERGY TEXAS LP DBA IGS ENERGY (LSE)'/*[CrName]*/, 0/*[ProcessFlag]*/, NULL/*[ProcessDate]*/, 1/*[Direction]*/, 28/*[TransactionTypeID]*/, 1/*[MarketID]*/, 1/*[ProviderID]*/, NULL/*[POLRClass]*/, NULL/*[TransactionTime]*/, NULL/*[TransactionTimeCode]*/, NULL/*[TransactionQualifier]*/, NULL/*[TransactionQueueTypeID]*/, GETDATE()/*[CreateDate]*/
)
SET @SourceId = SCOPE_IDENTITY()

INSERT INTO tbl_814_Service(  [814_Key], [AssignId], [ServiceTypeCode1], [ServiceType1], [ServiceTypeCode2], [ServiceType2], [ServiceTypeCode3], [ServiceType3], [ServiceTypeCode4], [ServiceType4], [ActionCode], [MaintenanceTypeCode], [DistributionLossFactorCode], [PremiseType], [BillType], [BillCalculator], [EsiId], [StationId], [SpecialNeedsIndicator], [PowerRegion], [EnergizedFlag], [EsiIdStartDate], [EsiIdEndDate], [EsiIdEligibilityDate], [NotificationWaiver], [SpecialReadSwitchDate], [PriorityCode], [PermitIndicator], [RTODate], [RTOTime], [CSAFlag], [MembershipID], [ESPAccountNumber], [LDCBillingCycle], [LDCBudgetBillingCycle], [WaterHeaters], [LDCBudgetBillingStatus], [PaymentArrangement], [NextMeterReadDate], [ParticipatingInterest], [EligibleLoadPercentage], [TaxExemptionPercent], [CapacityObligation], [TransmissionObligation], [TotalKWHHistory], [NumberOfMonthsHistory], [PeakDemandHistory], [AirConditioners], [PreviousEsiId], [GasPoolId], [LBMPZone], [ResidentialTaxPortion], [ESPCommodityPrice], [ESPFixedCharge], [ESPChargesCommTaxRate], [ESPChargesResTaxRate], [GasSupplyServiceOption], [FundsAuthorization], [BudgetBillingStatus], [FixedMonthlyCharge], [TaxRate], [CommodityPrice], [MeterCycleCodeDesc], [BillCycleCodeDesc], [FeeApprovedApplied], [MarketerCustomerAccountNumber], [GasSupplyServiceOptionCode], [HumanNeeds], [ReinstatementDate], [MeterCycleCode], [SystemNumber], [StateLicenseNumber], [SupplementalAccountNumber], [NewCustomerIndicator], [PaymentCategory], [PreviousESPAccountNumber], [RenewableEnergyIndicator], [SICCode], [ApprovalCodeIndicator], [RenewableEnergyCertification], [NewPremiseIndicator], [SalesResponsibility], [CustomerReferenceNumber], [TransactionReferenceNumber], [ESPTransactionNumber], [OldESPAccountNumber], [DFIIdentificationNumber], [DFIAccountNumber], [DFIIndicator1], [DFIIndicator2], [DFIQualifier], [DFIRoutingNumber], [SpecialReadSwitchTime], [CustomerAuthorization], [LDCAccountBalance], [DisputedAmount], [CurrentBalance], [ArrearsBalance], [IntervalStatusType], [LDCSupplierBalance], [BudgetPlan], [BudgetInstallment], [Deposit], [RemainingUtilBalanceBucket1], [RemainingUtilBalanceBucket2], [RemainingUtilBalanceBucket3], [RemainingUtilBalanceBucket4], [RemainingUtilBalanceBucket5], [RemainingUtilBalanceBucket6], [UnmeteredAcct], [PaymentOption], [MaxDailyAmt], [MeterAccessNote], [SpecialNeedsExpirationDate], [SwitchHoldStatusIndicator], [SpecialMeterConfig], [MaximumGeneration], [IgnoreRescind], [ServiceDeliveryPoint], [GasCapacityAssignment], [CPAEnrollmentTypes], [DaysInArrears], [RU_Notes], [RD_SiteCharacterDate], [SupplierPricingStructureNr], [SupplierGroupNumber], [IndustrialClassificationCode], [UtilityTaxExemptStatus], [AccountSettlementIndicator], [NypaDiscountIndicator], [UtilityDiscount], [IcapEffectiveDate], [FutureIcapEffectiveDate], [FutureIcap], [ChangeCancellationFee], [CancellationFee], [MunicipalAggregation])
VALUES (
 @SourceId/*[814_Key]*/, 1/*[AssignId]*/, 'SH'/*[ServiceTypeCode1]*/, 'EL'/*[ServiceType1]*/, 'SH'/*[ServiceTypeCode2]*/, 'CE'/*[ServiceType2]*/, 'SH'/*[ServiceTypeCode3]*/, 'MVI'/*[ServiceType3]*/, 'SH'/*[ServiceTypeCode4]*/, 'HU'/*[ServiceType4]*/, 'A'/*[ActionCode]*/, NULL/*[MaintenanceTypeCode]*/, 'A'/*[DistributionLossFactorCode]*/, '01'/*[PremiseType]*/, NULL/*[BillType]*/, NULL/*[BillCalculator]*/, '10443720004064914'/*[EsiId]*/, 'WHTRK'/*[StationId]*/, NULL/*[SpecialNeedsIndicator]*/, NULL/*[PowerRegion]*/, NULL/*[EnergizedFlag]*/, NULL/*[EsiIdStartDate]*/, NULL/*[EsiIdEndDate]*/, NULL/*[EsiIdEligibilityDate]*/, NULL/*[NotificationWaiver]*/, '20170817'/*[SpecialReadSwitchDate]*/, NULL/*[PriorityCode]*/, NULL/*[PermitIndicator]*/, NULL/*[RTODate]*/, NULL/*[RTOTime]*/, NULL/*[CSAFlag]*/, NULL/*[MembershipID]*/, NULL/*[ESPAccountNumber]*/, NULL/*[LDCBillingCycle]*/, NULL/*[LDCBudgetBillingCycle]*/, NULL/*[WaterHeaters]*/, NULL/*[LDCBudgetBillingStatus]*/, NULL/*[PaymentArrangement]*/, NULL/*[NextMeterReadDate]*/, NULL/*[ParticipatingInterest]*/, NULL/*[EligibleLoadPercentage]*/, NULL/*[TaxExemptionPercent]*/, NULL/*[CapacityObligation]*/, NULL/*[TransmissionObligation]*/, NULL/*[TotalKWHHistory]*/, NULL/*[NumberOfMonthsHistory]*/, NULL/*[PeakDemandHistory]*/, NULL/*[AirConditioners]*/, NULL/*[PreviousEsiId]*/, NULL/*[GasPoolId]*/, NULL/*[LBMPZone]*/, NULL/*[ResidentialTaxPortion]*/, NULL/*[ESPCommodityPrice]*/, NULL/*[ESPFixedCharge]*/, NULL/*[ESPChargesCommTaxRate]*/, NULL/*[ESPChargesResTaxRate]*/, NULL/*[GasSupplyServiceOption]*/, NULL/*[FundsAuthorization]*/, NULL/*[BudgetBillingStatus]*/, NULL/*[FixedMonthlyCharge]*/, NULL/*[TaxRate]*/, NULL/*[CommodityPrice]*/, NULL/*[MeterCycleCodeDesc]*/, NULL/*[BillCycleCodeDesc]*/, NULL/*[FeeApprovedApplied]*/, NULL/*[MarketerCustomerAccountNumber]*/, NULL/*[GasSupplyServiceOptionCode]*/, 'N'/*[HumanNeeds]*/, NULL/*[ReinstatementDate]*/, NULL/*[MeterCycleCode]*/, NULL/*[SystemNumber]*/, NULL/*[StateLicenseNumber]*/, NULL/*[SupplementalAccountNumber]*/, NULL/*[NewCustomerIndicator]*/, NULL/*[PaymentCategory]*/, NULL/*[PreviousESPAccountNumber]*/, NULL/*[RenewableEnergyIndicator]*/, NULL/*[SICCode]*/, NULL/*[ApprovalCodeIndicator]*/, NULL/*[RenewableEnergyCertification]*/, NULL/*[NewPremiseIndicator]*/, NULL/*[SalesResponsibility]*/, NULL/*[CustomerReferenceNumber]*/, NULL/*[TransactionReferenceNumber]*/, NULL/*[ESPTransactionNumber]*/, NULL/*[OldESPAccountNumber]*/, NULL/*[DFIIdentificationNumber]*/, NULL/*[DFIAccountNumber]*/, NULL/*[DFIIndicator1]*/, NULL/*[DFIIndicator2]*/, NULL/*[DFIQualifier]*/, NULL/*[DFIRoutingNumber]*/, NULL/*[SpecialReadSwitchTime]*/, NULL/*[CustomerAuthorization]*/, NULL/*[LDCAccountBalance]*/, NULL/*[DisputedAmount]*/, NULL/*[CurrentBalance]*/, NULL/*[ArrearsBalance]*/, NULL/*[IntervalStatusType]*/, NULL/*[LDCSupplierBalance]*/, NULL/*[BudgetPlan]*/, NULL/*[BudgetInstallment]*/, NULL/*[Deposit]*/, NULL/*[RemainingUtilBalanceBucket1]*/, NULL/*[RemainingUtilBalanceBucket2]*/, NULL/*[RemainingUtilBalanceBucket3]*/, NULL/*[RemainingUtilBalanceBucket4]*/, NULL/*[RemainingUtilBalanceBucket5]*/, NULL/*[RemainingUtilBalanceBucket6]*/, ''/*[UnmeteredAcct]*/, 'N'/*[PaymentOption]*/, NULL/*[MaxDailyAmt]*/, NULL/*[MeterAccessNote]*/, NULL/*[SpecialNeedsExpirationDate]*/, NULL/*[SwitchHoldStatusIndicator]*/, NULL/*[SpecialMeterConfig]*/, NULL/*[MaximumGeneration]*/, NULL/*[IgnoreRescind]*/, NULL/*[ServiceDeliveryPoint]*/, NULL/*[GasCapacityAssignment]*/, NULL/*[CPAEnrollmentTypes]*/, NULL/*[DaysInArrears]*/, NULL/*[RU_Notes]*/, NULL/*[RD_SiteCharacterDate]*/, NULL/*[SupplierPricingStructureNr]*/, NULL/*[SupplierGroupNumber]*/, NULL/*[IndustrialClassificationCode]*/, NULL/*[UtilityTaxExemptStatus]*/, NULL/*[AccountSettlementIndicator]*/, NULL/*[NypaDiscountIndicator]*/, NULL/*[UtilityDiscount]*/, NULL/*[IcapEffectiveDate]*/, NULL/*[FutureIcapEffectiveDate]*/, NULL/*[FutureIcap]*/, NULL/*[ChangeCancellationFee]*/, NULL/*[CancellationFee]*/, NULL/*[MunicipalAggregation]*/
)
SET @ServiceKey = SCOPE_IDENTITY()

INSERT INTO tbl_814_Name ( [814_Key], [EntityIdType], [EntityName], [EntityName2], [EntityName3], [EntityDuns], [EntityIdCode], [Address1], [Address2], [City], [State], [PostalCode], [CountryCode], [ContactCode], [ContactName], [ContactPhoneNbr1], [ContactPhoneNbr2], [ContactPhoneNbr3], [EntityFirstName], [EntityLastName], [CustType], [TaxingDistrict], [EntityMiddleName], [County], [EntityEmail])
VALUES(
 @SourceId/*[814_Key]*/, '8R'/*[EntityIdType]*/, 'OLIVIA T HARJADI'/*[EntityName]*/, NULL/*[EntityName2]*/, NULL/*[EntityName3]*/, NULL/*[EntityDuns]*/, NULL/*[EntityIdCode]*/, '7440 LA VISTA DR APT 269'/*[Address1]*/, NULL/*[Address2]*/, 'DALLAS'/*[City]*/, 'TX'/*[State]*/, '752145814'/*[PostalCode]*/, NULL/*[CountryCode]*/, NULL/*[ContactCode]*/, NULL/*[ContactName]*/, NULL/*[ContactPhoneNbr1]*/, NULL/*[ContactPhoneNbr2]*/, NULL/*[ContactPhoneNbr3]*/, NULL/*[EntityFirstName]*/, NULL/*[EntityLastName]*/, NULL/*[CustType]*/, NULL/*[TaxingDistrict]*/, NULL/*[EntityMiddleName]*/, NULL/*[County]*/, NULL/*[EntityEmail]*/
)
INSERT INTO tbl_814_Service_Meter ( [Service_Key], [EntityIdCode], [MeterNumber], [MeterCode], [MeterType], [LoadProfile], [RateClass], [RateSubClass], [MeterCycle], [MeterCycleDayOfMonth], [SpecialNeedsIndicator], [OldMeterNumber], [MeterOwnerIndicator], [EntityType], [TimeOFUse], [ESPRateCode], [OrganizationName], [FirstName], [MiddleName], [NamePrefix], [NameSuffix], [IdentificationCode], [EntityName2], [EntityName3], [Address1], [Address2], [City], [State], [Zip], [CountryCode], [County], [PlanNumber], [ServicesReferenceNumber], [AffiliationNumber], [CostElement], [CoverageCode], [LossReportNumber], [GeographicNumber], [ItemNumber], [LocationNumber], [PriceListNumber], [ProductType], [QualityInspectionArea], [ShipperCarOrderNumber], [StandardPointLocation], [ReportIdentification], [Supplier], [Area], [CollectorIdentification], [VendorAgentNumber], [VendorAbbreviation], [VendorIdNumber], [VendorOrderNumber], [PricingStructureCode], [MeterOwnerDUNS], [MeterOwner], [MeterInstallerDUNS], [MeterInstaller], [MeterReaderDUNS], [MeterReader], [MeterMaintenanceProviderDUNS], [MeterMaintenanceProvider], [MeterDataManagementAgentDUNS], [MeterDataManagementAgent], [SchedulingCoordinatorDUNS], [SchedulingCoordinator], [MeterInstallPending], [PackageOption], [UsageCode], [MeterServiceVoltage], [LossFactor], [AMSIndicator], [SummaryInterval], [NextCycleRate], [VariableRateIndicator], [RateTerm], [RateTermDateExpirationDate])
VALUES(
  @ServiceKey/*[Service_Key]*/, NULL/*[EntityIdCode]*/, '125892623LG'/*[MeterNumber]*/, NULL/*[MeterCode]*/, 'KHMON'/*[MeterType]*/, 'RESHIWR_NCENT_IDR_WS_NOTOU'/*[LoadProfile]*/, '00'/*[RateClass]*/, '0000'/*[RateSubClass]*/, '17'/*[MeterCycle]*/, NULL/*[MeterCycleDayOfMonth]*/, NULL/*[SpecialNeedsIndicator]*/, NULL/*[OldMeterNumber]*/, NULL/*[MeterOwnerIndicator]*/, NULL/*[EntityType]*/, NULL/*[TimeOFUse]*/, NULL/*[ESPRateCode]*/, NULL/*[OrganizationName]*/, NULL/*[FirstName]*/, NULL/*[MiddleName]*/, NULL/*[NamePrefix]*/, NULL/*[NameSuffix]*/, NULL/*[IdentificationCode]*/, NULL/*[EntityName2]*/, NULL/*[EntityName3]*/, NULL/*[Address1]*/, NULL/*[Address2]*/, NULL/*[City]*/, NULL/*[State]*/, NULL/*[Zip]*/, NULL/*[CountryCode]*/, NULL/*[County]*/, NULL/*[PlanNumber]*/, NULL/*[ServicesReferenceNumber]*/, NULL/*[AffiliationNumber]*/, NULL/*[CostElement]*/, NULL/*[CoverageCode]*/, NULL/*[LossReportNumber]*/, NULL/*[GeographicNumber]*/, NULL/*[ItemNumber]*/, NULL/*[LocationNumber]*/, NULL/*[PriceListNumber]*/, NULL/*[ProductType]*/, NULL/*[QualityInspectionArea]*/, NULL/*[ShipperCarOrderNumber]*/, NULL/*[StandardPointLocation]*/, NULL/*[ReportIdentification]*/, NULL/*[Supplier]*/, NULL/*[Area]*/, NULL/*[CollectorIdentification]*/, NULL/*[VendorAgentNumber]*/, NULL/*[VendorAbbreviation]*/, NULL/*[VendorIdNumber]*/, NULL/*[VendorOrderNumber]*/, NULL/*[PricingStructureCode]*/, NULL/*[MeterOwnerDUNS]*/, NULL/*[MeterOwner]*/, NULL/*[MeterInstallerDUNS]*/, NULL/*[MeterInstaller]*/, NULL/*[MeterReaderDUNS]*/, NULL/*[MeterReader]*/, NULL/*[MeterMaintenanceProviderDUNS]*/, NULL/*[MeterMaintenanceProvider]*/, NULL/*[MeterDataManagementAgentDUNS]*/, NULL/*[MeterDataManagementAgent]*/, NULL/*[SchedulingCoordinatorDUNS]*/, NULL/*[SchedulingCoordinator]*/, NULL/*[MeterInstallPending]*/, NULL/*[PackageOption]*/, NULL/*[UsageCode]*/, NULL/*[MeterServiceVoltage]*/, NULL/*[LossFactor]*/, 'AMSR'/*[AMSIndicator]*/, 'AMSR'/*[SummaryInterval]*/, NULL/*[NextCycleRate]*/, NULL/*[VariableRateIndicator]*/, NULL/*[RateTerm]*/, NULL/*[RateTermDateExpirationDate]*/
)

SET @MeterKey = SCOPE_IDENTITY()

INSERT INTO tbl_814_Service_Meter_Type ([Meter_Key], [MeterMultiplier], [MeterType], [ProductType], [TimeOfUse], [NumberOfDials], [UnmeteredNumberOfDevices], [UnmeteredDescription], [StartMeterRead], [EndMeterRead], [ChangeReason], [TimeOfUse2])
VALUES (
  @MeterKey/*[Meter_Key]*/, 1/*[MeterMultiplier]*/, 'KHMON'/*[MeterType]*/, NULL/*[ProductType]*/, 51/*[TimeOfUse]*/, '5.0'/*[NumberOfDials]*/, NULL/*[UnmeteredNumberOfDevices]*/, NULL/*[UnmeteredDescription]*/, NULL/*[StartMeterRead]*/, NULL/*[EndMeterRead]*/, NULL/*[ChangeReason]*/, NULL/*[TimeOfUse2]*/
)

INSERT INTO daes_Accent..CustomerTransactionRequest 
( [UserID], [CustID], [PremID], [TransactionType], [ActionCode], [TransactionDate], [Direction], [RequestDate], [ServiceActionCode], [ServiceAction], [StatusCode], [StatusReason], [SourceID], [TransactionNumber], [ReferenceSourceID], [ReferenceNumber], [OriginalSourceID], [ESIID], [ResponseKey], [AlertID], [ProcessFlag], [ProcessDate], [EventCleared], [EventValidated], [DelayedEventValidated], [ConditionalEventValidated], [TransactionTypeID], [CreateDate], [BulkInsertKey], [MeterAccessNote])
VALUES (
 NULL/*[UserID]*/, @CustId, @PremID, '814'/*[TransactionType]*/, '05'/*[ActionCode]*/, '2017-08-07'/*[TransactionDate]*/, 1/*[Direction]*/, '2017-08-17'/*[RequestDate]*/, 'A'/*[ServiceActionCode]*/, 'S'/*[ServiceAction]*/, NULL/*[StatusCode]*/, NULL/*[StatusReason]*/, @SourceID, '167528458620170807094645133305'/*[TransactionNumber]*/, @RefSourceId, @RefNumber, NULL/*[OriginalSourceID]*/, '10443720004064914'/*[ESIID]*/, NULL/*[ResponseKey]*/, NULL/*[AlertID]*/, 0/*[ProcessFlag]*/, NULL/*[ProcessDate]*/, 0/*[EventCleared]*/, 0/*[EventValidated]*/, 0/*[DelayedEventValidated]*/, 0/*[ConditionalEventValidated]*/, 28/*[TransactionTypeID]*/, GETDATE()/*[CreateDate]*/, NULL/*[BulkInsertKey]*/, NULL/*[MeterAccessNote]*/
)
";
            SqlHelper.ExecuteNonQuery(_appConfig.ConnectionCsr, CommandType.Text, sql);
        }

        private static void GenerateEvents(List<int> eventTypeIds)
        {
            var list = CIS.Element.Core.Event.EventTypeList.LoadDirectlyCallableEvents(_clientConfig.ClientId);

            eventTypeIds.ForEach(id =>
            {
                var htParams = new Hashtable { { "EventTypeID", id } };
                var _event = list.SingleOrDefault(x => x.EventTypeID == id);
                new CIS.Engine.Event.EventGenerator().GenerateEvent(_clientConfig.ClientId, htParams, _clientConfig.Client, _appConfig.ConnectionCsr, _clientConfig.ConnectionBillingAdmin, _event.AssemblyName, _event.ClassName);
            });
            

            //var maintenance = new MyMaintenance(_appConfig.ConnectionCsr, _appConfig.ConnectionMarket, _clientConfig.ConnectionBillingAdmin);
            //maintenance.GenerateEvents();
        }

        private static void ProcessEvents()
        {
            var engine = new CIS.Engine.Event.Queue(_clientConfig.ConnectionBillingAdmin);
            engine.ProcessEventQueue(_appConfig.ClientID, _appConfig.ConnectionCsr, _appConfig.ConnectionMarket, _appConfig.ClientAbbreviation);
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

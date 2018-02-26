// ReSharper disable InconsistentNaming
using System.Runtime.CompilerServices;
using Aurea.Maintenance.Debugger.Common.Extensions;
using Aurea.TaskToaster;

namespace Aurea.Maintenance.Debugger.Stream
{
    using System;
    using System.Linq;
    using System.Text;
    using Common;
    using Common.Models;
    using CIS.BusinessEntity;
    using System.Collections;
    using System.Data;
    using System.Data.SqlClient;

    using System.Collections.Generic;
    using System.IO;
    using System.Reflection;
    using Aurea.Logging;


    public class MyExport : CIS.Clients.Stream.Export.MainProcess
    {
        public MyExport() : base()
        {
            base.Client = Clients.Stream.Abbreviation();
            base.ServiceName = Assembly.GetEntryAssembly().FullName;
        }

        public MyExport(string connectionMarket, string connectionCSR, string connectionAdmin) 
            : base(connectionMarket, connectionCSR, connectionAdmin)
        {
            base.Client = Clients.Stream.Abbreviation();
            base.ClientID = Clients.Stream.Id();
            base.ServiceName = Assembly.GetEntryAssembly().FullName;
        }

        public void MyExportTransactions()
        {
            base.ExportTransactions();
        }

        public void MyCreateMarket810()
        {
            base.CreateMarket810();
        }

        public void MyCreateMarket814()
        {
            base.CreateMarket814();
        }
    }

    class Program
    {
        private static readonly ClientEnvironmentConfiguration _clientConfig = ClientConfiguration.GetClientConfiguration(Clients.Stream, Stages.Development, TransactionMode.Enlist);
        private static readonly GlobalApplicationConfigurationDS.GlobalApplicationConfiguration _appConfig = ClientConfiguration.SetConfigurationContext(_clientConfig);
        private static readonly ILogger _logger = new Logger();
        private static readonly string _appDir = Path.GetDirectoryName(Assembly.GetEntryAssembly().Location);
        private static readonly string _mockDataDir = Path.Combine(_appDir, "MockData");

        static void Main(string[] args)
        {
            #region oldCases
            /*
            SimulatePostEnrollmentEvent(clientConfiguration);
            CreateMockData();
            SimulateInbound814E();
            Simulate_AESCIS_11221();
            Simulate_AESCIS_19713();
            
            

            //copy customers from saes
            var saesCustIds = new List<int> { 762884, 762872, 762865, 762706, 762664, 697013, 696563, 695975, 695962, 695922 };
            CopyCustomerFromUA(saesCustIds);

             */
            #endregion




            // copy customers from paes
            var paesCustIds = new List<int> { 766783, 766775, 765715, 759617 };
            CopyCustomerFromProd(paesCustIds);
            var resp = "Y";
            while ("Y".Equals(resp,StringComparison.InvariantCultureIgnoreCase))
            {
                Simulate_AESCIS_20523();
                Console.WriteLine("do you want to Repeat call");
                resp = Console.ReadLine().Trim();
            }

            _logger.Info("Debug session has ended");
            Console.ReadLine();
        }

        private static void Simulate_AESCIS_20523()
        {
            var myExport = new MyExport(_appConfig.ConnectionMarket, _appConfig.ConnectionCsr, _clientConfig.ConnectionBillingAdmin);

            GenerateEvents(new List<int> { 10 });
            ProcessEvents();

            // export 814
            myExport.MyCreateMarket814();
        }

        private static void CopyCustomerFromProd(List<int> custIds)
        {
            custIds.ForEach((custId) =>
            {
                var sql = string.Format(MockData.Scripts.CustomerExportScript, custId, 5);
                DB.ImportQueryResultsFromProduction(sql, _appConfig.ConnectionCsr, _appDir);
            });
        }

        private static void CopyCustomerFromUA(List<int> custIds)
        {
            custIds.ForEach((custId) =>
            {
                var sql = string.Format(MockData.Scripts.CustomerExportScript, custId, 5);
                DB.ImportQueryResultsFromUa( sql, _appConfig.ConnectionCsr, _appDir);
            });
        }

        private static void Simulate_AESCIS_19713()
        {
            var myExport = new MyExport(_appConfig.ConnectionMarket, _appConfig.ConnectionCsr, _clientConfig.ConnectionBillingAdmin);
            DB.ImportFiles(_mockDataDir, "New-C1", _appConfig.ConnectionCsr);

            PromoteEnrollCustomers();

            GenerateEvents(new List<int> {10});
            ProcessEvents();

            // export 814
            myExport.MyCreateMarket814();
        }

        private static void PromoteEnrollCustomers()
        {
            CIS.Clients.Stream.MaintenanceHandlers.Enrollment enrollmentPromotion = new CIS.Clients.Stream.MaintenanceHandlers.Enrollment(_appConfig.ConnectionCsr);
            enrollmentPromotion.ProcessEnrollments();
        }

        private static void GenerateEvents(List<int> eventTypeIds)
        {
            var list = CIS.Element.Core.Event.EventTypeList.Load(_clientConfig.ClientId);

            eventTypeIds.ForEach(id =>
            {
                var htParams = new Hashtable { { "EventTypeID", id } };
                var _event = list.SingleOrDefault(x => x.EventTypeID == id);
                new CIS.Engine.Event.EventGenerator().GenerateEvent(_clientConfig.ClientId, htParams, _clientConfig.Client, _appConfig.ConnectionCsr, _clientConfig.ConnectionBillingAdmin, _event.AssemblyName, _event.ClassName);
            });
        }

        private static void ProcessEvents()
        {
            var engine = new CIS.Engine.Event.Queue(_clientConfig.ConnectionBillingAdmin);
            engine.ProcessEventQueue(_appConfig.ClientID, _appConfig.ConnectionCsr, _appConfig.ConnectionMarket, _appConfig.ClientAbbreviation);
        }

        private static void Simulate_AESCIS_11221()
        {
            var myExport = new MyExport(_appConfig.ConnectionMarket, _appConfig.ConnectionCsr, _clientConfig.ConnectionBillingAdmin);
            Simulate810Export(myExport);
            Simulate814Export(myExport);
        }

        private static void Simulate810Export(MyExport myExport)
        {
            var requestIds = new List<int> { 33496779 , 32061771 , 33496822 };
            // copy records for 810 simulation, Customer, Premise, Meter, CustomerAdditionalInfo, AccountsReceivable, Address, Consumption, Invoice ...
            DB.ImportFiles(_mockDataDir, "AESCIS-11221-810", _appConfig.ConnectionCsr);
            
            MarkCustomerTransactionRequestsAsUnProcessed(requestIds);

            // export 810
            myExport.MyCreateMarket810();
        }

        private static void Simulate814Export(MyExport myExport)
        {
            var requestIds = new List<int> { 33489671, 33057378 };
            // copy records for 814 simulation, Customer, Premise, CustomerAdditionalInfo, AccountsReceivable, Address...
            DB.ImportFiles(_mockDataDir, "AESCIS-11221-814", _appConfig.ConnectionCsr);

            // mark Enrollment as 814 ready
            MarkCustomerTransactionRequestsAsUnProcessed(requestIds);

            // export 814
            myExport.MyCreateMarket814();
        }

        private static void MarkCustomerTransactionRequestsAsUnProcessed(List<int> requestIds)
        {
            // mark latest invoice for 810 CTR creation
            var sqlQueries = new StringBuilder();
            requestIds.ForEach(id =>
                sqlQueries.AppendLine(
                    $"UPDATE CustomerTransactionRequest SET SourceID = NULL, TransactionNumber = NULL, ProcessFlag = 0, ProcessDate = NULL WHERE RequestID = {id}"));
            DB.ExecuteQuery(sqlQueries.ToString(), _appConfig.ConnectionCsr);
        }

        private static void SimulateInbound814E()
        {
            var engine = new CIS.Engine.Event.Queue(_clientConfig.ConnectionBillingAdmin);
            engine.ProcessEventQueue(_clientConfig.ClientId, _appConfig.ConnectionCsr, _appConfig.ConnectionMarket, _appConfig.ClientAbbreviation);
        }

        private static void SimulatePostEnrollmentEvent()
        {
            var pe = new CIS.Clients.Stream.PostEnrollment
            {
                ConnectionString = _appConfig.ConnectionCsr
            };
            pe.Process(30396997);
        }

        private static void CreateMockData()
        {
            var sqlString = @"
                -- begin create mock data
                USE daes_Stream

                DECLARE @ClientID int
                DECLARE @EventId int
                DECLARE @ESIID VARCHAR(20)
                DECLARE @CustNo VARCHAR(10)
                DECLARE @PremID int
                DECLARE @PremNo VARCHAR(10)
                DECLARE @CustID int
                DECLARE @MasterCustID int
                DECLARE @AddrID int
                DECLARE @CustRateID int
                DECLARE @TransRateID int
                DECLARE @RateTransitionID int
                DECLARE @CSPID int
                DECLARE @CustomerTypeID int
                DECLARE @CustType CHAR(1)
                DECLARE @TDSP int
                DECLARE @LDCID int
                DECLARE @TransactionNumber VARCHAR(100)
                DECLARE @TransactionTypeID int
                DECLARE @TransactionType VARCHAR(10)
                DECLARE @ActionCode VARCHAR(4)
                DECLARE @Direction BIT
                DECLARE @ServiceActionCode VARCHAR(10)
                DECLARE @ServiceAction VARCHAR(10)
                DECLARE @ReferenceNumber VARCHAR(100)
                DECLARE @SourceID int
                DECLARE @PremiseEnrollmentPendingStatus int
                DECLARE @PremiseEnrollmentAcceptedStatus int
                DECLARE @PremiseActiveStatus int
                DECLARE @RequestID int
                DECLARE @EventingQueueId int
                DECLARE @PremiseUpdateActionID int
                DECLARE @CustomerUpdateActionID int
                DECLARE @PostEnrollmentActionID int
                DECLARE @PremiseUpdateEventActionQueueId int
                DECLARE @CustomerEventActionQueueId int
                DECLARE @PostEnrollmentEventActionQueueId int 
                DECLARE @AccountsReceivableID int

                SET @ClientID = 45 --Stream
                SET @EventId = 73616--Simple Customer Evaluation
                SET @ESIID = '10032251418552570' --change with some random
                SET @CustNo = '874157210'
                SET @PremNo = '266536589524081'
                SET @CSPID = 1
                SET @CustomerTypeID = 11
                SET @MasterCustID = 10001
                SET @CustType = 'C'
                SET @TDSP = 13
                SET @TransactionNumber = '194214320_0000001' --some random number
                SET @TransactionTypeID = 47
                SET @TransactionType = '814'
                SET @ActionCode = 'E'
                SET @Direction = 1
                SET @ServiceActionCode = 'A'
                SET @ServiceAction = 'S'
                SET @ReferenceNumber = '58649174' -- some random number
                SET @SourceID = 4858392 -- some random number

                SET @PremiseEnrollmentPendingStatus = 0
                SET @PremiseEnrollmentAcceptedStatus = 1
                SET @PremiseActiveStatus = 11

                PRINT 'Create Address'
                INSERT INTO [Address]
                 ([ValidationStatusID], [AttnMS], [Addr1], [Addr2], [City], [State], [Zip], [Zip4], [DPBC], [CityID], [CountyID], [County], [HomePhone], [WorkPhone], [FaxPhone], [OtherPhone], [Email], [ESIID], [GeoCode], [Status], [DeliveryPointCode], [CreateDate], [Migr_Enrollid], [PhoneExtension], [OtherExtension], [FaxExtension], [TaxingDistrict], [TaxInCity], [CchVersion])
                SELECT 
                  0, 'Mock Cust Name', 'Mock 1797 Lexington Ave ', NULL, 'New York', 'NY', '10029', NULL, NULL, NULL, NULL, NULL, 'M2124260402', NULL, NULL, NULL, 'mock@email.com', NULL, 'US36061A0003', 'A', NULL, GETDATE(), NULL, NULL, NULL, NULL, NULL, 1, 2
                SET @AddrID = SCOPE_IDENTITY()

                PRINT 'Create Rate For Customer'
                IF NOT EXISTS (SELECT 1 FROM [Rate] WHERE RateCode = 'PREM_654643')
                BEGIN
	                INSERT INTO [Rate]
	                 ([CSPID], [RateCode], [RateDesc], [EffectiveDate], [ExpirationDate], [RateType], [PlanType], [IsMajority], [TemplateFlag], [LDCCode], [CreateDate], [UserID], [RatePackageName], [CustType], [ServiceType], [DivisionCode], [ConsUnitId], [ActiveFlag], [LDCRateCode], [migr_plan_id], [migr_custid])
	                SELECT
	                 NULL, 'PREM_654643', '', '2017-08-24 00:00:00.000', '2019-08-24 00:00:00.000', '', 0, 0, 1, NULL, '2017-08-24 16:00:24.307', NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL
	                SET @CustRateID = SCOPE_IDENTITY()
                END
                ELSE
                BEGIN
	                SET @CustRateID = (SELECT RateID FROM [Rate] WHERE RateCode = 'PREM_654643')
                END

                PRINT 'Create Rate For Transition'
                IF NOT EXISTS (SELECT 1 FROM [Rate] WHERE RateCode = 'NY_CONED_E_COMMERCIAL_FIXED_Rate')
                BEGIN
	                INSERT INTO [Rate]
	                 ([CSPID], [RateCode], [RateDesc], [EffectiveDate], [ExpirationDate], [RateType], [PlanType], [IsMajority], [TemplateFlag], [LDCCode], [CreateDate], [UserID], [RatePackageName], [CustType], [ServiceType], [DivisionCode], [ConsUnitId], [ActiveFlag], [LDCRateCode], [migr_plan_id], [migr_custid])
	                SELECT
	                 1, 'NY_CONED_E_COMMERCIAL_FIXED_Rate', 'NY CONED Electric Commercial Fixed', '2013-05-01 00:00:00.000', NULL, NULL, NULL, 0, 1, 13, '2013-06-06 22:32:24.287', NULL, 'NY_CONED_E_COMMERCIAL_FIXED_RatePackage', NULL, 'E', NULL, NULL, 1, NULL, NULL, NULL
	                SET @TransRateID = SCOPE_IDENTITY()
                END
                ELSE
                BEGIN
	                SET @TransRateID = (SELECT RateID FROM [Rate] WHERE RateCode = 'NY_CONED_E_COMMERCIAL_FIXED_Rate')
                END

                PRINT 'Create AccountsReceivable'
                INSERT INTO [AccountsReceivable]
                 ( PastDue, StatusID, Terms)
                SELECT
                 'N', 1, '26'
                SET @AccountsReceivableID = SCOPE_IDENTITY()

                PRINT 'Create Customer'
                INSERT INTO [Customer]
                 ([CSPID], [CustomerTypeID], [CustNo], [CustName], [LastName], [FirstName], [MidName], [CompanyName], [DBA], [FederalTaxID], [AcctsRecID], [DistributedAR], [ProductionCycle], [BillCycle], [RateID], [SiteAddrID], [MailAddrId], [CorrAddrID], [BillCustID], [MasterCustID], [Master], [CustStatus], [BilledThru], [CSRStatus], [CustType], [Services], [FEIN], [DOB], [Taxable], [LateFees], [NoOfAccts], [ConsolidatedInv], [SummaryInv], [MsgID], [TDSPTemplateID], [TDSPGroupID], [LifeSupportIndictor], [LifeSupportStatus], [LifeSupportDate], [SpecialBenefitsPlan], [BillFormat], [PrintLayoutID], [CreditScore], [HitIndicator], [RequiredDeposit], [AccountManager], [EnrollmentAlias], [ContractID], [UserDefined1], [CreateDate], [RateChangeDate], [PermitContactName], [CustomerPrivacy], [UsagePrivacy], [LidaDiscount])
                SELECT 
                  @CSPID, @CustomerTypeID, @CustNo, 'Mock Cust Name', 'Mock Cust LName', 'Mock Cust FName', NULL, NULL, 'Mock DBA', NULL, @AccountsReceivableID, NULL, 55, 55, @CustRateID, @AddrID, @AddrId, @AddrID, NULL, @MasterCustID, 'N', 'A', NULL, NULL, @CustType, NULL, 8661, NULL, 'Y', 'N', NULL, NULL, 'N', NULL, 3, 1, 'N', NULL, NULL, NULL, 1, 1, NULL, NULL, 'N', NULL, NULL, NULL, 'None', DATEADD(dd, -2, GETDATE()), NULL, NULL, 0, 0, 0
                SET @CustID = SCOPE_IDENTITY()

                PRINT 'Create CustomerAdditionalInfo'
                INSERT INTO [CustomerAdditionalInfo]
                 (CustID,CSPDUNSID,BillingTypeID,BillingDayOfMonth,MasterCustID_2,MasterCustID_3,MasterCustID_4,TaxAssessment,ContractPeriod,ContractDate,AccessVerificationType,AccessVerificationData,ClientAccountNo,InstitutionID,TransitNum,AccountNum,MigrationAccountNo,MigrationFirstServed,MigrationKwh,CollectionsStageID,CollectionsStatus,CollectionsDate,KeyAccount,DisconnectLtr,AuthorizedReleaseName,AuthorizedReleaseDOB,AuthorizedReleaseFederalTaxID,EFTFlag,PromiseToPayFlag,DisconnectFlag,CreditHoldFlag,RawConsumptionImportFlag,CustomerProtectionStatus,MCPEFlag,HasLocationMasterFlag,DivisionID,DivisionCode,DriverLicenseNo,PromotionCodeID,CustomerDUNS,CustomerGroupID,EarlyTermFee,EarlyTermFeeUpdateDate,DeceasedFlag,BankruptFlag,CollectionsAgencyID,DoNotCall,CustomerSecretWord,PrintGroupID,CurrentCustNo,IsDPP,UnitNumber,CustomerCategoryID,SubsequentDepositExempt,AutoPayFlag,SpecialNeedsFlag,SpecialNeedsEndDate,SpecialNeedsQualifierTypeID,CsrImportDate,OnSwitchHold,SwitchHoldStartDate,DPPStatusID,UpdateContactInfoFlag,IsFriendlyLatePaymentReminderSent,IsLowIncome,ExtendedCustTypeId,AutoPayLastUpdated,GreenEnergyOptIn,SocialCauseID,SocialCauseCode,SecondaryContactFirstName,SecondaryContactLastName,SecondaryContactPhone,SecondaryContactRelationId,SSN,ServiceAccount,EncryptionPasswordTypeId,EncryptionPasswordCustomValue,CustInfo1,CustInfo2,CustInfo3,CustInfo4,CustInfo5,SalesAgent,Broker,PromoCode,CommissionType,CommissionAmount,ReferralID,CampaignName,AccessDBID,SalesChannel,TCPAAuthorization,IsPUCComplaint,CancellationFee,MunicipalAggregation)
                SELECT
                 @CustID, 7, 3, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL,'B2418147', NULL, NULL, NULL, NULL, NULL, NULL, 0, 0, NULL, NULL, 'N', 'Mock Ahsan Nadeem', NULL, NULL, 0, 0, 0, 0, 1, 0, 0, 0, 1, 'SGENormal',  NULL, NULL, NULL, 3, 0.00, '2017-08-24 16:00:24.580', 0, 0, NULL, 0, NULL, NULL, NULL, 0, NULL, NULL, 0, 0, 0, NULL, NULL, NULL, 0, NULL, NULL, 0, 0, 0, NULL, '2017-08-24 16:00:25.010', 0, NULL, NULL, NULL, NULL, NULL, NULL, NULL, 'C', NULL, NULL, 0, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, 0.00, NULL, NULL, NULL, NULL, 'NA', 0, NULL, NULL

                PRINT 'Create LDC'
                SET @LDCID = 13
                /*
                INSERT INTO LDC
                 ([LDCID], [LDCCode], [LDCName], [LDCShortName], [DUNS], [PriceToBeat], [PriceToBeatDate], [MarketID], [MarketCode], [LDCFederalTaxID], [RateDeliveryTypeID], [PremNoLabel], [VariableHUFlag], [VariableHULimit], [IsAllowCustomLDCRateCode])
                SELECT
                 @LDCID, 13, 'Consolidated Edison of New York', 'CONED', '006982359', NULL, NULL, 3, 'NY', NULL, 1, 'LDC Account No', 0, NULL, 0 
                */

                PRINT 'Create Premise'
                INSERT INTO [Premise]
                 ([CustID], [CSPID], [AddrID], [TDSPTemplateID], [ServiceCycle], [TDSP], [TaxAssessment], [PremNo], [PremDesc], [PremStatus], [PremType], [LocationCode], [SpecialNeedsFlag], [SpecialNeedsStatus], [SpecialNeedsDate], [ReadingIncrement], [Metered], [Taxable], [BeginServiceDate], [EndServiceDate], [SourceLevel], [StatusID], [StatusDate], [CreateDate], [UnitID], [PropertyCommonID], [RateID], [DeleteFlag], [LBMPId], [PipelineId], [GasLossId], [LDCID], [GasPoolID], [DeliveryPoint], [ConsumptionBandIndex], [LastModifiedDate], [CreatedByID], [ModifiedByID], [BillingAccountNumber], [NameKey], [GasSupplyServiceOption], [IntervalUsageTypeId], [LDC_UnMeteredAcct], [AltPremNo], [OnSwitchHold], [SwitchHoldStartDate], [ConsumptionImportTypeId], [TDSPTemplateEffectiveDate], [ServiceDeliveryPoint], [UtilityContractID], [LidaDiscount], [GasCapacityAssignment], [CPAEnrollmentTypes], [IsTOU], [SupplierPricingStructureNr], [SupplierGroupNumber])
                SELECT
                  @CustID, @CSPID, @AddrID, NULL, NULL, @TDSP, 47, @PremNo, 'Mock 1797 Lexington Ave Stos , New York, NY 10029', 'Bill', 'Elec', NULL, 0, NULL, NULL, NULL, '?', 'Y', NULL, NULL, NULL, @PremiseEnrollmentAcceptedStatus, DATEADD(dd, -1, GETDATE()), DATEADD(dd, -2, GETDATE()), NULL, NULL, NULL, 0, NULL, NULL, NULL, @LDCID, NULL, NULL, NULL, NULL, NULL, NULL, '4001654643', NULL, 'A', NULL, 0, NULL, 0, NULL, 1, NULL, NULL, NULL, 0, NULL, NULL, 0, NULL, NULL
                SET @PremID = SCOPE_IDENTITY()

                PRINT 'Create RateIndexType'
                IF NOT EXISTS(SELECT 1 FROM RateIndexType WHERE RateIndexTypeID = '286')
                BEGIN
                  INSERT INTO RateIndexType 
                   ( [RateIndexTypeID], [RateIndexType], [Active])
                  SELECT
                    286, 'NY_CONED_E_COMMERCIAL_FIXED', 1
                END

                PRINT 'Create RateIndexRange'
                IF NOT EXISTS(SELECT 1 FROM RateIndexRange WHERE RateIndexTypeID = '286')
                BEGIN
                  INSERT INTO RateIndexRange 
                   ([RateIndexTypeID], [DateFrom], [DateTo], [IndexRate])
                  SELECT
                   286, '2013-06-22 00:00:00.000', '2020-06-22 00:00:00.000', 0.00010000
                END

                PRINT 'Create RateTransition'
                IF NOT EXISTS(SELECT 1 FROM [RateTransition] WHERE RateID = @TransRateID)
                BEGIN
	                INSERT INTO [RateTransition]
	                 ([CustID], [RateID], [UserID], [CreatedDate], [SwitchDate], [EndDate], [StatusID], [SoldDate], [RolloverFlag])
	                SELECT
	                  @CustID, @TransRateID, 0, '2017-08-24 16:00:50.327', '2017-09-08 00:00:00.000', '2018-09-08 00:00:00.000', 2, '2017-08-24 00:00:00.000', 0
	                SET @RateTransitionID = SCOPE_IDENTITY()
                END
                ELSE
                BEGIN
	                SET @RateTransitionID = (SELECT RateTransitionID FROM [RateTransition] WHERE RateID = @TransRateID)
	                UPDATE RateTransition SET CustID = @CustID WHERE RateTransitionID = @RateTransitionID
                END

                PRINT 'Create RateDetail for Customer'
                IF (SELECT COUNT(*) FROM RateDetail WHERE RateID = @CustRateID) = 0 
                BEGIN
                INSERT INTO [RateDetail]
                 ([RateID], [CategoryID], [RateTypeID], [ConsUnitID], [RateDescID], [EffectiveDate], [ExpirationDate], [RateAmt], [RateAmt2], [RateAmt3], [FixedAdder], [MinDetAmt], [MaxDetAmt], [GLAcct], [RangeLower], [RangeUpper], [CustType], [Graduated], [Progressive], [AmountCap], [MaxRateAmt], [MinRateAmt], [CategoryRollup], [Taxable], [ChargeType], [MiscData1], [FixedCapRate], [ScaleFactor1], [ScaleFactor2], [TemplateRateDetID], [Margin], [HALRateDetailId], [UsageClassId], [LegacyRateDetailId], [Building], [ServiceTypeID], [TaxCategoryID], [UtilityID], [UtilityInvoiceTemplateDetailID], [Active], [StatusID], [RateVariableTypeId], [MinDays], [MaxDays], [BlockPriceIndicator], [RateTransitionId], [CreateDate], [MeterMultiplierFlag], [BlendRatio], [ContractVolumeID], [CreatedByUserId], [ModifiedByUserId], [ModifiedDate], [TOUTemplateID], [TOUTemplateRegisterID], [TOUTemplateRegisterName])
                SELECT
                 @CustRateID, 1, 3002, 5, 1, '2017-08-24 00:00:00.0000000', '2018-09-08 00:00:00.0000000', 0.08550000, NULL, NULL, NULL, 0.00, 0.00, '', NULL, NULL, '', '', '', 'N', '', '', '', 'Y', 'C', 51, 286.00000000, NULL, NULL, 565647, NULL, NULL, NULL, NULL, '', NULL, 0, NULL, NULL, 1, 1, NULL, NULL, NULL, 0, @RateTransitionID, '2017-08-24 16:00:50.433', 0, NULL, NULL, NULL, NULL, NULL, NULL, NULL, ''
                END

                PRINT 'Create RateDetail for Transition'
                IF (SELECT COUNT(*) FROM RateDetail WHERE RateID = @TransRateID) = 0 
                BEGIN
                INSERT INTO [RateDetail]
                 ([RateID], [CategoryID], [RateTypeID], [ConsUnitID], [RateDescID], [EffectiveDate], [ExpirationDate], [RateAmt], [RateAmt2], [RateAmt3], [FixedAdder], [MinDetAmt], [MaxDetAmt], [GLAcct], [RangeLower], [RangeUpper], [CustType], [Graduated], [Progressive], [AmountCap], [MaxRateAmt], [MinRateAmt], [CategoryRollup], [Taxable], [ChargeType], [MiscData1], [FixedCapRate], [ScaleFactor1], [ScaleFactor2], [TemplateRateDetID], [Margin], [HALRateDetailId], [UsageClassId], [LegacyRateDetailId], [Building], [ServiceTypeID], [TaxCategoryID], [UtilityID], [UtilityInvoiceTemplateDetailID], [Active], [StatusID], [RateVariableTypeId], [MinDays], [MaxDays], [BlockPriceIndicator], [RateTransitionId], [CreateDate], [MeterMultiplierFlag], [BlendRatio], [ContractVolumeID], [CreatedByUserId], [ModifiedByUserId], [ModifiedDate], [TOUTemplateID], [TOUTemplateRegisterID], [TOUTemplateRegisterName])
                SELECT
                 @TransRateID, 1, 3002, 5, 1, '2013-05-01 00:00:00.0000000', NULL, NULL, NULL, NULL, NULL, 0.00, 0.00, NULL, NULL, NULL, NULL, NULL, NULL, 'N', NULL, NULL, NULL, 'Y', 'C', 51, 286.0000, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, 1, 1, NULL, NULL, NULL, 0, NULL, '2013-06-06 22:32:24.410', 0, NULL, NULL, 0, NULL, NULL, NULL, NULL, NULL
                END

                PRINT 'Create Product For RateTransition'
                IF NOT EXISTS (SELECT 1 FROM Product WHERE RateID = @TransRateID)
                BEGIN
                  INSERT INTO Product
                   (RateID,LDCCode,PlanType,TDSPTemplateID,Description,BeginDate,EndDate,CustType,Graduated,RangeTier1,RangeTier2,SortOrder,ActiveFlag,Uplift,CSATDSPTemplateID,CAATDSPTemplateID,PriceDescription,MarketingCode,RateTypeID,ConsUnitID,[Default],DivisionCode,RateDescription,ServiceType,CSPId,TermsId,RolloverProductId,CommissionId,CommissionAmt,CancelFeeId,MonthlyChargeId,ProductCode,RatePackageId,ProductName,TermDate,DiscountTypeId,DiscountAmount,ProductZoneID,IsGreen,IsBestChoice,ActiveEnrollmentFlag,CreditScoreThreshold,DepositAmount,ProductNote,Incentives)
                  SELECT
                    @TransRateID, 13, 1, 3,'NY CONED Electric Commercial Fixed', '2013-05-01 00:00:00.000', NULL, 'C', 'N', NULL, NULL, NULL, 1, NULL, NULL, NULL, 'NY CONED Electric Commerical Fixed Rate Plan', NULL, NULL,5, 0, NULL, NULL, 'E', 1, 47, 365, NULL, NULL, NULL, NULL, 'NY_CONED_E_COMMERCIAL_FIXED', 258040, 'NY CONED Electric Commercial Fixed', NULL, NULL, NULL, 9, 0, 0, 1, NULL, NULL, NULL, NULL
                END

                PRINT 'Create CustomerTransactionRequest'
                INSERT INTO CustomerTransactionRequest
                 ([UserID], [CustID], [PremID], [TransactionType], [ActionCode], [TransactionDate], [Direction], [RequestDate], [ServiceActionCode], [ServiceAction], [StatusCode], [StatusReason], [SourceID], [TransactionNumber], [ReferenceSourceID], [ReferenceNumber], [OriginalSourceID], [ESIID], [ResponseKey], [AlertID], [ProcessFlag], [ProcessDate], [EventCleared], [EventValidated], [DelayedEventValidated], [ConditionalEventValidated], [TransactionTypeID], [CreateDate], [BulkInsertKey], [MeterAccessNote])
                SELECT
                 NULL, @CustID, @PremID, @TransactionType, @ActionCode, GETDATE(), @Direction, DATEADD(dd,-7,GETDATE()), @ServiceActionCode, @ServiceAction, NULL, NULL, @SourceID, @TransactionNumber, 0, @ReferenceNumber, NULL, @ESIID, NULL, NULL, 0, NULL, 0, 0, 0, 0, @TransactionTypeID, GETDATE(), NULL, NULL
                SET @RequestID = SCOPE_IDENTITY()

                USE daes_BillingAdmin

                PRINT 'Create EventingQueue'
                INSERT INTO EventingQueue
                        (ClientId, EventId, Status, CreateDate)
                VALUES  (@ClientID, @EventId, 0, GETDATE())
                SET @EventingQueueId = SCOPE_IDENTITY()

                SET @PremiseUpdateActionID = (SELECT EventActionID FROM saes_BillingAdmin..EventAction WHERE EventID = 73616 AND ActionID  IN (SELECT ActionID FROM daes_BillingAdmin..Action WHERE ActionName = 'PremiseUpdate'))
                SET @CustomerUpdateActionID = (SELECT EventActionID FROM saes_BillingAdmin..EventAction WHERE EventID = 73616 AND ActionID  IN (SELECT ActionID FROM daes_BillingAdmin..Action WHERE ActionName = 'CustomerUpdate'))
                SET @PostEnrollmentActionID = (SELECT EventActionID FROM saes_BillingAdmin..EventAction WHERE EventID = 73616 AND ActionID IN (SELECT ActionID FROM daes_BillingAdmin..Action WHERE ActionName = 'PostEnrollment'))

                PRINT 'Create EventActionQueue'
                INSERT INTO EventActionQueue
                        (EventingQueueId, EventActionId, CustId, PremId, RequestId, ESIID, EventDate, Status, SourceId, UserId)
                SELECT @EventingQueueId, @PremiseUpdateActionID, @CustId, @PremId, @RequestID, @ESIID, GETDATE(), 0, -1, 0
                SET @PremiseUpdateEventActionQueueId = SCOPE_IDENTITY()

                INSERT INTO EventActionQueue
                        (EventingQueueId, EventActionId, CustId, PremId, RequestId, ESIID, EventDate, Status, SourceId, UserId)
                SELECT @EventingQueueId, @CustomerUpdateActionID, @CustId, @PremId, @RequestID, @ESIID, GETDATE(), 0, -1, 0
                SET @CustomerEventActionQueueId = SCOPE_IDENTITY()

                INSERT INTO EventActionQueue
                        (EventingQueueId, EventActionId, CustId, PremId, RequestId, ESIID, EventDate, Status, SourceId, UserId)
                SELECT @EventingQueueId, @PostEnrollmentActionID, @CustId, @PremId, @RequestID, @ESIID, GETDATE(), 0, -1, 0
                SET @PostEnrollmentEventActionQueueId = SCOPE_IDENTITY()

                PRINT 'Create EventActionQueueParameter'
                INSERT INTO EventActionQueueParameter
                        (EventActionQueueId, ActionParameterId, ParameterValue, ActionParameterFunctionId)
                SELECT @PremiseUpdateEventActionQueueId, 42, 1, 0
                UNION
                SELECT @PremiseUpdateEventActionQueueId, 76, 1, 0
                UNION
                SELECT @PremiseUpdateEventActionQueueId, 77, 8, 0
                INSERT INTO EventActionQueueParameter
                        (EventActionQueueId, ActionParameterId, ParameterValue, ActionParameterFunctionId)
                SELECT @CustomerEventActionQueueId, 28, 'A', 0
                ";
            DB.ExecuteQuery(sqlString, _appConfig.ConnectionCsr);
        }
    }
}

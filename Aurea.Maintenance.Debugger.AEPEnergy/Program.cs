namespace Aurea.Maintenance.Debugger.AEPEnergy
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using System.Threading.Tasks;
    using System.Data;
    using System.Data.SqlClient;

    using Common;
    using Common.Models;
    using CIS.BusinessEntity;
    using System.Collections;
    using CIS.Framework.Common;
    using Aurea.Maintenance.Debugger.Common.Extensions;
    using CIS.Framework.Data;
    using System.IO;
    using System.Reflection;
    using Aurea;

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

        public void MyImportTransaction()
        {
            this.ImportTransaction();

        }
    }

    public class MyMaintenance : CIS.Clients.AEPEnergy.Maintenance
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

    class Program
    {
        private static ClientEnvironmentConfiguration _clientConfig;
        private static GlobalApplicationConfigurationDS.GlobalApplicationConfiguration _appConfig;

        static void Main(string[] args)
        {
            
            // Set client configuration and then the application configuration context.            
            _clientConfig = ClientConfiguration.GetClientConfiguration(Clients.AEP, Stages.Development);
            _appConfig = ClientConfiguration.SetConfigurationContext(_clientConfig);

            //System.ServiceModel.ServiceSecurityContext.Current.PrimaryIdentity.Name = Clients.AEP.GetServiceGuid.ToString();

            //Simulate_AESCIS17193("4184386");
            Simulate_AESCIS16615(19981, 543, DateTime.Parse("2017-12-05T23:31:41-06:00"), "N", DateTime.Today.Date);
        }

        private static void Simulate_AESCIS16615(int customerId, int productId, DateTime soldDate, string municipalAggregation, DateTime? switchDate = null)
        {
            //CopyProduct, CopyCustomer
            CreateProducts("AESCIS-16615");
            CopyCustomerWithID(customerId);
            CIS.Clients.AEPEnergy.RateType.RateUtility.ApplyRateTransition(customerId, productId, soldDate, municipalAggregation, switchDate);
        }

        private static void Simulate_AESCIS17193(string custNo)
        {
            CopyCustomerWithNumber(custNo);
            ClearOldRecords(custNo);
            CopyRateWithCustomerNumber(custNo);
            Create814EMarketMock(custNo, DateTime.Now);
            ImportTransaction();
            GenerateEventsFor814Market();
            ProcessEvents();
            //ExecuteTask("CustomerPromotionTask");
            //ExecuteTask("CustomerPromotionTask");
            //ExecuteTask("WelcomeLetterTask");
            //ProductRolloverProcessor
        }

        private static void ImportTransaction()
        {
            var baseImport = new MyImport()
            {
                ConnectionAdmin = _clientConfig.ConnectionBillingAdmin,
                ConnectionMarket = _appConfig.ConnectionMarket,
                ConnectionCsr = _appConfig.ConnectionCsr,
                ClientID = _clientConfig.ClientId,
                Client = _clientConfig.Client

            };
            baseImport.MyImportTransaction();
        }

        private static void ExecuteTask(string taskId)
        {
            var adminDataAccess = new CIS.Clients.AEPEnergy.Miramar.DataAccess.AdminDataAccess(_clientConfig.ConnectionBillingAdmin);
            var clientInfo = adminDataAccess.LoadClientInfo(CIS.Clients.AEPEnergy.Common.ClientConfigurationFactory.ClientId);
            //clientInfo.AdminConnection = _clientConfig.ConnectionBillingAdmin;
            //clientInfo.ClientConnection = "";
            var factory = new CIS.Clients.AEPEnergy.Miramar.MiramarTaskFactory();
            var instance = factory.GetTask(clientInfo, taskId);
            var serviceToken = new CIS.Clients.AEPEnergy.Infrastructure.Threading.ServiceToken();
            instance.Execute(serviceToken);
        }

        private static void ExecuteCustomerPromotionTask()
        {
            
        }

        private static void GenerateEventsFor814Market()
        {

            var htParams = new Hashtable() { { "EventTypeID", 10 } }; ;

            var gen = new CIS.Framework.Event.EventGenerator.SimpleMarketTransactionEvaluation(_appConfig.ConnectionCsr, _clientConfig.ConnectionBillingAdmin)
            {
                ConnectionStringBillingAdmin = _clientConfig.ConnectionBillingAdmin,
                ConnectionStringCsr = _appConfig.ConnectionCsr,
                Client = _clientConfig.Client,
                ClientID = _clientConfig.ClientId
            };

            gen.Generate(_clientConfig.ClientId, htParams);

            /*
            var maintenance = new MyMaintenance(_clientConfiguration.ConnectionCsr, _clientConfiguration.ConnectionMarket, Utility.BillingAdminDEV);
            //will create events for all configured eventtype on client
            maintenance.GenerateEvents();

            //will create event queue for CTR
            //maintenance.Sim();
            */
        }

        private static void ProcessEvents()
        {
            var engine = new CIS.Engine.Event.Queue(_clientConfig.ConnectionBillingAdmin);
            engine.ProcessEventQueue(_appConfig.ClientID, _appConfig.ConnectionCsr, _appConfig.ConnectionMarket, _appConfig.ClientAbbreviation);
        }

        private static void ClearOldRecords(string custNo)
        {
            string sql = $@"
PRINT 'Clean Old Transactions, Market Files and EventQueue'
DECLARE @CustNo AS VARCHAR(20) = '{custNo}'
DECLARE @CustID AS INT = (SELECT CustID FROM saes_AEPEnergy..Customer WHERE CustNo = @CustNo)
DECLARE @PremID AS INT = (SELECT PremId FROM saes_AEPEnergy..Premise WHERE CustId = @CustID)

DELETE FROM daes_AEPEnergyMarket..tbl_814_Service_Date WHERE Service_Key IN (2144892)
DELETE FROM daes_AEPEnergyMarket..tbl_814_Service_Meter WHERE Service_Key IN (2144892)
DELETE FROM daes_AEPEnergyMarket..tbl_814_Service WHERE [814_Key] IN (2154897)
DELETE FROM daes_AEPEnergyMarket..tbl_814_header WHERE [814_Key] IN (2154897)
DELETE FROM daes_AEPEnergy..CustomerTransactionRequest WHERE SourceId IN (2154897)

DELETE FROM daes_BillingAdmin..EventingQueue WHERE ClientId = 52 AND EventingQueueId IN (SELECT EventingQueueId FROM daes_BillingAdmin..EventActionQueue WHERE CustId = @CustId)
DELETE FROM daes_BillingAdmin..EventActionQueueParameter WHERE EventActionQueueId IN (SELECT EventActionQueueId FROM daes_BillingAdmin..EventActionQueue WHERE CustId = @CustId)
DELETE FROM daes_BillingAdmin..EventActionQueue WHERE CustId = @CustId

DELETE FROM daes_AEPEnergy..ChangeLogDetail WHERE ChangeLogID IN (SELECT ChangeLogID FROM daes_AEPEnergy..ChangeLog WHERE CustID = @CustId OR PremID = @PremID)

DELETE FROM daes_AEPEnergy..ChangeLog WHERE (CustID = @CustId OR PremID = @PremID)

UPDATE daes_AEPEnergy..Premise SET StatusId = 0/*, BeginServiceDate = NULL*/ WHERE CustId = @CustId

DELETE FROM daes_AEPEnergy..EnrollCustomer 
WHERE
 CsrCustId = @CustId

DELETE FROM daes_AEPEnergy..RateDetail 
WHERE
 RateDetId in (
 SELECT RateDetId FROM saes_AEPEnergy..RateDetail WHERE
  RateId = (SELECT RateId FROM saes_AEPEnergy..Customer WHERE CustId = @CustId) 
  AND RateTransitionId IN (SELECT RateTransitionId from saes_AEPEnergy..RateTransition WHERE CustId = @CustId AND RolloverFlag = 0)
 UNION
 SELECT RateDetId FROM saes_AEPEnergy..RateDetail WHERE RateId IN (SELECT RateId FROM saes_AEPEnergy..RateTransition WHERE CustId = @CustId)
 )

DELETE FROM daes_AEPEnergy..Product
WHERE
 RateId in (
 SELECT RateId FROM saes_AEPEnergy..Customer WHERE CustId = @CustId
 UNION
 SELECT RateId FROM saes_AEPEnergy..RateTransition WHERE CustId = @CustId
 )
 

DELETE FROM daes_AEPEnergy..RateTransition 
WHERE
 RateTransitionID in (
 SELECT RateTransitionID FROM saes_AEPEnergy..RateTransition WHERE CustId = @CustId
 UNION
 SELECT RateTransitionID FROM saes_AEPEnergy..RateTransition WHERE RateId = (SELECT RateId FROM saes_AEPEnergy..Customer WHERE CustId = @CustId)
 )

DELETE FROM daes_AEPEnergy..RateTransition 
WHERE CustId = @CustID

DELETE FROM daes_AEPEnergy..Rate
WHERE
 RateID in (
 SELECT RateId FROM saes_AEPEnergy..Customer WHERE CustId = @CustId
 UNION
 SELECT RateId FROM saes_AEPEnergy..RateTransition WHERE CustId = @CustId
 )


";
            try
            {
                DB.ExecuteQuery(sql, _appConfig.ConnectionCsr);
            }
            catch
            {
            }
            try
            {
                DB.ExecuteQuery(sql, _appConfig.ConnectionCsr);
            }
            catch
            {

            }
        }

        private static void Create814EMarketMock(string custNo, DateTime transactionDate)
        {
            string sql = $@"PRINT 'BEGIN Copy Market Header 814_R Files'
USE daes_AEPEnergyMarket
DECLARE @CustNo AS VARCHAR(20) = '{custNo}'
DECLARE @CustID AS INT = (SELECT CustID FROM saes_AEPEnergy..Customer WHERE CustNo = @CustNo)

SET IDENTITY_INSERT daes_AEPEnergyMarket..tbl_814_header ON
INSERT INTO daes_AEPEnergyMarket..tbl_814_header ([814_Key], [MarketFileId], [TransactionSetId], [TransactionSetControlNbr], [TransactionSetPurposeCode], [TransactionNbr], [TransactionDate], [ReferenceNbr], [ActionCode], [TdspDuns], [TdspName], [CrDuns], [CrName], [ProcessFlag], [ProcessDate], [Direction], [TransactionTypeID], [MarketID], [ProviderID], [POLRClass], [TransactionTime], [TransactionTimeCode], [TransactionQueueTypeID], [TransactionQualifier], [CreateDate])
SELECT  [814_Key], [MarketFileId], [TransactionSetId], [TransactionSetControlNbr], [TransactionSetPurposeCode], [TransactionNbr], '{transactionDate.ToString("yyyy-MM-dd")}', [ReferenceNbr], [ActionCode], [TdspDuns], [TdspName], [CrDuns], [CrName], 0/*[ProcessFlag]*/, NULL/*[ProcessDate]*/, [Direction], [TransactionTypeID], [MarketID], [ProviderID], [POLRClass], [TransactionTime], [TransactionTimeCode], [TransactionQueueTypeID], [TransactionQualifier], [CreateDate]
FROM   saes_AEPEnergyMarket..tbl_814_header sh 
WHERE TransactionSetId = '814' AND ActionCode = 'E' AND TransactionTypeId = 39 AND Direction = 1 AND [814_Key] IN (SELECT SourceId FROM saes_AEPEnergy..CustomerTransactionRequest WHERE CustID = @CustID AND TransactionTypeId = 39)
AND NOT EXISTS(SELECT 1 FROM daes_AEPEnergyMarket..tbl_814_header dh WHERE sh.[814_Key] = dh.[814_Key] )
SET IDENTITY_INSERT daes_AEPEnergyMarket..tbl_814_header OFF

SET IDENTITY_INSERT daes_AEPEnergyMarket..tbl_814_Service ON
INSERT INTO daes_AEPEnergyMarket..tbl_814_Service ([Service_Key], [814_Key], [AssignId], [ServiceTypeCode1], [ServiceType1], [ServiceTypeCode2], [ServiceType2], [ServiceTypeCode3], [ServiceType3], [ServiceTypeCode4], [ServiceType4], [ActionCode], [MaintenanceTypeCode], [DistributionLossFactorCode], [PremiseType], [BillType], [BillCalculator], [EsiId], [StationId], [SpecialNeedsIndicator], [PowerRegion], [EnergizedFlag], [EsiIdStartDate], [EsiIdEndDate], [EsiIdEligibilityDate], [NotificationWaiver], [SpecialReadSwitchDate], [PriorityCode], [PermitIndicator], [RTODate], [RTOTime], [CSAFlag], [MembershipID], [ESPAccountNumber], [LDCBillingCycle], [LDCBudgetBillingCycle], [WaterHeaters], [LDCBudgetBillingStatus], [PaymentArrangement], [NextMeterReadDate], [ParticipatingInterest], [EligibleLoadPercentage], [TaxExemptionPercent], [CapacityObligation], [TransmissionObligation], [TotalKWHHistory], [NumberOfMonthsHistory], [PeakDemandHistory], [AirConditioners], [PreviousEsiId], [GasPoolId], [LBMPZone], [ResidentialTaxPortion], [ESPCommodityPrice], [ESPFixedCharge], [ESPChargesCommTaxRate], [ESPChargesResTaxRate], [GasSupplyServiceOption], [FundsAuthorization], [BudgetBillingStatus], [FixedMonthlyCharge], [TaxRate], [CommodityPrice], [MeterCycleCodeDesc], [BillCycleCodeDesc], [FeeApprovedApplied], [MarketerCustomerAccountNumber], [GasSupplyServiceOptionCode], [HumanNeeds], [ReinstatementDate], [MeterCycleCode], [SystemNumber], [StateLicenseNumber], [SupplementalAccountNumber], [NewCustomerIndicator], [PaymentCategory], [PreviousESPAccountNumber], [RenewableEnergyIndicator], [SICCode], [ApprovalCodeIndicator], [RenewableEnergyCertification], [NewPremiseIndicator], [SalesResponsibility], [CustomerReferenceNumber], [TransactionReferenceNumber], [ESPTransactionNumber], [OldESPAccountNumber], [DFIIdentificationNumber], [DFIAccountNumber], [DFIIndicator1], [DFIIndicator2], [DFIQualifier], [DFIRoutingNumber], [SpecialReadSwitchTime], [LDCAccountBalance], [DisputedAmount], [CurrentBalance], [ArrearsBalance], [LDCSupplierBalance], [BudgetPlan], [BudgetInstallment], [Deposit], [RemainingUtilBalanceBucket1], [RemainingUtilBalanceBucket2], [RemainingUtilBalanceBucket3], [RemainingUtilBalanceBucket4], [RemainingUtilBalanceBucket5], [RemainingUtilBalanceBucket6], [IntervalStatusType], [CustomerAuthorization], [UnmeteredAcct], [PaymentOption], [MaxDailyAmt], [MeterAccessNote], [SpecialNeedsExpirationDate], [SwitchHoldStatusIndicator], [SpecialMeterConfig], [MaximumGeneration], [ServiceDeliveryPoint], [IgnoreRescind], [GasCapacityAssignment], [CPAEnrollmentTypes], [DaysInArrears], [RU_Notes], [RD_SiteCharacterDate], [SupplierPricingStructureNr], [SupplierGroupNumber], [IndustrialClassificationCode], [UtilityTaxExemptStatus], [AccountSettlementIndicator], [NypaDiscountIndicator], [UtilityDiscount], [IcapEffectiveDate], [FutureIcapEffectiveDate], [FutureIcap], [ChangeCancellationFee], [CancellationFee], [MunicipalAggregation])
SELECT  [Service_Key], [814_Key], [AssignId], [ServiceTypeCode1], [ServiceType1], [ServiceTypeCode2], [ServiceType2], [ServiceTypeCode3], [ServiceType3], [ServiceTypeCode4], [ServiceType4], [ActionCode], [MaintenanceTypeCode], [DistributionLossFactorCode], [PremiseType], [BillType], [BillCalculator], [EsiId], [StationId], [SpecialNeedsIndicator], [PowerRegion], [EnergizedFlag], [EsiIdStartDate], [EsiIdEndDate], [EsiIdEligibilityDate], [NotificationWaiver], [SpecialReadSwitchDate], [PriorityCode], [PermitIndicator], [RTODate], [RTOTime], [CSAFlag], [MembershipID], [ESPAccountNumber], [LDCBillingCycle], [LDCBudgetBillingCycle], [WaterHeaters], [LDCBudgetBillingStatus], [PaymentArrangement], [NextMeterReadDate], [ParticipatingInterest], [EligibleLoadPercentage], [TaxExemptionPercent], [CapacityObligation], [TransmissionObligation], [TotalKWHHistory], [NumberOfMonthsHistory], [PeakDemandHistory], [AirConditioners], [PreviousEsiId], [GasPoolId], [LBMPZone], [ResidentialTaxPortion], [ESPCommodityPrice], [ESPFixedCharge], [ESPChargesCommTaxRate], [ESPChargesResTaxRate], [GasSupplyServiceOption], [FundsAuthorization], [BudgetBillingStatus], [FixedMonthlyCharge], [TaxRate], [CommodityPrice], [MeterCycleCodeDesc], [BillCycleCodeDesc], [FeeApprovedApplied], [MarketerCustomerAccountNumber], [GasSupplyServiceOptionCode], [HumanNeeds], [ReinstatementDate], [MeterCycleCode], [SystemNumber], [StateLicenseNumber], [SupplementalAccountNumber], [NewCustomerIndicator], [PaymentCategory], [PreviousESPAccountNumber], [RenewableEnergyIndicator], [SICCode], [ApprovalCodeIndicator], [RenewableEnergyCertification], [NewPremiseIndicator], [SalesResponsibility], [CustomerReferenceNumber], [TransactionReferenceNumber], [ESPTransactionNumber], [OldESPAccountNumber], [DFIIdentificationNumber], [DFIAccountNumber], [DFIIndicator1], [DFIIndicator2], [DFIQualifier], [DFIRoutingNumber], [SpecialReadSwitchTime], [LDCAccountBalance], [DisputedAmount], [CurrentBalance], [ArrearsBalance], [LDCSupplierBalance], [BudgetPlan], [BudgetInstallment], [Deposit], [RemainingUtilBalanceBucket1], [RemainingUtilBalanceBucket2], [RemainingUtilBalanceBucket3], [RemainingUtilBalanceBucket4], [RemainingUtilBalanceBucket5], [RemainingUtilBalanceBucket6], [IntervalStatusType], [CustomerAuthorization], [UnmeteredAcct], [PaymentOption], [MaxDailyAmt], [MeterAccessNote], [SpecialNeedsExpirationDate], [SwitchHoldStatusIndicator], [SpecialMeterConfig], [MaximumGeneration], [ServiceDeliveryPoint], [IgnoreRescind], [GasCapacityAssignment], [CPAEnrollmentTypes], [DaysInArrears], [RU_Notes], [RD_SiteCharacterDate], [SupplierPricingStructureNr], [SupplierGroupNumber], [IndustrialClassificationCode], [UtilityTaxExemptStatus], [AccountSettlementIndicator], [NypaDiscountIndicator], [UtilityDiscount], [IcapEffectiveDate], [FutureIcapEffectiveDate], [FutureIcap], [ChangeCancellationFee], [CancellationFee], [MunicipalAggregation]
FROM   saes_AEPEnergyMarket..tbl_814_Service ss
WHERE [814_Key] IN  (SELECT SourceId FROM saes_AEPEnergy..CustomerTransactionRequest WHERE CustID = @CustID AND TransactionTypeId = 39 AND Direction = 1 )
AND NOT EXISTS(SELECT 1 FROM daes_AEPEnergyMarket..tbl_814_Service ds WHERE ss.[814_Key] = ds.[814_Key] )
SET IDENTITY_INSERT daes_AEPEnergyMarket..tbl_814_Service OFF

SET IDENTITY_INSERT daes_AEPEnergyMarket..tbl_814_Service_Date ON
INSERT INTO daes_AEPEnergyMarket..tbl_814_Service_Date ([Date_Key], [Service_Key], [Qualifier], [Date], [Time], [TimeCode], [PeriodFormat], [Period], [NotesDate])
SELECT  [Date_Key], [Service_Key], [Qualifier], [Date], [Time], [TimeCode], [PeriodFormat], [Period], [NotesDate]
FROM   saes_AEPEnergyMarket..tbl_814_Service_Date ss
WHERE [Service_Key] IN (SELECT Service_Key FROM daes_AEPEnergyMarket..tbl_814_Service WHERE [814_Key] IN (SELECT SourceId FROM saes_AEPEnergy..CustomerTransactionRequest WHERE CustID = @CustID AND TransactionTypeId = 39 AND Direction = 1 ))
AND NOT EXISTS(SELECT 1 FROM daes_AEPEnergyMarket..tbl_814_Service_Date dss WHERE ss.[Service_Key] = dss.[Service_Key] )
SET IDENTITY_INSERT daes_AEPEnergyMarket..tbl_814_Service_Date OFF


SET IDENTITY_INSERT daes_AEPEnergyMarket..tbl_814_Service_Meter ON
INSERT INTO daes_AEPEnergyMarket..tbl_814_Service_Meter ([Meter_Key], [Service_Key], [EntityIdCode], [MeterNumber], [MeterCode], [MeterType], [LoadProfile], [RateClass], [RateSubClass], [MeterCycle], [MeterCycleDayOfMonth], [SpecialNeedsIndicator], [OldMeterNumber], [MeterOwnerIndicator], [EntityType], [TimeOFUse], [ESPRateCode], [OrganizationName], [FirstName], [MiddleName], [NamePrefix], [NameSuffix], [IdentificationCode], [EntityName2], [EntityName3], [Address1], [Address2], [City], [State], [Zip], [CountryCode], [County], [PlanNumber], [ServicesReferenceNumber], [AffiliationNumber], [CostElement], [CoverageCode], [LossReportNumber], [GeographicNumber], [ItemNumber], [LocationNumber], [PriceListNumber], [ProductType], [QualityInspectionArea], [ShipperCarOrderNumber], [StandardPointLocation], [ReportIdentification], [Supplier], [Area], [CollectorIdentification], [VendorAgentNumber], [VendorAbbreviation], [VendorIdNumber], [VendorOrderNumber], [PricingStructureCode], [MeterOwnerDUNS], [MeterOwner], [MeterInstallerDUNS], [MeterInstaller], [MeterReaderDUNS], [MeterReader], [MeterMaintenanceProviderDUNS], [MeterMaintenanceProvider], [MeterDataManagementAgentDUNS], [MeterDataManagementAgent], [SchedulingCoordinatorDUNS], [SchedulingCoordinator], [MeterInstallPending], [PackageOption], [UsageCode], [MeterServiceVoltage], [LossFactor], [AMSIndicator], [SummaryInterval], [NextCycleRate], [VariableRateIndicator], [RateTerm], [RateTermDateExpirationDate])
SELECT  [Meter_Key], [Service_Key], [EntityIdCode], [MeterNumber], [MeterCode], [MeterType], [LoadProfile], [RateClass], [RateSubClass], [MeterCycle], [MeterCycleDayOfMonth], [SpecialNeedsIndicator], [OldMeterNumber], [MeterOwnerIndicator], [EntityType], [TimeOFUse], [ESPRateCode], [OrganizationName], [FirstName], [MiddleName], [NamePrefix], [NameSuffix], [IdentificationCode], [EntityName2], [EntityName3], [Address1], [Address2], [City], [State], [Zip], [CountryCode], [County], [PlanNumber], [ServicesReferenceNumber], [AffiliationNumber], [CostElement], [CoverageCode], [LossReportNumber], [GeographicNumber], [ItemNumber], [LocationNumber], [PriceListNumber], [ProductType], [QualityInspectionArea], [ShipperCarOrderNumber], [StandardPointLocation], [ReportIdentification], [Supplier], [Area], [CollectorIdentification], [VendorAgentNumber], [VendorAbbreviation], [VendorIdNumber], [VendorOrderNumber], [PricingStructureCode], [MeterOwnerDUNS], [MeterOwner], [MeterInstallerDUNS], [MeterInstaller], [MeterReaderDUNS], [MeterReader], [MeterMaintenanceProviderDUNS], [MeterMaintenanceProvider], [MeterDataManagementAgentDUNS], [MeterDataManagementAgent], [SchedulingCoordinatorDUNS], [SchedulingCoordinator], [MeterInstallPending], [PackageOption], [UsageCode], [MeterServiceVoltage], [LossFactor], [AMSIndicator], [SummaryInterval], [NextCycleRate], [VariableRateIndicator], [RateTerm], [RateTermDateExpirationDate]
FROM   saes_AEPEnergyMarket..tbl_814_Service_Meter ss
WHERE [Service_Key] IN (SELECT Service_Key FROM daes_AEPEnergyMarket..tbl_814_Service WHERE [814_Key] IN (SELECT SourceId FROM saes_AEPEnergy..CustomerTransactionRequest WHERE CustID = @CustID AND TransactionTypeId = 39 AND Direction = 1 ))
AND NOT EXISTS(SELECT 1 FROM daes_AEPEnergyMarket..tbl_814_Service_Meter dss WHERE ss.[Service_Key] = dss.[Service_Key] )
SET IDENTITY_INSERT daes_AEPEnergyMarket..tbl_814_Service_Meter OFF

";
            DB.ExecuteQuery(sql, _appConfig.ConnectionCsr);
        }

        private static void CreateProducts(string filter)
        {
            string dirToProcess = Path.Combine(Path.GetDirectoryName(Assembly.GetEntryAssembly().Location), "MockData");
            DB.ImportTextFiles(dirToProcess, filter, _appConfig.ConnectionCsr);
            //Directory.EnumerateFiles(dirToProcess, $"*{filter}*.txt", SearchOption.AllDirectories).ForEach(
            //    filename =>
            //    {
            //        DB.Import2DatabaseFromTextFile(filename, _appConfig.ConnectionCsr);
            //    }
            //);
        }

        private static void CopyProduct(int productId, int maxDepth = 30)
        {
            string sql = $@"
USE daes_AEPEnergy
DECLARE @MAX_DEPTH INT = {maxDepth};
DECLARE @CURR_PRODUCT_ID INT = 0;
DECLARE @PREV_PRODUCT_ID INT = {productId};
DECLARE @RATE_ID INT = 0;
DECLARE @CTR INT = 0;

DECLARE @Products TABLE (cpProductId INT, cpRollOverProductID INT);
DECLARE @Rates TABLE (RateId INT);

WHILE @CTR < @MAX_DEPTH
BEGIN
	SELECT @RATE_ID = RateId, @CURR_PRODUCT_ID = RollOverProductId FROM saes_AEPEnergy..Product WHERE ProductId = @PREV_PRODUCT_ID;

	INSERT INTO @Rates VALUES (@RATE_ID)
	
	IF ISNULL(@CURR_PRODUCT_ID, @PREV_PRODUCT_ID)  = @PREV_PRODUCT_ID --if there
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
SET IDENTITY_INSERT daes_AEPEnergy..Rate ON
INSERT INTO daes_AEPEnergy..Rate
( RateID,CSPID,RateCode,RateDesc,EffectiveDate,ExpirationDate,RateType,PlanType,IsMajority,TemplateFlag,LDCCode,CreateDate,UserID,RatePackageName,CustType,ServiceType,DivisionCode,ConsUnitId,ActiveFlag,LDCRateCode,migr_plan_id,migr_custid)
SELECT
 RateID,CSPID,RateCode,RateDesc,EffectiveDate,ExpirationDate,RateType,PlanType,IsMajority,TemplateFlag,LDCCode,CreateDate,UserID,RatePackageName,CustType,ServiceType,DivisionCode,ConsUnitId,ActiveFlag,LDCRateCode,migr_plan_id,migr_custid
FROM saes_AEPEnergy..Rate src
WHERE
  src.RateId IN (SELECT RateId FROM @Rates)
  AND NOT EXISTS(SELECT 1 FROM daes_AEPEnergy..Rate dst WHERE src.RateId = dst.RateId )
SET IDENTITY_INSERT daes_AEPEnergy..Rate OFF


PRINT 'Copy RateDetail'
SET IDENTITY_INSERT daes_AEPEnergy..RateDetail ON
INSERT INTO daes_AEPEnergy..RateDetail
( RateDetID,RateID,CategoryID,RateTypeID,ConsUnitID,RateDescID,EffectiveDate,ExpirationDate,RateAmt,RateAmt2,RateAmt3,FixedAdder,MinDetAmt,MaxDetAmt,GLAcct,RangeLower,RangeUpper,CustType,Graduated,Progressive,AmountCap,MaxRateAmt,MinRateAmt,CategoryRollup,Taxable,ChargeType,MiscData1,FixedCapRate,ScaleFactor1,ScaleFactor2,TemplateRateDetID,Margin,HALRateDetailId,UsageClassId,LegacyRateDetailId,Building,ServiceTypeID,TaxCategoryID,UtilityID,UtilityInvoiceTemplateDetailID,Active,StatusID,RateVariableTypeId,MinDays,MaxDays,BlockPriceIndicator,RateTransitionId,CreateDate,MeterMultiplierFlag,BlendRatio,ContractVolumeID,CreatedByUserId,ModifiedByUserId,ModifiedDate,TOUTemplateID,TOUTemplateRegisterID,TOUTemplateRegisterName)
SELECT
 RateDetID,RateID,CategoryID,RateTypeID,ConsUnitID,RateDescID,EffectiveDate,ExpirationDate,RateAmt,RateAmt2,RateAmt3,FixedAdder,MinDetAmt,MaxDetAmt,GLAcct,RangeLower,RangeUpper,CustType,Graduated,Progressive,AmountCap,MaxRateAmt,MinRateAmt,CategoryRollup,Taxable,ChargeType,MiscData1,FixedCapRate,ScaleFactor1,ScaleFactor2,TemplateRateDetID,Margin,HALRateDetailId,UsageClassId,LegacyRateDetailId,Building,ServiceTypeID,TaxCategoryID,UtilityID,UtilityInvoiceTemplateDetailID,Active,StatusID,RateVariableTypeId,MinDays,MaxDays,BlockPriceIndicator,RateTransitionId,CreateDate,MeterMultiplierFlag,BlendRatio,ContractVolumeID,CreatedByUserId,ModifiedByUserId,ModifiedDate,TOUTemplateID,TOUTemplateRegisterID,TOUTemplateRegisterName
FROM daes_AEPEnergy..RateDetail src
WHERE
  src.RateId IN (SELECT RateId FROM @Rates)
  AND NOT EXISTS(SELECT 1 FROM daes_AEPEnergy..RateDetail dst WHERE src.RateDetId = dst.RateDetId )
SET IDENTITY_INSERT daes_AEPEnergy..RateDetail OFF


PRINT 'Copy Product'
SET IDENTITY_INSERT daes_AEPEnergy..Product ON
INSERT INTO daes_AEPEnergy..Product
( [ProductID], [RateID], [LDCCode], [PlanType], [TDSPTemplateID], [Description], [BeginDate], [EndDate], [CustType], [Graduated], [RangeTier1], [RangeTier2], [SortOrder], [ActiveFlag], [Uplift], [CSATDSPTemplateID], [CAATDSPTemplateID], [PriceDescription], [MarketingCode], [RateTypeID], [ConsUnitID], [Default], [DivisionCode], [RateDescription], [ServiceType], [CSPId], [TermsId], [RolloverProductId], [CommissionId], [CommissionAmt], [CancelFeeId], [MonthlyChargeId], [ProductCode], [RatePackageId], [ProductName], [TermDate], [DiscountTypeId], [DiscountAmount], [ProductZoneID], [IsGreen], [IsBestChoice], [ActiveEnrollmentFlag], [CreditScoreThreshold], [DepositAmount], [Incentives])
SELECT
 [ProductID], [RateID], [LDCCode], [PlanType], [TDSPTemplateID], [Description], [BeginDate], [EndDate], [CustType], [Graduated], [RangeTier1], [RangeTier2], [SortOrder], [ActiveFlag], [Uplift], [CSATDSPTemplateID], [CAATDSPTemplateID], [PriceDescription], [MarketingCode], [RateTypeID], [ConsUnitID], [Default], [DivisionCode], [RateDescription], [ServiceType], [CSPId], [TermsId], [cpRolloverProductId], [CommissionId], [CommissionAmt], [CancelFeeId], [MonthlyChargeId], [ProductCode], [RatePackageId], [ProductName], [TermDate], [DiscountTypeId], [DiscountAmount], [ProductZoneID], [IsGreen], [IsBestChoice], [ActiveEnrollmentFlag], [CreditScoreThreshold], [DepositAmount], [Incentives]
FROM @Products cp
INNER JOIN saes_AEPEnergy..Product src ON cp.cpProductId = src.ProductId
WHERE
  src.ProductID IN (SELECT ProductID FROM @Products)
  AND NOT EXISTS(SELECT 1 FROM daes_AEPEnergy..Product dst WHERE src.ProductId = dst.ProductId )
SET IDENTITY_INSERT daes_AEPEnergy..Product OFF


";
            DB.ExecuteQuery(sql, _appConfig.ConnectionCsr);
        }

        private static void CopyCustomerWithNumber(string custNo)
        {
            string sql = $"SELECT CustID FROM saes_AEPEnergy..Customer WHERE CustNo = '{custNo}'";
            int custId = DB.ReadSingleValue<int>(sql, _appConfig.ConnectionCsr);
            
            if (custId > 0)
            {
                CopyCustomerWithID(custId);
            }
        }

        private static void CopyCustomerWithID(int custId)
        {
            string sql = $@"USE daes_AEPEnergy
DECLARE @ClientID AS INT = (SELECT ClientID FROM daes_BillingAdmin..Client WHERE Client='AEP')
DECLARE @CustID AS INT = {custId}
DECLARE @CustNo AS VARCHAR(20) = (SELECT CustNo FROM saes_AEPEnergy..Customer WHERE CustID = @CustID)

PRINT 'BEGIN Copy Customer'

PRINT 'Copy Addresses'
SET IDENTITY_INSERT daes_AEPEnergy..Address ON
INSERT INTO daes_AEPEnergy..Address ([AddrID], [ValidationStatusID], [AttnMS], [Addr1], [Addr2], [City], [State], [Zip], [Zip4], [DPBC], [CityID], [CountyID], [County], [HomePhone], [WorkPhone], [FaxPhone], [OtherPhone], [Email], [ESIID], [GeoCode], [Status], [DeliveryPointCode], [CreateDate], [Migr_Enrollid], [PhoneExtension], [OtherExtension], [FaxExtension], [TaxingDistrict], [TaxInCity], [CchVersion])
SELECT [AddrID], [ValidationStatusID], [AttnMS], [Addr1], [Addr2], [City], [State], [Zip], [Zip4], [DPBC], [CityID], [CountyID], [County], [HomePhone], [WorkPhone], [FaxPhone], [OtherPhone], [Email], [ESIID], [GeoCode], [Status], [DeliveryPointCode], [CreateDate], [Migr_Enrollid], [PhoneExtension], [OtherExtension], [FaxExtension], [TaxingDistrict], [TaxInCity], [CchVersion] 
FROM  saes_AEPEnergy..Address src
WHERE
    AddrID IN (
	    SELECT SiteAddrId FROM saes_AEPEnergy..Customer WHERE CustID = @CustID
        UNION
        SELECT MailAddrId FROM saes_AEPEnergy..Customer WHERE CustID = @CustID
        UNION
        SELECT CorrAddrId FROM saes_AEPEnergy..Customer WHERE CustID = @CustID
        UNION
        SELECT AddrId FROM saes_AEPEnergy..Premise WHERE CustID = @CustID
    )
    AND NOT EXISTS (SELECT 1 FROM daes_AEPEnergy..Address dst WHERE src.AddrID = dst.AddrId)

SET IDENTITY_INSERT daes_AEPEnergy..Address OFF

PRINT 'Copy Rate'
SET IDENTITY_INSERT daes_AEPEnergy..Rate ON
INSERT INTO daes_AEPEnergy..Rate ([RateID], [CSPID], [RateCode], [RateDesc], [EffectiveDate], [ExpirationDate], [RateType], [PlanType], [IsMajority], [TemplateFlag], [LDCCode], [CreateDate], [UserID], [RatePackageName], [CustType], [ServiceType], [DivisionCode], [ConsUnitId], [ActiveFlag], [LDCRateCode], [migr_plan_id], [migr_custid])
SELECT  [RateID], [CSPID], [RateCode], [RateDesc], [EffectiveDate], [ExpirationDate], [RateType], [PlanType], [IsMajority], [TemplateFlag], [LDCCode], [CreateDate], [UserID], [RatePackageName], [CustType], [ServiceType], [DivisionCode], [ConsUnitId], [ActiveFlag], [LDCRateCode], [migr_plan_id], [migr_custid]
FROM  saes_AEPEnergy..Rate src
WHERE
    RateID IN (
	    SELECT RateID FROM saes_AEPEnergy..Customer WHERE CustID = @CustID
        UNION
        SELECT RateID FROM saes_AEPEnergy..Product WHERE ProductId IN (SELECT ProductID FROM saes_AEPEnergy..Contract WHERE CustID = @CustID)
    )
    AND NOT EXISTS(SELECT 1 FROM daes_AEPEnergy..Rate dst WHERE src.RateID = dst.RateID )
SET IDENTITY_INSERT daes_AEPEnergy..Rate OFF

PRINT 'Copy Customer'
SET IDENTITY_INSERT daes_AEPEnergy..Customer ON

INSERT INTO daes_AEPEnergy..Customer ([CustID], [CSPID], [CSPCustID], [PropertyID], [PropertyCustID], [CustomerTypeID], [CustNo], [CustName], [LastName], [FirstName], [MidName], [CompanyName], [DBA], [FederalTaxID], [AcctsRecID], [DistributedAR], [ProductionCycle], [BillCycle], [RateID], [SiteAddrID], [MailAddrId], [CorrAddrID], [MailToSiteAddress], [BillCustID], [MasterCustID], [Master], [CustStatus], [BilledThru], [CSRStatus], [CustType], [Services], [FEIN], [DOB], [Taxable], [LateFees], [NoOfAccts], [ConsolidatedInv], [SummaryInv], [MsgID], [TDSPTemplateID], [TDSPGroupID], [LifeSupportIndictor], [LifeSupportStatus], [LifeSupportDate], [SpecialBenefitsPlan], [BillFormat], [PrintLayoutID], [CreditScore], [HitIndicator], [RequiredDeposit], [AccountManager], [EnrollmentAlias], [ContractID], [ContractTerm], [ContractStartDate], [ContractEndDate], [UserDefined1], [CreateDate], [RateChangeDate], [ConversionAccountNo], [PermitContactName], [CustomerPrivacy], [UsagePrivacy], [CompanyRegistrationNumber], [VATNumber], [AccountStatus], [AutoCreditAfterInvoiceFlag], [LidaDiscount], [DoNotDisconnect], [DDPlus1], [CsrImportDate], [DeliveryTypeID], [SpecialNeedsAddrID], [PaymentModelId], [PORFlag], [PowerOutageAddrId])
SELECT [CustID], [CSPID], [CSPCustID], [PropertyID], [PropertyCustID], [CustomerTypeID], [CustNo], [CustName], [LastName], [FirstName], [MidName], [CompanyName], [DBA], [FederalTaxID], [AcctsRecID], [DistributedAR], [ProductionCycle], [BillCycle], [RateID], [SiteAddrID], [MailAddrId], [CorrAddrID], [MailToSiteAddress], [BillCustID], [MasterCustID], [Master], [CustStatus], [BilledThru], [CSRStatus], [CustType], [Services], [FEIN], [DOB], [Taxable], [LateFees], [NoOfAccts], [ConsolidatedInv], [SummaryInv], [MsgID], [TDSPTemplateID], [TDSPGroupID], [LifeSupportIndictor], [LifeSupportStatus], [LifeSupportDate], [SpecialBenefitsPlan], [BillFormat], [PrintLayoutID], [CreditScore], [HitIndicator], [RequiredDeposit], [AccountManager], [EnrollmentAlias], [ContractID], [ContractTerm], [ContractStartDate], [ContractEndDate], [UserDefined1], [CreateDate], [RateChangeDate], [ConversionAccountNo], [PermitContactName], [CustomerPrivacy], [UsagePrivacy], [CompanyRegistrationNumber], [VATNumber], [AccountStatus], [AutoCreditAfterInvoiceFlag], [LidaDiscount], [DoNotDisconnect], [DDPlus1], [CsrImportDate], [DeliveryTypeID], [SpecialNeedsAddrID], [PaymentModelId], [PORFlag], [PowerOutageAddrId]
FROM  saes_AEPEnergy..Customer WHERE CustID = @CustID
AND NOT EXISTS(SELECT 1 FROM daes_AEPEnergy..Customer WHERE CustId = @CustID)
SET IDENTITY_INSERT daes_AEPEnergy..Customer OFF

PRINT 'Copy Premise'
ALTER TABLE daes_AEPEnergy..Customer NOCHECK CONSTRAINT ALL
ALTER TABLE daes_AEPEnergy..Premise NOCHECK CONSTRAINT ALL
SET IDENTITY_INSERT daes_AEPEnergy..Premise ON

INSERT INTO daes_AEPEnergy..Premise ([PremID], [CustID], [CSPID], [AddrID], [TDSPTemplateID], [ServiceCycle], [TDSP], [TaxAssessment], [PremNo], [PremDesc], [PremStatus], [PremType], [LocationCode], [SpecialNeedsFlag], [SpecialNeedsStatus], [SpecialNeedsDate], [ReadingIncrement], [Metered], [Taxable], [BeginServiceDate], [EndServiceDate], [SourceLevel], [StatusID], [StatusDate], [CreateDate], [UnitID], [PropertyCommonID], [RateID], [DeleteFlag], [LBMPId], [PipelineId], [GasLossId], [LDCID], [GasPoolID], [DeliveryPoint], [ConsumptionBandIndex], [LastModifiedDate], [CreatedByID], [ModifiedByID], [BillingAccountNumber], [NameKey], [GasSupplyServiceOption], [IntervalUsageTypeId], [LDC_UnMeteredAcct], [AltPremNo], [OnSwitchHold], [SwitchHoldStartDate], [ConsumptionImportTypeId], [TDSPTemplateEffectiveDate], [ServiceDeliveryPoint], [UtilityContractID], [LidaDiscount], [GasCapacityAssignment], [CPAEnrollmentTypes], [IsTOU], [SupplierPricingStructureNr], [SupplierGroupNumber])
SELECT [PremID], [CustID], [CSPID], [AddrID], [TDSPTemplateID], [ServiceCycle], [TDSP], [TaxAssessment], [PremNo], [PremDesc], [PremStatus], [PremType], [LocationCode], [SpecialNeedsFlag], [SpecialNeedsStatus], [SpecialNeedsDate], [ReadingIncrement], [Metered], [Taxable], NULL/*[BeginServiceDate]*/, [EndServiceDate], [SourceLevel], 0/*[StatusID]*/, [StatusDate], [CreateDate], [UnitID], [PropertyCommonID], [RateID], [DeleteFlag], [LBMPId], [PipelineId], [GasLossId], [LDCID], [GasPoolID], [DeliveryPoint], [ConsumptionBandIndex], [LastModifiedDate], [CreatedByID], [ModifiedByID], [BillingAccountNumber], [NameKey], [GasSupplyServiceOption], [IntervalUsageTypeId], [LDC_UnMeteredAcct], [AltPremNo], [OnSwitchHold], [SwitchHoldStartDate], [ConsumptionImportTypeId], [TDSPTemplateEffectiveDate], [ServiceDeliveryPoint], [UtilityContractID], [LidaDiscount], [GasCapacityAssignment], [CPAEnrollmentTypes], [IsTOU], [SupplierPricingStructureNr], [SupplierGroupNumber]
FROM  saes_AEPEnergy..Premise src
WHERE
    CustID = @CustID
    AND NOT EXISTS(SELECT 1 FROM daes_AEPEnergy..Premise dst WHERE src.PremId = dst.PremId)
SET IDENTITY_INSERT daes_AEPEnergy..Premise OFF
ALTER TABLE daes_AEPEnergy..Premise WITH CHECK CHECK CONSTRAINT ALL

PRINT 'Copy Product'
SET IDENTITY_INSERT daes_AEPEnergy..Product ON

INSERT INTO daes_AEPEnergy..Product ([ProductID], [RateID], [LDCCode], [PlanType], [TDSPTemplateID], [Description], [BeginDate], [EndDate], [CustType], [Graduated], [RangeTier1], [RangeTier2], [SortOrder], [ActiveFlag], [Uplift], [CSATDSPTemplateID], [CAATDSPTemplateID], [PriceDescription], [MarketingCode], [RateTypeID], [ConsUnitID], [Default], [DivisionCode], [RateDescription], [ServiceType], [CSPId], [TermsId], [RolloverProductId], [CommissionId], [CommissionAmt], [CancelFeeId], [MonthlyChargeId], [ProductCode], [RatePackageId], [ProductName], [TermDate], [DiscountTypeId], [DiscountAmount], [ProductZoneID], [IsGreen], [IsBestChoice], [ActiveEnrollmentFlag], [CreditScoreThreshold], [DepositAmount], [Incentives])
SELECT  [ProductID], [RateID], [LDCCode], [PlanType], [TDSPTemplateID], [Description], [BeginDate], [EndDate], [CustType], [Graduated], [RangeTier1], [RangeTier2], [SortOrder], [ActiveFlag], [Uplift], [CSATDSPTemplateID], [CAATDSPTemplateID], [PriceDescription], [MarketingCode], -1 /*[RateTypeID]*/, [ConsUnitID], [Default], [DivisionCode], [RateDescription], [ServiceType], [CSPId], [TermsId], /*[RolloverProductId]*/NULL, [CommissionId], [CommissionAmt], [CancelFeeId], [MonthlyChargeId], [ProductCode], [RatePackageId], [ProductName], [TermDate], [DiscountTypeId], [DiscountAmount], [ProductZoneID], [IsGreen], [IsBestChoice], [ActiveEnrollmentFlag], [CreditScoreThreshold], [DepositAmount], [Incentives]
FROM  saes_AEPEnergy..Product src
WHERE 
    ProductId IN (SELECT ProductID FROM saes_AEPEnergy..Contract WHERE CustID = @CustID)
    AND NOT EXISTS(SELECT 1 FROM daes_AEPEnergy..Product dst WHERE src.ProductId = dst.ProductId)
SET IDENTITY_INSERT daes_AEPEnergy..Product OFF

PRINT 'Copy Contract'
SET IDENTITY_INSERT daes_AEPEnergy..Contract ON

INSERT INTO daes_AEPEnergy..Contract ([ContractID], [SignedDate], [BeginDate], [EndDate], [TermLength], [ContractTypeID], [ContactName], [ContactPhone], [ContactFax], [ProductID], [CreatedByID], [CreateDate], [CustID], [AutoRenewFlag], [Service], [ActiveFlag], [RateCode], [TDSPTemplateID], [ContractTerm], [RateDetID], [RateID], [Terms], [ContractName], [ContractLength], [AccountManagerID], [MeterChargeCode], [AggregatorFee], [TermDate], [Bandwidth], [FinanceCharge], [ContractNumber], [PremID], [AnnualUsage], [CurePeriod], [ContractStatusID], [RenewalRate], [RenewalStartDate], [RenewalTerm], [ChangeReason])
SELECT  [ContractID], [SignedDate], [BeginDate], [EndDate], [TermLength], [ContractTypeID], [ContactName], [ContactPhone], [ContactFax], [ProductID], [CreatedByID], [CreateDate], [CustID], [AutoRenewFlag], [Service], [ActiveFlag], [RateCode], [TDSPTemplateID], [ContractTerm], [RateDetID], [RateID], [Terms], [ContractName], [ContractLength], [AccountManagerID], [MeterChargeCode], [AggregatorFee], [TermDate], [Bandwidth], [FinanceCharge], [ContractNumber], [PremID], [AnnualUsage], [CurePeriod], [ContractStatusID], [RenewalRate], [RenewalStartDate], [RenewalTerm], [ChangeReason]
FROM  saes_AEPEnergy..Contract src
WHERE
    CustID = @CustID
    AND NOT EXISTS(SELECT 1 FROM daes_AEPEnergy..Contract dst WHERE src.ContractID = dst.ContractID)
SET IDENTITY_INSERT daes_AEPEnergy..Contract OFF

PRINT 'Copy AccountsReceivable'
SET IDENTITY_INSERT daes_AEPEnergy..AccountsReceivable ON

INSERT INTO daes_AEPEnergy..AccountsReceivable ([AcctsRecID], [ResetDate], [ARDate], [PrevBal], [CurrInvs], [CurrPmts], [CurrAdjs], [BalDue], [LateFee], [LateFeeRate], [LateFeeMaxAmount], [LateFeeTypeID], [AuthorizedPymt], [PastDue], [BalAge0], [BalAge1], [BalAge2], [BalAge3], [BalAge4], [BalAge5], [BalAge6], [Deposit], [DepositBeginDate], [PaymentPlanFlag], [PaymentPlanTrueUpFlag], [PaymentPlanAmount], [PaymentPlanTrueUpPeriod], [PaymentPlanTrueUpThresholdAmount], [PaymentPlanTrueUpType], [PaymentPlanEffectiveDate], [PrePaymentFlag], [PrePaymentDailyAmount], [CapitalCredit], [Terms], [StatusID], [GracePeriod], [PaymentPlanTotalVariance], [PaymentPlanVarianceUnit], [LateFeeGracePeriod], [CancelFeeTypeId], [CancelFeeAmount], [Migr_acct_no], [InvoiceMinimumAmount], [LateFeeThresholdAmt], [LastInvoiceAcctsRecHistID], [LastPaymentAcctsRecHistId], [LastAdjustmentAcctsRecHistId], [DeferredBalance])
SELECT  [AcctsRecID], [ResetDate], [ARDate], [PrevBal], [CurrInvs], [CurrPmts], [CurrAdjs], [BalDue], [LateFee], [LateFeeRate], [LateFeeMaxAmount], [LateFeeTypeID], [AuthorizedPymt], [PastDue], [BalAge0], [BalAge1], [BalAge2], [BalAge3], [BalAge4], [BalAge5], [BalAge6], [Deposit], [DepositBeginDate], [PaymentPlanFlag], [PaymentPlanTrueUpFlag], [PaymentPlanAmount], [PaymentPlanTrueUpPeriod], [PaymentPlanTrueUpThresholdAmount], [PaymentPlanTrueUpType], [PaymentPlanEffectiveDate], [PrePaymentFlag], [PrePaymentDailyAmount], [CapitalCredit], [Terms], [StatusID], [GracePeriod], [PaymentPlanTotalVariance], [PaymentPlanVarianceUnit], [LateFeeGracePeriod], [CancelFeeTypeId], [CancelFeeAmount], [Migr_acct_no], [InvoiceMinimumAmount], [LateFeeThresholdAmt], [LastInvoiceAcctsRecHistID], [LastPaymentAcctsRecHistId], [LastAdjustmentAcctsRecHistId], [DeferredBalance]
FROM  saes_AEPEnergy..AccountsReceivable src
WHERE
    AcctsRecID IN (SELECT AcctsRecID FROM saes_AEPEnergy..Customer WHERE CustID = @CustID)
    AND NOT EXISTS(SELECT 1 FROM daes_AEPEnergy..AccountsReceivable dst WHERE src.AcctsRecID = dst.AcctsRecID)
SET IDENTITY_INSERT daes_AEPEnergy..AccountsReceivable OFF

PRINT 'Copy CustomerAdditionalInfo'
--SET IDENTITY_INSERT daes_AEPEnergy..CustomerAdditionalInfo ON

INSERT INTO daes_AEPEnergy..CustomerAdditionalInfo ([CustID], [CSPDUNSID], [BillingTypeID], [BillingDayOfMonth], [MasterCustID_2], [MasterCustID_3], [MasterCustID_4], [TaxAssessment], [ContractPeriod], [ContractDate], [AccessVerificationType], [AccessVerificationData], [ClientAccountNo], [InstitutionID], [TransitNum], [AccountNum], [MigrationAccountNo], [MigrationFirstServed], [MigrationKwh], [CollectionsStageID], [CollectionsStatus], [CollectionsDate], [KeyAccount], [DisconnectLtr], [AuthorizedReleaseName], [AuthorizedReleaseDOB], [AuthorizedReleaseFederalTaxID], [EFTFlag], [PromiseToPayFlag], [DisconnectFlag], [CreditHoldFlag], [RawConsumptionImportFlag], [CustomerProtectionStatus], [MCPEFlag], [HasLocationMasterFlag], [DivisionID], [DivisionCode], [DriverLicenseNo], [PromotionCodeID], [CustomerDUNS], [CustomerGroupID], [EarlyTermFee], [EarlyTermFeeUpdateDate], [DeceasedFlag], [BankruptFlag], [CollectionsAgencyID], [DoNotCall], [CustomerSecretWord], [PrintGroupID], [CurrentCustNo], [IsDPP], [UnitNumber], [CustomerCategoryID], [SubsequentDepositExempt], [AutoPayFlag], [SpecialNeedsFlag], [SpecialNeedsEndDate], [SpecialNeedsQualifierTypeID], [CsrImportDate], [OnSwitchHold], [SwitchHoldStartDate], [DPPStatusID], [UpdateContactInfoFlag], [IsFriendlyLatePaymentReminderSent], [IsLowIncome], [ExtendedCustTypeId], [AutoPayLastUpdated], [GreenEnergyOptIn], [SocialCauseID], [SocialCauseCode], [SecondaryContactFirstName], [SecondaryContactLastName], [SecondaryContactPhone], [SecondaryContactRelationId], [SSN], [ServiceAccount], [IsPUCComplaint], [EncryptionPasswordTypeId], [EncryptionPasswordCustomValue], [CustInfo1], [CustInfo2], [CustInfo3], [CustInfo4], [CustInfo5], [SalesAgent], [Broker], [PromoCode], [CommissionType], [CommissionAmount], [ReferralID], [CampaignName], [AccessDBID], [SalesChannel], [TCPAAuthorization], [CancellationFee], [MunicipalAggregation])
SELECT  [CustID], [CSPDUNSID], [BillingTypeID], [BillingDayOfMonth], [MasterCustID_2], [MasterCustID_3], [MasterCustID_4], [TaxAssessment], [ContractPeriod], [ContractDate], [AccessVerificationType], [AccessVerificationData], [ClientAccountNo], [InstitutionID], [TransitNum], [AccountNum], [MigrationAccountNo], [MigrationFirstServed], [MigrationKwh], [CollectionsStageID], [CollectionsStatus], [CollectionsDate], [KeyAccount], [DisconnectLtr], [AuthorizedReleaseName], [AuthorizedReleaseDOB], [AuthorizedReleaseFederalTaxID], [EFTFlag], [PromiseToPayFlag], [DisconnectFlag], [CreditHoldFlag], [RawConsumptionImportFlag], [CustomerProtectionStatus], [MCPEFlag], [HasLocationMasterFlag], [DivisionID], [DivisionCode], [DriverLicenseNo], [PromotionCodeID], [CustomerDUNS], [CustomerGroupID], [EarlyTermFee], [EarlyTermFeeUpdateDate], [DeceasedFlag], [BankruptFlag], [CollectionsAgencyID], [DoNotCall], [CustomerSecretWord], [PrintGroupID], [CurrentCustNo], [IsDPP], [UnitNumber], [CustomerCategoryID], [SubsequentDepositExempt], [AutoPayFlag], [SpecialNeedsFlag], [SpecialNeedsEndDate], [SpecialNeedsQualifierTypeID], [CsrImportDate], [OnSwitchHold], [SwitchHoldStartDate], [DPPStatusID], [UpdateContactInfoFlag], [IsFriendlyLatePaymentReminderSent], [IsLowIncome], [ExtendedCustTypeId], [AutoPayLastUpdated], [GreenEnergyOptIn], [SocialCauseID], [SocialCauseCode], [SecondaryContactFirstName], [SecondaryContactLastName], [SecondaryContactPhone], [SecondaryContactRelationId], [SSN], [ServiceAccount], [IsPUCComplaint], [EncryptionPasswordTypeId], [EncryptionPasswordCustomValue], [CustInfo1], [CustInfo2], [CustInfo3], [CustInfo4], [CustInfo5], [SalesAgent], [Broker], [PromoCode], [CommissionType], [CommissionAmount], [ReferralID], [CampaignName], [AccessDBID], [SalesChannel], [TCPAAuthorization], [CancellationFee], [MunicipalAggregation]
FROM  saes_AEPEnergy..CustomerAdditionalInfo WHERE CustID = @CustID
AND NOT EXISTS(SELECT 1 FROM daes_AEPEnergy..CustomerAdditionalInfo WHERE CustID = @CustID)
--SET IDENTITY_INSERT daes_AEPEnergy..CustomerAdditionalInfo OFF

PRINT 'END Copy Customer'";
            DB.ExecuteQuery(sql, _appConfig.ConnectionCsr);
        }

        private static void CopyRateWithCustomerNumber(string custNo)
        {
            var sql = $"SELECT CustID FROM saes_AEPEnergy..Customer WHERE CustNo = '{custNo}";
            int custId = DB.ReadSingleValue<int>(sql, _appConfig.ConnectionCsr);
            if (custId > 0)
            {
                CopyRateWithCustomerID(custId);
            }
        }

        private static void CopyRateWithCustomerID(int custId)
        {
            string sql = $@"
USE daes_AEPEnergy
DECLARE @CustID AS INT = {custId}
DECLARE @CustNo AS VARCHAR(20) = (SELECT CustNo FROM saes_AEPEnergy..Customer WHERE CustId = @CustID)

PRINT 'Copy Rate from saes to daes'
ALTER TABLE daes_AEPEnergy..Rate NOCHECK CONSTRAINT ALL
SET IDENTITY_INSERT daes_AEPEnergy..Rate ON
INSERT INTO daes_AEPEnergy..Rate
 (RateID,CSPID,RateCode,RateDesc,EffectiveDate,ExpirationDate,RateType,PlanType,IsMajority,TemplateFlag,LDCCode,CreateDate,UserID,RatePackageName,CustType,ServiceType,DivisionCode,ConsUnitId,ActiveFlag,LDCRateCode,migr_plan_id,migr_custid)
SELECT
 RateID,CSPID,RateCode,RateDesc,EffectiveDate,ExpirationDate,RateType,PlanType,IsMajority,TemplateFlag,LDCCode,CreateDate,UserID,RatePackageName,CustType,ServiceType,DivisionCode,ConsUnitId,ActiveFlag,LDCRateCode,migr_plan_id,migr_custid
FROM saes_AEPEnergy..Rate r
WHERE
 RateID in (
 SELECT RateId FROM saes_AEPEnergy..Customer WHERE CustId = @CustId
 UNION
 SELECT RateId FROM saes_AEPEnergy..RateTransition WHERE CustId = @CustId
 )
 AND (NOT EXISTS (SELECT 1 FROM daes_AEPEnergy..Rate WHERE RateId = r.RateId))

SET IDENTITY_INSERT daes_AEPEnergy..Rate OFF
PRINT 'Enabling constrains on Rate'
ALTER TABLE daes_AEPEnergy..Rate WITH CHECK CHECK CONSTRAINT ALL


PRINT 'Copy RateTransition from saes to daes'
ALTER TABLE daes_AEPEnergy..RateTransition NOCHECK CONSTRAINT ALL
SET IDENTITY_INSERT daes_AEPEnergy..RateTransition ON
INSERT INTO daes_AEPEnergy..RateTransition
 (RateTransitionID,CustID,RateID,UserID,CreatedDate,SwitchDate,EndDate,StatusID,SoldDate,RolloverFlag)
SELECT
 RateTransitionID,CustID,RateID,UserID,CreatedDate,SwitchDate,DATEADD(YEAR,1,SwitchDate)/*EndDate*/,StatusID,SoldDate,RolloverFlag
FROM saes_AEPEnergy..RateTransition rt
WHERE
 RateTransitionID in (
 SELECT RateTransitionID FROM saes_AEPEnergy..RateTransition WHERE CustId = @CustId
 UNION
 SELECT RateTransitionID FROM saes_AEPEnergy..RateTransition WHERE RateId = (SELECT RateId FROM saes_AEPEnergy..Customer WHERE CustId = @CustId)
 )
 AND RollOverFlag = 0
 AND (NOT EXISTS (SELECT 1 FROM daes_AEPEnergy..RateTransition WHERE RateTransitionID = rt.RateTransitionID))

SET IDENTITY_INSERT daes_AEPEnergy..RateTransition OFF
PRINT 'Enabling constrains on RateTransition'
ALTER TABLE daes_AEPEnergy..RateTransition WITH CHECK CHECK CONSTRAINT ALL

PRINT 'Copy RateDetail from saes to daes'
ALTER TABLE daes_AEPEnergy..RateDetail NOCHECK CONSTRAINT ALL
SET IDENTITY_INSERT daes_AEPEnergy..RateDetail ON
INSERT INTO daes_AEPEnergy..RateDetail
 (RateDetID,RateID,CategoryID,RateTypeID,ConsUnitID,RateDescID,EffectiveDate,ExpirationDate,RateAmt,RateAmt2,RateAmt3,FixedAdder,MinDetAmt,MaxDetAmt,GLAcct,RangeLower,RangeUpper,CustType,Graduated,Progressive,AmountCap,MaxRateAmt,MinRateAmt,CategoryRollup,Taxable,ChargeType,MiscData1,FixedCapRate,ScaleFactor1,ScaleFactor2,TemplateRateDetID,Margin,HALRateDetailId,UsageClassId,LegacyRateDetailId,Building,ServiceTypeID,TaxCategoryID,UtilityID,UtilityInvoiceTemplateDetailID,Active,StatusID,RateVariableTypeId,MinDays,MaxDays,BlockPriceIndicator,RateTransitionId,CreateDate,MeterMultiplierFlag,BlendRatio,ContractVolumeID,CreatedByUserId,ModifiedByUserId,ModifiedDate,TOUTemplateID,TOUTemplateRegisterID,TOUTemplateRegisterName)
SELECT
 RateDetID,RateID,CategoryID,RateTypeID,ConsUnitID,RateDescID,EffectiveDate,IIF(ExpirationDate IS NULL, NULL, DATEADD(YEAR,1,EffectiveDate)),RateAmt,RateAmt2,RateAmt3,FixedAdder,MinDetAmt,MaxDetAmt,GLAcct,RangeLower,RangeUpper,CustType,Graduated,Progressive,AmountCap,MaxRateAmt,MinRateAmt,CategoryRollup,Taxable,ChargeType,MiscData1,FixedCapRate,ScaleFactor1,ScaleFactor2,TemplateRateDetID,Margin,HALRateDetailId,UsageClassId,LegacyRateDetailId,Building,ServiceTypeID,TaxCategoryID,UtilityID,UtilityInvoiceTemplateDetailID,Active,StatusID,RateVariableTypeId,MinDays,MaxDays,BlockPriceIndicator,RateTransitionId,CreateDate,MeterMultiplierFlag,BlendRatio,ContractVolumeID,CreatedByUserId,ModifiedByUserId,ModifiedDate,TOUTemplateID,TOUTemplateRegisterID,TOUTemplateRegisterName
FROM saes_AEPEnergy..RateDetail rdet
WHERE
 RateDetId in (
 SELECT RateDetId FROM saes_AEPEnergy..RateDetail WHERE
  RateId = (SELECT RateId FROM saes_AEPEnergy..Customer WHERE CustId = @CustId) 
  AND RateTransitionId IN (SELECT RateTransitionId from saes_AEPEnergy..RateTransition WHERE CustId = @CustId AND RolloverFlag = 0)
 UNION
 SELECT RateDetId FROM saes_AEPEnergy..RateDetail WHERE RateId IN (SELECT RateId FROM saes_AEPEnergy..RateTransition WHERE CustId = @CustId)
 )
 AND (NOT EXISTS (SELECT 1 FROM daes_AEPEnergy..RateDetail WHERE RateDetID = rdet.RateDetID))

SET IDENTITY_INSERT daes_AEPEnergy..RateDetail OFF
PRINT 'Enabling constraings on RateDetails'
ALTER TABLE daes_AEPEnergy..RateDetail WITH CHECK CHECK CONSTRAINT ALL

PRINT 'Copy Products from saes to daes'

ALTER TABLE daes_AEPEnergy..Product NOCHECK CONSTRAINT ALL
SET IDENTITY_INSERT daes_AEPEnergy..Product ON
INSERT INTO daes_AEPEnergy..Product
 ([ProductID], [RateID], [LDCCode], [PlanType], [TDSPTemplateID], [Description], [BeginDate], [EndDate], [CustType], [Graduated], [RangeTier1], [RangeTier2], [SortOrder], [ActiveFlag], [Uplift], [CSATDSPTemplateID], [CAATDSPTemplateID], [PriceDescription], [MarketingCode], [RateTypeID], [ConsUnitID], [Default], [DivisionCode], [RateDescription], [ServiceType], [CSPId], [TermsId], [RolloverProductId], [CommissionId], [CommissionAmt], [CancelFeeId], [MonthlyChargeId], [ProductCode], [RatePackageId], [ProductName], [TermDate], [DiscountTypeId], [DiscountAmount], [ProductZoneID], [IsGreen], [IsBestChoice], [ActiveEnrollmentFlag], [CreditScoreThreshold], [DepositAmount], [Incentives])
SELECT
  [ProductID], [RateID], [LDCCode], [PlanType], [TDSPTemplateID], [Description], [BeginDate], [EndDate], [CustType], [Graduated], [RangeTier1], [RangeTier2], [SortOrder], [ActiveFlag], [Uplift], [CSATDSPTemplateID], [CAATDSPTemplateID], [PriceDescription], [MarketingCode], [RateTypeID], [ConsUnitID], [Default], [DivisionCode], [RateDescription], [ServiceType], [CSPId], [TermsId], [RolloverProductId], [CommissionId], [CommissionAmt], [CancelFeeId], [MonthlyChargeId], [ProductCode], [RatePackageId], [ProductName], [TermDate], [DiscountTypeId], [DiscountAmount], [ProductZoneID], [IsGreen], [IsBestChoice], [ActiveEnrollmentFlag], [CreditScoreThreshold], [DepositAmount], [Incentives]
FROM saes_AEPEnergy..Product pr
WHERE
 RateId in (
 SELECT RateId FROM saes_AEPEnergy..Customer WHERE CustId = @CustId
 UNION
 SELECT RateId FROM saes_AEPEnergy..RateTransition WHERE CustId = @CustId
 )
 AND (NOT EXISTS (SELECT 1 FROM daes_AEPEnergy..Product WHERE ProductId = pr.ProductId))
SET IDENTITY_INSERT daes_AEPEnergy..Product OFF

PRINT 'Enabling constrains on Product'
ALTER TABLE daes_AEPEnergy..Product WITH CHECK CHECK CONSTRAINT ALL

PRINT 'Copy EnrollCustomer'
ALTER TABLE daes_AEPEnergy..EnrollCustomer NOCHECK CONSTRAINT ALL
SET IDENTITY_INSERT daes_AEPEnergy..EnrollCustomer ON
INSERT INTO daes_AEPEnergy..EnrollCustomer
 ( [EnrollCustID], [EnrollMasterCustID], [ProductID], [PropertyID], [RateID], [CustCategory], [CustType], [CustName], [LastName], [FirstName], [MiddleName], [Salutation], [DBA], [TaxID], [CreditScore], [BillingContact], [BillingAddress1], [BillingAddress2], [BillingCity], [BillingState], [BillingZip], [BillingPhone], [BillingHomePhone], [BillingEmail], [AccountManager], [Taxable], [LifeSupport], [SpanishBill], [AuthorizedContact], [AuthorizedPhone], [DigitalSignature], [SalesChannel], [SalesStationID], [SalesUserID], [SalesCode], [VerifyStationID], [VerifyUserID], [VerifyCode], [Status], [Comments], [RejectReason], [RejectComments], [EnrollmentSentFlag], [EnrollmentSentDate], [CreateUserID], [CreateDate], [SuspendDate], [eBillImportDate], [CsrCustID], [NotificationWaiver], [PermitContactName], [TDSPTemplateID], [LidaDiscount], [CsaFlag], [DoNotDisconnect], [ContractID], [Service], [Master], [CsrImportDate], [CustomerAccountNumber], [ContractEndDate], [ContractTerms], [ContractStartDate], [UtilityAccountNumber], [GasRateID], [BillingTypeID], [Qeze], [DivisionID], [DivisionCode], [HasLocationMasterFlag], [ConversionAccountNO], [PlanTypeID], [LateFeeGracePeriod], [SupplierPricingStructure], [BillingFax], [InvoiceDeliveryTypeID], [TaxExemptionTypeID], [TaxExemptionNumber], [TaxExemptionCertificateOnFileFlag], [Zip4], [PrimaryPhone], [SalesSourceID], [IndependentAgentID], [QCDateTimeStamp], [EnrollStatusID], [Last4SSN], [CallEmailAuthFlag], [ReferralFlag], [SwitchAuthFlag], [TermsDisclosureFlag], [ReferralDetails], [ReachOutStep], [AlternatePhone], [StreamTrackingNumber], [CRMNumber], [CompanyName], [EnrollmentId], [PORFlag], [MeterAccessNote], [ExperianConfirmationNumber], [CurrentProviderID], [ReferralName], [ModifiedDate], [ModifiedBy], [Title], [PreferredSalesExecutive], [SSN], [AutoPayBill], [ServiceAccount], [ServiceState], [DepositAmount], [IsWelcomePacketRequired], [SalesAgent], [Broker], [PromoCode], [MasterAccountNumber], [CampaignName], [AccessDBID], [CommissionType], [CommissionAmount], [ReferralID], [RPNumber], [TCPAAuthorization], [CancellationFee], [MunicipalAggregation])
SELECT
   [EnrollCustID], [EnrollMasterCustID], [ProductID], [PropertyID], [RateID], [CustCategory], [CustType], [CustName], [LastName], [FirstName], [MiddleName], [Salutation], [DBA], [TaxID], [CreditScore], [BillingContact], [BillingAddress1], [BillingAddress2], [BillingCity], [BillingState], [BillingZip], [BillingPhone], [BillingHomePhone], [BillingEmail], [AccountManager], [Taxable], [LifeSupport], [SpanishBill], [AuthorizedContact], [AuthorizedPhone], [DigitalSignature], [SalesChannel], [SalesStationID], [SalesUserID], [SalesCode], [VerifyStationID], [VerifyUserID], [VerifyCode], [Status], [Comments], [RejectReason], [RejectComments], [EnrollmentSentFlag], [EnrollmentSentDate], [CreateUserID], [CreateDate], [SuspendDate], [eBillImportDate], [CsrCustID], [NotificationWaiver], [PermitContactName], [TDSPTemplateID], [LidaDiscount], [CsaFlag], [DoNotDisconnect], [ContractID], [Service], [Master], [CsrImportDate], [CustomerAccountNumber], [ContractEndDate], [ContractTerms], [ContractStartDate], [UtilityAccountNumber], [GasRateID], [BillingTypeID], [Qeze], [DivisionID], [DivisionCode], [HasLocationMasterFlag], [ConversionAccountNO], [PlanTypeID], [LateFeeGracePeriod], [SupplierPricingStructure], [BillingFax], [InvoiceDeliveryTypeID], [TaxExemptionTypeID], [TaxExemptionNumber], [TaxExemptionCertificateOnFileFlag], [Zip4], [PrimaryPhone], [SalesSourceID], [IndependentAgentID], [QCDateTimeStamp], [EnrollStatusID], [Last4SSN], [CallEmailAuthFlag], [ReferralFlag], [SwitchAuthFlag], [TermsDisclosureFlag], [ReferralDetails], [ReachOutStep], [AlternatePhone], [StreamTrackingNumber], [CRMNumber], [CompanyName], [EnrollmentId], [PORFlag], [MeterAccessNote], [ExperianConfirmationNumber], [CurrentProviderID], [ReferralName], [ModifiedDate], [ModifiedBy], [Title], [PreferredSalesExecutive], [SSN], [AutoPayBill], [ServiceAccount], [ServiceState], [DepositAmount], [IsWelcomePacketRequired], [SalesAgent], [Broker], [PromoCode], [MasterAccountNumber], [CampaignName], [AccessDBID], [CommissionType], [CommissionAmount], [ReferralID], [RPNumber], [TCPAAuthorization], [CancellationFee], [MunicipalAggregation]
FROM saes_AEPEnergy..EnrollCustomer ec
WHERE
 CsrCustId = @CustId
 AND (NOT EXISTS (SELECT 1 FROM daes_AEPEnergy..EnrollCustomer WHERE EnrollCustId = ec.EnrollCustId))
SET IDENTITY_INSERT daes_AEPEnergy..EnrollCustomer OFF

PRINT 'Enabling constrains on EnrollCustomer'
ALTER TABLE daes_AEPEnergy..EnrollCustomer WITH CHECK CHECK CONSTRAINT ALL

";
            DB.ExecuteQuery(sql, _appConfig.ConnectionCsr);
        }
        
    }
}

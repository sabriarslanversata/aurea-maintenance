using System;
using System.Collections;
using Aurea.Maintenance.Debugger.Common;
using System.Data.SqlClient;
using System.Data;
using CIS.Clients.Spark.Common;
using CIS.Import;

namespace Aurea.Maintenance.Debugger.Spark
{
    public class Program
    {
        public class MyMaintenance : CIS.Clients.Spark.Maintenance
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
        private static CIS.BusinessEntity.GlobalApplicationConfigurationDS.GlobalApplicationConfiguration _clientConfiguration;

        public static void Main(string[] args)
        {
            _clientConfiguration = Utility.SetSecurity(Utility.BillingAdminDEV, Utility.Clients["SPK"]);
            // Set culture to en-EN to prevent string manipulation issues in base code
            Utility.SetThreadCulture("en-US");
            simulateAESCIS14129("002576466");
            //ProcessEvents();

            //PrepareMockDataForLetterGeneration();
            //GenerateEventsForLetterGeneration();
            //RestoreData2OriginalLetterGeneration();
        }

        private static void simulateAESCIS14129(string custNo)
        {
            CopyCustomer(custNo);
            ClearOldRecords(custNo);
            Create814DMarketMock(custNo, transactionDate:DateTime.Now.AddDays(-21), requestDate:DateTime.Now.AddDays(1));
            ImportTransaction();
            GenerateEventsFor814Market();
            ProcessEvents();
            
            Create814RMarketMock(custNo, transactionDate:DateTime.Now.AddDays(-14));
            ImportTransaction();
            GenerateEventsFor814Market();
            ProcessEvents();

            Change814DSchedule(scheduleDate: DateTime.Now.AddDays(-5));
            ProcessEvents();
        }

        
        private static void ImportTransaction()
        {
            var baseImport = new MyImport()
            {
                ConnectionAdmin = Utility.BillingAdminDEV,
                ConnectionMarket = _clientConfiguration.ConnectionMarket,
                ConnectionCsr = _clientConfiguration.ConnectionCsr,
                ClientID = Utility.Clients["SPK"],
                Client = "SPK"
                
            };
            baseImport.myImportTransaction();
        }

        private static void GenerateEventsFor814Market()
        {
            
            var htParams = new Hashtable() { { "EventTypeID", 10 } }; ;

            var gen = new CIS.Framework.Event.EventGenerator.SimpleMarketTransactionEvaluation(
                _clientConfiguration.ConnectionCsr, Utility.BillingAdminDEV)
            {
                ConnectionStringBillingAdmin = Utility.BillingAdminDEV,
                ConnectionStringCsr = _clientConfiguration.ConnectionCsr,
                Client = "SPK",
                ClientID = Utility.Clients["SPK"]
            };

            gen.Generate(Utility.Clients["SPK"], htParams);
            /*
            var maintenance = new MyMaintenance(_clientConfiguration.ConnectionCsr, _clientConfiguration.ConnectionMarket, Utility.BillingAdminDEV);
            //will create events for all configured eventtype on client
            maintenance.GenerateEvents();

            //will create event queue for CTR
            //maintenance.Sim();
            */
        }

        private static void GenerateEventsForLetterGeneration()
        {
            var maintenance = new MyMaintenance(_clientConfiguration.ConnectionCsr, _clientConfiguration.ConnectionMarket, Utility.BillingAdminDEV);
            //will create events for all configured eventtype on client
            //maintenance.GenerateEvents();

            //will create renewal letters
            maintenance.GenerateContractRenewalNoticeLetters();
            
            
            //will create email queue
            maintenance.QueueLettersForEmail();
        }

        private static void ProcessEvents()
        {
            var engine = new CIS.Engine.Event.Queue(Utility.BillingAdminDEV);
            engine.ProcessEventQueue(_clientConfiguration.ClientID, _clientConfiguration.ConnectionCsr, _clientConfiguration.ConnectionMarket, _clientConfiguration.ClientAbbreviation);
        }

        private static void PrepareMockDataForLetterGeneration()
        {
            DB.ExecuteQuery("UPDATE config.LDCConfiguration SET SendRenewalNoticeOne = 0 WHERE LDCId IN (SELECT LDCId FROM LDC WHERE MarketID = 1)");
            DB.ExecuteQuery("DELETE FROM Letter WHERE CreateDate > DateAdd(dd,-2,GETDATE()) ");
            DB.ExecuteQuery("UPDATE Premise SET LDCId = 11 WHERE CustID = 1865754");
            DB.ExecuteQuery("UPDATE config.LDCConfiguration SET QueueLettersForEmail = 1 WHERE LDCId = 11");
            DB.ExecuteQuery("UPDATE Customer SET ContractEndDate = '2016-12-30' WHERE CustID = 1865754");
            DB.ExecuteQuery("DELETE FROM Letter WHERE LetterTypeId = 701 AND CustID = 1865754");
            DB.ExecuteQuery("UPDATE Contract SET EndDate = '2016-12-31' WHERE CustID = 1865754");
            DB.ExecuteQuery("UPDATE CustomerAdditionalInfo SET BillingTypeId = 2 WHERE CustID = 1865754");
            DB.ExecuteQuery("UPDATE Address SET Email = 'test@example.com' WHERE AddrID IN (SELECT MailAddrId FROM Customer WHERE CustID = 1865754)");
        }

        private static void RestoreData2OriginalLetterGeneration()
        {
            DB.ExecuteQuery("UPDATE Address SET Email = NULL WHERE AddrID IN (SELECT MailAddrId FROM Customer WHERE CustID = 1865754)");
            DB.ExecuteQuery("UPDATE CustomerAdditionalInfo SET BillingTypeId = 1 WHERE CustID = 1865754");
            DB.ExecuteQuery("UPDATE Contract SET EndDate = '2017-11-30' WHERE CustID = 1865754");
            DB.ExecuteQuery("UPDATE Customer SET ContractEndDate = '2017-11-29' WHERE CustID = 1865754");
            DB.ExecuteQuery("UPDATE config.LDCConfiguration SET QueueLettersForEmail = 0 WHERE LDCId = 11");
            DB.ExecuteQuery("UPDATE Premise SET LDCId = 3 WHERE CustID = 1865754");
            DB.ExecuteQuery("UPDATE config.LDCConfiguration SET SendRenewalNoticeOne = 1 WHERE LDCId IN (SELECT LDCId FROM LDC WHERE MarketID = 1)");
        }

        private static void ClearOldRecords(string custNo)
        {
            string sql = $@"
PRINT 'Clean Old Transactions, Market Files and EventQueue'
DECLARE @CustNo AS VARCHAR(20) = '{custNo}'
DECLARE @CustID AS INT = (SELECT CustID FROM saes_Spark..Customer WHERE CustNo = @CustNo)
DECLARE @PremID AS INT = (SELECT PremId FROM saes_Spark..Premise WHERE CustId = @CustID)

DELETE FROM daes_SparkMarket..tbl_814_Service_Status WHERE Service_Key IN (22791310, 22916323)
DELETE FROM daes_SparkMarket..tbl_814_Service_Account_Change WHERE Service_Key IN (22791310, 22916323)
DELETE FROM daes_SparkMarket..tbl_814_Service_Date WHERE Service_Key IN (22791310, 22916323)
DELETE FROM daes_SparkMarket..tbl_814_Service_Reject WHERE Service_Key IN (22791310, 22916323)
DELETE FROM daes_SparkMarket..tbl_814_Service WHERE [814_Key] IN (22748557,  22873520)
DELETE FROM daes_SparkMarket..tbl_814_header WHERE [814_Key] IN (22748557,  22873520)
DELETE FROM daes_Spark..CustomerTransactionRequest WHERE SourceId IN (22748557,  22873520)

DELETE FROM daes_BillingAdmin..EventingQueue WHERE ClientId = 48 AND EventingQueueId IN (SELECT EventingQueueId FROM daes_BillingAdmin..EventActionQueue WHERE CustId = @CustId)
DELETE FROM daes_BillingAdmin..EventActionQueueParameter WHERE EventActionQueueId IN (SELECT EventActionQueueId FROM daes_BillingAdmin..EventActionQueue WHERE CustId = @CustId)
DELETE FROM daes_BillingAdmin..EventActionQueue WHERE CustId = @CustId

DELETE FROM daes_Spark..ChangeLogDetail WHERE ChangeLogID IN (SELECT ChangeLogID FROM daes_Spark..ChangeLog WHERE CustID = @CustId OR PremID = @PremID)

DELETE FROM daes_Spark..ChangeLog WHERE (CustID = @CustId OR PremID = @PremID)
UPDATE daes_Spark..Premise SET StatusId = 10 WHERE CustId = @CustId";
            try
            {
                DB.ExecuteQuery(sql);
            }
            catch 
            {
            }
            try
            {
                DB.ExecuteQuery(sql);
            }
            catch
            {
                
            }
        }

        private static void Create814DMarketMock(string custNo, DateTime transactionDate, DateTime requestDate)
        {
            string sql = $@"PRINT 'BEGIN Copy Market Header 814_D Files'
USE daes_SparkMarket
DECLARE @CustNo AS VARCHAR(20) = '{custNo}'
DECLARE @CustID AS INT = (SELECT CustID FROM saes_Spark..Customer WHERE CustNo = @CustNo)
SET IDENTITY_INSERT daes_SparkMarket..tbl_814_header ON
INSERT INTO daes_SparkMarket..tbl_814_header ([814_Key], [MarketFileId], [TransactionSetId], [TransactionSetControlNbr], [TransactionSetPurposeCode], [TransactionNbr], [TransactionDate], [ReferenceNbr], [ActionCode], [TdspDuns], [TdspName], [CrDuns], [CrName], [ProcessFlag], [ProcessDate], [Direction], [TransactionTypeID], [MarketID], [ProviderID], [POLRClass], [TransactionTime], [TransactionTimeCode], [TransactionQueueTypeID], [TransactionQualifier], [CreateDate])
SELECT  [814_Key], [MarketFileId], [TransactionSetId], [TransactionSetControlNbr], [TransactionSetPurposeCode], [TransactionNbr], '{transactionDate.ToString("yyyy-MM-dd")}', [ReferenceNbr], [ActionCode], [TdspDuns], [TdspName], [CrDuns], [CrName], 0/*[ProcessFlag]*/, NULL/*[ProcessDate]*/, [Direction], [TransactionTypeID], [MarketID], [ProviderID], [POLRClass], [TransactionTime], [TransactionTimeCode], [TransactionQueueTypeID], [TransactionQualifier], [CreateDate]
FROM   saes_SparkMarket..tbl_814_header sh 
WHERE TransactionSetId = '814' AND ActionCode = 'D' AND TransactionTypeId = 43 AND [814_Key] IN (SELECT SourceId FROM saes_Spark..CustomerTransactionRequest WHERE CustID = @CustID AND TransactionTypeId = 43)
AND NOT EXISTS(SELECT 1 FROM daes_SparkMarket..tbl_814_header dh WHERE sh.[814_Key] = dh.[814_Key] )
SET IDENTITY_INSERT daes_SparkMarket..tbl_814_header OFF

SET IDENTITY_INSERT daes_SparkMarket..tbl_814_Service ON
INSERT INTO daes_SparkMarket..tbl_814_Service ([Service_Key], [814_Key], [AssignId], [ServiceTypeCode1], [ServiceType1], [ServiceTypeCode2], [ServiceType2], [ServiceTypeCode3], [ServiceType3], [ServiceTypeCode4], [ServiceType4], [ActionCode], [MaintenanceTypeCode], [DistributionLossFactorCode], [PremiseType], [BillType], [BillCalculator], [EsiId], [StationId], [SpecialNeedsIndicator], [PowerRegion], [EnergizedFlag], [EsiIdStartDate], [EsiIdEndDate], [EsiIdEligibilityDate], [NotificationWaiver], [SpecialReadSwitchDate], [PriorityCode], [PermitIndicator], [RTODate], [RTOTime], [CSAFlag], [MembershipID], [ESPAccountNumber], [LDCBillingCycle], [LDCBudgetBillingCycle], [WaterHeaters], [LDCBudgetBillingStatus], [PaymentArrangement], [NextMeterReadDate], [ParticipatingInterest], [EligibleLoadPercentage], [TaxExemptionPercent], [CapacityObligation], [TransmissionObligation], [TotalKWHHistory], [NumberOfMonthsHistory], [PeakDemandHistory], [AirConditioners], [PreviousEsiId], [GasPoolId], [LBMPZone], [ResidentialTaxPortion], [ESPCommodityPrice], [ESPFixedCharge], [ESPChargesCommTaxRate], [ESPChargesResTaxRate], [GasSupplyServiceOption], [FundsAuthorization], [BudgetBillingStatus], [FixedMonthlyCharge], [TaxRate], [CommodityPrice], [MeterCycleCodeDesc], [BillCycleCodeDesc], [FeeApprovedApplied], [MarketerCustomerAccountNumber], [GasSupplyServiceOptionCode], [HumanNeeds], [ReinstatementDate], [MeterCycleCode], [SystemNumber], [StateLicenseNumber], [SupplementalAccountNumber], [NewCustomerIndicator], [PaymentCategory], [PreviousESPAccountNumber], [RenewableEnergyIndicator], [SICCode], [ApprovalCodeIndicator], [RenewableEnergyCertification], [NewPremiseIndicator], [SalesResponsibility], [CustomerReferenceNumber], [TransactionReferenceNumber], [ESPTransactionNumber], [OldESPAccountNumber], [DFIIdentificationNumber], [DFIAccountNumber], [DFIIndicator1], [DFIIndicator2], [DFIQualifier], [DFIRoutingNumber], [SpecialReadSwitchTime], [LDCAccountBalance], [DisputedAmount], [CurrentBalance], [ArrearsBalance], [LDCSupplierBalance], [BudgetPlan], [BudgetInstallment], [Deposit], [RemainingUtilBalanceBucket1], [RemainingUtilBalanceBucket2], [RemainingUtilBalanceBucket3], [RemainingUtilBalanceBucket4], [RemainingUtilBalanceBucket5], [RemainingUtilBalanceBucket6], [IntervalStatusType], [CustomerAuthorization], [UnmeteredAcct], [PaymentOption], [MaxDailyAmt], [MeterAccessNote], [SpecialNeedsExpirationDate], [SwitchHoldStatusIndicator], [SpecialMeterConfig], [MaximumGeneration], [ServiceDeliveryPoint], [IgnoreRescind], [GasCapacityAssignment], [CPAEnrollmentTypes], [DaysInArrears], [RU_Notes], [RD_SiteCharacterDate], [SupplierPricingStructureNr], [SupplierGroupNumber], [IndustrialClassificationCode], [UtilityTaxExemptStatus], [AccountSettlementIndicator], [NypaDiscountIndicator], [UtilityDiscount], [IcapEffectiveDate], [FutureIcapEffectiveDate], [FutureIcap], [ChangeCancellationFee], [CancellationFee], [MunicipalAggregation])
SELECT  [Service_Key], [814_Key], [AssignId], [ServiceTypeCode1], [ServiceType1], [ServiceTypeCode2], [ServiceType2], [ServiceTypeCode3], [ServiceType3], [ServiceTypeCode4], [ServiceType4], [ActionCode], [MaintenanceTypeCode], [DistributionLossFactorCode], [PremiseType], [BillType], [BillCalculator], [EsiId], [StationId], [SpecialNeedsIndicator], [PowerRegion], [EnergizedFlag], [EsiIdStartDate], [EsiIdEndDate], [EsiIdEligibilityDate], [NotificationWaiver], '{requestDate.ToString("yyyy-MM-dd")}', [PriorityCode], [PermitIndicator], [RTODate], [RTOTime], [CSAFlag], [MembershipID], [ESPAccountNumber], [LDCBillingCycle], [LDCBudgetBillingCycle], [WaterHeaters], [LDCBudgetBillingStatus], [PaymentArrangement], [NextMeterReadDate], [ParticipatingInterest], [EligibleLoadPercentage], [TaxExemptionPercent], [CapacityObligation], [TransmissionObligation], [TotalKWHHistory], [NumberOfMonthsHistory], [PeakDemandHistory], [AirConditioners], [PreviousEsiId], [GasPoolId], [LBMPZone], [ResidentialTaxPortion], [ESPCommodityPrice], [ESPFixedCharge], [ESPChargesCommTaxRate], [ESPChargesResTaxRate], [GasSupplyServiceOption], [FundsAuthorization], [BudgetBillingStatus], [FixedMonthlyCharge], [TaxRate], [CommodityPrice], [MeterCycleCodeDesc], [BillCycleCodeDesc], [FeeApprovedApplied], [MarketerCustomerAccountNumber], [GasSupplyServiceOptionCode], [HumanNeeds], [ReinstatementDate], [MeterCycleCode], [SystemNumber], [StateLicenseNumber], [SupplementalAccountNumber], [NewCustomerIndicator], [PaymentCategory], [PreviousESPAccountNumber], [RenewableEnergyIndicator], [SICCode], [ApprovalCodeIndicator], [RenewableEnergyCertification], [NewPremiseIndicator], [SalesResponsibility], [CustomerReferenceNumber], [TransactionReferenceNumber], [ESPTransactionNumber], [OldESPAccountNumber], [DFIIdentificationNumber], [DFIAccountNumber], [DFIIndicator1], [DFIIndicator2], [DFIQualifier], [DFIRoutingNumber], [SpecialReadSwitchTime], [LDCAccountBalance], [DisputedAmount], [CurrentBalance], [ArrearsBalance], [LDCSupplierBalance], [BudgetPlan], [BudgetInstallment], [Deposit], [RemainingUtilBalanceBucket1], [RemainingUtilBalanceBucket2], [RemainingUtilBalanceBucket3], [RemainingUtilBalanceBucket4], [RemainingUtilBalanceBucket5], [RemainingUtilBalanceBucket6], [IntervalStatusType], [CustomerAuthorization], [UnmeteredAcct], [PaymentOption], [MaxDailyAmt], [MeterAccessNote], [SpecialNeedsExpirationDate], [SwitchHoldStatusIndicator], [SpecialMeterConfig], [MaximumGeneration], [ServiceDeliveryPoint], [IgnoreRescind], [GasCapacityAssignment], [CPAEnrollmentTypes], [DaysInArrears], [RU_Notes], [RD_SiteCharacterDate], [SupplierPricingStructureNr], [SupplierGroupNumber], [IndustrialClassificationCode], [UtilityTaxExemptStatus], [AccountSettlementIndicator], [NypaDiscountIndicator], [UtilityDiscount], [IcapEffectiveDate], [FutureIcapEffectiveDate], [FutureIcap], [ChangeCancellationFee], [CancellationFee], [MunicipalAggregation]
FROM   saes_SparkMarket..tbl_814_Service ss
WHERE [814_Key] IN  (SELECT SourceId FROM saes_Spark..CustomerTransactionRequest WHERE CustID = @CustID AND TransactionTypeId = 43)
AND NOT EXISTS(SELECT 1 FROM daes_SparkMarket..tbl_814_Service ds WHERE ss.[814_Key] = ds.[814_Key] )
SET IDENTITY_INSERT daes_SparkMarket..tbl_814_Service OFF

SET IDENTITY_INSERT daes_SparkMarket..tbl_814_Service_Status ON
INSERT INTO daes_SparkMarket..tbl_814_Service_Status ([Status_Key], [Service_Key], [StatusCode], [StatusReason], [StatusType])
SELECT  [Status_Key], [Service_Key], [StatusCode], [StatusReason], [StatusType]
FROM   saes_SparkMarket..tbl_814_Service_Status ss
WHERE [Service_Key] IN  (SELECT Service_Key FROM daes_SparkMarket..tbl_814_Service WHERE [814_Key] IN (SELECT [814_Key] FROM saes_Spark..CustomerTransactionRequest WHERE CustID = @CustID AND TransactionTypeId = 43))
AND NOT EXISTS(SELECT 1 FROM daes_SparkMarket..tbl_814_Service_Status dss WHERE ss.[Service_Key] = dss.[Service_Key] )
SET IDENTITY_INSERT daes_SparkMarket..tbl_814_Service_Status OFF

SET IDENTITY_INSERT daes_SparkMarket..tbl_814_Service_Date ON
INSERT INTO daes_SparkMarket..tbl_814_Service_Date ([Date_Key], [Service_Key], [Qualifier], [Date], [Time], [TimeCode], [PeriodFormat], [Period], [NotesDate])
SELECT  [Date_Key], [Service_Key], [Qualifier], [Date], [Time], [TimeCode], [PeriodFormat], [Period], [NotesDate]
FROM   saes_SparkMarket..tbl_814_Service_Date ss
WHERE [Service_Key] IN (SELECT Service_Key FROM daes_SparkMarket..tbl_814_Service WHERE [814_Key] IN (SELECT [814_Key] FROM saes_Spark..CustomerTransactionRequest WHERE CustID = @CustID AND TransactionTypeId = 43))
AND NOT EXISTS(SELECT 1 FROM daes_SparkMarket..tbl_814_Service_Date dss WHERE ss.[Service_Key] = dss.[Service_Key] )
SET IDENTITY_INSERT daes_SparkMarket..tbl_814_Service_Date OFF";
            DB.ExecuteQuery(sql);
        }

        private static void Create814RMarketMock(string custNo, DateTime transactionDate)
        {
            string sql = $@"PRINT 'BEGIN Copy Market Header 814_R Files'
USE daes_SparkMarket
DECLARE @CustNo AS VARCHAR(20) = '{custNo}'
DECLARE @CustID AS INT = (SELECT CustID FROM saes_Spark..Customer WHERE CustNo = @CustNo)
SET IDENTITY_INSERT daes_SparkMarket..tbl_814_header ON
INSERT INTO daes_SparkMarket..tbl_814_header ([814_Key], [MarketFileId], [TransactionSetId], [TransactionSetControlNbr], [TransactionSetPurposeCode], [TransactionNbr], [TransactionDate], [ReferenceNbr], [ActionCode], [TdspDuns], [TdspName], [CrDuns], [CrName], [ProcessFlag], [ProcessDate], [Direction], [TransactionTypeID], [MarketID], [ProviderID], [POLRClass], [TransactionTime], [TransactionTimeCode], [TransactionQueueTypeID], [TransactionQualifier], [CreateDate])
SELECT  [814_Key], [MarketFileId], [TransactionSetId], [TransactionSetControlNbr], [TransactionSetPurposeCode], [TransactionNbr], '{transactionDate.ToString("yyyy-MM-dd")}', [ReferenceNbr], [ActionCode], [TdspDuns], [TdspName], [CrDuns], [CrName], 0/*[ProcessFlag]*/, NULL/*[ProcessDate]*/, [Direction], [TransactionTypeID], [MarketID], [ProviderID], [POLRClass], [TransactionTime], [TransactionTimeCode], [TransactionQueueTypeID], [TransactionQualifier], [CreateDate]
FROM   saes_SparkMarket..tbl_814_header sh 
WHERE TransactionSetId = '814' AND ActionCode = 'R' AND TransactionTypeId = 41 AND [814_Key] IN (SELECT SourceId FROM saes_Spark..CustomerTransactionRequest WHERE CustID = @CustID AND TransactionTypeId = 41)
AND NOT EXISTS(SELECT 1 FROM daes_SparkMarket..tbl_814_header dh WHERE sh.[814_Key] = dh.[814_Key] )
SET IDENTITY_INSERT daes_SparkMarket..tbl_814_header OFF

SET IDENTITY_INSERT daes_SparkMarket..tbl_814_Service ON
INSERT INTO daes_SparkMarket..tbl_814_Service ([Service_Key], [814_Key], [AssignId], [ServiceTypeCode1], [ServiceType1], [ServiceTypeCode2], [ServiceType2], [ServiceTypeCode3], [ServiceType3], [ServiceTypeCode4], [ServiceType4], [ActionCode], [MaintenanceTypeCode], [DistributionLossFactorCode], [PremiseType], [BillType], [BillCalculator], [EsiId], [StationId], [SpecialNeedsIndicator], [PowerRegion], [EnergizedFlag], [EsiIdStartDate], [EsiIdEndDate], [EsiIdEligibilityDate], [NotificationWaiver], [SpecialReadSwitchDate], [PriorityCode], [PermitIndicator], [RTODate], [RTOTime], [CSAFlag], [MembershipID], [ESPAccountNumber], [LDCBillingCycle], [LDCBudgetBillingCycle], [WaterHeaters], [LDCBudgetBillingStatus], [PaymentArrangement], [NextMeterReadDate], [ParticipatingInterest], [EligibleLoadPercentage], [TaxExemptionPercent], [CapacityObligation], [TransmissionObligation], [TotalKWHHistory], [NumberOfMonthsHistory], [PeakDemandHistory], [AirConditioners], [PreviousEsiId], [GasPoolId], [LBMPZone], [ResidentialTaxPortion], [ESPCommodityPrice], [ESPFixedCharge], [ESPChargesCommTaxRate], [ESPChargesResTaxRate], [GasSupplyServiceOption], [FundsAuthorization], [BudgetBillingStatus], [FixedMonthlyCharge], [TaxRate], [CommodityPrice], [MeterCycleCodeDesc], [BillCycleCodeDesc], [FeeApprovedApplied], [MarketerCustomerAccountNumber], [GasSupplyServiceOptionCode], [HumanNeeds], [ReinstatementDate], [MeterCycleCode], [SystemNumber], [StateLicenseNumber], [SupplementalAccountNumber], [NewCustomerIndicator], [PaymentCategory], [PreviousESPAccountNumber], [RenewableEnergyIndicator], [SICCode], [ApprovalCodeIndicator], [RenewableEnergyCertification], [NewPremiseIndicator], [SalesResponsibility], [CustomerReferenceNumber], [TransactionReferenceNumber], [ESPTransactionNumber], [OldESPAccountNumber], [DFIIdentificationNumber], [DFIAccountNumber], [DFIIndicator1], [DFIIndicator2], [DFIQualifier], [DFIRoutingNumber], [SpecialReadSwitchTime], [LDCAccountBalance], [DisputedAmount], [CurrentBalance], [ArrearsBalance], [LDCSupplierBalance], [BudgetPlan], [BudgetInstallment], [Deposit], [RemainingUtilBalanceBucket1], [RemainingUtilBalanceBucket2], [RemainingUtilBalanceBucket3], [RemainingUtilBalanceBucket4], [RemainingUtilBalanceBucket5], [RemainingUtilBalanceBucket6], [IntervalStatusType], [CustomerAuthorization], [UnmeteredAcct], [PaymentOption], [MaxDailyAmt], [MeterAccessNote], [SpecialNeedsExpirationDate], [SwitchHoldStatusIndicator], [SpecialMeterConfig], [MaximumGeneration], [ServiceDeliveryPoint], [IgnoreRescind], [GasCapacityAssignment], [CPAEnrollmentTypes], [DaysInArrears], [RU_Notes], [RD_SiteCharacterDate], [SupplierPricingStructureNr], [SupplierGroupNumber], [IndustrialClassificationCode], [UtilityTaxExemptStatus], [AccountSettlementIndicator], [NypaDiscountIndicator], [UtilityDiscount], [IcapEffectiveDate], [FutureIcapEffectiveDate], [FutureIcap], [ChangeCancellationFee], [CancellationFee], [MunicipalAggregation])
SELECT  [Service_Key], [814_Key], [AssignId], [ServiceTypeCode1], [ServiceType1], [ServiceTypeCode2], [ServiceType2], [ServiceTypeCode3], [ServiceType3], [ServiceTypeCode4], [ServiceType4], [ActionCode], [MaintenanceTypeCode], [DistributionLossFactorCode], [PremiseType], [BillType], [BillCalculator], [EsiId], [StationId], [SpecialNeedsIndicator], [PowerRegion], [EnergizedFlag], [EsiIdStartDate], [EsiIdEndDate], [EsiIdEligibilityDate], [NotificationWaiver], [SpecialReadSwitchDate], [PriorityCode], [PermitIndicator], [RTODate], [RTOTime], [CSAFlag], [MembershipID], [ESPAccountNumber], [LDCBillingCycle], [LDCBudgetBillingCycle], [WaterHeaters], [LDCBudgetBillingStatus], [PaymentArrangement], [NextMeterReadDate], [ParticipatingInterest], [EligibleLoadPercentage], [TaxExemptionPercent], [CapacityObligation], [TransmissionObligation], [TotalKWHHistory], [NumberOfMonthsHistory], [PeakDemandHistory], [AirConditioners], [PreviousEsiId], [GasPoolId], [LBMPZone], [ResidentialTaxPortion], [ESPCommodityPrice], [ESPFixedCharge], [ESPChargesCommTaxRate], [ESPChargesResTaxRate], [GasSupplyServiceOption], [FundsAuthorization], [BudgetBillingStatus], [FixedMonthlyCharge], [TaxRate], [CommodityPrice], [MeterCycleCodeDesc], [BillCycleCodeDesc], [FeeApprovedApplied], [MarketerCustomerAccountNumber], [GasSupplyServiceOptionCode], [HumanNeeds], [ReinstatementDate], [MeterCycleCode], [SystemNumber], [StateLicenseNumber], [SupplementalAccountNumber], [NewCustomerIndicator], [PaymentCategory], [PreviousESPAccountNumber], [RenewableEnergyIndicator], [SICCode], [ApprovalCodeIndicator], [RenewableEnergyCertification], [NewPremiseIndicator], [SalesResponsibility], [CustomerReferenceNumber], [TransactionReferenceNumber], [ESPTransactionNumber], [OldESPAccountNumber], [DFIIdentificationNumber], [DFIAccountNumber], [DFIIndicator1], [DFIIndicator2], [DFIQualifier], [DFIRoutingNumber], [SpecialReadSwitchTime], [LDCAccountBalance], [DisputedAmount], [CurrentBalance], [ArrearsBalance], [LDCSupplierBalance], [BudgetPlan], [BudgetInstallment], [Deposit], [RemainingUtilBalanceBucket1], [RemainingUtilBalanceBucket2], [RemainingUtilBalanceBucket3], [RemainingUtilBalanceBucket4], [RemainingUtilBalanceBucket5], [RemainingUtilBalanceBucket6], [IntervalStatusType], [CustomerAuthorization], [UnmeteredAcct], [PaymentOption], [MaxDailyAmt], [MeterAccessNote], [SpecialNeedsExpirationDate], [SwitchHoldStatusIndicator], [SpecialMeterConfig], [MaximumGeneration], [ServiceDeliveryPoint], [IgnoreRescind], [GasCapacityAssignment], [CPAEnrollmentTypes], [DaysInArrears], [RU_Notes], [RD_SiteCharacterDate], [SupplierPricingStructureNr], [SupplierGroupNumber], [IndustrialClassificationCode], [UtilityTaxExemptStatus], [AccountSettlementIndicator], [NypaDiscountIndicator], [UtilityDiscount], [IcapEffectiveDate], [FutureIcapEffectiveDate], [FutureIcap], [ChangeCancellationFee], [CancellationFee], [MunicipalAggregation]
FROM   saes_SparkMarket..tbl_814_Service ss
WHERE [814_Key] IN  (SELECT SourceId FROM saes_Spark..CustomerTransactionRequest WHERE CustID = @CustID AND TransactionTypeId = 41)
AND NOT EXISTS(SELECT 1 FROM daes_SparkMarket..tbl_814_Service ds WHERE ss.[814_Key] = ds.[814_Key] )
SET IDENTITY_INSERT daes_SparkMarket..tbl_814_Service OFF

SET IDENTITY_INSERT daes_SparkMarket..tbl_814_Service_Date ON
INSERT INTO daes_SparkMarket..tbl_814_Service_Date ([Date_Key], [Service_Key], [Qualifier], [Date], [Time], [TimeCode], [PeriodFormat], [Period], [NotesDate])
SELECT  [Date_Key], [Service_Key], [Qualifier], [Date], [Time], [TimeCode], [PeriodFormat], [Period], [NotesDate]
FROM   saes_SparkMarket..tbl_814_Service_Date ss
WHERE [Service_Key] IN (SELECT Service_Key FROM daes_SparkMarket..tbl_814_Service WHERE [814_Key] IN (SELECT [814_Key] FROM saes_Spark..CustomerTransactionRequest WHERE CustID = @CustID AND TransactionTypeId = 41))
AND NOT EXISTS(SELECT 1 FROM daes_SparkMarket..tbl_814_Service_Date dss WHERE ss.[Service_Key] = dss.[Service_Key] )
SET IDENTITY_INSERT daes_SparkMarket..tbl_814_Service_Date OFF";
            DB.ExecuteQuery(sql);
        }

        private static void CopyCustomer(string custNo)
        {
            string sql = $@"USE daes_Spark
DECLARE @ClientID AS INT = (SELECT ClientID FROM daes_BillingAdmin..Client WHERE Client='SPK')
DECLARE @CustNo AS VARCHAR(20) = '{custNo}'
DECLARE @CustID AS INT = (SELECT CustID FROM saes_Spark..Customer WHERE CustNo = @CustNo)

PRINT 'BEGIN Copy Customer'

PRINT 'Copy Addresses'
SET IDENTITY_INSERT daes_Spark..Address ON

INSERT INTO daes_Spark..Address ([AddrID], [ValidationStatusID], [AttnMS], [Addr1], [Addr2], [City], [State], [Zip], [Zip4], [DPBC], [CityID], [CountyID], [County], [HomePhone], [WorkPhone], [FaxPhone], [OtherPhone], [Email], [ESIID], [GeoCode], [Status], [DeliveryPointCode], [CreateDate], [Migr_Enrollid], [PhoneExtension], [OtherExtension], [FaxExtension], [TaxingDistrict], [TaxInCity], [CchVersion])
SELECT [AddrID], [ValidationStatusID], [AttnMS], [Addr1], [Addr2], [City], [State], [Zip], [Zip4], [DPBC], [CityID], [CountyID], [County], [HomePhone], [WorkPhone], [FaxPhone], [OtherPhone], [Email], [ESIID], [GeoCode], [Status], [DeliveryPointCode], [CreateDate], [Migr_Enrollid], [PhoneExtension], [OtherExtension], [FaxExtension], [TaxingDistrict], [TaxInCity], [CchVersion] 
FROM  saes_Spark..Address WHERE AddrID IN (
	SELECT SiteAddrId FROM saes_Spark..Customer WHERE CustID = @CustID
)
AND NOT EXISTS (
SELECT 1 FROM daes_Spark..Address WHERE AddrID IN (
	SELECT SiteAddrId FROM saes_Spark..Customer WHERE CustID = @CustID
	)
)

INSERT INTO daes_Spark..Address ([AddrID], [ValidationStatusID], [AttnMS], [Addr1], [Addr2], [City], [State], [Zip], [Zip4], [DPBC], [CityID], [CountyID], [County], [HomePhone], [WorkPhone], [FaxPhone], [OtherPhone], [Email], [ESIID], [GeoCode], [Status], [DeliveryPointCode], [CreateDate], [Migr_Enrollid], [PhoneExtension], [OtherExtension], [FaxExtension], [TaxingDistrict], [TaxInCity], [CchVersion])
SELECT [AddrID], [ValidationStatusID], [AttnMS], [Addr1], [Addr2], [City], [State], [Zip], [Zip4], [DPBC], [CityID], [CountyID], [County], [HomePhone], [WorkPhone], [FaxPhone], [OtherPhone], [Email], [ESIID], [GeoCode], [Status], [DeliveryPointCode], [CreateDate], [Migr_Enrollid], [PhoneExtension], [OtherExtension], [FaxExtension], [TaxingDistrict], [TaxInCity], [CchVersion] 
FROM  saes_Spark..Address WHERE AddrID IN (
	SELECT MailAddrId FROM saes_Spark..Customer WHERE CustID = @CustID
)
AND NOT EXISTS (
SELECT 1 FROM daes_Spark..Address WHERE AddrID IN (
	SELECT MailAddrId FROM saes_Spark..Customer WHERE CustID = @CustID
	)
)

INSERT INTO daes_Spark..Address ([AddrID], [ValidationStatusID], [AttnMS], [Addr1], [Addr2], [City], [State], [Zip], [Zip4], [DPBC], [CityID], [CountyID], [County], [HomePhone], [WorkPhone], [FaxPhone], [OtherPhone], [Email], [ESIID], [GeoCode], [Status], [DeliveryPointCode], [CreateDate], [Migr_Enrollid], [PhoneExtension], [OtherExtension], [FaxExtension], [TaxingDistrict], [TaxInCity], [CchVersion])
SELECT [AddrID], [ValidationStatusID], [AttnMS], [Addr1], [Addr2], [City], [State], [Zip], [Zip4], [DPBC], [CityID], [CountyID], [County], [HomePhone], [WorkPhone], [FaxPhone], [OtherPhone], [Email], [ESIID], [GeoCode], [Status], [DeliveryPointCode], [CreateDate], [Migr_Enrollid], [PhoneExtension], [OtherExtension], [FaxExtension], [TaxingDistrict], [TaxInCity], [CchVersion] 
FROM  saes_Spark..Address WHERE AddrID IN (
	SELECT CorrAddrId FROM saes_Spark..Customer WHERE CustID = @CustID
)
AND NOT EXISTS (
SELECT 1 FROM daes_Spark..Address WHERE AddrID IN (
	SELECT CorrAddrId FROM saes_Spark..Customer WHERE CustID = @CustID
	)
)

INSERT INTO daes_Spark..Address ([AddrID], [ValidationStatusID], [AttnMS], [Addr1], [Addr2], [City], [State], [Zip], [Zip4], [DPBC], [CityID], [CountyID], [County], [HomePhone], [WorkPhone], [FaxPhone], [OtherPhone], [Email], [ESIID], [GeoCode], [Status], [DeliveryPointCode], [CreateDate], [Migr_Enrollid], [PhoneExtension], [OtherExtension], [FaxExtension], [TaxingDistrict], [TaxInCity], [CchVersion])
SELECT [AddrID], [ValidationStatusID], [AttnMS], [Addr1], [Addr2], [City], [State], [Zip], [Zip4], [DPBC], [CityID], [CountyID], [County], [HomePhone], [WorkPhone], [FaxPhone], [OtherPhone], [Email], [ESIID], [GeoCode], [Status], [DeliveryPointCode], [CreateDate], [Migr_Enrollid], [PhoneExtension], [OtherExtension], [FaxExtension], [TaxingDistrict], [TaxInCity], [CchVersion] 
FROM  saes_Spark..Address WHERE AddrID IN (
	SELECT AddrId FROM saes_Spark..Premise WHERE CustID = @CustID
)
AND NOT EXISTS (
SELECT 1 FROM daes_Spark..Address WHERE AddrID IN (
	SELECT AddrId FROM saes_Spark..Premise WHERE CustID = @CustID
	)
)

SET IDENTITY_INSERT daes_Spark..Address OFF

PRINT 'Copy Rate'
SET IDENTITY_INSERT daes_Spark..Rate ON

INSERT INTO daes_Spark..Rate ([RateID], [CSPID], [RateCode], [RateDesc], [EffectiveDate], [ExpirationDate], [RateType], [PlanType], [IsMajority], [TemplateFlag], [LDCCode], [CreateDate], [UserID], [RatePackageName], [CustType], [ServiceType], [DivisionCode], [ConsUnitId], [ActiveFlag], [LDCRateCode], [migr_plan_id], [migr_custid])
SELECT  [RateID], [CSPID], [RateCode], [RateDesc], [EffectiveDate], [ExpirationDate], [RateType], [PlanType], [IsMajority], [TemplateFlag], [LDCCode], [CreateDate], [UserID], [RatePackageName], [CustType], [ServiceType], [DivisionCode], [ConsUnitId], [ActiveFlag], [LDCRateCode], [migr_plan_id], [migr_custid]
FROM  saes_Spark..Rate WHERE RateID IN (
	SELECT RateID FROM saes_Spark..Customer WHERE CustID = @CustID
)
AND NOT EXISTS(SELECT 1 FROM daes_Spark..Rate WHERE RateID IN (
	SELECT RateID FROM saes_Spark..Customer WHERE CustID = @CustID
	)
)

INSERT INTO daes_Spark..Rate ([RateID], [CSPID], [RateCode], [RateDesc], [EffectiveDate], [ExpirationDate], [RateType], [PlanType], [IsMajority], [TemplateFlag], [LDCCode], [CreateDate], [UserID], [RatePackageName], [CustType], [ServiceType], [DivisionCode], [ConsUnitId], [ActiveFlag], [LDCRateCode], [migr_plan_id], [migr_custid])
SELECT  [RateID], [CSPID], [RateCode], [RateDesc], [EffectiveDate], [ExpirationDate], [RateType], [PlanType], [IsMajority], [TemplateFlag], [LDCCode], [CreateDate], [UserID], [RatePackageName], [CustType], [ServiceType], [DivisionCode], [ConsUnitId], [ActiveFlag], [LDCRateCode], [migr_plan_id], [migr_custid]
FROM  saes_Spark..Rate WHERE RateID IN (
	SELECT RateID FROM saes_Spark..Product WHERE ProductId IN (SELECT ProductID FROM saes_Spark..Contract WHERE CustID = @CustID)
)
AND NOT EXISTS(SELECT 1 FROM daes_Spark..Rate WHERE RateID IN (
	SELECT RateID FROM saes_Spark..Product WHERE ProductId IN (SELECT ProductID FROM saes_Spark..Contract WHERE CustID = @CustID)
	)
)
SET IDENTITY_INSERT daes_Spark..Rate OFF

PRINT 'Copy Customer'
SET IDENTITY_INSERT daes_Spark..Customer ON

INSERT INTO daes_Spark..Customer ([CustID], [CSPID], [CSPCustID], [PropertyID], [PropertyCustID], [CustomerTypeID], [CustNo], [CustName], [LastName], [FirstName], [MidName], [CompanyName], [DBA], [FederalTaxID], [AcctsRecID], [DistributedAR], [ProductionCycle], [BillCycle], [RateID], [SiteAddrID], [MailAddrId], [CorrAddrID], [MailToSiteAddress], [BillCustID], [MasterCustID], [Master], [CustStatus], [BilledThru], [CSRStatus], [CustType], [Services], [FEIN], [DOB], [Taxable], [LateFees], [NoOfAccts], [ConsolidatedInv], [SummaryInv], [MsgID], [TDSPTemplateID], [TDSPGroupID], [LifeSupportIndictor], [LifeSupportStatus], [LifeSupportDate], [SpecialBenefitsPlan], [BillFormat], [PrintLayoutID], [CreditScore], [HitIndicator], [RequiredDeposit], [AccountManager], [EnrollmentAlias], [ContractID], [ContractTerm], [ContractStartDate], [ContractEndDate], [UserDefined1], [CreateDate], [RateChangeDate], [ConversionAccountNo], [PermitContactName], [CustomerPrivacy], [UsagePrivacy], [CompanyRegistrationNumber], [VATNumber], [AccountStatus], [AutoCreditAfterInvoiceFlag], [LidaDiscount], [DoNotDisconnect], [DDPlus1], [CsrImportDate], [DeliveryTypeID], [SpecialNeedsAddrID], [PaymentModelId], [PORFlag], [PowerOutageAddrId])
SELECT [CustID], [CSPID], [CSPCustID], [PropertyID], [PropertyCustID], [CustomerTypeID], [CustNo], [CustName], [LastName], [FirstName], [MidName], [CompanyName], [DBA], [FederalTaxID], [AcctsRecID], [DistributedAR], [ProductionCycle], [BillCycle], [RateID], [SiteAddrID], [MailAddrId], [CorrAddrID], [MailToSiteAddress], [BillCustID], [MasterCustID], [Master], [CustStatus], [BilledThru], [CSRStatus], [CustType], [Services], [FEIN], [DOB], [Taxable], [LateFees], [NoOfAccts], [ConsolidatedInv], [SummaryInv], [MsgID], [TDSPTemplateID], [TDSPGroupID], [LifeSupportIndictor], [LifeSupportStatus], [LifeSupportDate], [SpecialBenefitsPlan], [BillFormat], [PrintLayoutID], [CreditScore], [HitIndicator], [RequiredDeposit], [AccountManager], [EnrollmentAlias], [ContractID], [ContractTerm], [ContractStartDate], [ContractEndDate], [UserDefined1], [CreateDate], [RateChangeDate], [ConversionAccountNo], [PermitContactName], [CustomerPrivacy], [UsagePrivacy], [CompanyRegistrationNumber], [VATNumber], [AccountStatus], [AutoCreditAfterInvoiceFlag], [LidaDiscount], [DoNotDisconnect], [DDPlus1], [CsrImportDate], [DeliveryTypeID], [SpecialNeedsAddrID], [PaymentModelId], [PORFlag], [PowerOutageAddrId]
FROM  saes_Spark..Customer WHERE CustID = @CustID
AND NOT EXISTS(SELECT 1 FROM daes_Spark..Customer WHERE CustId = @CustID)
SET IDENTITY_INSERT daes_Spark..Customer OFF

PRINT 'Copy Premise'
ALTER TABLE daes_Spark..Customer NOCHECK CONSTRAINT ALL
ALTER TABLE daes_Spark..Premise NOCHECK CONSTRAINT ALL
SET IDENTITY_INSERT daes_Spark..Premise ON

INSERT INTO daes_Spark..Premise ([PremID], [CustID], [CSPID], [AddrID], [TDSPTemplateID], [ServiceCycle], [TDSP], [TaxAssessment], [PremNo], [PremDesc], [PremStatus], [PremType], [LocationCode], [SpecialNeedsFlag], [SpecialNeedsStatus], [SpecialNeedsDate], [ReadingIncrement], [Metered], [Taxable], [BeginServiceDate], [EndServiceDate], [SourceLevel], [StatusID], [StatusDate], [CreateDate], [UnitID], [PropertyCommonID], [RateID], [DeleteFlag], [LBMPId], [PipelineId], [GasLossId], [LDCID], [GasPoolID], [DeliveryPoint], [ConsumptionBandIndex], [LastModifiedDate], [CreatedByID], [ModifiedByID], [BillingAccountNumber], [NameKey], [GasSupplyServiceOption], [IntervalUsageTypeId], [LDC_UnMeteredAcct], [AltPremNo], [OnSwitchHold], [SwitchHoldStartDate], [ConsumptionImportTypeId], [TDSPTemplateEffectiveDate], [ServiceDeliveryPoint], [UtilityContractID], [LidaDiscount], [GasCapacityAssignment], [CPAEnrollmentTypes], [IsTOU], [SupplierPricingStructureNr], [SupplierGroupNumber])
SELECT [PremID], [CustID], [CSPID], [AddrID], [TDSPTemplateID], [ServiceCycle], [TDSP], [TaxAssessment], [PremNo], [PremDesc], [PremStatus], [PremType], [LocationCode], [SpecialNeedsFlag], [SpecialNeedsStatus], [SpecialNeedsDate], [ReadingIncrement], [Metered], [Taxable], [BeginServiceDate], [EndServiceDate], [SourceLevel], 10/*[StatusID]*/, [StatusDate], [CreateDate], [UnitID], [PropertyCommonID], [RateID], [DeleteFlag], [LBMPId], [PipelineId], [GasLossId], [LDCID], [GasPoolID], [DeliveryPoint], [ConsumptionBandIndex], [LastModifiedDate], [CreatedByID], [ModifiedByID], [BillingAccountNumber], [NameKey], [GasSupplyServiceOption], [IntervalUsageTypeId], [LDC_UnMeteredAcct], [AltPremNo], [OnSwitchHold], [SwitchHoldStartDate], [ConsumptionImportTypeId], [TDSPTemplateEffectiveDate], [ServiceDeliveryPoint], [UtilityContractID], [LidaDiscount], [GasCapacityAssignment], [CPAEnrollmentTypes], [IsTOU], [SupplierPricingStructureNr], [SupplierGroupNumber]
FROM  saes_Spark..Premise WHERE CustID = @CustID
AND NOT EXISTS(SELECT 1 FROM daes_Spark..Premise WHERE CustId = @CustID)
SET IDENTITY_INSERT daes_Spark..Premise OFF
ALTER TABLE daes_Spark..Premise WITH CHECK CHECK CONSTRAINT ALL

PRINT 'Copy Product'
SET IDENTITY_INSERT daes_Spark..Product ON

INSERT INTO daes_Spark..Product ([ProductID], [RateID], [LDCCode], [PlanType], [TDSPTemplateID], [Description], [BeginDate], [EndDate], [CustType], [Graduated], [RangeTier1], [RangeTier2], [SortOrder], [ActiveFlag], [Uplift], [CSATDSPTemplateID], [CAATDSPTemplateID], [PriceDescription], [MarketingCode], [RateTypeID], [ConsUnitID], [Default], [DivisionCode], [RateDescription], [ServiceType], [CSPId], [TermsId], [RolloverProductId], [CommissionId], [CommissionAmt], [CancelFeeId], [MonthlyChargeId], [ProductCode], [RatePackageId], [ProductName], [TermDate], [DiscountTypeId], [DiscountAmount], [ProductZoneID], [IsGreen], [IsBestChoice], [ActiveEnrollmentFlag], [CreditScoreThreshold], [DepositAmount], [Incentives])
SELECT  [ProductID], [RateID], [LDCCode], [PlanType], [TDSPTemplateID], [Description], [BeginDate], [EndDate], [CustType], [Graduated], [RangeTier1], [RangeTier2], [SortOrder], [ActiveFlag], [Uplift], [CSATDSPTemplateID], [CAATDSPTemplateID], [PriceDescription], [MarketingCode], -1 /*[RateTypeID]*/, [ConsUnitID], [Default], [DivisionCode], [RateDescription], [ServiceType], [CSPId], [TermsId], /*[RolloverProductId]*/NULL, [CommissionId], [CommissionAmt], [CancelFeeId], [MonthlyChargeId], [ProductCode], [RatePackageId], [ProductName], [TermDate], [DiscountTypeId], [DiscountAmount], [ProductZoneID], [IsGreen], [IsBestChoice], [ActiveEnrollmentFlag], [CreditScoreThreshold], [DepositAmount], [Incentives]
FROM  saes_Spark..Product WHERE ProductId IN (SELECT ProductID FROM saes_Spark..Contract WHERE CustID = @CustID)
AND NOT EXISTS(SELECT 1 FROM daes_Spark..Product WHERE ProductId IN (SELECT ProductID FROM saes_Spark..Contract WHERE CustID = @CustID))
SET IDENTITY_INSERT daes_Spark..Product OFF

PRINT 'Copy Contract'
SET IDENTITY_INSERT daes_Spark..Contract ON

INSERT INTO daes_Spark..Contract ([ContractID], [SignedDate], [BeginDate], [EndDate], [TermLength], [ContractTypeID], [ContactName], [ContactPhone], [ContactFax], [ProductID], [CreatedByID], [CreateDate], [CustID], [AutoRenewFlag], [Service], [ActiveFlag], [RateCode], [TDSPTemplateID], [ContractTerm], [RateDetID], [RateID], [Terms], [ContractName], [ContractLength], [AccountManagerID], [MeterChargeCode], [AggregatorFee], [TermDate], [Bandwidth], [FinanceCharge], [ContractNumber], [PremID], [AnnualUsage], [CurePeriod], [ContractStatusID], [RenewalRate], [RenewalStartDate], [RenewalTerm], [ChangeReason])
SELECT  [ContractID], [SignedDate], [BeginDate], [EndDate], [TermLength], [ContractTypeID], [ContactName], [ContactPhone], [ContactFax], [ProductID], [CreatedByID], [CreateDate], [CustID], [AutoRenewFlag], [Service], [ActiveFlag], [RateCode], [TDSPTemplateID], [ContractTerm], [RateDetID], [RateID], [Terms], [ContractName], [ContractLength], [AccountManagerID], [MeterChargeCode], [AggregatorFee], [TermDate], [Bandwidth], [FinanceCharge], [ContractNumber], [PremID], [AnnualUsage], [CurePeriod], [ContractStatusID], [RenewalRate], [RenewalStartDate], [RenewalTerm], [ChangeReason]
FROM  saes_Spark..Contract WHERE CustID = @CustID
AND NOT EXISTS(SELECT 1 FROM daes_Spark..Contract WHERE CustID = @CustID)
SET IDENTITY_INSERT daes_Spark..Contract OFF

PRINT 'Copy AccountsReceivable'
SET IDENTITY_INSERT daes_Spark..AccountsReceivable ON

INSERT INTO daes_Spark..AccountsReceivable ([AcctsRecID], [ResetDate], [ARDate], [PrevBal], [CurrInvs], [CurrPmts], [CurrAdjs], [BalDue], [LateFee], [LateFeeRate], [LateFeeMaxAmount], [LateFeeTypeID], [AuthorizedPymt], [PastDue], [BalAge0], [BalAge1], [BalAge2], [BalAge3], [BalAge4], [BalAge5], [BalAge6], [Deposit], [DepositBeginDate], [PaymentPlanFlag], [PaymentPlanTrueUpFlag], [PaymentPlanAmount], [PaymentPlanTrueUpPeriod], [PaymentPlanTrueUpThresholdAmount], [PaymentPlanTrueUpType], [PaymentPlanEffectiveDate], [PrePaymentFlag], [PrePaymentDailyAmount], [CapitalCredit], [Terms], [StatusID], [GracePeriod], [PaymentPlanTotalVariance], [PaymentPlanVarianceUnit], [LateFeeGracePeriod], [CancelFeeTypeId], [CancelFeeAmount], [Migr_acct_no], [InvoiceMinimumAmount], [LateFeeThresholdAmt], [LastInvoiceAcctsRecHistID], [LastPaymentAcctsRecHistId], [LastAdjustmentAcctsRecHistId], [DeferredBalance])
SELECT  [AcctsRecID], [ResetDate], [ARDate], [PrevBal], [CurrInvs], [CurrPmts], [CurrAdjs], [BalDue], [LateFee], [LateFeeRate], [LateFeeMaxAmount], [LateFeeTypeID], [AuthorizedPymt], [PastDue], [BalAge0], [BalAge1], [BalAge2], [BalAge3], [BalAge4], [BalAge5], [BalAge6], [Deposit], [DepositBeginDate], [PaymentPlanFlag], [PaymentPlanTrueUpFlag], [PaymentPlanAmount], [PaymentPlanTrueUpPeriod], [PaymentPlanTrueUpThresholdAmount], [PaymentPlanTrueUpType], [PaymentPlanEffectiveDate], [PrePaymentFlag], [PrePaymentDailyAmount], [CapitalCredit], [Terms], [StatusID], [GracePeriod], [PaymentPlanTotalVariance], [PaymentPlanVarianceUnit], [LateFeeGracePeriod], [CancelFeeTypeId], [CancelFeeAmount], [Migr_acct_no], [InvoiceMinimumAmount], [LateFeeThresholdAmt], [LastInvoiceAcctsRecHistID], [LastPaymentAcctsRecHistId], [LastAdjustmentAcctsRecHistId], [DeferredBalance]
FROM  saes_Spark..AccountsReceivable WHERE AcctsRecID IN (SELECT AcctsRecID FROM saes_Spark..Customer WHERE CustID = @CustID)
AND NOT EXISTS(SELECT 1 FROM daes_Spark..AccountsReceivable WHERE AcctsRecID IN (SELECT AcctsRecID FROM saes_Spark..Customer WHERE CustID = @CustID))
SET IDENTITY_INSERT daes_Spark..AccountsReceivable OFF

PRINT 'Copy CustomerAdditionalInfo'
--SET IDENTITY_INSERT daes_Spark..CustomerAdditionalInfo ON

INSERT INTO daes_Spark..CustomerAdditionalInfo ([CustID], [CSPDUNSID], [BillingTypeID], [BillingDayOfMonth], [MasterCustID_2], [MasterCustID_3], [MasterCustID_4], [TaxAssessment], [ContractPeriod], [ContractDate], [AccessVerificationType], [AccessVerificationData], [ClientAccountNo], [InstitutionID], [TransitNum], [AccountNum], [MigrationAccountNo], [MigrationFirstServed], [MigrationKwh], [CollectionsStageID], [CollectionsStatus], [CollectionsDate], [KeyAccount], [DisconnectLtr], [AuthorizedReleaseName], [AuthorizedReleaseDOB], [AuthorizedReleaseFederalTaxID], [EFTFlag], [PromiseToPayFlag], [DisconnectFlag], [CreditHoldFlag], [RawConsumptionImportFlag], [CustomerProtectionStatus], [MCPEFlag], [HasLocationMasterFlag], [DivisionID], [DivisionCode], [DriverLicenseNo], [PromotionCodeID], [CustomerDUNS], [CustomerGroupID], [EarlyTermFee], [EarlyTermFeeUpdateDate], [DeceasedFlag], [BankruptFlag], [CollectionsAgencyID], [DoNotCall], [CustomerSecretWord], [PrintGroupID], [CurrentCustNo], [IsDPP], [UnitNumber], [CustomerCategoryID], [SubsequentDepositExempt], [AutoPayFlag], [SpecialNeedsFlag], [SpecialNeedsEndDate], [SpecialNeedsQualifierTypeID], [CsrImportDate], [OnSwitchHold], [SwitchHoldStartDate], [DPPStatusID], [UpdateContactInfoFlag], [IsFriendlyLatePaymentReminderSent], [IsLowIncome], [ExtendedCustTypeId], [AutoPayLastUpdated], [GreenEnergyOptIn], [SocialCauseID], [SocialCauseCode], [SecondaryContactFirstName], [SecondaryContactLastName], [SecondaryContactPhone], [SecondaryContactRelationId], [SSN], [ServiceAccount], [IsPUCComplaint], [EncryptionPasswordTypeId], [EncryptionPasswordCustomValue], [CustInfo1], [CustInfo2], [CustInfo3], [CustInfo4], [CustInfo5], [SalesAgent], [Broker], [PromoCode], [CommissionType], [CommissionAmount], [ReferralID], [CampaignName], [AccessDBID], [SalesChannel], [TCPAAuthorization], [CancellationFee], [MunicipalAggregation])
SELECT  [CustID], [CSPDUNSID], [BillingTypeID], [BillingDayOfMonth], [MasterCustID_2], [MasterCustID_3], [MasterCustID_4], [TaxAssessment], [ContractPeriod], [ContractDate], [AccessVerificationType], [AccessVerificationData], [ClientAccountNo], [InstitutionID], [TransitNum], [AccountNum], [MigrationAccountNo], [MigrationFirstServed], [MigrationKwh], [CollectionsStageID], [CollectionsStatus], [CollectionsDate], [KeyAccount], [DisconnectLtr], [AuthorizedReleaseName], [AuthorizedReleaseDOB], [AuthorizedReleaseFederalTaxID], [EFTFlag], [PromiseToPayFlag], [DisconnectFlag], [CreditHoldFlag], [RawConsumptionImportFlag], [CustomerProtectionStatus], [MCPEFlag], [HasLocationMasterFlag], [DivisionID], [DivisionCode], [DriverLicenseNo], [PromotionCodeID], [CustomerDUNS], [CustomerGroupID], [EarlyTermFee], [EarlyTermFeeUpdateDate], [DeceasedFlag], [BankruptFlag], [CollectionsAgencyID], [DoNotCall], [CustomerSecretWord], [PrintGroupID], [CurrentCustNo], [IsDPP], [UnitNumber], [CustomerCategoryID], [SubsequentDepositExempt], [AutoPayFlag], [SpecialNeedsFlag], [SpecialNeedsEndDate], [SpecialNeedsQualifierTypeID], [CsrImportDate], [OnSwitchHold], [SwitchHoldStartDate], [DPPStatusID], [UpdateContactInfoFlag], [IsFriendlyLatePaymentReminderSent], [IsLowIncome], [ExtendedCustTypeId], [AutoPayLastUpdated], [GreenEnergyOptIn], [SocialCauseID], [SocialCauseCode], [SecondaryContactFirstName], [SecondaryContactLastName], [SecondaryContactPhone], [SecondaryContactRelationId], [SSN], [ServiceAccount], [IsPUCComplaint], [EncryptionPasswordTypeId], [EncryptionPasswordCustomValue], [CustInfo1], [CustInfo2], [CustInfo3], [CustInfo4], [CustInfo5], [SalesAgent], [Broker], [PromoCode], [CommissionType], [CommissionAmount], [ReferralID], [CampaignName], [AccessDBID], [SalesChannel], [TCPAAuthorization], [CancellationFee], [MunicipalAggregation]
FROM  saes_Spark..CustomerAdditionalInfo WHERE CustID = @CustID
AND NOT EXISTS(SELECT 1 FROM daes_Spark..CustomerAdditionalInfo WHERE CustID = @CustID)
--SET IDENTITY_INSERT daes_Spark..CustomerAdditionalInfo OFF

PRINT 'END Copy Customer'";
            DB.ExecuteQuery(sql);
        }

        private static void Change814DSchedule(DateTime scheduleDate)
        {
            string sql = $@"
UPDATE daes_SparkMarket..tbl_814_service SET SpecialReadSwitchDate = '{scheduleDate.ToString("yyyy-MM-dd")}' WHERE [814_Key] = 22748557
UPDATE daes_Spark..CustomerTransactionRequest SET RequestDate = '{scheduleDate.ToString("yyyy-MM-dd")}' WHERE SourceId = 22748557 AND TransactionTypeId = 43
UPDATE daes_BillingAdmin..EventActionQueue SET ScheduledDate = '{scheduleDate.ToString("yyyy-MM-dd")}' WHERE RequestID IN (SELECT RequestID FROM daes_Spark..CustomerTransactionRequest WHERE CustID = 1807055 AND TransactionTypeId = 43) AND ISNULL(ScheduledDate,0) <> 0
";
            DB.ExecuteQuery(sql);
        }

        public sealed class DB
        {
            public static void ExecuteQuery(string sql)
            {
                using (IDbConnection connection = new SqlConnection(_clientConfiguration.ConnectionCsr))
                {
                    connection.Open();
                    var cmd = connection.CreateCommand();
                    cmd.CommandType = CommandType.Text;
                    cmd.CommandText = sql;
                    cmd.CommandTimeout = 1000 * 60 * 5;
                    cmd.ExecuteNonQuery();
                    connection.Close();
                }
            }
        }
    }
}

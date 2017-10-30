using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

using Aurea.Maintenance.Debugger.Common;

using CIS.BusinessEntity;
//using CIS.Element.Billing;
//using Csla.Validation;

using System.Data.SqlClient;

namespace Aurea.Maintenance.Debugger.Stream
{
    class Program
    {
        static void Main(string[] args)
        {
            var clientConfiguration = Utility.SetSecurity(Utility.BillingAdminDEV, Utility.Clients["SGE"]);

            //SimulatePostEnrollmentEvent(clientConfiguration);
            CreateMockData(clientConfiguration);
            SimulateInbound814E(clientConfiguration);
        }

        #region private methods

        private static void SimulateInbound814E(GlobalApplicationConfigurationDS.GlobalApplicationConfiguration clientConfiguration)
        {
            // Set culture to en-EN to prevent string manipulation issues in base code
            SetThreadCulture("en-US");

            var engine = new CIS.Engine.Event.Queue(Utility.BillingAdminDEV);
            engine.ProcessEventQueue(clientConfiguration.ClientID, clientConfiguration.ConnectionCsr, clientConfiguration.ConnectionMarket, clientConfiguration.ClientAbbreviation);
        }

        private static void SimulatePostEnrollmentEvent(GlobalApplicationConfigurationDS.GlobalApplicationConfiguration clientConfiguration)
        {
            // Set culture to en-EN to prevent string manipulation issues in base code
            SetThreadCulture("en-US");

            var pe = new CIS.Clients.Stream.PostEnrollment
            {
                ConnectionString = clientConfiguration.ConnectionCsr
            };
            pe.Process(30396997);
        }

        #endregion

        #region helper methods


        private static void SetThreadCulture(string culture)
        {
            Thread.CurrentThread.CurrentCulture = System.Globalization.CultureInfo.GetCultureInfo(culture);
            Thread.CurrentThread.CurrentUICulture = System.Globalization.CultureInfo.GetCultureInfo(culture);
        }

        private static void CreateMockData(GlobalApplicationConfigurationDS.GlobalApplicationConfiguration clientConfiguration)
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
                DECLARE @RateID int
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
                SET @LDCID = 13
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

                PRINT 'Create Rate'
                IF NOT EXISTS (SELECT 1 FROM [Rate] WHERE RateCode = 'PREM_654643')
                BEGIN
	                INSERT INTO [Rate]
	                    ([CSPID], [RateCode], [RateDesc], [EffectiveDate], [ExpirationDate], [RateType], [PlanType], [IsMajority], [TemplateFlag], [LDCCode], [CreateDate], [UserID], [RatePackageName], [CustType], [ServiceType], [DivisionCode], [ConsUnitId], [ActiveFlag], [LDCRateCode], [migr_plan_id], [migr_custid])
	                SELECT
	                    NULL, 'PREM_654643', '', '2017-08-24 00:00:00.000', '2019-08-24 00:00:00.000', '', 0, 0, 1, NULL, '2017-08-24 16:00:24.307', NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL
	                SET @RateID = SCOPE_IDENTITY()
                END
                ELSE
                BEGIN
	                SET @RateID = (SELECT RateID FROM [Rate] WHERE RateCode = 'PREM_654643')
                END

                PRINT 'Create Customer'
                INSERT INTO [Customer]
                    ([CSPID], [CustomerTypeID], [CustNo], [CustName], [LastName], [FirstName], [MidName], [CompanyName], [DBA], [FederalTaxID], [AcctsRecID], [DistributedAR], [ProductionCycle], [BillCycle], [RateID], [SiteAddrID], [MailAddrId], [CorrAddrID], [BillCustID], [MasterCustID], [Master], [CustStatus], [BilledThru], [CSRStatus], [CustType], [Services], [FEIN], [DOB], [Taxable], [LateFees], [NoOfAccts], [ConsolidatedInv], [SummaryInv], [MsgID], [TDSPTemplateID], [TDSPGroupID], [LifeSupportIndictor], [LifeSupportStatus], [LifeSupportDate], [SpecialBenefitsPlan], [BillFormat], [PrintLayoutID], [CreditScore], [HitIndicator], [RequiredDeposit], [AccountManager], [EnrollmentAlias], [ContractID], [UserDefined1], [CreateDate], [RateChangeDate], [PermitContactName], [CustomerPrivacy], [UsagePrivacy], [LidaDiscount])
                SELECT 
                    @CSPID, @CustomerTypeID, @CustNo, 'Mock Cust Name', 'Mock Cust LName', 'Mock Cust FName', NULL, NULL, 'Mock DBA', NULL, NULL /*[AcctsRecID]*/, NULL, 55, 55, @RateID, @AddrID, @AddrId, @AddrID, NULL, @MasterCustID, 'N', 'A', NULL, NULL, @CustType, NULL, 8661, NULL, 'Y', 'N', NULL, NULL, 'N', NULL, 3, 1, 'N', NULL, NULL, NULL, 1, 1, NULL, NULL, 'N', NULL, NULL, NULL, 'None', DATEADD(dd, -2, GETDATE()), NULL, NULL, 0, 0, 0
                SET @CustID = SCOPE_IDENTITY()

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
                IF NOT EXISTS(SELECT 1 FROM [RateTransition] WHERE RateID = @RateID)
                BEGIN
	                INSERT INTO [RateTransition]
	                    ([CustID], [RateID], [UserID], [CreatedDate], [SwitchDate], [EndDate], [StatusID], [SoldDate], [RolloverFlag])
	                SELECT
	                    @CustID, @RateID, 0, '2017-08-24 16:00:50.327', '2017-09-08 00:00:00.000', '2018-09-08 00:00:00.000', 2, '2017-08-24 00:00:00.000', 0
	                SET @RateTransitionID = SCOPE_IDENTITY()
                END
                ELSE
                BEGIN
	                SET @RateTransitionID = (SELECT RateTransitionID FROM [RateTransition] WHERE RateID = @RateID)
                END

                PRINT 'Create RateDetail'
                INSERT INTO [RateDetail]
                    ([RateID], [CategoryID], [RateTypeID], [ConsUnitID], [RateDescID], [EffectiveDate], [ExpirationDate], [RateAmt], [RateAmt2], [RateAmt3], [FixedAdder], [MinDetAmt], [MaxDetAmt], [GLAcct], [RangeLower], [RangeUpper], [CustType], [Graduated], [Progressive], [AmountCap], [MaxRateAmt], [MinRateAmt], [CategoryRollup], [Taxable], [ChargeType], [MiscData1], [FixedCapRate], [ScaleFactor1], [ScaleFactor2], [TemplateRateDetID], [Margin], [HALRateDetailId], [UsageClassId], [LegacyRateDetailId], [Building], [ServiceTypeID], [TaxCategoryID], [UtilityID], [UtilityInvoiceTemplateDetailID], [Active], [StatusID], [RateVariableTypeId], [MinDays], [MaxDays], [BlockPriceIndicator], [RateTransitionId], [CreateDate], [MeterMultiplierFlag], [BlendRatio], [ContractVolumeID], [CreatedByUserId], [ModifiedByUserId], [ModifiedDate], [TOUTemplateID], [TOUTemplateRegisterID], [TOUTemplateRegisterName])
                SELECT
                    @RateID, 1, 3002, 5, 1, '2017-08-24 00:00:00.0000000', '2018-09-08 00:00:00.0000000', 0.08550000, NULL, NULL, NULL, 0.00, 0.00, '', NULL, NULL, '', '', '', 'N', '', '', '', 'Y', 'C', 51, 286.00000000, NULL, NULL, 565647, NULL, NULL, NULL, NULL, '', NULL, 0, NULL, NULL, 1, 1, NULL, NULL, NULL, 0, @RateTransitionID, '2017-08-24 16:00:50.433', 0, NULL, NULL, NULL, NULL, NULL, NULL, NULL, ''

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

                SET @PremiseUpdateActionID = (SELECT EventActionID FROM saes_BillingAdmin..EventAction WHERE EventID = 73616 AND ActionID IN (SELECT ActionID FROM daes_BillingAdmin..Action WHERE ActionName = 'PremiseUpdate'))
                SET @CustomerUpdateActionID = (SELECT EventActionID FROM saes_BillingAdmin..EventAction WHERE EventID = 73616 AND ActionID IN (SELECT ActionID FROM daes_BillingAdmin..Action WHERE ActionName = 'CustomerUpdate'))
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

            using (IDbConnection connection = new SqlConnection(clientConfiguration.ConnectionCsr))
            {
                connection.Open();
                var cmd = connection.CreateCommand();
                cmd.CommandType = CommandType.Text;
                cmd.CommandText = sqlString;
                cmd.CommandTimeout = 1000 * 60 * 5;
                cmd.ExecuteNonQuery();
                connection.Close();
            }
        }

        #endregion
    }
}

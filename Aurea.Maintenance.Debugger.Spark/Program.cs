using System.IO;
using System.Reflection;
using System.Threading;
using Aurea.TaskToaster;
using Aurea.TaskToaster.Client.Spark.Tasks;
using CIS.Clients.Spark.Model.Common;
using CIS.Clients.Spark.Model.Product;

namespace Aurea.Maintenance.Debugger.Spark
{
    using System;
    using Common;
    using Common.Models;
    using CIS.BusinessEntity;
    using System.Collections;
    using System.Data;
    using System.Data.SqlClient;
    using CIS.Framework.Common;


    using Aurea.Logging;

    using CIS.Clients.Spark.Model;
    using CIS.Clients.Spark.Processors.Customers.Context;
    using CIS.Clients.Spark.Processors.Customers.Task;
    using System.Collections.Generic;
    using System.Linq;
    using System.Runtime.Remoting.Contexts;
    using CIS.Clients.Spark.Model.Customer;
    using CIS.Framework.Data;
    using Aurea.Maintenance.Debugger.Common.Extensions;
    using Aurea.Maintenance.Debugger.Spark.Models;


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
                ClientID = Clients.Spark.Id();
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

        private static ClientEnvironmentConfiguration _clientConfig;
        private static GlobalApplicationConfigurationDS.GlobalApplicationConfiguration _appConfig;

        // ReSharper disable once InconsistentNaming
        private static readonly ILogger _logger = new Logger();
        private static string appDir = Path.Combine(Path.GetDirectoryName(Assembly.GetEntryAssembly().Location), "MockData");
        public static void Main(string[] args)
        {
            // Set client configuration and then the application configuration context.            
            _clientConfig = ClientConfiguration.GetClientConfiguration(Clients.Spark, Stages.Development);
            _appConfig = ClientConfiguration.SetConfigurationContext(_clientConfig);

            #region oldCases
            /*
            SimulateCalcuateNextRateTransitionDate();
            simulateAESCIS14129("002576466");
            ProcessEvents();

            PrepareMockDataForLetterGeneration();
            GenerateEventsForLetterGeneration();
            RestoreData2OriginalLetterGeneration();

            //TestCopyCustomer();
            var myCustList = new List<int> {1782208, 101993, 500551};
            //FindAndCopyCustomersWhichRTsWithNo814_C();
            ClearOldDataForCustomer(myCustList);
            CopyCustomersAndDetailsForProductRollOver(myCustList.Where(x => x != 101993).ToList());

            var sql = "UPDATE daes_Spark..EventEvaluationQueue SET Status = 1 WHERE Status = 0";
            SqlHelper.ExecuteNonQuery(_appConfig.ConnectionCsr, CommandType.Text, sql);
            //sql = $"UPDATE daes_Spark..EventEvaluationQueue SET Status = 0 WHERE Status = 1 AND SourceId IN (SELECT ChangeRequestId FROM daes_Spark..ChangeRequest WHERE ElementId = 6 AND CustomerID IN ({string.Join(",", myCustList.Where(x => x != 101993).ToList())}))";
            //SqlHelper.ExecuteNonQuery(_appConfig.ConnectionCsr, CommandType.Text, sql);
            sql = "UPDATE daes_Spark..EventEvaluationQueue SET Status = 0 WHERE Status = 1 AND SourceId IN (SELECT ChangeRequestId FROM daes_Spark..ChangeRequest WHERE ElementId = 6 AND ElementPrimaryKey IN (7474157, 12867318, 12983207, 13278416))";
            SqlHelper.ExecuteNonQuery(_appConfig.ConnectionCsr, CommandType.Text, sql);
            sql = "UPDATE daes_Spark..ChangeRequest SET StatusId = 1 WHERE ElementId = 6 AND ElementPrimaryKey IN (7474157, 12867318, 12983207, 13278416)";
            SqlHelper.ExecuteNonQuery(_appConfig.ConnectionCsr, CommandType.Text, sql);
            //SimulateCalcuateNextRateTransitionDate();
            //SimulateProductRollOver(myCustList);
            //SimulateCustomerContractEvaluation(myCustList);
            GenerateEventsFromEventEvaluationQueue();
            ProcessEvents();
            //GenerateEventsFromEventEvaluationQueue();
            //ProcessEvents();
            

            Simulate_AESCIS18511();
            Simulate_AESCIS19714();
            */
            #endregion

            Simulate_AESCIS_20017();

            _logger.Info("Debug session end");
            Console.ReadLine();
        }

        private static void Simulate_AESCIS_20017()
        {
            DB.ExecuteQuery("DISABLE TRIGGER ALL ON ChangeRequest;", _appConfig.ConnectionCsr);

            DB.ImportFiles(appDir, "20017-C1", _appConfig.ConnectionCsr);

            DB.ExecuteQuery("ENABLE TRIGGER ALL ON ChangeRequest;", _appConfig.ConnectionCsr);

            var custId = 1527408;
            var minTransactionDate = "2017-05-27";
            var sql = $"DELETE FROM CustomerTransactionRequest WHERE CustId = {custId}  AND TransactionType='814' AND ActionCode = 'C' AND Direction = 1 AND TransactionDate>'{minTransactionDate}'";
            DB.ExecuteQuery(sql, _appConfig.ConnectionCsr);
            
            
            // execute import task
            ExecuteImportChangeRequests();
            GenerateEvents(new List<int>() {27});
            ProcessEvents();

            GenerateEvents(new List<int>() { 16 });
            ProcessEvents();
        }

        private static void ExecuteImportChangeRequests()
        {
            var t = new CIS.Import.Billing.ChangeRequest(_appConfig.ConnectionCsr, _appConfig.ConnectionMarket, _clientConfig.ConnectionBillingAdmin, Clients.AEP.Id(), _logger);
            t.Import();
        }

        private static void Simulate_AESCIS19714()
        {
            DB.ImportFiles(appDir, "Past", _appConfig.ConnectionCsr);

            SimulateCustomerContractEvaluation();
        }

        private static void Simulate_AESCIS18511()
        {
            string dirToProcess = Path.Combine(Path.GetDirectoryName(Assembly.GetEntryAssembly().Location), "MockData");
            DB.ImportFiles(dirToProcess, "AESCIS-18511", _appConfig.ConnectionCsr);

            ExecuteEnrollCustomerPromotionTask();
            //EvaluateCustomerDepositStatusforEnrollment();
            //EvaluateCustomerforFutureDatedEnrollment();
            SendEnrollMarketTransaction();
            // --EnrollTransaction
            // --no need to export we just need 814 record, Export 814
            //GenerateEvents(new List<int> {18});//generate events EnrollCustomerEvaluation 18
            //ProcessEvents();
        }

        private static void ExecuteEnrollCustomerPromotionTask()
        {
            var taskContext = new TaskContext
            {
                Logger = _logger,
                ClientId = Clients.Spark.Id(),
                BillingAdminConnection = _clientConfig.ConnectionBillingAdmin,
                ClientConnection = _appConfig.ConnectionCsr,
            };

            var promotionTask = new PromotionTask { Context = taskContext };
            promotionTask.Execute();

            SimulateWelcomeLetterGenerateTask(taskContext);

            EvaluateCustomerDepositStatusforEnrollment();
            EvaluateCustomerforFutureDatedEnrollment();

            //var depositHoldEvaluationTask = new DepositHoldEvaluationTask {Context = taskContext};
            //depositHoldEvaluationTask.Execute();

            //var depositStatusEvaluationTask = new DepositStatusEvaluationTask {Context = taskContext};
            //depositStatusEvaluationTask.Execute();

            //var futureDateEnrollmentEvaluationTask = new FutureDateEnrollmentEvaluationTask {Context = taskContext};
            //futureDateEnrollmentEvaluationTask.Execute();

            //var futureDateHoldEvaluationTask = new FutureDateHoldEvaluationTask {Context = taskContext};
            //futureDateHoldEvaluationTask.Execute();

            //var sendEnrollMarketTransactionTask = new SendEnrollMarketTransactionTask {Context = taskContext};
            //sendEnrollMarketTransactionTask.Execute();
        }

        private static void SimulateWelcomeLetterGenerateTask(TaskContext taskContext)
        {
            //disabled 
            //var welcomeLetterGenerationTask = new WelcomeLetterGenerationTask { Context = taskContext };
            //welcomeLetterGenerationTask.Execute();

            var dataGateway = new CIS.Clients.Spark.Model.DataGateway
            {
                ClientId = taskContext.ClientId,
                ConnectionBillingAdmin = taskContext.BillingAdminConnection,
                ConnectionCsr = taskContext.ClientConnection,
                UserId = 289
            };
            var context = new CIS.Clients.Spark.Processors.Enrollment.Context.GenerateWelcomeLetterContext(dataGateway, taskContext.Logger);
            var welcomeLetterTask = new CIS.Clients.Spark.Processors.Enrollment.Task.GenerateWelcomePacket(context);
            foreach (var model in welcomeLetterTask.ListReadyForWelcomeLetterGeneration())
            {
                context.CustomerDataGateway.UpdateCustomerStatus(model.CustId, CustomerStatus.Dictionary[CustomerStatusOption.EnrollmentProcessing_LetterGenerated]);
                context.CorrelationDataGateway.InsertState(model.IstaCorrelationId, SourceTypeOption.LetterGeneration, model.CustId, CorrelationStateOption.WelcomeLetterSent, "Welcome Letter sent succesfully");
            }
        }

        private static void EvaluateCustomerDepositStatusforEnrollment()
        {
            var maintenance = new MyMaintenance(_appConfig.ConnectionCsr, _appConfig.ConnectionMarket, _clientConfig.ConnectionBillingAdmin);
            maintenance.EvaluateCustomerDepositStatusforEnrollment();
        }

        private static void EvaluateCustomerforFutureDatedEnrollment()
        {
            var maintenance = new MyMaintenance(_appConfig.ConnectionCsr, _appConfig.ConnectionMarket, _clientConfig.ConnectionBillingAdmin);
            maintenance.EvaluateCustomerforFutureDatedEnrollment();
        }

        private static void SendEnrollMarketTransaction()
        {
            var maintenance = new MyMaintenance(_appConfig.ConnectionCsr, _appConfig.ConnectionMarket, _clientConfig.ConnectionBillingAdmin);
            maintenance.SendEnrollMarketTransaction();
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


            //var maintenance = new MyMaintenance(_appConfig.ConnectionCsr, _appConfig.ConnectionMarket, _clientConfig.ConnectionBillingAdmin);
            //maintenance.GenerateEvents();
        }

        private static void TestCopyCustomer()
        {
            var myCust = new Customer();
            myCust.CustID = 1;
            var result = myCust.CopyEntityFromSaes2Daes(_appConfig.ConnectionCsr, "Spark", _logger, true);
        }

        private static void PrepareDataForRTTrigger(int custId)
        {
            var sql = $@"SELECT TOP 2 RateTransitionID from daes_Spark..RateTransition WHERE CustId = {custId} ORDER BY CreatedDate DESC";
            using (var ds = SqlHelper.ExecuteDataset(_appConfig.ConnectionCsr, CommandType.Text, sql))
            {
                if (ds == null || ds.Tables.Count == 0 || ds.Tables[0].Rows.Count == 0)
                return;
                var lastRtIds = ds.Tables[0].AsEnumerable().Select(row => row.Field<int>("RateTransitionID")).ToArray();
                if (lastRtIds.Length > 1)
                {
                    //sql = $"DELETE FROM daes_Spark.ClientCustomer.RateTransition WHERE RateTransitionID = {lastRtIds[0]}";
                    //SqlHelper.ExecuteNonQuery(_appConfig.ConnectionCsr, CommandType.Text, sql);
                    //sql = $"DELETE FROM daes_Spark..RateTransition WHERE RateTransitionID = {lastRtIds[0]}";
                    //SqlHelper.ExecuteNonQuery(_appConfig.ConnectionCsr, CommandType.Text, sql);

                    //change next ldcRateCode

                    //sql = $"UPDATE daes_Spark.ClientCustomer.RateTransition SET LDCRateCode = CONVERT(float,LDCRateCode)+0.001 WHERE RateTransitionID = {lastRtIds[1]}";
                    //SqlHelper.ExecuteNonQuery(_appConfig.ConnectionCsr, CommandType.Text, sql);

                }
            }

            //sql = $"SELECT RateId FROM daes_Spark..Customer WHERE CustId = {custId}";
            //using (var reader = SqlHelper.ExecuteReader(_appConfig.ConnectionCsr, CommandType.Text, sql))
            //{
            //    int RateId = 0;
            //    while (reader.Read())
            //        RateId = reader.GetInt32(0);
            //    sql = $"DELETE FROM RateDetail WHERE RateDetID = (SELECT TOP 1 RateDetID FROM RateDetail WHERE RateId = {RateId} ORDER BY 1 DESC)";
            //    SqlHelper.ExecuteNonQuery(_appConfig.ConnectionCsr, CommandType.Text, sql);
            //}
        }

        private static void FindAndCopyCustomersWhichRTsWithNo814_C()
        {
            var custIdSql = @"
            SELECT top 1000 rt.CustId
            FROM dbo.RateTransition rt
            left JOIN dbo.RateTransition prevRT WITH(NOLOCK) ON prevRt.RateTransitionID = (SELECT MAX(RateTransitionID) FROM dbo.RateTransition rt2 WHERE rt2.CustID = rt.CustID AND rt2.RateTransitionID < rt.RateTransitionID) 
            LEFT JOIN dbo.ChangeRequest cr WITH(NOLOCK) ON cr.ElementPrimaryKey = rt.RateTransitionID AND cr.ElementID = 6
            left join Product pro WITH(NOLOCK) on pro.RateID = rt.RateID
            left join Product pro2 WITH(NOLOCK) on pro2.RateID = prevRT.RateID
            left join Premise p WITH(NOLOCK) on p.CustId = rt.CustId
            Left Join PremiseStatus ps WITH(NOLOCK) ON ps.PremiseStatusID = p.StatusID
            left JOIN CustomerAdditionalInfo cai WITH(NOLOCK) ON cai.CustID = rt.CustID
            WHERE
                rt.RateID<> prevRT.RateID
                AND cr.ChangeRequestID IS  NULL
                AND ps.ActiveFlag = 1
                AND pro.ProductCode<> pro2.ProductCode
                AND p.LDCId = 13
                AND cai.BillingTypeID = 3
            order by rt.SwitchDate desc
            ";

            var ds = SqlHelper.ExecuteDataset(_appConfig.ConnectionCsr.Replace("daes_", "saes_"), CommandType.Text, custIdSql);
            if (ds == null || ds.Tables.Count == 0 || ds.Tables[0].Rows.Count == 0)
                return;
            var custIdList = ds.Tables[0].AsEnumerable().Select(row => row.Field<int>("CustId")).ToList();

            //custIdList = ds.Tables[0].AsEnumerable().Select(row => row.Field<int>("CustId")).ToList();

            CopyCustomersAndDetailsForProductRollOver(custIdList);
        }

        private static void ClearOldDataForCustomer(List<int> custIdList)
        {

            //clear
            // +Customer, +Address, +Premise, +CustomerAdditionalInfo, +AccountsReceivable, +Rate, +RateDetail, +RateTransition, +Product, --Terms, 
            // +Contract, +ClientCustomer.Contract, +Meter, +EdiLoadProfile, +ClientCustomer.RateTransition,
            // +RateIndexType, +RateIndexRange, +ClientCustomer.ChangeProductRequest, +ClientCorrelation.CorrelationRequest, +ClientCustomer.CustomerCommission
            // +ChangeProductRequest, +ChangeRequest, +ChangeRequestDetail, +ChangeRequestDetailTransaction
            var currentIterationNumber = 0;
            foreach (var custId in custIdList)
            {
                var sql = $@"
USE daes_Spark
DECLARE @CustID AS INT = {custId}
DECLARE @CustNo AS VARCHAR(10) = (SELECT CustNo FROM Customer WHERE CustId = @CustId)
DECLARE @AcctsRecId AS INT = (SELECT AcctsRecID FROM saes_Spark..Customer WHERE CustID = @CustID)
UPDATE daes_Spark..Premise SET StatusId = 11 WHERE CustID = @CustID

UPDATE daes_Spark..Customer SET CustStatus='I', AcctsRecID = NULL WHERE CustID = @CustID

PRINT 'Clean ChangeRequestDetailTransaction'
DELETE FROM daes_Spark..ChangeRequestDetailTransaction
WHERE ChangeRequestDetailID IN (SELECT ChangeRequestDetailID FROM saes_Spark..ChangeRequestDetail WHERE ChangeRequestID IN (SELECT ChangeRequestID FROM saes_Spark..ChangeRequest WHERE CustomerId = @CustId))

PRINT 'Clean ChangeRequestDetail'
DELETE FROM daes_Spark..ChangeRequestDetail
WHERE ChangeRequestID IN (SELECT ChangeRequestID FROM saes_Spark..ChangeRequest WHERE CustomerId = @CustId)

PRINT 'Clean ChangeRequest'
DELETE FROM daes_Spark..ChangeRequest
WHERE CustomerId = @CustId

PRINT 'Clean ClientCustomer.ChangeProductRequest'
DELETE FROM daes_Spark.ClientCustomer.ChangeProductRequest
WHERE CustId = @CustId

PRINT 'Clean ClientCustomer.CustomerCommission'
DELETE FROM daes_Spark.ClientCustomer.CustomerCommission
WHERE CustId = @CustId 

PRINT 'Clean ClientCorrelation.CorrelationRequest'
DELETE FROM daes_Spark.ClientCorrelation.CorrelationRequest
WHERE CustomerAccountNumber = @CustNo 

PRINT 'Clean ClientCustomer.ChangeProductRequest'
DELETE FROM daes_Spark.ClientCustomer.ChangeProductRequest
WHERE CustId = @CustId

PRINT 'Clean Meter'
DELETE FROM daes_Spark..Meter
WHERE MeterID IN (SELECT MeterId FROM saes_Spark..Meter WHERE PremID IN (SELECT PremID FROM saes_Spark..Premise WHERE CustId = @CustId))

PRINT 'Clean ClientCustomer.CustomerAdditionalInfo'
DELETE FROM daes_Spark.ClientCustomer.CustomerAdditionalInfo
WHERE CustId = @CustId

PRINT 'Clean CustomerAdditionalInfo'
DELETE FROM daes_Spark..CustomerAdditionalInfo
WHERE CustID = @CustID

IF(ISNULL(@AcctsRecId,0)>0)
BEGIN
PRINT 'Clean AccountsReceivable'
DELETE FROM daes_Spark..AccountsReceivable
WHERE AcctsRecID IN (SELECT AcctsRecID FROM saes_Spark..Customer WHERE CustID = @CustID)
END

PRINT 'Clean ClientCustomer.Contract'
DELETE FROM daes_Spark.ClientCustomer.Contract
WHERE ContractId IN (SELECT ContractId FROM saes_Spark..Contract WHERE CustId = @CustId)

PRINT 'Clean Contract'
DELETE FROM daes_Spark..Contract
WHERE CustID = @CustID

PRINT 'Clean Product'
UPDATE Product SET RolloverProductID=NULL WHERE ProductId IN (
            SELECT ProductID FROM saes_Spark..Contract WHERE CustID = @CustID
            UNION
            SELECT ProductId FROM saes_Spark.ClientCustomer.ChangeProductRequest WHERE CustId = @CustId
            UNION
            SELECT SegmentationRolloverProductId FROM saes_Spark.ClientCustomer.ChangeProductRequest WHERE CustId = @CustId
			)
DELETE FROM ClientCustomer.ChangeProductRequest WHERE ProductId IN (
            SELECT ProductID FROM saes_Spark..Contract WHERE CustID = @CustID
            UNION
            SELECT ProductId FROM saes_Spark.ClientCustomer.ChangeProductRequest WHERE CustId = @CustId
            UNION
            SELECT SegmentationRolloverProductId FROM saes_Spark.ClientCustomer.ChangeProductRequest WHERE CustId = @CustId
			)

DELETE FROM ClientCustomer.CustomerCommission WHERE ProductId IN (
            SELECT ProductID FROM saes_Spark..Contract WHERE CustID = @CustID
            UNION
            SELECT ProductId FROM saes_Spark.ClientCustomer.ChangeProductRequest WHERE CustId = @CustId
            UNION
            SELECT SegmentationRolloverProductId FROM saes_Spark.ClientCustomer.ChangeProductRequest WHERE CustId = @CustId
			)
DELETE FROM Product WHERE ProductId IN (
			SELECT RolloverProductID FROM saes_Spark..Contract WHERE CustID = @CustID
            UNION
            SELECT RolloverProductID FROM saes_Spark.ClientCustomer.ChangeProductRequest WHERE CustId = @CustId
			)

DELETE FROM daes_Spark..Product 
WHERE
    (
         src.ProductId IN (
            SELECT ProductID FROM saes_Spark..Contract WHERE CustID = @CustID
            UNION
            SELECT ProductId FROM saes_Spark.ClientCustomer.ChangeProductRequest WHERE CustId = @CustId
            UNION
            SELECT SegmentationRolloverProductId FROM saes_Spark.ClientCustomer.ChangeProductRequest WHERE CustId = @CustId
         )
         or
         src.RateId IN (SELECT RateId FROM saes_Spark..Customer WHERE CustId = @CustId)
     ) 


PRINT 'Clean Premise'
DELETE FROM daes_Spark..Premise
WHERE CustID = @CustID

PRINT 'Clean Customer'

DELETE FROM daes_Spark..CustomerHierarchy WHERE CustId = @CustID
DELETE FROM daes_Spark..ChangeLog WHERE CustId = @CustID
DELETE FROM daes_Spark..PremiseStatusLog WHERE CustId = @CustID
DELETE FROM daes_Spark..CustomerBudgetReview WHERE CustId = @CustID
DELETE FROM daes_Spark..Customer WHERE CustID = @CustID

PRINT 'Clean RateTransition'
DELETE FROM daes_Spark..RateTransition
WHERE CustId = @CustId OR RateTransitionID IN (SELECT RateTransitionID FROM saes_Spark..RateTransition WHERE CustId = @CustId)

PRINT 'Clean RateDetail'
DELETE FROM daes_Spark..RateDetail
WHERE
  RateId IN (
    SELECT RateId FROM saes_Spark..Customer WHERE CustId = @CustId
    UNION
    SELECT RateID FROM saes_Spark..Product WHERE ProductId IN (
            SELECT ProductID FROM saes_Spark..Contract WHERE CustID = @CustID
            UNION
            SELECT ProductId FROM saes_Spark.ClientCustomer.ChangeProductRequest WHERE CustId = @CustId
            UNION
            SELECT SegmentationRolloverProductId FROM saes_Spark.ClientCustomer.ChangeProductRequest WHERE CustId = @CustId
    )
  )

PRINT 'Clean Rate'
DELETE FROM daes_Spark..Rate
WHERE
    RateID IN (
        SELECT RateID FROM saes_Spark..Product WHERE ProductId IN (
            SELECT ProductID FROM saes_Spark..Contract WHERE CustID = @CustID
            UNION
            SELECT ProductId FROM saes_Spark.ClientCustomer.ChangeProductRequest WHERE CustId = @CustId
            UNION
            SELECT SegmentationRolloverProductId FROM saes_Spark.ClientCustomer.ChangeProductRequest WHERE CustId = @CustId
        )
        UNION
        SELECT RateID FROM saes_Spark..Customer WHERE CustID = @CustID
    )

PRINT 'Clean Addresses'
DELETE FROM daes_Spark..Address
WHERE AddrID IN (
	SELECT SiteAddrId FROM saes_Spark..Customer WHERE CustID = @CustID
    UNION
    SELECT MailAddrId FROM saes_Spark..Customer WHERE CustID = @CustID
    UNION
    SELECT CorrAddrId FROM saes_Spark..Customer WHERE CustID = @CustID
    UNION
    SELECT AddrId FROM saes_Spark..Premise WHERE CustID = @CustID
    UNION
    SELECT AddrId FROM saes_Spark..Meter WHERE PremID IN (SELECT PremID FROM saes_Spark..Premise WHERE CustId = @CustId)
)
";
                try
                {
                    SqlHelper.ExecuteNonQuery(_appConfig.ConnectionCsr, CommandType.Text, sql);

                }
                catch (Exception e)
                {
                    _logger.Error(e, $"Error Occurred when clearing customer {custId}, currentIterationNumber {currentIterationNumber}");
                    try
                    {
                        SqlHelper.ExecuteNonQuery(_appConfig.ConnectionCsr, CommandType.Text, sql);
                    }
                    catch (Exception e1)
                    {
                        _logger.Error(e, $"Error Occurred when clearing customer {custId}, currentIterationNumber {currentIterationNumber}");
                    }
                }
                currentIterationNumber++;
                _logger.Info($"{currentIterationNumber} cleared");

            }
        }

        private static void CopyCustomersAndDetailsForProductRollOver(List<int> custIdList)
        {
            //copy
            // +Customer, +Address, +Premise, +CustomerAdditionalInfo, +AccountsReceivable, +Rate, +RateDetail, +RateTransition, +Product, --Terms, 
            // +Contract, +ClientCustomer.Contract, +Meter, +EdiLoadProfile, +ClientCustomer.RateTransition,
            // +RateIndexType, +RateIndexRange, +ClientCustomer.ChangeProductRequest, +ClientCorrelation.CorrelationRequest, +ClientCustomer.CustomerCommission
            // +ChangeProductRequest, +ChangeRequest, +ChangeRequestDetail, +ChangeRequestDetailTransaction
            var currentIterationNumber = 0;
            foreach (var custId in custIdList)
            {
                using (var ts = TransactionFactory.CreateTransactionScope())
                {
                            var sql = $@"
DECLARE @CustID AS INT = {custId}
DECLARE @CustNo AS VARCHAR(10) = (SELECT CustNo FROM Customer WHERE CustId = @CustId)

PRINT 'BEGIN Copy Customer'

PRINT 'Copy Addresses'
SET IDENTITY_INSERT daes_Spark..Address ON

INSERT INTO daes_Spark..Address
([AddrID], [ValidationStatusID], [AttnMS], [Addr1], [Addr2], [City], [State], [Zip], [Zip4], [DPBC], [CityID], [CountyID], [County], [HomePhone], [WorkPhone], [FaxPhone], [OtherPhone], [Email], [ESIID], [GeoCode], [Status], [DeliveryPointCode], [CreateDate], [Migr_Enrollid], [PhoneExtension], [OtherExtension], [FaxExtension], [TaxingDistrict], [TaxInCity], [CchVersion])
SELECT
  [AddrID], [ValidationStatusID], [AttnMS], [Addr1], [Addr2], [City], [State], [Zip], [Zip4], [DPBC], [CityID], [CountyID], [County], [HomePhone], [WorkPhone], [FaxPhone], [OtherPhone], [Email], [ESIID], [GeoCode], [Status], [DeliveryPointCode], [CreateDate], [Migr_Enrollid], [PhoneExtension], [OtherExtension], [FaxExtension], [TaxingDistrict], [TaxInCity], [CchVersion] 
FROM  saes_Spark..Address src
WHERE AddrID IN (
	SELECT SiteAddrId FROM saes_Spark..Customer WHERE CustID = @CustID
    UNION
    SELECT MailAddrId FROM saes_Spark..Customer WHERE CustID = @CustID
    UNION
    SELECT CorrAddrId FROM saes_Spark..Customer WHERE CustID = @CustID
    UNION
    SELECT AddrId FROM saes_Spark..Premise WHERE CustID = @CustID
    UNION
    SELECT AddrId FROM saes_Spark..Meter WHERE PremID IN (SELECT PremID FROM saes_Spark..Premise WHERE CustId = @CustId)
)
AND NOT EXISTS (SELECT 1 FROM daes_Spark..Address dst WHERE src.AddrID = dst.AddrID)


SET IDENTITY_INSERT daes_Spark..Address OFF

PRINT 'Copy Rate'
SET IDENTITY_INSERT daes_Spark..Rate ON


INSERT INTO daes_Spark..Rate
([RateID], [CSPID], [RateCode], [RateDesc], [EffectiveDate], [ExpirationDate], [RateType], [PlanType], [IsMajority], [TemplateFlag], [LDCCode], [CreateDate], [UserID], [RatePackageName], [CustType], [ServiceType], [DivisionCode], [ConsUnitId], [ActiveFlag], [LDCRateCode], [migr_plan_id], [migr_custid])
SELECT
  [RateID], [CSPID], [RateCode], [RateDesc], [EffectiveDate], [ExpirationDate], [RateType], [PlanType], [IsMajority], [TemplateFlag], [LDCCode], [CreateDate], [UserID], [RatePackageName], [CustType], [ServiceType], [DivisionCode], [ConsUnitId], [ActiveFlag], [LDCRateCode], [migr_plan_id], [migr_custid]
FROM  saes_Spark..Rate src
WHERE
    RateID IN (
        SELECT RateID FROM saes_Spark..Product WHERE ProductId IN (
            SELECT ProductID FROM saes_Spark..Contract WHERE CustID = @CustID
            UNION
            SELECT ProductId FROM saes_Spark.ClientCustomer.ChangeProductRequest WHERE CustId = @CustId
            UNION
            SELECT SegmentationRolloverProductId FROM saes_Spark.ClientCustomer.ChangeProductRequest WHERE CustId = @CustId
        )
        UNION
        SELECT RateID FROM saes_Spark..Customer WHERE CustID = @CustID
    )
    AND NOT EXISTS(SELECT 1 FROM daes_Spark..Rate dst WHERE src.RateID = dst.RateId)

SET IDENTITY_INSERT daes_Spark..Rate OFF

PRINT 'Copy RateDetail'

SET IDENTITY_INSERT daes_Spark..RateDetail ON
INSERT INTO daes_Spark..RateDetail
([RateDetID], [RateID], [CategoryID], [RateTypeID], [ConsUnitID], [RateDescID], [EffectiveDate], [ExpirationDate], [RateAmt], [RateAmt2], [RateAmt3], [FixedAdder], [MinDetAmt], [MaxDetAmt], [GLAcct], [RangeLower], [RangeUpper], [CustType], [Graduated], [Progressive], [AmountCap], [MaxRateAmt], [MinRateAmt], [CategoryRollup], [Taxable], [ChargeType], [MiscData1], [FixedCapRate], [ScaleFactor1], [ScaleFactor2], [TemplateRateDetID], [Margin], [HALRateDetailId], [UsageClassId], [LegacyRateDetailId], [Building], [ServiceTypeID], [TaxCategoryID], [UtilityID], [UtilityInvoiceTemplateDetailID], [Active], [StatusID], [RateVariableTypeId], [MinDays], [MaxDays], [BlockPriceIndicator], [RateTransitionId], [CreateDate], [MeterMultiplierFlag], [BlendRatio], [ContractVolumeID], [CreatedByUserId], [ModifiedByUserId], [ModifiedDate], [TOUTemplateID], [TOUTemplateRegisterID], [TOUTemplateRegisterName])
SELECT 
 [RateDetID], [RateID], [CategoryID], [RateTypeID], [ConsUnitID], [RateDescID], [EffectiveDate], [ExpirationDate], [RateAmt], [RateAmt2], [RateAmt3], [FixedAdder], [MinDetAmt], [MaxDetAmt], [GLAcct], [RangeLower], [RangeUpper], [CustType], [Graduated], [Progressive], [AmountCap], [MaxRateAmt], [MinRateAmt], [CategoryRollup], [Taxable], [ChargeType], [MiscData1], [FixedCapRate], [ScaleFactor1], [ScaleFactor2], [TemplateRateDetID], [Margin], [HALRateDetailId], [UsageClassId], [LegacyRateDetailId], [Building], [ServiceTypeID], [TaxCategoryID], [UtilityID], [UtilityInvoiceTemplateDetailID], [Active], [StatusID], [RateVariableTypeId], [MinDays], [MaxDays], [BlockPriceIndicator], [RateTransitionId], [CreateDate], [MeterMultiplierFlag], [BlendRatio], [ContractVolumeID], [CreatedByUserId], [ModifiedByUserId], [ModifiedDate], [TOUTemplateID], [TOUTemplateRegisterID], [TOUTemplateRegisterName]
FROM saes_Spark..RateDetail src
WHERE
  src.RateId IN (
    SELECT RateId FROM saes_Spark..Customer WHERE CustId = @CustId
    UNION
    SELECT RateID FROM saes_Spark..Product WHERE ProductId IN (
            SELECT ProductID FROM saes_Spark..Contract WHERE CustID = @CustID
            UNION
            SELECT ProductId FROM saes_Spark.ClientCustomer.ChangeProductRequest WHERE CustId = @CustId
            UNION
            SELECT SegmentationRolloverProductId FROM saes_Spark.ClientCustomer.ChangeProductRequest WHERE CustId = @CustId
    )
  )
  AND NOT EXISTS(SELECT 1 FROM daes_Spark..RateDetail dst WHERE src.RateDetID = dst.RateDetID )

SET IDENTITY_INSERT daes_Spark..RateDetail OFF

PRINT 'Copy RateTransition'
SET IDENTITY_INSERT daes_Spark..RateTransition ON
INSERT INTO daes_Spark..RateTransition
([RateTransitionID], [CustID], [RateID], [UserID], [CreatedDate], [SwitchDate], [EndDate], [StatusID], [SoldDate], [RolloverFlag])
SELECT
 [RateTransitionID], [CustID], [RateID], [UserID], [CreatedDate], [SwitchDate], [EndDate], [StatusID], [SoldDate], [RolloverFlag]
FROM saes_Spark..RateTransition src
WHERE
  src.CustId = @CustId
  AND NOT EXISTS(SELECT 1 FROM daes_Spark..RateTransition dst WHERE src.RateTransitionId = dst.RateTransitionId )

SET IDENTITY_INSERT daes_Spark..RateTransition OFF

INSERT INTO daes_Spark.ClientCustomer.RateTransition
( [RateTransitionID], [ProductChangeType], [LDCRateCode], [NextRateTransitionDate], [SegmentationLabel], [SegmentationAdjustmentToEnergyRate], [MeterReadDate], [DesiredStartDate])
SELECT
  [RateTransitionID], [ProductChangeType], [LDCRateCode], [NextRateTransitionDate], [SegmentationLabel], [SegmentationAdjustmentToEnergyRate], [MeterReadDate], [DesiredStartDate]
FROM saes_Spark.ClientCustomer.RateTransition src
WHERE
  src.RateTransitionID IN (SELECT RateTransitionID FROM saes_Spark..RateTransition WHERE CustId = @CustId) 
  AND NOT EXISTS(SELECT 1 FROM daes_Spark.ClientCustomer.RateTransition dst WHERE src.RateTransitionID = dst.RateTransitionID )

PRINT 'Copy Customer'
SET IDENTITY_INSERT daes_Spark..Customer ON

INSERT INTO daes_Spark..Customer ([CustID], [CSPID], [CSPCustID], [PropertyID], [PropertyCustID], [CustomerTypeID], [CustNo], [CustName], [LastName], [FirstName], [MidName], [CompanyName], [DBA], [FederalTaxID], [AcctsRecID], [DistributedAR], [ProductionCycle], [BillCycle], [RateID], [SiteAddrID], [MailAddrId], [CorrAddrID], [MailToSiteAddress], [BillCustID], [MasterCustID], [Master], [CustStatus], [BilledThru], [CSRStatus], [CustType], [Services], [FEIN], [DOB], [Taxable], [LateFees], [NoOfAccts], [ConsolidatedInv], [SummaryInv], [MsgID], [TDSPTemplateID], [TDSPGroupID], [LifeSupportIndictor], [LifeSupportStatus], [LifeSupportDate], [SpecialBenefitsPlan], [BillFormat], [PrintLayoutID], [CreditScore], [HitIndicator], [RequiredDeposit], [AccountManager], [EnrollmentAlias], [ContractID], [ContractTerm], [ContractStartDate], [ContractEndDate], [UserDefined1], [CreateDate], [RateChangeDate], [ConversionAccountNo], [PermitContactName], [CustomerPrivacy], [UsagePrivacy], [CompanyRegistrationNumber], [VATNumber], [AccountStatus], [AutoCreditAfterInvoiceFlag], [LidaDiscount], [DoNotDisconnect], [DDPlus1], [CsrImportDate], [DeliveryTypeID], [SpecialNeedsAddrID], [PaymentModelId], [PORFlag], [PowerOutageAddrId])
SELECT [CustID], [CSPID], [CSPCustID], [PropertyID], [PropertyCustID], [CustomerTypeID], [CustNo], [CustName], [LastName], [FirstName], [MidName], [CompanyName], [DBA], [FederalTaxID], [AcctsRecID], [DistributedAR], [ProductionCycle], [BillCycle], [RateID], [SiteAddrID], [MailAddrId], [CorrAddrID], [MailToSiteAddress], [BillCustID], [MasterCustID], [Master], [CustStatus], [BilledThru], [CSRStatus], [CustType], [Services], [FEIN], [DOB], [Taxable], [LateFees], [NoOfAccts], [ConsolidatedInv], [SummaryInv], [MsgID], [TDSPTemplateID], [TDSPGroupID], [LifeSupportIndictor], [LifeSupportStatus], [LifeSupportDate], [SpecialBenefitsPlan], [BillFormat], [PrintLayoutID], [CreditScore], [HitIndicator], [RequiredDeposit], [AccountManager], [EnrollmentAlias], [ContractID], [ContractTerm], [ContractStartDate], [ContractEndDate], [UserDefined1], [CreateDate], [RateChangeDate], [ConversionAccountNo], [PermitContactName], [CustomerPrivacy], [UsagePrivacy], [CompanyRegistrationNumber], [VATNumber], [AccountStatus], [AutoCreditAfterInvoiceFlag], [LidaDiscount], [DoNotDisconnect], [DDPlus1], [CsrImportDate], [DeliveryTypeID], [SpecialNeedsAddrID], [PaymentModelId], [PORFlag], [PowerOutageAddrId]
FROM  saes_Spark..Customer src
WHERE
    CustID = @CustID
    AND NOT EXISTS(SELECT 1 FROM daes_Spark..Customer dst WHERE src.CustId = dst.CustID)

SET IDENTITY_INSERT daes_Spark..Customer OFF

PRINT 'Copy Premise'
ALTER TABLE daes_Spark..Customer NOCHECK CONSTRAINT ALL

ALTER TABLE daes_Spark..Premise NOCHECK CONSTRAINT ALL
SET IDENTITY_INSERT daes_Spark..Premise ON

INSERT INTO daes_Spark..Premise ([PremID], [CustID], [CSPID], [AddrID], [TDSPTemplateID], [ServiceCycle], [TDSP], [TaxAssessment], [PremNo], [PremDesc], [PremStatus], [PremType], [LocationCode], [SpecialNeedsFlag], [SpecialNeedsStatus], [SpecialNeedsDate], [ReadingIncrement], [Metered], [Taxable], [BeginServiceDate], [EndServiceDate], [SourceLevel], [StatusID], [StatusDate], [CreateDate], [UnitID], [PropertyCommonID], [RateID], [DeleteFlag], [LBMPId], [PipelineId], [GasLossId], [LDCID], [GasPoolID], [DeliveryPoint], [ConsumptionBandIndex], [LastModifiedDate], [CreatedByID], [ModifiedByID], [BillingAccountNumber], [NameKey], [GasSupplyServiceOption], [IntervalUsageTypeId], [LDC_UnMeteredAcct], [AltPremNo], [OnSwitchHold], [SwitchHoldStartDate], [ConsumptionImportTypeId], [TDSPTemplateEffectiveDate], [ServiceDeliveryPoint], [UtilityContractID], [LidaDiscount], [GasCapacityAssignment], [CPAEnrollmentTypes], [IsTOU], [SupplierPricingStructureNr], [SupplierGroupNumber])
SELECT [PremID], [CustID], [CSPID], [AddrID], [TDSPTemplateID], [ServiceCycle], [TDSP], [TaxAssessment], [PremNo], [PremDesc], [PremStatus], [PremType], [LocationCode], [SpecialNeedsFlag], [SpecialNeedsStatus], [SpecialNeedsDate], [ReadingIncrement], [Metered], [Taxable], [BeginServiceDate], [EndServiceDate], [SourceLevel], 10/*[StatusID]*/, [StatusDate], [CreateDate], [UnitID], [PropertyCommonID], [RateID], [DeleteFlag], [LBMPId], [PipelineId], [GasLossId], [LDCID], [GasPoolID], [DeliveryPoint], [ConsumptionBandIndex], [LastModifiedDate], [CreatedByID], [ModifiedByID], [BillingAccountNumber], [NameKey], [GasSupplyServiceOption], [IntervalUsageTypeId], [LDC_UnMeteredAcct], [AltPremNo], [OnSwitchHold], [SwitchHoldStartDate], [ConsumptionImportTypeId], [TDSPTemplateEffectiveDate], [ServiceDeliveryPoint], [UtilityContractID], [LidaDiscount], [GasCapacityAssignment], [CPAEnrollmentTypes], [IsTOU], [SupplierPricingStructureNr], [SupplierGroupNumber]
FROM  saes_Spark..Premise src
WHERE
    CustID = @CustID
    AND NOT EXISTS(SELECT 1 FROM daes_Spark..Premise dst WHERE src.PremId = dst.PremId)

SET IDENTITY_INSERT daes_Spark..Premise OFF
ALTER TABLE daes_Spark..Premise WITH CHECK CHECK CONSTRAINT ALL

PRINT 'Copy Product'
SET IDENTITY_INSERT daes_Spark..Product ON

INSERT INTO daes_Spark..Product 
([ProductID], [RateID], [LDCCode], [PlanType], [TDSPTemplateID], [Description], [BeginDate], [EndDate], [CustType], [Graduated], [RangeTier1], [RangeTier2], [SortOrder], [ActiveFlag], [Uplift], [CSATDSPTemplateID], [CAATDSPTemplateID], [PriceDescription], [MarketingCode], [RateTypeID], [ConsUnitID], [Default], [DivisionCode], [RateDescription], [ServiceType], [CSPId], [TermsId], [RolloverProductId], [CommissionId], [CommissionAmt], [CancelFeeId], [MonthlyChargeId], [ProductCode], [RatePackageId], [ProductName], [TermDate], [DiscountTypeId], [DiscountAmount], [ProductZoneID], [IsGreen], [IsBestChoice], [ActiveEnrollmentFlag], [CreditScoreThreshold], [DepositAmount], [Incentives])
SELECT
 [ProductID], [RateID], [LDCCode], [PlanType], [TDSPTemplateID], [Description], [BeginDate], [EndDate], [CustType], [Graduated], [RangeTier1], [RangeTier2], [SortOrder], [ActiveFlag], [Uplift], [CSATDSPTemplateID], [CAATDSPTemplateID], [PriceDescription], [MarketingCode], -1 /*[RateTypeID]*/, [ConsUnitID], [Default], [DivisionCode], [RateDescription], [ServiceType], [CSPId], [TermsId], /*[RolloverProductId]*/NULL, [CommissionId], [CommissionAmt], [CancelFeeId], [MonthlyChargeId], [ProductCode], [RatePackageId], [ProductName], [TermDate], [DiscountTypeId], [DiscountAmount], [ProductZoneID], [IsGreen], [IsBestChoice], [ActiveEnrollmentFlag], [CreditScoreThreshold], [DepositAmount], [Incentives]
FROM  saes_Spark..Product src
WHERE
    (
         src.ProductId IN (
            SELECT ProductID FROM saes_Spark..Contract WHERE CustID = @CustID
            UNION
            SELECT ProductId FROM saes_Spark.ClientCustomer.ChangeProductRequest WHERE CustId = @CustId
            UNION
            SELECT SegmentationRolloverProductId FROM saes_Spark.ClientCustomer.ChangeProductRequest WHERE CustId = @CustId
         )
         or
         src.RateId IN (SELECT RateId FROM saes_Spark..Customer WHERE CustId = @CustId)
     ) 
    AND NOT EXISTS(SELECT 1 FROM daes_Spark..Product dst WHERE dst.ProductId = src.ProductId )

SET IDENTITY_INSERT daes_Spark..Product OFF

PRINT 'Copy Contract'
SET IDENTITY_INSERT daes_Spark..Contract ON

INSERT INTO daes_Spark..Contract ([ContractID], [SignedDate], [BeginDate], [EndDate], [TermLength], [ContractTypeID], [ContactName], [ContactPhone], [ContactFax], [ProductID], [CreatedByID], [CreateDate], [CustID], [AutoRenewFlag], [Service], [ActiveFlag], [RateCode], [TDSPTemplateID], [ContractTerm], [RateDetID], [RateID], [Terms], [ContractName], [ContractLength], [AccountManagerID], [MeterChargeCode], [AggregatorFee], [TermDate], [Bandwidth], [FinanceCharge], [ContractNumber], [PremID], [AnnualUsage], [CurePeriod], [ContractStatusID], [RenewalRate], [RenewalStartDate], [RenewalTerm], [ChangeReason])
SELECT  [ContractID], [SignedDate], [BeginDate], [EndDate], [TermLength], [ContractTypeID], [ContactName], [ContactPhone], [ContactFax], [ProductID], [CreatedByID], [CreateDate], [CustID], [AutoRenewFlag], [Service], [ActiveFlag], [RateCode], [TDSPTemplateID], [ContractTerm], [RateDetID], [RateID], [Terms], [ContractName], [ContractLength], [AccountManagerID], [MeterChargeCode], [AggregatorFee], [TermDate], [Bandwidth], [FinanceCharge], [ContractNumber], [PremID], [AnnualUsage], [CurePeriod], [ContractStatusID], [RenewalRate], [RenewalStartDate], [RenewalTerm], [ChangeReason]
FROM  saes_Spark..Contract src
WHERE CustID = @CustID
AND NOT EXISTS(SELECT 1 FROM daes_Spark..Contract dst WHERE src.ContractID = dst.ContractID)
SET IDENTITY_INSERT daes_Spark..Contract OFF

PRINT 'Copy ClientCustomer.Contract'
INSERT INTO daes_Spark.ClientCustomer.Contract
( [ContractID], [SegmentationAdjustmentToEnergyRate], [SegmentationLabel])
SELECT
 [ContractID], [SegmentationAdjustmentToEnergyRate], [SegmentationLabel]
FROM saes_Spark.ClientCustomer.Contract src
WHERE
  src.ContractId IN (SELECT ContractId FROM saes_Spark..Contract WHERE CustId = @CustId)
  AND NOT EXISTS(SELECT 1 FROM daes_Spark.ClientCustomer.Contract dst WHERE src.ContractID = dst.ContractID )

PRINT 'Copy AccountsReceivable'
SET IDENTITY_INSERT daes_Spark..AccountsReceivable ON

INSERT INTO daes_Spark..AccountsReceivable ([AcctsRecID], [ResetDate], [ARDate], [PrevBal], [CurrInvs], [CurrPmts], [CurrAdjs], [BalDue], [LateFee], [LateFeeRate], [LateFeeMaxAmount], [LateFeeTypeID], [AuthorizedPymt], [PastDue], [BalAge0], [BalAge1], [BalAge2], [BalAge3], [BalAge4], [BalAge5], [BalAge6], [Deposit], [DepositBeginDate], [PaymentPlanFlag], [PaymentPlanTrueUpFlag], [PaymentPlanAmount], [PaymentPlanTrueUpPeriod], [PaymentPlanTrueUpThresholdAmount], [PaymentPlanTrueUpType], [PaymentPlanEffectiveDate], [PrePaymentFlag], [PrePaymentDailyAmount], [CapitalCredit], [Terms], [StatusID], [GracePeriod], [PaymentPlanTotalVariance], [PaymentPlanVarianceUnit], [LateFeeGracePeriod], [CancelFeeTypeId], [CancelFeeAmount], [Migr_acct_no], [InvoiceMinimumAmount], [LateFeeThresholdAmt], [LastInvoiceAcctsRecHistID], [LastPaymentAcctsRecHistId], [LastAdjustmentAcctsRecHistId], [DeferredBalance])
SELECT  [AcctsRecID], [ResetDate], [ARDate], [PrevBal], [CurrInvs], [CurrPmts], [CurrAdjs], [BalDue], [LateFee], [LateFeeRate], [LateFeeMaxAmount], [LateFeeTypeID], [AuthorizedPymt], [PastDue], [BalAge0], [BalAge1], [BalAge2], [BalAge3], [BalAge4], [BalAge5], [BalAge6], [Deposit], [DepositBeginDate], [PaymentPlanFlag], [PaymentPlanTrueUpFlag], [PaymentPlanAmount], [PaymentPlanTrueUpPeriod], [PaymentPlanTrueUpThresholdAmount], [PaymentPlanTrueUpType], [PaymentPlanEffectiveDate], [PrePaymentFlag], [PrePaymentDailyAmount], [CapitalCredit], [Terms], [StatusID], [GracePeriod], [PaymentPlanTotalVariance], [PaymentPlanVarianceUnit], [LateFeeGracePeriod], [CancelFeeTypeId], [CancelFeeAmount], [Migr_acct_no], [InvoiceMinimumAmount], [LateFeeThresholdAmt], [LastInvoiceAcctsRecHistID], [LastPaymentAcctsRecHistId], [LastAdjustmentAcctsRecHistId], [DeferredBalance]
FROM  saes_Spark..AccountsReceivable src
WHERE
    AcctsRecID IN (SELECT AcctsRecID FROM saes_Spark..Customer WHERE CustID = @CustID)
    AND NOT EXISTS(SELECT 1 FROM daes_Spark..AccountsReceivable dst WHERE src.AcctsRecID = dst.AcctsRecID)
SET IDENTITY_INSERT daes_Spark..AccountsReceivable OFF

PRINT 'Copy CustomerAdditionalInfo'
--SET IDENTITY_INSERT daes_Spark..CustomerAdditionalInfo ON

INSERT INTO daes_Spark..CustomerAdditionalInfo ([CustID], [CSPDUNSID], [BillingTypeID], [BillingDayOfMonth], [MasterCustID_2], [MasterCustID_3], [MasterCustID_4], [TaxAssessment], [ContractPeriod], [ContractDate], [AccessVerificationType], [AccessVerificationData], [ClientAccountNo], [InstitutionID], [TransitNum], [AccountNum], [MigrationAccountNo], [MigrationFirstServed], [MigrationKwh], [CollectionsStageID], [CollectionsStatus], [CollectionsDate], [KeyAccount], [DisconnectLtr], [AuthorizedReleaseName], [AuthorizedReleaseDOB], [AuthorizedReleaseFederalTaxID], [EFTFlag], [PromiseToPayFlag], [DisconnectFlag], [CreditHoldFlag], [RawConsumptionImportFlag], [CustomerProtectionStatus], [MCPEFlag], [HasLocationMasterFlag], [DivisionID], [DivisionCode], [DriverLicenseNo], [PromotionCodeID], [CustomerDUNS], [CustomerGroupID], [EarlyTermFee], [EarlyTermFeeUpdateDate], [DeceasedFlag], [BankruptFlag], [CollectionsAgencyID], [DoNotCall], [CustomerSecretWord], [PrintGroupID], [CurrentCustNo], [IsDPP], [UnitNumber], [CustomerCategoryID], [SubsequentDepositExempt], [AutoPayFlag], [SpecialNeedsFlag], [SpecialNeedsEndDate], [SpecialNeedsQualifierTypeID], [CsrImportDate], [OnSwitchHold], [SwitchHoldStartDate], [DPPStatusID], [UpdateContactInfoFlag], [IsFriendlyLatePaymentReminderSent], [IsLowIncome], [ExtendedCustTypeId], [AutoPayLastUpdated], [GreenEnergyOptIn], [SocialCauseID], [SocialCauseCode], [SecondaryContactFirstName], [SecondaryContactLastName], [SecondaryContactPhone], [SecondaryContactRelationId], [SSN], [ServiceAccount], [IsPUCComplaint], [EncryptionPasswordTypeId], [EncryptionPasswordCustomValue], [CustInfo1], [CustInfo2], [CustInfo3], [CustInfo4], [CustInfo5], [SalesAgent], [Broker], [PromoCode], [CommissionType], [CommissionAmount], [ReferralID], [CampaignName], [AccessDBID], [SalesChannel], [TCPAAuthorization], [CancellationFee], [MunicipalAggregation])
SELECT  [CustID], [CSPDUNSID], [BillingTypeID], [BillingDayOfMonth], [MasterCustID_2], [MasterCustID_3], [MasterCustID_4], [TaxAssessment], [ContractPeriod], [ContractDate], [AccessVerificationType], [AccessVerificationData], [ClientAccountNo], [InstitutionID], [TransitNum], [AccountNum], [MigrationAccountNo], [MigrationFirstServed], [MigrationKwh], [CollectionsStageID], [CollectionsStatus], [CollectionsDate], [KeyAccount], [DisconnectLtr], [AuthorizedReleaseName], [AuthorizedReleaseDOB], [AuthorizedReleaseFederalTaxID], [EFTFlag], [PromiseToPayFlag], [DisconnectFlag], [CreditHoldFlag], [RawConsumptionImportFlag], [CustomerProtectionStatus], [MCPEFlag], [HasLocationMasterFlag], [DivisionID], [DivisionCode], [DriverLicenseNo], [PromotionCodeID], [CustomerDUNS], [CustomerGroupID], [EarlyTermFee], [EarlyTermFeeUpdateDate], [DeceasedFlag], [BankruptFlag], [CollectionsAgencyID], [DoNotCall], [CustomerSecretWord], [PrintGroupID], [CurrentCustNo], [IsDPP], [UnitNumber], [CustomerCategoryID], [SubsequentDepositExempt], [AutoPayFlag], [SpecialNeedsFlag], [SpecialNeedsEndDate], [SpecialNeedsQualifierTypeID], [CsrImportDate], [OnSwitchHold], [SwitchHoldStartDate], [DPPStatusID], [UpdateContactInfoFlag], [IsFriendlyLatePaymentReminderSent], [IsLowIncome], [ExtendedCustTypeId], [AutoPayLastUpdated], [GreenEnergyOptIn], [SocialCauseID], [SocialCauseCode], [SecondaryContactFirstName], [SecondaryContactLastName], [SecondaryContactPhone], [SecondaryContactRelationId], [SSN], [ServiceAccount], [IsPUCComplaint], [EncryptionPasswordTypeId], [EncryptionPasswordCustomValue], [CustInfo1], [CustInfo2], [CustInfo3], [CustInfo4], [CustInfo5], [SalesAgent], [Broker], [PromoCode], [CommissionType], [CommissionAmount], [ReferralID], [CampaignName], [AccessDBID], [SalesChannel], [TCPAAuthorization], [CancellationFee], [MunicipalAggregation]
FROM  saes_Spark..CustomerAdditionalInfo WHERE CustID = @CustID
AND NOT EXISTS(SELECT 1 FROM daes_Spark..CustomerAdditionalInfo WHERE CustID = @CustID)
--SET IDENTITY_INSERT daes_Spark..CustomerAdditionalInfo OFF

PRINT 'Copy ClientCustomer.CustomerAdditionalInfo'
INSERT INTO daes_Spark.ClientCustomer.CustomerAdditionalInfo
([CustID], [InvoiceDueDays], [RolloverProductId], [ContractPath], [MarketerCode], [PromoCode], [SalesAgent], [SegmentationLabel], [SegmentationAdjustmentToEnergyRate], [ReleaseDate], [EstimatedFlowDate], [ReferAFriendToken], [DoesOnFlowDateResetContractDates], [IsEmployee], [EarlyTerminationFee], [CsrAutoPayFlag], [OCCNumber], [DealType], [DCQ], [SICCode], [EnrollmentRateClass], [IsRenewalEligible], [ExternalSalesId], [EnrollmentCategory], [ReferralID], [SalesChannel], [AutoPaySource], [AutoPayLastUpdated], [AutoPayFlag], [SoldDate])
SELECT
 [CustID], [InvoiceDueDays], [RolloverProductId], [ContractPath], [MarketerCode], [PromoCode], [SalesAgent], [SegmentationLabel], [SegmentationAdjustmentToEnergyRate], [ReleaseDate], [EstimatedFlowDate], [ReferAFriendToken], [DoesOnFlowDateResetContractDates], [IsEmployee], [EarlyTerminationFee], [CsrAutoPayFlag], [OCCNumber], [DealType], [DCQ], [SICCode], [EnrollmentRateClass], [IsRenewalEligible], [ExternalSalesId], [EnrollmentCategory], [ReferralID], [SalesChannel], [AutoPaySource], [AutoPayLastUpdated], [AutoPayFlag], [SoldDate]
FROM daes_Spark.ClientCustomer.CustomerAdditionalInfo src
WHERE
	src.CustId = @CustId
	AND NOT EXISTS(SELECT 1 FROM daes_Spark.ClientCustomer.CustomerAdditionalInfo dst WHERE src.CustId = dst.CustId )

PRINT 'Copy EdiLoadProfile'

SET IDENTITY_INSERT daes_Spark..EdiLoadProfile ON
INSERT INTO daes_Spark..EdiLoadProfile
([EdiLoadProfileId], [LoadProfile], [LDCCOde], [Migr_LoadProfile])
SELECT
 [EdiLoadProfileId], [LoadProfile], [LDCCOde], [Migr_LoadProfile]
FROM saes_Spark..EdiLoadProfile src
WHERE
  src.EdiLoadProfileId IN ( SELECT EdiLoadProfileId FROM saes_Spark..Meter WHERE MeterId IN (SELECT MeterId FROM saes_Spark..Meter WHERE PremID IN (SELECT PremID FROM saes_Spark..Premise WHERE CustId = @CustId)))
  AND NOT EXISTS(SELECT 1 FROM daes_Spark..EdiLoadProfile dst WHERE src.EdiLoadProfileId = dst.EdiLoadProfileId )

SET IDENTITY_INSERT daes_Spark..EdiLoadProfile OFF

PRINT 'Copy Meter'
SET IDENTITY_INSERT daes_Spark..Meter ON
INSERT INTO daes_Spark..Meter
( [MeterID], [ESIIDID], [AcctID], [AddrID], [TypeID], [PremID], [MeterNo], [MeterUniqueNo], [Pool], [MeterReadType], [MeterFactoryID], [MeterFactor], [BegRead], [EndRead], [DateFrom], [Dateto], [MeterStatus], [SourceLevel], [CreateDate], [EdiRateClassId], [EdiLoadProfileId], [AMSIndicator])
SELECT
 [MeterID], [ESIIDID], [AcctID], [AddrID], [TypeID], [PremID], [MeterNo], [MeterUniqueNo], [Pool], [MeterReadType], [MeterFactoryID], [MeterFactor], [BegRead], [EndRead], [DateFrom], [Dateto], [MeterStatus], [SourceLevel], [CreateDate], [EdiRateClassId], [EdiLoadProfileId], [AMSIndicator]
FROM saes_Spark..Meter src
WHERE
  src.MeterID IN (SELECT MeterId FROM saes_Spark..Meter WHERE PremID IN (SELECT PremID FROM saes_Spark..Premise WHERE CustId = @CustId))
  AND NOT EXISTS(SELECT 1 FROM daes_Spark..Meter dst WHERE src.MeterId = dst.MeterId )

SET IDENTITY_INSERT daes_Spark..Meter OFF

PRINT 'Copy RateIndexType'
SET IDENTITY_INSERT daes_Spark..RateIndexType ON
INSERT INTO daes_Spark..RateIndexType
( [RateIndexTypeID], [RateIndexType], [Active])
SELECT
 [RateIndexTypeID], [RateIndexType], [Active]
FROM saes_Spark..RateIndexType src
WHERE
    src.RateIndexTypeID IN (
        SELECT CONVERT(INT, FixedCapRate) FROM saes_Spark..RateDetail WHERE FixedCapRate IS NOT NULL AND RateId IN (
            SELECT RateId FROM saes_Spark..Customer WHERE CustId = @CustId
            UNION
            SELECT RateID FROM saes_Spark..Product WHERE ProductId IN (
                SELECT ProductID FROM saes_Spark..Contract WHERE CustID = @CustID
                UNION
                SELECT ProductId FROM saes_Spark.ClientCustomer.ChangeProductRequest WHERE CustId = @CustId
                UNION
                SELECT SegmentationRolloverProductId FROM saes_Spark.ClientCustomer.ChangeProductRequest WHERE CustId = @CustId
            )
        ) 
    )
    AND NOT EXISTS(SELECT 1 FROM daes_Spark..RateIndexType dst WHERE src.RateIndexTypeID = dst.RateIndexTypeID )

SET IDENTITY_INSERT daes_Spark..RateIndexType OFF

PRINT 'Copy RateIndexRange'
SET IDENTITY_INSERT daes_Spark..RateIndexRange ON
INSERT INTO daes_Spark..RateIndexRange
( [RateIndexRangeID], [RateIndexTypeID], [DateFrom], [DateTo], [IndexRate])
SELECT
 [RateIndexRangeID], [RateIndexTypeID], [DateFrom], [DateTo], [IndexRate]
FROM saes_Spark..RateIndexRange src
WHERE
    src.RateIndexTypeID IN (
        SELECT CONVERT(INT, FixedCapRate) FROM saes_Spark..RateDetail WHERE FixedCapRate IS NOT NULL AND RateId IN (
            SELECT RateId FROM saes_Spark..Customer WHERE CustId = @CustId
            UNION
            SELECT RateID FROM saes_Spark..Product WHERE ProductId IN (
                SELECT ProductID FROM saes_Spark..Contract WHERE CustID = @CustID
                UNION
                SELECT ProductId FROM saes_Spark.ClientCustomer.ChangeProductRequest WHERE CustId = @CustId
                UNION
                SELECT SegmentationRolloverProductId FROM saes_Spark.ClientCustomer.ChangeProductRequest WHERE CustId = @CustId
            )
        )
    ) 
    AND NOT EXISTS(SELECT 1 FROM daes_Spark..RateIndexRange dst WHERE src.RateIndexRangeID = dst.RateIndexRangeID )

SET IDENTITY_INSERT daes_Spark..RateIndexRange OFF

PRINT 'Copy ClientCustomer.ChangeProductRequest'
SET IDENTITY_INSERT daes_Spark.ClientCustomer.ChangeProductRequest ON
INSERT INTO daes_Spark.ClientCustomer.ChangeProductRequest
( [ChangeProductRequestID], [CustId], [ProductId], [ProductChangeType], [ProductEffectiveDate], [PromoCode], [SalesAgent], [MarketerCode], [ContractPath], [ReferAFriendToken], [SoldDate], [SegmentationLabel], [SegmentationAdjustmentToEnergyRate], [SegmentationRolloverProductId], [RequestedContractStartDate], [ContractTermsInMonths], [ActualContractStartDate], [DeletedFlag], [CTRRequestId], [ContractId], [EarlyTerminationFee], [LDCRateCode], [CreateDate], [MeterReadDate], [BufferDate])
SELECT
 [ChangeProductRequestID], [CustId], [ProductId], [ProductChangeType], [ProductEffectiveDate], [PromoCode], [SalesAgent], [MarketerCode], [ContractPath], [ReferAFriendToken], [SoldDate], [SegmentationLabel], [SegmentationAdjustmentToEnergyRate], [SegmentationRolloverProductId], [RequestedContractStartDate], [ContractTermsInMonths], [ActualContractStartDate], [DeletedFlag], [CTRRequestId], NULL/*[ContractId]*/, [EarlyTerminationFee], [LDCRateCode], [CreateDate], [MeterReadDate], [BufferDate]
FROM saes_Spark.ClientCustomer.ChangeProductRequest src
WHERE
  src.CustId = @CustId
  AND NOT EXISTS(SELECT 1 FROM daes_Spark.ClientCustomer.ChangeProductRequest dst WHERE src.ChangeProductRequestID = dst.ChangeProductRequestID )

SET IDENTITY_INSERT daes_Spark.ClientCustomer.ChangeProductRequest OFF

PRINT 'Copy ClientCorrelation.CorrelationRequest'
SET IDENTITY_INSERT daes_Spark.ClientCorrelation.CorrelationRequest ON
INSERT INTO daes_Spark.ClientCorrelation.CorrelationRequest
( [CorrelationRequestID], [ClientRequestID], [IstaCorrelationID], [RequestSourceType], [RequestSourceID], [CustomerAccountNumber], [RequestDate], [RequestMessage], [BatchRequestId], [IsSuccess])
SELECT
 [CorrelationRequestID], [ClientRequestID], [IstaCorrelationID], [RequestSourceType], [RequestSourceID], [CustomerAccountNumber], [RequestDate], [RequestMessage], [BatchRequestId], [IsSuccess]
FROM saes_Spark.ClientCorrelation.CorrelationRequest src
WHERE
  src.CustomerAccountNumber = @CustNo 
  AND NOT EXISTS(SELECT 1 FROM daes_Spark.ClientCorrelation.CorrelationRequest dst WHERE src.CorrelationRequestID = dst.CorrelationRequestID )

SET IDENTITY_INSERT daes_Spark.ClientCorrelation.CorrelationRequest OFF


PRINT 'Copy ClientCustomer.CustomerCommission'
SET IDENTITY_INSERT daes_Spark.ClientCustomer.CustomerCommission ON
INSERT INTO daes_Spark.ClientCustomer.CustomerCommission
( [CustomerCommissionId], [CustId], [ProductId], [BrokerCode], [BrokerName], [ContractPromoCode], [MarketerCode], [SalesChannelCode], [UtilityDuns], [CreateDate], [AggregatorFee])
SELECT
 [CustomerCommissionId], [CustId], [ProductId], [BrokerCode], [BrokerName], [ContractPromoCode], [MarketerCode], [SalesChannelCode], [UtilityDuns], [CreateDate], [AggregatorFee]
FROM saes_Spark.ClientCustomer.CustomerCommission src
WHERE
  src.CustId = @CustId 
  AND NOT EXISTS(SELECT 1 FROM daes_Spark.ClientCustomer.CustomerCommission dst WHERE src.CustomerCommissionId = dst.CustomerCommissionId )

SET IDENTITY_INSERT daes_Spark.ClientCustomer.CustomerCommission OFF

PRINT 'Copy ClientCustomer.ChangeProductRequest'
SET IDENTITY_INSERT daes_Spark.ClientCustomer.ChangeProductRequest ON
INSERT INTO daes_Spark.ClientCustomer.ChangeProductRequest
( [ChangeProductRequestID], [CustId], [ProductId], [ProductChangeType], [ProductEffectiveDate], [PromoCode], [SalesAgent], [MarketerCode], [ContractPath], [ReferAFriendToken], [SoldDate], [SegmentationLabel], [SegmentationAdjustmentToEnergyRate], [SegmentationRolloverProductId], [RequestedContractStartDate], [ContractTermsInMonths], [ActualContractStartDate], [DeletedFlag], [CTRRequestId], [ContractId], [EarlyTerminationFee], [LDCRateCode], [CreateDate], [MeterReadDate], [BufferDate])
SELECT
 [ChangeProductRequestID], [CustId], [ProductId], [ProductChangeType], [ProductEffectiveDate], [PromoCode], [SalesAgent], [MarketerCode], [ContractPath], [ReferAFriendToken], [SoldDate], [SegmentationLabel], [SegmentationAdjustmentToEnergyRate], [SegmentationRolloverProductId], [RequestedContractStartDate], [ContractTermsInMonths], [ActualContractStartDate], [DeletedFlag], [CTRRequestId], [ContractId], [EarlyTerminationFee], [LDCRateCode], [CreateDate], [MeterReadDate], [BufferDate]
FROM saes_Spark.ClientCustomer.ChangeProductRequest src
WHERE
  src.CustId = @CustId
  AND NOT EXISTS(SELECT 1 FROM daes_Spark.ClientCustomer.ChangeProductRequest dst WHERE src.ChangeProductRequestID = dst.ChangeProductRequestID )

SET IDENTITY_INSERT daes_Spark.ClientCustomer.ChangeProductRequest OFF

PRINT 'Copy ChangeRequest'
SET IDENTITY_INSERT daes_Spark..ChangeRequest ON
INSERT INTO daes_Spark..ChangeRequest
( [ChangeRequestID], [CustomerID], [PremiseID], [ElementID], [ElementPrimaryKey], [RequiresTransactionFlag], [RequiresApprovalFlag], [ScheduledDate], [StatusID], [CreatedByUserID], [CreateDate], [ModifiedByUserID], [ModifiedDate], [EffectiveDate], [ChangeTransactionID])
SELECT
 [ChangeRequestID], [CustomerID], [PremiseID], [ElementID], [ElementPrimaryKey], [RequiresTransactionFlag], [RequiresApprovalFlag], [ScheduledDate], 1/*[StatusID]*/, [CreatedByUserID], [CreateDate], [ModifiedByUserID], [ModifiedDate], [EffectiveDate], NULL/*[ChangeTransactionID]*/
FROM saes_Spark..ChangeRequest src
WHERE
  src.CustomerId = @CustId
  AND NOT EXISTS(SELECT 1 FROM daes_Spark..ChangeRequest dst WHERE src.ChangeRequestID = dst.ChangeRequestID )

SET IDENTITY_INSERT daes_Spark..ChangeRequest OFF

PRINT 'Copy ChangeRequestDetail'
SET IDENTITY_INSERT daes_Spark..ChangeRequestDetail ON
INSERT INTO daes_Spark..ChangeRequestDetail
( [ChangeRequestDetailID], [ChangeRequestID], [FieldID], [NewValue], [OldValue], [ChangeCode], [SecObjectControlID])
SELECT
 [ChangeRequestDetailID], [ChangeRequestID], [FieldID], [NewValue], [OldValue], [ChangeCode], [SecObjectControlID]
FROM saes_Spark..ChangeRequestDetail src
WHERE
  src.ChangeRequestID IN (SELECT ChangeRequestID FROM saes_Spark..ChangeRequest WHERE CustomerId = @CustId)
  AND NOT EXISTS(SELECT 1 FROM daes_Spark..ChangeRequestDetail dst WHERE src.ChangeRequestID = dst.ChangeRequestID )

SET IDENTITY_INSERT daes_Spark..ChangeRequestDetail OFF


PRINT 'Copy ChangeRequestDetailTransaction'
SET IDENTITY_INSERT daes_Spark..ChangeRequestDetailTransaction ON
INSERT INTO daes_Spark..ChangeRequestDetailTransaction
( [ChangeRequestDetailTransactionID], [ChangeRequestDetailID], [RequestID], [PremiseID])
SELECT
 [ChangeRequestDetailTransactionID], [ChangeRequestDetailID], [RequestID], [PremiseID]
FROM saes_Spark..ChangeRequestDetailTransaction src
WHERE
  src.ChangeRequestDetailID IN (SELECT ChangeRequestDetailID FROM saes_Spark..ChangeRequestDetail WHERE ChangeRequestID IN (SELECT ChangeRequestID FROM saes_Spark..ChangeRequest WHERE CustomerId = @CustId))
  AND NOT EXISTS(SELECT 1 FROM daes_Spark..ChangeRequestDetailTransaction dst WHERE src.ChangeRequestDetailTransactionID = dst.ChangeRequestDetailTransactionID )

SET IDENTITY_INSERT daes_Spark..ChangeRequestDetailTransaction OFF
";
                    try
                    {
                        SqlHelper.ExecuteNonQuery(_appConfig.ConnectionCsr, CommandType.Text, sql);
                        PrepareDataForRTTrigger(custId);
                        ts.Complete();
                    }
                    catch (Exception e)
                    {
                        _logger.Error(e, $"Error Occurred when copying customer {custId}, currentIterationNumber {currentIterationNumber}");
                    }
                    currentIterationNumber++;
                    _logger.Info($"{currentIterationNumber} copied");
                }
            }
        }

        private static void SimulateProductRollOver(List<int> desiredCusList)
        {
            var dataGateway = new DataGateway
            {
                ClientId = 48,
                ConnectionBillingAdmin = _clientConfig.ConnectionBillingAdmin,
                ConnectionCsr = _appConfig.ConnectionCsr,
                UserId = 0
            };
            var context = new ProductRolloverContext(_logger, dataGateway);
            var rollover = new ProductRollover(context);
            var dataset = SqlHelper.ExecuteDataset(_appConfig.ConnectionCsr, CommandType.StoredProcedure, "cspProductRolloverList");
            if (dataset == null || dataset.Tables.Count == 0 || dataset.Tables[0].Rows.Count == 0)
                return;
            
            var lstcustomersForRollover = (from DataRow dr in dataset.Tables[0].Rows
                select new CustomerProductRolloverModel()
                {
                    CustId = CIS.Framework.Data.Utility.GetInt32(dr, "CustId", 0),
                    SoldDate = CIS.Framework.Data.Utility.GetDateTime(dr, "SoldDate", DateTime.MinValue),
                    EndDate = CIS.Framework.Data.Utility.GetDateTime(dr, "EndDate", DateTime.MinValue),
                    IsRollover = CIS.Framework.Data.Utility.GetBool(dr, "RolloverFlag", false),
                    RolloverProductId = CIS.Framework.Data.Utility.GetInt32(dr, "RolloverProductId", 0),
                    SwitchDate = CIS.Framework.Data.Utility.GetDateTime(dr, "StartDate", DateTime.MinValue),
                    CurrentRateTransitionId = CIS.Framework.Data.Utility.GetInt32(dr, "CurrentRateTransitionId", 0),
                }).ToList();

            foreach (var customerProductRolloverModel in lstcustomersForRollover)
            {
                if (!desiredCusList.Contains(customerProductRolloverModel.CustId)) 
                    continue;
                var customer = context.CustomerDataGateway.LoadCustomerInfo(customerProductRolloverModel.CustId);
                CIS.Element.Core.PremiseInfoList listPremises = CIS.Element.Core.PremiseInfoList.Search(customerProductRolloverModel.CustId, "", "", "", true, "");
                if (customer.BillingTypeId == 3 && listPremises.Count == 1) 
                    rollover.Process(customerProductRolloverModel);
            }
        }

        private static void SimulateCustomerContractEvaluation()
        {
            try
            {
                var dataGateway = new DataGateway
                {
                    ClientId = 48,
                    ConnectionBillingAdmin = _clientConfig.ConnectionBillingAdmin,
                    ConnectionCsr = _appConfig.ConnectionCsr,
                    UserId = 0,
                };

                //if (!(Thread.CurrentPrincipal is SecurityPrincipal))
                //Utility.SetSecurityContext(dataGateway.ClientId, dataGateway.ConnectionBillingAdmin, dataGateway.UserId);

                var context = new ChangeProductEvaluationContext(_logger, dataGateway);
                var contractEvalution = new ChangeProductEvaluation(context);

                List<ChangeProductModel> list = contractEvalution.GetCustomersEligibleForContractCreation();
                foreach (var model in list)
                {
                    try
                    {
                        contractEvalution.CreateContract(model);
                    }
                    catch (Exception ex)
                    {
                        _logger.Error(ex,
                            $"Exception Logged for ChangeProductRequest id: {model.Id}, CustomerId: {model.CustId}");
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.Error(ex);
            }
        }

        private static void SimulateCustomerContractEvaluation(List<int> desiredCustList)
        {
            try
            {
                var dataGateway = new DataGateway
                {
                    ClientId = 48,
                    ConnectionBillingAdmin = _clientConfig.ConnectionBillingAdmin,
                    ConnectionCsr = _appConfig.ConnectionCsr,
                    UserId = 0,
                };

                //if (!(Thread.CurrentPrincipal is SecurityPrincipal))
                    //Utility.SetSecurityContext(dataGateway.ClientId, dataGateway.ConnectionBillingAdmin, dataGateway.UserId);

                var context = new ChangeProductEvaluationContext(_logger, dataGateway);
                var contractEvalution = new ChangeProductEvaluation(context);

                List<ChangeProductModel> list = contractEvalution.GetCustomersEligibleForContractCreation();
                foreach (var model in list.Where(x=>x.MarketerCode == "ICC"))
                {
                    if (!desiredCustList.Contains(model.CustId)) 
                        continue;
                
                    try
                    {
                        contractEvalution.CreateContract(model);
                    }
                    catch (Exception ex)
                    {
                        _logger.Error(ex, $"Exception Logged for ChangeProductRequest id: {model.Id}, CustomerId: {model.CustId}");
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.Error(ex);
            }
        }

        private static void SimulateCalcuateNextRateTransitionDate()
        {
            var maintenance = new MyMaintenance(_appConfig.ConnectionCsr, _appConfig.ConnectionMarket, _clientConfig.ConnectionBillingAdmin);
            maintenance.CalcuateNextRateTransitionDate();
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
                ConnectionAdmin = _clientConfig.ConnectionBillingAdmin,
                ConnectionMarket = _appConfig.ConnectionMarket,
                ConnectionCsr = _appConfig.ConnectionCsr,
                ClientID = _clientConfig.ClientId,
                Client = _clientConfig.Client
                
            };
            baseImport.myImportTransaction();
        }

        private static void GenerateEventsFromEventEvaluationQueue()
        {

            var htParams = new Hashtable() { { "EventTypeID", 7 } }; ;

            var gen = new CIS.Framework.Event.EventGenerator.EventQueueEvaluation(_appConfig.ConnectionCsr, _clientConfig.ConnectionBillingAdmin)
            {
                ConnectionStringBillingAdmin = _clientConfig.ConnectionBillingAdmin,
                ConnectionStringCsr = _appConfig.ConnectionCsr,
                Client = _clientConfig.Client,
                ClientID = _clientConfig.ClientId
            };

            gen.Generate(_clientConfig.ClientId, htParams);
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

        private static void GenerateEventsForLetterGeneration()
        {
            var maintenance = new MyMaintenance(_appConfig.ConnectionCsr, _appConfig.ConnectionMarket, _clientConfig.ConnectionBillingAdmin);
            //will create events for all configured eventtype on client
            //maintenance.GenerateEvents();

            //will create renewal letters
            maintenance.GenerateContractRenewalNoticeLetters();
            
            
            //will create email queue
            maintenance.QueueLettersForEmail();
        }

        private static void ProcessEvents()
        {
            var engine = new CIS.Engine.Event.Queue(_clientConfig.ConnectionBillingAdmin);
            engine.ProcessEventQueue(_appConfig.ClientID, _appConfig.ConnectionCsr, _appConfig.ConnectionMarket, _appConfig.ClientAbbreviation);
        }

        private static void PrepareMockDataForLetterGeneration()
        {
            SqlHelper.ExecuteNonQuery(_appConfig.ConnectionCsr, CommandType.Text, "UPDATE config.LDCConfiguration SET SendRenewalNoticeOne = 0 WHERE LDCId IN (SELECT LDCId FROM LDC WHERE MarketID = 1)");
            SqlHelper.ExecuteNonQuery(_appConfig.ConnectionCsr, CommandType.Text, "DELETE FROM Letter WHERE CreateDate > DateAdd(dd,-2,GETDATE()) ");
            SqlHelper.ExecuteNonQuery(_appConfig.ConnectionCsr, CommandType.Text, "UPDATE Premise SET LDCId = 11 WHERE CustID = 1865754");
            SqlHelper.ExecuteNonQuery(_appConfig.ConnectionCsr, CommandType.Text, "UPDATE config.LDCConfiguration SET QueueLettersForEmail = 1 WHERE LDCId = 11");
            SqlHelper.ExecuteNonQuery(_appConfig.ConnectionCsr, CommandType.Text, "UPDATE Customer SET ContractEndDate = '2016-12-30' WHERE CustID = 1865754");
            SqlHelper.ExecuteNonQuery(_appConfig.ConnectionCsr, CommandType.Text, "DELETE FROM Letter WHERE LetterTypeId = 701 AND CustID = 1865754");
            SqlHelper.ExecuteNonQuery(_appConfig.ConnectionCsr, CommandType.Text, "UPDATE Contract SET EndDate = '2016-12-31' WHERE CustID = 1865754");
            SqlHelper.ExecuteNonQuery(_appConfig.ConnectionCsr, CommandType.Text, "UPDATE CustomerAdditionalInfo SET BillingTypeId = 2 WHERE CustID = 1865754");
            SqlHelper.ExecuteNonQuery(_appConfig.ConnectionCsr, CommandType.Text, "UPDATE Address SET Email = 'test@example.com' WHERE AddrID IN (SELECT MailAddrId FROM Customer WHERE CustID = 1865754)");
        }

        private static void RestoreData2OriginalLetterGeneration()
        {
            SqlHelper.ExecuteNonQuery(_appConfig.ConnectionCsr, CommandType.Text, "UPDATE Address SET Email = NULL WHERE AddrID IN (SELECT MailAddrId FROM Customer WHERE CustID = 1865754)");
            SqlHelper.ExecuteNonQuery(_appConfig.ConnectionCsr, CommandType.Text, "UPDATE CustomerAdditionalInfo SET BillingTypeId = 1 WHERE CustID = 1865754");
            SqlHelper.ExecuteNonQuery(_appConfig.ConnectionCsr, CommandType.Text, "UPDATE Contract SET EndDate = '2017-11-30' WHERE CustID = 1865754");
            SqlHelper.ExecuteNonQuery(_appConfig.ConnectionCsr, CommandType.Text, "UPDATE Customer SET ContractEndDate = '2017-11-29' WHERE CustID = 1865754");
            SqlHelper.ExecuteNonQuery(_appConfig.ConnectionCsr, CommandType.Text, "UPDATE config.LDCConfiguration SET QueueLettersForEmail = 0 WHERE LDCId = 11");
            SqlHelper.ExecuteNonQuery(_appConfig.ConnectionCsr, CommandType.Text, "UPDATE Premise SET LDCId = 3 WHERE CustID = 1865754");
            SqlHelper.ExecuteNonQuery(_appConfig.ConnectionCsr, CommandType.Text, "UPDATE config.LDCConfiguration SET SendRenewalNoticeOne = 1 WHERE LDCId IN (SELECT LDCId FROM LDC WHERE MarketID = 1)");
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
                SqlHelper.ExecuteNonQuery(_appConfig.ConnectionCsr, CommandType.Text, sql);
            }
            catch 
            {
            }
            try
            {
                SqlHelper.ExecuteNonQuery(_appConfig.ConnectionCsr, CommandType.Text, sql);
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
            SqlHelper.ExecuteNonQuery(_appConfig.ConnectionCsr, CommandType.Text, sql);
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
            SqlHelper.ExecuteNonQuery(_appConfig.ConnectionCsr, CommandType.Text, sql);
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
            SqlHelper.ExecuteNonQuery(_appConfig.ConnectionCsr, CommandType.Text, sql);
        }

        private static void Change814DSchedule(DateTime scheduleDate)
        {
            string sql = $@"
UPDATE daes_SparkMarket..tbl_814_service SET SpecialReadSwitchDate = '{scheduleDate.ToString("yyyy-MM-dd")}' WHERE [814_Key] = 22748557
UPDATE daes_Spark..CustomerTransactionRequest SET RequestDate = '{scheduleDate.ToString("yyyy-MM-dd")}' WHERE SourceId = 22748557 AND TransactionTypeId = 43
UPDATE daes_BillingAdmin..EventActionQueue SET ScheduledDate = '{scheduleDate.ToString("yyyy-MM-dd")}' WHERE RequestID IN (SELECT RequestID FROM daes_Spark..CustomerTransactionRequest WHERE CustID = 1807055 AND TransactionTypeId = 43) AND ISNULL(ScheduledDate,0) <> 0
";
            SqlHelper.ExecuteNonQuery(_appConfig.ConnectionCsr, CommandType.Text, sql);
        }
    }
}


DECLARE @CustId INT = {0}
DECLARE @MAX_DEPTH INT = {1}

DECLARE @PremIds TABLE (PremId INT)
INSERT INTO @PremIds 
SELECT PremId FROM Premise WHERE CustId = @CustID

DECLARE @EnrollCustIds TABLE (EnrollCustId INT)
INSERT INTO @EnrollCustIds 
SELECT EnrollCustId FROM EnrollCustomer WHERE CsrCustId = @CustID

DECLARE @AcctsRecIDs TABLE (AcctsRecID INT)
INSERT INTO @AcctsRecIDs
SELECT AcctsRecID FROM Customer WHERE CustId = @CustID


SELECT CAST((SELECT * FROM Customer (NOLOCK) WHERE CustId = @CustID ORDER BY 1 FOR XML AUTO) as varchar(MAX))
SELECT CAST((SELECT * FROM Contract (NOLOCK) WHERE CustId = @CustID ORDER BY 1 FOR XML AUTO) as varchar(MAX))
SELECT CAST((SELECT * FROM Premise (NOLOCK) WHERE CustId = @CustID ORDER BY 1 FOR XML AUTO) as varchar(MAX))
SELECT CAST((SELECT * FROM CustomerAdditionalInfo (NOLOCK) WHERE CustId = @CustID FOR XML AUTO) as varchar(MAX))
SELECT CAST((SELECT * FROM AccountsReceivable (NOLOCK) WHERE AcctsRecID IN (SELECT AcctsRecId FROM @AcctsRecIDs) FOR XML AUTO) as varchar(MAX))
SELECT CAST((SELECT * FROM Meter (NOLOCK) WHERE PremId IN (SELECT PremId FROM @PremIds) ORDER BY 1 FOR XML AUTO) as varchar(MAX))
SELECT CAST((SELECT * FROM EdiLoadProfile (NOLOCK) WHERE EdiLoadProfileId IN (SELECT EdiLoadProfileId FROM Meter WHERE PremId IN (SELECT PremId FROM @PremIds)) ORDER BY 1 FOR XML AUTO) as varchar(MAX))
SELECT CAST((SELECT * FROM EdiRateClass (NOLOCK) WHERE EdiRateClassId IN (SELECT EdiRateClassId FROM Meter WHERE PremId IN (SELECT PremId FROM @PremIds)) ORDER BY 1 FOR XML AUTO) as varchar(MAX))

SELECT CAST((SELECT * FROM Address (NOLOCK) WHERE AddrId IN (
	    SELECT SiteAddrId			FROM Customer (NOLOCK) WHERE CustID = @CustID
        UNION
        SELECT MailAddrId			FROM Customer (NOLOCK) WHERE CustID = @CustID
        UNION
        SELECT CorrAddrId			FROM Customer (NOLOCK) WHERE CustID = @CustID
        UNION
        SELECT SpecialNeedsAddrId	FROM Customer (NOLOCK) WHERE CustID = @CustID
        UNION
        SELECT PowerOutageAddrId	FROM Customer (NOLOCK) WHERE CustID = @CustID
        UNION
        SELECT AddrId				FROM Premise (NOLOCK) WHERE CustID = @CustID
    ) 
ORDER BY 1 FOR XML AUTO ) as varchar(MAX))

SELECT CAST((SELECT * FROM RateTransition (NOLOCK) WHERE CustId = @CustID  ORDER BY 1 FOR XML AUTO ) as varchar(MAX))
SELECT CAST((SELECT * FROM CustomerTransactionRequest (NOLOCK) WHERE CustID = @CustID ORDER BY 1 FOR XML AUTO ) as varchar(MAX))
SELECT CAST((SELECT * FROM EnrollCustomer (NOLOCK) WHERE EnrollCustID IN (SELECT EnrollCustId FROM @EnrollCustIds) ORDER BY 1 FOR XML AUTO ) as varchar(MAX))
SELECT CAST((SELECT * FROM EnrollCustomerPremise (NOLOCK) WHERE EnrollCustID IN (SELECT EnrollCustId FROM @EnrollCustIds) ORDER BY 1 FOR XML AUTO ) as varchar(MAX))
SELECT CAST((SELECT * FROM EnrollCustomerDocuments (NOLOCK) WHERE EnrollCustID IN (SELECT EnrollCustId FROM @EnrollCustIds) ORDER BY 1 FOR XML AUTO ) as varchar(MAX))
SELECT CAST((SELECT * FROM EnrollCustomerNote (NOLOCK) WHERE EnrollCustID IN (SELECT EnrollCustId FROM @EnrollCustIds) ORDER BY 1 FOR XML AUTO ) as varchar(MAX))
--SELECT CAST((SELECT * FROM EnrollCustomerPremiseDeposit (NOLOCK) WHERE EnrollCustID IN (SELECT EnrollCustId FROM @EnrollCustIds) ORDER BY 1 FOR XML AUTO ) as varchar(MAX))
SELECT CAST((SELECT * FROM EnrollCustomerPremiseTaxPercentage (NOLOCK) WHERE EnrollPremiseId IN (SELECT EnrollPremiseID FROM EnrollCustomerPremise WHERE EnrollCustID IN (SELECT EnrollCustId FROM @EnrollCustIds)) ORDER BY 1 FOR XML AUTO ) as varchar(MAX))
--SELECT CAST((SELECT * FROM EnrollCustomerEnrollStatusHistory (NOLOCK) WHERE EnrollCustId IN (SELECT EnrollCustId FROM @EnrollCustIds) ORDER BY 1 FOR XML AUTO ) as varchar(MAX))
SELECT CAST((SELECT * FROM Consumption (NOLOCK) WHERE MeterId IN (SELECT MeterId FROM Meter WHERE PremId IN (SELECT PremId FROM @PremIds))  ORDER BY 1 FOR XML AUTO ) as varchar(MAX))
SELECT CAST((SELECT * FROM Invoice (NOLOCK) Where CustId = @CustID ORDER BY 1 FOR XML AUTO ) as varchar(MAX))
SELECT CAST((SELECT InvoiceXMLID, InvoiceID, '<hint>NoNeedForThisTestCase</hint>' as InvoiceXML, CreateDate, PrintFormatID, PrintStatusID, PhysicalFile, PDFContent, HasPDF, MachineName FROM InvoiceXML (NOLOCK) WHERE InvoiceId IN (SELECT InvoiceId FROM Invoice WHERE CustId = @CustID) ORDER BY 1 FOR XML AUTO ) as varchar(MAX))
SELECT CAST((SELECT * FROM InvoiceLog WHERE InvLogId IN (SELECT InvLogId from Invoice (NOLOCK) Where CustId = @CustID)  ORDER BY 1 FOR XML AUTO ) as varchar(MAX))
SELECT CAST((SELECT * FROM AccountsReceivableHistory (NOLOCK) WHERE CustId = @CustID  ORDER BY 1 FOR XML AUTO ) as varchar(MAX))
SELECT CAST((SELECT * FROM InvoiceDetail (NOLOCK) WHERE InvoiceID IN (SELECT InvoiceId from Invoice Where CustId = @CustID)  ORDER BY 1 FOR XML AUTO ) as varchar(MAX))
SELECT CAST((SELECT * FROM ConsumptionDetail (NOLOCK) WHERE ConsDetId IN (SELECT ConsDetID FROM InvoiceDetail WHERE InvoiceID IN (SELECT InvoiceId from Invoice Where CustId = @CustID) )  ORDER BY 1 FOR XML AUTO ) as varchar(MAX))
SELECT CAST((SELECT * FROM TdspInvoice (NOLOCK) WHERE InvoiceId IN (SELECT InvoiceId from Invoice Where CustId = @CustID) AND RequestID IN (SELECT RequestID FROM CustomerTransactionRequest WHERE CustID = @CustID)  ORDER BY 1 FOR XML AUTO ) as varchar(MAX))

DECLARE @Rates TABLE (RateId INT);
DECLARE @Products2BeCopied TABLE (productId INT);

INSERT INTO @Rates
SELECT RateId FROM (
SELECT RateId FROM Customer (NOLOCK) WHERE CustId = @CustID
UNION
SELECT RateId FROM RateTransition (NOLOCK) WHERE CustId = @CustID
UNION
SELECT RateId FROM Product (NOLOCK) WHERE ProductID IN (
	SELECT ProductID FROM EnrollCustomerPremise (NOLOCK) WHERE EnrollCustID IN (SELECT EnrollCustId FROM @EnrollCustIds)
	UNION
	SELECT ProductID FROM ClientCustomer.ChangeProductRequest (NOLOCK) WHERE CustId = @CustId
	UNION
	SELECT SegmentationRolloverProductId FROM ClientCustomer.ChangeProductRequest (NOLOCK) WHERE CustId = @CustId
	UNION
	SELECT ProductId FROM Contract (NOLOCK) WHERE CustId = @CustId
	UNION
	SELECT RolloverProductId FROM ClientCustomer.CustomerAdditionalInfo (NOLOCK) WHERE CustId = @CustId
)
) r
GROUP BY r.RateId

INSERT INTO @Products2BeCopied
SELECT ProductId FROM (
SELECT ProductId FROM Product (NOLOCK) WHERE RateId IN (SELECT RateId from RateTransition WHERE CustId = @CustID) 
UNION
SELECT ProductID FROM EnrollCustomerPremise (NOLOCK) WHERE EnrollCustID IN (SELECT EnrollCustId FROM @EnrollCustIds)
UNION
SELECT ProductID FROM ClientCustomer.ChangeProductRequest (NOLOCK) WHERE CustId = @CustId
UNION
SELECT SegmentationRolloverProductId FROM ClientCustomer.ChangeProductRequest (NOLOCK) WHERE CustId = @CustId
UNION
SELECT ProductId FROM Contract (NOLOCK) WHERE CustId = @CustId
UNION
SELECT RolloverProductId FROM ClientCustomer.CustomerAdditionalInfo (NOLOCK) WHERE CustId = @CustId
) p
GROUP BY p.ProductId


DECLARE @FirstProductId INT
DECLARE @Products TABLE (cpProductId INT, cpRollOverProductID INT)

DECLARE c CURSOR FOR 
	SELECT ProductID FROM @Products2BeCopied

OPEN c
FETCH NEXT FROM c INTO @FirstProductId
WHILE @@FETCH_STATUS = 0
BEGIN

	DECLARE @CURR_PRODUCT_ID INT = 0
	DECLARE @PREV_PRODUCT_ID INT = @FirstProductId
	DECLARE @RATE_ID INT = 0
	DECLARE @CTR INT = 0

	WHILE @CTR < @MAX_DEPTH
	BEGIN
		SELECT @RATE_ID = RateId, @CURR_PRODUCT_ID = RollOverProductId FROM Product (NOLOCK) WHERE ProductId = @PREV_PRODUCT_ID;
		IF NOT EXISTS(SELECT 1 FROM @Rates WHERE RateId = @RATE_ID)
		BEGIN
			INSERT INTO @Rates VALUES (@RATE_ID)
		END
	
		IF ISNULL(@CURR_PRODUCT_ID, @PREV_PRODUCT_ID)  = @PREV_PRODUCT_ID
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
	FETCH NEXT FROM c INTO @FirstProductId
END
CLOSE c
DEALLOCATE c

SELECT CAST((SELECT * FROM Rate (NOLOCK) WHERE RateId IN (SELECT RateId FROM @Rates) ORDER BY 1 FOR XML AUTO ) as varchar(MAX))
SELECT CAST((SELECT * FROM RateDetail (NOLOCK) WHERE RateId IN (SELECT RateId FROM @Rates) ORDER BY 1 FOR XML AUTO ) as varchar(MAX))
SELECT CAST((SELECT * FROM Product (NOLOCK) WHERE ProductID IN (SELECT cpProductId FROM @Products) ORDER BY 1 FOR XML AUTO ) as varchar(MAX))
SELECT CAST((SELECT * FROM RateIndexRange (NOLOCK) WHERE RateIndexTypeID IN (SELECT CAST(FixedCapRate as INT) FROM RateDetail WHERE RateID IN (SELECT RateId FROM @Rates)) ORDER BY 1 FOR XML AUTO ) as varchar(MAX))
SELECT CAST((SELECT * FROM RateIndexType (NOLOCK) WHERE RateIndexTypeId IN (SELECT CAST(FixedCapRate as INT) FROM RateDetail WHERE RateID IN (SELECT RateId FROM @Rates)) ORDER BY 1 FOR XML AUTO ) as varchar(MAX))


SELECT CAST((SELECT * FROM ClientCustomer.ChangeProductRequest (NOLOCK) WHERE CustId = @CustID ORDER BY 1 FOR XML AUTO ) as varchar(MAX))
SELECT CAST((SELECT * FROM ClientCustomer.ChangeProductRequestBillingDeterminant (NOLOCK) WHERE ChangeProductRequestID IN (SELECT ChangeProductRequestID FROM ClientCustomer.ChangeProductRequest (NOLOCK) WHERE CustId = @CustID) ORDER BY 1 FOR XML AUTO ) as varchar(MAX))
SELECT CAST((SELECT * FROM ClientCustomer.ChangeProductRequestCustomerCommission (NOLOCK) WHERE ChangeProductRequestID IN (SELECT ChangeProductRequestID FROM ClientCustomer.ChangeProductRequest (NOLOCK) WHERE CustId = @CustID) ORDER BY 1 FOR XML AUTO ) as varchar(MAX))

SELECT CAST((SELECT * FROM ClientCustomer.RateTransition (NOLOCK) WHERE RateTransitionId IN (SELECT RateTransitionID  FROM RateTransition (NOLOCK)  WHERE CustId = @CustID ) ORDER BY 1 FOR XML AUTO ) as varchar(MAX))
SELECT CAST((SELECT RateTransitionHistoryID, RateTransitionId, ProductChangeType, LDCRateCode, NextRateTransitionDate, SegmentationLabel, SegmentationAdjustmentToEnergyRate, MeterReadDate, DesiredStartDate, IPAddress, HostName, ApplicationName, LoginName, '' SQLtext, Operation, ChangedOn FROM ClientCustomer.RateTransitionHistory (NOLOCK) WHERE RateTransitionId IN (SELECT RateTransitionID  FROM RateTransition (NOLOCK)  WHERE CustId = @CustID ) ORDER BY 1 FOR XML AUTO ) as varchar(MAX))

SELECT CAST((SELECT * FROM ClientCustomer.Contract (NOLOCK) WHERE ContractId IN (SELECT ContractId FROM Contract WHERE CustId = @CustId) ORDER BY 1 FOR XML AUTO ) as varchar(MAX))
SELECT CAST((SELECT * FROM ClientCustomer.CustomerAdditionalInfo (NOLOCK) WHERE CustId = @CustId ORDER BY 1 FOR XML AUTO ) as varchar(MAX))
SELECT CAST((SELECT * FROM ClientCustomer.CustomerBillingDeterminant (NOLOCK) WHERE CustId = @CustId ORDER BY 1 FOR XML AUTO ) as varchar(MAX))
SELECT CAST((SELECT * FROM ClientCustomer.CustomerCommission (NOLOCK) WHERE CustId = @CustId ORDER BY 1 FOR XML AUTO ) as varchar(MAX))

-- add whatever you want to copy
-- be carefull !!!

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


SELECT CAST((SELECT * FROM Customer WHERE CustId = @CustID ORDER BY 1 FOR XML AUTO) as varchar(MAX))
SELECT CAST((SELECT * FROM Premise WHERE CustId = @CustID ORDER BY 1 FOR XML AUTO) as varchar(MAX))
SELECT CAST((SELECT * FROM CustomerAdditionalInfo WHERE CustId = @CustID FOR XML AUTO) as varchar(MAX))
SELECT CAST((SELECT * FROM AccountsReceivable WHERE AcctsRecID IN (SELECT AcctsRecId FROM @AcctsRecIDs) FOR XML AUTO) as varchar(MAX))
SELECT CAST((SELECT * FROM Meter WHERE PremId IN (SELECT PremId FROM @PremIds) ORDER BY 1 FOR XML AUTO) as varchar(MAX))
SELECT CAST((SELECT * FROM EdiLoadProfile WHERE EdiLoadProfileId IN (SELECT EdiLoadProfileId FROM Meter WHERE PremId IN (SELECT PremId FROM @PremIds)) ORDER BY 1 FOR XML AUTO) as varchar(MAX))
SELECT CAST((SELECT * FROM EdiRateClass WHERE EdiRateClassId IN (SELECT EdiRateClassId FROM Meter WHERE PremId IN (SELECT PremId FROM @PremIds)) ORDER BY 1 FOR XML AUTO) as varchar(MAX))

SELECT CAST((SELECT * FROM Address WHERE AddrId IN (
	    SELECT SiteAddrId			FROM Customer WHERE CustID = @CustID
        UNION
        SELECT MailAddrId			FROM Customer WHERE CustID = @CustID
        UNION
        SELECT CorrAddrId			FROM Customer WHERE CustID = @CustID
        UNION
        SELECT SpecialNeedsAddrId	FROM Customer WHERE CustID = @CustID
        UNION
        SELECT PowerOutageAddrId	FROM Customer WHERE CustID = @CustID
        UNION
        SELECT AddrId				FROM Premise WHERE CustID = @CustID
    ) 
ORDER BY 1 FOR XML AUTO ) as varchar(MAX))

SELECT CAST((SELECT * FROM RateTransition WHERE CustId = @CustID  ORDER BY 1 FOR XML AUTO ) as varchar(MAX))
SELECT CAST((SELECT * FROM CustomerTransactionRequest WHERE CustID = @CustID ORDER BY 1 FOR XML AUTO ) as varchar(MAX))
SELECT CAST((SELECT * FROM EnrollCustomer WHERE EnrollCustID IN (SELECT EnrollCustId FROM @EnrollCustIds) ORDER BY 1 FOR XML AUTO ) as varchar(MAX))
SELECT CAST((SELECT * FROM EnrollCustomerPremise WHERE EnrollCustID IN (SELECT EnrollCustId FROM @EnrollCustIds) ORDER BY 1 FOR XML AUTO ) as varchar(MAX))
SELECT CAST((SELECT * FROM EnrollCustomerDocuments WHERE EnrollCustID IN (SELECT EnrollCustId FROM @EnrollCustIds) ORDER BY 1 FOR XML AUTO ) as varchar(MAX))
SELECT CAST((SELECT * FROM EnrollCustomerNote WHERE EnrollCustID IN (SELECT EnrollCustId FROM @EnrollCustIds) ORDER BY 1 FOR XML AUTO ) as varchar(MAX))
SELECT CAST((SELECT * FROM EnrollCustomerPremiseDeposit WHERE EnrollCustID IN (SELECT EnrollCustId FROM @EnrollCustIds) ORDER BY 1 FOR XML AUTO ) as varchar(MAX))
SELECT CAST((SELECT * FROM EnrollCustomerPremiseTaxPercentage WHERE EnrollPremiseId IN (SELECT EnrollPremiseID FROM EnrollCustomerPremise WHERE EnrollCustID IN (SELECT EnrollCustId FROM @EnrollCustIds)) ORDER BY 1 FOR XML AUTO ) as varchar(MAX))
SELECT CAST((SELECT * FROM EnrollCustomerEnrollStatusHistory WHERE EnrollCustId IN (SELECT EnrollCustId FROM @EnrollCustIds) ORDER BY 1 FOR XML AUTO ) as varchar(MAX))
SELECT CAST((SELECT * FROM Consumption WHERE MeterId IN (SELECT MeterId FROM Meter WHERE PremId IN (SELECT PremId FROM @PremIds))  ORDER BY 1 FOR XML AUTO ) as varchar(MAX))
SELECT CAST((SELECT * FROM Invoice Where CustId = @CustID ORDER BY 1 FOR XML AUTO ) as varchar(MAX))
SELECT CAST((SELECT InvoiceXMLID, InvoiceID, '<hint>NoNeedForThisTestCase</hint>' as InvoiceXML, CreateDate, PrintFormatID, PrintStatusID, PhysicalFile, PDFContent, HasPDF, MachineName FROM InvoiceXML WHERE InvoiceId IN (SELECT InvoiceId FROM Invoice WHERE CustId = @CustID) ORDER BY 1 FOR XML AUTO ) as varchar(MAX))
SELECT CAST((SELECT * FROM InvoiceLog WHERE InvLogId IN (SELECT InvLogId from Invoice Where CustId = @CustID)  ORDER BY 1 FOR XML AUTO ) as varchar(MAX))
SELECT CAST((SELECT * FROM AccountsReceivableHistory WHERE CustId = @CustID  ORDER BY 1 FOR XML AUTO ) as varchar(MAX))
SELECT CAST((SELECT * FROM InvoiceDetail WHERE InvoiceID IN (SELECT InvoiceId from Invoice Where CustId = @CustID)  ORDER BY 1 FOR XML AUTO ) as varchar(MAX))
SELECT CAST((SELECT * FROM ConsumptionDetail WHERE
	ConsDetId IN (SELECT ConsDetID FROM InvoiceDetail WHERE InvoiceID IN (SELECT InvoiceId from Invoice Where CustId = @CustID)) 
	OR ConsId IN (SELECT ConsId FROM Consumption WHERE MeterId IN (SELECT MeterId FROM Meter WHERE PremId IN (SELECT PremId FROM @PremIds)) )  ORDER BY 1 FOR XML AUTO ) as varchar(MAX))

SELECT CAST((SELECT * FROM TdspInvoice WHERE InvoiceId IN (SELECT InvoiceId from Invoice Where CustId = @CustID) AND RequestID IN (SELECT RequestID FROM CustomerTransactionRequest WHERE CustID = @CustID) ORDER BY 1 FOR XML AUTO ) as varchar(MAX))

SELECT CAST((SELECT * FROM TDSPCharges WHERE InvoiceId IN (SELECT InvoiceId from Invoice Where CustId = @CustID) OR ESIID IN (SELECT PremNo FROM Premise WHERE PremID IN (SELECT PremId FROM @PremIds)) ORDER BY 1 FOR XML AUTO ) as varchar(MAX))
SELECT CAST((SELECT * FROM CustomerDddcFactor WHERE Premid IN (SELECT PremId FROM @PremIds) ORDER BY 1 FOR XML AUTO ) as varchar(MAX))
SELECT CAST((SELECT * FROM ARAdjustment WHERE CustId = @CustId ORDER BY 1 FOR XML AUTO ) as varchar(MAX))
SELECT CAST((SELECT * FROM AccountsReceivableHistory WHERE CustId = @CustId ORDER BY 1 FOR XML AUTO ) as varchar(MAX))
SELECT CAST((SELECT * FROM InvoiceSpecialCharges WHERE CustId = @CustId ORDER BY 1 FOR XML AUTO ) as varchar(MAX))
SELECT CAST((SELECT * FROM PaymentDetail WHERE CustId = @CustId ORDER BY 1 FOR XML AUTO ) as varchar(MAX))
SELECT CAST((SELECT * FROM Payment WHERE EXISTS(SELECT 1 FROM PaymentDetail WHERE PaymentId = Payment.PaymentId AND CustId = @CustId) ORDER BY 1 FOR XML AUTO ) as varchar(MAX))

DECLARE @Rates TABLE (RateId INT);
DECLARE @Products2BeCopied TABLE (productId INT);

INSERT INTO @Rates
SELECT RateId FROM (
SELECT RateId FROM Customer WHERE CustId = @CustID
UNION
SELECT RateId FROM RateTransition WHERE CustId = @CustID
UNION
SELECT RateId FROM Product WHERE ProductID IN (SELECT ProductID FROM EnrollCustomerPremise WHERE EnrollCustID IN (SELECT EnrollCustId FROM @EnrollCustIds))
) r
GROUP BY r.RateId

INSERT INTO @Products2BeCopied
SELECT ProductId FROM (
SELECT ProductId FROM Product WHERE RateId IN (SELECT RateId from RateTransition WHERE CustId = @CustID) 
UNION
SELECT ProductID FROM EnrollCustomerPremise WHERE EnrollCustID IN (SELECT EnrollCustId FROM @EnrollCustIds)
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
		SELECT @RATE_ID = RateId, @CURR_PRODUCT_ID = RollOverProductId FROM Product WHERE ProductId = @PREV_PRODUCT_ID;
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

SELECT CAST((SELECT * FROM Rate WHERE RateId IN (SELECT RateId FROM @Rates) ORDER BY 1 FOR XML AUTO ) as varchar(MAX))
SELECT CAST((SELECT * FROM RateDetail WHERE RateId IN (SELECT RateId FROM @Rates) ORDER BY 1 FOR XML AUTO ) as varchar(MAX))
SELECT CAST((SELECT * FROM Product WHERE ProductID IN (SELECT cpProductId FROM @Products) ORDER BY 1 FOR XML AUTO ) as varchar(MAX))
SELECT CAST((SELECT * FROM RateIndexRange WHERE RateIndexTypeID IN (SELECT CAST(FixedCapRate as INT) FROM RateDetail WHERE RateID IN (SELECT RateId FROM @Rates)) ORDER BY 1 FOR XML AUTO ) as varchar(MAX))
SELECT CAST((SELECT * FROM RateIndexType WHERE RateIndexTypeId IN (SELECT CAST(FixedCapRate as INT) FROM RateDetail WHERE RateID IN (SELECT RateId FROM @Rates)) ORDER BY 1 FOR XML AUTO ) as varchar(MAX))

-- add whatever you want to copy
-- be carefull !!!
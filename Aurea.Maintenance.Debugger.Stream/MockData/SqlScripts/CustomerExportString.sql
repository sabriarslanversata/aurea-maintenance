DECLARE @CustId INT = {0}

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

SELECT CAST((SELECT * FROM Rate WHERE RateId IN (
SELECT RateId FROM Customer WHERE CustId = @CustID
UNION
SELECT RateId FROM RateTransition WHERE CustId = @CustID
UNION
SELECT RateId FROM Product WHERE ProductID IN (SELECT ProductID FROM EnrollCustomerPremise WHERE EnrollCustID IN (SELECT EnrollCustId FROM @EnrollCustIds))
) 
ORDER BY 1 FOR XML AUTO ) as varchar(MAX))

SELECT CAST((SELECT * FROM RateDetail WHERE RateId IN (
SELECT RateId from Customer where CustId = @CustID
UNION
SELECT RateId from RateTransition WHERE CustId = @CustID
UNION
SELECT RateId FROM Product WHERE ProductID IN (SELECT ProductID FROM EnrollCustomerPremise WHERE EnrollCustID IN (SELECT EnrollCustId FROM @EnrollCustIds))
) 
ORDER BY 1 FOR XML AUTO ) as varchar(MAX))

SELECT CAST((SELECT * FROM RateTransition WHERE CustId = @CustID  ORDER BY 1 FOR XML AUTO ) as varchar(MAX))

SELECT CAST((SELECT * FROM Product WHERE
 RateId IN (SELECT RateId from RateTransition WHERE CustId = @CustID) 
 OR ProductID IN (SELECT ProductID FROM EnrollCustomerPremise WHERE EnrollCustID IN (SELECT EnrollCustId FROM @EnrollCustIds)) 
ORDER BY 1 FOR XML AUTO ) as varchar(MAX))

SELECT CAST((SELECT * FROM RateIndexRange WHERE RateIndexTypeID IN (SELECT CAST(FixedCapRate as INT) FROM RateDetail WHERE RateID IN (
SELECT RateId from Customer where CustId = @CustID
UNION
SELECT RateId from RateTransition WHERE CustId = @CustID
UNION
SELECT RateId FROM Product WHERE ProductID IN (SELECT ProductID FROM EnrollCustomerPremise WHERE EnrollCustID IN (SELECT EnrollCustId FROM @EnrollCustIds))
)) 
ORDER BY 1 FOR XML AUTO ) as varchar(MAX))

SELECT CAST((SELECT * FROM RateIndexType WHERE RateIndexTypeId IN (SELECT CAST(FixedCapRate as INT) FROM RateDetail WHERE RateID IN (
SELECT RateId from Customer where CustId = @CustID
UNION
SELECT RateId from RateTransition WHERE CustId = @CustID
UNION
SELECT RateId FROM Product WHERE ProductID IN (SELECT ProductID FROM EnrollCustomerPremise WHERE EnrollCustID IN (SELECT EnrollCustId FROM @EnrollCustIds))
)) 
ORDER BY 1 FOR XML AUTO ) as varchar(MAX))

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
SELECT CAST((SELECT InvoiceXMLID, InvoiceID, '<hint>NoNeedForThisTestCase</hint>' as InvoiceXML, CreateDate, PrintFormatID, PrintStatusID, PhysicalFile, PDFContent, HasPDF, MachineName 
FROM InvoiceXML WHERE InvoiceId IN (SELECT InvoiceId FROM Invoice WHERE CustId = @CustID) ORDER BY 1 FOR XML AUTO ) as varchar(MAX))

SELECT CAST((SELECT * FROM InvoiceLog WHERE InvLogId IN (SELECT InvLogId from Invoice Where CustId = @CustID)  ORDER BY 1 FOR XML AUTO ) as varchar(MAX))
SELECT CAST((SELECT * FROM AccountsReceivableHistory WHERE CustId = @CustID  ORDER BY 1 FOR XML AUTO ) as varchar(MAX))
SELECT CAST((SELECT * FROM InvoiceDetail WHERE InvoiceID IN (SELECT InvoiceId from Invoice Where CustId = @CustID)  ORDER BY 1 FOR XML AUTO ) as varchar(MAX))

SELECT CAST((SELECT * FROM ConsumptionDetail WHERE ConsDetId IN (SELECT ConsDetID FROM InvoiceDetail WHERE InvoiceID IN (SELECT InvoiceId from Invoice Where CustId = @CustID) )  ORDER BY 1 FOR XML AUTO ) as varchar(MAX))
SELECT CAST((SELECT * FROM TdspInvoice WHERE InvoiceId IN (SELECT InvoiceId from Invoice Where CustId = @CustID) AND RequestID IN (SELECT RequestID FROM CustomerTransactionRequest WHERE CustID = @CustID)  ORDER BY 1 FOR XML AUTO ) as varchar(MAX))


SELECT CAST((SELECT * FROM Product WHERE ProductId IN (349, 359) FOR XML AUTO ) as varchar(MAX))

SELECT CAST((SELECT * FROM Rate WHERE RateId IN (
SELECT RateId FROM Product WHERE ProductID IN (349, 359)
) 
ORDER BY 1 FOR XML AUTO ) as varchar(MAX))

SELECT CAST((SELECT * FROM RateDetail WHERE RateId IN (
SELECT RateId FROM Product WHERE ProductID IN (349, 359)
) 
ORDER BY 1 FOR XML AUTO ) as varchar(MAX))

SELECT CAST((SELECT * FROM RateIndexRange WHERE RateIndexTypeID IN (SELECT CAST(FixedCapRate as INT) FROM RateDetail WHERE RateID IN (
SELECT RateId FROM Product WHERE ProductID IN (349, 359)
)) 
ORDER BY 1 FOR XML AUTO ) as varchar(MAX))

SELECT CAST((SELECT * FROM RateIndexType WHERE RateIndexTypeId IN (SELECT CAST(FixedCapRate as INT) FROM RateDetail WHERE RateID IN (
SELECT RateId FROM Product WHERE ProductID IN (349, 359)
)) 
ORDER BY 1 FOR XML AUTO ) as varchar(MAX))

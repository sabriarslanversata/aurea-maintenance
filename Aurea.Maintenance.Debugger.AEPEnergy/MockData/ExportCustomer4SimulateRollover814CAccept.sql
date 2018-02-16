DECLARE @CustId INT = {0}
DECLARE @PremId INT = (SELECT PremId FROM Premise WHERE CustId = @CustId)
DECLARE @AcctsRecID INT = (SELECT AcctsRecID FROM Customer WHERE CustId = @CustId)
DECLARE @RateTransitionIdToSimulate INT = (SELECT MIN(RateTransitionId) FROM RateTransition WHERE CustId = @CustId AND RolloverFlag = 1 AND MONTH(CreatedDate) = MONTH(GETDATE()) )
DECLARE @ChangeRequestIdToInvoke INT = (SELECT ChangeRequestId FROM ChangeRequest WHERE ElementId = 6 AND ElementPrimaryKey = @RateTransitionIdToSimulate)

SELECT CAST((SELECT * FROM Customer WHERE CustId = @CustId ORDER BY 1 FOR XML AUTO) as varchar(MAX))
SELECT CAST((SELECT * FROM Premise WHERE CustId = @CustId ORDER BY 1 FOR XML AUTO) as varchar(MAX))
SELECT CAST((SELECT * FROM CustomerAdditionalInfo WHERE CustId = @CustId ORDER BY 1 FOR XML AUTO) as varchar(MAX))
SELECT CAST((SELECT * FROM AccountsReceivable WHERE AcctsRecID = @AcctsRecID ORDER BY 1 FOR XML AUTO) as varchar(MAX))
SELECT CAST((SELECT * FROM Meter WHERE PremId = @PremId ORDER BY 1 FOR XML AUTO) as varchar(MAX))
SELECT CAST((SELECT * FROM Address WHERE AddrId IN (
	    SELECT SiteAddrId	FROM Customer WHERE CustID = @CustId
        UNION
        SELECT MailAddrId	FROM Customer WHERE CustID = @CustId
        UNION
        SELECT CorrAddrId	FROM Customer WHERE CustID = @CustId
        UNION
        SELECT AddrId FROM Premise WHERE CustID = @CustId
    ) ORDER BY 1 
FOR XML AUTO) as varchar(MAX))

SELECT CAST((SELECT * FROM Rate WHERE RateId IN (
SELECT RateId FROM Customer WHERE CustId = @CustId
UNION
SELECT RateId FROM RateTransition WHERE CustId = @CustId
) ORDER BY 1 
FOR XML AUTO) as varchar(MAX))

SELECT CAST((SELECT * FROM RateDetail WHERE RateId IN (
SELECT RateId from Customer where CustId = @CustId
UNION
SELECT RateId from RateTransition WHERE CustId = @CustId
) ORDER BY 1 
FOR XML AUTO) as varchar(MAX))

SELECT CAST((SELECT * FROM RateTransition WHERE CustId = @CustId ORDER BY 1 FOR XML AUTO) as varchar(MAX))
SELECT CAST((SELECT * FROM Product WHERE RateId IN (SELECT RateId from RateTransition WHERE CustId = @CustId) ORDER BY 1 FOR XML AUTO) as varchar(MAX))
SELECT CAST((SELECT * FROM CustomerTransactionRequest WHERE CustID = @CustId ORDER BY 1 FOR XML AUTO) as varchar(MAX))

SELECT CAST((SELECT * FROM Consumption WHERE MeterId IN (SELECT MeterId FROM Meter WHERE PremId = (SELECT PremId FROM Premise WHERE CustId = @CustId)) ORDER BY 1 FOR XML AUTO) as varchar(MAX))
SELECT CAST((SELECT * FROM Invoice Where CustId = @CustId ORDER BY 1 FOR XML AUTO) as varchar(MAX))
SELECT CAST((select InvoiceXMLID, InvoiceID, '<hint>NoNeedForThisTestCase</hint>' as InvoiceXML, CreateDate, PrintFormatID, PrintStatusID, PhysicalFile, PDFContent, HasPDF, MachineName FROM InvoiceXML WHERE InvoiceId IN (SELECT InvoiceId FROM Invoice WHERE CustId = @CustId) ORDER BY 1 FOR XML AUTO) as varchar(MAX))
SELECT CAST((SELECT * FROM InvoiceLog WHERE InvLogId IN (SELECT InvLogId from Invoice Where CustId = @CustId) ORDER BY 1 FOR XML AUTO) as varchar(MAX))
SELECT CAST((SELECT * FROM AccountsReceivableHistory WHERE CustId = @CustId ORDER BY 1 FOR XML AUTO) as varchar(MAX))
SELECT CAST((SELECT * FROM InvoiceDetail WHERE InvoiceID IN (SELECT InvoiceId from Invoice Where CustId = @CustId) ORDER BY 1 FOR XML AUTO) as varchar(MAX))
SELECT CAST((SELECT * FROM ConsumptionDetail WHERE ConsDetId IN (SELECT ConsDetID FROM InvoiceDetail WHERE InvoiceID IN (SELECT InvoiceId from Invoice Where CustId = @CustId) ) ORDER BY 1 FOR XML AUTO) as varchar(MAX))
SELECT CAST((SELECT * FROM ChangeRequest WHERE ChangeRequestId = @ChangeRequestIdToInvoke ORDER BY 1 FOR XML AUTO) as varchar(MAX))
SELECT CAST((SELECT * FROM ChangeRequestDetail WHERE ChangeRequestId = @ChangeRequestIdToInvoke ORDER BY 1 FOR XML AUTO) as varchar(MAX))
SELECT CAST((SELECT * FROM ChangeTransaction WHERE ChangeTransactionId = (SELECT ChangeTransactionId FROM ChangeRequest WHERE ChangeRequestId = @ChangeRequestIdToInvoke) ORDER BY 1 FOR XML AUTO) as varchar(MAX))
SELECT CAST((SELECT * FROM ChangeTransactionData WHERE ChangeTransactionId = (SELECT ChangeTransactionId FROM ChangeRequest WHERE ChangeRequestId = @ChangeRequestIdToInvoke) ORDER BY 1 FOR XML AUTO) as varchar(MAX))


DECLARE @OutboundRequestId INT = (SELECT TOP 1 RequestId FROM CustomerTransactionRequest WHERE CustId = @CustId AND TransactionType = '814' AND ActionCode = 'C' AND Direction = 0 ORDER BY 1 DESC)

SELECT CAST((SELECT * FROM paes_AEPEnergyMarket..tbl_814_header WHERE [814_Key] IN (SELECT SourceId FROM CustomerTransactionRequest WHERE RequestID = @OutboundRequestId) ORDER BY 1 FOR XML AUTO) as varchar(MAX))
SELECT CAST((SELECT * FROM paes_AEPEnergyMarket..tbl_814_Name WHERE [814_Key] IN (SELECT SourceId FROM CustomerTransactionRequest WHERE RequestID = @OutboundRequestId) ORDER BY 1 FOR XML AUTO) as varchar(MAX))
SELECT CAST((SELECT * FROM paes_AEPEnergyMarket..tbl_814_Service WHERE [814_Key] IN (SELECT SourceId FROM CustomerTransactionRequest WHERE RequestID = @OutboundRequestId) ORDER BY 1 FOR XML AUTO) as varchar(MAX))
SELECT CAST((SELECT * FROM paes_AEPEnergyMarket..tbl_814_Service_Account_Change WHERE Service_Key IN (SELECT Service_Key FROM paes_AEPEnergyMarket..tbl_814_Service WHERE [814_Key] IN (SELECT SourceId FROM CustomerTransactionRequest WHERE RequestID = @OutboundRequestId)) ORDER BY 1 FOR XML AUTO) as varchar(MAX))
SELECT CAST((SELECT * FROM paes_AEPEnergyMarket..tbl_814_Service_Date WHERE Service_Key IN (SELECT Service_Key FROM paes_AEPEnergyMarket..tbl_814_Service WHERE [814_Key] IN (SELECT SourceId FROM CustomerTransactionRequest WHERE RequestID = @OutboundRequestId)) ORDER BY 1 FOR XML AUTO) as varchar(MAX))
SELECT CAST((SELECT * FROM paes_AEPEnergyMarket..tbl_814_Service_Meter WHERE Service_Key IN (SELECT Service_Key FROM paes_AEPEnergyMarket..tbl_814_Service WHERE [814_Key] IN (SELECT SourceId FROM CustomerTransactionRequest WHERE RequestID = @OutboundRequestId)) ORDER BY 1 FOR XML AUTO) as varchar(MAX))
SELECT CAST((SELECT * FROM paes_AEPEnergyMarket..tbl_814_Service_Meter_Change WHERE Meter_Key IN (SELECT Meter_Key FROM paes_AEPEnergyMarket..tbl_814_Service_Meter WHERE Service_Key IN (SELECT Service_Key FROM paes_AEPEnergyMarket..tbl_814_Service WHERE [814_Key] IN (SELECT SourceId FROM CustomerTransactionRequest WHERE RequestID = @OutboundRequestId))) ORDER BY 1 FOR XML AUTO) as varchar(MAX))
SELECT CAST((SELECT * FROM paes_AEPEnergyMarket..tbl_814_Service_Meter_Contact WHERE ServiceMeter_Key IN (SELECT Meter_Key FROM paes_AEPEnergyMarket..tbl_814_Service_Meter WHERE Service_Key IN (SELECT Service_Key FROM paes_AEPEnergyMarket..tbl_814_Service WHERE [814_Key] IN (SELECT SourceId FROM CustomerTransactionRequest WHERE RequestID = @OutboundRequestId))) ORDER BY 1 FOR XML AUTO) as varchar(MAX))
SELECT CAST((SELECT * FROM paes_AEPEnergyMarket..tbl_814_Service_Meter_Reject WHERE ServiceMeter_Key IN (SELECT Meter_Key FROM paes_AEPEnergyMarket..tbl_814_Service_Meter WHERE Service_Key IN (SELECT Service_Key FROM paes_AEPEnergyMarket..tbl_814_Service WHERE [814_Key] IN (SELECT SourceId FROM CustomerTransactionRequest WHERE RequestID = @OutboundRequestId))) ORDER BY 1 FOR XML AUTO) as varchar(MAX))
SELECT CAST((SELECT * FROM paes_AEPEnergyMarket..tbl_814_Service_Meter_TOU WHERE Meter_Key IN (SELECT Meter_Key FROM paes_AEPEnergyMarket..tbl_814_Service_Meter WHERE Service_Key IN (SELECT Service_Key FROM paes_AEPEnergyMarket..tbl_814_Service WHERE [814_Key] IN (SELECT SourceId FROM CustomerTransactionRequest WHERE RequestID = @OutboundRequestId))) ORDER BY 1 FOR XML AUTO) as varchar(MAX))
SELECT CAST((SELECT * FROM paes_AEPEnergyMarket..tbl_814_Service_Meter_Type WHERE Meter_Key IN (SELECT Meter_Key FROM paes_AEPEnergyMarket..tbl_814_Service_Meter WHERE Service_Key IN (SELECT Service_Key FROM paes_AEPEnergyMarket..tbl_814_Service WHERE [814_Key] IN (SELECT SourceId FROM CustomerTransactionRequest WHERE RequestID = @OutboundRequestId))) ORDER BY 1 FOR XML AUTO) as varchar(MAX))
SELECT CAST((SELECT * FROM paes_AEPEnergyMarket..tbl_814_Service_Reject WHERE Service_Key IN (SELECT Service_Key FROM paes_AEPEnergyMarket..tbl_814_Service WHERE [814_Key] IN (SELECT SourceId FROM CustomerTransactionRequest WHERE RequestID = @OutboundRequestId)) ORDER BY 1 FOR XML AUTO) as varchar(MAX))
SELECT CAST((SELECT * FROM paes_AEPEnergyMarket..tbl_814_Service_Status WHERE Service_Key IN (SELECT Service_Key FROM paes_AEPEnergyMarket..tbl_814_Service WHERE [814_Key] IN (SELECT SourceId FROM CustomerTransactionRequest WHERE RequestID = @OutboundRequestId)) ORDER BY 1 FOR XML AUTO) as varchar(MAX))
SELECT CAST((SELECT * FROM paes_AEPEnergyMarket..tblMarketFile WHERE MarketFileId IN (SELECT MarketFileId FROM paes_AEPEnergyMarket..tbl_814_header WHERE [814_Key] IN (SELECT SourceId FROM CustomerTransactionRequest WHERE RequestID = @OutboundRequestId)) ORDER BY 1 FOR XML AUTO) as varchar(MAX))

DECLARE @RequestIds TABLE (RequestId INT)
INSERT INTO @RequestIds
SELECT RequestId FROM CustomerTransactionRequest WHERE CustId = @CustId AND TransactionType = '814' AND ActionCode = 'C' AND Direction = 1 AND RequestId > @OutboundRequestId ORDER BY 1

SELECT CAST((SELECT * FROM paes_AEPEnergyMarket..tbl_814_header WHERE [814_Key] IN (SELECT SourceId FROM CustomerTransactionRequest WHERE RequestID IN (SELECT RequestId FROM @RequestIds)) ORDER BY 1 FOR XML AUTO) as varchar(MAX))
SELECT CAST((SELECT * FROM paes_AEPEnergyMarket..tbl_814_Name WHERE [814_Key] IN (SELECT SourceId FROM CustomerTransactionRequest WHERE RequestID IN (SELECT RequestId FROM @RequestIds)) ORDER BY 1 FOR XML AUTO) as varchar(MAX))
SELECT CAST((SELECT * FROM paes_AEPEnergyMarket..tbl_814_Service WHERE [814_Key] IN (SELECT SourceId FROM CustomerTransactionRequest WHERE RequestID IN (SELECT RequestId FROM @RequestIds)) ORDER BY 1 FOR XML AUTO) as varchar(MAX))
SELECT CAST((SELECT * FROM paes_AEPEnergyMarket..tbl_814_Service_Account_Change WHERE Service_Key IN (SELECT Service_Key FROM paes_AEPEnergyMarket..tbl_814_Service WHERE [814_Key] IN (SELECT SourceId FROM CustomerTransactionRequest WHERE RequestID IN (SELECT RequestId FROM @RequestIds))) ORDER BY 1 FOR XML AUTO) as varchar(MAX))
SELECT CAST((SELECT * FROM paes_AEPEnergyMarket..tbl_814_Service_Date WHERE Service_Key IN (SELECT Service_Key FROM paes_AEPEnergyMarket..tbl_814_Service WHERE [814_Key] IN (SELECT SourceId FROM CustomerTransactionRequest WHERE RequestID IN (SELECT RequestId FROM @RequestIds))) ORDER BY 1 FOR XML AUTO) as varchar(MAX))
SELECT CAST((SELECT * FROM paes_AEPEnergyMarket..tbl_814_Service_Meter WHERE Service_Key IN (SELECT Service_Key FROM paes_AEPEnergyMarket..tbl_814_Service WHERE [814_Key] IN (SELECT SourceId FROM CustomerTransactionRequest WHERE RequestID IN (SELECT RequestId FROM @RequestIds))) ORDER BY 1 FOR XML AUTO) as varchar(MAX))
SELECT CAST((SELECT * FROM paes_AEPEnergyMarket..tbl_814_Service_Meter_Change WHERE Meter_Key IN (SELECT Meter_Key FROM paes_AEPEnergyMarket..tbl_814_Service_Meter WHERE Service_Key IN (SELECT Service_Key FROM paes_AEPEnergyMarket..tbl_814_Service WHERE [814_Key] IN (SELECT SourceId FROM CustomerTransactionRequest WHERE RequestID IN (SELECT RequestId FROM @RequestIds)))) ORDER BY 1 FOR XML AUTO) as varchar(MAX))
SELECT CAST((SELECT * FROM paes_AEPEnergyMarket..tbl_814_Service_Meter_Contact WHERE ServiceMeter_Key IN (SELECT Meter_Key FROM paes_AEPEnergyMarket..tbl_814_Service_Meter WHERE Service_Key IN (SELECT Service_Key FROM paes_AEPEnergyMarket..tbl_814_Service WHERE [814_Key] IN (SELECT SourceId FROM CustomerTransactionRequest WHERE RequestID IN (SELECT RequestId FROM @RequestIds)))) ORDER BY 1 FOR XML AUTO) as varchar(MAX))
SELECT CAST((SELECT * FROM paes_AEPEnergyMarket..tbl_814_Service_Meter_Reject WHERE ServiceMeter_Key IN (SELECT Meter_Key FROM paes_AEPEnergyMarket..tbl_814_Service_Meter WHERE Service_Key IN (SELECT Service_Key FROM paes_AEPEnergyMarket..tbl_814_Service WHERE [814_Key] IN (SELECT SourceId FROM CustomerTransactionRequest WHERE RequestID IN (SELECT RequestId FROM @RequestIds)))) ORDER BY 1 FOR XML AUTO) as varchar(MAX))
SELECT CAST((SELECT * FROM paes_AEPEnergyMarket..tbl_814_Service_Meter_TOU WHERE Meter_Key IN (SELECT Meter_Key FROM paes_AEPEnergyMarket..tbl_814_Service_Meter WHERE Service_Key IN (SELECT Service_Key FROM paes_AEPEnergyMarket..tbl_814_Service WHERE [814_Key] IN (SELECT SourceId FROM CustomerTransactionRequest WHERE RequestID IN (SELECT RequestId FROM @RequestIds)))) ORDER BY 1 FOR XML AUTO) as varchar(MAX))
SELECT CAST((SELECT * FROM paes_AEPEnergyMarket..tbl_814_Service_Meter_Type WHERE Meter_Key IN (SELECT Meter_Key FROM paes_AEPEnergyMarket..tbl_814_Service_Meter WHERE Service_Key IN (SELECT Service_Key FROM paes_AEPEnergyMarket..tbl_814_Service WHERE [814_Key] IN (SELECT SourceId FROM CustomerTransactionRequest WHERE RequestID IN (SELECT RequestId FROM @RequestIds)))) ORDER BY 1 FOR XML AUTO) as varchar(MAX))
SELECT CAST((SELECT * FROM paes_AEPEnergyMarket..tbl_814_Service_Reject WHERE Service_Key IN (SELECT Service_Key FROM paes_AEPEnergyMarket..tbl_814_Service WHERE [814_Key] IN (SELECT SourceId FROM CustomerTransactionRequest WHERE RequestID IN (SELECT RequestId FROM @RequestIds))) ORDER BY 1 FOR XML AUTO) as varchar(MAX))
SELECT CAST((SELECT * FROM paes_AEPEnergyMarket..tbl_814_Service_Status WHERE Service_Key IN (SELECT Service_Key FROM paes_AEPEnergyMarket..tbl_814_Service WHERE [814_Key] IN (SELECT SourceId FROM CustomerTransactionRequest WHERE RequestID IN (SELECT RequestId FROM @RequestIds))) ORDER BY 1 FOR XML AUTO) as varchar(MAX))
SELECT CAST((SELECT * FROM paes_AEPEnergyMarket..tblMarketFile WHERE MarketFileId IN (SELECT MarketFileId FROM paes_AEPEnergyMarket..tbl_814_header WHERE [814_Key] IN (SELECT SourceId FROM CustomerTransactionRequest WHERE RequestID IN (SELECT RequestId FROM @RequestIds))) ORDER BY 1 FOR XML AUTO) as varchar(MAX))


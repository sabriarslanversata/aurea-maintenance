DECLARE @CustId INT = {1}
DECLARE @PremId INT = (SELECT PremId FROM Premise WHERE CustId = @CustId)
DECLARE @AcctsRecID INT = (SELECT AcctsRecID FROM Customer WHERE CustId = @CustId)
DECLARE @RateTransitionIdToSimulate INT = (SELECT RateTransitionId FROM RateTransition WHERE CustId = @CustId AND UserId = 289)
DECLARE @ChangeRequestIdToInvoke INT = (SELECT ChangeRequestId FROM ChangeRequest WHERE ElementId = 6 AND ElementPrimaryKey = @RateTransitionIdToSimulate)

SELECT * FROM Customer WHERE CustId = @CustId FOR XML AUTO
SELECT * FROM Premise WHERE CustId = @CustId FOR XML AUTO
SELECT * FROM CustomerAdditionalInfo WHERE CustId = @CustId FOR XML AUTO
SELECT * FROM AccountsReceivable WHERE AcctsRecID = @AcctsRecID FOR XML AUTO
SELECT * FROM Meter WHERE PremId = @PremId FOR XML AUTO
SELECT * FROM Address WHERE AddrId IN (
	    SELECT SiteAddrId	FROM Customer WHERE CustID = @CustId
        UNION
        SELECT MailAddrId	FROM Customer WHERE CustID = @CustId
        UNION
        SELECT CorrAddrId	FROM Customer WHERE CustID = @CustId
        UNION
        SELECT AddrId FROM Premise WHERE CustID = @CustId
    ) FOR XML AUTO

SELECT * FROM Rate WHERE RateId IN (
SELECT RateId FROM Customer WHERE CustId = @CustId
UNION
SELECT RateId FROM RateTransition WHERE CustId = @CustId
) FOR XML AUTO

SELECT * FROM RateDetail WHERE RateId IN (
SELECT RateId from Customer where CustId = @CustId
UNION
SELECT RateId from RateTransition WHERE CustId = @CustId
) FOR XML AUTO

SELECT * FROM RateTransition WHERE CustId = @CustId FOR XML AUTO
SELECT * FROM Product WHERE RateId IN (SELECT RateId from RateTransition WHERE CustId = @CustId) FOR XML AUTO
SELECT * FROM CustomerTransactionRequest WHERE CustID = @CustId ORDER BY 1 FOR XML AUTO

SELECT * FROM Consumption WHERE MeterId IN (SELECT MeterId FROM Meter WHERE PremId = (SELECT PremId FROM Premise WHERE CustId = @CustId)) FOR XML AUTO
SELECT * FROM Invoice Where CustId = @CustId FOR XML AUTO
select InvoiceXMLID, InvoiceID, '<hint>NoNeedForThisTestCase</hint>' as InvoiceXML, CreateDate, PrintFormatID, PrintStatusID, PhysicalFile, PDFContent, HasPDF, MachineName 
FROM InvoiceXML WHERE InvoiceId IN (SELECT InvoiceId FROM Invoice WHERE CustId = @CustId) FOR XML AUTO
SELECT * FROM InvoiceLog WHERE InvLogId IN (SELECT InvLogId from Invoice Where CustId = @CustId) FOR XML AUTO
SELECT * FROM AccountsReceivableHistory WHERE CustId = @CustId FOR XML AUTO
SELECT * FROM InvoiceDetail WHERE InvoiceID IN (SELECT InvoiceId from Invoice Where CustId = @CustId) FOR XML AUTO
SELECT * FROM ConsumptionDetail WHERE ConsDetId IN (SELECT ConsDetID FROM InvoiceDetail WHERE InvoiceID IN (SELECT InvoiceId from Invoice Where CustId = @CustId) ) FOR XML AUTO
SELECT * FROM ChangeRequest WHERE ChangeRequestId = @ChangeRequestIdToInvoke FOR XML AUTO
SELECT * FROM ChangeRequestDetail WHERE ChangeRequestId = @ChangeRequestIdToInvoke FOR XML AUTO
SELECT * FROM ChangeTransaction WHERE ChangeTransactionId = (SELECT ChangeTransactionId FROM ChangeRequest WHERE ChangeRequestId = @ChangeRequestIdToInvoke) FOR XML AUTO
SELECT * FROM ChangeTransactionData WHERE ChangeTransactionId = (SELECT ChangeTransactionId FROM ChangeRequest WHERE ChangeRequestId = @ChangeRequestIdToInvoke) FOR XML AUTO


DECLARE @OutboundRequestId INT = (SELECT TOP 1 RequestId FROM CustomerTransactionRequest WHERE CustId = @CustId AND TransactionType = '814' AND ActionCode = 'C' AND Direction = 0 ORDER BY 1 DESC)

SELECT * FROM paes_AEPEnergyMarket..tbl_814_header WHERE [814_Key] IN (SELECT SourceId FROM CustomerTransactionRequest WHERE RequestID = @OutboundRequestId) FOR XML AUTO
SELECT * FROM paes_AEPEnergyMarket..tbl_814_Name WHERE [814_Key] IN (SELECT SourceId FROM CustomerTransactionRequest WHERE RequestID = @OutboundRequestId) FOR XML AUTO
SELECT * FROM paes_AEPEnergyMarket..tbl_814_Service WHERE [814_Key] IN (SELECT SourceId FROM CustomerTransactionRequest WHERE RequestID = @OutboundRequestId) FOR XML AUTO
SELECT * FROM paes_AEPEnergyMarket..tbl_814_Service_Account_Change WHERE Service_Key IN (SELECT Service_Key FROM paes_AEPEnergyMarket..tbl_814_Service WHERE [814_Key] IN (SELECT SourceId FROM CustomerTransactionRequest WHERE RequestID = @OutboundRequestId)) FOR XML AUTO
SELECT * FROM paes_AEPEnergyMarket..tbl_814_Service_Date WHERE Service_Key IN (SELECT Service_Key FROM paes_AEPEnergyMarket..tbl_814_Service WHERE [814_Key] IN (SELECT SourceId FROM CustomerTransactionRequest WHERE RequestID = @OutboundRequestId)) FOR XML AUTO
SELECT * FROM paes_AEPEnergyMarket..tbl_814_Service_Meter WHERE Service_Key IN (SELECT Service_Key FROM paes_AEPEnergyMarket..tbl_814_Service WHERE [814_Key] IN (SELECT SourceId FROM CustomerTransactionRequest WHERE RequestID = @OutboundRequestId)) FOR XML AUTO
SELECT * FROM paes_AEPEnergyMarket..tbl_814_Service_Meter_Change WHERE Meter_Key IN (SELECT Meter_Key FROM paes_AEPEnergyMarket..tbl_814_Service_Meter WHERE Service_Key IN (SELECT Service_Key FROM paes_AEPEnergyMarket..tbl_814_Service WHERE [814_Key] IN (SELECT SourceId FROM CustomerTransactionRequest WHERE RequestID = @OutboundRequestId))) FOR XML AUTO
SELECT * FROM paes_AEPEnergyMarket..tbl_814_Service_Meter_Contact WHERE Meter_Key IN (SELECT Meter_Key FROM paes_AEPEnergyMarket..tbl_814_Service_Meter WHERE Service_Key IN (SELECT Service_Key FROM paes_AEPEnergyMarket..tbl_814_Service WHERE [814_Key] IN (SELECT SourceId FROM CustomerTransactionRequest WHERE RequestID = @OutboundRequestId))) FOR XML AUTO
SELECT * FROM paes_AEPEnergyMarket..tbl_814_Service_Meter_Reject WHERE Meter_Key IN (SELECT Meter_Key FROM paes_AEPEnergyMarket..tbl_814_Service_Meter WHERE Service_Key IN (SELECT Service_Key FROM paes_AEPEnergyMarket..tbl_814_Service WHERE [814_Key] IN (SELECT SourceId FROM CustomerTransactionRequest WHERE RequestID = @OutboundRequestId))) FOR XML AUTO
SELECT * FROM paes_AEPEnergyMarket..tbl_814_Service_Meter_TOU WHERE Meter_Key IN (SELECT Meter_Key FROM paes_AEPEnergyMarket..tbl_814_Service_Meter WHERE Service_Key IN (SELECT Service_Key FROM paes_AEPEnergyMarket..tbl_814_Service WHERE [814_Key] IN (SELECT SourceId FROM CustomerTransactionRequest WHERE RequestID = @OutboundRequestId))) FOR XML AUTO
SELECT * FROM paes_AEPEnergyMarket..tbl_814_Service_Meter_Type WHERE Meter_Key IN (SELECT Meter_Key FROM paes_AEPEnergyMarket..tbl_814_Service_Meter WHERE Service_Key IN (SELECT Service_Key FROM paes_AEPEnergyMarket..tbl_814_Service WHERE [814_Key] IN (SELECT SourceId FROM CustomerTransactionRequest WHERE RequestID = @OutboundRequestId))) FOR XML AUTO
SELECT * FROM paes_AEPEnergyMarket..tbl_814_Service_Reject WHERE Service_Key IN (SELECT Service_Key FROM paes_AEPEnergyMarket..tbl_814_Service WHERE [814_Key] IN (SELECT SourceId FROM CustomerTransactionRequest WHERE RequestID = @OutboundRequestId)) FOR XML AUTO
SELECT * FROM paes_AEPEnergyMarket..tbl_814_Service_Status WHERE Service_Key IN (SELECT Service_Key FROM paes_AEPEnergyMarket..tbl_814_Service WHERE [814_Key] IN (SELECT SourceId FROM CustomerTransactionRequest WHERE RequestID = @OutboundRequestId)) FOR XML AUTO

SELECT * FROM paes_AEPEnergyMarket..tblMarketFile WHERE MarketFileId IN (SELECT MarketFileId FROM paes_AEPEnergyMarket..tbl_814_header WHERE [814_Key] IN (SELECT SourceId FROM CustomerTransactionRequest WHERE RequestID = @OutboundRequestId)) FOR XML AUTO

DECLARE @RequestIds TABLE (RequestId INT)
INSERT INTO @RequestIds
SELECT RequestId FROM CustomerTransactionRequest WHERE CustId = @CustId AND TransactionType = '814' AND ActionCode = 'C' AND Direction = 1 AND RequestId > @OutboundRequestId ORDER BY 1

SELECT * FROM paes_AEPEnergyMarket..tbl_814_header WHERE [814_Key] IN (SELECT SourceId FROM CustomerTransactionRequest WHERE RequestID IN (SELECT RequestId FROM @RequestIds)) FOR XML AUTO
SELECT * FROM paes_AEPEnergyMarket..tbl_814_Name WHERE [814_Key] IN (SELECT SourceId FROM CustomerTransactionRequest WHERE RequestID IN (SELECT RequestId FROM @RequestIds)) FOR XML AUTO
SELECT * FROM paes_AEPEnergyMarket..tbl_814_Service WHERE [814_Key] IN (SELECT SourceId FROM CustomerTransactionRequest WHERE RequestID IN (SELECT RequestId FROM @RequestIds)) FOR XML AUTO
SELECT * FROM paes_AEPEnergyMarket..tbl_814_Service_Account_Change WHERE Service_Key IN (SELECT Service_Key FROM paes_AEPEnergyMarket..tbl_814_Service WHERE [814_Key] IN (SELECT SourceId FROM CustomerTransactionRequest WHERE RequestID IN (SELECT RequestId FROM @RequestIds))) FOR XML AUTO
SELECT * FROM paes_AEPEnergyMarket..tbl_814_Service_Date WHERE Service_Key IN (SELECT Service_Key FROM paes_AEPEnergyMarket..tbl_814_Service WHERE [814_Key] IN (SELECT SourceId FROM CustomerTransactionRequest WHERE RequestID IN (SELECT RequestId FROM @RequestIds))) FOR XML AUTO
SELECT * FROM paes_AEPEnergyMarket..tbl_814_Service_Meter WHERE Service_Key IN (SELECT Service_Key FROM paes_AEPEnergyMarket..tbl_814_Service WHERE [814_Key] IN (SELECT SourceId FROM CustomerTransactionRequest WHERE RequestID IN (SELECT RequestId FROM @RequestIds))) FOR XML AUTO
SELECT * FROM paes_AEPEnergyMarket..tbl_814_Service_Meter_Change WHERE Meter_Key IN (SELECT Meter_Key FROM paes_AEPEnergyMarket..tbl_814_Service_Meter WHERE Service_Key IN (SELECT Service_Key FROM paes_AEPEnergyMarket..tbl_814_Service WHERE [814_Key] IN (SELECT SourceId FROM CustomerTransactionRequest WHERE RequestID IN (SELECT RequestId FROM @RequestIds)))) FOR XML AUTO
SELECT * FROM paes_AEPEnergyMarket..tbl_814_Service_Meter_Contact WHERE Meter_Key IN (SELECT Meter_Key FROM paes_AEPEnergyMarket..tbl_814_Service_Meter WHERE Service_Key IN (SELECT Service_Key FROM paes_AEPEnergyMarket..tbl_814_Service WHERE [814_Key] IN (SELECT SourceId FROM CustomerTransactionRequest WHERE RequestID IN (SELECT RequestId FROM @RequestIds)))) FOR XML AUTO
SELECT * FROM paes_AEPEnergyMarket..tbl_814_Service_Meter_Reject WHERE Meter_Key IN (SELECT Meter_Key FROM paes_AEPEnergyMarket..tbl_814_Service_Meter WHERE Service_Key IN (SELECT Service_Key FROM paes_AEPEnergyMarket..tbl_814_Service WHERE [814_Key] IN (SELECT SourceId FROM CustomerTransactionRequest WHERE RequestID IN (SELECT RequestId FROM @RequestIds)))) FOR XML AUTO
SELECT * FROM paes_AEPEnergyMarket..tbl_814_Service_Meter_TOU WHERE Meter_Key IN (SELECT Meter_Key FROM paes_AEPEnergyMarket..tbl_814_Service_Meter WHERE Service_Key IN (SELECT Service_Key FROM paes_AEPEnergyMarket..tbl_814_Service WHERE [814_Key] IN (SELECT SourceId FROM CustomerTransactionRequest WHERE RequestID IN (SELECT RequestId FROM @RequestIds)))) FOR XML AUTO
SELECT * FROM paes_AEPEnergyMarket..tbl_814_Service_Meter_Type WHERE Meter_Key IN (SELECT Meter_Key FROM paes_AEPEnergyMarket..tbl_814_Service_Meter WHERE Service_Key IN (SELECT Service_Key FROM paes_AEPEnergyMarket..tbl_814_Service WHERE [814_Key] IN (SELECT SourceId FROM CustomerTransactionRequest WHERE RequestID IN (SELECT RequestId FROM @RequestIds)))) FOR XML AUTO
SELECT * FROM paes_AEPEnergyMarket..tbl_814_Service_Reject WHERE Service_Key IN (SELECT Service_Key FROM paes_AEPEnergyMarket..tbl_814_Service WHERE [814_Key] IN (SELECT SourceId FROM CustomerTransactionRequest WHERE RequestID IN (SELECT RequestId FROM @RequestIds))) FOR XML AUTO
SELECT * FROM paes_AEPEnergyMarket..tbl_814_Service_Status WHERE Service_Key IN (SELECT Service_Key FROM paes_AEPEnergyMarket..tbl_814_Service WHERE [814_Key] IN (SELECT SourceId FROM CustomerTransactionRequest WHERE RequestID IN (SELECT RequestId FROM @RequestIds))) FOR XML AUTO
SELECT * FROM paes_AEPEnergyMarket..tblMarketFile WHERE MarketFileId IN (SELECT MarketFileId FROM paes_AEPEnergyMarket..tbl_814_header WHERE [814_Key] IN (SELECT SourceId FROM CustomerTransactionRequest WHERE RequestID IN (SELECT RequestId FROM @RequestIds))) FOR XML AUTO


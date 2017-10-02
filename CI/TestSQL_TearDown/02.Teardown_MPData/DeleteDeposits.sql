USE [MinistryPlatform]
GO

-- Get list of batches
DECLARE @BatchIDs TABLE ( Batch_ID INT )
INSERT INTO @BatchIDs (Batch_ID) SELECT Batch_ID FROM [dbo].[Batches] WHERE Deposit_ID IN (100000000, 100000001, 100000002)

-- Get list of payments
DECLARE @PaymentIDs TABLE ( Payment_ID INT )
INSERT INTO @PaymentIDs (Payment_ID) SELECT Payment_ID FROM [dbo].[Payments] WHERE Batch_ID IN (SELECT Batch_ID FROM @BatchIDs)

-- Get list of invoices
DECLARE @InvoiceIDs TABLE (Invoice_ID INT)
INSERT INTO @InvoiceIDs (Invoice_ID) SELECT Invoice_ID FROM [dbo].[Invoice_Detail] WHERE Invoice_Detail_ID IN (SELECT Invoice_Detail_ID FROM [dbo].[Payment_Detail] WHERE Payment_ID IN (SELECT Payment_ID FROM @PaymentIDs))

-- Delete Payment Details
DELETE FROM [dbo].[Payment_Detail] WHERE Payment_ID IN (SELECT Payment_ID FROM @PaymentIDs)

-- Delete Invoices Details
DELETE FROM [dbo].[Invoice_Detail] WHERE Invoice_ID IN (SELECT Invoice_ID FROM @InvoiceIDs)

-- Delete Invoices
DELETE FROM [dbo].[Invoices] WHERE Invoice_ID IN (SELECT Invoice_ID FROM @InvoiceIDs)

-- Delete Payments
DELETE FROM [dbo].[Payments] WHERE Payment_ID IN (SELECT Payment_ID FROM @PaymentIDs)

-- Delete Donation Distrubitions
DELETE FROM [dbo].[Donation_Distributions] WHERE Donation_ID IN (SELECT Donation_ID FROM [dbo].[Donations] WHERE Batch_ID IN (SELECT Batch_ID FROM @BatchIDs))

-- Delete Donations
DELETE FROM [dbo].[Donations] WHERE Batch_ID IN (SELECT Batch_ID FROM @BatchIDs)

-- Delete Batchs
DELETE FROM [dbo].[Batches] WHERE Batch_ID IN (SELECT Batch_ID FROM @BatchIDs)

-- Delete Deposit
DELETE FROM [dbo].[Deposits] WHERE Deposit_ID IN (100000000, 100000001, 100000002)
GO

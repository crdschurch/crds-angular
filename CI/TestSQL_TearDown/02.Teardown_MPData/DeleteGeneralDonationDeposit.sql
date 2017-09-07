USE [MinistryPlatform]
GO

-- Get data to delete
DECLARE @DepositID as int
set @DepositID = 100000000

DECLARE @BatchID as int
set @BatchID = SELECT Batch_ID FROM [dbo].[Batches] WHERE Deposit_ID = @DepositID

-- Delete Donation Distrubition
DELETE FROM [dbo].[Donation_Distribution_ID] WHERE Donation_ID IN (SELECT Donation_ID FROM [dbo].[Donations] WHERE Batch_ID = @BatchID)

-- Delete Donations
DELETE FROM [dbo].[Donations] WHERE Batch_ID = @BatchID

-- Delete Batch
DELETE FROM [dbo].[Batches] WHERE Batch_ID = @BatchID

-- Delete Deposit
DELETE FROM [dbo].[Deposits] WHERE Deposit_ID = @DepositID

GO

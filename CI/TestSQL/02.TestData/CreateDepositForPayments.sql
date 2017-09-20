USE [MinistryPlatform]
GO

-- Create deposit 
SET IDENTITY_INSERT [dbo].[Deposits] ON;

--Store the current identity value so we can reset it.
DECLARE @currentDepositId  as int
set @currentDepositId = IDENT_CURRENT('Deposits');

DECLARE @depositId as int
set @depositId = 100000002;

INSERT INTO [dbo].Deposits
(Deposit_ID,Deposit_Name           ,Deposit_Total,Deposit_Amount,Processor_Fee_Total,Deposit_Date                        ,Account_Number  ,Batch_Count,Domain_ID,Exported,Notes,__ExternalBatchID,Processor_Transfer_ID) VALUES
(@depositId,'(auto) Deposit With Two Batches',200.00       ,500.00        ,0.00               ,(convert(datetime, '2017-09-01', 1)),'474893274983'  ,1          ,1        ,0       ,null,null              ,null);

SET IDENTITY_INSERT [dbo].[Deposits] OFF;

--This command resets the identity value so that if someone adds contacts a big ID. 
DBCC CHECKIDENT (Deposits, reseed, @currentDepositId);

-- Setup a payment batch
DECLARE @batchId as int

INSERT INTO [dbo].Batches
(Batch_Name             ,Setup_Date                                     ,Batch_Total,Item_Count,Batch_Entry_Type_ID,Batch_Type_ID,Default_Program,Source_Event,Deposit_ID,Finalize_Date                                  ,Domain_ID,Congregation_ID,_Import_Counter,Source_File,Default_Payment_Type,Currency,Operator_User,__ExternalBatchID,Default_Program_ID_List,Processor_Transfer_ID) VALUES
('(auto) Payment batch',(convert(datetime,'2017-08-31 11:00:00.000',1)),300.00     ,3         ,12                 ,NULL         ,NULL           ,NULL        ,@depositId,(convert(datetime, '2017-09-01 16:00:00.000',1)),1        ,1              ,NULL           ,NULL       ,4                   ,'USD'   ,4445333      ,NULL             ,3                      ,NULL);

SET @batchId = SCOPE_IDENTITY()

-- Get Contact info for payments
DECLARE @benContactID as int
SET @benContactID  = (SELECT Contact_ID FROM Contacts WHERE First_Name = "Ben" AND Last_Name = "Kenobi")

DECLARE @wilmaContactID as int
SET @wilmaContactID  = (SELECT Contact_ID FROM Contacts WHERE First_Name = "Wilma" AND Last_Name = "Flintstone")

DECLARE @richardContactID as int
SET @richardContactID  = (SELECT Contact_ID FROM Contacts WHERE First_Name = "Richard" AND Last_Name = "Tremplay")

-- Create some payments
INSERT INTO [dbo].[Payments]
(Payment_Total,Contact_ID      ,Domain_ID,Payment_Date                                   ,Gateway_Response,Transaction_Code             ,Notes,Merchant_Batch,Payment_Type_ID,Item_Number,Processed,Currency,Invoice_Number,Batch_ID,Payment_Status_ID,Processor_Fee_Amount) VALUES
(100          ,@wilmaContactId ,1        ,(convert(datetime, '2017-08-20 13:36:00.00',1)),NULL            ,'ch_1B4C7iDpgPmDp9CAaiNOF6VN',NULL ,NULL          ,4              ,NULL       ,NULL     ,NULL    ,3085          ,@batchID,1                 ,0)

INSERT INTO [dbo].[Payments]
(Payment_Total,Contact_ID,Domain_ID,Payment_Date                                   ,Gateway_Response,Transaction_Code             ,Notes,Merchant_Batch,Payment_Type_ID,Item_Number,Processed,Currency,Invoice_Number,Batch_ID,Payment_Status_ID,Processor_Fee_Amount) VALUES
(100          ,@benContactID ,1    ,(convert(datetime, '2017-08-20 13:36:00.00',1)),NULL            ,'ch_1B4C89DpgPmDp9CAOylVOf0F',NULL ,NULL          ,4              ,NULL       ,NULL     ,NULL    ,3086          ,@batchID,1                 ,0)

INSERT INTO [dbo].[Payments]
(Payment_Total,Contact_ID       ,Domain_ID,Payment_Date                                   ,Gateway_Response,Transaction_Code             ,Notes,Merchant_Batch,Payment_Type_ID,Item_Number,Processed,Currency,Invoice_Number,Batch_ID,Payment_Status_ID,Processor_Fee_Amount) VALUES
(100          ,@richardContactID,1        ,(convert(datetime, '2017-08-20 13:36:00.00',1)),NULL            ,'ch_1B4CAMDpgPmDp9CAC8AgHQ0r',NULL ,NULL          ,4              ,NULL       ,NULL     ,NULL    ,3087          ,@batchID,1                 ,0)

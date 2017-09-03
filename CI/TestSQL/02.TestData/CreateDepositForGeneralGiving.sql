USE [MinistryPlatform]
GO

-- Get donor records
DECLARE @benDonorID as int
SET @benDonorID = (SELECT Donor_ID FROM Donors WHERE Contact_ID = (SELECT Contact_ID FROM Contacts WHERE First_Name = "Ben" AND Last_Name = "Kenobi"))

DECLARE @WilmaDonorID as int
SET @WilmaDonorID = (SELECT Donor_ID FROM Donors WHERE Contact_ID = (SELECT Contact_ID FROM Contacts WHERE First_Name = "Wilma" AND Last_Name = "Flintstone"))

DECLARE @richardDonorID as int
SET @richardDonorID = (SELECT Donor_ID FROM Donors WHERE Contact_ID = (SELECT Contact_ID FROM Contacts WHERE First_Name = "Richard" AND Last_Name = "Tremplay"))

-- Create deposit 
SET IDENTITY_INSERT [dbo].[Deposits] ON;

--Store the current identity value so we can reset it.
DECLARE @currentDepositId  as int
set @currentDepositId = IDENT_CURRENT('Deposits');

DECLARE @depositId as int
set @depositId = 100000000;

INSERT INTO [dbo].Deposits
(Deposit_ID,Deposit_Name           ,Deposit_Total,Deposit_Amount,Processor_Fee_Total,Deposit_Date                 ,Account_Number  ,Batch_Count,Domain_ID,Exported,Notes,__ExternalBatchID,Processor_Transfer_ID) VALUES
(@depositId,'(auto) General Giving',200.00       ,200.00        ,0.00               ,{d '2017-09-01'},'474893274983'  ,1          ,1        ,0       ,null,null              ,null);

SET IDENTITY_INSERT [dbo].[Deposits] OFF;

--This command resets the identity value so that if someone adds contacts a big ID. 
DBCC CHECKIDENT (Deposits, reseed, @currentDepositId);

-- Setup batch
DECLARE @batchId as int

INSERT INTO [dbo].Batches
(Batch_Name             ,Setup_Date                                     ,Batch_Total,Item_Count,Batch_Entry_Type_ID,Batch_Type_ID,Default_Program,Source_Event,Deposit_ID,Finalize_Date                                  ,Domain_ID,Congregation_ID,_Import_Counter,Source_File,Default_Payment_Type,Currency,Operator_User,__ExternalBatchID,Default_Program_ID_List,Processor_Transfer_ID) VALUES
('(auto) General Giving',(convert(datetime,'2017-08-30 11:00:00.000',1) ,200.00     ,3         ,12                 ,NULL         ,NULL           ,NULL        ,@depositId,(convert(datetime, '2017-09-01 16:00:00.000',1),1        ,1              ,NULL           ,NULL       ,4                   ,'USD'   ,4445333      ,NULL             ,3                      ,NULL);

SET @batchId = SCOPE_IDENTITY()

-- Create Some Donations
DECLARE @benDonation as int
DECLARE @WilmaDonation as int
DECLARE @richardDonation as int 

INSERT INTO [dbo].Donations 
(Donor_ID      ,Donation_Amount,Donation_Date                           ,Payment_Type_ID,Non_Cash_Asset_Type_ID,Item_Number,Batch_ID,Notes,Donor_Account_ID,[Anonymous],Check_Scanner_Batch,Donation_Status_Information,Donation_Status_ID,Donation_Status_Date            ,Donation_Status_Notes,Online_Donation_Information,Transaction_Code,Subscription_Code,Gateway_Response,Processed,Domain_ID,Currency,Receipted,Invoice_Number,Receipt_Number,__ExternalContributionID,__ExternalPaymentID,__ExternalGiverID,__ExternalDonorID,__ExteralMasterID1,__ExternalMasterID2,Registered_Donor,Processor_ID,Processor_Fee_Amount,Reconcile_Change_Needed,Reconcile_Change_Complete,Position) VALUES
(@benDonorID   ,50.0000        ,(convert(datetime,'8/30/2017 11:00 AM',1),4              ,null                  ,null       ,@batchId,null ,null            ,null       ,null               ,null                       ,2                 ,(convert(datetime,'9-1-2017',1),null                 ,null                       ,null            ,null             ,null            ,null     ,1        ,'USD'    ,0        ,null          ,null          ,null                    ,null               ,null             ,null             ,null              ,null               ,null            ,null        ,null                ,null                   ,null                    ,1 );
SET @benDonation = SCOPE_IDENTITY()

INSERT INTO [dbo].Donations 
(Donor_ID      ,Donation_Amount,Donation_Date                                  ,Payment_Type_ID,Non_Cash_Asset_Type_ID,Item_Number,Batch_ID,Notes,Donor_Account_ID,[Anonymous],Check_Scanner_Batch,Donation_Status_Information,Donation_Status_ID,Donation_Status_Date ,Donation_Status_Notes,Online_Donation_Information,Transaction_Code,Subscription_Code,Gateway_Response,Processed,Domain_ID,Currency,Receipted,Invoice_Number,Receipt_Number,__ExternalContributionID,__ExternalPaymentID,__ExternalGiverID,__ExternalDonorID,__ExteralMasterID1,__ExternalMasterID2,Registered_Donor,Processor_ID,Processor_Fee_Amount,Reconcile_Change_Needed,Reconcile_Change_Complete,Position) VALUES
(@WilmaDonorID ,50.0000        ,{d '8/30/2017 11:00 AM'}                       ,4              ,null                  ,null       ,@batchId,null ,null            ,null       ,null               ,null                       ,2                 ,{d '9-1-2017'}       ,null                 ,null                       ,null            ,null             ,null            ,null     ,1        ,'USD'    ,0        ,null          ,null          ,null                    ,null               ,null             ,null             ,null              ,null               ,null            ,null        ,null                ,null                   ,null                    ,1 );
SET @WilmaDonation = SCOPE_IDENTITY()

INSERT INTO [dbo].Donations 
(Donor_ID        ,Donation_Amount,Donation_Date                                  ,Payment_Type_ID,Non_Cash_Asset_Type_ID,Item_Number,Batch_ID,Notes,Donor_Account_ID,[Anonymous],Check_Scanner_Batch,Donation_Status_Information,Donation_Status_ID,Donation_Status_Date ,Donation_Status_Notes,Online_Donation_Information,Transaction_Code,Subscription_Code,Gateway_Response,Processed,Domain_ID,Currency,Receipted,Invoice_Number,Receipt_Number,__ExternalContributionID,__ExternalPaymentID,__ExternalGiverID,__ExternalDonorID,__ExteralMasterID1,__ExternalMasterID2,Registered_Donor,Processor_ID,Processor_Fee_Amount,Reconcile_Change_Needed,Reconcile_Change_Complete,Position) VALUES
(@richardDonorID ,100.0000       ,{d '8/30/2017 11:00 AM'}                       ,4              ,null                  ,null       ,@batchId,null ,null            ,null       ,null               ,null                       ,2                 ,{d '9-1-2017'}       ,null                 ,null                       ,null            ,null             ,null            ,null     ,1        ,'USD'    ,0        ,null          ,null          ,null                    ,null               ,null             ,null             ,null              ,null               ,null            ,null        ,null                ,null                   ,null                    ,1 );
SET @richardDonation = SCOPE_IDENTITY()

-- Setup distrubtions

USE [MinistryPlatform]
GO

-- Get donor records
DECLARE @benDonorID as int
SET @benDonorID = (SELECT Donor_ID FROM Donors WHERE Contact_ID = (SELECT Contact_ID FROM Contacts WHERE First_Name = 'Ben' AND Last_Name = 'Kenobi'))

DECLARE @andersonDonorID as int
SET @andersonDonorID = (SELECT Donor_ID FROM Donors WHERE Contact_ID = (SELECT Contact_ID FROM Contacts WHERE First_Name = 'Anderson' AND Last_Name = 'Cooper'))

DECLARE @richardDonorID as int
SET @richardDonorID = (SELECT Donor_ID FROM Donors WHERE Contact_ID = (SELECT Contact_ID FROM Contacts WHERE First_Name = 'Richard' AND Last_Name = 'Tremplay'))

-- Create deposit 
SET IDENTITY_INSERT [dbo].[Deposits] ON;

--Store the current identity value so we can reset it.
DECLARE @currentDepositId  as int
set @currentDepositId = IDENT_CURRENT('Deposits');

DECLARE @depositId as int
set @depositId = 100000001;

INSERT INTO [dbo].[Deposits]
(Deposit_ID,Deposit_Name           ,Deposit_Total,Deposit_Amount,Processor_Fee_Total,Deposit_Date                        ,Account_Number  ,Batch_Count,Domain_ID,Exported,Notes,__ExternalBatchID,Processor_Transfer_ID) VALUES
(@depositId,'(auto) Two Batches'  ,500.00       ,500.00        ,0.00               ,(convert(datetime, '09/01/2017', 101)),'474893274983'  ,1          ,1        ,0       ,null,null              ,null);

SET IDENTITY_INSERT [dbo].[Deposits] OFF;

--This command resets the identity value so that if someone adds contacts a big ID. 
DBCC CHECKIDENT (Deposits, reseed, @currentDepositId);

-- Setup batch
DECLARE @batchId as int

INSERT INTO [dbo].[Batches]
(Batch_Name             ,Setup_Date                                     ,Batch_Total,Item_Count,Batch_Entry_Type_ID,Batch_Type_ID,Default_Program,Source_Event,Deposit_ID,Finalize_Date                                  ,Domain_ID,Congregation_ID,_Import_Counter,_Source_File,Default_Payment_Type,Currency,Operator_User,__ExternalBatchID,Default_Program_ID_List,Processor_Transfer_ID) VALUES
('(auto) General Giving',(convert(datetime,'2017-08-30 11:00:00.000',21)),200.00     ,3         ,12                 ,NULL         ,NULL           ,NULL        ,@depositId,(convert(datetime, '2017-09-01 16:00:00.000',21)),1        ,1              ,NULL           ,NULL       ,4                   ,'USD'   ,4445333      ,NULL             ,3                      ,NULL);

SET @batchId = SCOPE_IDENTITY()

-- Create Some Donations
DECLARE @benDonationID as int
DECLARE @andersonDonationID as int
DECLARE @richardDonationID as int 

INSERT INTO [dbo].[Donations] 
(Donor_ID      ,Donation_Amount,Donation_Date                             ,Payment_Type_ID,Non_Cash_Asset_Type_ID,Item_Number,Batch_ID,Notes,Donor_Account_ID,[Anonymous],Check_Scanner_Batch,Donation_Status_Information,Donation_Status_ID,Donation_Status_Date            ,Donation_Status_Notes,Online_Donation_Information,Transaction_Code,Subscription_Code,Gateway_Response,Processed,Domain_ID,Currency,Receipted,Invoice_Number,Receipt_Number,__ExternalContributionID,__ExternalPaymentID,__ExternalGiverID,__ExternalDonorID,__ExteralMasterID1,__ExternalMasterID2,Registered_Donor,Processor_ID,Processor_Fee_Amount,Reconcile_Change_Needed,Reconcile_Change_Complete,Position) VALUES
(@benDonorID   ,50.0000        ,(convert(datetime,'8/30/2017 11:00 AM',101)),4              ,null                  ,null       ,@batchId,null ,null            ,null       ,null               ,null                       ,2                 ,(convert(datetime,'9-1-2017',110)),null                 ,null                       ,null            ,null             ,null            ,null     ,1        ,'USD'    ,0        ,null          ,null          ,null                    ,null               ,null             ,null             ,null              ,null               ,null            ,null        ,null                ,null                   ,null                    ,1 );
SET @benDonationID = SCOPE_IDENTITY()

INSERT INTO [dbo].[Donations] 
(Donor_ID      ,Donation_Amount,Donation_Date                                    ,Payment_Type_ID,Non_Cash_Asset_Type_ID,Item_Number,Batch_ID,Notes,Donor_Account_ID,[Anonymous],Check_Scanner_Batch,Donation_Status_Information,Donation_Status_ID,Donation_Status_Date               ,Donation_Status_Notes,Online_Donation_Information,Transaction_Code,Subscription_Code,Gateway_Response,Processed,Domain_ID,Currency,Receipted,Invoice_Number,Receipt_Number,__ExternalContributionID,__ExternalPaymentID,__ExternalGiverID,__ExternalDonorID,__ExteralMasterID1,__ExternalMasterID2,Registered_Donor,Processor_ID,Processor_Fee_Amount,Reconcile_Change_Needed,Reconcile_Change_Complete,Position) VALUES
(@andersonDonorID ,50.0000        ,(convert(datetime, '8/30/2017 11:00 AM', 101)),4              ,null                  ,null       ,@batchId,null ,null            ,null       ,null               ,null                       ,2                 ,(convert(datetime, '9-1-2017',110)),null                 ,null                       ,null            ,null             ,null            ,null     ,1        ,'USD'    ,0       ,null          ,null          ,null                    ,null               ,null             ,null             ,null              ,null               ,null            ,null        ,null                ,null                   ,null                     ,1 );
SET @andersonDonationID = SCOPE_IDENTITY()

INSERT INTO [dbo].[Donations] 
(Donor_ID        ,Donation_Amount,Donation_Date                                ,Payment_Type_ID,Non_Cash_Asset_Type_ID,Item_Number,Batch_ID,Notes,Donor_Account_ID,[Anonymous],Check_Scanner_Batch,Donation_Status_Information,Donation_Status_ID,Donation_Status_Date                    ,Donation_Status_Notes,Online_Donation_Information,Transaction_Code,Subscription_Code,Gateway_Response,Processed,Domain_ID,Currency,Receipted,Invoice_Number,Receipt_Number,__ExternalContributionID,__ExternalPaymentID,__ExternalGiverID,__ExternalDonorID,__ExteralMasterID1,__ExternalMasterID2,Registered_Donor,Processor_ID,Processor_Fee_Amount,Reconcile_Change_Needed,Reconcile_Change_Complete,Position) VALUES
(@richardDonorID ,100.0000       ,(convert(datetime, '8/30/2017 11:00 AM',101)),4              ,null                  ,null       ,@batchId,null ,null            ,null       ,null               ,null                       ,2                 ,(convert(datetime, '9-1-2017',110))     ,null                 ,null                       ,null            ,null             ,null            ,null     ,1        ,'USD'    ,0       ,null          ,null          ,null                    ,null               ,null             ,null             ,null              ,null               ,null            ,null        ,null                ,null                   ,null                     ,1 );
SET @richardDonationID = SCOPE_IDENTITY()

-- Setup distrubtions
INSERT INTO [dbo].[Donation_Distributions]
(Donation_ID   ,Amount ,Program_ID,Pledge_ID,Target_Event,Soft_Credit_Donor,Notes,Domain_ID,__ExternalContributionID,__ExternalCommitmentID,Congregation_ID,Message_Sent,HC_Donor_Congregation_ID) VALUES
(@benDonationID,50.000 ,3         ,NULL     ,NULL        ,NULL             ,NULL ,1        ,NULL                    ,NULL                  ,1              ,NULL        ,NULL);

INSERT INTO [dbo].[Donation_Distributions]
(Donation_ID        ,Amount,Program_ID,Pledge_ID,Target_Event,Soft_Credit_Donor,Notes,Domain_ID,__ExternalContributionID,__ExternalCommitmentID,Congregation_ID,Message_Sent,HC_Donor_Congregation_ID) VALUES
(@andersonDonationID,50.000,3         ,NULL     ,NULL        ,NULL             ,NULL ,1        ,NULL                    ,NULL                  ,1              ,NULL        ,NULL);

INSERT INTO [dbo].[Donation_Distributions]
(Donation_ID       ,Amount ,Program_ID,Pledge_ID,Target_Event,Soft_Credit_Donor,Notes,Domain_ID,__ExternalContributionID,__ExternalCommitmentID,Congregation_ID,Message_Sent,HC_Donor_Congregation_ID) VALUES
(@richardDonationID,150.000,3         ,NULL     ,NULL        ,NULL             ,NULL ,1        ,NULL                    ,NULL                  ,1              ,NULL        ,NULL);

-- Setup a payment batch
INSERT INTO [dbo].[Batches]
(Batch_Name             ,Setup_Date                                     ,Batch_Total,Item_Count,Batch_Entry_Type_ID,Batch_Type_ID,Default_Program,Source_Event,Deposit_ID,Finalize_Date                                  ,Domain_ID,Congregation_ID,_Import_Counter,_Source_File,Default_Payment_Type,Currency,Operator_User,__ExternalBatchID,Default_Program_ID_List,Processor_Transfer_ID) VALUES
('(auto) Payment batch',(convert(datetime,'2017-08-31 11:00:00.000',21)),300.00     ,3         ,12                 ,NULL         ,NULL           ,NULL        ,@depositId,(convert(datetime, '2017-09-01 16:00:00.000',21)),1        ,1              ,NULL           ,NULL       ,4                   ,'USD'   ,4445333      ,NULL             ,3                      ,NULL);

SET @batchId = SCOPE_IDENTITY()

-- Get Contact info for payments
DECLARE @benContactID as int
SET @benContactID  = (SELECT Contact_ID FROM Contacts WHERE First_Name = 'Ben' AND Last_Name = 'Kenobi')

DECLARE @andersonContactID as int
SET @andersonContactID  = (SELECT Contact_ID FROM Contacts WHERE First_Name = 'Anderson' AND Last_Name = 'Cooper')

DECLARE @richardContactID as int
SET @richardContactID  = (SELECT Contact_ID FROM Contacts WHERE First_Name = 'Richard' AND Last_Name = 'Tremplay')

-- Create a invoices
DECLARE @benInvoiceID as int
DECLARE @andersonInvoiceID as int
DECLARE @richardInvoiceID as int 

INSERT INTO [dbo].[Invoices]
(Purchaser_Contact_ID,Invoice_Status_ID,Invoice_Total,Invoice_Date                                  ,Domain_ID,Notes,Currency) values
(@benContactID       ,2                ,360.00       ,(convert(datetime, '2016-12-19 10:15:00.000', 21)),1        ,NULL ,NULL)
SET @benInvoiceID = SCOPE_IDENTITY()

INSERT INTO [dbo].[Invoices]
(Purchaser_Contact_ID,Invoice_Status_ID,Invoice_Total,Invoice_Date                                  ,Domain_ID,Notes,Currency) values
(@andersonContactID       ,2                ,365.00       ,(convert(datetime, '2016-12-29 10:15:00.000',21)),1        ,NULL ,NULL)
SET @andersonInvoiceID = SCOPE_IDENTITY()

INSERT INTO [dbo].[Invoices]
(Purchaser_Contact_ID,Invoice_Status_ID,Invoice_Total,Invoice_Date                                  ,Domain_ID,Notes,Currency) values
(@richardContactID       ,2                ,460.00       ,(convert(datetime, '2017-01-19 10:15:00.000',21)),1        ,NULL ,NULL)
SET @richardInvoiceID = SCOPE_IDENTITY()

-- Create some payments
DECLARE @benPaymentID as int
DECLARE @andersonPaymentID as int
DECLARE @richardPaymentID as int 

INSERT INTO [dbo].[Payments]
(Payment_Total,Contact_ID      ,Domain_ID,Payment_Date                                    ,Gateway_Response,Transaction_Code             ,Notes,Merchant_Batch,Payment_Type_ID,Item_Number,Processed,Currency,Invoice_Number ,Batch_ID,Payment_Status_ID,Processor_Fee_Amount) VALUES
(100          ,@andersonContactId ,1        ,(convert(datetime, '2017-08-20 13:36:00.00',21)),NULL            ,'ch_1B4C7iDpgPmDp9CAaiNOF6VN',NULL ,NULL          ,4              ,NULL       ,NULL     ,NULL    ,@andersonInvoiceID,@batchId,1                      ,0)
SET @andersonPaymentID = SCOPE_IDENTITY()

INSERT INTO [dbo].[Payments]
(Payment_Total,Contact_ID,Domain_ID,Payment_Date                                   ,Gateway_Response,Transaction_Code             ,Notes,Merchant_Batch,Payment_Type_ID,Item_Number,Processed,Currency,Invoice_Number,Batch_ID,Payment_Status_ID,Processor_Fee_Amount) VALUES
(100          ,@benContactID ,1    ,(convert(datetime, '2017-08-20 13:36:00.00',21)),NULL           ,'ch_1B4C89DpgPmDp9CAOylVOf0F',NULL ,NULL          ,4              ,NULL       ,NULL     ,NULL    ,@benInvoiceId ,@batchID,1                 ,0)
SET @benPaymentID = SCOPE_IDENTITY()

INSERT INTO [dbo].[Payments]
(Payment_Total,Contact_ID       ,Domain_ID,Payment_Date                                   ,Gateway_Response,Transaction_Code             ,Notes,Merchant_Batch,Payment_Type_ID,Item_Number,Processed,Currency,Invoice_Number   ,Batch_ID,Payment_Status_ID,Processor_Fee_Amount) VALUES
(100          ,@richardContactID,1        ,(convert(datetime, '2017-08-20 13:36:00.00',21)),NULL            ,'ch_1B4CAMDpgPmDp9CAC8AgHQ0r',NULL ,NULL          ,4              ,NULL       ,NULL     ,NULL   ,@richardInvoiceID,@batchID,1                 ,0)
SET @richardPaymentID = SCOPE_IDENTITY()

-- Create a invoice details
DECLARE @benInvoiceDetailID as int
DECLARE @andersonInvoiceDetailID as int
DECLARE @richardInvoiceDetailID as int 

INSERT INTO [dbo].[Invoice_Detail]
(Invoice_ID   ,Recipient_Contact_ID,Event_Participant_ID,Item_Quantity,Line_Total,Product_ID,Product_Option_Price_ID,Domain_ID,Item_Note,Recipient_Name,Recipient_Address,Recipient_Email,Recipient_Phone) VALUES
(@benInvoiceID,@benContactID       ,NULL                ,1            ,360       ,9         ,NULL                   ,1        ,NULL     ,NULL          ,NULL             ,NULL           ,NULL)
SET @benInvoiceDetailID = SCOPE_IDENTITY()

INSERT INTO [dbo].[Invoice_Detail]
(Invoice_ID   ,Recipient_Contact_ID,Event_Participant_ID,Item_Quantity,Line_Total,Product_ID,Product_Option_Price_ID,Domain_ID,Item_Note,Recipient_Name,Recipient_Address,Recipient_Email,Recipient_Phone) VALUES
(@andersonInvoiceID,@andersonContactID   ,NULL                ,1            ,365       ,9         ,NULL                   ,1        ,NULL     ,NULL          ,NULL             ,NULL           ,NULL)
SET @andersonInvoiceDetailID = SCOPE_IDENTITY()

INSERT INTO [dbo].[Invoice_Detail]
(Invoice_ID       ,Recipient_Contact_ID,Event_Participant_ID,Item_Quantity,Line_Total,Product_ID,Product_Option_Price_ID,Domain_ID,Item_Note,Recipient_Name,Recipient_Address,Recipient_Email,Recipient_Phone) VALUES
(@richardInvoiceID,@richardContactID       ,NULL                ,1            ,460       ,9         ,NULL                   ,1        ,NULL     ,NULL          ,NULL             ,NULL           ,NULL)
SET @richardInvoiceDetailID = SCOPE_IDENTITY()

-- Create you some payment details
INSERT INTO [dbo].[Payment_Detail]
(Payment_ID   ,Payment_Amount,Invoice_Detail_ID  ,Domain_ID,Congregation_ID) VALUES
(@benPaymentID,100.00        ,@benInvoiceDetailID,1        ,5)

INSERT INTO [dbo].[Payment_Detail]
(Payment_ID     ,Payment_Amount,Invoice_Detail_ID    ,Domain_ID,Congregation_ID) VALUES
(@andersonPaymentID,100.00        ,@andersonInvoiceDetailID,1        ,5)

INSERT INTO [dbo].[Payment_Detail]
(Payment_ID       ,Payment_Amount,Invoice_Detail_ID      ,Domain_ID,Congregation_ID) VALUES
(@richardPaymentID,100.00        ,@richardInvoiceDetailID,1        ,5)

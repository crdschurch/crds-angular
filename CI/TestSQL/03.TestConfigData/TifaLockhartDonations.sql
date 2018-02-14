USE [MinistryPlatform]
GO

--Tifa Lockhart Donations
--Add a noncash asset donation with 2 distributions
DECLARE @tifaDonorId as int
set @tifaDonorId = (select donor_record from Contacts where Email_Address = 'mpcrds+tifalockhart@gmail.com');

DECLARE @thisYear as VARCHAR(4)
set @thisYear = CONVERT(VARCHAR(4), YEAR(GETDATE()));

--2000 non/cash asset donation of type stock
INSERT INTO [dbo].donations 
(Donor_ID     ,Donation_Amount,Donation_Date                                     ,Payment_Type_ID,Non_Cash_Asset_Type_ID,Item_Number,Batch_ID,Notes,Donor_Account_ID,[Anonymous],Check_Scanner_Batch,Donation_Status_Information,Donation_Status_ID,Donation_Status_Date                              ,Donation_Status_Notes,Online_Donation_Information,Transaction_Code,Subscription_Code,Gateway_Response,Processed,Domain_ID,Currency,Receipted,Invoice_Number,Receipt_Number,__ExternalContributionID,__ExternalPaymentID,__ExternalGiverID,__ExternalDonorID,__ExteralMasterID1,__ExternalMasterID2,Registered_Donor,Processor_ID,Processor_Fee_Amount,Reconcile_Change_Needed,Reconcile_Change_Complete) VALUES
(@tifaDonorId ,2000.0000      ,CAST(@thisYear+'-01-06 12:27:27' as smalldatetime),6              ,1                     ,null       ,null    ,null ,null            ,null       ,null               ,null                       ,2                 ,CAST(@thisYear+'-01-06 12:27:27' as smalldatetime),null                 ,null                       ,null            ,null             ,null            ,null     ,1        ,null    ,0        ,null          ,null          ,null                    ,null               ,null             ,null             ,null              ,null               ,1               ,null        ,null                ,null                   ,null                     );

--1000 distribution to General fund. 
INSERT INTO [dbo].donation_distributions 
(Donation_ID                                                                                         ,Amount   ,Program_ID,Pledge_ID,Target_Event,Soft_Credit_Donor,Notes,Domain_ID,__ExternalContributionID,__ExternalCommitmentID,Congregation_ID) VALUES
((select top 1 Donation_ID from donations where donor_id = @tifaDonorId order by Donation_date desc) ,1000.0000,3         ,null     ,null        ,null             ,null ,1        ,null                    ,null                  ,5              );

--1000 distribution to (t) Test Pledge Program1 fund.
INSERT INTO [dbo].donation_distributions 
(Donation_ID                                                                                         ,Amount   ,Program_ID                                                                       ,Pledge_ID,Target_Event,Soft_Credit_Donor,Notes,Domain_ID,__ExternalContributionID,__ExternalCommitmentID,Congregation_ID) VALUES
((select top 1 Donation_ID from donations where donor_id = @tifaDonorId order by Donation_date desc) ,1000.0000,(Select program_id from programs where program_name = '(t) Test Pledge Program1'),null     ,null        ,null             ,null ,1        ,null                    ,null                  ,5              );
GO
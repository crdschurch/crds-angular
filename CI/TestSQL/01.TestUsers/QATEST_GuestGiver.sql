--Guest Giver Account - This is creates a contact-only guest giver account for a registered user
USE [MinistryPlatform]
GO

DECLARE @processorID as varchar(20);

IF (SELECT URL from DP_Bookmarks where name like '%demo%') is not null
	SET @processorID = 'cus_6YjXxCzFsV300g';
ELSE
	SET @processorID = 'cus_8Lez4HV887Qnwo';

--Create Contact Record
INSERT INTO [dbo].Contacts 
(Company,Display_Name ,Nickname     ,Contact_Status_ID,Household_Position_ID,Email_Address        ,Bulk_Email_Opt_Out,Bulk_SMS_Opt_Out,Contact_GUID,Domain_ID) VALUES
(0      ,'Guest Giver','Guest Giver',1                ,1                    ,'mpcrds+20@gmail.com',0                 ,0               ,NEWID()     ,1        );

DECLARE @contactID int = SCOPE_IDENTITY();

--Donor RECORD
INSERT INTO [dbo].Donors 
(Contact_ID,Statement_Frequency_ID,Statement_Type_ID,Statement_Method_ID,Setup_Date                ,Cancel_Envelopes,Notes            ,Domain_ID,Processor_ID) VALUES
(@contactID,3                     ,1                ,4                  ,{ts '2015-07-06 12:03:37'},0               ,'Scripted Donor' ,1        ,@processorID);

DECLARE @donor_id int = SCOPE_IDENTITY();

--Update Contact Record
UPDATE [dbo].Contacts 
SET Donor_Record = @donor_id 
WHERE contact_id = @contactID;

GO

--Guest Giver Account - This is creates a contact-only guest giver account for a registered user
USE [MinistryPlatform]
GO

DECLARE @processorID as varchar(255);

IF (SELECT URL from DP_Bookmarks where name like '%demo%') is not null
	SET @processorID = 'cus_6YjXxCzFsV300g';
ELSE
	SET @processorID = 'cus_8Lez4HV887Qnwo';
	
--Guest Giving ACCOUNT
--Contact Record
INSERT INTO [dbo].Contacts 
(Company,Display_Name ,Nickname     ,Contact_Status_ID,Household_ID,Household_Position_ID,Participant_Record,Donor_Record,Email_Address        ,Email_Unlisted,Bulk_Email_Opt_Out,Bulk_SMS_Opt_Out,Mobile_Phone,Mobile_Carrier,Mobile_Phone_Unlisted,Company_Phone,Pager_Phone,Fax_Phone,User_Account,Web_Page,Remove_From_Directory,Industry_ID,Occupation_ID,Employer_Name,[SSN/EIN],Anniversary_Date,HS_Graduation_Year,Current_School,Contact_GUID,ID_Card,Domain_ID) VALUES
(0      ,'Guest Giver','Guest Giver',1                ,null        ,1                    ,null              ,null        ,'mpcrds+20@gmail.com',null          ,0                 ,0               ,null        ,null          ,null                 ,null         ,null       ,null     ,null        ,null    ,null                 ,null       ,null         ,null         ,null     ,null            ,null              ,null          ,NEWID()     ,null   ,1        );

DECLARE @contactID as int
SET @contactID = (select top 1 contact_id from contacts where email_address = 'mpcrds+20@gmail.com' and last_name is null);

--Donor RECORD
INSERT INTO [dbo].Donors 
(Contact_ID,Statement_Frequency_ID,Statement_Type_ID,Statement_Method_ID,Setup_Date                ,Envelope_No,Cancel_Envelopes,Notes,First_Contact_Made,Domain_ID,__ExternalPersonID,_First_Donation_Date,_Last_Donation_Date,Processor_ID) VALUES
(@contactID,3                     ,1                ,4                  ,{ts '2015-07-06 12:03:37'},null       ,0               ,'Scripted Donor' ,null              ,1        ,null              ,null                ,null               ,@processorID);

--Update Contact Record
UPDATE [dbo].Contacts set Donor_Record = (select donor_id from donors where contact_id = @contactID) where contact_id = @contactID;
GO

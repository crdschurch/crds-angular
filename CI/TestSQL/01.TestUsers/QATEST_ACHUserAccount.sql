--Registered Account - This is a user that has ACH saved as payment data in stripe and should not change to credit card. 
USE [MinistryPlatform]
GO

DECLARE @processorID as varchar(255);

IF (SELECT URL from DP_Bookmarks where name like '%demo%') is not null
	SET @processorID = 'cus_6cAhwmmKyaw9D5';
ELSE
	SET @processorID = 'cus_8gBB5FUZFB9b0n';

--Mpcrds+32@gmail.com contact record
DECLARE @contactID as int
set @contactID = (select contact_id from contacts where Email_Address = 'mpcrds+32@gmail.com' and Last_Name = 'Fry');

INSERT INTO [dbo].donors 
(Contact_ID,Statement_Frequency_ID,Statement_Type_ID,Statement_Method_ID,Setup_Date                ,Envelope_No,Cancel_Envelopes,Notes,First_Contact_Made,Domain_ID,__ExternalPersonID,_First_Donation_Date,_Last_Donation_Date,Processor_ID) VALUES
(@contactID,1                     ,1                ,2                  ,{ts '2015-07-15 16:19:22'},null       ,0               ,'Scripted Donor' ,null              ,1        ,null              ,null                ,null               ,@processorID);

DECLARE @donor_id as int
set @donor_id = (Select donor_ID from donors where contact_id = @contactID);

--Update Contact Record
update [dbo].Contacts set Donor_Record = @donor_id where contact_id = @contactID;
GO

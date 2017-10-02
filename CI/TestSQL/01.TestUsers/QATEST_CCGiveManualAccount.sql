--Registered Account - Existing CC donor for manual testing
USE [MinistryPlatform]
GO

--mpcrds+16@gmail.com contact record
DECLARE @contactID as int
set @contactID = (select contact_id from contacts where Email_Address = 'mpcrds+16@gmail.com' and Last_Name = 'Gentry');

INSERT INTO [dbo].donors 
(Contact_ID,Statement_Frequency_ID,Statement_Type_ID,Statement_Method_ID,Setup_Date                    ,Envelope_No,Cancel_Envelopes,Notes,First_Contact_Made,Domain_ID,__ExternalPersonID,_First_Donation_Date,_Last_Donation_Date,Processor_ID        ) VALUES
(@contactID,1                     ,1                ,2                  ,{ts '2015-08-05 12:26:57.503'},null       ,0               ,'Scripted Donor' ,null              ,1        ,null              ,null                ,null               ,'cus_6ort2MTNc0l0Uh');

DECLARE @donor_id as int
set @donor_id = (Select donor_ID from donors where contact_id = @contactID);

--Update Contact Record
update [dbo].Contacts set Donor_Record = @donor_id where contact_id = @contactID;
GO
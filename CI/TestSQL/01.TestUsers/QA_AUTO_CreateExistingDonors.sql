--Modify automation contacts created by createTestUsers.ps1, causing them to be existing donors
USE [MinistryPlatform]
GO

--
--mpcrds+auto+1@gmail.com (existing ACH donor)
--
--Retrieve contact ID
DECLARE @contactID as int
set @contactID = (select contact_id from contacts where Email_Address = 'mpcrds+auto+1@gmail.com' and Last_Name = 'Karpyshyn');

--Create a Donor record for mpcrds+auto+1@gmail.com (used the Stripe processor ID from a newly-created ACH transaction)
DECLARE @processorID as varchar(255);
if (select @@servername) like '%demo%'
  set @processorID = 'cus_9sx2eijgfgS97o';
else
  set @processorID = 'cus_9rioLxKczE0gLb';

INSERT INTO [dbo].donors 
(Contact_ID,Statement_Frequency_ID,Statement_Type_ID,Statement_Method_ID,Setup_Date                    ,Envelope_No,Cancel_Envelopes,Notes,First_Contact_Made,Domain_ID,__ExternalPersonID,_First_Donation_Date,_Last_Donation_Date,Processor_ID        ) VALUES
(@contactID,1                     ,1                ,2                  ,{ts '2017-01-03 15:26:57.503'},null       ,0              ,'Scripted Donor' ,null              ,1        ,null              ,null                ,null               ,@processorID);

--Retrieve Donor ID from new Donor record
DECLARE @donor_id as int
set @donor_id = (Select donor_ID from donors where contact_id = @contactID);

--Update Contact record with Donor ID
update [dbo].Contacts set Donor_Record = @donor_ID where contact_id = @contactID;
GO


--
--mpcrds+auto+2@gmail.com (existing CC donor)
--
--Retrieve contact ID
DECLARE @contactID as int
set @contactID = (select contact_id from contacts where Email_Address = 'mpcrds+auto+2@gmail.com' and Last_Name = 'Kenobi');

--Create a Donor record for mpcrds+auto+2@gmail.com (used the Stripe processor ID from a newly-created CC transaction)
DECLARE @processorID as varchar(255);
if (select @@servername) like '%demo%'
  set @processorID = 'cus_9sx3C3pje6vvFk';
else
  set @processorID = 'cus_9s2PUrgn12xY0n';

INSERT INTO [dbo].donors
(Contact_ID,Statement_Frequency_ID,Statement_Type_ID,Statement_Method_ID,Setup_Date                    ,Envelope_No,Cancel_Envelopes,Notes,First_Contact_Made,Domain_ID,__ExternalPersonID,_First_Donation_Date,_Last_Donation_Date,Processor_ID        ) VALUES
(@contactID,1                     ,1                ,2                  ,{ts '2017-01-03 15:26:57.503'},null       ,0              ,'Scripted Donor' ,null              ,1        ,null              ,null                ,null               ,@processorID);

--Retrieve Donor ID from new Donor record
DECLARE @donor_id as int
set @donor_id = (Select donor_ID from donors where contact_id = @contactID);

--Update Contact record with Donor ID
update [dbo].Contacts set Donor_Record = @donor_ID where contact_id = @contactID;

GO

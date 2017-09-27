USE [MinistryPlatform]
GO

--Get the required data to add to our contact. 
Declare @contactID as int
Set @contactID = (select contact_id from contacts where email_address = 'mpcrds+auto+fredflintstone@gmail.com' and last_name = 'Flintstone');

Declare @houseHoldID as int
set @houseHoldID = (select houseHold_ID from contacts where contact_id = @contactID);

Declare @participantID as int
set @participantID = (select participant_record from contacts where contact_id = @contactID);

Declare @userAccount as int
set @userAccount = (select user_account from contacts where contact_id = @contactID);

--Update old contact record so we can delete it. 
UPDATE [dbo].Contacts
SET Household_ID = null, Participant_Record = null, User_Account = null
WHERE email_address = 'mpcrds+auto+fredflintstone@gmail.com' and last_name = 'Flintstone';

--Temporarily update the participant and user account records - Please don't fail.
UPDATE [dbo].Participants 
SET Contact_ID = 1
WHERE Participant_ID = @participantID;

UPDATE [dbo].dp_users
SET Contact_ID = 1
WHERE USER_ID = @userAccount;

--Just get rid of this so we can delete Fred's old contact record
DELETE From [dbo].CONTACT_HOUSEHOLDS
WHERE CONTACT_ID = @contactID;

DECLARE @communicationID as int
set @communicationID = (Select Communication_ID from dp_Communications where TO_CONTACT = @contactID);

DELETE from [dbo].dp_commands 
WHERE communication_id = @communicationID;

DELETE from [dbo].dp_Contact_Publications 
WHERE contact_id = @contactID;

DELETE from [dbo].dp_communication_messages 
WHERE Communication_ID = @communicationID;

Delete from [dbo].dp_Communications
WHERE Communication_ID = @communicationID;

Delete from [dbo].Activity_Log
WHERE Contact_id = @contactID;

--Delete the old contact record for Fred
DELETE FROM [dbo].Contacts where Contact_ID = @contactID;

--Address
SET IDENTITY_INSERT [dbo].[Addresses] ON;

DECLARE @addressId as int
set @addressId = IDENT_CURRENT('Addresses')+1;

INSERT INTO [dbo].Addresses 
(Address_ID,Address_Line_1        ,Address_Line_2,City  ,[State/Region],Postal_Code,Foreign_Country,Country_Code,Domain_ID,Carrier_Route,Lot_Number,Delivery_Point_Code,Delivery_Point_Check_Digit,Latitude,Longitude,Altitude,Time_Zone,Bar_Code,Area_Code,Last_Validation_Attempt,County,Validated,Do_Not_Validate,Last_GeoCode_Attempt,__ExternalAddressID) VALUES
(@addressID,'301 Cobblestone Way',null          ,'CITY','OH'          ,'45209'    ,'United States','USA'       ,1        ,null         ,null      ,null               ,null                      ,null    ,null     ,null    ,null     ,null    ,null     ,null                   ,null  ,null     ,null           ,null                ,null               );

SET IDENTITY_INSERT [dbo].[Addresses] OFF;

--Household updates
UPDATE [dbo].Households 
SET Address_ID = @addressID, Home_Phone = '123-765-4323', Congregation_ID = 6
WHERE houseHold_ID = @houseHoldID;

--Fred Flintstone Contact
SET IDENTITY_INSERT [dbo].[Contacts] ON;

--Store the current identity value so we can reset it.
DECLARE @currentContactId as int
set @currentContactId = IDENT_CURRENT('Contacts');

set @contactID = 100000010;

INSERT INTO [dbo].Contacts 
(Contact_ID,Company,Company_Name,Display_Name   ,Prefix_ID,First_Name,Middle_Name,Last_Name   ,Suffix_ID,Nickname ,Date_of_Birth   ,Gender_ID,Marital_Status_ID,Contact_Status_ID,Household_ID,Household_Position_ID,Participant_Record,Donor_Record,Email_Address                  ,Email_Unlisted,Bulk_Email_Opt_Out,Bulk_SMS_Opt_Out,Mobile_Phone  ,Mobile_Carrier,Mobile_Phone_Unlisted,Company_Phone ,Pager_Phone,Fax_Phone,User_Account,Web_Page,Remove_From_Directory,Industry_ID,Occupation_ID,Employer_Name,[SSN/EIN],Anniversary_Date,HS_Graduation_Year,Current_School,Contact_GUID ,ID_Card,Domain_ID,__ShelbyID,__ExternalHouseholdID,__ExternalPersonID,__ExternalUserID,__ExternalBusinessID,Maiden_Name,__LastLegacyLogin,__LegacyUserName,__LegacyUserID,__LegacyEmailAddress) VALUES
(@contactID,0      ,null        ,'Flintstone, Fred',1        ,'Fred'   ,'S'        ,'Flintstone'    ,null     ,'Fred'  ,{d '1975-01-01'},1        ,2                ,1                ,@houseHoldID,1                    ,@participantID    ,null        ,'mpcrds+auto+fredflintstone@gmail.com' ,null          ,0                 ,0               ,'513-654-8745',null          ,null                 ,'555-365-4125',null       ,null     ,@userAccount,null    ,null                 ,null       ,null         ,null         ,null     ,null            ,null              ,null          ,NEWID()      ,null   ,1        ,null      ,null                 ,null              ,null            ,null                ,null       ,null             ,null            ,null          ,null                );

SET IDENTITY_INSERT [dbo].[Contacts] OFF;

--This command resets the identity value so that if someone adds contacts a big ID. 
DBCC CHECKIDENT (Contacts, reseed, @currentContactId);

DECLARE @processorID as varchar(255);

IF ((SELECT @@SERVERNAME) like '%demo%')
	SET @processorID = 'cus_A5SVYfrwMNfi4w';
ELSE
	SET @processorID = 'cus_A5Sil6Ppz0rbW6';

-- Fred Flintstone Donor RECORD
INSERT INTO [dbo].Donors 
(Contact_ID,Statement_Frequency_ID,Statement_Type_ID,Statement_Method_ID,Setup_Date                ,Envelope_No,Cancel_Envelopes,Notes,First_Contact_Made,Domain_ID,__ExternalPersonID,_First_Donation_Date,_Last_Donation_Date,Processor_ID) VALUES
(@contactID,3                     ,1                ,4                  ,{ts '2015-07-06 12:03:37'},null       ,0               ,'Scripted Donor' ,null              ,1        ,null              ,null                ,null               ,@processorID);

--Contact_Household record
INSERT INTO Contact_Households 
(Contact_ID,Household_ID,Household_Position_ID,Household_Type_ID,Primary_Family,Notes,End_Date,Domain_ID) VALUES
(@contactID,@houseHoldID,1                    ,1                ,null          ,null ,null    ,1        );

--Fred Flintstone Updates
update [dbo].Contacts set Donor_Record = (select donor_id from donors where contact_id = @contactID) where Contact_ID = @contactID;
update [dbo].Contacts set Participant_Record = @participantID where CONTACT_ID = @contactID;
update [dbo].Contacts set User_Account = @userAccount where Contact_ID = @contactID;
update [dbo].Participants set Contact_id = @contactID where participant_id = @participantID;
update [dbo].dp_users set Contact_id = @contactId where user_id = @userAccount;
GO

--Household for Wilma Flintstone
Declare @contactID as int --old contact ID
Set @contactID = (select contact_id from contacts where email_address = 'mpcrds+auto+wilmaflintstone@gmail.com' and last_name = 'Flintstone');

DECLARE @houseHoldID as int
set @houseHoldID = (select houseHold_ID from Contacts where email_address = 'mpcrds+auto+fredflintstone@gmail.com' and last_name = 'Flintstone');

--Participant Record for Wilma
Declare @participantID as int
set @participantID = (select participant_record from contacts where email_address = 'mpcrds+auto+wilmaflintstone@gmail.com' and last_name = 'Flintstone');

--User Account for Wilma
Declare @userAccount as int
set @userAccount = (select user_account from contacts where contact_id = @contactID);

--Update old contact record so we can delete it. 
UPDATE [dbo].Contacts
SET Household_ID = null, Participant_Record = null, User_Account = null
WHERE contact_id = @contactID;

--Temporarily update the participant and user account records - Please don't fail.
UPDATE [dbo].Participants 
SET Contact_ID = 1
WHERE Participant_ID = @participantID;

UPDATE [dbo].dp_users
SET Contact_ID = 1
WHERE USER_ID = @userAccount;

--Just get rid of this so we can delete Wilma's old contact record
DECLARE @communicationID as int
set @communicationID = (Select Communication_ID from dp_Communications where TO_CONTACT = @contactID);

DELETE From [dbo].CONTACT_HOUSEHOLDS
WHERE CONTACT_ID = @contactID;

DELETE from [dbo].dp_commands 
WHERE communication_id = @communicationID;

DELETE from [dbo].dp_Contact_Publications 
WHERE contact_id = @contactID;

DELETE from [dbo].dp_communication_messages 
WHERE Communication_ID = @communicationID;

Delete from [dbo].dp_Communications
WHERE Communication_ID = @communicationID;

Delete from [dbo].Activity_Log
WHERE Contact_id = @contactID;

--Delete the old contact record
DELETE FROM [dbo].Contacts where contact_id = @contactID;

SET IDENTITY_INSERT [dbo].[Contacts] ON;

--Store the current identity value so we can reset it.
DECLARE @currentContactId as int
set @currentContactId = IDENT_CURRENT('Contacts');

set @contactID = 100000011;

--Wilma Flintstone contact record
INSERT INTO [dbo].Contacts 
(Contact_ID,Company,Company_Name,Display_Name    ,Prefix_ID,First_Name,Middle_Name,Last_Name ,Suffix_ID,Nickname,Date_of_Birth   ,Gender_ID,Marital_Status_ID,Contact_Status_ID,Household_ID,Household_Position_ID,Participant_Record,Donor_Record,Email_Address                  ,Email_Unlisted,Bulk_Email_Opt_Out,Bulk_SMS_Opt_Out,Mobile_Phone  ,Mobile_Carrier,Mobile_Phone_Unlisted,Company_Phone,Pager_Phone,Fax_Phone,User_Account,Web_Page,Remove_From_Directory,Industry_ID,Occupation_ID,Employer_Name,[SSN/EIN],Anniversary_Date,HS_Graduation_Year,Current_School,Contact_GUID,ID_Card,Domain_ID,__ShelbyID,__ExternalHouseholdID,__ExternalPersonID,__ExternalUserID,__ExternalBusinessID,Maiden_Name,__LastLegacyLogin,__LegacyUserName,__LegacyUserID,__LegacyEmailAddress) VALUES
(@contactID,0      ,null        ,'Flintstone, Wilma',2        ,'Wilma'    ,'A'        ,'Flintstone',null     ,'Wilma'  ,{d '1975-01-01'},2        ,2                ,1                ,@houseHoldID,1                    ,@participantID    ,null        ,'mpcrds+auto+wilmaflintstone@gmail.com',null          ,0                 ,0               ,'321-444-8184',null          ,null                 ,null         ,null       ,null     ,@userAccount,null    ,null                 ,null       ,null         ,null         ,null     ,null            ,null              ,null          ,NEWID()     ,null   ,1        ,null      ,null                 ,null              ,null            ,null                ,null       ,null             ,null            ,null          ,null                );

SET IDENTITY_INSERT [dbo].[Contacts] OFF;

--This command resets the identity value so that if someone adds contacts a big ID is not used. 
DBCC CHECKIDENT (Contacts, reseed, @currentContactId);

DECLARE @processorID as varchar(255);

IF ((SELECT @@SERVERNAME) like '%demo%')
	SET @processorID = 'cus_A5SjBRrHWHDuMC';
ELSE
	SET @processorID = 'cus_A5SNLFyZ8tADNJ';


--Wilma Flintstone Donor RECORD
INSERT INTO [dbo].Donors 
(Contact_ID,Statement_Frequency_ID,Statement_Type_ID,Statement_Method_ID,Setup_Date                ,Envelope_No,Cancel_Envelopes,Notes,First_Contact_Made,Domain_ID,__ExternalPersonID,_First_Donation_Date,_Last_Donation_Date,Processor_ID        ) VALUES
(@contactID,1                     ,1                ,2                  ,{ts '2015-07-06 12:03:37'},null       ,0               ,'Scripted Donor' ,null              ,1        ,null              ,null                ,null               ,@processorID);

--Contact_Household record
INSERT INTO Contact_Households 
(Contact_ID,Household_ID,Household_Position_ID,Household_Type_ID,Primary_Family,Notes,End_Date,Domain_ID) VALUES
(@contactID,@houseHoldID,1                    ,1                ,null          ,null ,null    ,1        );

--Wilma Flintstone Updates
update [dbo].Contacts set Donor_Record = (select donor_id from donors where contact_id = @contactID) where Contact_ID = @contactID;
update [dbo].Contacts set Participant_Record = @participantID where contact_id = @contactID;
update [dbo].Contacts set User_Account = @userAccount where contact_id = @contactID;
update [dbo].Participants set contact_id = @contactID where participant_id = @participantID;
update [dbo].Dp_users set contact_id = @contactID where User_Id = @userAccount;
GO

--Pebbles Flintstone (age 14)
Declare @contactID as int
Set @contactID = (select contact_id from contacts where email_address = 'mpcrds+auto+pebblesflintstone@gmail.com' and last_name = 'Flintstone');

DECLARE @houseHoldID as int
set @houseHoldID = (select houseHold_ID from contacts where email_address = 'mpcrds+auto+fredflintstone@gmail.com' and last_name = 'Flintstone');

--Participant Record for Pebbles
Declare @participantID as int
set @participantID = (select participant_record from contacts where contact_id = @contactID);

--User Account for Pebbles
Declare @userAccount as int
set @userAccount = (select user_account from contacts where contact_id = @contactId);

--Update old contact record so we can delete it. 
UPDATE [dbo].Contacts
SET Household_ID = null, Participant_Record = null, User_Account = null
WHERE contact_id = @contactID;

--Temporarily update the participant and user account records - Please don't fail.
UPDATE [dbo].Participants 
SET Contact_ID = 1
WHERE Participant_ID = @participantID;

UPDATE [dbo].dp_users
SET Contact_ID = 1
WHERE USER_ID = @userAccount;

--Just get rid of this so we can delete Pebbles's old contact record
DECLARE @communicationID as int
set @communicationID = (Select Communication_ID from dp_Communications where TO_CONTACT = @contactID);

DELETE From [dbo].CONTACT_HOUSEHOLDS
WHERE CONTACT_ID = @contactID;

DELETE from [dbo].dp_commands 
WHERE communication_id = @communicationID;

DELETE from [dbo].dp_Contact_Publications 
WHERE contact_id = @contactID;

DELETE from [dbo].dp_communication_messages 
WHERE Communication_ID = @communicationID;

Delete from [dbo].dp_Communications
WHERE Communication_ID = @communicationID;

Delete from [dbo].Activity_Log
WHERE Contact_id = @contactID;

--Delete the old contact record
DELETE FROM [dbo].Contacts where email_address = 'mpcrds+auto+pebblesflintstone@gmail.com' and last_name = 'Flintstone';

--Pebbles Flintstone Contact
SET IDENTITY_INSERT [dbo].[Contacts] ON;

--Store the current identity value so we can reset it.
DECLARE @currentContactId as int
set @currentContactId = IDENT_CURRENT('Contacts');

set @contactID = 100000012;

INSERT INTO [dbo].Contacts 
(Contact_ID,Company,Company_Name,Display_Name      ,Prefix_ID,First_Name,Middle_Name,Last_Name,Suffix_ID,Nickname ,Date_of_Birth   ,Gender_ID,Marital_Status_ID,Contact_Status_ID,Household_ID,Household_Position_ID,Participant_Record,Donor_Record,Email_Address                    ,Email_Unlisted,Bulk_Email_Opt_Out,Bulk_SMS_Opt_Out,Mobile_Phone  ,Mobile_Carrier,Mobile_Phone_Unlisted,Company_Phone,Pager_Phone,Fax_Phone,User_Account,Web_Page,Remove_From_Directory,Industry_ID,Occupation_ID,Employer_Name,[SSN/EIN],Anniversary_Date,HS_Graduation_Year,Current_School,Contact_GUID,ID_Card,Domain_ID,__ShelbyID,__ExternalHouseholdID,__ExternalPersonID,__ExternalUserID,__ExternalBusinessID,Maiden_Name,__LastLegacyLogin,__LegacyUserName,__LegacyUserID,__LegacyEmailAddress) VALUES
(@contactID,0      ,null        ,'Flintstone, Pebbles',null     ,'Pebbles' ,null       ,'Flintstone',null     ,'Pebbles',{d '2001-01-01'},2        ,1                ,1                ,@houseHoldID,2                    ,@participantID    ,null        ,'mpcrds+auto+pebblesflintstone@gmail.com',null          ,0                 ,0               ,'123-548-4232',null          ,null                 ,null         ,null       ,null     ,@userAccount,null    ,null                 ,null       ,null         ,null         ,null     ,null            ,null              ,null          ,NEWID()     ,null   ,1        ,null      ,null                 ,null              ,null            ,null                ,null       ,null             ,null            ,null          ,null                );

SET IDENTITY_INSERT [dbo].[Contacts] OFF;

--This command resets the identity value so that if someone adds contacts a big ID. 
DBCC CHECKIDENT (Contacts, reseed, @currentContactId);

-- Pebbles Flintstone Donor RECORD
INSERT INTO [dbo].Donors 
(Contact_ID,Statement_Frequency_ID,Statement_Type_ID,Statement_Method_ID,Setup_Date                ,Envelope_No,Cancel_Envelopes,Notes,First_Contact_Made,Domain_ID,__ExternalPersonID,_First_Donation_Date,_Last_Donation_Date,Processor_ID) VALUES
(@contactID,3                     ,1                ,4                  ,{ts '2015-07-06 12:03:37'},null       ,0               ,null ,null              ,1        ,null              ,null                ,null               ,null);

--Contact_Household record
INSERT INTO Contact_Households 
(Contact_ID,Household_ID,Household_Position_ID,Household_Type_ID,Primary_Family,Notes,End_Date,Domain_ID) VALUES
(@contactID,@houseHoldID,1                    ,2                ,null          ,null ,null    ,1        );

--Pebbles Flintstone Updates
update [dbo].Contacts set Donor_Record = (select donor_id from donors where contact_id = @contactID) where Contact_ID = @contactID;
update [dbo].Contacts set Participant_Record = @participantID where contact_id = @contactID;
update [dbo].Contacts set User_Account = @userAccount where contact_id = @contactID;
update [dbo].Participants set Contact_ID = @contactID where participant_id = @participantID;
update [dbo].Dp_users set contact_id = @contactID where user_id = @userAccount;
GO

--Fred Flintstone's Request to Join Kids' Club Response Record
SET IDENTITY_INSERT [dbo].[Responses] ON;

DECLARE @respID as int
set @respID = IDENT_CURRENT('Responses')+1;

DECLARE @partID as int
set @partID = (select Participant_Record from Contacts where email_address = 'mpcrds+auto+fredflintstone@gmail.com');

INSERT INTO [dbo].Responses 
(Response_ID,Response_Date             ,Opportunity_ID,Participant_ID,Comments                        ,Website_Submission,First_Name,Last_Name,Email,Phone,Follow_up_Information,Response_Result_ID,Closed,Domain_ID,Event_ID,__ExternalCESTPID) VALUES
(@respID    ,{ts '2015-07-02 08:17:17'},115           ,@partID       ,'Request on 7/2/2015 8:17:17 AM',null              ,null      ,null     ,null ,null ,null                 ,null              ,0     ,1        ,null    ,null             );

SET IDENTITY_INSERT [dbo].[Responses] OFF;
GO

--Pebbles Flintstone's Request to Join Kids' Club Response Record
SET IDENTITY_INSERT [dbo].[Responses] ON;

DECLARE @respID as int
set @respID = IDENT_CURRENT('Responses')+1;

DECLARE @partID as int
set @partID = (select Participant_Record from Contacts where email_address = 'mpcrds+auto+pebblesflintstone@gmail.com');

INSERT INTO [dbo].Responses 
(Response_ID,Response_Date             ,Opportunity_ID,Participant_ID,Comments                        ,Website_Submission,First_Name,Last_Name,Email,Phone,Follow_up_Information,Response_Result_ID,Closed,Domain_ID,Event_ID,__ExternalCESTPID) VALUES
(@respID    ,{ts '2015-07-02 08:20:21'},115           ,@partID       ,'Request on 7/2/2015 8:20:21 AM',null              ,null      ,null     ,null ,null ,null                 ,null              ,0 ,1        ,null    ,null             )

SET IDENTITY_INSERT [dbo].[Responses] OFF;
GO

--Family Relationships
DECLARE @fredContact as int
set @fredContact = (select Contact_ID from Contacts where email_address = 'mpcrds+auto+fredflintstone@gmail.com');

DECLARE @wilmaContact as int
set @wilmaContact = (select Contact_ID from Contacts where email_address = 'mpcrds+auto+wilmaflintstone@gmail.com');

DECLARE @pebblesContact as int
set @pebblesContact = (select Contact_ID from Contacts where email_address = 'mpcrds+auto+pebblesflintstone@gmail.com');


--Fred married to Wilma
INSERT INTO [dbo].Contact_Relationships 
(Contact_ID ,Relationship_ID,Related_Contact_ID,Start_Date                ,End_Date,Domain_ID,Notes                       ,_Triggered_By) VALUES
(@fredContact,1              ,@wilmaContact       ,{ts '2015-07-02 08:15:06'},null    ,1        ,'Created by Add Family Tool',null      );

--Fred Foster Parent of Pebbles Parent of Kids
INSERT INTO [dbo].Contact_Relationships 
(Contact_ID   ,Relationship_ID,Related_Contact_ID,Start_Date,End_Date,Domain_ID,Notes,_Triggered_By) VALUES
(@fredContact,21           ,@pebblesContact     ,null      ,null    ,1        ,null ,null      );


--Wilma foster Parent Of Pebbles
INSERT INTO [dbo].Contact_Relationships 
(Contact_ID  ,Relationship_ID,Related_Contact_ID,Start_Date,End_Date,Domain_ID,Notes,_Triggered_By) VALUES
(@wilmaContact,21             ,@pebblesContact   ,null      ,null    ,1        ,null ,null      );

SET IDENTITY_INSERT [dbo].[Contact_Relationships] OFF;
GO

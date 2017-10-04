USE [MinistryPlatform]
GO

--Get the required data to add to our contact. 
Declare @contactID as int
Set @contactID = (select contact_id from contacts where email_address = 'mpcrds+cloudstrife@gmail.com' and last_name = 'Strife');

Declare @houseHoldID as int
set @houseHoldID = (select houseHold_ID from contacts where contact_id = @contactID);

Declare @participantID as int
set @participantID = (select participant_record from contacts where contact_id = @contactID);

Declare @userAccount as int
set @userAccount = (select user_account from contacts where contact_id = @contactID);

--Update old contact record so we can delete it. 
UPDATE [dbo].Contacts
SET Household_ID = null, Participant_Record = null, User_Account = null
WHERE email_address = 'mpcrds+cloudstrife@gmail.com' and last_name = 'Strife';

--Temporarily update the participant and user account records - Please don't fail.
UPDATE [dbo].Participants 
SET Contact_ID = 1
WHERE Participant_ID = @participantID;

UPDATE [dbo].dp_users
SET Contact_ID = 1
WHERE USER_ID = @userAccount;

--Just get rid of this so we can delete Cloud's old contact record
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

--Delete the old contact record for cloud
DELETE FROM [dbo].Contacts where Contact_ID = @contactID;

--Address
SET IDENTITY_INSERT [dbo].[Addresses] ON;

DECLARE @addressId as int
set @addressId = IDENT_CURRENT('Addresses')+1;

INSERT INTO [dbo].Addresses 
(Address_ID,Address_Line_1        ,Address_Line_2,City  ,[State/Region],Postal_Code,Foreign_Country,Country_Code,Domain_ID,Carrier_Route,Lot_Number,Delivery_Point_Code,Delivery_Point_Check_Digit,Latitude,Longitude,Altitude,Time_Zone,Bar_Code,Area_Code,Last_Validation_Attempt,County,Validated,Do_Not_Validate,Last_GeoCode_Attempt,__ExternalAddressID) VALUES
(@addressID,'123 Midgar Section 6',null          ,'CITY','OH'          ,'45209'    ,'United States','USA'       ,1        ,null         ,null      ,null               ,null                      ,null    ,null     ,null    ,null     ,null    ,null     ,null                   ,null  ,null     ,null           ,null                ,null               );

SET IDENTITY_INSERT [dbo].[Addresses] OFF;

--Household updates
UPDATE [dbo].Households 
SET Address_ID = @addressID, Home_Phone = '123-765-4323', Congregation_ID = 6
WHERE houseHold_ID = @houseHoldID;

--Cloud Strife Contact
SET IDENTITY_INSERT [dbo].[Contacts] ON;

--Store the current identity value so we can reset it.
DECLARE @currentContactId as int
set @currentContactId = IDENT_CURRENT('Contacts');

set @contactID = 100000000;

INSERT INTO [dbo].Contacts 
(Contact_ID,Company,Company_Name,Display_Name   ,Prefix_ID,First_Name,Middle_Name,Last_Name   ,Suffix_ID,Nickname ,Date_of_Birth   ,Gender_ID,Marital_Status_ID,Contact_Status_ID,Household_ID,Household_Position_ID,Participant_Record,Donor_Record,Email_Address                  ,Email_Unlisted,Bulk_Email_Opt_Out,Bulk_SMS_Opt_Out,Mobile_Phone  ,Mobile_Carrier,Mobile_Phone_Unlisted,Company_Phone ,Pager_Phone,Fax_Phone,User_Account,Web_Page,Remove_From_Directory,Industry_ID,Occupation_ID,Employer_Name,[SSN/EIN],Anniversary_Date,HS_Graduation_Year,Current_School,Contact_GUID ,ID_Card,Domain_ID,__ShelbyID,__ExternalHouseholdID,__ExternalPersonID,__ExternalUserID,__ExternalBusinessID,Maiden_Name,__LastLegacyLogin,__LegacyUserName,__LegacyUserID,__LegacyEmailAddress) VALUES
(@contactID,0      ,null        ,'Strife, Cloud',1        ,'Cloud'   ,'S'        ,'Strife'    ,null     ,'Cloud'  ,{d '1975-01-01'},1        ,2                ,1                ,@houseHoldID,1                    ,@participantID    ,null        ,'mpcrds+cloudstrife@gmail.com' ,null          ,0                 ,0               ,'513-654-8745',null          ,null                 ,'555-365-4125',null       ,null     ,@userAccount,null    ,null                 ,null       ,null         ,null         ,null     ,null            ,null              ,null          ,NEWID()      ,null   ,1        ,null      ,null                 ,null              ,null            ,null                ,null       ,null             ,null            ,null          ,null                );

SET IDENTITY_INSERT [dbo].[Contacts] OFF;

--This command resets the identity value so that if someone adds contacts a big ID. 
DBCC CHECKIDENT (Contacts, reseed, @currentContactId);

DECLARE @processorID as varchar(255);

IF (SELECT URL from DP_Bookmarks where name like '%demo%') is not null
	SET @processorID = 'cus_8gGoksUHAuJgQi';
ELSE
	SET @processorID = 'cus_8gGm4dhM3ul1Cp';

-- Cloud Strife Donor RECORD
INSERT INTO [dbo].Donors 
(Contact_ID,Statement_Frequency_ID,Statement_Type_ID,Statement_Method_ID,Setup_Date                ,Envelope_No,Cancel_Envelopes,Notes,First_Contact_Made,Domain_ID,__ExternalPersonID,_First_Donation_Date,_Last_Donation_Date,Processor_ID) VALUES
(@contactID,3                     ,1                ,4                  ,{ts '2015-07-06 12:03:37'},null       ,0               ,'Scripted Donor' ,null              ,1        ,null              ,null                ,null               ,@processorID);

--Contact_Household record
INSERT INTO Contact_Households 
(Contact_ID,Household_ID,Household_Position_ID,Household_Type_ID,Primary_Family,Notes,End_Date,Domain_ID) VALUES
(@contactID,@houseHoldID,1                    ,1                ,null          ,null ,null    ,1        );

--Cloud Strife Updates
update [dbo].Contacts set Donor_Record = (select donor_id from donors where contact_id = @contactID) where Contact_ID = @contactID;
update [dbo].Contacts set Participant_Record = @participantID where CONTACT_ID = @contactID;
update [dbo].Contacts set User_Account = @userAccount where Contact_ID = @contactID;
update [dbo].Participants set Contact_id = @contactID where participant_id = @participantID;
update [dbo].dp_users set Contact_id = @contactId where user_id = @userAccount;
GO

--Household for Tifa Lockhart
Declare @contactID as int --old contact ID
Set @contactID = (select contact_id from contacts where email_address = 'mpcrds+tifalockhart@gmail.com' and last_name = 'Lockhart');

DECLARE @houseHoldID as int
set @houseHoldID = (select houseHold_ID from Contacts where email_address = 'mpcrds+cloudstrife@gmail.com' and last_name = 'Strife');

--Participant Record for Tifa
Declare @participantID as int
set @participantID = (select participant_record from contacts where email_address = 'mpcrds+tifalockhart@gmail.com' and last_name = 'Lockhart');

--User Account for Tifa
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

--Just get rid of this so we can delete Tifa's old contact record
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

set @contactID = 100000001;

--Tifa Lockhart contact record
INSERT INTO [dbo].Contacts 
(Contact_ID,Company,Company_Name,Display_Name    ,Prefix_ID,First_Name,Middle_Name,Last_Name ,Suffix_ID,Nickname,Date_of_Birth   ,Gender_ID,Marital_Status_ID,Contact_Status_ID,Household_ID,Household_Position_ID,Participant_Record,Donor_Record,Email_Address                  ,Email_Unlisted,Bulk_Email_Opt_Out,Bulk_SMS_Opt_Out,Mobile_Phone  ,Mobile_Carrier,Mobile_Phone_Unlisted,Company_Phone,Pager_Phone,Fax_Phone,User_Account,Web_Page,Remove_From_Directory,Industry_ID,Occupation_ID,Employer_Name,[SSN/EIN],Anniversary_Date,HS_Graduation_Year,Current_School,Contact_GUID,ID_Card,Domain_ID,__ShelbyID,__ExternalHouseholdID,__ExternalPersonID,__ExternalUserID,__ExternalBusinessID,Maiden_Name,__LastLegacyLogin,__LegacyUserName,__LegacyUserID,__LegacyEmailAddress) VALUES
(@contactID,0      ,null        ,'Lockhart, Tifa',2        ,'Tifa'    ,'A'        ,'Lockhart',null     ,'Tifa'  ,{d '1975-01-01'},2        ,2                ,1                ,@houseHoldID,1                    ,@participantID    ,null        ,'mpcrds+tifalockhart@gmail.com',null          ,0                 ,0               ,'321-444-8184',null          ,null                 ,null         ,null       ,null     ,@userAccount,null    ,null                 ,null       ,null         ,null         ,null     ,null            ,null              ,null          ,NEWID()     ,null   ,1        ,null      ,null                 ,null              ,null            ,null                ,null       ,null             ,null            ,null          ,null                );

SET IDENTITY_INSERT [dbo].[Contacts] OFF;

--This command resets the identity value so that if someone adds contacts a big ID is not used. 
DBCC CHECKIDENT (Contacts, reseed, @currentContactId);

DECLARE @processorID as varchar(255);

IF (SELECT URL from DP_Bookmarks where name like '%demo%') is not null
	SET @processorID = 'cus_85UsQReBytr2dn';
ELSE
	SET @processorID = 'cus_8gGvY5NF7fPjI8';


--Tifa Lockhart Donor RECORD
INSERT INTO [dbo].Donors 
(Contact_ID,Statement_Frequency_ID,Statement_Type_ID,Statement_Method_ID,Setup_Date                ,Envelope_No,Cancel_Envelopes,Notes,First_Contact_Made,Domain_ID,__ExternalPersonID,_First_Donation_Date,_Last_Donation_Date,Processor_ID        ) VALUES
(@contactID,1                     ,1                ,2                  ,{ts '2015-07-06 12:03:37'},null       ,0               ,'Scripted Donor' ,null              ,1        ,null              ,null                ,null               ,'');

--Contact_Household record
INSERT INTO Contact_Households 
(Contact_ID,Household_ID,Household_Position_ID,Household_Type_ID,Primary_Family,Notes,End_Date,Domain_ID) VALUES
(@contactID,@houseHoldID,1                    ,1                ,null          ,null ,null    ,1        );

--Tifa Lockhart Updates
update [dbo].Contacts set Donor_Record = (select donor_id from donors where contact_id = @contactID) where Contact_ID = @contactID;
update [dbo].Contacts set Participant_Record = @participantID where contact_id = @contactID;
update [dbo].Contacts set User_Account = @userAccount where contact_id = @contactID;
update [dbo].Participants set contact_id = @contactID where participant_id = @participantID;
update [dbo].Dp_users set contact_id = @contactID where User_Id = @userAccount;
GO

--Marlene Wallace (age 14)
Declare @contactID as int
Set @contactID = (select contact_id from contacts where email_address = 'mpcrds+marlenewallace@gmail.com' and last_name = 'Wallace');

DECLARE @houseHoldID as int
set @houseHoldID = (select houseHold_ID from contacts where email_address = 'mpcrds+cloudstrife@gmail.com' and last_name = 'Strife');

--Participant Record for Marlene
Declare @participantID as int
set @participantID = (select participant_record from contacts where contact_id = @contactID);

--User Account for Marlene
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

--Just get rid of this so we can delete Marlene's old contact record
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
DELETE FROM [dbo].Contacts where email_address = 'mpcrds+marlenewallace@gmail.com' and last_name = 'Wallace';

--Marlene Wallace Contact
SET IDENTITY_INSERT [dbo].[Contacts] ON;

--Store the current identity value so we can reset it.
DECLARE @currentContactId as int
set @currentContactId = IDENT_CURRENT('Contacts');

set @contactID = 100000002;

INSERT INTO [dbo].Contacts 
(Contact_ID,Company,Company_Name,Display_Name      ,Prefix_ID,First_Name,Middle_Name,Last_Name,Suffix_ID,Nickname ,Date_of_Birth   ,Gender_ID,Marital_Status_ID,Contact_Status_ID,Household_ID,Household_Position_ID,Participant_Record,Donor_Record,Email_Address                    ,Email_Unlisted,Bulk_Email_Opt_Out,Bulk_SMS_Opt_Out,Mobile_Phone  ,Mobile_Carrier,Mobile_Phone_Unlisted,Company_Phone,Pager_Phone,Fax_Phone,User_Account,Web_Page,Remove_From_Directory,Industry_ID,Occupation_ID,Employer_Name,[SSN/EIN],Anniversary_Date,HS_Graduation_Year,Current_School,Contact_GUID,ID_Card,Domain_ID,__ShelbyID,__ExternalHouseholdID,__ExternalPersonID,__ExternalUserID,__ExternalBusinessID,Maiden_Name,__LastLegacyLogin,__LegacyUserName,__LegacyUserID,__LegacyEmailAddress) VALUES
(@contactID,0      ,null        ,'Wallace, Marlene',null     ,'Marlene' ,null       ,'Wallace',null     ,'Marlene',{d '2001-01-01'},2        ,1                ,1                ,@houseHoldID,2                    ,@participantID    ,null        ,'mpcrds+marlenewallace@gmail.com',null          ,0                 ,0               ,'123-548-4232',null          ,null                 ,null         ,null       ,null     ,@userAccount,null    ,null                 ,null       ,null         ,null         ,null     ,null            ,null              ,null          ,NEWID()     ,null   ,1        ,null      ,null                 ,null              ,null            ,null                ,null       ,null             ,null            ,null          ,null                );

SET IDENTITY_INSERT [dbo].[Contacts] OFF;

--This command resets the identity value so that if someone adds contacts a big ID. 
DBCC CHECKIDENT (Contacts, reseed, @currentContactId);

-- Marlene Wallace Donor RECORD
INSERT INTO [dbo].Donors 
(Contact_ID,Statement_Frequency_ID,Statement_Type_ID,Statement_Method_ID,Setup_Date                ,Envelope_No,Cancel_Envelopes,Notes,First_Contact_Made,Domain_ID,__ExternalPersonID,_First_Donation_Date,_Last_Donation_Date,Processor_ID) VALUES
(@contactID,3                     ,1                ,4                  ,{ts '2015-07-06 12:03:37'},null       ,0               ,null ,null              ,1        ,null              ,null                ,null               ,null);

--Contact_Household record
INSERT INTO Contact_Households 
(Contact_ID,Household_ID,Household_Position_ID,Household_Type_ID,Primary_Family,Notes,End_Date,Domain_ID) VALUES
(@contactID,@houseHoldID,1                    ,2                ,null          ,null ,null    ,1        );

--Marlene Wallace Updates
update [dbo].Contacts set Donor_Record = (select donor_id from donors where contact_id = @contactID) where Contact_ID = @contactID;
update [dbo].Contacts set Participant_Record = @participantID where contact_id = @contactID;
update [dbo].Contacts set User_Account = @userAccount where contact_id = @contactID;
update [dbo].Participants set Contact_ID = @contactID where participant_id = @participantID;
update [dbo].Dp_users set contact_id = @contactID where user_id = @userAccount;
GO

--Cloud Strife's Request to Join Kids' Club Response Record
SET IDENTITY_INSERT [dbo].[Responses] ON;

DECLARE @respID as int
set @respID = IDENT_CURRENT('Responses')+1;

DECLARE @partID as int
set @partID = (select Participant_Record from Contacts where email_address = 'mpcrds+cloudstrife@gmail.com');

INSERT INTO [dbo].Responses 
(Response_ID,Response_Date             ,Opportunity_ID,Participant_ID,Comments                        ,Website_Submission,First_Name,Last_Name,Email,Phone,Follow_up_Information,Response_Result_ID,Closed,Domain_ID,Event_ID,__ExternalCESTPID) VALUES
(@respID    ,{ts '2015-07-02 08:17:17'},115           ,@partID       ,'Request on 7/2/2015 8:17:17 AM',null              ,null      ,null     ,null ,null ,null                 ,null              ,0     ,1        ,null    ,null             );

SET IDENTITY_INSERT [dbo].[Responses] OFF;
GO

--Marlene Wallace's Request to Join Kids' Club Response Record
SET IDENTITY_INSERT [dbo].[Responses] ON;

DECLARE @respID as int
set @respID = IDENT_CURRENT('Responses')+1;

DECLARE @partID as int
set @partID = (select Participant_Record from Contacts where email_address = 'mpcrds+marlenewallace@gmail.com');

INSERT INTO [dbo].Responses 
(Response_ID,Response_Date             ,Opportunity_ID,Participant_ID,Comments                        ,Website_Submission,First_Name,Last_Name,Email,Phone,Follow_up_Information,Response_Result_ID,Closed,Domain_ID,Event_ID,__ExternalCESTPID) VALUES
(@respID    ,{ts '2015-07-02 08:20:21'},115           ,@partID       ,'Request on 7/2/2015 8:20:21 AM',null              ,null      ,null     ,null ,null ,null                 ,null              ,0 ,1        ,null    ,null             )

SET IDENTITY_INSERT [dbo].[Responses] OFF;
GO

--Family Relationships
DECLARE @cloudContact as int
set @cloudContact = (select Contact_ID from Contacts where email_address = 'mpcrds+cloudstrife@gmail.com');

DECLARE @tifaContact as int
set @tifaContact = (select Contact_ID from Contacts where email_address = 'mpcrds+tifalockhart@gmail.com');

DECLARE @marleneContact as int
set @marleneContact = (select Contact_ID from Contacts where email_address = 'mpcrds+marlenewallace@gmail.com');


--Cloud married to Tifa
INSERT INTO [dbo].Contact_Relationships 
(Contact_ID ,Relationship_ID,Related_Contact_ID,Start_Date                ,End_Date,Domain_ID,Notes                       ,_Triggered_By) VALUES
(@cloudContact,1              ,@tifaContact       ,{ts '2015-07-02 08:15:06'},null    ,1        ,'Created by Add Family Tool',null      );

--Cloud Foster Parent of Marlene Parent of Kids
INSERT INTO [dbo].Contact_Relationships 
(Contact_ID   ,Relationship_ID,Related_Contact_ID,Start_Date,End_Date,Domain_ID,Notes,_Triggered_By) VALUES
(@cloudContact,21           ,@marleneContact     ,null      ,null    ,1        ,null ,null      );


--Tifa foster Parent Of Marlene
INSERT INTO [dbo].Contact_Relationships 
(Contact_ID  ,Relationship_ID,Related_Contact_ID,Start_Date,End_Date,Domain_ID,Notes,_Triggered_By) VALUES
(@tifaContact,21             ,@marleneContact   ,null      ,null    ,1        ,null ,null      );

SET IDENTITY_INSERT [dbo].[Contact_Relationships] OFF;
GO

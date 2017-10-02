USE [MinistryPlatform]
GO


DECLARE @fatherContactId AS INT
DECLARE @fatherDOB DATE

DECLARE @motherContactId AS INT
DECLARE @child8ContactId AS INT
DECLARE @child4ContactId AS INT
DECLARE @child2ContactId AS INT

DECLARE @currentContactId AS INT

DECLARE @motherDOB DATE
DECLARE @child8DOB DATE
DECLARE @child4DOB DATE
DECLARE @child2DOB DATE

DECLARE @fatherParticipantId AS INT
DECLARE @motherParticipantId AS INT
DECLARE @child8ParticipantId AS INT
DECLARE @child4ParticipantId AS INT
DECLARE @child2ParticipantId AS INT

DECLARE @currentParticipantId AS INT

DECLARE @fContactId AS INT
DECLARE @mContactId AS INT
-------------------------------------------------------------------------------------------------------------------------------------------
--Set Current contact record to 100000030--
SET @currentContactId = IDENT_CURRENT('Contacts');

SET IDENTITY_INSERT [dbo].[Contacts] ON;

SET @fContactId = 100000030;

INSERT INTO Contacts 
(Contact_ID,Company,Company_Name,Display_Name,Prefix_ID,First_Name,Middle_Name,Last_Name,Suffix_ID,Nickname ,Date_of_Birth   ,Gender_ID,Marital_Status_ID,Contact_Status_ID,Household_ID,Household_Position_ID,Participant_Record,Donor_Record,Email_Address,Email_Unlisted,Bulk_Email_Opt_Out,Bulk_SMS_Opt_Out,Mobile_Phone  ,Mobile_Carrier,Mobile_Phone_Unlisted,Company_Phone,Pager_Phone,Fax_Phone,User_Account,Web_Page,Remove_From_Directory,Industry_ID,Occupation_ID,Employer_Name,[SSN/EIN],Anniversary_Date,HS_Graduation_Year,Current_School,Contact_GUID,ID_Card,Domain_ID,__ShelbyID,__ExternalHouseholdID,__ExternalPersonID,__ExternalUserID,__ExternalBusinessID,Maiden_Name,__LastLegacyLogin,__LegacyUserName,__LegacyUserID,__LegacyEmailAddress) VALUES
(@fContactId,0      ,null        ,'temp'      ,2        ,'temp'    ,null       ,'Temp'   ,null     ,'temp'   ,{d '1975-01-01'},1        ,1                ,1                ,null        ,3                    ,null              ,null        ,null         ,null          ,0                 ,0               ,''           ,null          ,null                 ,null         ,null       ,null     ,null        ,null    ,null                 ,null       ,null         ,null         ,null     ,null            ,null              ,null          ,NEWID()     ,null   ,1        ,null      ,null                 ,null              ,null            ,null                ,null       ,null             ,null            ,null          ,null                );

SET @mContactId = 100000031;

INSERT INTO Contacts 
(Contact_ID,Company,Company_Name,Display_Name,Prefix_ID,First_Name,Middle_Name,Last_Name,Suffix_ID,Nickname ,Date_of_Birth   ,Gender_ID,Marital_Status_ID,Contact_Status_ID,Household_ID,Household_Position_ID,Participant_Record,Donor_Record,Email_Address,Email_Unlisted,Bulk_Email_Opt_Out,Bulk_SMS_Opt_Out,Mobile_Phone  ,Mobile_Carrier,Mobile_Phone_Unlisted,Company_Phone,Pager_Phone,Fax_Phone,User_Account,Web_Page,Remove_From_Directory,Industry_ID,Occupation_ID,Employer_Name,[SSN/EIN],Anniversary_Date,HS_Graduation_Year,Current_School,Contact_GUID,ID_Card,Domain_ID,__ShelbyID,__ExternalHouseholdID,__ExternalPersonID,__ExternalUserID,__ExternalBusinessID,Maiden_Name,__LastLegacyLogin,__LegacyUserName,__LegacyUserID,__LegacyEmailAddress) VALUES
(@mContactId,0      ,null        ,'temp'      ,2        ,'temp'    ,null       ,'Temp'   ,null     ,'temp'   ,{d '1975-01-01'},1        ,1                ,1                ,null        ,3                    ,null              ,null        ,null         ,null          ,0                 ,0               ,''	      ,null          ,null                 ,null         ,null       ,null     ,null        ,null    ,null                 ,null       ,null         ,null         ,null     ,null            ,null              ,null          ,NEWID()     ,null   ,1        ,null      ,null                 ,null              ,null            ,null                ,null       ,null             ,null            ,null          ,null                );

DBCC CHECKIDENT (Contacts, reseed, @currentContactId);
SET IDENTITY_INSERT [dbo].[Contacts] OFF;
DBCC CHECKIDENT (Contacts, reseed, @currentContactId);


--------------------------------------------------------------------------------------------------------------------------------------------
--Get the required data to add to our contact. 

SET @fatherContactId = (SELECT Contact_ID FROM Contacts WHERE Email_Address = 'mpcrds+auto+georgesmith@gmail.com' and Last_Name = 'Smith');
SET @motherContactId = (SELECT Contact_ID FROM Contacts WHERE Email_Address = 'mpcrds+auto+suesmith@gmail.com' and Last_Name = 'Smith');

DECLARE @fatheHouseholdId AS INT
SET @fatheHouseholdId = (SELECT Household_ID FROM Contacts WHERE Contact_ID = @fatherContactId);

SET @fatherParticipantId = (SELECT Participant_Record FROM Contacts WHERE Contact_ID = @fatherContactId);
SET @motherParticipantId = (SELECT Participant_Record FROM Contacts WHERE Contact_ID = @motherContactId);

DECLARE @fatherUserAccount AS INT
SET @fatherUserAccount = (SELECT User_Account FROM Contacts WHERE Contact_ID = @fatherContactId);
DECLARE @motherUserAccount AS INT
SET @motherUserAccount = (SELECT User_Account FROM Contacts WHERE Contact_ID = @motherContactId);


--Update old contact record so we can delete it. 
UPDATE [dbo].Contacts
SET Household_ID = null, Participant_Record = null, User_Account = null
WHERE email_address = 'mpcrds+auto+georgesmith@gmail.com' and last_name = 'Smith';
UPDATE [dbo].Contacts
SET Household_ID = null, Participant_Record = null, User_Account = null
WHERE email_address = 'mpcrds+auto+suesmith@gmail.com' and last_name = 'Smith';

--Temporarily update the participant and user account records - Please don't fail.
UPDATE [dbo].Participants 
SET Contact_ID = @fContactId WHERE Participant_ID = @fatherParticipantId;
UPDATE [dbo].Participants 
SET Contact_ID = @mContactId WHERE Participant_ID = @motherParticipantId;

UPDATE [dbo].dp_users
SET Contact_ID = @fContactId WHERE USER_ID = @fatherUserAccount;
UPDATE [dbo].dp_users
SET Contact_ID = @mContactId WHERE USER_ID = @motherUserAccount;

--Just get rid of this so we can delete George's old contact record
DELETE FROM Contact_Households WHERE Contact_ID = @fatherContactId;
DELETE FROM Contact_Households WHERE Contact_ID = @motherContactId;

DECLARE @fatherCommunicationId AS INT
SET @fatherCommunicationId = (SELECT Communication_ID FROM dp_Communications WHERE TO_CONTACT = @fatherContactId);
DECLARE @motherCommunicationId AS INT
SET @motherCommunicationId = (SELECT Communication_ID FROM dp_Communications WHERE TO_CONTACT = @motherContactId);

DELETE FROM [dbo].dp_commands WHERE Communication_ID = @fatherCommunicationId;
DELETE FROM [dbo].dp_Contact_Publications WHERE Contact_ID = @fatherContactId;
DELETE FROM [dbo].dp_communication_messages WHERE Communication_ID = @fatherCommunicationId;
DELETE FROM [dbo].dp_Communications WHERE Communication_ID = @fatherCommunicationId;
DELETE FROM [dbo].Activity_Log WHERE Contact_iD = @fatherContactId;
DELETE FROM [dbo].dp_commands WHERE Communication_ID = @motherCommunicationId;
DELETE FROM [dbo].dp_Contact_Publications WHERE Contact_ID = @motherContactId;
DELETE FROM [dbo].dp_communication_messages WHERE Communication_ID = @motherCommunicationId;
DELETE FROM [dbo].dp_Communications WHERE Communication_ID = @motherCommunicationId;
DELETE FROM [dbo].Activity_Log WHERE Contact_iD = @motherContactId;

--Delete the old contact record for George
DELETE FROM [dbo].Contacts where Contact_ID = @fatherContactId;
DELETE FROM [dbo].Contacts where Contact_ID = @motherContactId;
-----------------------------------------------------------------------------------------------------------------------------

--Insert New Address
DECLARE @currentAddressId AS INT
SET @currentAddressId = IDENT_CURRENT('Addresses');

SET IDENTITY_INSERT [dbo].[Addresses] ON;

DECLARE @addressId AS INT
SET @addressId = 100000030;

INSERT INTO Addresses 
(Address_ID,Address_Line_1         ,Address_Line_2,City    ,[State/Region],Postal_Code,Foreign_Country,Country_Code,Domain_ID,Carrier_Route,Lot_Number,Delivery_Point_Code,Delivery_Point_Check_Digit,Latitude,Longitude,Altitude,Time_Zone,Bar_Code,Area_Code,Last_Validation_Attempt,County,Validated,Do_Not_Validate,Last_GeoCode_Attempt,__ExternalAddressID) VALUES
(@addressID,'12 Ridge Ave.',null          ,'Oakley','OH'          ,'45067'    ,'United States','USA'       ,1        ,null         ,null      ,null               ,null                      ,null    ,null     ,null    ,null     ,null    ,null     ,null                   ,null  ,null     ,null           ,null                ,null               );

DBCC CHECKIDENT (Addresses, reseed, @currentAddressId);
SET IDENTITY_INSERT [dbo].[Addresses] OFF;
DBCC CHECKIDENT (Addresses, reseed, @currentAddressId);
-------------------------------------------------------------------------------------------------------------------------------------------

--Insert new Household

DECLARE @currenthouseholdId AS INT
SET @currentHouseholdId = IDENT_CURRENT('Households');

SET IDENTITY_INSERT [dbo].[Households] ON;

DECLARE @householdID AS INT
SET @householdId = 100000030;

INSERT INTO Households 
(Household_ID,Household_Name,Address_ID  ,Home_Phone      ,Domain_ID,Congregation_ID  ,Care_Person, Household_Source_ID ,Family_Call_Number, Household_Preferences     ,Home_Phone_Unlisted   , Home_Address_Unlisted, Bulk_Mail_Opt_Out, _Last_Donation, _Last_Activity, __ExternalHouseholdID, __ExternalBusinessID) VALUES
(@householdId,'Smith'    ,@addressId  	 ,'555-963-5090'  ,1        ,6                ,null       , 48                ,null              , null                      ,null                  , null                 , 0                ,null           ,null           ,null                  , null);

DBCC CHECKIDENT (Households, reseed, @currentHouseholdId);
SET IDENTITY_INSERT [dbo].[Households] OFF;
DBCC CHECKIDENT (Households, reseed, @currentHouseholdId);
-------------------------------------------------------------------------------------------------------------------------------------------
--Update father Contact Record

SET @fatherContactId = @fContactId;
SET @fatherDOB = DATEADD(year, -40, GETDATE());

UPDATE [dbo].Contacts 
SET Display_Name = 'Smith,George', Prefix_ID = 1,First_Name = 'George',  Middle_Name = null, Last_Name = 'Smith', Nickname = 'George', Date_of_Birth = @fatherDOB , Gender_ID = 1, Marital_Status_ID = 2, Household_ID = @householdId, Household_Position_ID = 1,Participant_Record = @fatherParticipantId, Email_Address = 'mpcrds+auto+georgesmith@gmail.com', Mobile_Phone = '555-963-5033', Company_Phone = null
WHERE Contact_ID = @fatherContactID;

SET @motherContactId = @mContactId;
SET @motherDOB = DATEADD(year, -38, GETDATE());

UPDATE [dbo].Contacts 
SET Display_Name = 'Smith,Sue', Prefix_ID = 2,First_Name = 'Sue',  Middle_Name = null, Last_Name = 'Smith', Nickname = 'Sue', Date_of_Birth = @motherDOB , Gender_ID = 2, Marital_Status_ID = 2, Household_ID = @householdId, Household_Position_ID = 1,Participant_Record = @motherParticipantId, Email_Address = 'mpcrds+auto+suesmith@gmail.com', Mobile_Phone = '555-963-5032', Company_Phone = null
WHERE Contact_ID = @motherContactID;


--Insert new Family members

SET @currentContactId = IDENT_CURRENT('Contacts');

SET IDENTITY_INSERT [dbo].[Contacts] ON;

SET @child8ContactId = ((SELECT MAX(Contact_ID) FROM Contacts)+1);
SET @child8DOB = DATEADD(year, -8, GETDATE());
INSERT INTO Contacts 
(Contact_ID      ,Company,Company_Name,Display_Name    ,Prefix_ID,First_Name,Middle_Name,Last_Name ,Suffix_ID,Nickname ,Date_of_Birth   ,Gender_ID,Marital_Status_ID,Contact_Status_ID,Household_ID,Household_Position_ID,Participant_Record,Donor_Record,Email_Address                    ,Email_Unlisted,Bulk_Email_Opt_Out,Bulk_SMS_Opt_Out,Mobile_Phone  ,Mobile_Carrier,Mobile_Phone_Unlisted,Company_Phone,Pager_Phone,Fax_Phone,User_Account,Web_Page,Remove_From_Directory,Industry_ID,Occupation_ID,Employer_Name,[SSN/EIN],Anniversary_Date,HS_Graduation_Year,Current_School,Contact_GUID,ID_Card,Domain_ID,__ShelbyID,__ExternalHouseholdID,__ExternalPersonID,__ExternalUserID,__ExternalBusinessID,Maiden_Name,__LastLegacyLogin,__LegacyUserName,__LegacyUserID,__LegacyEmailAddress) VALUES
(@child8ContactId,0      ,null       ,'Smith,Frank',2        ,'Frank'   ,null       ,'Smith',null     ,'Frank'         ,@child8DOB      ,1        ,1                ,1                ,@householdId,2                    ,null              ,null        ,'mpcrds+auto+franksmith@gmail.com',null          ,0                 ,0               ,'',null          ,null                 ,null         ,null       ,null     ,null        ,null    ,null                 ,null       ,null         ,null         ,null     ,null            ,null              ,null          ,NEWID()     ,null   ,1        ,null      ,null                 ,null              ,null            ,null                ,null       ,null             ,null            ,null          ,null                );

SET @child4ContactId = ((SELECT MAX(Contact_ID) FROM Contacts)+1);
SET @child4DOB = DATEADD(year, -4, GETDATE());
INSERT INTO Contacts 
(Contact_ID      ,Company,Company_Name,Display_Name         ,Prefix_ID,First_Name  ,Middle_Name,Last_Name ,Suffix_ID,Nickname ,Date_of_Birth   ,Gender_ID,Marital_Status_ID,Contact_Status_ID,Household_ID,Household_Position_ID,Participant_Record,Donor_Record,Email_Address                     ,Email_Unlisted,Bulk_Email_Opt_Out,Bulk_SMS_Opt_Out,Mobile_Phone  ,Mobile_Carrier,Mobile_Phone_Unlisted,Company_Phone,Pager_Phone,Fax_Phone,User_Account,Web_Page,Remove_From_Directory,Industry_ID,Occupation_ID,Employer_Name,[SSN/EIN],Anniversary_Date,HS_Graduation_Year,Current_School,Contact_GUID,ID_Card,Domain_ID,__ShelbyID,__ExternalHouseholdID,__ExternalPersonID,__ExternalUserID,__ExternalBusinessID,Maiden_Name,__LastLegacyLogin,__LegacyUserName,__LegacyUserID,__LegacyEmailAddress) VALUES
(@child4ContactId,0      ,null        ,'Smith,Zoe'    ,2        ,'Zoe'    ,null       ,'Smith',null     ,'Zoe'                ,@child4DOB      ,2        ,1                ,1                ,@householdId,2                    ,null              ,null        ,'mpcrds+auto+zoesmith@gmail.com',null          ,0                 ,0               ,'',null          ,null                 ,null         ,null       ,null     ,null        ,null    ,null                 ,null       ,null         ,null         ,null     ,null            ,null              ,null          ,NEWID()     ,null   ,1        ,null      ,null                 ,null              ,null            ,null                ,null       ,null             ,null            ,null          ,null                );

SET @child2ContactId = ((SELECT MAX(Contact_ID) FROM Contacts)+1);
SET @child2DOB = DATEADD(year, -2, GETDATE());
INSERT INTO Contacts 
(Contact_ID      ,Company,Company_Name,Display_Name,Prefix_ID,First_Name  ,Middle_Name,Last_Name ,Suffix_ID,Nickname ,Date_of_Birth ,Gender_ID,Marital_Status_ID,Contact_Status_ID,Household_ID,Household_Position_ID,Participant_Record,Donor_Record,Email_Address                     ,Email_Unlisted,Bulk_Email_Opt_Out,Bulk_SMS_Opt_Out,Mobile_Phone  ,Mobile_Carrier,Mobile_Phone_Unlisted,Company_Phone,Pager_Phone,Fax_Phone,User_Account,Web_Page,Remove_From_Directory,Industry_ID,Occupation_ID,Employer_Name,[SSN/EIN],Anniversary_Date,HS_Graduation_Year,Current_School,Contact_GUID,ID_Card,Domain_ID,__ShelbyID,__ExternalHouseholdID,__ExternalPersonID,__ExternalUserID,__ExternalBusinessID,Maiden_Name,__LastLegacyLogin,__LegacyUserName,__LegacyUserID,__LegacyEmailAddress) VALUES
(@child2ContactId,0      ,null        ,'Smith,Jim' ,2        ,'Jim'       ,null       ,'Smith'   ,null     ,'Jim'    ,@child2DOB    ,1        ,1                ,1                ,@householdId,2                    ,null              ,null        ,'mpcrds+auto+jimsmith@gmail.com'  ,null          ,0                 ,0               ,''            ,null          ,null                 ,null         ,null       ,null     ,null        ,null    ,null                 ,null       ,null         ,null         ,null     ,null            ,null              ,null          ,NEWID()     ,null   ,1        ,null      ,null                 ,null              ,null            ,null                ,null       ,null             ,null            ,null          ,null                );

DBCC CHECKIDENT (Contacts, reseed, @currentContactId);
SET IDENTITY_INSERT [dbo].[Contacts] OFF;
DBCC CHECKIDENT (Contacts, reseed, @currentContactId);

-------------------------------------------------------------------------------------------------------------------------------------------
--Insert Participant records

SET IDENTITY_INSERT [dbo].[Participants] ON;


SET @child8ParticipantId = ((SELECT MAX(Participant_ID) FROM Participants) +1);
INSERT INTO Participants 
( Participant_ID          , Contact_ID       , Participant_Type_ID,Participant_Start_Date, Participant_End_Date, Notes, Domain_ID, __ExternalPersonID, _First_Attendance_Ever, _Second_Attendance_Ever, _Third_Attendance_Ever, _Last_Attendance_Ever) VALUES
( @child8ParticipantId   , @child8ContactId, 1                  ,'01/01/2015'          , null                , null , 1        , null              , null                  ,  null                  ,  null                 ,null                  );

SET @child4ParticipantId = ((SELECT MAX(Participant_ID) FROM Participants) +1);
INSERT INTO Participants 
( Participant_ID         , Contact_ID      , Participant_Type_ID, Participant_Start_Date, Participant_End_Date, Notes, Domain_ID, __ExternalPersonID, _First_Attendance_Ever, _Second_Attendance_Ever, _Third_Attendance_Ever, _Last_Attendance_Ever) VALUES
( @child4ParticipantId   , @child4ContactId, 1                  , '01/01/2015'          , null                , null , 1        , null              , null                  ,  null                  ,  null                 ,null                  );

SET @child2ParticipantId = ((SELECT MAX(Participant_ID) FROM Participants) +1);
INSERT INTO Participants 
( Participant_ID         , Contact_ID      , Participant_Type_ID, Participant_Start_Date, Participant_End_Date, Notes, Domain_ID, __ExternalPersonID, _First_Attendance_Ever, _Second_Attendance_Ever, _Third_Attendance_Ever, _Last_Attendance_Ever) VALUES
( @child2ParticipantId   , @child2ContactId, 1                  , '01/01/2015'          , null                , null , 1        , null              , null                  ,  null                  ,  null                 ,null                  );

SET IDENTITY_INSERT [dbo].[Participants] OFF;

--Update Contact record with participant records


UPDATE [dbo].Contacts 
SET Participant_Record = @child8ParticipantId
WHERE Contact_ID = @child8ContactId;


UPDATE [dbo].Contacts 
SET Participant_Record = @child4ParticipantId
WHERE Contact_ID = @child4ContactId;

UPDATE [dbo].Contacts 
SET Participant_Record = @child2ParticipantId
WHERE Contact_ID = @child2ContactId;
-------------------------------------------------------------------------------------------------------------------------------------------

--Update the relationships between family members

SET IDENTITY_INSERT [dbo].[Contact_Relationships] OFF;

INSERT INTO [dbo].Contact_Relationships 
(Contact_ID      ,Relationship_ID,Related_Contact_ID,Start_Date  ,End_Date,Domain_ID,Notes                       ,_Triggered_By) VALUES
(@fatherContactId,1              ,@motherContactId  ,null        ,null    ,1        ,'Created by Add Family Tool',null      );

INSERT INTO [dbo].Contact_Relationships 
(Contact_ID      ,Relationship_ID,Related_Contact_ID,Start_Date,End_Date,Domain_ID,Notes,_Triggered_By) VALUES
(@fatherContactId,6              ,@child8ContactId ,null      ,null    ,1        ,null ,null      );

INSERT INTO [dbo].Contact_Relationships 
(Contact_ID      ,Relationship_ID,Related_Contact_ID,Start_Date,End_Date,Domain_ID,Notes,_Triggered_By) VALUES
(@fatherContactId,6              ,@child4ContactId  ,null      ,null    ,1        ,null ,null      );

INSERT INTO [dbo].Contact_Relationships 
(Contact_ID      ,Relationship_ID,Related_Contact_ID,Start_Date,End_Date,Domain_ID,Notes,_Triggered_By) VALUES
(@fatherContactId,6              ,@child2ContactId  ,null      ,null    ,1        ,null ,null      );

INSERT INTO [dbo].Contact_Relationships 
(Contact_ID      ,Relationship_ID,Related_Contact_ID,Start_Date,End_Date,Domain_ID,Notes,_Triggered_By) VALUES
(@motherContactId,6              ,@child8ContactId ,null      ,null    ,1        ,null ,null      );

INSERT INTO [dbo].Contact_Relationships 
(Contact_ID      ,Relationship_ID,Related_Contact_ID,Start_Date,End_Date,Domain_ID,Notes,_Triggered_By) VALUES
(@motherContactId,6              ,@child4ContactId  ,null      ,null    ,1        ,null ,null      );

INSERT INTO [dbo].Contact_Relationships 
(Contact_ID      ,Relationship_ID,Related_Contact_ID,Start_Date,End_Date,Domain_ID,Notes,_Triggered_By) VALUES
(@motherContactId,6              ,@child2ContactId ,null      ,null    ,1        ,null ,null      );

INSERT INTO [dbo].Contact_Relationships 
(Contact_ID      ,Relationship_ID,Related_Contact_ID,Start_Date,End_Date,Domain_ID,Notes,_Triggered_By) VALUES
(@child8ContactId,2              ,@child4ContactId  ,null      ,null    ,1        ,null ,null      );

INSERT INTO [dbo].Contact_Relationships 
(Contact_ID      ,Relationship_ID,Related_Contact_ID,Start_Date,End_Date,Domain_ID,Notes,_Triggered_By) VALUES
(@child8ContactId,2              ,@child2ContactId  ,null      ,null    ,1        ,null ,null      );

INSERT INTO [dbo].Contact_Relationships 
(Contact_ID      ,Relationship_ID,Related_Contact_ID,Start_Date,End_Date,Domain_ID,Notes,_Triggered_By) VALUES
(@child4ContactId,2              ,@child2ContactId  ,null      ,null    ,1        ,null ,null      );

SET IDENTITY_INSERT [dbo].[Contact_Relationships] OFF;
-------------------------------------------------------------------------------------------------------------------------------------------

--Create donor records for family

INSERT INTO [dbo].Donors 
(Contact_ID      ,Statement_Frequency_ID,Statement_Type_ID,Statement_Method_ID,Setup_Date                ,Envelope_No,Cancel_Envelopes,Notes,First_Contact_Made,Domain_ID,__ExternalPersonID,_First_Donation_Date,_Last_Donation_Date,Processor_ID) VALUES
(@fatherContactId,3                     ,1                ,4                  ,{ts '2015-07-06 12:03:37'},null       ,0               ,null ,null              ,1        ,null              ,null                ,null               ,null);

INSERT INTO [dbo].Donors 
(Contact_ID      ,Statement_Frequency_ID,Statement_Type_ID,Statement_Method_ID,Setup_Date                ,Envelope_No,Cancel_Envelopes,Notes,First_Contact_Made,Domain_ID,__ExternalPersonID,_First_Donation_Date,_Last_Donation_Date,Processor_ID) VALUES
(@motherContactId,3                     ,1                ,4                  ,{ts '2015-07-06 12:03:37'},null       ,0               ,null ,null              ,1        ,null              ,null                ,null               ,null);


-------------------------------------------------------------------------------------------------------------------------------------------

--Create Group Participants for the children
declare @Child8GroupID as int
set @Child8GroupID = (select group_id from Groups Where Group_Name = 'Kids Club Grade 3')

INSERT INTO Group_Participants
(Group_ID       ,Participant_ID          ,Group_Role_ID  ,Domain_ID,Start_Date   ,End_Date,Employee_Role,Hours_Per_Week,Notes,__ExternalPersonGroupRoleID,__ExternalGroupRoleID,__CanManageEvents,__CanMANageMembers,__EmailOptOut,__ISAnonymous,__ServiceTimeID,_First_Attendance,_Second_Attendance,_Third_Attendance,_Last_Attendance,Child_Care_Requested,Share_With_Group,Email_Opt_Out,Need_Book,Preferred_Serving_Time_ID,Enrolled_By,Auto_Promote) VALUES
(@Child8GroupID ,@child8ParticipantId    ,16             ,1        ,'01/01/2015' , null   ,0         ,null          ,null ,null                       ,null                 ,null             ,null              ,null         ,null         ,null           ,null             ,null              ,null             ,null            ,0                   ,null            ,null         ,0        ,null                     ,null       ,1);

declare @Child4GroupID as int
set @Child4GroupID = (select group_id from Groups Where Group_Name = 'Kids Club 4 Year Old March')

INSERT INTO Group_Participants
(Group_ID        ,Participant_ID        ,Group_Role_ID  ,Domain_ID,Start_Date   ,End_Date,Employee_Role,Hours_Per_Week,Notes,__ExternalPersonGroupRoleID,__ExternalGroupRoleID,__CanManageEvents,__CanMANageMembers,__EmailOptOut,__ISAnonymous,__ServiceTimeID,_First_Attendance,_Second_Attendance,_Third_Attendance,_Last_Attendance,Child_Care_Requested,Share_With_Group,Email_Opt_Out,Need_Book,Preferred_Serving_Time_ID,Enrolled_By,Auto_Promote) VALUES
(@Child4GroupID  ,@child4ParticipantId  ,16             ,1       ,'01/01/2015'  ,null    ,0            ,null          ,null ,null                       ,null                 ,null             ,null              ,null         ,null         ,null           ,null             ,null              ,null             ,null            ,0                   ,null            ,null         ,0        ,null                     ,null       ,1);

declare @Child2GroupID as int
set @Child2GroupID = (select group_id from Groups Where Group_Name = 'Kids Club 2 Year Old March')

INSERT INTO Group_Participants
(Group_ID       ,Participant_ID          ,Group_Role_ID  ,Domain_ID,Start_Date   ,End_Date,Employee_Role,Hours_Per_Week,Notes,__ExternalPersonGroupRoleID,__ExternalGroupRoleID,__CanManageEvents,__CanMANageMembers,__EmailOptOut,__ISAnonymous,__ServiceTimeID,_First_Attendance,_Second_Attendance,_Third_Attendance,_Last_Attendance,Child_Care_Requested,Share_With_Group,Email_Opt_Out,Need_Book,Preferred_Serving_Time_ID,Enrolled_By,Auto_Promote) VALUES
(@Child2GroupID ,@child2ParticipantId    ,16             ,1        ,'01/01/2015' , null   ,0         ,null          ,null ,null                       ,null                 ,null             ,null              ,null         ,null         ,null           ,null             ,null              ,null             ,null            ,0                   ,null            ,null         ,0        ,null                     ,null       ,1);

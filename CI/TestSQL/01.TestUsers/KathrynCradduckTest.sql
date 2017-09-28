USE [MinistryPlatform]
GO

--kcradduck+testing@crossroads.net Contact (providing last name filters out the Guest Giver record)
DECLARE @contactID as int
set @contactID = (select contact_id from contacts where Email_Address = 'kcradduck+testing@crossroads.net' and Last_Name = 'Cradduck-Test');

UPDATE [dbo].Contacts
SET Prefix_ID = 1, Middle_Name = 'Ann', Date_of_Birth = {d '1983-01-01'}, Gender_ID = 1, Marital_Status_ID = 2, Mobile_Phone = '513-654-8745',Company_Phone = '555-365-4125'
WHERE contact_id = @contactID;

--Address
SET IDENTITY_INSERT [dbo].[Addresses] ON;

DECLARE @addressId as int
set @addressId = IDENT_CURRENT('Addresses')+1;

INSERT INTO [dbo].Addresses 
(Address_ID,Address_Line_1             ,Address_Line_2,City  ,[State/Region],Postal_Code,Foreign_Country,Country_Code,Domain_ID,Carrier_Route,Lot_Number,Delivery_Point_Code,Delivery_Point_Check_Digit,Latitude,Longitude,Altitude,Time_Zone,Bar_Code,Area_Code,Last_Validation_Attempt,County    ,Validated,Do_Not_Validate,Last_GeoCode_Attempt,__ExternalAddressID) VALUES
(@addressID,'1234 Kathyrn Testing Lane',null          ,'CITY','OH'          ,'45209'    ,'United States','USA'       ,1        ,null         ,null      ,null               ,null                      ,null    ,null     ,null    ,null     ,null    ,null     ,null                   ,'County!' ,null     ,null           ,null                ,null               );

SET IDENTITY_INSERT [dbo].[Addresses] OFF;

-- Kathyrn Household
DECLARE @houseHoldID as int
set @houseHoldID = (select Household_ID from contacts where contact_id = @contactID);

UPDATE [dbo].HouseHolds
SET Address_ID = @addressID, Home_Phone = '123-867-5309', Congregation_ID = 6
WHERE Household_ID = @houseHoldID;

--Kathyrn donor
INSERT INTO [dbo].donors 
(Contact_ID,Statement_Frequency_ID,Statement_Type_ID,Statement_Method_ID,Setup_Date                    ,Envelope_No,Cancel_Envelopes,Notes,First_Contact_Made,Domain_ID,__ExternalPersonID,_First_Donation_Date,_Last_Donation_Date,Processor_ID        ) VALUES
(@contactID,1                     ,1                ,2                  ,{ts '2015-08-05 12:26:57.503'},null       ,0               ,'Scripted Donor' ,null              ,1        ,null              ,null                ,null               ,'cus_7I3CcWLFnecIAV');

DECLARE @userID as int
set @userID = (select user_account from contacts where contact_id= @contactID);

INSERT INTO [dbo].dp_user_roles 
(User_ID,Role_ID,Domain_ID) VALUES
(@userId,2      ,1        );

INSERT INTO [dbo].dp_user_roles 
(User_ID,Role_ID,Domain_ID) VALUES
(@userId,77     ,1        );

INSERT INTO [dbo].dp_user_roles 
(User_ID,Role_ID,Domain_ID) VALUES
(@userId,5      ,1        );

INSERT INTO [dbo].dp_user_roles 
(User_ID,Role_ID,Domain_ID) VALUES
(@userId,6      ,1        );

INSERT INTO [dbo].dp_user_roles 
(User_ID,Role_ID,Domain_ID) VALUES
(@userId,7      ,1        );

INSERT INTO [dbo].dp_user_roles 
(User_ID,Role_ID,Domain_ID) VALUES
(@userId,60     ,1        );

DECLARE @donor_id as int
set @donor_id = (Select donor_ID from donors where contact_id = @contactID);

--Kathyrn Cradduck-Test Updates
update [dbo].Contacts set Donor_Record = @donor_id where contact_id = @contactID;
GO

--Contact for kcradduck+husband@crossroads.net
DECLARE @contactID as int
set @contactID = (select contact_id from contacts where Email_Address = 'kcradduck+husband@crossroads.net' and Last_Name = 'Cradduck-Test');

--Household for Husband Cradduck-Test
DECLARE @houseHoldID as int
set @houseHoldID = (select HouseHold_ID from Contacts where email_address = 'kcradduck+testing@crossroads.net' and last_name = 'cradduck-test');

--Update Contact Record
UPDATE [dbo].Contacts 
SET Prefix_ID = 2, Middle_Name = 'A', Gender_ID = 2, Marital_Status_ID = 2, Household_ID = @houseHoldID, Date_of_Birth = {d '1975-01-01'}, Mobile_Phone = '321-654-8184'
WHERE Contact_ID = @contactID;

--Husband Donor
INSERT INTO [dbo].donors 
(Contact_ID,Statement_Frequency_ID,Statement_Type_ID,Statement_Method_ID,Setup_Date                    ,Envelope_No,Cancel_Envelopes,Notes,First_Contact_Made,Domain_ID,__ExternalPersonID,_First_Donation_Date,_Last_Donation_Date,Processor_ID        ) VALUES
(@contactID,1                     ,1                ,2                  ,{ts '2015-08-05 12:26:57.503'},null       ,0               ,null ,null              ,1        ,null              ,null                ,null               ,null                );

DECLARE @donor_id as int
set @donor_id = (Select donor_ID from donors where contact_id = @contactID);

--Husband Cradduck-Test Updates
update [dbo].Contacts set Donor_Record = @donor_id where contact_id = @contactID;
GO

--Contact for kcradduck+kid14@crossroads.net
DECLARE @contactID as int
set @contactID = (select contact_id from contacts where Email_Address = 'kcradduck+kid14@crossroads.net' and Last_Name = 'Cradduck-Test');

--Household for Kid14 Cradduck-Test
DECLARE @houseHoldID as int
set @houseHoldID = (select HouseHold_ID from Contacts where email_address = 'kcradduck+testing@crossroads.net' and Last_Name = 'Cradduck-Test');

--Kid14 Cradduck-Test Contact Updates
UPDATE[dbo].Contacts 
SET Date_of_Birth = {d '2001-01-01'}, Gender_ID = 2, Marital_Status_ID = 1, Household_Position_ID = 2, HouseHold_ID = @houseHoldID, Mobile_Phone = '321-548-6154'
WHERE Contact_ID = @contactID;
GO

--Kid17 Cradduck-Test Contact Record
DECLARE @contactID as int
set @contactID = (select contact_id from contacts where Email_Address = 'kcradduck+kid17@crossroads.net' and Last_Name = 'Cradduck-Test');

--Household for Kid17 Cradduck-Test
DECLARE @houseHoldID as int
set @houseHoldID = (select HouseHold_ID from Contacts where email_address = 'kcradduck+testing@crossroads.net' and Last_Name = 'Cradduck-Test');

--Kid17 Cradduck-Test Contact Updates
UPDATE [dbo].Contacts 
SET Middle_Name = 'D', Date_of_Birth = {d '1998-01-01'}, Marital_Status_ID = 1, HouseHold_ID = @houseHoldID, Household_Position_ID = 4, Mobile_Phone = '654-818-1425'
WHERE contact_id = @contactID;
GO

--Kathyrn Cradduck-Test's Request to Join Kids' Club Response Record
DECLARE @partID as int
set @partID = (select Participant_Record from Contacts where email_address = 'kcradduck+testing@crossroads.net' and last_name = 'Cradduck-Test');

INSERT INTO [dbo].Responses 
(Response_Date             ,Opportunity_ID,Participant_ID,Comments                        ,Website_Submission,First_Name,Last_Name,Email,Phone,Follow_up_Information,Response_Result_ID,Closed,Domain_ID,Event_ID,__ExternalCESTPID) VALUES
({ts '2015-07-02 08:17:17'},115           ,@partID       ,'Request on 7/2/2015 8:17:17 AM',null              ,null      ,null     ,null ,null ,null                 ,null              ,0     ,1        ,null    ,null             );
GO

--Kid14 Cradduck-Test's Request to Join Kids' Club Response Record
DECLARE @partID as int
set @partID = (select Participant_Record from Contacts where email_address = 'kcradduck+kid14@crossroads.net' and last_name = 'Cradduck-Test');

INSERT INTO [dbo].Responses 
(Response_Date             ,Opportunity_ID,Participant_ID,Comments                        ,Website_Submission,First_Name,Last_Name,Email,Phone,Follow_up_Information,Response_Result_ID,Closed,Domain_ID,Event_ID,__ExternalCESTPID) VALUES
({ts '2015-07-02 08:20:21'},115           ,@partID       ,'Request on 7/2/2015 8:20:21 AM',null              ,null      ,null     ,null ,null ,null                 ,null              ,0 ,1        ,null    ,null             )

GO

--Family Relationships
DECLARE @momContact as int
set @momContact = (select Contact_ID from Contacts where email_address = 'kcradduck+testing@crossroads.net' and last_name = 'Cradduck-Test');

DECLARE @dadContact as int
set @dadContact = (select Contact_ID from Contacts where email_address = 'kcradduck+husband@crossroads.net' and last_name = 'Cradduck-Test');

DECLARE @kid14Contact as int
set @kid14Contact = (select Contact_ID from Contacts where email_address = 'kcradduck+kid14@crossroads.net' and last_name = 'Cradduck-Test');

DECLARE @kid17Contact as int
set @kid17Contact = (select Contact_ID from Contacts where email_address = 'kcradduck+kid17@crossroads.net' and last_name = 'Cradduck-Test');

--Dad Married to Mom
INSERT INTO [dbo].Contact_Relationships 
(Contact_ID ,Relationship_ID,Related_Contact_ID,Start_Date                ,End_Date,Domain_ID,Notes                       ,_Triggered_By) VALUES
(@dadContact,1              ,@momContact       ,{ts '2015-07-02 08:15:06'},null    ,1        ,'Created by Add Family Tool',null      );

--Dad Parent of Kids
INSERT INTO [dbo].Contact_Relationships 
(Contact_ID,Relationship_ID,Related_Contact_ID,Start_Date,End_Date,Domain_ID,Notes,_Triggered_By) VALUES
(@dadContact   ,6              ,@kid14Contact ,null      ,null    ,1        ,null ,null      );

INSERT INTO [dbo].Contact_Relationships 
(Contact_ID ,Relationship_ID,Related_Contact_ID,Start_Date,End_Date,Domain_ID,Notes,_Triggered_By) VALUES
(@dadContact,6              ,@kid17Contact     ,null      ,null    ,1        ,null ,null      );

--Mom Parent Of kids
INSERT INTO [dbo].Contact_Relationships 
(Contact_ID ,Relationship_ID,Related_Contact_ID,Start_Date,End_Date,Domain_ID,Notes,_Triggered_By) VALUES
(@momContact,6              ,@kid14Contact     ,null      ,null    ,1        ,null ,null      );

INSERT INTO [dbo].Contact_Relationships 
(Contact_ID ,Relationship_ID,Related_Contact_ID,Start_Date,End_Date,Domain_ID,Notes,_Triggered_By) VALUES
(@momContact,6              ,@kid17Contact     ,null      ,null    ,1        ,null ,null      );

--Kids Siblings
INSERT INTO [dbo].Contact_Relationships 
(Contact_ID   ,Relationship_ID,Related_Contact_ID,Start_Date,End_Date,Domain_ID,Notes,_Triggered_By) VALUES
(@kid14Contact,2              ,@kid17Contact     ,null      ,null    ,1        ,null ,null      );

SET IDENTITY_INSERT [dbo].[Contact_Relationships] OFF;
GO

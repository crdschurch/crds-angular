USE [MinistryPlatform]
GO

--Rtheil+testing@crossroads.net Contact
DECLARE @contactID as int
set @contactID = (select contact_id from contacts where Email_Address = 'rtheil+Testing@crossroads.net' and Last_Name = 'Theil-Test');

UPDATE [dbo].Contacts
SET Prefix_ID = 1, Middle_Name = 'Brian', Date_of_Birth = {d '1975-01-01'}, Gender_ID = 1, Marital_Status_ID = 2, Mobile_Phone = '513-654-8745',Company_Phone = '555-365-4125'
WHERE contact_id = @contactID;

--Address
SET IDENTITY_INSERT [dbo].[Addresses] ON;

DECLARE @addressId as int
set @addressId = IDENT_CURRENT('Addresses')+1;

INSERT INTO [dbo].Addresses 
(Address_ID,Address_Line_1          ,Address_Line_2,City  ,[State/Region],Postal_Code,Foreign_Country,Country_Code,Domain_ID,Carrier_Route,Lot_Number,Delivery_Point_Code,Delivery_Point_Check_Digit,Latitude,Longitude,Altitude,Time_Zone,Bar_Code,Area_Code,Last_Validation_Attempt,County    ,Validated,Do_Not_Validate,Last_GeoCode_Attempt,__ExternalAddressID) VALUES
(@addressID,'1234 Rich Testing Lane',null          ,'CITY','OH'          ,'45209'    ,'United States','USA'       ,1        ,null         ,null      ,null               ,null                      ,null    ,null     ,null    ,null     ,null    ,null     ,null                   ,'County!' ,null     ,null           ,null                ,null               );

SET IDENTITY_INSERT [dbo].[Addresses] OFF;

-- Rich Household
DECLARE @houseHoldID as int
set @houseHoldID = (select Household_ID from contacts where contact_id = @contactID);

UPDATE [dbo].HouseHolds
SET Address_ID = @addressID, Home_Phone = '123-867-5309', Congregation_ID = 6
WHERE Household_ID = @houseHoldID;

--Rich donor
INSERT INTO [dbo].donors 
(Contact_ID,Statement_Frequency_ID,Statement_Type_ID,Statement_Method_ID,Setup_Date                    ,Envelope_No,Cancel_Envelopes,Notes,First_Contact_Made,Domain_ID,__ExternalPersonID,_First_Donation_Date,_Last_Donation_Date,Processor_ID        ) VALUES
(@contactID,1                     ,1                ,2                  ,{ts '2015-08-05 12:26:57.503'},null       ,0               ,'Scripted Donor' ,null              ,1        ,null              ,null                ,null               ,'cus_7I3CcWLFnecIAV');

DECLARE @donor_id as int
set @donor_id = (Select donor_ID from donors where contact_id = @contactID);

--Rich Theil-Test Updates
update [dbo].Contacts set Donor_Record = @donor_id where contact_id = @contactID;
GO

--Contact for rtheil+wife@crossroads.net
DECLARE @contactID as int
set @contactID = (select contact_id from contacts where Email_Address = 'rtheil+wife@crossroads.net' and Last_Name = 'Theil-Test');

--Household for Wife Theil-Test
DECLARE @houseHoldID as int
set @houseHoldID = (select HouseHold_ID from Contacts where email_address = 'rtheil+testing@crossroads.net');

--Update Contact Record
UPDATE [dbo].Contacts 
SET Prefix_ID = 2, Middle_Name = 'A', Gender_ID = 2, Marital_Status_ID = 2, Household_ID = @houseHoldID, Date_of_Birth = {d '1975-01-01'}, Mobile_Phone = '321-654-8184'
WHERE Contact_ID = @contactID;

--Wife Donor
INSERT INTO [dbo].donors 
(Contact_ID,Statement_Frequency_ID,Statement_Type_ID,Statement_Method_ID,Setup_Date                    ,Envelope_No,Cancel_Envelopes,Notes,First_Contact_Made,Domain_ID,__ExternalPersonID,_First_Donation_Date,_Last_Donation_Date,Processor_ID        ) VALUES
(@contactID,1                     ,1                ,2                  ,{ts '2015-08-05 12:26:57.503'},null       ,0               ,null ,null              ,1        ,null              ,null                ,null               ,null                );

DECLARE @donor_id as int
set @donor_id = (Select donor_ID from donors where contact_id = @contactID);

--Wife Theil-Test Updates
update [dbo].Contacts set Donor_Record = @donor_id where contact_id = @contactID;
GO

--Contact for rtheil+kid14@crossroads.net
DECLARE @contactID as int
set @contactID = (select contact_id from contacts where Email_Address = 'rtheil+kid14@crossroads.net' and Last_Name = 'Theil-Test');

--Household for Kid14 Theil-Test
DECLARE @houseHoldID as int
set @houseHoldID = (select HouseHold_ID from Contacts where email_address = 'rtheil+testing@crossroads.net');

--Kid14 Theil-Test Contact Updates
UPDATE[dbo].Contacts 
SET Date_of_Birth = {d '2001-01-01'}, Gender_ID = 2, Marital_Status_ID = 1, Household_Position_ID = 2, HouseHold_ID = @houseHoldID, Mobile_Phone = '321-548-6154'
WHERE Contact_ID = @contactID;
GO

--Kid17 Theil-Test Contact Record
DECLARE @contactID as int
set @contactID = (select contact_id from contacts where Email_Address = 'rtheil+kid17@crossroads.net' and Last_Name = 'Theil-Test');

--Household for Kid17 Theil-Test
DECLARE @houseHoldID as int
set @houseHoldID = (select HouseHold_ID from Contacts where email_address = 'rtheil+testing@crossroads.net');

--Kid17 Theil-Test Contact Updates
UPDATE [dbo].Contacts 
SET Middle_Name = 'D', Date_of_Birth = {d '1998-01-01'}, Marital_Status_ID = 1, HouseHold_ID = @houseHoldID, Household_Position_ID = 4, Mobile_Phone = '654-818-1425'
WHERE contact_id = @contactID;
GO

--Rich Theil-Test's Request to Join Kids' Club Response Record
SET IDENTITY_INSERT [dbo].[Responses] ON;

DECLARE @respID as int
set @respID = IDENT_CURRENT('Responses')+1;

DECLARE @partID as int
set @partID = (select Participant_Record from Contacts where display_name = 'Theil-Test, Rich');

INSERT INTO [dbo].Responses 
(Response_ID,Response_Date             ,Opportunity_ID,Participant_ID,Comments                        ,Website_Submission,First_Name,Last_Name,Email,Phone,Follow_up_Information,Response_Result_ID,Closed,Domain_ID,Event_ID,__ExternalCESTPID) VALUES
(@respID    ,{ts '2015-07-02 08:17:17'},115           ,@partID       ,'Request on 7/2/2015 8:17:17 AM',null              ,null      ,null     ,null ,null ,null                 ,null              ,0     ,1        ,null    ,null             );

SET IDENTITY_INSERT [dbo].[Responses] OFF;
GO

--Kid14 Theil-Test's Request to Join Kids' Club Response Record
SET IDENTITY_INSERT [dbo].[Responses] ON;

DECLARE @respID as int
set @respID = IDENT_CURRENT('Responses')+1;

DECLARE @partID as int
set @partID = (select Participant_Record from Contacts where display_name = 'Theil-Test, Kid14');

INSERT INTO [dbo].Responses 
(Response_ID,Response_Date             ,Opportunity_ID,Participant_ID,Comments                        ,Website_Submission,First_Name,Last_Name,Email,Phone,Follow_up_Information,Response_Result_ID,Closed,Domain_ID,Event_ID,__ExternalCESTPID) VALUES
(@respID    ,{ts '2015-07-02 08:20:21'},115           ,@partID       ,'Request on 7/2/2015 8:20:21 AM',null              ,null      ,null     ,null ,null ,null                 ,null              ,0 ,1        ,null    ,null             )

SET IDENTITY_INSERT [dbo].[Responses] OFF;
GO

--Family Relationships
DECLARE @dadContact as int
set @dadContact = (select Contact_ID from Contacts where display_name = 'Theil-Test, Rich');

DECLARE @momContact as int
set @momContact = (select Contact_ID from Contacts where display_name = 'Theil-Test, Wife');

DECLARE @kid14Contact as int
set @kid14Contact = (select Contact_ID from Contacts where display_name = 'Theil-Test, Kid14');

DECLARE @kid17Contact as int
set @kid17Contact = (select Contact_ID from Contacts where display_name = 'Theil-Test, Kid17');

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
GO

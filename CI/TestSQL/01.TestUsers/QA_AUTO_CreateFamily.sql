USE [MinistryPlatform]
GO

--
--mpcrds+auto+husband@gmail.com
--
--Retrieve Contact ID for Anakin Skywalker
DECLARE @contactID as int
set @contactID = (select contact_id from contacts where Email_Address = 'mpcrds+auto+husband@gmail.com' and Last_Name = 'Skywalker');

--Update Anakin's Contact record
UPDATE [dbo].Contacts
SET Prefix_ID = 1, Middle_Name = 'Vader', Date_of_Birth = {d '1975-01-01'}, Gender_ID = 1, Marital_Status_ID = 2, Mobile_Phone = '513-654-8745',Company_Phone = '555-365-4125'
WHERE contact_id = @contactID;

--Create an Address record for Anakin
SET IDENTITY_INSERT [dbo].[Addresses] ON;

DECLARE @addressId as int
set @addressId = IDENT_CURRENT('Addresses')+1;

INSERT INTO [dbo].Addresses 
(Address_ID,Address_Line_1          ,Address_Line_2,City  ,[State/Region],Postal_Code,Foreign_Country,Country_Code,Domain_ID,Carrier_Route,Lot_Number,Delivery_Point_Code,Delivery_Point_Check_Digit,Latitude,Longitude,Altitude,Time_Zone,Bar_Code,Area_Code,Last_Validation_Attempt,County    ,Validated,Do_Not_Validate,Last_GeoCode_Attempt,__ExternalAddressID) VALUES
(@addressId,'1234 Tatooine Lane',null          ,'CITY','OH'          ,'45209'    ,'United States','USA'       ,1        ,null         ,null      ,null               ,null                      ,null    ,null     ,null    ,null     ,null    ,null     ,null                   ,'County!' ,null     ,null           ,null                ,null               );

SET IDENTITY_INSERT [dbo].[Addresses] OFF;

--Retrieve Household ID for Anakin
DECLARE @houseHoldID as int
set @houseHoldID = (select Household_ID from contacts where contact_id = @contactID);

--Update Household with Anakin's Address
UPDATE [dbo].HouseHolds
SET Address_ID = @addressId, Home_Phone = '123-867-5309', Congregation_ID = 6
WHERE Household_ID = @houseHoldID;

--Create Donor record for Anakin
INSERT INTO [dbo].donors 
(Contact_ID,Statement_Frequency_ID,Statement_Type_ID,Statement_Method_ID,Setup_Date                    ,Envelope_No,Cancel_Envelopes,Notes,First_Contact_Made,Domain_ID,__ExternalPersonID,_First_Donation_Date,_Last_Donation_Date,Processor_ID        ) VALUES
(@contactID,1                     ,2                ,2                  ,{ts '2017-01-03 12:26:57.503'},null       ,0               ,'Scripted Donor' ,null              ,1        ,null              ,null                ,null               ,'cus_9s5SXiOeM71zQH');

--Retrieve Donor ID for Anakin
DECLARE @donor_id as int
set @donor_id = (Select donor_ID from donors where contact_id = @contactID);

--Update Anakin's Contact record with the Donor ID
UPDATE [dbo].Contacts SET Donor_Record = @donor_id WHERE contact_id = @contactID;

GO


--
--mpcrds+auto+wife@gmail.com
--
--Retrieve Contact ID for Padme Amidala
DECLARE @contactID as int
set @contactID = (select contact_id from contacts where Email_Address = 'mpcrds+auto+wife@gmail.com' and Last_Name = 'Amidala');

--Retrieve Anakin Skywalker's Household ID
DECLARE @houseHoldID as int
set @houseHoldID = (select HouseHold_ID from Contacts where email_address = 'mpcrds+auto+husband@gmail.com');

--Update Padme's Contact record
UPDATE [dbo].Contacts 
SET Prefix_ID = 2, Middle_Name = 'Queen', Gender_ID = 2, Marital_Status_ID = 2, Household_ID = @houseHoldID, Date_of_Birth = {d '1975-01-01'}, Mobile_Phone = '321-654-8184'
WHERE Contact_ID = @contactID;

--Create Padme's Donor record
INSERT INTO [dbo].donors 
(Contact_ID,Statement_Frequency_ID,Statement_Type_ID,Statement_Method_ID,Setup_Date                    ,Envelope_No,Cancel_Envelopes,Notes,First_Contact_Made,Domain_ID,__ExternalPersonID,_First_Donation_Date,_Last_Donation_Date,Processor_ID        ) VALUES
(@contactID,1                     ,2                ,2                  ,{ts '2017-01-03 12:26:57.503'},null       ,0               ,null ,null              ,1        ,null              ,null                ,null               ,null                );

--Retrieve Donor ID for Padme
DECLARE @donor_id as int
set @donor_id = (Select donor_ID from donors where contact_id = @contactID);

--Update Padme's Contact record with the Donor ID
update [dbo].Contacts set Donor_Record = @donor_id where contact_id = @contactID;

GO


--
--mpcrds+auto+child1@gmail.com
--
DECLARE @contactID as int
set @contactID = (select contact_id from contacts where Email_Address = 'mpcrds+auto+child1@gmail.com' and Last_Name = 'Skywalker');

DECLARE @houseHoldID as int
set @houseHoldID = (select HouseHold_ID from Contacts where email_address = 'mpcrds+auto+husband@gmail.com');

UPDATE[dbo].Contacts 
SET Date_of_Birth = {d '2001-01-01'}, Gender_ID = 2, Marital_Status_ID = 1, Household_Position_ID = 2, HouseHold_ID = @houseHoldID, Mobile_Phone = '321-548-6154'
WHERE Contact_ID = @contactID;
GO


--
--mpcrds+auto+child2@gmail.com
--
DECLARE @contactID as int
set @contactID = (select contact_id from contacts where Email_Address = 'mpcrds+auto+child2@gmail.com' and Last_Name = 'Organa');

DECLARE @houseHoldID as int
set @houseHoldID = (select HouseHold_ID from Contacts where email_address = 'mpcrds+auto+husband@gmail.com');

UPDATE [dbo].Contacts 
SET Middle_Name = 'D', Date_of_Birth = {d '1998-01-01'}, Marital_Status_ID = 1, HouseHold_ID = @houseHoldID, Household_Position_ID = 4, Mobile_Phone = '654-818-1425'
WHERE contact_id = @contactID;
GO


--
--Additional family setup
--

--Response record for Anakin Skywalker's request to join Kids' Club
SET IDENTITY_INSERT [dbo].[Responses] ON;

DECLARE @respID as int
set @respID = IDENT_CURRENT('Responses')+1;

DECLARE @partID as int
set @partID = (select Participant_Record from Contacts where display_name = 'Skywalker, Anakin');

INSERT INTO [dbo].Responses 
(Response_ID,Response_Date             ,Opportunity_ID,Participant_ID,Comments                        ,Website_Submission,First_Name,Last_Name,Email,Phone,Follow_up_Information,Response_Result_ID,Closed,Domain_ID,Event_ID,__ExternalCESTPID) VALUES
(@respID    ,{ts '2015-07-02 08:17:17'},115           ,@partID       ,'Request on 7/2/2015 8:17:17 AM',null              ,null      ,null     ,null ,null ,null                 ,null              ,0     ,1        ,null    ,null             );

SET IDENTITY_INSERT [dbo].[Responses] OFF;
GO


--Response record for Luke Skywalker's request to join Kids' Club
SET IDENTITY_INSERT [dbo].[Responses] ON;

DECLARE @respID as int
set @respID = IDENT_CURRENT('Responses')+1;

DECLARE @partID as int
set @partID = (select Participant_Record from Contacts where display_name = 'Skywalker, Luke');

INSERT INTO [dbo].Responses 
(Response_ID,Response_Date             ,Opportunity_ID,Participant_ID,Comments                        ,Website_Submission,First_Name,Last_Name,Email,Phone,Follow_up_Information,Response_Result_ID,Closed,Domain_ID,Event_ID,__ExternalCESTPID) VALUES
(@respID    ,{ts '2015-07-02 08:20:21'},115           ,@partID       ,'Request on 7/2/2015 8:20:21 AM',null              ,null      ,null     ,null ,null ,null                 ,null              ,0 ,1        ,null    ,null             )

SET IDENTITY_INSERT [dbo].[Responses] OFF;
GO


--Family Relationships
DECLARE @dadContact as int
set @dadContact = (select Contact_ID from Contacts where display_name = 'Skywalker, Anakin');

DECLARE @momContact as int
set @momContact = (select Contact_ID from Contacts where display_name = 'Amidala, Padme');

DECLARE @kid1Contact as int
set @kid1Contact = (select Contact_ID from Contacts where display_name = 'Skywalker, Luke');

DECLARE @kid2Contact as int
set @kid2Contact = (select Contact_ID from Contacts where display_name = 'Organa, Leia');

--Dad Married to Mom
INSERT INTO [dbo].Contact_Relationships 
(Contact_ID ,Relationship_ID,Related_Contact_ID,Start_Date                ,End_Date,Domain_ID,Notes                       ,_Triggered_By) VALUES
(@dadContact,1              ,@momContact       ,{ts '2015-07-02 08:15:06'},null    ,1        ,'Created by Add Family Tool',null      );

--Dad Parent of Kids
INSERT INTO [dbo].Contact_Relationships 
(Contact_ID,Relationship_ID,Related_Contact_ID,Start_Date,End_Date,Domain_ID,Notes,_Triggered_By) VALUES
(@dadContact   ,6              ,@kid1Contact ,null      ,null    ,1        ,null ,null      );

INSERT INTO [dbo].Contact_Relationships 
(Contact_ID ,Relationship_ID,Related_Contact_ID,Start_Date,End_Date,Domain_ID,Notes,_Triggered_By) VALUES
(@dadContact,6              ,@kid2Contact     ,null      ,null    ,1        ,null ,null      );

--Mom Parent Of kids
INSERT INTO [dbo].Contact_Relationships 
(Contact_ID ,Relationship_ID,Related_Contact_ID,Start_Date,End_Date,Domain_ID,Notes,_Triggered_By) VALUES
(@momContact,6              ,@kid1Contact     ,null      ,null    ,1        ,null ,null      );

INSERT INTO [dbo].Contact_Relationships 
(Contact_ID ,Relationship_ID,Related_Contact_ID,Start_Date,End_Date,Domain_ID,Notes,_Triggered_By) VALUES
(@momContact,6              ,@kid2Contact     ,null      ,null    ,1        ,null ,null      );

--Kids Siblings
INSERT INTO [dbo].Contact_Relationships 
(Contact_ID   ,Relationship_ID,Related_Contact_ID,Start_Date,End_Date,Domain_ID,Notes,_Triggered_By) VALUES
(@kid1Contact,2              ,@kid2Contact     ,null      ,null    ,1        ,null ,null      );
GO

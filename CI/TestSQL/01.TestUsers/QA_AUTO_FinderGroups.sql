--Registered Account - Connect Users

USE [MinistryPlatform]
GO

DECLARE @contactID     AS INT
SET @contactID =      (SELECT Contact_ID 
				       FROM Contacts 
				       WHERE Email_Address = 'mpcrds+auto+matcauthon@gmail.com' and Last_Name = 'Cauthon');

DECLARE @houseHoldID   AS INT
SET @houseHoldID =    (SELECT Household_ID 
					   FROM   Contacts 
					   WHERE  Contact_ID = @contactID);
				  
 
 -- Update partcipant record.
-- NOTE..For a test user that you only want to be in a group and not a connect user, change Host_Status_ID = 0
-- Group_Leader_Status_ID 4 = approved, 1 = not applied

DECLARE @participantID AS INT
SET @participantID =  (SELECT Participant_ID 
					   FROM   Participants 
					   WHERE  Contact_ID = @contactID);

UPDATE [dbo].Participants
SET   Participant_Type_ID = 1, Domain_ID = 1, Show_On_Map = 1, Host_Status_ID = 3, Group_Leader_Status_ID = 4 
WHERE Participant_ID = @participantID 

SET IDENTITY_INSERT [dbo].[Addresses] ON;
DECLARE @addressID AS INT
SET @addressId = IDENT_CURRENT('Addresses')+1
INSERT INTO [dbo].Addresses 
(Address_ID, Address_Line_1    , City    ,[State/Region],Postal_Code,Foreign_Country,Country_Code,Domain_ID,Latitude   ,Longitude ) 
VALUES
(@addressID, '1881 Mulberry St', 'Goshen','OH'          ,'45122'    ,'United States','USA'       ,1        ,'39.233869','-84.159939' );
 
 SET IDENTITY_INSERT [dbo].[Addresses] OFF;
 
 UPDATE  [dbo].Households
 SET Address_ID = @addressID
 WHERE Household_ID = @houseHoldID

 -- Create Group
-- For new group, Change Group_name
-- Group_type_ID 1 = small group
-- Ministry_ID 8 = spiritual growth. Run this query for all minitstries: select * from dbo.Ministries

DECLARE @groupIdSG   AS INT
SET IDENTITY_INSERT [dbo].[Groups] ON;
SET @groupIdSG = (SELECT IDENT_CURRENT('Groups')) + 1 ;

-- Set up Mat as leader of the group
-- change group name
INSERT INTO Groups
( Group_ID  , Group_Name                       , Group_Type_ID, Ministry_ID, Congregation_ID, Primary_Contact, Description                   , Start_Date      , Offsite_Meeting_Address, Group_Is_Full, Available_Online, Domain_ID, Deadline_Passed_Message_ID , Send_Attendance_Notification , Send_Service_Notification , Child_Care_Available) 
VALUES
( @groupIdSG, '(t+auto) Band of the Red Hand'  , 1            , 8          ,  1             ,  @contactID    , 'Finder group for automation' , {d '2015-11-01'},  @addressID            ,0             , 1                , 1        , 58                         ,  0                           , 0                         , 0                   ) ;

SET IDENTITY_INSERT [dbo].[Groups] OFF;

INSERT INTO dbo.Group_Participants
( Group_ID  , Participant_ID, Group_Role_ID, Domain_ID, Start_Date      , Employee_Role, Auto_Promote ) 
VALUES
( @groupIdSG, @participantID, 22           , 1        , {d '2015-11-01'}, 0            , 1            );

GO

-----------------------------------------------------------------------------------------------------
--Add Talmanes Delovinde to Mat's group

USE [MinistryPlatform]
GO

DECLARE @contactID     AS INT
SET @contactID =      (SELECT Contact_ID 
				       FROM Contacts 
				       WHERE Email_Address = 'mpcrds+auto+talmanesdelovinde@gmail.com' and Last_Name = 'Delovinde');

DECLARE @houseHoldID   AS INT
SET @houseHoldID =    (SELECT Household_ID 
					   FROM   Contacts 
					   WHERE  Contact_ID = @contactID);

 
 -- Update partcipant record.
-- NOTE..For a test user that you only want to be in a group and not a connect user, change Host_Status_ID = 0
-- Group_Leader_Status_ID 4 = approved, 1 = not applied

DECLARE @participantID AS INT
SET @participantID =  (SELECT Participant_ID 
					   FROM   Participants 
					   WHERE  Contact_ID = @contactID);

UPDATE [dbo].Participants
SET   Participant_Type_ID = 1, Domain_ID = 1, Show_On_Map = 1, Host_Status_ID = 3, Group_Leader_Status_ID = 1 
WHERE Participant_ID = @participantID 

SET IDENTITY_INSERT [dbo].[Addresses] ON;
DECLARE @addressID AS INT
SET @addressId = IDENT_CURRENT('Addresses')+1
INSERT INTO [dbo].Addresses 
(Address_ID, Address_Line_1  , City        ,[State/Region],Postal_Code,Foreign_Country,Country_Code,Domain_ID,Latitude   ,Longitude ) 
VALUES
(@addressID, '1881 Main St'  , 'Goshen'    ,'OH'          ,'45122'    ,'United States','USA'       ,1        ,'39.234512','-84.160610');
 
 SET IDENTITY_INSERT [dbo].[Addresses] OFF;
 
 UPDATE  [dbo].Households
 SET Address_ID = @addressID 
 WHERE Household_ID = @houseHoldID

 -- Create Group
-- For new group, Change Group_name
-- Group_type_ID 1 = small group
-- Ministry_ID 8 = spiritual growth. Run this query for all minitstries: select * from dbo.Ministries

DECLARE @groupID       AS INT 
SET @groupID =        (SELECT Group_ID 
				       FROM   Groups 
				       WHERE  Group_Name = '(t+auto) Band of the Red Hand');--Change to group you want the person inserted into

INSERT INTO dbo.Group_Participants
( Group_ID, Participant_ID, Group_Role_ID, Domain_ID, Start_Date      , Employee_Role, Auto_Promote ) 
VALUES
( @groupId, @participantID, 16           , 1        , {d '2015-11-01'}, 0            , 1            );

-----------------------------------------------------------------------------------------------------
--Add Nalesean Aldiaya to Mat's group

USE [MinistryPlatform]
GO

DECLARE @contactID     AS INT
SET @contactID =      (SELECT Contact_ID 
				       FROM Contacts 
				       WHERE Email_Address = 'mpcrds+auto+naleseanaldiaya@gmail.com' and Last_Name = 'Aldiaya');

DECLARE @houseHoldID   AS INT
SET @houseHoldID =    (SELECT Household_ID 
					   FROM   Contacts 
					   WHERE  Contact_ID = @contactID);

 
 -- Update partcipant record.
-- NOTE..For a test user that you only want to be in a group and not a connect user, change Host_Status_ID = 0
-- Group_Leader_Status_ID 4 = approved, 1 = not applied

DECLARE @participantID AS INT
SET @participantID =  (SELECT Participant_ID 
					   FROM   Participants 
					   WHERE  Contact_ID = @contactID);

UPDATE [dbo].Participants
SET   Participant_Type_ID = 1, Domain_ID = 1, Show_On_Map = 1, Host_Status_ID = 3, Group_Leader_Status_ID = 1 
WHERE Participant_ID = @participantID 

SET IDENTITY_INSERT [dbo].[Addresses] ON;
DECLARE @addressID AS INT
SET @addressId = IDENT_CURRENT('Addresses')+1
INSERT INTO [dbo].Addresses 
(Address_ID, Address_Line_1  , City        ,[State/Region],Postal_Code,Foreign_Country,Country_Code,Domain_ID,Latitude,Longitude ) 
VALUES
(@addressID, '1821 Walnut St'  , 'Goshen'    ,'OH'          ,'45122'    ,'United States','USA'       ,1      ,'39.234363','-84.161875'   );
 
 SET IDENTITY_INSERT [dbo].[Addresses] OFF;
 
 UPDATE  [dbo].Households
 SET Address_ID = @addressID 
 WHERE Household_ID = @houseHoldID

 -- Create Group
-- For new group, Change Group_name
-- Group_type_ID 1 = small group
-- Ministry_ID 8 = spiritual growth. Run this query for all minitstries: select * from dbo.Ministries

DECLARE @groupID       AS INT 
SET @groupID =        (SELECT Group_ID 
				       FROM   Groups 
				       WHERE  Group_Name = '(t+auto) Band of the Red Hand');--Change to group you want the person inserted into

INSERT INTO dbo.Group_Participants
( Group_ID, Participant_ID, Group_Role_ID, Domain_ID, Start_Date      , Employee_Role, Auto_Promote ) 
VALUES
( @groupId, @participantID, 16           , 1        , {d '2015-11-01'}, 0            , 1            );
--Registered Account - Connect Users 

USE [MinistryPlatform]
GO

DECLARE @contactID     AS INT
SET @contactID =      (SELECT Contact_ID 
				       FROM Contacts 
				       WHERE Email_Address = 'mpcrds+picard@gmail.com' and Last_Name = 'Picard');

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
(Address_ID, Address_Line_1  , City        ,[State/Region],Postal_Code,Foreign_Country,Country_Code,Domain_ID,Latitude    ,Longitude  ) 
VALUES
(@addressID, '5144 Rybolt Rd', 'Cincinnati','OH'          ,'45248'    ,'United States','USA'       ,1        ,'39.185298' ,'-84.665607' );
 
 SET IDENTITY_INSERT [dbo].[Addresses] OFF;
 
 UPDATE  [dbo].Households
 SET Address_ID = @addressID
 WHERE Household_ID = @houseHoldID

 -- Create Group
-- For new group, Change Group_name
-- Group_type_ID 1 = small group
-- Ministry_ID 8 = spiritual growth. Run this query for all minitstries: SELECT * FROM dbo.Ministries

DECLARE @groupIdSG   AS INT
SET IDENTITY_INSERT [dbo].[Groups] ON;
SET @groupIdSG = (SELECT IDENT_CURRENT('Groups')) + 1 ;

-- Set up Jean-Luc as leader of the group
-- change group name
INSERT INTO Groups
( Group_ID  , Group_Name                       , Group_Type_ID, Ministry_ID, Congregation_ID, Primary_Contact, Description                   , Start_Date      , Offsite_Meeting_Address, Group_Is_Full, Available_Online, Meeting_Time, Meeting_Day_ID, Domain_ID, Deadline_Passed_Message_ID , Send_Attendance_Notification , Send_Service_Notification , Child_Care_Available) 
VALUES
( @groupIdSG, '(t+auto) The Next Generation'   , 1            , 8          ,  1             ,  @contactID    , 'Finder group for automation' , {d '2015-11-01'},  @addressID            ,0             , 1               , '17:00:00'  , 1             , 1        , 58                         ,  0                           , 0                         , 0                   ) ;
SET IDENTITY_INSERT [dbo].[Groups] OFF;

-- Add Interest to group
DECLARE @AttributeID   AS INT
SET IDENTITY_INSERT [dbo].Attributes ON;
SET @AttributeID = (SELECT IDENT_CURRENT('Attributes')) + 1 ;
INSERT INTO Attributes
(  [Attribute_ID], [Attribute_Name]     , [Attribute_Type_ID], [Attribute_Category_ID], [Domain_ID], [Sort_Order] )
VALUES
(  @AttributeID  , 'Automation testing' , 90                 , 20                     , 1          , 0            )

-- Add existing attributes to group
INSERT INTO [dbo].[Group_Attributes]
(  [Attribute_ID]                                                                  , [Group_ID] , [Domain_ID], [Start_Date] )
VALUES
(  (SELECT attribute_id FROM Attributes WHERE Attribute_Name='Automation testing') , @groupIdSG , 1          , {d '2015-11-01'} )


INSERT INTO [dbo].[Group_Attributes]
(  [Attribute_ID]                                                                       , [Group_ID]      , [Domain_ID], [Start_Date]     )
VALUES
(  (SELECT attribute_id FROM Attributes WHERE [Description]='(men and women together)') , @groupIdSG      , 1          , {d '2015-11-01'} )

INSERT INTO [dbo].[Group_Attributes]
(  [Attribute_ID]                                                                                           , [Group_ID]    , [Domain_ID], [Start_Date]     )
VALUES
(  (SELECT attribute_id FROM Attributes WHERE Attribute_Name='College Students' AND Attribute_Type_ID = 91) ,  @groupIdSG   , 1          , {d '2015-11-01'} )


INSERT INTO dbo.Group_Participants
(  Group_ID  , Participant_ID, Group_Role_ID, Domain_ID, Start_Date      , Employee_Role, Auto_Promote ) 
VALUES
(  @groupIdSG, @participantID, 22           , 1        , {d '2015-11-01'}, 0            , 1            );


GO

-----------------------------------------------------------------------------------------------------
--Add 3 more group leaders Lt. Data

USE [MinistryPlatform]
GO

DECLARE @contactID     AS INT
SET @contactID =      (SELECT Contact_ID 
				       FROM Contacts 
				       WHERE Email_Address = 'mpcrds+data@gmail.com' and Last_Name = 'Data');

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
(Address_ID , Address_Line_1  , City        ,[State/Region],Postal_Code,Foreign_Country,Country_Code,Domain_ID,Latitude    ,Longitude  ) 
VALUES
(@addressID, '5117 Rybolt Rd', 'Cincinnati','OH'          ,'45248'    ,'United States','USA'       ,1        ,'39.184111' ,'-84.666405' );

SET IDENTITY_INSERT [dbo].[Addresses] OFF;
 
UPDATE  [dbo].Households
SET Address_ID = @addressID 
WHERE Household_ID = @houseHoldID

 -- Create Group
-- For new group, Change Group_name
-- Group_type_ID 1 = small group
-- Ministry_ID 8 = spiritual growth. Run this query for all minitstries: SELECT * FROM dbo.Ministries

DECLARE @groupID       AS INT 
SET @groupID =        (SELECT Group_ID 
				       FROM   Groups 
				       WHERE  Group_Name = '(t+auto) The Next Generation');--Change to group you want the person inserted into

INSERT INTO dbo.Group_Participants
( Group_ID, Participant_ID, Group_Role_ID, Domain_ID, Start_Date      , Employee_Role, Auto_Promote ) 
VALUES
( @groupID, @participantID, 22           , 1        , {d '2015-11-01'}, 0            , 1            );

-----------------------------------------------------------------------
---------------2nd leader Lt. Worf
USE [MinistryPlatform]
GO
DECLARE @contactID     AS INT
SET @contactID =      (SELECT Contact_ID 
				       FROM Contacts 
				       WHERE Email_Address = 'mpcrds+worf@gmail.com' and Last_Name = 'Worf');

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
(Address_ID, Address_Line_1  , City        ,[State/Region],Postal_Code,Foreign_Country,Country_Code,Domain_ID,Latitude   ,Longitude ) 
VALUES
(@addressID, '5121 Rybolt'  , 'Cincinnati'  ,'OH'          ,'45248'    ,'United States','USA'       ,1        ,'39.184353','-84.666394');
 
 SET IDENTITY_INSERT [dbo].[Addresses] OFF;
 
 UPDATE  [dbo].Households
 SET Address_ID = @addressID 
 WHERE Household_ID = @houseHoldID

 -- Create Group
-- For new group, Change Group_name
-- Group_type_ID 1 = small group
-- Ministry_ID 8 = spiritual growth. Run this query for all minitstries: SELECT * FROM dbo.Ministries

DECLARE @groupID       AS INT 
SET @groupID =        (SELECT Group_ID 
				       FROM   Groups 
				       WHERE  Group_Name = '(t+auto) The Next Generation');--Change to group you want the person inserted into

INSERT INTO dbo.Group_Participants
( Group_ID, Participant_ID, Group_Role_ID, Domain_ID, Start_Date      , Employee_Role, Auto_Promote ) 
VALUES
( @groupID, @participantID, 22           , 1        , {d '2015-11-01'}, 0            , 1            );

-----------------------------------------------------------------------
---------------3nd leader Commander Riker
USE [MinistryPlatform]
GO
DECLARE @contactID     AS INT
SET @contactID =      (SELECT Contact_ID 
				       FROM Contacts 
				       WHERE Email_Address = 'mpcrds+riker@gmail.com' and Last_Name = 'Riker');

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
(Address_ID, Address_Line_1  , City        ,[State/Region],Postal_Code,Foreign_Country,Country_Code,Domain_ID,Latitude   ,Longitude ) 
VALUES
(@addressID, '5170 Rybolt'  , 'Cincinnati' ,'OH'          ,'45248'    ,'United States','USA'       ,1        ,'39.185774','-84.665124');
 
 SET IDENTITY_INSERT [dbo].[Addresses] OFF;
 
 UPDATE  [dbo].Households
 SET Address_ID = @addressID 
 WHERE Household_ID = @houseHoldID

 -- Create Group
-- For new group, Change Group_name
-- Group_type_ID 1 = small group
-- Ministry_ID 8 = spiritual growth. Run this query for all minitstries: SELECT * FROM dbo.Ministries

DECLARE @groupID       AS INT 
SET @groupID =        (SELECT Group_ID 
				       FROM   Groups 
				       WHERE  Group_Name = '(t+auto) The Next Generation');--Change to group you want the person inserted into

INSERT INTO dbo.Group_Participants
( Group_ID, Participant_ID, Group_Role_ID, Domain_ID, Start_Date      , Employee_Role, Auto_Promote ) 
VALUES
( @groupID, @participantID, 22           , 1        , {d '2015-11-01'}, 0            , 1            );

-----------------------------------------------------------------------------------------------------
--Add approved leader status but only apprentice Deanna Troy to Picard's group

USE [MinistryPlatform]
GO

DECLARE @contactID     AS INT
SET @contactID =      (SELECT Contact_ID 
				       FROM Contacts 
				       WHERE Email_Address = 'mpcrds+troi@gmail.com' and Last_Name = 'Troi');

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
-- Ministry_ID 8 = spiritual growth. Run this query for all minitstries: SELECT * FROM dbo.Ministries

DECLARE @groupID       AS INT 
SET @groupID =        (SELECT Group_ID 
				       FROM   Groups 
				       WHERE  Group_Name = '(t+auto) The Next Generation');--Change to group you want the person inserted into

INSERT INTO dbo.Group_Participants
( Group_ID, Participant_ID, Group_Role_ID, Domain_ID, Start_Date      , Employee_Role, Auto_Promote ) 
VALUES
( @groupID, @participantID, 66           , 1        , {d '2015-11-01'}, 0            , 1            );


-----------------------------------------------------------------------------------------------------
--Add not applied leader status but apprentice Geordi La Forge Picard's group

USE [MinistryPlatform]
GO

DECLARE @contactID     AS INT
SET @contactID =      (SELECT Contact_ID 
				       FROM Contacts 
				       WHERE Email_Address = 'mpcrds+laforge@gmail.com' and Last_Name = 'La Forge');

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
(@addressID, '5226 Rybolt'  , 'Cincinnati' ,'OH'          ,'45248'    ,'United States','USA'       ,1        ,'39.187375','-84.665444');
 
 SET IDENTITY_INSERT [dbo].[Addresses] OFF;
 
 UPDATE  [dbo].Households
 SET Address_ID = @addressID 
 WHERE Household_ID = @houseHoldID

 -- Create Group
-- For new group, Change Group_name
-- Group_type_ID 1 = small group
-- Ministry_ID 8 = spiritual growth. Run this query for all minitstries: SELECT * FROM dbo.Ministries

DECLARE @groupID       AS INT 
SET @groupID =        (SELECT Group_ID 
				       FROM   Groups 
				       WHERE  Group_Name = '(t+auto) The Next Generation');--Change to group you want the person inserted into

INSERT INTO dbo.Group_Participants
( Group_ID, Participant_ID, Group_Role_ID, Domain_ID, Start_Date      , Employee_Role, Auto_Promote ) 
VALUES
( @groupID, @participantID, 66           , 1        , {d '2015-11-01'}, 0            , 1            );

-----------------------------------------------------------------------------------------------------
--Add  members whoopi and O'brian with and approved status Picard's group

USE [MinistryPlatform]
GO

DECLARE @contactID     AS INT
SET @contactID =      (SELECT Contact_ID 
				       FROM Contacts 
				       WHERE Email_Address = 'mpcrds+guinan@gmail.com' and Last_Name = 'Guinan');

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
(Address_ID, Address_Line_1  , City        ,[State/Region],Postal_Code,Foreign_Country,Country_Code,Domain_ID,Latitude   ,Longitude ) 
VALUES
(@addressID, '6735 Hearne '  , 'Cincinnati' ,'OH'          ,'45248'    ,'United States','USA'       ,1        ,'39.187864','-84.666908');
 
 SET IDENTITY_INSERT [dbo].[Addresses] OFF;
 
 UPDATE  [dbo].Households
 SET Address_ID = @addressID 
 WHERE Household_ID = @houseHoldID

 -- Create Group
-- For new group, Change Group_name
-- Group_type_ID 1 = small group
-- Ministry_ID 8 = spiritual growth. Run this query for all minitstries: SELECT * FROM dbo.Ministries

DECLARE @groupID       AS INT 
SET @groupID =        (SELECT Group_ID 
				       FROM   Groups 
				       WHERE  Group_Name = '(t+auto) The Next Generation');--Change to group you want the person inserted into

INSERT INTO dbo.Group_Participants
( Group_ID, Participant_ID, Group_Role_ID, Domain_ID, Start_Date      , Employee_Role, Auto_Promote ) 
VALUES
( @groupID, @participantID, 16           , 1        , {d '2015-11-01'}, 0            , 1            );

USE [MinistryPlatform]
GO

DECLARE @contactID     AS INT
SET @contactID =      (SELECT Contact_ID 
				       FROM Contacts 
				       WHERE Email_Address = 'mpcrds+obrien@gmail.com' and Last_Name = 'OBrien');

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
(Address_ID, Address_Line_1  , City        ,[State/Region],Postal_Code,Foreign_Country,Country_Code,Domain_ID,Latitude   ,Longitude ) 
VALUES
(@addressID, '6724 Hearne'  , 'Cincinnati' ,'OH'          ,'45248'    ,'United States','USA'       ,1        ,'39.188365','-84.666525');
 
 SET IDENTITY_INSERT [dbo].[Addresses] OFF;
 
 UPDATE  [dbo].Households
 SET Address_ID = @addressID 
 WHERE Household_ID = @houseHoldID

 -- Create Group
-- For new group, Change Group_name

-- Group_type_ID 1 = small group
-- Ministry_ID 8 = spiritual growth. Run this query for all minitstries: SELECT * FROM dbo.Ministries

DECLARE @groupID       AS INT 
SET @groupID =        (SELECT Group_ID 
				       FROM   Groups 
				       WHERE  Group_Name = '(t+auto) The Next Generation');--Change to group you want the person inserted into

INSERT INTO dbo.Group_Participants
( Group_ID, Participant_ID, Group_Role_ID, Domain_ID, Start_Date      , Employee_Role, Auto_Promote ) 
VALUES
( @groupID, @participantID, 16           , 1        , {d '2015-11-01'}, 0            , 1            );

-----------------------------------------------------------------------------------------------------
--Add  member crusher not applied to be a leader
USE [MinistryPlatform]
GO

DECLARE @contactID     AS INT
SET @contactID =      (SELECT Contact_ID 
				       FROM Contacts 
				       WHERE Email_Address = 'mpcrds+crusher@gmail.com' and Last_Name = 'Crusher');

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
(@addressID, '6768 Hearne'  , 'Cincinnati' ,'OH'          ,'45248'    ,'United States','USA'       ,1        ,'39.188451','-84.667836');
 
 SET IDENTITY_INSERT [dbo].[Addresses] OFF;
 
 UPDATE  [dbo].Households
 SET Address_ID = @addressID 
 WHERE Household_ID = @houseHoldID

 -- Create Group
-- For new group, Change Group_name

-- Group_type_ID 1 = small group
-- Ministry_ID 8 = spiritual growth. Run this query for all minitstries: SELECT * FROM dbo.Ministries

DECLARE @groupID       AS INT 
SET @groupID =        (SELECT Group_ID 
				       FROM   Groups 
				       WHERE  Group_Name = '(t+auto) The Next Generation');--Change to group you want the person inserted into

INSERT INTO dbo.Group_Participants
( Group_ID, Participant_ID, Group_Role_ID, Domain_ID, Start_Date      , Employee_Role, Auto_Promote ) 
VALUES
( @groupID, @participantID, 16           , 1        , {d '2015-11-01'}, 0            , 1            );
GO

---------Add connect Albert Einstein as connect leader for automation

USE [MinistryPlatform]
GO

DECLARE @contactID     AS INT
SET @contactID =      (SELECT Contact_ID 
				       FROM Contacts 
				       WHERE Email_Address = 'mpcrds+ae@gmail.com' and Last_Name = 'Einstein');

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
(Address_ID, Address_Line_1  , City        ,[State/Region],Postal_Code,Foreign_Country,Country_Code,Domain_ID,Latitude    ,Longitude  ) 
VALUES
(@addressID, '5144 Rybolt Rd', 'Cincinnati','OH'          ,'45248'    ,'United States','USA'       ,1        ,'39.185298' ,'-84.665607' );
 
 SET IDENTITY_INSERT [dbo].[Addresses] OFF;
 
 UPDATE  [dbo].Households
 SET Address_ID = @addressID
 WHERE Household_ID = @houseHoldID

 -- Create Group
-- For new group, Change Group_name
-- Group_type_ID 1 = small group
-- Ministry_ID 8 = spiritual growth. Run this query for all minitstries: SELECT * FROM dbo.Ministries

DECLARE @groupIdSG   AS INT
SET IDENTITY_INSERT [dbo].[Groups] ON;
SET @groupIdSG = (SELECT IDENT_CURRENT('Groups')) + 1 ;

-- Set up Albert as leader of the group
-- change group name
INSERT INTO Groups
( Group_ID  , Group_Name    , Group_Type_ID, Ministry_ID, Congregation_ID, Primary_Contact, Description                    , Start_Date      , Offsite_Meeting_Address, Group_Is_Full, Available_Online, Domain_ID, Deadline_Passed_Message_ID , Send_Attendance_Notification , Send_Service_Notification , Child_Care_Available) 
VALUES
( @groupIdSG, 'Albert, E'   , 30           , 8          ,  1             ,  @contactID    , 'connect group for automation' , {d '2015-11-01'},  @addressID            ,0             , 1               , 1        , 58                         ,  0                           , 0                         , 0                   ) ;

SET IDENTITY_INSERT [dbo].[Groups] OFF;

INSERT INTO dbo.Group_Participants
( Group_ID  , Participant_ID, Group_Role_ID, Domain_ID, Start_Date      , Employee_Role, Auto_Promote ) 
VALUES
( @groupIdSG, @participantID, 22           , 1        , {d '2015-11-01'}, 0            , 1            );

GO

---------Add connect Stephen Hawking as connect leader for automation

USE [MinistryPlatform]
GO

DECLARE @contactID     AS INT
SET @contactID =      (SELECT Contact_ID 
				       FROM Contacts 
				       WHERE Email_Address = 'mpcrds+sh@gmail.com' and Last_Name = 'Hawking');

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
(Address_ID, Address_Line_1  , City        ,[State/Region],Postal_Code,Foreign_Country,Country_Code,Domain_ID,Latitude    ,Longitude  ) 
VALUES
(@addressID, '5117 Rybolt Rd', 'Cincinnati','OH'          ,'45248'    ,'United States','USA'       ,1        ,'39.184144' ,'-84.666359' );
 
 SET IDENTITY_INSERT [dbo].[Addresses] OFF;
 
 UPDATE  [dbo].Households
 SET Address_ID = @addressID
 WHERE Household_ID = @houseHoldID

 -- Create Group
-- For new group, Change Group_name
-- Group_type_ID 1 = small group
-- Ministry_ID 8 = spiritual growth. Run this query for all minitstries: SELECT * FROM dbo.Ministries

DECLARE @groupIdSG   AS INT
SET IDENTITY_INSERT [dbo].[Groups] ON;
SET @groupIdSG = (SELECT IDENT_CURRENT('Groups')) + 1 ;

-- Set up Stephen as leader of the group
-- change group name
INSERT INTO Groups
( Group_ID  , Group_Name    , Group_Type_ID, Ministry_ID, Congregation_ID, Primary_Contact, Description                    , Start_Date      , Offsite_Meeting_Address, Group_Is_Full, Available_Online, Domain_ID, Deadline_Passed_Message_ID , Send_Attendance_Notification , Send_Service_Notification , Child_Care_Available) 
VALUES
( @groupIdSG, 'Stephen, H'   , 30           , 8          ,  1             ,  @contactID    , 'connect group for automation' , {d '2015-11-01'},  @addressID            ,0             , 1               , 1        , 58                         ,  0                           , 0                         , 0                   ) ;

SET IDENTITY_INSERT [dbo].[Groups] OFF;

INSERT INTO dbo.Group_Participants
( Group_ID  , Participant_ID, Group_Role_ID, Domain_ID, Start_Date      , Employee_Role, Auto_Promote ) 
VALUES
( @groupIdSG, @participantID, 22           , 1        , {d '2015-11-01'}, 0            , 1            );

GO
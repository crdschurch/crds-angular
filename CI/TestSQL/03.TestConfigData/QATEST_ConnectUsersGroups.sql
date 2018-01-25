--Registered Account - Connect Users 

USE [MinistryPlatform]
GO

DECLARE @contactID     AS INT
SET @contactID =      (SELECT Contact_ID 
				       FROM dp_Users 
				       WHERE User_Name = 'mpcrds+picard@gmail.com');

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

 -- Create Group (if does not already exist)
-- For new group, Change Group_name
-- Group_type_ID 1 = small group
-- Ministry_ID 8 = spiritual growth. Run this query for all minitstries: SELECT * FROM dbo.Ministries
DECLARE @groupIdSG   AS INT;
SET @groupIdSG = (SELECT TOP 1 Group_ID FROM [dbo].Groups WHERE Group_Name = '(t+auto) The Next Generation' ORDER BY Group_ID asc);

-- Add Interest to group
--Create attribute if it doesn't exist
DECLARE @AttributeID   AS INT;
SET @AttributeID = (SELECT Attribute_ID FROM Attributes WHERE Attribute_Name = 'Automation testing');

IF @AttributeID is null
BEGIN	
	SET IDENTITY_INSERT [dbo].Attributes ON;
	SET @AttributeID = (SELECT IDENT_CURRENT('Attributes')) + 1 ;
	INSERT INTO Attributes
	(  [Attribute_ID], [Attribute_Name]     , [Attribute_Type_ID], [Attribute_Category_ID], [Domain_ID], [Sort_Order] )
	VALUES
	(  @AttributeID  , 'Automation testing' , 90                 , 20                     , 1          , 0            )
	SET IDENTITY_INSERT [dbo].Attributes OFF;
END


-- Add existing attributes to group
INSERT INTO [dbo].[Group_Attributes]
(  [Attribute_ID] , [Group_ID] , [Domain_ID], [Start_Date] )
VALUES
(  @AttributeID   , @groupIdSG , 1          , {d '2015-11-01'} )


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
				       FROM dp_Users 
				       WHERE User_Name = 'mpcrds+data@gmail.com');

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

 -- Create Group
-- For new group, Change Group_name
-- Group_type_ID 1 = small group
-- Ministry_ID 8 = spiritual growth. Run this query for all minitstries: SELECT * FROM dbo.Ministries

--Creating this group participant WILL FAIL because the group doesn't exist yet. Will be fixed when Test Config folder is redone.
DECLARE @groupID       AS INT 
SET @groupID =        (SELECT TOP 1 Group_ID 
				       FROM   Groups 
				       WHERE  Group_Name = '(t+auto) The Next Generation'
					   ORDER BY Group_ID asc);--Change to group you want the person inserted into

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
				       FROM dp_Users 
				       WHERE User_Email = 'mpcrds+worf@gmail.com');

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

 -- Create Group
-- For new group, Change Group_name
-- Group_type_ID 1 = small group
-- Ministry_ID 8 = spiritual growth. Run this query for all minitstries: SELECT * FROM dbo.Ministries

DECLARE @groupID       AS INT 
SET @groupID =        (SELECT TOP 1 Group_ID 
				       FROM   Groups 
				       WHERE  Group_Name = '(t+auto) The Next Generation'
					   ORDER BY Group_ID asc);--Change to group you want the person inserted into

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
				       FROM dp_Users 
				       WHERE User_Name = 'mpcrds+riker@gmail.com');

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

 -- Create Group
-- For new group, Change Group_name
-- Group_type_ID 1 = small group
-- Ministry_ID 8 = spiritual growth. Run this query for all minitstries: SELECT * FROM dbo.Ministries

DECLARE @groupID       AS INT 
SET @groupID =        (SELECT TOP 1 Group_ID 
				       FROM   Groups 
				       WHERE  Group_Name = '(t+auto) The Next Generation'
					   ORDER BY Group_ID asc);--Change to group you want the person inserted into

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
				       FROM dp_Users 
				       WHERE User_Name = 'mpcrds+troi@gmail.com');

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

 -- Create Group
-- For new group, Change Group_name
-- Group_type_ID 1 = small group
-- Ministry_ID 8 = spiritual growth. Run this query for all minitstries: SELECT * FROM dbo.Ministries

DECLARE @groupID       AS INT 
SET @groupID =        (SELECT TOP 1 Group_ID 
				       FROM   Groups 
				       WHERE  Group_Name = '(t+auto) The Next Generation'
					   ORDER BY Group_ID asc);--Change to group you want the person inserted into

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
				       FROM dp_Users 
				       WHERE User_Name = 'mpcrds+laforge@gmail.com');

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
 -- Create Group
-- For new group, Change Group_name
-- Group_type_ID 1 = small group
-- Ministry_ID 8 = spiritual growth. Run this query for all minitstries: SELECT * FROM dbo.Ministries

DECLARE @groupID       AS INT 
SET @groupID =        (SELECT TOP 1 Group_ID 
				       FROM   Groups 
				       WHERE  Group_Name = '(t+auto) The Next Generation'
					   ORDER BY Group_ID asc);--Change to group you want the person inserted into

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
				       FROM dp_Users 
				       WHERE User_Name = 'mpcrds+guinan@gmail.com');

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

 -- Create Group
-- For new group, Change Group_name
-- Group_type_ID 1 = small group
-- Ministry_ID 8 = spiritual growth. Run this query for all minitstries: SELECT * FROM dbo.Ministries

DECLARE @groupID       AS INT 
SET @groupID =        (SELECT TOP 1 Group_ID 
				       FROM   Groups 
				       WHERE  Group_Name = '(t+auto) The Next Generation'
					   ORDER BY Group_ID asc);--Change to group you want the person inserted into

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

-- Create Group
-- For new group, Change Group_name

-- Group_type_ID 1 = small group
-- Ministry_ID 8 = spiritual growth. Run this query for all minitstries: SELECT * FROM dbo.Ministries

DECLARE @groupID       AS INT 
SET @groupID =        (SELECT TOP 1 Group_ID 
				       FROM   Groups 
				       WHERE  Group_Name = '(t+auto) The Next Generation'
					   ORDER BY Group_ID asc);--Change to group you want the person inserted into

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
				       FROM dp_Users 
				       WHERE User_Name = 'mpcrds+crusher@gmail.com');

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

 -- Create Group
-- For new group, Change Group_name

-- Group_type_ID 1 = small group
-- Ministry_ID 8 = spiritual growth. Run this query for all minitstries: SELECT * FROM dbo.Ministries

DECLARE @groupID       AS INT 
SET @groupID =        (SELECT TOP 1 Group_ID 
				       FROM   Groups 
				       WHERE  Group_Name = '(t+auto) The Next Generation'
					   ORDER BY Group_ID asc);--Change to group you want the person inserted into

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
				       FROM dp_Users 
				       WHERE User_Name = 'mpcrds+ae@gmail.com');

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

 -- Create Group
-- For new group, Change Group_name
-- Group_type_ID 1 = small group
-- Ministry_ID 8 = spiritual growth. Run this query for all minitstries: SELECT * FROM dbo.Ministries

--THIS WILL FAIL since group does not exist yet
DECLARE @groupIdSG   AS INT
SET @groupIdSG = (select top 1 group_id from Groups where group_name = '(t) Albert E');

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
				       FROM dp_Users 
				       WHERE User_Name = 'mpcrds+sh@gmail.com');

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

 -- Create Group
-- For new group, Change Group_name
-- Group_type_ID 1 = small group
-- Ministry_ID 8 = spiritual growth. Run this query for all minitstries: SELECT * FROM dbo.Ministries

--THIS WILL FAIL since group does not exist yet
DECLARE @groupIdSG   AS INT
SET @groupIdSG = (select top 1 group_id from Groups where group_name = '(t) Stephen H');

INSERT INTO dbo.Group_Participants
( Group_ID  , Participant_ID, Group_Role_ID, Domain_ID, Start_Date      , Employee_Role, Auto_Promote ) 
VALUES
( @groupIdSG, @participantID, 22           , 1        , {d '2015-11-01'}, 0            , 1            );

GO
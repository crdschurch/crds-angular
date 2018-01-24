--Registered Account - Connect Users

USE [MinistryPlatform]
GO

DECLARE @contactID     AS INT
SET @contactID =      (SELECT Contact_ID 
				       FROM Contacts 
				       WHERE Email_Address = 'mpcrds+auto+matcauthon@gmail.com' and Last_Name = 'Cauthon');
	  
 
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


 -- Create Group
-- For new group, Change Group_name
-- Group_type_ID 1 = small group
-- Ministry_ID 8 = spiritual growth. Run this query for all minitstries: select * from dbo.Ministries

--Creating group participants WILL FAIL because the group doesn't exist when this script is being run. Will be fixed when 03. Test Configs is updated 
DECLARE @groupIdSG INT;
SET @groupIdSG = (select top 1 group_id from groups where group_name = '(t+auto) Band of the Red Hand');

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

 -- Create Group
-- For new group, Change Group_name
-- Group_type_ID 1 = small group
-- Ministry_ID 8 = spiritual growth. Run this query for all minitstries: select * from dbo.Ministries

--Creating group participants WILL FAIL because the group doesn't exist when this script is being run. Will be fixed when 03. Test Configs is updated 
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

 -- Create Group
-- For new group, Change Group_name
-- Group_type_ID 1 = small group
-- Ministry_ID 8 = spiritual growth. Run this query for all minitstries: select * from dbo.Ministries

--Creating group participants WILL FAIL because the group doesn't exist when this script is being run. Will be fixed when 03. Test Configs is updated 
DECLARE @groupID       AS INT 
SET @groupID =        (SELECT Group_ID 
				       FROM   Groups 
				       WHERE  Group_Name = '(t+auto) Band of the Red Hand');--Change to group you want the person inserted into

INSERT INTO dbo.Group_Participants
( Group_ID, Participant_ID, Group_Role_ID, Domain_ID, Start_Date      , Employee_Role, Auto_Promote ) 
VALUES
( @groupId, @participantID, 16           , 1        , {d '2015-11-01'}, 0            , 1            );
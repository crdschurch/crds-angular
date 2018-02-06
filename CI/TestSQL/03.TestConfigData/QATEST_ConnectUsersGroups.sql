--Registered Account - Connect Users 

USE [MinistryPlatform]
GO

--All has been transfered, here for reference
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
GO
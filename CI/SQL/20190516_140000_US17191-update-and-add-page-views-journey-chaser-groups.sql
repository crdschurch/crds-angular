-------------------------------------------------
-- US17191 - Update and add page views to support
-- Journey and Chaser groups
-- Author: Brian Cavanaugh
-- Date: 2019-05-16
-------------------------------------------------

USE [MinistryPlatform]
GO

UPDATE dp_Page_Views
SET View_Title = 'Current Chaser Participants'
WHERE Page_View_ID = 1130

GO

UPDATE dp_Page_Views
SET View_Title = 'Current Chaser Leaders'
WHERE Page_View_ID = 93316

GO

INSERT INTO [dbo].[dp_Page_Views]
	([View_Title], [Page_ID]
	,[Field_List]
	,[View_Clause]
	)
	VALUES
	('Current Journey Participants', 316
	, 'Participant_ID_Table_Contact_ID_Table.[Display_Name] AS [Display Name] , Group_ID_Table.[Group_Name] AS [Group Name] , Group_ID_Table.[Available_Online] AS [Public Flag] , Participant_ID_Table_Group_Leader_Status_ID_Table.[Group_Leader_Status] AS [Group Leader Status] , Participant_ID_Table_Contact_ID_Table_Household_ID_Table_Congregation_ID_Table.[Congregation_Name] AS [Congregation Name] , Group_ID_Table_Group_Type_ID_Table.[Group_Type] AS [Group Type] , Group_ID_Table.[Start_Date] AS [Start Date] , Group_Role_ID_Table.[Role_Title] AS [Role Title] , Group_Participants.[Group_Participant_ID] AS [Group Participant ID]'
	, 'Group_ID_Table.[End_Date] IS NULL AND Group_Participants.[End_Date] IS NULL AND Group_Role_ID_Table.[Group_Role_ID] != 22 AND EXISTS (Select group_ID from GROUP_ATTRIBUTES where group_id = group_participants.group_id and attribute_id = (select top 1 attribute_id from attributes where attribute_category_id = 1008 order by start_date desc))'
	)

GO

INSERT INTO [dbo].[dp_Page_Views]
	([View_Title], [Page_ID]
	,[Field_List]
	,[View_Clause]
	)
	VALUES
	('Current Journey Leaders', 316
	, 'Participant_ID_Table_Contact_ID_Table.[Display_Name] AS [Display Name] , Participant_ID_Table_Contact_ID_Table.[Email_Address] AS [Email Address] , Group_ID_Table.[Group_Name] AS [Group Name] , Group_ID_Table.[Start_Date] AS [Start Date] , Group_ID_Table.[Available_Online] AS [Public Flag] , Group_Role_ID_Table.[Role_Title] AS [Role Title] , Participant_ID_Table_Group_Leader_Status_ID_Table.[Group_Leader_Status] AS [Group Leader Status] , Participant_ID_Table_Contact_ID_Table_Household_ID_Table_Congregation_ID_Table.[Congregation_Name] AS [Congregation Name] , Group_ID_Table_Group_Type_ID_Table.[Group_Type] AS [Group Type] , [dbo].[crds_GetGroupParticipantCount] (Group_ID_Table.[Group_ID]) AS [Group_Participant_Count] , Group_ID_Table.[End_Date] AS [Group End Date] , Group_Participants.[End_Date] AS [Participant End Date] , Group_Role_ID_Table.[Group_Role_ID] AS [Group Role ID] , Group_ID_Table.[Group_ID] AS [Group ID] , Participant_ID_Table_Contact_ID_Table_Household_ID_Table_Address_ID_Table.[Postal_Code] AS [Postal Code]'
	, 'Group_ID_Table.[End_Date] IS NULL AND Group_Participants.[End_Date] IS NULL AND Group_Role_ID_Table.[Group_Role_ID] = 22 AND EXISTS (Select group_ID from GROUP_ATTRIBUTES where group_id = group_participants.group_id and attribute_id = (select top 1 attribute_id from attributes where attribute_category_id = 1008 order by start_date desc))'
	)

GO

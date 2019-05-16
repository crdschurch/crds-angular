-------------------------------------------------
-- US17191 - Update and add page views to support
-- Journey and Chaser groups
-- Author: Brian Cavanaugh
-- Date: 2019-05-16
-------------------------------------------------


UPDATE dp_Page_Views
SET View_Title = 'Current Chaser Participants'
WHERE Page_View_ID = 1130

UPDATE dp_Page_Views
SET View_Title = 'Current Chaser Leaders'
WHERE Page_View_ID = 93316

INSERT INTO [dbo].[dp_Page_Views]
	([View_Title], [Page_ID]
	,[Field_List]
	,[View_Clause]
	)
	VALUES
	('Current Journey Participants', 316
	, 'Participant_ID_Table_Contact_ID_Table.[Display_Name] AS [Display Name] , Group_ID_Table.[Group_Name] AS [Group Name] , Group_ID_Table.[Available_Online] AS [Public Flag] , Participant_ID_Table_Group_Leader_Status_ID_Table.[Group_Leader_Status] AS [Group Leader Status] , Participant_ID_Table_Contact_ID_Table_Household_ID_Table_Congregation_ID_Table.[Congregation_Name] AS [Congregation Name] , Group_ID_Table_Group_Type_ID_Table.[Group_Type] AS [Group Type] , Group_ID_Table.[Start_Date] AS [Start Date] , Group_Role_ID_Table.[Role_Title] AS [Role Title] , Group_Participants.[Group_Participant_ID] AS [Group Participant ID]'
	, 'Group_ID_Table.[End_Date] IS NULL AND Group_Participants.[End_Date] IS NULL AND Group_Role_ID_Table.[Group_Role_ID] != 22 AND EXISTS (Select group_ID from GROUP_ATTRIBUTES where group_id = group_participants.group_id and attribute_id = (select top 1 attribute_id from attributes where attribute_category_id = 51 order by start_date desc))'
	)

INSERT INTO [dbo].[dp_Page_Views]
	([View_Title], [Page_ID]
	,[Field_List]
	,[View_Clause]
	)
	VALUES
	('Current Journey Leaders', 316
	, 'Participant_ID_Table_Contact_ID_Table.[Display_Name] AS [Display Name] , Group_ID_Table.[Group_Name] AS [Group Name] , Group_ID_Table.[Available_Online] AS [Public Flag] , Participant_ID_Table_Group_Leader_Status_ID_Table.[Group_Leader_Status] AS [Group Leader Status] , Participant_ID_Table_Contact_ID_Table_Household_ID_Table_Congregation_ID_Table.[Congregation_Name] AS [Congregation Name] , Group_ID_Table_Group_Type_ID_Table.[Group_Type] AS [Group Type] , Group_ID_Table.[Start_Date] AS [Start Date] , Group_Role_ID_Table.[Role_Title] AS [Role Title] , Group_Participants.[Group_Participant_ID] AS [Group Participant ID]'
	, 'Group_ID_Table.[End_Date] IS NULL AND Group_Participants.[End_Date] IS NULL AND Group_Role_ID_Table.[Group_Role_ID] != 22 AND EXISTS (Select group_ID from GROUP_ATTRIBUTES where group_id = group_participants.group_id and attribute_id = (select top 1 attribute_id from attributes where attribute_category_id = 51 order by start_date desc))'
	)

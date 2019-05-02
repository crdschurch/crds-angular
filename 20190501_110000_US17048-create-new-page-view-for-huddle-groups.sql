
---------------------------------------------------------------
-- Brian Cavanaugh - Callibrity
-- 5/1/2019
-- Create new page view to display Participant Congregation and
-- rename Congregation to Group Congregation for Huddle Groups
---------------------------------------------------------------


USE [MinistryPlatform]
GO

INSERT INTO [dbo].[dp_Page_Views]
           ([View_Title]
           ,[Page_ID]
           ,[Description]
           ,[Field_List]
           ,[View_Clause]
           ,[Order_By]
           ,[User_ID]
           ,[User_Group_ID])
     VALUES
           ('Huddle - Participant Congregation'
           , 316
           , null
           , 'Participant_ID_Table_Contact_ID_Table.[Display_Name] AS [GP_Display_Name] , Participant_ID_Table_Contact_ID_Table.[Nickname] AS [Nickname] , Participant_ID_Table_Contact_ID_Table.[First_Name] AS [First Name] , Group_ID_Table.[Group_Name] AS [Group Name] , Group_ID_Table_Congregation_ID_Table.[Congregation_Name] AS [Group Congregation] , Participant_ID_Table_Contact_ID_Table_Household_ID_Table_Congregation_ID_Table.[Congregation_Name] AS [Participant Congregation] , Group_Role_ID_Table.[Role_Title] AS [Role Title] , Group_ID_Table_Ministry_ID_Table.[Ministry_Name] AS [Ministry Name] , Group_ID_Table_Parent_Group_Table.[Group_Name] AS [Parent Group] , Group_ID_Table_Group_Type_ID_Table.[Group_Type] AS [Group Type] , Group_Participants.[Start_Date] AS [Start Date] , Participant_ID_Table_Contact_ID_Table.[Email_Address] AS [GP_Email_Address] , Participant_ID_Table_Contact_ID_Table.[Mobile_Phone] AS [GP_Mobile_Phone] , Group_ID_Table.[Group_ID] AS [Group ID] , Preferred_Serving_Event_Type_ID_Table.[Event_Type] AS [Preferred Serving Time]'
           , 'Group_ID_Table_Group_Type_ID_Table.[Group_Type] LIKE ''%Huddle%'''
           , null
           , null
           , null)
GO


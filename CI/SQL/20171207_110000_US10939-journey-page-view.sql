USE MinistryPlatform
GO

DECLARE @PageViewId AS INT = 1130

SET IDENTITY_INSERT dp_Page_Views ON;

IF NOT EXISTS(SELECT * FROM dp_Page_Views WHERE Page_View_ID = @PageViewId)
BEGIN
	INSERT INTO dp_Page_Views(Page_View_ID, View_Title, Page_ID, Field_List, View_Clause)
    VALUES(@PageViewId,
	       'Journey Participants',
		   316,
		   'Participant_ID_Table_Contact_ID_Table.[Display_Name] AS [Display Name] , Group_ID_Table.[Group_Name] AS [Group Name] , Participant_ID_Table_Group_Leader_Status_ID_Table.[Group_Leader_Status] AS [Group Leader Status] , Participant_ID_Table_Contact_ID_Table_Household_ID_Table_Congregation_ID_Table.[Congregation_Name] AS [Congregation Name] , Group_ID_Table_Group_Type_ID_Table.[Group_Type] AS [Group Type] , Group_ID_Table.[Start_Date] AS [Start Date] , Group_Role_ID_Table.[Role_Title] AS [Role Title] , Group_Participants.[Group_Participant_ID] AS [Group Participant ID] , [dbo].[crds_GroupCount] (Group_Participants.Participant_ID) AS [Group_Count] , [dbo].[crds_OnSiteAndOffSite] (Group_Participants.Participant_ID) AS [Onsite_And_Offsite] , [dbo].[crds_StartedASmallGroup] (Group_Participants.Participant_ID) AS [Started_Group] ',
		   'EXISTS (Select group_ID from GROUP_ATTRIBUTES where group_id = group_participants.group_id and attribute_id = (select top 1 attribute_id from attributes where attribute_category_id = 51 order by start_date desc)) '
		   )
END

SET IDENTITY_INSERT dp_Page_Views OFF;

GO

USE MinistryPlatform;
GO

DECLARE @PAGEVIEWID INTEGER = 93316;
DECLARE @TITLE VARCHAR(100) = 'Current Journey Leaders';
DECLARE @PAGEID INTEGER = 316;
DECLARE @FIELDLIST VARCHAR(2000) = 'Participant_ID_Table_Contact_ID_Table.[Display_Name] AS [Display Name] , Participant_ID_Table_Contact_ID_Table.[Email_Address] AS [Email Address] , Group_ID_Table.[Group_Name] AS [Group Name] , Group_ID_Table.[Start_Date] AS [Start Date] , Group_ID_Table.[Available_Online] AS [Public Flag] , Group_Role_ID_Table.[Role_Title] AS [Role Title] , Participant_ID_Table_Group_Leader_Status_ID_Table.[Group_Leader_Status] AS [Group Leader Status] , Participant_ID_Table_Contact_ID_Table_Household_ID_Table_Congregation_ID_Table.[Congregation_Name] AS [Congregation Name] , Group_ID_Table_Group_Type_ID_Table.[Group_Type] AS [Group Type] , [dbo].[crds_GetGroupParticipantCount] (Group_ID_Table.[Group_ID]) AS [Group_Participant_Count] , Group_ID_Table.[End_Date] AS [Group End Date] , Group_Participants.[End_Date] AS [Participant End Date] , Group_Role_ID_Table.[Group_Role_ID] AS [Group Role ID] , Group_ID_Table.[Group_ID] AS [Group ID] , Participant_ID_Table_Contact_ID_Table_Household_ID_Table_Address_ID_Table.[Postal_Code] AS [Postal Code] ';
DECLARE @VIEWCLAUSE VARCHAR(2000) = 'Group_ID_Table.[End_Date] IS NULL  AND Group_Participants.[End_Date] IS NULL  AND Group_Role_ID_Table.[Group_Role_ID] = 22  AND EXISTS (Select group_ID from GROUP_ATTRIBUTES where group_id = group_participants.group_id and attribute_id = (select top 1 attribute_id from attributes where attribute_category_id = 51 order by start_date desc))';

IF EXISTS(SELECT * FROM dp_Page_Views where page_view_id = @PAGEVIEWID)
BEGIN
	UPDATE dp_Page_Views 
	SET View_Title = @TITLE,
	    Page_ID = @PAGEID,
		Field_List = @FIELDLIST,
		View_Clause = @VIEWCLAUSE
	WHERE page_view_id = @PAGEVIEWID
END
ELSE
BEGIN
	INSERT INTO dp_Page_Views(Page_View_ID, View_Title, Page_ID, Field_List, View_Clause)
	   VALUES (@PAGEVIEWID, @TITLE, @PAGEID, @FIELDLIST, @VIEWCLAUSE);
END
GO

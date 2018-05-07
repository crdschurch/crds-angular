-- select * from dp_Page_Views where View_Title = 'Huddle - Completed Cirriculum'
USE MinistryPlatform
GO

DECLARE @PAGEVIEWID INTEGER = 1134;

IF NOT EXISTS(SELECT * FROM dp_Page_Views WHERE Page_View_ID = @PAGEVIEWID )
BEGIN
  SET IDENTITY_INSERT dp_Page_Views ON
  INSERT INTO dp_Page_Views(Page_View_ID, View_Title, Page_ID, Field_List, View_Clause)
	  VALUES(@PAGEVIEWID,
	         'Huddle - Completed Cirriculum', 
			 316,
			 'Participant_ID_Table_Contact_ID_Table.[Display_Name] AS [Display Name] , Participant_ID_Table_Contact_ID_Table.[Email_Address] AS [Email Address] , Group_ID_Table_Group_Type_ID_Table.[Group_Type] AS [Group Type] , Participant_ID_Table_Huddle_Status_ID_Table.[Huddle_Status] AS [Huddle Status]  ',
			 'Group_ID_Table_Group_Type_ID_Table.[Group_Type] = ''Huddle''  AND Participant_ID_Table_Huddle_Status_ID_Table.[Huddle_Status_ID] IN (''2'',''4'',''5'',''6'',''7'')  '
			);
  SET IDENTITY_INSERT dp_Page_Views OFF
END
GO

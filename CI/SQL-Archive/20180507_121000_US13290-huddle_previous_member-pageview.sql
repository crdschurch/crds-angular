USE MinistryPlatform
GO

DECLARE @PAGEVIEWID INTEGER = 1133;

IF NOT EXISTS(SELECT * FROM dp_Page_Views WHERE Page_View_ID = @PAGEVIEWID )
BEGIN
  SET IDENTITY_INSERT dp_Page_Views ON
  INSERT INTO dp_Page_Views(Page_View_ID, View_Title, Page_ID, Field_List, View_Clause)
	  VALUES(@PAGEVIEWID,
	         'Huddle - Previous Members', 
			 316,
			 'Group_ID_Table.[Group_Name] AS [Group Name], Group_Role_ID_Table.[Role_Title] AS [Role Title] ,  Group_ID_Table_Congregation_ID_Table.[Congregation_Name] AS [Congregation Name] , Participant_ID_Table_Contact_ID_Table.[Display_Name] AS [Display Name] , Participant_ID_Table_Contact_ID_Table.[Email_Address] AS [Email Address],(SELECT TOP 1 Attributes.Attribute_Name FROM Contact_Attributes, Attributes where Contact_Attributes.Attribute_ID=Attributes.Attribute_ID AND Contact_Attributes.Contact_ID=Participant_ID_Table_Contact_ID_Table.[Contact_ID] and Attributes.Attribute_Type_ID = 1005) as [Generation], Participant_ID_Table_Huddle_Status_ID_Table.[Huddle_Status] AS [Huddle Status] ',
			 'Group_Participants.[End_Date] IS NOT NULL  AND Group_ID_Table_Group_Type_ID_Table.[Group_Type_ID] = ''31'' '
			);
  SET IDENTITY_INSERT dp_Page_Views OFF
END
GO

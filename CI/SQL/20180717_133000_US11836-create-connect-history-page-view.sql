USE MinistryPlatform
GO

DECLARE @PAGEVIEWID INTEGER = 1139;

IF NOT EXISTS(SELECT * FROM dp_Page_Views WHERE Page_View_ID = @PAGEVIEWID )
BEGIN
  SET IDENTITY_INSERT dp_Page_Views ON
  INSERT INTO dp_Page_Views(Page_View_ID, View_Title, Page_ID, Field_List, View_Clause)
	  VALUES(@PAGEVIEWID,
	         'Connect Participants On Map', 
			 646,
			 'cr_Connect_History.[Transaction_Date] AS [Transaction Date] , Connect_Status_ID_Table.[Connect_Status] AS [Connect Status] , Participant_ID_Table_Contact_ID_Table.[First_Name] AS [First Name] , Participant_ID_Table_Contact_ID_Table.[Last_Name] AS [Last Name] , Participant_ID_Table_Contact_ID_Table.[Email_Address] AS [Email Address] , Participant_ID_Table_Contact_ID_Table_Household_ID_Table_Address_ID_Table.[Address_Line_1] AS [Address Line 1] , Participant_ID_Table_Contact_ID_Table_Household_ID_Table_Address_ID_Table.[City] AS [City] , Participant_ID_Table_Contact_ID_Table_Household_ID_Table_Address_ID_Table.[State/Region] AS [State/Region] , Participant_ID_Table_Contact_ID_Table_Household_ID_Table_Address_ID_Table.[Postal_Code] AS [Postal Code] , Participant_ID_Table_Host_Status_ID_Table.[Description] AS [Description] ',
			 'Participant_ID_Table.[Show_On_Map] IN (''1'',''0'') '
			);
  SET IDENTITY_INSERT dp_Page_Views OFF
END
GO

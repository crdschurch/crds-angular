USE [MinistryPlatform];
GO

DECLARE @Page_View_ID INTEGER = 1137;

IF NOT EXISTS(SELECT 1 FROM dp_Page_Views WHERE Page_View_ID = @Page_View_ID )
BEGIN
    SET IDENTITY_INSERT dp_Page_Views ON
    INSERT INTO dp_Page_Views(Page_View_ID, View_Title, Page_ID, Field_List, View_Clause)
    VALUES(
        @Page_View_ID,
        'Deceased Exceptions', 
        292,
        'Contacts.Contact_ID,Contacts.Display_Name,Contact_Status_ID_Table.Contact_Status,Contacts.Date_Of_Death',
        '(Contacts.Contact_Status_ID = 3 AND Contacts.Date_Of_Death IS NULL) OR (Contacts.Contact_Status_ID <> 3 AND Contacts.Date_Of_Death IS NOT NULL)'
    );
    SET IDENTITY_INSERT dp_Page_Views OFF
END
GO

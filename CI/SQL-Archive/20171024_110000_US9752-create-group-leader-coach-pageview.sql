USE MinistryPlatform
GO

DECLARE @PAGEID INT = 355;
DECLARE @PAGEVIEWID INT = 1128;

DECLARE @FIELDLIST NVARCHAR(4000) = 'Contact_ID_Table.[First_Name] AS [First Name], Contact_ID_Table.[Last_Name] AS [Last Name], Contact_ID_Table.[Email_Address] AS [Email Address], Contact_ID_Table.[Mobile_Phone] AS [Mobile Phone], Contact_ID_Table_Household_ID_Table_Congregation_ID_Table.[Congregation_Name] AS [Congregation Name], [dbo].crds_GetCoach(Contact_ID_Table.Contact_ID) AS Coach, [dbo].crds_GetMentor(Contact_ID_Table.Contact_ID) AS Mentor'
DECLARE @VIEWCLAUSE NVARCHAR(4000) = 'Participants.[Group_Leader_Status_ID] = 4 '

DELETE dp_Page_Views WHERE Page_View_ID = @PAGEVIEWID

IF NOT EXISTS(SELECT * FROM dp_Page_Views WHERE Page_View_ID = @PAGEVIEWID)
BEGIN
	SET IDENTITY_INSERT dp_Page_Views ON
	INSERT INTO dp_Page_Views(Page_View_ID, View_Title, Page_ID, Field_List, View_Clause)
				VALUES(@PAGEVIEWID, 'Group Leader Coaches', @PAGEID, @FIELDLIST, @VIEWCLAUSE)
	SET IDENTITY_INSERT dp_Page_Views OFF
END
GO


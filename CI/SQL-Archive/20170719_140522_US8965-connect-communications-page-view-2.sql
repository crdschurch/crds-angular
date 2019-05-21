USE [MinistryPlatform]
GO

DECLARE @PageID int = 629
DECLARE @PageViewID int = 1126


IF NOT EXISTS(SELECT * FROM dp_Page_Views WHERE Page_View_ID = @PageViewID)
BEGIN
	SET IDENTITY_INSERT dp_Page_Views ON
	INSERT INTO [dbo].[dp_Page_Views]
           ([Page_View_ID]
		   ,[View_Title]
           ,[Page_ID]
           ,[Description]
           ,[Field_List]
           ,[View_Clause]
           ,[Order_By])
     VALUES
           (@PageViewID
		   ,'Groups Information'
           ,@PageID
           ,'Groups specific data for tracking community member interaction in finder'
           ,'From_Contact_ID_Table.[Display_Name] AS From_Contact_Name
              , From_Contact_ID_Table.[Email_Address] AS From_Contact_Email
              , To_Contact_ID_Table.[Display_Name] AS To_Contact_Name
              , To_Contact_ID_Table.[Email_Address] AS To_Contact_Email
              , Communication_Date, Communication_Type_ID_Table.[Communication_Type]
              , Communication_Status_ID_Table.[Communication_Status]
              , Group_ID_Table.[Group_Name] AS Group_Name'
           ,'[Communication_Type_ID] in (4,5,6)'
           ,'Communication_Date DESC')
	SET IDENTITY_INSERT dp_Page_Views OFF
END

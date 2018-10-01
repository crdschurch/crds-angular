USE [MinistryPlatform]
GO

DECLARE @PageViewID int = 1121

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
		   ,'Small Group Leader Status'
           ,355
           ,'Small Group Leader Status and when the status was last set'
           ,'Contact_ID_Table.[Nickname] AS [Nickname]
			   , Contact_ID_Table.[First_Name] AS [First Name]
			   , Contact_ID_Table.[Last_Name] AS [Last Name]
			   , Contact_ID_Table.[Email_Address] AS [Email Address]
			   , Contact_ID_Table.[Mobile_Phone] AS [Mobile Phone]
			   , Contact_ID_Table_Household_ID_Table_Congregation_ID_Table.[Congregation_Name] AS [Congregation Name]
			   , Group_Leader_Status_ID_Table.[Group_Leader_Status] AS [Group Leader Status]
			   , (SELECT MAX(Date_Time) FROM dp_Audit_Log AL INNER JOIN dp_Audit_Detail AD ON AL.Audit_Item_ID = AD.Audit_Item_ID WHERE Table_Name = ''Participants'' AND Record_ID = Participant_ID AND Field_Name = ''Group_Leader_Status_ID'') AS [Status Last Modified]'
           ,'(SELECT MAX(Date_Time) FROM dp_Audit_Log AL INNER JOIN dp_Audit_Detail AD ON AL.Audit_Item_ID = AD.Audit_Item_ID WHERE Table_Name = ''Participants'' AND Record_ID = Participant_ID AND Field_Name = ''Group_Leader_Status_ID'') IS NOT NULL'
           ,'[Status Last Modified] DESC')
	SET IDENTITY_INSERT dp_Page_Views OFF
END

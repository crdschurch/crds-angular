USE [MinistryPlatform]
GO

-- =============================================
-- Author:      Doug Shannon
-- Create date: 2017-06-01
-- Description:	Adds template for Student lead group
-- (Note: ID created via Identity Maintenance proc)
-- =============================================

DECLARE @TemplateID int = 2021
DECLARE @Body VARCHAR(max) = '<div>A community member has applied to be a group leader and may want to lead groups for students.  Please complete a background check and then let us know when it has been completed.</div></br>
							  <div>First Name: [First_Name]</div>
							  <div>Last Name: [Last_Name]</div>
							  <div>Email Address: [Email_Address]</div>
							  <div>If you have questions, email us at <a href="mailto:groups@crossroads.net">groups@crossroads.net</a>.</div><br />'
DECLARE @Subject VARCHAR(max) = 'Action Required: Group Leader Application - Background Check'

IF NOT EXISTS (SELECT 1 FROM [dbo].[dp_Communications] WHERE Communication_ID = @TemplateID)
BEGIN
	SET IDENTITY_INSERT [dbo].[dp_Communications] ON
	INSERT INTO [dp_Communications]
	(
		 [Communication_ID]
		,[Author_User_ID]
		,[Subject]
		,[Body]
		,[Domain_ID]
		,[Start_Date]
		,[From_Contact]
		,[Reply_to_Contact]
		,[Template]
		,[Active]
	)
	VALUES
	(
		 @TemplateID
		,5
	    ,@Subject
		,@Body
		,1
		,GetDate()
		,7675411 -- accounts@crossroads.net
		,7675411 -- accounts@crossroads.net
		,1
		,1
	)

	SET IDENTITY_INSERT [dbo].[dp_Communications] OFF
END
ELSE
BEGIN
   UPDATE [dbo].[dp_Communications]
   SET [Body] = @Body
      ,[Subject] = @Subject
	  ,[From_Contact] = 7675411
	  ,[Reply_to_Contact] = 7675411
	  ,[Template] = 1
	  ,[Active] = 1
   WHERE Communication_ID = @TemplateID
END
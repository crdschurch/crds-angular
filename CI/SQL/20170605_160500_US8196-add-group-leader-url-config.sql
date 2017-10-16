USE [MinistryPlatform]
GO

-- =============================================
-- Author:      Doug Shannon
-- Create date: 2017-06-05
-- Description:	Adds Group Leader URL Config Value
-- =============================================

DECLARE @Configuration_Setting_ID int = 301
DECLARE @Application_Code VARCHAR(max) = 'GROUPLEADER'
DECLARE @Key_Name VARCHAR(max) = 'UrlSegment'
DECLARE @Config_Value VARCHAR(max) = '/groups/leader'
DECLARE @Config_Description VARCHAR(max) = 'This acts as the switch between the new and old group leader application. {/group-leader (new)} or {/groups/leader (old)}.'
DECLARE @Domain_ID int = 1
DECLARE @Warning VARCHAR(max) = 'Incorrectly editing configuration settings can result in application errors. Changes may not take effect immediately. Contact support@thinkministry.com if you would like assistance.'

IF NOT EXISTS (SELECT 1 FROM [dbo].[dp_Configuration_Settings] WHERE Configuration_Setting_ID = @Configuration_Setting_ID)
BEGIN
	SET IDENTITY_INSERT [dbo].[dp_Configuration_Settings] ON
	INSERT INTO [dp_Configuration_Settings]
	(
		 [Configuration_Setting_ID]
		,[Application_Code]
		,[Key_Name]
		,[Value]
		,[Description]
		,[Domain_ID]
		,[_Warning]
	)
	VALUES
	(
		 @Configuration_Setting_ID
		,@Application_Code
	    ,@Key_Name
		,@Config_Value
		,@Config_Description
		,@Domain_ID
		,@Warning
	)

	SET IDENTITY_INSERT [dbo].[dp_Configuration_Settings] OFF
END
ELSE
BEGIN
   UPDATE [dbo].[dp_Configuration_Settings]
   SET   [Application_Code] = @Application_Code
		,[Key_Name] = @Key_Name
		,[Value] = @Config_Value
		,[Description] = @Config_Description
		,[Domain_ID] = @Domain_ID
		,[_Warning] = @Warning
   WHERE Configuration_Setting_ID = @Configuration_Setting_ID
END
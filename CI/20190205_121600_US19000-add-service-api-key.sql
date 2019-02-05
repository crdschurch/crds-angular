USE [MinistryPlatform]
GO

-- =============================================
-- Author:      John Cleaver
-- Create date: 2019-02-05
-- Description:	Adds Finance API Key Config Value
-- =============================================

DECLARE @Application_Code VARCHAR(max) = 'SERVICES'
DECLARE @Key_Name VARCHAR(max) = 'GatewayServiceKey'
DECLARE @Config_Value VARCHAR(max) = '77811978-0c01-4e63-9703-14e58846533b'
DECLARE @Config_Description VARCHAR(max) = 'This is used to validate a service to service call to Gateway from another service.'
DECLARE @Domain_ID int = 1
DECLARE @Warning VARCHAR(max) = 'Changing this setting will cause Gateway to not accept external service calls.'

IF NOT EXISTS (SELECT 1 FROM [dbo].[dp_Configuration_Settings] WHERE Key_Name = @Key_Name)
BEGIN
	INSERT INTO [dp_Configuration_Settings]
	(
		[Application_Code]
		,[Key_Name]
		,[Value]
		,[Description]
		,[Domain_ID]
		,[_Warning]
	)
	VALUES
	(
		@Application_Code
	    ,@Key_Name
		,@Config_Value
		,@Config_Description
		,@Domain_ID
		,@Warning
	)
END

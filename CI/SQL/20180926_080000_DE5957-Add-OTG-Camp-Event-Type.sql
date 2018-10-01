USE [MinistryPlatform]
GO

SET IDENTITY_INSERT [dbo].[Event_Types] ON

--EVENT_TYPE_ID picked manually - 9 is not used in any of the environment
--EXEC [dbo].[Get_Next_Available_ID] @tableName='Event_Types', @description = 'OTG Camp'; threw 'Could not find table'
DECLARE @OTG_CAMP_EVENT_TYPE_ID int = 9;
DECLARE @OTG_CAMP_EVENT_TYPE_NAME nvarchar(50) = N'OTG Camp';
DECLARE @OTG_CAMP_EVENT_TYPE_DESCRIPTION nvarchar(50) = N'Off site camps';
DECLARE @DEFAULT_DOMAIN_ID int = 1;

IF NOT EXISTS( SELECT 1 FROM [dbo].[Event_Types] WHERE [Event_Type] = @OTG_CAMP_EVENT_TYPE_NAME)
BEGIN

	INSERT INTO [dbo].[Event_Types]
			   ([Event_Type_ID]
			   ,[Event_Type]
			   ,[Description]
			   ,[Domain_ID]
			   ,[Allow_Multiday_Event]
			   ,[Show_On_Event_Tool])
		 VALUES
			   (@OTG_CAMP_EVENT_TYPE_ID,
			    @OTG_CAMP_EVENT_TYPE_NAME
			   ,@OTG_CAMP_EVENT_TYPE_DESCRIPTION
			   ,@DEFAULT_DOMAIN_ID
			   ,1
			   ,1)
END

SET IDENTITY_INSERT [dbo].[Event_Types] OFF
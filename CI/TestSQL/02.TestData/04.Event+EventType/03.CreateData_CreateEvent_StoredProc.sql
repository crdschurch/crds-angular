USE [MinistryPlatform]
GO
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

-- =============================================
-- Author:      Henney, Sarah
-- Create date: 01/22/2018
-- Description: Creates (if nonexistent) or Updates event. Adds event to group if group given.
-- Output:      @event_id contains the event id, @event_group_id contains the event group id,
--              @error_message contains basic error message
-- =============================================


-- Defines cr_QA_Create_Event
IF NOT EXISTS ( SELECT  *
	FROM    sys.objects
	WHERE   object_id = OBJECT_ID(N'cr_QA_Create_Event')
			AND type IN ( N'P', N'PC' ) )
	EXEC('CREATE PROCEDURE dbo.cr_QA_Create_Event
	@event_name nvarchar(75),
	@start_date datetime,
	@end_date datetime,
	@event_type_name nvarchar(50),
	@congregation_id int,
	@primary_contact_email nvarchar(254),
	@program_name nvarchar(130),
	@location_id int,
	@group_name nvarchar(75),
	@error_message nvarchar(500) OUTPUT,
	@event_id int OUTPUT,
	@event_group_id int OUTPUT AS SET NOCOUNT ON;')
GO
ALTER PROCEDURE [dbo].[cr_QA_Create_Event]
	@event_name nvarchar(75),
	@start_date datetime,
	@end_date datetime,
	@event_type_name nvarchar(50),
	@congregation_id int,
	@primary_contact_email nvarchar(254),
	@program_name nvarchar(130),
	@location_id int,
	@group_name nvarchar(75),
	@error_message nvarchar(500) OUTPUT,
	@event_id int OUTPUT,
	@event_group_id int OUTPUT
AS
BEGIN
	SET NOCOUNT ON;
	
	--Enforce required parameters
	IF @event_name is null
	BEGIN
		SET @error_message = 'Event name cannot be null'+CHAR(13);
		RETURN;
	END;


	--Required fields
	SET @congregation_id = ISNULL(@congregation_id, 5); --Not site specific
	SET @start_date = ISNULL(@start_date, DATEADD(yy, DATEDIFF(yy, 0, GETDATE()), 0)); --Defaults to 1/1/[current year]
	SET @end_date = ISNULL(@end_date, DATEADD(yy, DATEDIFF(yy, 0, GETDATE()) + 1, -1)); --Defaults to 12/31/[current year]

	DECLARE @allow_checkin bit = 1;
	DECLARE @cancelled bit = 0;
	DECLARE @featured_on_calendar bit = 0;
	DECLARE @ignore_program_groups bit = 0;
	DECLARE @setup_time smallint = 0;
	DECLARE @cleanup_time smallint = 0;
	DECLARE @on_batch_tool bit = 0;
	DECLARE @prohibit_guests bit = 0;
	DECLARE @send_reminder bit = 1;
	DECLARE @visibility_level int = 4; --Public
	DECLARE @force_login bit = 0;

	DECLARE @event_type_id int;
	SET @event_type_id = (SELECT TOP 1 Event_Type_ID FROM [dbo].Event_Types WHERE Event_Type = @event_type_name ORDER BY Event_Type_ID ASC);
	IF @event_type_id is null
	BEGIN
		SET @error_message = 'Could not find event type with name '+@event_type_name+CHAR(13);
		RETURN;
	END;

	DECLARE @contact_id int = (SELECT Contact_ID FROM [dbo].dp_Users WHERE User_Name = @primary_contact_email);
	IF @contact_id is null
	BEGIN
		SET @error_message = 'Could not find contact with email '+@primary_contact_email+CHAR(13);
		RETURN;
	END;

	DECLARE @program_id int = (SELECT TOP 1 Program_ID FROM [dbo].Programs where Program_Name = @program_name ORDER BY Program_ID ASC);
	IF @program_id is null
	BEGIN
		SET @error_message = 'Could not find program with name '+@program_name+CHAR(13);
		RETURN;
	END;


	--Optional fields
	DECLARE @approved bit = 1;
	DECLARE @web_approved bit = 1;
	DECLARE @reminder_sent bit = 0;
	DECLARE @reminder_days_prior_id int = 2; --2 days
	
			
	--Create/Update event
	--Event names do not need to be unique, so identify by time too
	SET @event_id = (SELECT TOP 1 Event_ID FROM [dbo].Events WHERE Event_Title = @event_name 
	AND Event_Start_Date = @start_date AND Event_End_Date = @end_date ORDER BY Event_ID ASC);

	IF @event_id is null
	BEGIN
		INSERT INTO [dbo].Events 
		(Event_Title,Event_Type_ID ,Congregation_ID ,Program_ID ,Primary_Contact,Event_Start_Date,Event_End_Date,Minutes_for_Setup,Minutes_for_Cleanup,Cancelled ,Visibility_Level_ID,Featured_On_Calendar ,[Allow_Check-in],Ignore_Program_Groups ,Prohibit_Guests ,Send_Reminder ,Domain_ID,On_Donation_Batch_Tool,Force_Login ) VALUES
		(@event_name,@event_type_id,@congregation_id,@program_id,@contact_id    ,@start_date     ,@end_date     ,@setup_time      ,@cleanup_time      ,@cancelled,@visibility_level  ,@featured_on_calendar,@allow_checkin  ,@ignore_program_groups,@prohibit_guests,@send_reminder,1        ,@on_batch_tool        ,@force_login);

		SET @event_id = SCOPE_IDENTITY();
	END;

	IF @event_id is not null
	BEGIN
		UPDATE [dbo].Events
		SET Event_Type_ID = @event_type_id,
		Congregation_ID = @congregation_id,
		Program_ID = @program_id,
		Primary_Contact = @contact_id,
		Minutes_for_Setup = @setup_time,
		Minutes_for_Cleanup = @cleanup_time,
		Cancelled = @cancelled,
		Visibility_Level_ID = @visibility_level,
		Featured_On_Calendar = @featured_on_calendar,
		[Allow_Check-in] = @allow_checkin,
		Ignore_Program_Groups = @ignore_program_groups,
		Prohibit_Guests = @prohibit_guests,
		On_Donation_Batch_Tool = @on_batch_tool,
		Force_Login = @force_login,
		Location_ID = @location_id,
		Send_Reminder = @send_reminder,
		Reminder_Sent = @reminder_sent,
		Reminder_Days_Prior_ID = @reminder_days_prior_id,
		_Approved = @approved,
		_Web_Approved = @web_approved
		WHERE Event_ID = @event_id;
	END
	ELSE
	BEGIN
		SET @error_message = 'Could not create event for some reason'+CHAR(13);
		RETURN;
	END;
	

	--Add event to group
	IF @group_name is not null
	BEGIN
		DECLARE @event_group_error nvarchar(500);
		EXEC [dbo].[cr_QA_Create_Event_Group] @group_name, @event_id, 
		@error_message = @event_group_error OUTPUT, @event_group_id = @event_group_id OUTPUT;

		SET @error_message = @error_message+@event_group_error;
	END;		
END
GO
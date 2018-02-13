USE [MinistryPlatform]
GO
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

-- =============================================
-- Author:		Henney, Sarah
-- Create date: 02/05/2018
-- Description:	Creates (if nonexistent) or Updates event participant. Event will be matched by name and given start
--              date, since event names are not all unique.
-- Output:      @event_participant_id contains the event participant id, @error_message contains basic error message
-- =============================================


-- Defines cr_QA_Create_Event_Participant
IF NOT EXISTS ( SELECT  *
	FROM    sys.objects
	WHERE   object_id = OBJECT_ID(N'cr_QA_Create_Event_Participant')
			AND type IN ( N'P', N'PC' ) )
	EXEC('CREATE PROCEDURE dbo.cr_QA_Create_Event_Participant
	@participant_email nvarchar(254),
	@event_name nvarchar(75),
	@event_start_date datetime,
	@participant_status_id int,
	@time_in datetime,
	@time_confirmed datetime,
	@setup_date datetime,
	@error_message nvarchar(500) OUTPUT,
	@event_participant_id int OUTPUT AS SET NOCOUNT ON;')
GO
ALTER PROCEDURE [dbo].[cr_QA_Create_Event_Participant]
	@participant_email nvarchar(254),
	@event_name nvarchar(75),
	@event_start_date datetime,
	@participant_status_id int,
	@time_in datetime,
	@time_confirmed datetime,
	@setup_date datetime,
	@error_message nvarchar(500) OUTPUT,
	@event_participant_id int OUTPUT
AS
BEGIN
	SET NOCOUNT ON;

	--Enforce required parameters
	IF @participant_email is null OR @event_name is null
	BEGIN
		SET @error_message = 'Participant email and event name cannot be null'+CHAR(13);
		RETURN;
	END;
	

	--Required fields
	SET @participant_status_id = ISNULL(@participant_status_id, 2); --Registered
	SET @event_start_date = ISNULL(@event_start_date, DATEADD(yy, DATEDIFF(yy, 0, GETDATE()), 0)); --Defaults to 1/1/[current year]

	DECLARE @event_id int;
	SET @event_id = (SELECT TOP 1 Event_ID FROM [dbo].Events WHERE Event_Title = @event_name AND Event_Start_Date = @event_start_date ORDER BY Event_ID ASC);
	IF @event_id is null
	BEGIN
		SET @error_message = 'Event with name '+@event_name+' and start date '+convert(nvarchar, @event_start_date)+' could not be found'+CHAR(13);
		RETURN;
	END;

	DECLARE @contact_id int = (SELECT Contact_ID FROM [dbo].dp_Users WHERE User_Name = @participant_email);
	IF @contact_id is null
	BEGIN
		SET @error_message = 'Could not find contact with email '+@participant_email+CHAR(13);
		RETURN;
	END;

	DECLARE @participant_id int = (SELECT Participant_Record FROM [dbo].Contacts WHERE Contact_ID = @contact_id);
	IF @participant_id is null
	BEGIN
		--Use defaults
		EXEC [dbo].[cr_QA_Create_Participant] @participant_email, null, null, null, null,
		@error_message = @error_message OUTPUT, @participant_id = @participant_id OUTPUT;

		IF @participant_id is null
			RETURN;
	END;

	
	--Create/Update event participant
	SET @event_participant_id = (SELECT TOP 1 Event_Participant_ID FROM [dbo].Event_Participants WHERE Participant_ID = @participant_id
	AND Event_ID = @event_id ORDER BY Event_Participant_ID ASC);

	IF @event_participant_id is null
	BEGIN
		INSERT INTO [dbo].Event_Participants 
		(Event_ID ,Participant_ID ,Participation_Status_ID,Domain_ID) VALUES
		(@event_id,@participant_id,@participant_status_id ,1        );

		SET @event_participant_id = SCOPE_IDENTITY();
	END;

	IF @event_participant_id is not null
	BEGIN
		UPDATE [dbo].Event_Participants
		SET Participation_Status_ID = @participant_status_id,
		Time_In = @time_in,
		Time_Confirmed = @time_confirmed,
		_Setup_Date = @setup_date
		WHERE Event_Participant_ID = @event_participant_id;
	END;
END
GO
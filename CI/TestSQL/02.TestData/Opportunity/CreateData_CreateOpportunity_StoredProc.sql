USE [MinistryPlatform]
GO
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

-- =============================================
-- Author:		Henney, Sarah
-- Create date: 01/15/2018
-- Description:	Creates (if nonexistent) or Updates an opportunity
-- Output:      @opportunity_id contains the opportunity id, @error_message contains basic error message
-- =============================================


-- Defines cr_QA_Create_Opportunity
IF NOT EXISTS ( SELECT  *
	FROM    sys.objects
	WHERE   object_id = OBJECT_ID(N'cr_QA_Create_Opportunity')
			AND type IN ( N'P', N'PC' ) )
	EXEC('CREATE PROCEDURE dbo.cr_QA_Create_Opportunity
	@opportunity_name nvarchar(50),
	@contact_email varchar(254),
	@program_name nvarchar(130),
	@role_id int,
	@shift_start time(7),
	@shift_end time(7),
	@minimum_participants smallint,
	@maximum_participants smallint,
	@room_name nvarchar(50),
	@group_name nvarchar(75),
	@event_type nvarchar(50),
	@description nvarchar(2000),
	@publish_date datetime,
	@signup_deadline int,
	@error_message nvarchar(500) OUTPUT,
	@opportunity_id int OUTPUT AS SET NOCOUNT ON;')
GO
ALTER PROCEDURE [dbo].[cr_QA_Create_Opportunity]
	@opportunity_name nvarchar(50),
	@contact_email varchar(254),
	@program_name nvarchar(130),
	@role_id int,
	@shift_start time(7),
	@shift_end time(7),
	@minimum_participants smallint,
	@maximum_participants smallint,
	@room_name nvarchar(50),
	@group_name nvarchar(75),
	@event_type nvarchar(50),
	@description nvarchar(2000),
	@publish_date datetime,
	@signup_deadline int,
	@error_message nvarchar(500) OUTPUT,
	@opportunity_id int OUTPUT
AS
BEGIN
	SET NOCOUNT ON;
	
	--Enforce required parameters
	IF @opportunity_name is null OR @contact_email is null
	BEGIN
		SET @error_message = 'Opportunity name and contact email cannot be null'+CHAR(13);
		RETURN;
	END;


	--Required fields
	SET @publish_date = ISNULL(@publish_date, DATEADD(yy, DATEDIFF(yy, 0, GETDATE()), 0)); --Defaults to 1/1/[current year]
	SET @role_id = ISNULL(@role_id, 16); --Member

	DECLARE @send_reminder bit = 1;
	DECLARE @visibility_level int = 4; --Public

	DECLARE @contact_id int = (SELECT Contact_ID FROM [dbo].dp_Users WHERE User_Name = @contact_email);
	IF @contact_id is null
	BEGIN
		SET @error_message = 'Could not find contact with email '+@contact_email+CHAR(13);
		RETURN;
	END;

	DECLARE @program_id int = (SELECT TOP 1 Program_ID FROM [dbo].Programs where Program_Name = @program_name ORDER BY Program_ID ASC);
	IF @program_id is null
	BEGIN
		SET @error_message = 'Could not find program with name '+@program_name+CHAR(13);
		RETURN;
	END;


	--Optional fields
	DECLARE @event_id int = null;
	DECLARE @minimum_age tinyint = 0;
	
	DECLARE @reminder_days_prior int = null;
	IF @send_reminder = 1
		SET @reminder_days_prior = 3;

	DECLARE @reminder_template_id int = null;
	IF @program_id in (83, 109) --KC Serve signup
		SET @reminder_template_id = 108700;
	IF @program_id in (106, 111) --First impressions, Spiritual Growth
		SET @reminder_template_id = 14567;

	DECLARE @event_type_id int = null;
	IF @event_type is not null
	BEGIN
		SET @event_type_id = (SELECT TOP 1 Event_Type_ID FROM [dbo].Event_Types WHERE Event_Type = @event_type ORDER BY Event_Type_ID ASC);
		IF @event_type_id is null
			SET @error_message = 'Could not find event type with name '+@event_type+'. Event type not included in opportunity '+@opportunity_name+CHAR(13);
	END;

	DECLARE @group_id int = null;
	IF @group_name is not null
	BEGIN
		SET @group_id = (SELECT TOP 1 Group_ID FROM [dbo].Groups WHERE Group_Name = @group_name ORDER BY Group_ID ASC);
		IF @group_id is null
			SET @error_message = @error_message+'Could not find group with name '+@group_name+'. Group not included in opportunity '+@opportunity_name+CHAR(13);
	END;
	
	
	--Create/Update opportunity
	SET @opportunity_id = (SELECT TOP 1 Opportunity_ID FROM [dbo].Opportunities WHERE Opportunity_Title = @opportunity_name ORDER BY Opportunity_ID ASC);
	IF @opportunity_id is null
	BEGIN
		INSERT INTO [dbo].Opportunities
		(Opportunity_Title,Group_Role_ID,Program_ID ,Visibility_Level_ID,Contact_Person,Publish_Date ,Send_Reminder ,Domain_ID) VALUES
		(@opportunity_name,@role_id     ,@program_id,@visibility_level  ,@contact_id   ,@publish_date,@send_reminder,1        );

		SET @opportunity_id = SCOPE_IDENTITY();
	END;

	IF @opportunity_id is not null
	BEGIN
		UPDATE [dbo].Opportunities
		SET Group_Role_ID = @role_id,
		Program_ID = @program_id, 
		Contact_Person = @contact_id,
		Visibility_Level_ID = @visibility_level,
		Publish_Date = @publish_date,
		Send_Reminder = @send_reminder,
		Add_to_Group = @group_id,
		Add_to_Event = @event_id,
		Minimum_Age = @minimum_age,
		Minimum_Needed = @minimum_participants,
		Maximum_Needed = @maximum_participants,
		Shift_Start = @shift_start,
		Shift_End = @shift_end,
		Event_Type_ID = @event_type_id,
		Sign_Up_Deadline_ID = @signup_deadline,
		Room = @room_name,
		Reminder_Days_Prior = @reminder_days_prior,
		Description = @description,  
		Reminder_Template = @reminder_template_id
		WHERE Opportunity_ID = @opportunity_id;
	END;
END
GO
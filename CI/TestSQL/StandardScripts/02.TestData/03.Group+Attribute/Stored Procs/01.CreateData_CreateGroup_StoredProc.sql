USE [MinistryPlatform]
GO
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

-- =============================================
-- Author:      Henney, Sarah
-- Create date: 02/05/2018
-- Description: Creates (if nonexistent) or Updates group information
-- Output:      @group_id contains the group id, @error_message contains basic error message
-- =============================================


-- Defines cr_QA_Create_Group
IF NOT EXISTS ( SELECT  *
	FROM    sys.objects
	WHERE   object_id = OBJECT_ID(N'cr_QA_Create_Group')
			AND type IN ( N'P', N'PC' ) )
	EXEC('CREATE PROCEDURE dbo.cr_QA_Create_Group
	@group_name nvarchar(75),
	@primary_contact_email nvarchar(254),
	@group_type_id int,
	@ministry_id int,
	@congregation_id int,
	@start_date datetime,
	@primary_contact_is_host bit,
	@child_care_available bit,
	@enable_waiting_list bit,
	@target_size int,
	@description nvarchar(max),
	@is_public nvarchar(1),
	@is_blog_enabled nvarchar(1),
	@is_web_enabled nvarchar(1),
	@deadline_passed_message_id int,
	@meeting_time time(7),
	@meeting_day nvarchar(10),
	@meeting_frequency_id int,
	@available_online bit,
	@error_message nvarchar(500) OUTPUT,
	@group_id int OUTPUT AS SET NOCOUNT ON;')
GO
ALTER PROCEDURE [dbo].[cr_QA_Create_Group] 
	@group_name nvarchar(75),
	@primary_contact_email nvarchar(254),
	@group_type_id int,
	@ministry_id int,
	@congregation_id int,
	@start_date datetime,
	@primary_contact_is_host bit,
	@child_care_available bit,
	@enable_waiting_list bit,
	@target_size int,
	@description nvarchar(max),
	@is_public nvarchar(1),
	@is_blog_enabled nvarchar(1),
	@is_web_enabled nvarchar(1),
	@deadline_passed_message_id int,
	@meeting_time time(7),
	@meeting_day nvarchar(10),
	@meeting_frequency_id int,
	@available_online bit,
	@error_message nvarchar(500) OUTPUT,
	@group_id int OUTPUT
AS
BEGIN
	SET NOCOUNT ON;

	--Enforce required parameters
	IF @group_name is null
	BEGIN
		SET @error_message = 'Group name cannot be null'+CHAR(13);
		RETURN;
	END;


	--Required fields
	SET @group_type_id = ISNULL(@group_type_id, 2); --General group
	SET @ministry_id = ISNULL(@ministry_id, 8); --Spiritual growth
	SET @congregation_id = ISNULL(@congregation_id, 5); --Not site specific
	SET @start_date = ISNULL(@start_date, DATEADD(yy, DATEDIFF(yy, 0, GETDATE()), 0)); --Defaults to 1/1/[current year]
	SET @child_care_available = ISNULL(@child_care_available, 0);
	
	DECLARE @kc_sort_order int = 999;
	DECLARE @kids_welcome bit = 0;
	DECLARE @promote_participants_only bit = 0;

	IF @primary_contact_email is null
	BEGIN
		SET @error_message = 'Primary contact email cannot be null'+CHAR(13);
		RETURN;
	END;
	DECLARE @primary_contact_id int = (SELECT Contact_ID FROM [dbo].dp_Users WHERE User_Name = @primary_contact_email);
	IF @primary_contact_id is null
	BEGIN
		SET @error_message = 'Could not find contact with email '+@primary_contact_email+CHAR(13);
		RETURN;
	END;


	--Optional fields
	SET @available_online = ISNULL(@available_online, 0);
	
	DECLARE @offsite_meeting_address int;
	IF ISNULL(@primary_contact_is_host, 0) = 1
	BEGIN
		DECLARE @household_id int = (SELECT Household_ID FROM [dbo].Contacts WHERE Contact_ID = @primary_contact_id);	
		IF @household_id is not null
			SET @offsite_meeting_address = (SELECT Address_ID FROM [dbo].Households WHERE Household_ID = @household_id);
		ELSE
			SET @error_message = 'Contact '+@primary_contact_email+' does not have an address. Cannot set group meeting location to host address.'+CHAR(13);
	END;

	IF @meeting_day is not null
	BEGIN
		DECLARE @meeting_day_id int = (SELECT Meeting_Day_ID FROM [dbo].Meeting_Days WHERE Meeting_Day = @meeting_day);
		IF @meeting_day_id is null
		BEGIN
			SET @error_message = @error_message + 'Meeting day called '+@meeting_day+' could not be found. Using default day instead.';
			SET @meeting_day_id = 2; --Monday
		END;
	END
	
	
	--Create/Update group
	SET @group_id = (SELECT TOP 1 Group_ID FROM [dbo].Groups WHERE Group_Name = @group_name ORDER BY Group_ID ASC);
	IF @group_id is null
	BEGIN
		INSERT INTO [dbo].Groups 
		(Group_Name ,Group_Type_ID ,Ministry_ID ,Congregation_ID ,Primary_Contact    ,Start_Date ,Domain_ID,Child_Care_Available ,KC_Sort_Order ,Kids_Welcome ,Promote_Participants_Only ) VALUES
		(@group_name,@group_type_id,@ministry_id,@congregation_id,@primary_contact_id,@start_date,1        ,@child_care_available,@kc_sort_order,@kids_welcome,@promote_participants_only);

		SET @group_id = SCOPE_IDENTITY();
	END;

	IF @group_id is not null
	BEGIN
		UPDATE [dbo].Groups
		SET Group_Type_ID = @group_type_id,
		Ministry_ID = @ministry_id, 
		Congregation_ID = @congregation_id,
		Primary_Contact = @primary_contact_id,
		Start_Date = @start_date,
		Child_Care_Available = @child_care_available,
		KC_Sort_Order = @kc_sort_order,
		Kids_Welcome = @kids_welcome,
		Promote_Participants_Only = @promote_participants_only,
		Enable_Waiting_List = @enable_waiting_list,
		__IsPublic = @is_public,
		__ISBlogEnabled = @is_blog_enabled,
		__ISWebEnabled = @is_web_enabled,
		Deadline_Passed_Message_ID = @deadline_passed_message_id,
		Meeting_Time = @meeting_time,
		Meeting_Day_ID = @meeting_day_id,
		Meeting_Frequency_ID = @meeting_frequency_id,
		Available_Online = @available_online,
		Offsite_Meeting_Address = @offsite_meeting_address,
		Description = @description,
		Target_Size = @target_size
		WHERE Group_ID = @group_id;
	END;
END
GO
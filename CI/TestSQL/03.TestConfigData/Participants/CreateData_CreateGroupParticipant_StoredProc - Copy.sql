USE [MinistryPlatform]
GO
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

-- =============================================
-- Author:		Henney, Sarah
-- Create date: 02/05/2018
-- Description:	Creates (if nonexistent) or Updates group participant
-- Output:      @group_participant_id contains the group participant id, @error_message contains basic error message
-- =============================================


-- Defines cr_QA_Create_Group_Participant
IF NOT EXISTS ( SELECT  *
	FROM    sys.objects
	WHERE   object_id = OBJECT_ID(N'cr_QA_Create_Group_Participant')
			AND type IN ( N'P', N'PC' ) )
	EXEC('CREATE PROCEDURE dbo.cr_QA_Create_Group_Participant
	@group_name nvarchar(75),
	@participant_email nvarchar(254),
	@group_role_id int,
	@start_date datetime,
	@error_message nvarchar(500) OUTPUT,
	@group_participant_id int OUTPUT AS SET NOCOUNT ON;')
GO
ALTER PROCEDURE [dbo].[cr_QA_Create_Group_Participant]
	@group_name nvarchar(75),
	@participant_email nvarchar(254),
	@group_role_id int,
	@start_date datetime,
	@error_message nvarchar(500) OUTPUT,
	@group_participant_id int OUTPUT
AS
BEGIN
	SET NOCOUNT ON;

	--Enforce required parameters
	IF @participant_email is null OR @group_name is null
	BEGIN
		SET @error_message = 'Participant email and group name cannot be null'+CHAR(13);
		RETURN;
	END;
	

	--Required fields
	SET @group_role_id = ISNULL(@group_role_id, 1); --Participant
	SET @start_date = ISNULL(@start_date, GETDATE());

	DECLARE @auto_promote bit = 1;
			
	DECLARE @group_id int = (SELECT TOP 1 Group_ID FROM [dbo].Groups WHERE Group_Name = @group_name ORDER BY Group_ID ASC);
	IF @group_id is null
	BEGIN
		SET @error_message = 'Group with name '+@group_name+' could not be found'+CHAR(13);
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
		EXEC [dbo].[cr_QA_Create_Participant] @participant_email, @start_date, null, null, null,
		@error_message = @error_message OUTPUT, @participant_id = @participant_id OUTPUT;

		IF @participant_id is null
		BEGIN
			RETURN;
		END;
	END;

	DECLARE @employee_role bit = 0;
	IF @participant_email like '%@crossroads.net'
		SET @employee_role = 1;

	
	--Create/Update group participant
	SET @group_participant_id = (SELECT TOP 1 Group_Participant_ID FROM [dbo].Group_Participants WHERE Participant_ID = @participant_id
	AND Group_ID = @group_id ORDER BY Group_Participant_ID ASC);

	IF @group_participant_id is null
	BEGIN
		INSERT INTO [dbo].Group_Participants 
		(Group_ID ,Participant_ID ,Group_Role_ID ,Start_Date ,Employee_Role ,Auto_Promote ,Domain_ID) VALUES
		(@group_id,@participant_id,@group_role_id,@start_date,@employee_role,@auto_promote,1        );

		SET @group_participant_id = SCOPE_IDENTITY();
	END;

	IF @group_participant_id is not null
	BEGIN
		UPDATE [dbo].Group_Participants
		SET Group_Role_ID = @group_role_id,
		Start_Date = @start_date,
		Employee_Role = @employee_role,
		Auto_Promote = @auto_promote
		WHERE Group_Participant_ID = @group_participant_id;
	END;
END
GO
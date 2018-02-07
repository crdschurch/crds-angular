USE [MinistryPlatform]
GO
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

-- =============================================
-- Author:		Henney, Sarah
-- Create date: 01/25/2018
-- Description:	Create (if nonexistent) or Update Participant record
-- =============================================


-- Defines cr_QA_Create_Participant
IF NOT EXISTS ( SELECT  *
	FROM    sys.objects
	WHERE   object_id = OBJECT_ID(N'cr_QA_Create_Participant')
			AND type IN ( N'P', N'PC' ) )
	EXEC('CREATE PROCEDURE dbo.cr_QA_Create_Participant
	@participant_email varchar(255),
	@start_date datetime,
	@show_on_map bit,
	@host_status int,
	@group_leader_status int,
	@error_message nvarchar(500) OUTPUT,
	@participant_id int OUTPUT AS SET NOCOUNT ON;')
GO
ALTER PROCEDURE [dbo].[cr_QA_Create_Participant] 
	@participant_email varchar(255),
	@start_date datetime,
	@show_on_map bit,
	@host_status int,
	@group_leader_status int,
	@error_message nvarchar(500) OUTPUT,
	@participant_id int OUTPUT
AS
BEGIN
	SET NOCOUNT ON;
	
	--Enforce required parameters
	IF @participant_email is null
	BEGIN
		SET @error_message = 'Participant email cannot be null'+CHAR(13);
		RETURN;
	END;
	
	--Required fields
	SET @start_date = ISNULL(@start_date, GETDATE());
	SET @show_on_map = ISNULL(@show_on_map, 0);
	SET @host_status = ISNULL(@host_status, 0); --Not applied
	SET @group_leader_status = ISNULL(@group_leader_status, 1); --Not applied
	
	DECLARE @participant_type_id int = 1;--*Temp Participant Type

	DECLARE @contact_id int;
	SET @contact_id = (SELECT Contact_ID FROM [dbo].dp_Users WHERE User_Name = @participant_email);
	IF @contact_id is null
	BEGIN
		SET @error_message = 'Could not find contact with email '+@participant_email+CHAR(13);
		RETURN;
	END;
	

	--Create/Update participant
	SET @participant_id = (SELECT Participant_Record FROM [dbo].Contacts WHERE Contact_ID = @contact_id);
	IF @participant_id is null
	BEGIN
		INSERT INTO [dbo].Participants
		(Contact_ID ,Participant_Type_ID ,Participant_Start_Date,Show_On_Map ,Host_Status_ID,Group_Leader_Status_ID,Domain_ID) VALUES
		(@contact_id,@participant_type_id,@start_date           ,@show_on_map,@host_status  ,@group_leader_status  ,1        );

		SET @participant_id = SCOPE_IDENTITY();

		UPDATE [dbo].Contacts SET Participant_Record = @participant_id WHERE Contact_ID = @contact_id;
	END;
	
	IF @participant_id is not null
	BEGIN
		UPDATE [dbo].Participants
		SET Participant_Type_ID = @participant_type_id,
		Participant_Start_Date = @start_date,		
		Show_On_Map = @show_on_map, Host_Status_ID = @host_status, 
		Group_Leader_Status_ID = @group_leader_status
		WHERE Participant_ID = @participant_id;
	END;
END
GO
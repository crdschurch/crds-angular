USE [MinistryPlatform]
GO
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

-- =============================================
-- Author:      Henney, Sarah
-- Create date: 01/16/2018
-- Description: Creates (if non-existent) or Updates response
-- Output:      @response_id contains the response id, @error_message contains basic error message
-- =============================================


-- Defines cr_QA_Create_Response
IF NOT EXISTS ( SELECT  *
	FROM    sys.objects
	WHERE   object_id = OBJECT_ID(N'cr_QA_Create_Response')
			AND type IN ( N'P', N'PC' ) )
	EXEC('CREATE PROCEDURE dbo.cr_QA_Create_Response
	@participant_email varchar(254),
	@opportunity_id int,
	@response_date datetime,
	@comments nvarchar(255),
	@error_message nvarchar(500) OUTPUT,
	@response_id int OUTPUT AS SET NOCOUNT ON;')
GO
ALTER PROCEDURE [dbo].[cr_QA_Create_Response] 
	@participant_email varchar(254),
	@opportunity_id int,
	@response_date datetime,
	@comments nvarchar(255),
	@error_message nvarchar(500) OUTPUT,
	@response_id int OUTPUT
AS
BEGIN
	SET NOCOUNT ON;
	
	--Enforce required parameters
	IF @participant_email is null OR @opportunity_id is null
	BEGIN
		SET @error_message = 'Participant email and opportunity id cannot be null'+CHAR(13);
		RETURN;
	END;


	--Required fields
	SET @response_date = ISNULL(@response_date, GETDATE());

	DECLARE @closed bit = 0;

	DECLARE @contact_id int;
	SET @contact_id = (SELECT Contact_ID FROM [dbo].dp_Users WHERE User_Name = @participant_email);
	IF @contact_id is null
	BEGIN
		SET @error_message = 'Could not find contact with email '+@participant_email+CHAR(13);
		RETURN;
	END;

	DECLARE @participant_id int;
	SET @participant_id = (SELECT Participant_Record FROM [dbo].Contacts WHERE Contact_ID = @contact_id);
	IF @participant_id is null
	BEGIN
		--Use defaults
		EXEC [dbo].[cr_QA_Create_Participant] @participant_email, null, null, null, null,
		@error_message = @error_message OUTPUT, @participant_id = @participant_id OUTPUT;
		IF @participant_id is null
		BEGIN
			SET @error_message = @error_message+'Could not create participant record for '+@participant_email+CHAR(13);
			RETURN;
		END;
	END


	--Create/Update response
	SET @response_id = (SELECT TOP 1 Response_ID FROM [dbo].Responses WHERE Participant_ID = @participant_id 
	AND Opportunity_ID = @opportunity_id ORDER BY Response_ID ASC);
	IF @response_id is null
	BEGIN
		INSERT INTO [dbo].Responses 
		(Participant_ID ,Opportunity_ID ,Response_Date ,Closed ,Domain_ID) VALUES
		(@participant_id,@opportunity_id,@response_date,@closed,1        );

		SET @response_id = SCOPE_IDENTITY();
	END

	IF @response_id is not null
	BEGIN
		UPDATE [dbo].Responses
		SET	Response_Date = @response_date,
		Closed = @closed,
		Comments = @comments
		WHERE Response_ID = @response_id;
	END;
END
GO
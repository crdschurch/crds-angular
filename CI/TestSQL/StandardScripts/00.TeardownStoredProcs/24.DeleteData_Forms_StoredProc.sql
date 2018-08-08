USE [MinistryPlatform]
GO
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

-- =============================================
-- Author:		Colubiale, AJ
-- Create date: 07/31/2018
-- Description:	Stored procedure declarations for deleting forms
-- =============================================

-- Defines cr_QA_Delete_Form
IF NOT EXISTS ( SELECT  *
	FROM    sys.objects
	WHERE   object_id = OBJECT_ID(N'cr_QA_Delete_Form')
			AND type IN ( N'P', N'PC' ) )
	EXEC('CREATE PROCEDURE dbo.cr_QA_Delete_Form
	@form_id int AS SET NOCOUNT ON;')
GO
ALTER PROCEDURE [dbo].[cr_QA_Delete_Form] 
	@form_id int
AS
BEGIN
	SET NOCOUNT ON;

	IF @form_id is null
		RETURN;

	--Delete foreign key entries that can't be nullified
	DELETE [dbo].cr_Form_Metadata WHERE Form_ID = @form_id;
	DELETE [dbo].cr_Role_Forms WHERE Form_ID = @form_id;

	-- Delete foreign key entries that can't be nullified with cascade
	DELETE FROM [dbo].Form_Response_Answers WHERE Form_Field_ID IN (SELECT Form_Field_ID FROM [dbo].Form_Fields WHERE Form_ID = @form_id);
	DELETE [dbo].Form_Fields WHERE Form_ID = @form_id;

	--Delete foreign key entries that can't be nullified using another stored proc
	--Delete Form Responses
	DECLARE @form_responses_to_delete TABLE
	(
		form_response_id int
	)
	INSERT INTO @form_responses_to_delete (form_response_id) SELECT Form_Response_ID
		FROM [dbo].Form_Responses WHERE Form_ID = @form_id;

	DECLARE @cur_entry_id int = 0;

	WHILE @cur_entry_id is not null
	BEGIN
		--Get top item in list
		Set @cur_entry_id = (SELECT TOP 1 form_response_id 
			FROM @form_responses_to_delete
			WHERE form_response_id > @cur_entry_id
			ORDER BY form_response_id ASC);

		--Delete using the stored proc
		IF @cur_entry_id is not null
		BEGIN
			EXEC [dbo].[cr_QA_Delete_Form_Response] @cur_entry_id;
		END
	END

	--Nullify foreign keys
	UPDATE [dbo].Events SET Registration_Form = null WHERE Registration_Form = @form_id;
	UPDATE [dbo].Opportunities SET Custom_Form = null WHERE Custom_Form = @form_id;
	UPDATE [dbo].Pledge_Campaigns SET Registration_Form = null WHERE Registration_Form = @form_id;
	
	DELETE [dbo].Forms WHERE Form_ID = @form_id;
END



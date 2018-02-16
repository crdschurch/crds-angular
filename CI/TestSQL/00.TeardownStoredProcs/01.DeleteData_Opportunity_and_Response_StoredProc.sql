USE [MinistryPlatform]
GO
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

-- =============================================
-- Author:		Henney, Sarah
-- Create date: 01/08/2018
-- Description:	Stored procedure declarations for deleting opportunity and response data
-- =============================================

-- Defines cr_QA_Delete_Response
IF NOT EXISTS ( SELECT  *
	FROM    sys.objects
	WHERE   object_id = OBJECT_ID(N'cr_QA_Delete_Response')
			AND type IN ( N'P', N'PC' ) )
	EXEC('CREATE PROCEDURE dbo.cr_QA_Delete_Response
	@response_id int AS SET NOCOUNT ON;')
GO
ALTER PROCEDURE [dbo].[cr_QA_Delete_Response] 
	@response_id int
AS
BEGIN
	SET NOCOUNT ON;

	IF @response_id is null
		RETURN;
	
	--Delete foreign key entries that can't be nullified
	DELETE [dbo].Response_Attributes WHERE Response_ID = @response_id;
	DELETE [dbo].Response_Follow_Ups WHERE Response_ID = @response_id;

	--Nullify foreign keys
	UPDATE [dbo].Event_Participants SET Response_ID = null WHERE Response_ID = @response_id;
	UPDATE [dbo].Form_Response_Answers SET Opportunity_Response = null WHERE Opportunity_Response = @response_id;
	UPDATE [dbo].Form_Responses SET Opportunity_Response = null WHERE Opportunity_Response = @response_id;
	
	DELETE [dbo].Responses WHERE Response_ID = @response_id;
END
GO


-- Defines cr_QA_Delete_Opportunity
IF NOT EXISTS ( SELECT  *
	FROM    sys.objects
	WHERE   object_id = OBJECT_ID(N'cr_QA_Delete_Opportunity')
			AND type IN ( N'P', N'PC' ) )
	EXEC('CREATE PROCEDURE dbo.cr_QA_Delete_Opportunity
	@opportunity_id int,
	@opportunity_name nvarchar(50) AS SET NOCOUNT ON;')
GO
ALTER PROCEDURE [dbo].[cr_QA_Delete_Opportunity]
	@opportunity_id int,
	@opportunity_name nvarchar(50)
AS
BEGIN
	SET NOCOUNT ON;

	IF @opportunity_id is null AND @opportunity_name is not null
		SET @opportunity_id = (SELECT TOP 1 Opportunity_ID FROM [dbo].Opportunities WHERE Opportunity_Title = @opportunity_name ORDER BY Opportunity_ID ASC);

	IF @opportunity_id is null
		RETURN;
	
	--Delete foreign key entries that can't be nullified
	DELETE [dbo].Opportunity_Attributes WHERE Opportunity_ID = @opportunity_id;

	--Delete foreign key entries that can't be nullified using another stored proc
	--Delete Responses
	DECLARE @responses_to_delete TABLE
	(
		response_id int
	)
	INSERT INTO @responses_to_delete (response_id) SELECT Response_ID 
		FROM [dbo].Responses WHERE Opportunity_ID = @opportunity_id;

	DECLARE @cur_entry_id int = 0;

	WHILE @cur_entry_id is not null
	BEGIN
		--Get top item in list
		Set @cur_entry_id = (SELECT TOP 1 response_id 
			FROM @responses_to_delete
			WHERE response_id > @cur_entry_id
			ORDER BY response_id ASC);

		--Delete using the stored proc
		IF @cur_entry_id is not null
		BEGIN
			EXEC [dbo].[cr_QA_Delete_Response] @cur_entry_id;
		END
	END

	--Nullify foreign keys
	UPDATE [dbo].Event_Participants SET Opportunity_ID = null WHERE Opportunity_ID = @opportunity_id;
	UPDATE [dbo].Form_Responses SET Opportunity_ID = null WHERE Opportunity_ID = @opportunity_id;
	
	DELETE [dbo].Opportunities WHERE Opportunity_ID = @opportunity_id;
END
GO
USE [MinistryPlatform]
GO
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

-- =============================================
-- Author:		Henney, Sarah
-- Create date: 02/16/2018
-- Description:	Stored procedure declarations for deleting pledge and pledge campaign data
-- =============================================

-- Defines cr_QA_Delete_Pledge
IF NOT EXISTS ( SELECT  *
	FROM    sys.objects
	WHERE   object_id = OBJECT_ID(N'cr_QA_Delete_Pledge')
			AND type IN ( N'P', N'PC' ) )
	EXEC('CREATE PROCEDURE dbo.cr_QA_Delete_Pledge @pledge_id int AS SET NOCOUNT ON;')
GO
ALTER PROCEDURE [dbo].[cr_QA_Delete_Pledge] 
	@pledge_id int
AS
BEGIN
	SET NOCOUNT ON;

	IF @pledge_id is null
		RETURN;

	--Nullify foreign keys
	UPDATE [dbo].Donation_Distributions SET Pledge_ID = null WHERE Pledge_ID = @pledge_id;
	UPDATE [dbo].Form_Response_Answers SET Pledge_ID = null WHERE Pledge_ID = @pledge_id;
	UPDATE [dbo].Scheduled_Donations SET Pledge_ID = null WHERE Pledge_ID = @pledge_id;

	DELETE [dbo].Pledges WHERE Pledge_ID = @pledge_id;
END
GO


-- Defines cr_QA_Delete_Pledge_Campaign
IF NOT EXISTS ( SELECT  *
	FROM    sys.objects
	WHERE   object_id = OBJECT_ID(N'cr_QA_Delete_Pledge_Campaign')
			AND type IN ( N'P', N'PC' ) )
	EXEC('CREATE PROCEDURE dbo.cr_QA_Delete_Pledge_Campaign
	@pledge_campaign_id int,
	@pledge_campaign_name nvarchar(50) AS SET NOCOUNT ON;')
GO
ALTER PROCEDURE [dbo].[cr_QA_Delete_Pledge_Campaign] 
	@pledge_campaign_id int,
	@pledge_campaign_name nvarchar(50)
AS
BEGIN
	SET NOCOUNT ON;

	IF @pledge_campaign_id is null AND @pledge_campaign_name is not null
		SET @pledge_campaign_id = (SELECT TOP 1 Pledge_Campaign_ID FROM [dbo].Pledge_Campaigns WHERE Campaign_Name = @pledge_campaign_name ORDER BY Pledge_Campaign_ID ASC);

	IF @pledge_campaign_id is null
		RETURN;

	--Delete foreign key entries that can't be nullified
	DELETE [dbo].cr_Campaign_Age_Exception WHERE Pledge_Campaign_ID = @pledge_campaign_id;
	DELETE [dbo].cr_Campaign_Private_Invitation WHERE Pledge_Campaign_ID = @pledge_campaign_id;
	
	--Delete foreign key entries that can't be nullified using another stored proc
	--Delete Pledges
	DECLARE @pledges_to_delete TABLE
	(
		pledge_id int
	)
	INSERT INTO @pledges_to_delete (pledge_id) SELECT Pledge_ID 
		FROM [dbo].Pledges WHERE Pledge_Campaign_ID = @pledge_campaign_id;

	DECLARE @cur_entry_id int = 0;

	WHILE @cur_entry_id is not null
	BEGIN
		--Get top item in list
		SET @cur_entry_id = (SELECT TOP 1 pledge_id 
			FROM @pledges_to_delete
			WHERE pledge_id > @cur_entry_id
			ORDER BY pledge_id ASC);

		--Delete using the stored proc
		IF @cur_entry_id is not null
		BEGIN
			EXEC [dbo].[cr_QA_Delete_Pledge] @cur_entry_id;
		END
	END

	--Nullify foreign keys
	UPDATE [dbo].Accounting_Companies SET Pledge_Campaign_ID = null WHERE Pledge_Campaign_ID = @pledge_campaign_id;
	UPDATE [dbo].Form_Responses SET Pledge_Campaign_ID = null WHERE Pledge_Campaign_ID = @pledge_campaign_id;
	UPDATE [dbo].Programs SET Pledge_Campaign_ID = null WHERE Pledge_Campaign_ID = @pledge_campaign_id;
	
	DELETE [dbo].pledge_campaigns WHERE pledge_campaign_ID = @pledge_campaign_id;
END
GO
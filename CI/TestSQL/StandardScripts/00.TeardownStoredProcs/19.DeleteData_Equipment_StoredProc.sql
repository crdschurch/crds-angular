USE [MinistryPlatform]
GO
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

-- =============================================
-- Author:		Henney, Sarah
-- Create date:	07/05/2018
-- Description:	Stored procedure declaration for deleting equipment data
-- =============================================

-- Defines cr_QA_Delete_Equipment
IF NOT EXISTS ( SELECT  *
	FROM    sys.objects
	WHERE   object_id = OBJECT_ID(N'cr_QA_Delete_Equipment')
			AND type IN ( N'P', N'PC' ) )
	EXEC('CREATE PROCEDURE dbo.cr_QA_Delete_Equipment
	@equipment_id int AS SET NOCOUNT ON;')
GO
ALTER PROCEDURE [dbo].[cr_QA_Delete_Equipment] 
	@equipment_id int
AS
BEGIN
	SET NOCOUNT ON;

	IF @equipment_id is null
		RETURN;
	
	--Delete foreign key entries that can't be nullified
	DELETE [dbo].Event_Equipment WHERE Equipment_ID = @equipment_id;
	
	DELETE [dbo].Equipment WHERE Equipment_ID = @equipment_id;
END
GO
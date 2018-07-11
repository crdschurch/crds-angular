USE [MinistryPlatform]
GO
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

-- =============================================
-- Author:		Henney, Sarah
-- Create date:	07/05/2018
-- Description:	Stored procedure declaration for deleting registration data
-- =============================================

-- Defines cr_QA_Delete_Registration
IF NOT EXISTS ( SELECT  *
	FROM    sys.objects
	WHERE   object_id = OBJECT_ID(N'cr_QA_Delete_Registration')
			AND type IN ( N'P', N'PC' ) )
	EXEC('CREATE PROCEDURE dbo.cr_QA_Delete_Registration
	@registration_id int AS SET NOCOUNT ON;')
GO
ALTER PROCEDURE [dbo].[cr_QA_Delete_Registration] 
	@registration_id int
AS
BEGIN
	SET NOCOUNT ON;

	IF @registration_id is null
		RETURN;
	
	--Delete foreign key entries that can't be nullified	
	DELETE [dbo].cr_Registration_Children_Attributes WHERE Registration_ID = @registration_id;
	DELETE [dbo].cr_Registration_Equipment_Attributes WHERE Registration_ID = @registration_id;
	DELETE [dbo].cr_Registration_PrepWork_Attributes WHERE Registration_ID = @registration_id;
	DELETE [dbo].cr_Registration_Project_Type WHERE Registration_ID = @registration_id;

	--Delete Group Connectors & Registrations
	DECLARE @group_connectors_to_delete TABLE
	(
		group_connector_id int
	)
	INSERT INTO @group_connectors_to_delete(group_connector_id) SELECT Group_Connector_ID 
		FROM [dbo].cr_Group_Connectors WHERE Primary_Registration = @registration_id;
		
	DELETE [dbo].cr_Group_Connector_Registrations WHERE Registration_ID = @registration_id
		OR Group_Connector_ID IN (SELECT group_connector_id FROM @group_connectors_to_delete);
	DELETE [dbo].cr_Group_Connectors WHERE Primary_Registration = @registration_id;
	
		
	DELETE [dbo].cr_Registrations WHERE Registration_ID = @registration_id;
END
GO
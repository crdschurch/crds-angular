USE [MinistryPlatform]
GO
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

-- =============================================
-- Author:		Henney, Sarah
-- Create date:	07/03/2018
-- Description:	Stored procedure declaration for deleting location data
-- =============================================

-- Defines cr_QA_Delete_Location
IF NOT EXISTS ( SELECT  *
	FROM    sys.objects
	WHERE   object_id = OBJECT_ID(N'cr_QA_Delete_Location')
			AND type IN ( N'P', N'PC' ) )
	EXEC('CREATE PROCEDURE dbo.cr_QA_Delete_Location
	@location_id int AS SET NOCOUNT ON;')
GO
ALTER PROCEDURE [dbo].[cr_QA_Delete_Location] 
	@location_id int
AS
BEGIN
	SET NOCOUNT ON;

	IF @location_id is null
		RETURN;
	
	--Delete foreign key entries that can't be nullified
	DELETE [dbo].cr_Kiosk_Configs WHERE Location_ID = @location_id;
	DELETE [dbo].cr_Organization_Locations WHERE Location_ID = @location_id;

	--Delete Projects
	DECLARE @projects_to_delete TABLE
	(
		project_id int
	)
	INSERT INTO @projects_to_delete(project_id) SELECT Project_ID 
		FROM [dbo].cr_projects WHERE Location_ID = @location_id;

	UPDATE [dbo].cr_Group_Connectors SET Project_ID = null 
		WHERE Project_ID in (SELECT project_id FROM @projects_to_delete);
	DELETE [dbo].cr_Projects WHERE Project_ID in (SELECT project_id FROM @projects_to_delete);

	--Delete Buildings
	DECLARE @building_to_delete TABLE
	(
		building_id int
	)
	INSERT INTO @building_to_delete (building_id) SELECT Building_ID
		FROM [dbo].Buildings WHERE location_id = @location_id;

	DECLARE @cur_entry_id int = 0;

	WHILE @cur_entry_id is not null
	BEGIN
		--Get top item in list
		Set @cur_entry_id = (SELECT TOP 1 building_id 
			FROM @building_to_delete
			WHERE building_id > @cur_entry_id
			ORDER BY building_id ASC);

		--Delete using the stored proc
		IF @cur_entry_id is not null
		BEGIN
			EXEC [dbo].[cr_QA_Delete_Building] @cur_entry_id;
		END
	END

	--Delete Registrations 
	DECLARE @registration_to_delete TABLE
	(
		registration_id int
	)
	INSERT INTO @registration_to_delete (registration_id) SELECT Registration_ID 
		FROM [dbo].cr_Registrations WHERE Preferred_Launch_Site_ID = @location_id;

	SET @cur_entry_id = 0;

	WHILE @cur_entry_id is not null
	BEGIN
		--Get top item in list
		Set @cur_entry_id = (SELECT TOP 1 registration_id 
			FROM @registration_to_delete
			WHERE registration_id > @cur_entry_id
			ORDER BY registration_id ASC);

		--Delete using the stored proc
		IF @cur_entry_id is not null
		BEGIN
			EXEC [dbo].[cr_QA_Delete_Registration] @cur_entry_id;
		END
	END

	--Nullify foreign keys
	UPDATE [dbo].Care_Cases SET Location_ID = null WHERE Location_ID = @location_id;
	UPDATE [dbo].Congregations SET Location_ID = null WHERE Location_ID = @location_id;
	UPDATE [dbo].Events SET Location_ID = null WHERE Location_ID = @location_id;
	
	DELETE [dbo].Locations WHERE Location_ID = @location_id;
END
GO
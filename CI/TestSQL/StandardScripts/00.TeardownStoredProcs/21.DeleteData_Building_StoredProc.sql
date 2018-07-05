USE [MinistryPlatform]
GO
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

-- =============================================
-- Author:		Henney, Sarah
-- Create date:	07/03/2018
-- Description:	Stored procedure declaration for deleting building data
-- =============================================

-- Defines cr_QA_Delete_Building
IF NOT EXISTS ( SELECT  *
	FROM    sys.objects
	WHERE   object_id = OBJECT_ID(N'cr_QA_Delete_Building')
			AND type IN ( N'P', N'PC' ) )
	EXEC('CREATE PROCEDURE dbo.cr_QA_Delete_Building
	@building_id int AS SET NOCOUNT ON;')
GO
ALTER PROCEDURE [dbo].[cr_QA_Delete_Building] 
	@building_id int
AS
BEGIN
	SET NOCOUNT ON;

	IF @building_id is null
		RETURN;
	
	--Delete foreign key entries that can't be nullified
	--Delete Rooms
	DECLARE @rooms_to_delete TABLE
	(
		room_id int
	)
	INSERT INTO @rooms_to_delete (room_id) SELECT Room_ID 
		FROM [dbo].Rooms WHERE Building_ID = @building_id;

	DECLARE @cur_entry_id int = 0;

	WHILE @cur_entry_id is not null
	BEGIN
		--Get top item in list
		Set @cur_entry_id = (SELECT TOP 1 room_id 
			FROM @rooms_to_delete
			WHERE room_id > @cur_entry_id
			ORDER BY room_id ASC);

		--Delete using the stored proc
		IF @cur_entry_id is not null
		BEGIN
			EXEC [dbo].[cr_QA_Delete_Room] @cur_entry_id;
		END
	END
	
	DELETE [dbo].Buildings WHERE Building_ID = @building_id;
END
GO
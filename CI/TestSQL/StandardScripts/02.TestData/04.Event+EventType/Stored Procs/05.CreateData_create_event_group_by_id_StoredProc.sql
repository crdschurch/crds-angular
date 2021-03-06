USE [MinistryPlatform]
GO
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

-- =============================================
-- Author:      Andrews, Chris
-- Create date: 2/9/2019
-- Description: Creates and event group using event and group Id
-- Output:      @group_id contains the group id, @error_message contains basic error message
-- =============================================


-- Defines cr_QA_Create_Event_Group_By_Id
IF NOT EXISTS ( SELECT  *
	FROM    sys.objects
	WHERE   object_id = OBJECT_ID(N'cr_QA_Create_Event_Group_By_Id')
			AND type IN ( N'P', N'PC' ) )
	EXEC('CREATE PROCEDURE dbo.cr_QA_Create_Event_Group_By_Id
	@group_id int,
	@event_id int,
	@error_message nvarchar(500) OUTPUT,
	@event_group_id int OUTPUT AS SET NOCOUNT ON;')
GO
ALTER PROCEDURE [dbo].[cr_QA_Create_Event_Group_By_Id] 
	@group_id int,
	@event_id int,
	@error_message nvarchar(500) OUTPUT,
	@event_group_id int OUTPUT
AS
BEGIN
	SET NOCOUNT ON;
	
	--Enforce required parameters
	IF @event_id is null
	BEGIN
		SET @error_message = 'Event id cannot be null'+CHAR(13);
		RETURN;
	END;


	--Required fields
	IF @group_id is null
	BEGIN
		SET @error_message = 'Group id cannot be null'+CHAR(13);
		RETURN;
	END;
			
	
	--Create event group
	SET @event_group_id = (SELECT TOP 1 Event_Group_ID FROM [dbo].Event_Groups WHERE Event_ID = @event_id AND Group_ID = @group_id ORDER BY Event_Group_ID ASC);
	IF @event_group_id is null
	BEGIN
		INSERT INTO [dbo].Event_Groups
		(Event_ID ,Group_ID ,Domain_ID) VALUES
		(@event_id,@group_id,1        );

		SET @event_group_id = SCOPE_IDENTITY();
	END;
END
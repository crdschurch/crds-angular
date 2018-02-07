USE [MinistryPlatform]
GO
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

-- =============================================
-- Author:		Henney, Sarah
-- Create date: 02/07/2018
-- Description:	Creates (if nonexistent) an event group
-- Output:      @event_group_id contains the event group id, @error_message contains basic error message
-- =============================================


-- Defines cr_QA_Create_Event_Group
IF NOT EXISTS ( SELECT  *
	FROM    sys.objects
	WHERE   object_id = OBJECT_ID(N'cr_QA_Create_Event_Group')
			AND type IN ( N'P', N'PC' ) )
	EXEC('CREATE PROCEDURE dbo.cr_QA_Create_Event_Group
	@group_name nvarchar(75),
	@event_id int,
	@error_message nvarchar(500) OUTPUT,
	@event_group_id int OUTPUT AS SET NOCOUNT ON;')
GO
ALTER PROCEDURE [dbo].[cr_QA_Create_Event_Group]
	@group_name nvarchar(75),
	@event_id int,
	@error_message nvarchar(500) OUTPUT,
	@event_group_id int OUTPUT
AS
BEGIN
	SET NOCOUNT ON;
	
	--Enforce required parameters
	IF @group_name is null OR @event_id is null
	BEGIN
		SET @error_message = 'Group name and event id cannot be null'+CHAR(13);
		RETURN;
	END;


	--Required fields
	DECLARE @group_id int;
	SET @group_id = (SELECT TOP 1 Group_ID FROM [dbo].Groups WHERE Group_Name = @group_name ORDER BY Group_ID ASC);
	IF @group_id is null
	BEGIN
		SET @error_message = 'Could not find group with name '+@group_name+CHAR(13);
		RETURN;
	END;
			
	
	--Create event group if non-existent
	SET @event_group_id = (SELECT TOP 1 Event_Group_ID FROM [dbo].Event_Groups WHERE Event_ID = @event_id AND Group_ID = @group_id ORDER BY Event_Group_ID ASC);
	IF @event_group_id is null
	BEGIN
		INSERT INTO [dbo].Event_Groups
		(Event_ID ,Group_ID ,Domain_ID) VALUES
		(@event_id,@group_id,1        );

		SET @event_group_id = SCOPE_IDENTITY();
	END;
END
GO
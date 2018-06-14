USE [MinistryPlatform]
GO
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

-- =============================================
-- Author:      Henney, Sarah
-- Create date: 06/14/2018
-- Description: Add child event to parent event
-- Output:      @parent_event_id contains the parent event id, @child_event_id contains the child event id, @error_message contains basic error message
-- =============================================


-- Defines cr_QA_Add_Child_Event
IF NOT EXISTS ( SELECT  *
	FROM    sys.objects
	WHERE   object_id = OBJECT_ID(N'cr_QA_Add_Child_Event')
			AND type IN ( N'P', N'PC' ) )
	EXEC('CREATE PROCEDURE dbo.cr_QA_Add_Child_Event
	@parent_event_name nvarchar(75),
	@child_event_name nvarchar(75),
	@error_message nvarchar(500) OUTPUT,
	@parent_event_id int OUTPUT,
	@child_event_id int OUTPUT AS SET NOCOUNT ON;')
GO
ALTER PROCEDURE [dbo].[cr_QA_Add_Child_Event] 
	@parent_event_name nvarchar(75),
	@child_event_name nvarchar(75),
	@error_message nvarchar(500) OUTPUT,
	@parent_event_id int OUTPUT,
	@child_event_id int OUTPUT
AS
BEGIN
	SET NOCOUNT ON;
	
	--Enforce required parameters
	IF @parent_event_name is null OR @child_event_name is null
		RETURN;


	--Required fields
	SET @parent_event_id = (SELECT TOP 1 Event_ID FROM [dbo].Events WHERE Event_Title = @parent_event_name ORDER BY Event_ID ASC);
	IF @parent_event_id is null
	BEGIN
		SET @error_message = 'Could not find event with name '+@parent_event_name+CHAR(13);
		RETURN;
	END;

	SET @child_event_id = (SELECT TOP 1 Event_ID FROM [dbo].Events WHERE Event_Title = @child_event_name ORDER BY Event_ID ASC);
	IF @child_event_id is null
	BEGIN
		SET @error_message = 'Could not find event with name '+@child_event_name+CHAR(13);
		RETURN;
	END;

	
	--Add child to parent event
	UPDATE [dbo].Events
	SET Parent_Event_ID = @parent_event_id
	WHERE Event_ID = @child_event_id;
END
GO
USE [MinistryPlatform]
GO
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

-- =============================================
-- Author:      Henney, Sarah
-- Create date: 02/05/2018
-- Description: Creates (if nonexistent) or Updates event type
-- Output:      @event_type_id contains the event type id, @error_message contains basic error message
-- =============================================


-- Defines cr_QA_Create_Event_Type
IF NOT EXISTS ( SELECT  *
	FROM    sys.objects
	WHERE   object_id = OBJECT_ID(N'cr_QA_Create_Event_Type')
			AND type IN ( N'P', N'PC' ) )
	EXEC('CREATE PROCEDURE dbo.cr_QA_Create_Event_Type
	@event_type_name nvarchar(50),
	@allow_multiday_event bit,
	@error_message nvarchar(500) OUTPUT,
	@event_type_id int OUTPUT AS SET NOCOUNT ON;')
GO
ALTER PROCEDURE [dbo].[cr_QA_Create_Event_Type]
	@event_type_name nvarchar(50),
	@allow_multiday_event bit,
	@error_message nvarchar(500) OUTPUT,
	@event_type_id int OUTPUT
AS
BEGIN
	SET NOCOUNT ON;
	
	--Enforce required parameters
	IF @event_type_name is null
	BEGIN
		SET @error_message = 'Event type name cannot be null'+CHAR(13);
		RETURN;
	END;


	--Required fields
	SET @allow_multiday_event = ISNULL(@allow_multiday_event, 0);
	
	DECLARE @show_on_event_tool bit = 1;
	DECLARE @show_on_mobile bit = 1;


	--Optional fields
	DECLARE @description nvarchar(50) = null;


	--Create/Update event type
	SET @event_type_id = (SELECT TOP 1 Event_Type_ID FROM Event_Types WHERE Event_Type = @event_type_name ORDER BY Event_Type_ID ASC);
	IF @event_type_id is null
	BEGIN
		INSERT INTO [dbo].Event_Types 
		(Event_Type      ,Allow_Multiday_Event ,Show_On_Event_Tool ,Show_On_MPMobile,Domain_ID) VALUES
		(@event_type_name,@allow_multiday_event,@show_on_event_tool,@show_on_mobile ,1        );

		SET @event_type_id = SCOPE_IDENTITY();
	END;
	
	IF @event_type_id is not null
	BEGIN
		UPDATE [dbo].Event_Types
		SET Allow_Multiday_Event = @allow_multiday_event,
		Show_On_Event_Tool = @show_on_event_tool,
		Show_On_MPMobile = @show_on_mobile,
		Description = @description
		WHERE Event_Type_ID = @event_type_id;
	END;
END
GO
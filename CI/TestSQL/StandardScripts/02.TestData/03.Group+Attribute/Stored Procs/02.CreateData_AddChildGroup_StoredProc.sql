USE [MinistryPlatform]
GO
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

-- =============================================
-- Author:      Henney, Sarah
-- Create date: 01/24/2018
-- Description: Add child group to parent group
-- Output:      @parent_group_id contains the parent group id, @child_group_id contains the child group id, @error_message contains basic error message
-- =============================================


-- Defines cr_QA_Add_Child_Group
IF NOT EXISTS ( SELECT  *
	FROM    sys.objects
	WHERE   object_id = OBJECT_ID(N'cr_QA_Add_Child_Group')
			AND type IN ( N'P', N'PC' ) )
	EXEC('CREATE PROCEDURE dbo.cr_QA_Add_Child_Group
	@parent_group_name nvarchar(75),
	@child_group_name nvarchar(75),
	@error_message nvarchar(500) OUTPUT,
	@parent_group_id int OUTPUT,
	@child_group_id int OUTPUT AS SET NOCOUNT ON;')
GO
ALTER PROCEDURE [dbo].[cr_QA_Add_Child_Group] 
	@parent_group_name nvarchar(75),
	@child_group_name nvarchar(75),
	@error_message nvarchar(500) OUTPUT,
	@parent_group_id int OUTPUT,
	@child_group_id int OUTPUT
AS
BEGIN
	SET NOCOUNT ON;
	
	--Required fields
	IF @parent_group_name is null
	BEGIN
		SET @error_message = 'Parent group name cannot be null'+CHAR(13);
		RETURN;
	END;
	SET @parent_group_id = (SELECT TOP 1 Group_ID FROM [dbo].Groups WHERE Group_Name = @parent_group_name ORDER BY Group_ID ASC);
	IF @parent_group_id is null
	BEGIN
		SET @error_message = 'Could not find group with name '+@parent_group_name+CHAR(13);
		RETURN;
	END;

	IF @child_group_name is null
	BEGIN
		SET @error_message = 'Child group name cannot be null'+CHAR(13);
		RETURN;
	END;
	SET @child_group_id = (SELECT TOP 1 Group_ID FROM [dbo].Groups WHERE Group_Name = @child_group_name ORDER BY Group_ID ASC);
	IF @child_group_id is null
	BEGIN
		SET @error_message = 'Could not find group with name '+@child_group_name+CHAR(13);
		RETURN;
	END;

	
	--Add child to parent group
	UPDATE [dbo].Groups
	SET Parent_Group = @parent_group_id
	WHERE Group_ID = @child_group_id;
END
GO
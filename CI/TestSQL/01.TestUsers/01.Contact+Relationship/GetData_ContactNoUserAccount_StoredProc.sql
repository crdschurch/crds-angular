USE [MinistryPlatform]
GO
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

-- =============================================
-- Author:      Henney, Sarah
-- Create date: 02/01/2018
-- Description: Finds a contact without a user account by their display name and email (optional)
-- Output:      @contact_id contains the contact id, @error_message contains basic error message
-- =============================================

-- Defines cr_QA_Get_Contact_No_User_Acount
IF NOT EXISTS ( SELECT  *
	FROM    sys.objects
	WHERE   object_id = OBJECT_ID(N'cr_QA_Get_Contact_No_User_Acount')
			AND type IN ( N'P', N'PC' ) )
	EXEC('CREATE PROCEDURE dbo.cr_QA_Get_Contact_No_User_Acount
	@display_name nvarchar(75),
	@contact_email nvarchar(255),
	@error_message nvarchar(500) OUTPUT,
	@contact_id int OUTPUT AS SET NOCOUNT ON;')
GO
ALTER PROCEDURE [dbo].[cr_QA_Get_Contact_No_User_Acount]
	@display_name nvarchar(75),
	@contact_email nvarchar(255),
	@error_message nvarchar(500) OUTPUT,
	@contact_id int OUTPUT
AS
BEGIN
	SET NOCOUNT ON;
	
	--Enforce required parameters
	IF @display_name is null
	BEGIN
		SET @error_message = 'Display name cannot be null'+CHAR(13);
		RETURN;
	END;

	--Find by both criteria
	IF @contact_email is not null
		SET @contact_id = (SELECT TOP 1 Contact_ID FROM [dbo].Contacts WHERE Display_Name = @display_name AND Email_Address = @contact_email AND User_Account is null ORDER BY Contact_ID ASC);
	
	--Find by display name only
	IF @contact_id is null
		SET @contact_id = (SELECT TOP 1 Contact_ID FROM [dbo].Contacts WHERE Display_Name = @display_name AND User_Account is null ORDER BY Contact_ID ASC);
	
	IF @contact_id is null
		SET @error_message = 'Could not find contact '+@display_name+' '+@contact_email+CHAR(13);	
END
GO
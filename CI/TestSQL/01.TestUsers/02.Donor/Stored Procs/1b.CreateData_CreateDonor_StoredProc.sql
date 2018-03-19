USE [MinistryPlatform]
GO
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

-- =============================================
-- Author:      Henney, Sarah
-- Create date: 01/15/2018
-- Description: Create (if nonexistent) or Update Donor record
-- Output:      @donor_id contains the donor id, @error_message contains basic error message
-- =============================================


-- Defines cr_QA_Create_Donor
IF NOT EXISTS ( SELECT  *
	FROM    sys.objects
	WHERE   object_id = OBJECT_ID(N'cr_QA_Create_Donor')
			AND type IN ( N'P', N'PC' ) )
	EXEC('CREATE PROCEDURE dbo.cr_QA_Create_Donor
	@donor_email varchar(254),
	@setup_date datetime,
	@statement_type_id int,
	@statement_frequency_id int,
	@statement_method_id int,
	@processor_id nvarchar(255),
	@error_message nvarchar(500) OUTPUT,
	@donor_id int OUTPUT AS SET NOCOUNT ON;')
GO
ALTER PROCEDURE [dbo].[cr_QA_Create_Donor] 
	@donor_email varchar(254),
	@setup_date datetime,
	@statement_type_id int,
	@statement_frequency_id int,
	@statement_method_id int,
	@processor_id nvarchar(255),
	@error_message nvarchar(500) OUTPUT,
	@donor_id int OUTPUT
AS
BEGIN
	SET NOCOUNT ON;
	
	--Enforce required parameters
	IF @donor_email is null
	BEGIN
		SET @error_message = 'Donor email cannot be null'+CHAR(13);
		RETURN;
	END;


	--Required fields
	DECLARE @contact_id int = (SELECT Contact_ID FROM [dbo].dp_Users WHERE User_Name = @donor_email);
	IF @contact_id is null
	BEGIN
		SET @error_message = 'Could not find contact with email '+@donor_email+CHAR(13);
		RETURN;
	END;

	EXEC [dbo].[cr_QA_Create_Donor_By_Contact_Id] @contact_id, @setup_date, @statement_type_id, @statement_frequency_id, @statement_method_id, @processor_id,
	@error_message = @error_message OUTPUT, @donor_id = @donor_id OUTPUT;
END
GO
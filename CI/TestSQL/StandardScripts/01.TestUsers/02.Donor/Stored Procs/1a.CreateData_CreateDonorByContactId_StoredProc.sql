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


-- Defines cr_QA_Create_Donor_By_Contact_Id
IF NOT EXISTS ( SELECT  *
	FROM    sys.objects
	WHERE   object_id = OBJECT_ID(N'cr_QA_Create_Donor_By_Contact_Id')
			AND type IN ( N'P', N'PC' ) )
	EXEC('CREATE PROCEDURE dbo.cr_QA_Create_Donor_By_Contact_Id
	@contact_id int,
	@setup_date datetime,
	@statement_type_id int,
	@statement_frequency_id int,
	@statement_method_id int,
	@processor_id nvarchar(255),
	@error_message nvarchar(500) OUTPUT,
	@donor_id int OUTPUT AS SET NOCOUNT ON;')
GO
ALTER PROCEDURE [dbo].[cr_QA_Create_Donor_By_Contact_Id] 
	@contact_id int,
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
	
	--Required fields
	SET @statement_type_id = ISNULL(@statement_type_id, (SELECT Value FROM dp_configuration_Settings WHERE Configuration_Setting_ID = 61)); --Use configured default (Individual)
	SET @setup_date = ISNULL(@setup_date, GETDATE()); --Today
	SET @statement_frequency_id = ISNULL(@statement_frequency_id, (SELECT Value FROM dp_configuration_Settings WHERE Configuration_Setting_ID = 60)); --Use configured default (Quarterly)
	SET @statement_method_id = ISNULL(@statement_method_id, 2); --Email/Online

	DECLARE @cancel_envelopes bit = 0;
	
	IF @contact_id is null
	BEGIN
		SET @error_message = 'Contact id cannot be null'+CHAR(13);
		RETURN;
	END;
	DECLARE @contact_count int = (SELECT count(Contact_ID) FROM [dbo].Contacts WHERE Contact_ID = @contact_id);
	IF @contact_count = 0
	BEGIN
		SET @error_message = 'Contact with id '+@contact_id+' does not exist'+CHAR(13);
		RETURN;
	END;


	--Optional fields
	DECLARE @notes nvarchar(500) = 'Scripted Donor'; --Please do not change - used by automation scripts

	
	--Create/Update donor
	SET @donor_id = (SELECT Donor_Record FROM [dbo].Contacts WHERE Contact_ID = @contact_id);	
	IF @donor_id is null
	BEGIN
		INSERT INTO [dbo].Donors
		(Contact_ID ,Statement_Frequency_ID ,Statement_Type_ID ,Statement_Method_ID ,Setup_Date ,Cancel_Envelopes ,Domain_ID) VALUES
		(@contact_id,@statement_frequency_id,@statement_type_id,@statement_method_id,@setup_date,@cancel_envelopes,1        );

		SET @donor_id = SCOPE_IDENTITY();

		UPDATE [dbo].Contacts SET Donor_Record = @donor_id WHERE Contact_ID = @contact_id;
	END;
	
	IF @donor_id is not null
	BEGIN
		UPDATE [dbo].Donors
		SET Statement_Frequency_ID = @statement_frequency_id,
		Statement_Type_ID = @statement_type_id,
		Statement_Method_ID = @statement_method_id,
		Setup_Date = @setup_date,
		Cancel_Envelopes = @cancel_envelopes,
		Processor_ID = @processor_id,
		Notes = @notes
		WHERE Donor_ID = @donor_id;
	END;
END
GO
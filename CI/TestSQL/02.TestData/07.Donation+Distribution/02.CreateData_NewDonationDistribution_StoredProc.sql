SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

-- =============================================
-- Author:      Henney, Sarah
-- Create date: 02/02/2018
-- Description: Creates new donation distribution with given information
-- Output:      @distribution_id contains the donation distribution id, @error_message contains basic error message
-- =============================================


-- Defines cr_QA_New_Donation_Distribution
IF NOT EXISTS ( SELECT  *
	FROM    sys.objects
	WHERE   object_id = OBJECT_ID(N'cr_QA_New_Donation_Distribution')
			AND type IN ( N'P', N'PC' ) )
	EXEC('CREATE PROCEDURE dbo.cr_QA_New_Donation_Distribution 
	@donation_id int,
	@distribution_amount money,
	@program_name nvarchar(130),
	@pledge_donor_email nvarchar(254),
	@soft_credit_donor_email nvarchar(254),
	@congregation_id int,
	@notes nvarchar(1000),
	@error_message nvarchar(500) OUTPUT,
	@distribution_id int OUTPUT AS SET NOCOUNT ON;')
GO
ALTER PROCEDURE [dbo].[cr_QA_New_Donation_Distribution] 
	@donation_id int,
	@distribution_amount money,
	@program_name nvarchar(130),
	@pledge_donor_email nvarchar(254),
	@soft_credit_donor_email nvarchar(254),
	@congregation_id int,
	@notes nvarchar(1000),
	@error_message nvarchar(500) OUTPUT,
	@distribution_id int OUTPUT
AS
BEGIN
	SET NOCOUNT ON;

	--Enforce required parameters
	IF @donation_id is null OR @distribution_amount is null
	BEGIN
		SET @error_message = 'Donation id and distribution amount cannot be null'+CHAR(13);
		RETURN
	END;


	--Required fields
	DECLARE @program_id int = 3; --General Giving
	IF @program_name is not null
	BEGIN
		SET @program_id = (SELECT TOP 1 Program_ID FROM [dbo].Programs WHERE Program_Name = @program_name ORDER BY Program_ID ASC);
		IF @program_id is null
		BEGIN
			SET @error_message = 'Could not find program with name '+@program_name+' so distribution will be made to General Giving instead'+CHAR(13);
			SET @program_id = 3;
		END
	END;


	--Optional fields
	SET @congregation_id = ISNULL(@congregation_id, 5); --Not site specific

	DECLARE @pledge_id int;
	IF @pledge_donor_email is not null
	BEGIN
		DECLARE @pledge_contact_id int = (SELECT Contact_ID FROM [dbo].dp_Users WHERE User_Name = @pledge_donor_email);
		IF @pledge_contact_id is not null
		BEGIN --Get donor's pledge towards campaign attached to program
			DECLARE @pledge_donor_id int = (SELECT Donor_Record FROM [dbo].Contacts WHERE Contact_ID = @pledge_contact_id);		
			DECLARE @pledge_campaign_id int = (SELECT Pledge_Campaign_ID FROM [dbo].Programs WHERE Program_ID = @program_id);

			IF @pledge_donor_id is not null AND @pledge_campaign_id is not null
				SET @pledge_id = (SELECT Pledge_ID FROM [dbo].Pledges WHERE Donor_ID = @pledge_donor_id AND Pledge_Campaign_ID = @pledge_campaign_id);
			ELSE
				SET @error_message = @error_message+'Could not find pledge for donor '+@pledge_donor_email+' towards pledge campaign for program '+@program_name+CHAR(13);
		END
		ELSE
			SET @error_message = @error_message+'Could not find pledge contact with email '+@pledge_donor_email+CHAR(13);
	END;

	DECLARE @soft_credit_donor_id int;
	IF @soft_credit_donor_email is not null
	BEGIN
		DECLARE @sc_contact_id int = (SELECT Contact_ID FROM [dbo].dp_Users WHERE User_Name = @soft_credit_donor_email);
		IF @sc_contact_id is not null
		BEGIN 
			SET @soft_credit_donor_id = (SELECT Donor_Record FROM [dbo].Contacts WHERE Contact_ID = @sc_contact_id);
			IF @soft_credit_donor_id is null
			BEGIN
				--Use defaults
				EXEC [dbo].[cr_QA_Create_Donor_By_Contact_Id] @sc_contact_id, null, null, null, null, null, 
				@error_message = @error_message OUTPUT, @donor_id = @soft_credit_donor_id OUTPUT;

				IF @soft_credit_donor_id is null
					SET @error_message = @error_message+'Could not create donor record for contact with email '+@soft_credit_donor_email+'. Donation distribution will not be soft credited to them.'+CHAR(13);
			END;
		END
		ELSE
			SET @error_message = @error_message+'Could not find soft credit donor with email '+@soft_credit_donor_email+'. Donation distribution will not be soft credited to them.'+CHAR(13);
	END;
	

	--Create distribution
	INSERT INTO [dbo].Donation_Distributions
	(Donation_ID ,Amount              ,Program_id ,Pledge_ID ,Soft_Credit_Donor    ,Notes ,Congregation_ID ,Domain_ID) VALUES
	(@donation_id,@distribution_amount,@program_id,@pledge_id,@soft_credit_donor_id,@notes,@congregation_id,1        );

	SET @distribution_id = SCOPE_IDENTITY();
END
GO

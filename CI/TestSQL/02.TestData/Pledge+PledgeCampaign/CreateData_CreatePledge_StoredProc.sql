USE [MinistryPlatform]
GO
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

-- =============================================
-- Author:		Henney, Sarah
-- Create date: 01/29/2018
-- Description:	Creates (if nonexistent) or Updates pledge information
-- Output:      @pledge_id contains the pledge id, @error_message contains basic error message
-- =============================================


-- Defines cr_QA_Create_Pledge
IF NOT EXISTS ( SELECT  *
	FROM    sys.objects
	WHERE   object_id = OBJECT_ID(N'cr_QA_Create_Pledge')
			AND type IN ( N'P', N'PC' ) )
	EXEC('CREATE PROCEDURE dbo.cr_QA_Create_Pledge
	@donor_email varchar(255),
	@campaign_name nvarchar(50),
	@amount_pledged money,
	@first_installment_date datetime,
	@installments_planned smallint,
	@error_message nvarchar(500) OUTPUT,
	@pledge_id int OUTPUT AS SET NOCOUNT ON;')
GO
ALTER PROCEDURE [dbo].[cr_QA_Create_Pledge]
	@donor_email varchar(255),
	@campaign_name nvarchar(50),
	@amount_pledged money,
	@first_installment_date datetime,
	@installments_planned smallint,
	@error_message nvarchar(500) OUTPUT,
	@pledge_id int OUTPUT
AS
BEGIN
	SET NOCOUNT ON;
	
	--Enforce required parameters
	IF @donor_email is null OR @campaign_name is null 
	BEGIN
		SET @error_message = 'Donor email and pledge campaign name cannot be null'+CHAR(13);
		RETURN;
	END;


	--Required fields
	SET @first_installment_date = ISNULL(@first_installment_date, GETDATE()); --Today
	SET @amount_pledged = ISNULL(@amount_pledged, 1000);

	DECLARE @pledge_status_id int = 1; --Active

	DECLARE @contact_id int = (SELECT Contact_ID FROM [dbo].dp_Users WHERE User_Name = @donor_email);
	IF @contact_id is null
	BEGIN
		SET @error_message = 'Could not find contact with email '+@donor_email+CHAR(13);
		RETURN;
	END;

	DECLARE @donor_id int = (SELECT Donor_Record FROM [dbo].Contacts WHERE Contact_ID = @contact_id);
	IF @donor_id is null
	BEGIN
		DECLARE @statement_frequency int = 1; --Quarterly
		DECLARE @statement_method int = 1; --Postal
		DECLARE @statement_type int = 1; --Individual

		INSERT INTO [dbo].Donors
		(Contact_ID ,Setup_Date,Statement_Frequency_ID,Statement_Method_ID,Statement_Type_ID,Domain_ID) VALUES
		(@contact_id,GETDATE() ,@statement_frequency  ,@statement_method  ,@statement_type  ,1        );

		SET @donor_id = SCOPE_IDENTITY();
	END;

	DECLARE @campaign_id INT;
	SET @campaign_id = (SELECT TOP 1 Pledge_Campaign_ID FROM [dbo].Pledge_Campaigns WHERE Campaign_Name = @campaign_name ORDER BY Pledge_Campaign_ID ASC);
	IF @campaign_id is null
	BEGIN
		SET @error_message = 'Could not find pledge campaign with name '+@campaign_name+CHAR(13);
		RETURN;
	END;

	SET @installments_planned = ISNULL(@installments_planned, 0);
	DECLARE @installments_per_year INT = 0;
	IF @installments_planned > 0
		SET @installments_per_year = 12;


	--Optional fields
	DECLARE @currency nvarchar(25) = 'USD';
	DECLARE @notes nvarchar(500) = null;


	--Create/Update pledge
	SET @pledge_id = (SELECT TOP 1 Pledge_ID FROM [dbo].Pledges WHERE Donor_ID = @donor_id AND Pledge_Campaign_ID = @campaign_id ORDER BY Pledge_ID ASC);
	IF @pledge_id is null
	BEGIN
		INSERT INTO [dbo].Pledges 
		(Donor_ID ,Pledge_Campaign_ID,Pledge_Status_ID ,Total_Pledge   ,Installments_Planned ,Installments_Per_Year ,First_Installment_Date ,Domain_ID) VALUES
		(@donor_id,@campaign_id      ,@pledge_status_id,@amount_pledged,@installments_planned,@installments_per_year,@first_installment_date,1        );

		SET @pledge_id = SCOPE_IDENTITY();
	END;
	
	IF @pledge_id is not null
	BEGIN
		UPDATE [dbo].Pledges
		SET Pledge_Status_ID = @pledge_status_id,
		Total_Pledge = @amount_pledged,
		Installments_Planned = @installments_planned,
		Installments_Per_Year = @installments_per_year,
		Notes = @notes, 
		Currency = @currency
		WHERE Pledge_ID = @pledge_id;
	END
END
GO
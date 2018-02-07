USE [MinistryPlatform]
GO
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

-- =============================================
-- Author:		Henney, Sarah
-- Create date: 01/22/2018
-- Description:	Creates (if nonexistent) or Updates program information
-- Output:      @program_id contains the program id, @error_message contains basic error message
-- =============================================


-- Defines cr_QA_Create_Program
IF NOT EXISTS ( SELECT  *
	FROM    sys.objects
	WHERE   object_id = OBJECT_ID(N'cr_QA_Create_Program')
			AND type IN ( N'P', N'PC' ) )
	EXEC('CREATE PROCEDURE dbo.cr_QA_Create_Program 
	@program_name nvarchar(130),
	@primary_contact_email nvarchar(254),
	@start_date datetime,
	@end_date datetime,
	@congregation_id int,
	@ministry_id int,
	@program_type_id int,
	@communication_id int,
	@pledge_campaign_name nvarchar(50),
	@available_online bit,
	@allow_recurring_giving bit,
	@error_message nvarchar(500) OUTPUT,
	@program_id int OUTPUT AS SET NOCOUNT ON;')
GO
ALTER PROCEDURE [dbo].[cr_QA_Create_Program] 
	@program_name nvarchar(130),
	@primary_contact_email nvarchar(254),
	@start_date datetime,
	@end_date datetime,
	@congregation_id int,
	@ministry_id int,
	@program_type_id int,
	@communication_id int,
	@pledge_campaign_name nvarchar(50),
	@available_online bit,
	@allow_recurring_giving bit,
	@error_message nvarchar(500) OUTPUT,
	@program_id int OUTPUT
AS
BEGIN
	SET NOCOUNT ON;
	
	--Enforce required parameters
	IF @program_name is null OR @primary_contact_email is null
	BEGIN
		SET @error_message = 'Program name and primary contact email cannot be null'+CHAR(13);
		RETURN;
	END;

	--Required fields
	SET @allow_recurring_giving = ISNULL(@allow_recurring_giving, 0);
	SET @start_date = ISNULL(@start_date, DATEADD(yy, DATEDIFF(yy, 0, GETDATE()), 0)); --Defaults to 1/1/[current year]
	SET @congregation_id = ISNULL(@congregation_id, 5); --Not site specific
	SET @ministry_id = ISNULL(@ministry_id, 1); --General

	DECLARE @allow_online_giving bit = 1;
	DECLARE @statement_header_id INT = 1; --Ministry
	DECLARE @on_batch_tool BIT = 1; --Will show in BMT
	DECLARE @on_event_tool bit = 1; --Will show in Create/Edit Event tool
	DECLARE @statement_title nvarchar(50) = SUBSTRING(@program_name, 1, 50);
	DECLARE @tax_deductible bit = 1;

	DECLARE @primary_contact_id int = (SELECT Contact_ID FROM [dbo].dp_Users WHERE User_Name = @primary_contact_email);
	IF @primary_contact_id is null
	BEGIN
		SET @error_message = 'Could not find contact for given email '+@primary_contact_email+CHAR(13);
		RETURN;
	END;


	--Optional fields
	SET @program_type_id = ISNULL(@program_type_id, 1); --Default to Fuel

	DECLARE @online_sort_order smallint = 2;
	DECLARE @pledge_campaign_id int = null;	

	--Update fields based on program type	
	IF @program_type_id = 3 --Trips
	BEGIN
		SET @statement_header_id = 2; --Trips
		SET @on_batch_tool = 0;
	END;

	IF @pledge_campaign_name is not null
	BEGIN
		SET @pledge_campaign_id = (SELECT TOP 1 Pledge_Campaign_ID FROM [dbo].Pledge_Campaigns WHERE Campaign_Name = @pledge_campaign_name);
		IF @pledge_campaign_id is null
			SET @error_message = 'Could not find pledge campaign with name '+@pledge_campaign_name+', so will not be added to program'+CHAR(13);
	END;	

	
	--Create/Edit program
	SET @program_id = (SELECT TOP 1 Program_ID FROM [dbo].Programs WHERE Program_Name = @program_name ORDER BY Program_ID ASC);
	IF @program_id is null
	BEGIN
		INSERT INTO [dbo].Programs 
		(Program_Name ,Congregation_ID ,Ministry_ID ,Start_Date ,Primary_Contact    ,Tax_Deductible_Donations,Statement_Title ,Statement_Header_ID ,Allow_Online_Giving ,On_Donation_Batch_Tool,Domain_ID,Allow_Recurring_Giving ,Show_On_Event_Tool) VALUES
		(@program_name,@congregation_id,@ministry_id,@start_date,@primary_contact_id,@tax_deductible         ,@statement_title,@statement_header_id,@allow_online_giving,@on_batch_tool        ,1        ,@allow_recurring_giving,@on_event_tool    );

		SET @program_id = SCOPE_IDENTITY();
	END;

	IF @program_id is not null
	BEGIN
		UPDATE [dbo].Programs
		SET Congregation_ID = @congregation_id,
		Ministry_ID = @ministry_id,
		Start_Date = @start_date,
		Primary_Contact = @primary_contact_id,
		Allow_Recurring_Giving = @allow_recurring_giving,
		Show_On_Event_Tool = @on_event_tool,
		Allow_Online_Giving = @allow_online_giving,
		End_Date = @end_date,
		Program_Type_ID = @program_type_id,
		Tax_Deductible_Donations = @tax_deductible, 
		Statement_Title = @statement_title,
		Statement_Header_ID = @statement_header_id,
		Online_Sort_Order = @online_sort_order,
		Pledge_Campaign_ID = @pledge_campaign_id,
		On_Donation_Batch_Tool = @on_batch_tool,
		Available_Online = @available_online,
		Communication_ID = @communication_id
		WHERE Program_ID = @program_id;
	END
	ELSE
	BEGIN
		SET @error_message = @error_message+'Could not create program for some reason'+CHAR(13);
		RETURN;
	END;

	--Add program to pledge campaign
	IF @pledge_campaign_id is not null
	BEGIN
		UPDATE [dbo].Pledge_Campaigns
		SET Program_ID = @program_id
		WHERE Pledge_Campaign_ID = @pledge_campaign_id;
	END;
END
GO
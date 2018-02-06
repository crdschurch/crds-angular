USE [MinistryPlatform]
GO
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

-- =============================================
-- Author:		Henney, Sarah
-- Create date: 02/01/2018
-- Description:	Creates an empty deposit with the given information
-- Output:      @deposit_id contains the created deposit id, @error_message contains basic error message
-- =============================================


-- Defines cr_QA_New_Empty_Deposit
IF NOT EXISTS ( SELECT  *
	FROM    sys.objects
	WHERE   object_id = OBJECT_ID(N'cr_QA_New_Empty_Deposit')
			AND type IN ( N'P', N'PC' ) )
	EXEC('CREATE PROCEDURE dbo.cr_QA_New_Empty_Deposit
	@deposit_name nvarchar(75),
	@deposit_date datetime,
	@account_number nvarchar(15),
	@error_message nvarchar(50) OUTPUT,
	@deposit_id int OUTPUT AS SET NOCOUNT ON;')
GO
ALTER PROCEDURE [dbo].[cr_QA_New_Empty_Deposit]
	@deposit_name nvarchar(75),
	@deposit_date datetime,
	@account_number nvarchar(15),
	@error_message nvarchar(50) OUTPUT,
	@deposit_id int OUTPUT
AS
BEGIN
	SET NOCOUNT ON;
	
	--Enforce required parameters
	IF @deposit_name is null
	BEGIN
		SET @error_message = 'deposit name cannot be null';
		RETURN;
	END;

	--Required fields
	SET @deposit_date = ISNULL(@deposit_date, DATEADD(yy, DATEDIFF(yy, 0, GETDATE()), 0)); --Defaults to 1/1/[current year]
	SET @account_number = ISNULL(@account_number, 'Test Account');

	DECLARE @batch_count int = 0;
	DECLARE @exported bit = 0;

	--total - processor fee = amount if deposit is balanced
	--total is sum of all batch totals
	DECLARE @deposit_total money = 0;
	DECLARE @processor_fee money = 0;
	DECLARE @deposit_amount money = 0;


	--Create deposit
	INSERT INTO [dbo].Deposits 
	(Deposit_Name ,Deposit_Total ,Deposit_Amount ,Processor_Fee_Total,Deposit_Date ,Account_Number ,Batch_Count ,Domain_ID,Exported ) VALUES
	(@deposit_name,@deposit_total,@deposit_amount,@processor_fee     ,@deposit_date,@account_number,@batch_count,1        ,@exported);
	
	SET @deposit_id = SCOPE_IDENTITY();
END
GO
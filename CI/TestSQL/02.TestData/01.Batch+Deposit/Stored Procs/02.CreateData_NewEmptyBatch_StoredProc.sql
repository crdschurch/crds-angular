USE [MinistryPlatform]
GO
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

-- =============================================
-- Author:      Henney, Sarah
-- Create date: 01/29/2018
-- Description: Creates an empty batch with given information
-- Output:      @batch_id contains the batch id, @error_message contains basic error message
-- =============================================


-- Defines cr_QA_New_Empty_Batch
IF NOT EXISTS ( SELECT  *
	FROM    sys.objects
	WHERE   object_id = OBJECT_ID(N'cr_QA_New_Empty_Batch')
			AND type IN ( N'P', N'PC' ) )
	EXEC('CREATE PROCEDURE dbo.cr_QA_New_Empty_Batch
	@batch_name nvarchar(75),
	@setup_date datetime,
	@finalize_date datetime,
	@user_email varchar(254),
	@congregation_id int,
	@deposit_id int,
	@default_program_id_list nvarchar(32),
	@error_message nvarchar(500) OUTPUT,
	@batch_id int OUTPUT AS SET NOCOUNT ON;')
GO
ALTER PROCEDURE [dbo].[cr_QA_New_Empty_Batch]
	@batch_name nvarchar(75),
	@setup_date datetime,
	@finalize_date datetime,
	@user_email varchar(254),
	@congregation_id int,
	@deposit_id int,
	@default_program_id_list nvarchar(32),
	@error_message nvarchar(500) OUTPUT,
	@batch_id int OUTPUT
AS
BEGIN
	SET NOCOUNT ON;
	
	--Enforce required parameters
	IF @batch_name is null
	BEGIN
		SET @error_message = 'Batch name cannot be null'+CHAR(13);
		RETURN;
	END;


	--Required fields
	SET @setup_date = ISNULL(@setup_date, DATEADD(yy, DATEDIFF(yy, 0, GETDATE()), 0)); --Defaults to 1/1/[current year]

	DECLARE @item_cout smallint = 0;
	DECLARE @batch_total money = 0;
	DECLARE @batch_entry_type int = 12; --Batch Manager Tool


	--Optional fields
	DECLARE @default_payment_type int = 4; --Credit Card
	DECLARE @currency varchar(25) = 'USD';
	
	DECLARE @operator_user_id int = null;
	IF @user_email is not null
	BEGIN
		SET @operator_user_id = (SELECT User_ID FROM [dbo].dp_Users WHERE User_Name = @user_email);
		IF @operator_user_id is null
			SET @error_message = 'Could not find contact with email '+@user_email+', so operator user will not be added to batch'+CHAR(13);
	END

	
	--Create batch
	INSERT INTO [dbo].Batches 
	(Batch_Name ,Setup_Date ,Batch_Total ,Item_Count,Batch_Entry_Type_ID,Deposit_ID ,Finalize_Date ,Domain_ID,Congregation_ID ,Default_Payment_Type ,Currency ,Operator_User    ,Default_Program_ID_List ) VALUES
	(@batch_name,@setup_date,@batch_total,@item_cout,@batch_entry_type  ,@deposit_id,@finalize_date,1        ,@congregation_id,@default_payment_type,@currency,@operator_user_id,@default_program_id_list);

	SET @batch_id = SCOPE_IDENTITY();
END
GO
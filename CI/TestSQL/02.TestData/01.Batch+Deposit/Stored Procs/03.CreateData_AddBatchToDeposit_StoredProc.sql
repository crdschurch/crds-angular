USE [MinistryPlatform]
GO
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

-- =============================================
-- Author:      Henney, Sarah
-- Create date: 02/01/2018
-- Description: Adds batch to deposit. Both must exist.
-- Output:      @deposit_id contains the created deposit id, @batch_id contains the batch id, @error_message contains basic error message
-- =============================================


-- Defines cr_QA_Add_Batch_To_Deposit
IF NOT EXISTS ( SELECT  *
	FROM    sys.objects
	WHERE   object_id = OBJECT_ID(N'cr_QA_Add_Batch_To_Deposit')
			AND type IN ( N'P', N'PC' ) )
	EXEC('CREATE PROCEDURE dbo.cr_QA_Add_Batch_To_Deposit 
	@deposit_name nvarchar(75),
	@batch_name nvarchar(75),
	@error_message nvarchar(500) OUTPUT,
	@deposit_id int OUTPUT,
	@batch_id int OUTPUT AS SET NOCOUNT ON;')
GO
ALTER PROCEDURE [dbo].[cr_QA_Add_Batch_To_Deposit]
	@deposit_name nvarchar(75),
	@batch_name nvarchar(75),
	@error_message nvarchar(500) OUTPUT,
	@deposit_id int OUTPUT,
	@batch_id int OUTPUT
AS
BEGIN
	SET NOCOUNT ON;
	
	--Retrieve deposit
	IF @deposit_name is null
	BEGIN
		SET @error_message = 'Deposit name cannot be null'+CHAR(13);
		RETURN;
	END;
	SET @deposit_id = (SELECT TOP 1 Deposit_ID FROM [dbo].Deposits WHERE Deposit_Name = @deposit_name ORDER BY Deposit_ID ASC);
	IF @deposit_id is null
	BEGIN
		SET @error_message = 'Deposit with name'+@deposit_name+' could not be found'+CHAR(13);
		RETURN;
	END;

	--Retrieve batch
	IF @batch_name is null
	BEGIN
		SET @error_message = 'Batch name cannot be null'+CHAR(13);
		RETURN;
	END;
	SET @batch_id = (SELECT TOP 1 Batch_ID FROM [dbo].Batches WHERE Batch_Name = @batch_name ORDER BY Batch_ID ASC);
	IF @batch_id is null
	BEGIN
		SET @error_message = 'Batch with name'+@batch_name+' could not be found'+CHAR(13);
		RETURN;
	END;


	--Add deposit to batch
	UPDATE [dbo].Batches
	SET Deposit_ID = @deposit_id
	WHERE Batch_ID = @batch_id;


	--Update deposit totals
	DECLARE @deposit_total money = (SELECT SUM(Batch_Total) FROM [dbo].Batches WHERE Deposit_ID = @deposit_id);
	DECLARE @batch_count int = (SELECT COUNT(Batch_ID) FROM [dbo].Batches WHERE Deposit_ID = @deposit_id);
	
	UPDATE [dbo].Deposits
	SET Deposit_Total = @deposit_total,
	Deposit_Amount = (@deposit_total - Processor_Fee_Total),
	Batch_Count = @batch_count
	WHERE Deposit_ID = @deposit_id;
END;
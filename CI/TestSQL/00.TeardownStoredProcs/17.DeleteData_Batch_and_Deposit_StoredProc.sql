USE [MinistryPlatform]
GO
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

-- =============================================
-- Author:		Henney, Sarah
-- Create date: 02/16/2018
-- Description:	Stored procedure declarations for deleting batch and deposit data.
-- =============================================

-- Defines cr_QA_Delete_Batch
IF NOT EXISTS ( SELECT  *
	FROM    sys.objects
	WHERE   object_id = OBJECT_ID(N'cr_QA_Delete_Batch')
			AND type IN ( N'P', N'PC' ) )
	EXEC('CREATE PROCEDURE dbo.cr_QA_Delete_Batch
	@batch_id int AS SET NOCOUNT ON;')
GO
ALTER PROCEDURE [dbo].[cr_QA_Delete_Batch] 
	@batch_id int
AS
BEGIN
	SET NOCOUNT ON;

	IF @batch_id is null
		RETURN;

	--Nullify foreign keys
	UPDATE [dbo].Payments SET Batch_ID = null WHERE Batch_ID = @batch_id;
	UPDATE [dbo].Donations SET Batch_ID = null WHERE Batch_ID = @batch_id;

	DELETE [dbo].Batches WHERE Batch_ID = @batch_id;
END
GO


-- Defines cr_QA_Delete_Batch_By_Name
IF NOT EXISTS ( SELECT  *
	FROM    sys.objects
	WHERE   object_id = OBJECT_ID(N'cr_QA_Delete_Batch_By_Name')
			AND type IN ( N'P', N'PC' ) )
	EXEC('CREATE PROCEDURE dbo.cr_QA_Delete_Batch_By_Name
	@batch_name nvarchar(75) AS SET NOCOUNT ON;')
GO
ALTER PROCEDURE [dbo].[cr_QA_Delete_Batch_By_Name] 
	@batch_name nvarchar(75)
AS
BEGIN
	SET NOCOUNT ON;

	IF @batch_name is null
		RETURN;
	
	DECLARE @batches_to_delete TABLE
	(
		batch_id int
	)
	INSERT INTO @batches_to_delete (batch_id) SELECT Batch_ID FROM [dbo].Batches WHERE Batch_Name = @batch_name;

	--Nullify foreign keys
	UPDATE [dbo].Payments SET Batch_ID = null WHERE Batch_ID IN (SELECT batch_id from @batches_to_delete);
	UPDATE [dbo].Donations SET Batch_ID = null WHERE Batch_ID IN (SELECT batch_id from @batches_to_delete);

	DELETE [dbo].Batches WHERE Batch_ID IN (SELECT batch_id from @batches_to_delete);
END
GO


-- Defines cr_QA_Delete_Deposit
IF NOT EXISTS ( SELECT  *
	FROM    sys.objects
	WHERE   object_id = OBJECT_ID(N'cr_QA_Delete_Deposit')
			AND type IN ( N'P', N'PC' ) )
	EXEC('CREATE PROCEDURE dbo.cr_QA_Delete_Deposit
	@deposit_id int AS SET NOCOUNT ON;')
GO
ALTER PROCEDURE [dbo].[cr_QA_Delete_Deposit] 
	@deposit_id int,
	@deposit_name nvarchar(75)
AS
BEGIN
	SET NOCOUNT ON;

	IF @deposit_id is null
		RETURN;

	--Nullify foreign keys
	UPDATE [dbo].Batches SET Deposit_ID = null WHERE Deposit_ID = @deposit_id;
	
	DELETE [dbo].Deposits WHERE Deposit_ID = @deposit_id;
END
GO


-- Defines cr_QA_Delete_Deposit_By_Name
IF NOT EXISTS ( SELECT  *
	FROM    sys.objects
	WHERE   object_id = OBJECT_ID(N'cr_QA_Delete_Deposit_By_Name')
			AND type IN ( N'P', N'PC' ) )
	EXEC('CREATE PROCEDURE dbo.cr_QA_Delete_Deposit_By_Name
	@deposit_name nvarchar(75) AS SET NOCOUNT ON;')
GO
ALTER PROCEDURE [dbo].[cr_QA_Delete_Deposit_By_Name] 
	@deposit_name nvarchar(75)
AS
BEGIN
	SET NOCOUNT ON;

	IF @deposit_name is null
		RETURN;

	DECLARE @deposits_to_delete TABLE
	(
		deposit_id int
	)
	INSERT INTO @deposits_to_delete (deposit_id) SELECT Deposit_ID FROM [dbo].Deposits 
	WHERE Deposit_Name = @deposit_name; 

	--Nullify foreign keys
	UPDATE [dbo].Batches SET Deposit_ID = null WHERE Deposit_ID IN (SELECT deposit_id FROM @deposits_to_delete);
	
	DELETE [dbo].Deposits WHERE Deposit_ID IN (SELECT deposit_id FROM @deposits_to_delete);
END
GO
USE [MinistryPlatform]
GO

SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO

IF EXISTS(SELECT * FROM [sys].[triggers] WHERE [name] = 'crds_tr_Small_Group_To_Firestore')
BEGIN
	--DISABLE TRIGGER
	ALTER TABLE [dbo].[Groups] DISABLE TRIGGER [crds_tr_Small_Group_To_Firestore]

	--DELETE TRIGGER
	DROP TRIGGER [crds_tr_Small_Group_To_Firestore]; 
END
GO



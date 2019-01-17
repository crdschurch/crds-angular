USE MinistryPlatform
GO

DECLARE @ID AS INT

SET @ID = (SELECT API_Procedure_ID
FROM [dbo].[dp_API_Procedures]
WHERE [Procedure_Name] = 'api_crds_Get_Manage_Children_data')

IF NOT EXISTS(select * from [dbo].[dp_Role_API_Procedures] WHERE API_Procedure_ID = @ID AND Role_ID = 112)
BEGIN
	INSERT INTO [dbo].[dp_Role_API_Procedures]
	([Role_ID],
	[API_Procedure_ID],
	[Domain_ID])
	VALUES
	(112,
	@ID,
	1)
END

USE MinistryPlatform
GO

DECLARE @ID AS INT

SET @ID = (SELECT API_Procedure_ID
FROM [dbo].[dp_API_Procedures]
WHERE [Procedure_Name] = 'api_crds_ImportEcheckEvent')

IF NOT EXISTS(select * from [dbo].[dp_Role_API_Procedures] WHERE API_Procedure_ID = @ID AND Role_ID = 112)
BEGIN
	INSERT INTO [dbo].[dp_Role_API_Procedures]
	([Role_ID],
	[API_Procedure_ID],
	[Domain_ID])
	VALUES
	(112,
	@ID,
	1)
END


USE MinistryPlatform
GO

DECLARE @ID AS INT

SET @ID = (SELECT API_Procedure_ID
FROM [dbo].[dp_API_Procedures]
WHERE [Procedure_Name] = 'api_crds_ResetEcheckEvent')

IF NOT EXISTS(select * from [dbo].[dp_Role_API_Procedures] WHERE API_Procedure_ID = @ID AND Role_ID = 112)
BEGIN
	INSERT INTO [dbo].[dp_Role_API_Procedures]
	([Role_ID],
	[API_Procedure_ID],
	[Domain_ID])
	VALUES
	(112,
	@ID,
	1)
END

USE MinistryPlatform
GO

DECLARE @ID AS INT

SET @ID = (SELECT API_Procedure_ID
FROM [dbo].[dp_API_Procedures]
WHERE [Procedure_Name] = 'api_crds_Get_Checkin_Room_Data')

IF NOT EXISTS(select * from [dbo].[dp_Role_API_Procedures] WHERE API_Procedure_ID = @ID AND Role_ID = 112)
BEGIN
	INSERT INTO [dbo].[dp_Role_API_Procedures]
	([Role_ID],
	[API_Procedure_ID],
	[Domain_ID])
	VALUES
	(112,
	@ID,
	1)
END

USE MinistryPlatform
GO

DECLARE @ID AS INT

SET @ID = (SELECT API_Procedure_ID
FROM [dbo].[dp_API_Procedures]
WHERE [Procedure_Name] = 'api_crds_Update_Single_Room_Checkin_Data')

IF NOT EXISTS(select * from [dbo].[dp_Role_API_Procedures] WHERE API_Procedure_ID = @ID AND Role_ID = 112)
BEGIN
	INSERT INTO [dbo].[dp_Role_API_Procedures]
	([Role_ID],
	[API_Procedure_ID],
	[Domain_ID])
	VALUES
	(112,
	@ID,
	1)
END

USE MinistryPlatform
GO

DECLARE @ID AS INT

SET @ID = (SELECT API_Procedure_ID
FROM [dbo].[dp_API_Procedures]
WHERE [Procedure_Name] = 'api_crds_Get_Checkin_Single_Room_Data')

IF NOT EXISTS(select * from [dbo].[dp_Role_API_Procedures] WHERE API_Procedure_ID = @ID AND Role_ID = 112)
BEGIN
	INSERT INTO [dbo].[dp_Role_API_Procedures]
	([Role_ID],
	[API_Procedure_ID],
	[Domain_ID])
	VALUES
	(112,
	@ID,
	1)
END

USE MinistryPlatform
GO

DECLARE @ID AS INT

SET @ID = (SELECT API_Procedure_ID
FROM [dbo].[dp_API_Procedures]
WHERE [Procedure_Name] = 'api_crds_Child_Signin_Search')

IF NOT EXISTS(select * from [dbo].[dp_Role_API_Procedures] WHERE API_Procedure_ID = @ID AND Role_ID = 112)
BEGIN
	INSERT INTO [dbo].[dp_Role_API_Procedures]
	([Role_ID],
	[API_Procedure_ID],
	[Domain_ID])
	VALUES
	(112,
	@ID,
	1)
END
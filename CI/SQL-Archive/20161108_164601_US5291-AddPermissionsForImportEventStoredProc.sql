USE MinistryPlatform
GO

IF NOT EXISTS(SELECT * FROM [dbo].[dp_API_Procedures] WHERE [procedure_name] = 'api_crds_ImportEcheckEvent')
BEGIN
	DECLARE @ID INT

	INSERT INTO [dbo].[dp_API_Procedures] ([procedure_name]) 
	VALUES('api_crds_ImportEcheckEvent')

	SET @ID = SCOPE_IDENTITY();

	IF NOT EXISTS(select * from [dbo].[dp_Role_API_Procedures] WHERE API_Procedure_ID =@ID AND Role_ID = 112)
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
END
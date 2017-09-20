USE MinistryPlatform;

DECLARE @PROCNAME NVARCHAR(128) = 'api_crds_Get_Finder_AWS_Data_For_Single_Group';
DECLARE @ROLEID INT = 62; --unauthenticated create

IF NOT EXISTS(SELECT * FROM [dbo].[dp_API_Procedures] where Procedure_Name = @PROCNAME)
BEGIN
	INSERT INTO [dbo].[dp_API_Procedures]([Procedure_Name], [Description]) VALUES(@PROCNAME, 'Get Single group for AWS Cloudsearch')
END

DECLARE @PROCID INT;
SELECT  @PROCID = [API_Procedure_ID] FROM [dbo].[dp_API_Procedures] WHERE Procedure_Name = @PROCNAME

IF NOT EXISTS(SELECT * FROM [dbo].[dp_Role_API_Procedures] WHERE Role_ID = @ROLEID AND API_Procedure_ID = @PROCID)
BEGIN
	INSERT INTO [dbo].[dp_Role_API_Procedures](Role_ID,API_Procedure_ID, Domain_ID) VALUES(@ROLEID, @PROCID, 1);
END

GO

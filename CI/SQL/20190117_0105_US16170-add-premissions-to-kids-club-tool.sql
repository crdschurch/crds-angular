USE MinistryPlatform
GO

IF NOT EXISTS(select * FROM [dbo].[dp_Role_Pages] WHERE Role_ID = 112 AND Page_ID = 420)
BEGIN
	INSERT INTO [dbo].[dp_Role_Pages]
		(Role_Id, Page_ID, Access_Level, Scope_All, Approver, File_Attacher, Data_Importer, Data_Exporter, Secure_Records, Allow_Comments, Quick_Add)
	Values  (112,     420,     0,            0,         0,        0,             0,         0,             0,              0,              0)
END


USE MinistryPlatform
GO

IF NOT EXISTS(select * FROM [dbo].[dp_Role_Pages] WHERE Role_ID = 112 AND Page_ID = 420)
BEGIN
	INSERT INTO [dbo].[dp_Role_Pages]
		(Role_Id, Page_ID, Access_Level, Scope_All, Approver, File_Attacher, Data_Importer, Data_Exporter, Secure_Records, Allow_Comments, Quick_Add)
	Values  (112,     277,     0,            0,         0,        0,             0,             0,              0,              0,         0)
END

USE MinistryPlatform
GO

IF NOT EXISTS(select * FROM [dbo].[dp_Role_Pages] WHERE Role_ID = 112 AND Page_ID = 420)
BEGIN
	INSERT INTO [dbo].[dp_Role_Pages]
		(Role_Id, Page_ID, Access_Level, Scope_All, Approver, File_Attacher, Data_Importer, Data_Exporter, Secure_Records, Allow_Comments, Quick_Add)
	Values  (112,     290,     3,            0,         0,        0,             0,         0,              0,              0,                0)
END

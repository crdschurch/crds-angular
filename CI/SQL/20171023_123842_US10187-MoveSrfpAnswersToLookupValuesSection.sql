USE MinistryPlatform;
GO

IF NOT EXISTS (SELECT 1 FROM [dbo].[dp_Page_Section_Pages] 
			   WHERE Page_Section_ID = 4 
			   AND Page_ID = 636)
BEGIN
	INSERT INTO [dbo].[dp_Page_Section_Pages] (
		 [Page_Section_ID]
		,[Page_ID]
	) VALUES (
		 4
		,636
	)
END
		
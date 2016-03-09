USE MinistryPlatform;
GO

IF NOT EXISTS (SELECT 1 FROM [dbo].[dp_Page_Section_Pages] 
			   WHERE Page_Section_ID = 21 
			   AND Page_ID = 10)
BEGIN
	INSERT INTO [dbo].[dp_Page_Section_Pages] (
		 [Page_Section_ID]
		,[Page_ID]
	) VALUES (
		 21
		,10
	)
END
		
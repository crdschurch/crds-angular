USE [MinistryPlatform]
GO

DECLARE @PAGEID int = 606;
DECLARE @PAGESECTIONID int = 22;

IF NOT EXISTS (SELECT 1 FROM [dbo].[dp_Pages] WHERE [Page_ID] = @PAGEID) 
BEGIN
	SET IDENTITY_INSERT [dbo].[dp_Pages] ON;
	INSERT INTO [dbo].[dp_Pages] (
		 [Page_ID]
		,[Display_Name]
		,[Singular_Name]
		,[Description]
		,[View_Order]
		,[Table_Name]
		,[Primary_Key]
		,[Display_Search]
		,[Default_Field_List]
		,[Selected_Record_Expression]
		,[Display_Copy]
	) VALUES (
		 @PAGEID
		,N'Allergies'
		,N'Allergy'
		,N'Allergies'
		,N'30'
		,N'cr_Allergy'
		,N'Allergy_ID'
		,1
		,N'Allergy_Type_ID_Table.[Description] as [Allergy_Type],cr_Allergy.[Description],[Reaction]'
		,N'cr_Allergy.Allergy_ID'
		,0
	)
	SET IDENTITY_INSERT [dbo].[dp_Pages] OFF;
END

IF NOT EXISTS ( SELECT 1 FROM [dbo].[dp_Page_Section_Pages] WHERE [Page_ID] = @PAGEID )
BEGIN
  INSERT INTO [dbo].[dp_Page_Section_Pages] (
		 [Page_ID]
		,[Page_Section_ID])
		VALUES (
		  @PAGEID
		 ,@PAGESECTIONID)
END
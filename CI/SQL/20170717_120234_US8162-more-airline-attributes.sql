use MinistryPlatform
GO

DECLARE @Frequent_Flyer_ID int = 63;
DECLARE @Domain_ID int = 1;
DECLARE @American_Attribute_Name nvarchar(17) = N'American Airlines';
DECLARE @American_Attribute_ID int = 9049

IF NOT EXISTS( SELECT 1 FROM [dbo].[Attributes] Where [Attribute_Type_ID] = @Frequent_Flyer_ID 
			  AND [Attribute_ID] =  @American_Attribute_ID)
BEGIN
	SET IDENTITY_INSERT [dbo].[Attributes] ON
	INSERT INTO [dbo].[Attributes] (
	     [Attribute_ID]
		,[Attribute_Name]
		,[Attribute_Type_ID]
		,[Domain_ID]
	) VALUES (
		 @American_Attribute_ID
		,@American_Attribute_Name
		,@Frequent_Flyer_ID
		,@Domain_ID
	)
	SET IDENTITY_INSERT [dbo].[Attributes] OFF
END
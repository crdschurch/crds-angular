USE [MinistryPlatform]
GO

SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO

IF NOT EXISTS (SELECT * FROM INFORMATION_SCHEMA.TABLES 
           WHERE TABLE_NAME = N'cr_Srfp_Category_Weight')
BEGIN
	CREATE TABLE [dbo].[cr_Srfp_Category_Weight](
		[Category_Weight_ID] [int] IDENTITY(1,1) NOT NULL,
		[Category_Char] [VARCHAR](1) NOT NULL,
		[Category_Name] [nvarchar](25) NOT NULL,
		[Category_Multiplier] [numeric](18, 8) NOT NULL,
		[Start_Date] [datetime] NOT NULL,
		[End_Date] [datetime] NULL,
	 CONSTRAINT [PK_Srfp_Category_Weight] PRIMARY KEY CLUSTERED 
	(
		[Category_Weight_ID] ASC
	)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
	) ON [PRIMARY]
	
	
	
	INSERT INTO cr_Srfp_Category_Weight (Category_Char, Category_Name, Category_Multiplier, Start_Date) VALUES ('F', 'Financial', 1.705, '2017-10-01');
	INSERT INTO cr_Srfp_Category_Weight (Category_Char, Category_Name, Category_Multiplier, Start_Date) VALUES ('I', 'Intellectual', 1, '2017-10-01');
	INSERT INTO cr_Srfp_Category_Weight (Category_Char, Category_Name, Category_Multiplier, Start_Date) VALUES ('P', 'Physical', 1, '2017-10-01');
	INSERT INTO cr_Srfp_Category_Weight (Category_Char, Category_Name, Category_Multiplier, Start_Date) VALUES ('R', 'Relational', 1, '2017-10-01');
	INSERT INTO cr_Srfp_Category_Weight (Category_Char, Category_Name, Category_Multiplier, Start_Date) VALUES ('S', 'Spiritual', 0.667, '2017-10-01');

END
GO


USE [MinistryPlatform]

DECLARE @SRFP_CatWeight_Page_ID INT = 640;
DECLARE @Staff_Role_Id INT = 1016; -- SRFP-CRDS

SET IDENTITY_INSERT dp_pages ON

IF NOT EXISTS(SELECT * FROM dp_pages WHERE Page_ID = @SRFP_CatWeight_Page_ID)
BEGIN
	INSERT INTO dp_Pages(Page_ID, Display_Name, Singular_Name, View_Order, Table_Name, Default_Field_List, Selected_Record_Expression, Primary_Key, Display_Copy)
	  VALUES(@SRFP_CatWeight_Page_ID,
	         'SRFP Category Weights', 
			 'SRFP Category Weight', 
			 127, 
			 'cr_Srfp_Category_Weight', 
			 'Category_Char, Category_Name, Category_Multiplier, Start_Date, End_Date', 
			 'Category_Weight_ID',  
			 'Category_Weight_ID',
			 1);
END

SET IDENTITY_INSERT dp_pages OFF

-- security
IF NOT EXISTS(SELECT * FROM dp_role_pages WHERE Page_ID = @SRFP_CatWeight_Page_ID and Role_ID = @Staff_Role_Id )
BEGIN
	INSERT INTO dp_role_pages(Role_ID, Page_ID, Access_Level, Scope_All, Approver, File_Attacher, Data_Importer, Data_Exporter, Secure_Records, Allow_Comments, Quick_Add)
	            VALUES(@Staff_Role_Id, @SRFP_CatWeight_Page_ID, 1 , 0,0,0,0,1,0,0,1)
END

GO

-- Page Section
DECLARE @SRFP_CatWeight_Page_ID INT = 640;
DECLARE @LookupValuesSectionId INT = 4;

IF NOT EXISTS (SELECT 1 FROM [dbo].[dp_Page_Section_Pages] 
			   WHERE Page_Section_ID = @LookupValuesSectionId
			   AND Page_ID = @SRFP_CatWeight_Page_ID)
BEGIN
	INSERT INTO [dbo].[dp_Page_Section_Pages] (
		 [Page_Section_ID]
		,[Page_ID]
	) VALUES (
		 @LookupValuesSectionId
		,@SRFP_CatWeight_Page_ID
	)
END

-- Current SRFP Category Weights VIEW
-- DECLARE @SRFP_CatWeight_Page_ID INT = 640;
DECLARE @Current_Weight_View_ID INT = 1129;

SET IDENTITY_INSERT dp_page_views ON

IF NOT EXISTS (SELECT 1 FROM [dbo].[dp_page_views] WHERE Page_View_Id = @Current_Weight_View_ID)
BEGIN
	INSERT INTO dp_page_views 
	(Page_View_ID           ,View_Title                     ,Page_ID                ,Description                    ,Field_List,View_Clause       ) VALUES
	(@Current_Weight_View_ID,'Current SRFP Category Weights',@SRFP_CatWeight_Page_ID,'Current SRFP Category Weights',null      ,'End_Date IS NULL');
	
	UPDATE [dbo].[dp_pages] SET Default_view = @Current_Weight_View_ID WHERE Page_ID = @SRFP_CatWeight_Page_ID;

END

SET IDENTITY_INSERT dp_page_views OFF

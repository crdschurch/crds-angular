USE [MinistryPlatform]
GO

IF NOT EXISTS (SELECT 1 FROM [dbo].[dp_Pages] WHERE Page_ID = 38)
BEGIN
SET IDENTITY_INSERT [dbo].[dp_Pages] ON

	INSERT INTO [dbo].[dp_Pages]
           ([Page_ID]
		   ,[Display_Name]
           ,[Singular_Name]
           ,[Description]
           ,[View_Order]
           ,[Table_Name]
           ,[Primary_Key]
           ,[Display_Search]
           ,[Default_Field_List]
           ,[Selected_Record_Expression]
           ,[Display_Copy])
     VALUES
           (38,
		   'Childcare Request Dates'
           ,'Childcare Request Date'
           ,'Display childcare request dates for childcare site owners'
           ,10
           ,'cr_Childcare_Request_Dates'
           ,'Childcare_Request_Date_ID'
           , 1
           ,('Childcare_Request_ID, Childcare_Request_Date, Approved')
		   ,'Childcare_Request_Date_ID'
           ,1);

	 SET IDENTITY_INSERT [dbo].[dp_Pages] OFF
END
GO
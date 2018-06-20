USE [MinistryPlatform]
GO

-- Insert into Page Sections
INSERT INTO [dbo].[dp_Page_Sections]
           ([Page_Section]
           ,[View_Order])
	 OUTPUT Inserted.Page_Section_ID
     VALUES
           ('Giving Statements'
           , 81)
GO


INSERT INTO dbo.dp_Pages
           ([Display_Name]
           ,[Singular_Name]
           ,[Description]
           ,[View_Order]
           ,[Table_Name]
           ,[Primary_Key]
           ,[Display_Search]
           ,[Default_Field_List]
           ,[Selected_Record_Expression]
           ,[Filter_Clause]
           ,[Start_Date_Field]
           ,[End_Date_Field]
           ,[Contact_ID_Field]
           ,[Default_View]
           ,[Pick_List_View]
           ,[Image_Name]
           ,[Direct_Delete_Only]
           ,[System_Name]
           ,[Date_Pivot_Field]
           ,[Custom_Form_Name]
           ,[Display_Copy])
     VALUES
			('Q1 Giving Statements', 'Q1 Giving Statement', 'List of contacts who have given for the first quarter of the year', 35, 'vw_crds_Q1_Giving_Statements', NULL, 0, 'ContactID, DonorId, DisplayName, StatementMethod, EmailAddress, Congregation', 'ContactId', NULL, NULL, NULL, 'contact_id', NULL, NULL, NULL, NULL, NULL, NULL, NULL, 1),
			('Q2 Giving Statements', 'Q2 Giving Statement', 'List of contacts who have given for the second quarter of the year', 36, 'vw_crds_Q2_Giving_Statements', NULL, 0, 'ContactID, DonorId, DisplayName, StatementMethod, EmailAddress, Congregation', 'ContactId', NULL, NULL, NULL, 'contact_id', NULL, NULL, NULL, NULL, NULL, NULL, NULL, 1),
			('Q3 Giving Statements', 'Q3 Giving Statement', 'List of contacts who have given for the third quarter of the year', 37, 'vw_crds_Q3_Giving_Statements', NULL, 0, 'ContactID, DonorId, DisplayName, StatementMethod, EmailAddress, Congregation', 'ContactId', NULL, NULL, NULL, 'contact_id', NULL, NULL, NULL, NULL, NULL, NULL, NULL, 1),
			('Q4 Giving Statements', 'Q4 Giving Statement', 'List of contacts who have given for the fourth quarter of the year', 38, 'vw_crds_Q4_Giving_Statements', NULL, 0, 'ContactID, DonorId, DisplayName, StatementMethod, EmailAddress, Congregation', 'ContactId', NULL, NULL, NULL, 'contact_id', NULL, NULL, NULL, NULL, NULL, NULL, NULL, 1)

GO



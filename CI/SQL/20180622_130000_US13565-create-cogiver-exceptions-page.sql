USE [MinistryPlatform]
GO


DECLARE @OutputTbl TABLE (Page_ID INT)

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
     OUTPUT INSERTED.Page_ID INTO @OutputTbl(Page_ID)
     VALUES
		('Co-giver Exceptions', 'Co-giver Exception', 'List of contacts for which there are co-giver exceptions', 39, 'vw_crds_Cogiver_Exceptions', NULL, 0, 'ContactID, DisplayName, Exception', 'ContactId', NULL, NULL, NULL, 'ContactId', NULL, NULL, NULL, NULL, NULL, NULL, NULL, 1)

      INSERT INTO [dbo].[dp_Role_Pages]
           ([Role_ID],[Page_ID],[Access_Level],[Scope_All],[Approver],[File_Attacher],[Data_Importer],[Data_Exporter],[Secure_Records],[Allow_Comments],[Quick_Add])
			SELECT 2,[Page_ID],3,0,0,1,0,1,1,0,1
			FROM @OutputTbl

      INSERT INTO [dbo].[dp_Role_Pages]
           ([Role_ID],[Page_ID],[Access_Level],[Scope_All],[Approver],[File_Attacher],[Data_Importer],[Data_Exporter],[Secure_Records],[Allow_Comments],[Quick_Add])
			SELECT 5,[Page_ID],0,0,0,0,0,0,0,0,0
			FROM @OutputTbl

-- Insert roles
      INSERT INTO [dbo].[dp_Role_Pages]
           ([Role_ID],[Page_ID],[Access_Level],[Scope_All],[Approver],[File_Attacher],[Data_Importer],[Data_Exporter],[Secure_Records],[Allow_Comments],[Quick_Add])
			SELECT 6,[Page_ID],3,0,0,1,0,1,1,0,0
			FROM @OutputTbl
			
      INSERT INTO [dbo].[dp_Role_Pages]
           ([Role_ID],[Page_ID],[Access_Level],[Scope_All],[Approver],[File_Attacher],[Data_Importer],[Data_Exporter],[Secure_Records],[Allow_Comments],[Quick_Add])
			SELECT 7,[Page_ID],0,0,0,0,0,0,0,0,0
			FROM @OutputTbl

      INSERT INTO [dbo].[dp_Role_Pages]
           ([Role_ID],[Page_ID],[Access_Level],[Scope_All],[Approver],[File_Attacher],[Data_Importer],[Data_Exporter],[Secure_Records],[Allow_Comments],[Quick_Add])
			SELECT 104,[Page_ID],1,0,0,1,0,1,0,0,1
			FROM @OutputTbl

	  INSERT INTO [dbo].[dp_Role_Pages]
           ([Role_ID],[Page_ID],[Access_Level],[Scope_All],[Approver],[File_Attacher],[Data_Importer],[Data_Exporter],[Secure_Records],[Allow_Comments],[Quick_Add])
			SELECT 105,[Page_ID],1,0,0,0,0,0,0,0,0
			FROM @OutputTbl

	  INSERT INTO [dbo].[dp_Role_Pages]
           ([Role_ID],[Page_ID],[Access_Level],[Scope_All],[Approver],[File_Attacher],[Data_Importer],[Data_Exporter],[Secure_Records],[Allow_Comments],[Quick_Add])
			SELECT 106,[Page_ID],0,0,0,0,0,1,0,0,0
			FROM @OutputTbl

	  INSERT INTO [dbo].[dp_Role_Pages]
           ([Role_ID],[Page_ID],[Access_Level],[Scope_All],[Approver],[File_Attacher],[Data_Importer],[Data_Exporter],[Secure_Records],[Allow_Comments],[Quick_Add])
			SELECT 107,[Page_ID],3,0,0,1,0,1,1,0,0
			FROM @OutputTbl

      INSERT INTO [dbo].[dp_Role_Pages]
           ([Role_ID],[Page_ID],[Access_Level],[Scope_All],[Approver],[File_Attacher],[Data_Importer],[Data_Exporter],[Secure_Records],[Allow_Comments],[Quick_Add])
			SELECT 1009,[Page_ID],2,0,0,0,0,0,0,0,0
			FROM @OutputTbl

    -- Insert into Page Sections
	DECLARE @PageSectionID int = (SELECT Page_Section_ID FROM dbo.dp_Page_Sections WHERE Page_Section = 'Giving Statements Online/Email')
    INSERT INTO [dbo].[dp_Page_Section_Pages]
           ([Page_ID]
           ,[Page_Section_ID]
           ,[User_ID])
    SELECT [Page_ID], @PageSectionID, null
	FROM @OutputTbl
GO





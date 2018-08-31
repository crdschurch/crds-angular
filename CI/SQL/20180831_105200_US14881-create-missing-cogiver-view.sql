USE [MinistryPlatform]
GO

/****** Object:  View [dbo].[vw_crds_Missing_Cogiver]    Script Date: 8/31/2018 8:44:22 AM ******/
SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO

-- Insert into the Pages Table
INSERT INTO [dbo].[dp_Pages]
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
           ('Missing Co-giver Relationships'
           ,'Missing Co-giver Relationship'
           ,'List of contacts in the Contact_Relationships table where there are two heads of household, but only one is in the table'
           ,40
           ,'Contacts'
           ,NULL
           ,NULL
           ,'Contacts.Contact_ID as Contact_ID,
Contacts.Display_Name as Display_Name,
Spouse_Contact_ID = (SELECT TOP 1 Contact_ID FROM Contacts c2 WHERE c2.Household_ID = Contacts.Household_ID and c2.Contact_ID <> Contacts.Contact_ID AND c2.Household_Position_ID = Contacts.Household_Position_ID AND c2.Household_Position_ID = 1 and c2.Contact_Status_ID = 1 ORDER BY c2.Contact_ID DESC),
Spouse_Name = (SELECT TOP 1 Display_Name FROM Contacts c2 WHERE c2.Household_ID = Contacts.Household_ID and c2.Contact_ID <> Contacts.Contact_ID AND c2.Household_Position_ID = Contacts.Household_Position_ID AND c2.Household_Position_ID = 1 and c2.Contact_Status_ID = 1 ORDER BY c2.Contact_ID DESC)'
           ,'Contact_ID'
           ,'        Contacts.Household_Position_ID = 1
        and Contacts.Contact_Status_ID = 1    -- Active
        and exists (
            select 1 from contacts c2 where c2.Household_ID = Contacts.Household_ID and c2.Contact_ID > Contacts.Contact_ID and c2.Household_Position_ID = 1 and c2.Contact_Status_ID = 1
        )
        and not exists (
            select 1 from Contact_Relationships cr where cr.Contact_ID = Contacts.Contact_ID and cr.Relationship_ID = 42 and cr.End_Date is null
        )'
           ,NULL
           ,NULL
           ,'Contacts.Contact_ID'
           ,NULL
           ,NULL
           ,NULL
           ,NULL
           ,NULL
           ,NULL
           ,NULL
           ,1)
GO
DECLARE @Page_ID int = SCOPE_IDENTITY()


      INSERT INTO [dbo].[dp_Role_Pages]
           ([Role_ID],[Page_ID],[Access_Level],[Scope_All],[Approver],[File_Attacher],[Data_Importer],[Data_Exporter],[Secure_Records],[Allow_Comments],[Quick_Add])
			SELECT 2,@Page_ID,3,0,0,1,0,1,1,0,1


      INSERT INTO [dbo].[dp_Role_Pages]
           ([Role_ID],[Page_ID],[Access_Level],[Scope_All],[Approver],[File_Attacher],[Data_Importer],[Data_Exporter],[Secure_Records],[Allow_Comments],[Quick_Add])
			SELECT 5,@Page_ID,0,0,0,0,0,0,0,0,0

-- Insert roles
      INSERT INTO [dbo].[dp_Role_Pages]
           ([Role_ID],[Page_ID],[Access_Level],[Scope_All],[Approver],[File_Attacher],[Data_Importer],[Data_Exporter],[Secure_Records],[Allow_Comments],[Quick_Add])
			SELECT 6,@Page_ID,3,0,0,1,0,1,1,0,0
						
      INSERT INTO [dbo].[dp_Role_Pages]
           ([Role_ID],[Page_ID],[Access_Level],[Scope_All],[Approver],[File_Attacher],[Data_Importer],[Data_Exporter],[Secure_Records],[Allow_Comments],[Quick_Add])
			SELECT 7,@Page_ID,0,0,0,0,0,0,0,0,0
			
      INSERT INTO [dbo].[dp_Role_Pages]
           ([Role_ID],[Page_ID],[Access_Level],[Scope_All],[Approver],[File_Attacher],[Data_Importer],[Data_Exporter],[Secure_Records],[Allow_Comments],[Quick_Add])
			SELECT 104,@Page_ID,1,0,0,1,0,1,0,0,1

	  INSERT INTO [dbo].[dp_Role_Pages]
           ([Role_ID],[Page_ID],[Access_Level],[Scope_All],[Approver],[File_Attacher],[Data_Importer],[Data_Exporter],[Secure_Records],[Allow_Comments],[Quick_Add])
			SELECT 105,@Page_ID,1,0,0,0,0,0,0,0,0

	  INSERT INTO [dbo].[dp_Role_Pages]
           ([Role_ID],[Page_ID],[Access_Level],[Scope_All],[Approver],[File_Attacher],[Data_Importer],[Data_Exporter],[Secure_Records],[Allow_Comments],[Quick_Add])
			SELECT 106,@Page_ID,0,0,0,0,0,1,0,0,0

	  INSERT INTO [dbo].[dp_Role_Pages]
           ([Role_ID],[Page_ID],[Access_Level],[Scope_All],[Approver],[File_Attacher],[Data_Importer],[Data_Exporter],[Secure_Records],[Allow_Comments],[Quick_Add])
			SELECT 107,@Page_ID,3,0,0,1,0,1,1,0,0

      INSERT INTO [dbo].[dp_Role_Pages]
           ([Role_ID],[Page_ID],[Access_Level],[Scope_All],[Approver],[File_Attacher],[Data_Importer],[Data_Exporter],[Secure_Records],[Allow_Comments],[Quick_Add])
			SELECT 1009,@Page_ID,2,0,0,0,0,0,0,0,0

    -- Insert into Page Sections
	DECLARE @PageSectionID int = (SELECT Page_Section_ID FROM dbo.dp_Page_Sections WHERE Page_Section = 'Giving Statements')

           
INSERT INTO [dbo].[dp_Page_Section_Pages]
           ([Page_ID]
           ,[Page_Section_ID]
           ,[User_ID])
     VALUES
           (@Page_ID
           ,@PageSectionID
           ,NULL)
GO





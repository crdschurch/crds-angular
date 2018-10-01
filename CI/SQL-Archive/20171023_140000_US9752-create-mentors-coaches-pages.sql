USE [MinistryPlatform]

DECLARE @Coaches_Page_ID INT = 638;
DECLARE @Mentors_Page_ID INT = 639;
DECLARE @Staff_Role_Id INT = 100; 
DECLARE @People_List_Page_Section_ID INT = 6;

SET IDENTITY_INSERT dp_pages ON

IF NOT EXISTS(SELECT * FROM dp_pages WHERE Page_ID = @Coaches_Page_ID)
BEGIN
	INSERT INTO dp_Pages(Page_ID, Display_Name, Singular_Name, View_Order, Table_Name, Default_Field_List, Selected_Record_Expression, Primary_Key, Display_Copy)
	  VALUES(@Coaches_Page_ID,
	         'Coaches', 
			 'Coaches', 
			 700, 
			 'cr_Coaches', 
			 'Leader_Contact_ID_Table.[Display_Name] AS [Group_Leader], Coach_Contact_ID_Table.[Display_Name] AS Coach, Start_Date, End_Date', 
			 'Coach_ID',  
			 'Coach_ID',
			 1);
END

IF NOT EXISTS(SELECT * FROM dp_pages WHERE Page_ID = @Mentors_Page_ID)
BEGIN
	INSERT INTO dp_Pages(Page_ID, Display_Name, Singular_Name, View_Order, Table_Name, Default_Field_List, Selected_Record_Expression, Primary_Key, Display_Copy)
	  VALUES(@Mentors_Page_ID,
	         'Mentors', 
			 'Mentors', 
			 701, 
			 'cr_Mentors', 
			 'Coach_Contact_ID_Table.[Display_Name] AS Coach, Mentor_Contact_ID_Table.[Display_Name] AS Mentor, Start_Date, End_Date', 
			 'Mentor_ID',  
			 'Mentor_ID',
			 1);
END

SET IDENTITY_INSERT dp_pages OFF

-- security
IF NOT EXISTS(SELECT * FROM dp_role_pages WHERE Page_ID = @Coaches_Page_ID and Role_ID = @Staff_Role_Id )
BEGIN
	INSERT INTO dp_role_pages(Role_ID, Page_ID, Access_Level, Scope_All, Approver, File_Attacher, Data_Importer, Data_Exporter, Secure_Records, Allow_Comments, Quick_Add)
	            VALUES(@Staff_Role_Id, @Coaches_Page_ID, 1 , 0,0,0,0,1,0,0,1)
END

IF NOT EXISTS(SELECT * FROM dp_role_pages WHERE Page_ID = @Mentors_Page_ID and Role_ID = @Staff_Role_Id )
BEGIN
	INSERT INTO dp_role_pages(Role_ID, Page_ID, Access_Level, Scope_All, Approver, File_Attacher, Data_Importer, Data_Exporter, Secure_Records, Allow_Comments, Quick_Add)
	            VALUES(@Staff_Role_Id, @Mentors_Page_ID, 1 , 0,0,0,0,1,0,0,1)
END

-- section
IF NOT EXISTS(SELECT * FROM dp_Page_Section_Pages WHERE page_section_id = @People_List_Page_Section_ID and Page_ID = @Coaches_Page_ID )
BEGIN
	INSERT INTO dp_Page_Section_Pages(Page_ID, Page_Section_ID) VALUES(@Coaches_Page_ID,@People_List_Page_Section_ID)
END

IF NOT EXISTS(SELECT * FROM dp_Page_Section_Pages WHERE page_section_id = @People_List_Page_Section_ID and Page_ID = @Mentors_Page_ID )
BEGIN
	INSERT INTO dp_Page_Section_Pages(Page_ID, Page_Section_ID) VALUES(@Mentors_Page_ID,@People_List_Page_Section_ID)
END

GO

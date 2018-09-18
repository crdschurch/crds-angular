USE [MinistryPlatform]

DECLARE @Connect_Status_Page_ID INT = 633;

DECLARE @Page_Section_Id INT = 4;

DECLARE @Staff_Role_Id INT = 100;

SET IDENTITY_INSERT dp_pages ON

IF NOT EXISTS(SELECT * FROM dp_pages WHERE Page_ID = @Connect_Status_Page_ID)
BEGIN
	INSERT INTO dp_Pages(Page_ID, Display_Name, Singular_Name, View_Order, Table_Name, Default_Field_List, Selected_Record_Expression, Primary_Key, Display_Copy)
	  VALUES(@Connect_Status_Page_ID,
	         'Connect Communications Status', 
			 'Connect Communications Status', 
			 125, 
			 'cr_Connect_Communications_Status', 
			 'Connect_Communications_Status_ID, Communication_Status', 
			 'Communication_Status',  
			 'Connect_Communications_Status_ID',
			 1);
END

SET IDENTITY_INSERT dp_pages OFF

-- page section
IF NOT EXISTS(SELECT * FROM dp_page_section_pages WHERE Page_ID = @Connect_Status_Page_ID AND page_section_id = @Page_Section_Id )
BEGIN
	INSERT INTO dp_Page_Section_Pages(Page_ID,Page_Section_ID) VALUES(@Connect_Status_Page_ID, @Page_Section_Id);
END

-- security
IF NOT EXISTS(SELECT * FROM dp_role_pages WHERE Page_ID = @Connect_Status_Page_ID and Role_ID = @Staff_Role_Id )
BEGIN
	INSERT INTO dp_role_pages(Role_ID, Page_ID, Access_Level, Scope_All, Approver, File_Attacher, Data_Importer, Data_Exporter, Secure_Records, Allow_Comments, Quick_Add)
	            VALUES(@Staff_Role_Id, @Connect_Status_Page_ID,0, 0,0,0,0,1,0,0,1)
END

GO

USE [MinistryPlatform]

DECLARE @SRFP_Answers_Page_ID INT = 636;

DECLARE @Staff_Role_Id INT = 1015; -- FRED - CRDS

SET IDENTITY_INSERT dp_pages ON

IF NOT EXISTS(SELECT * FROM dp_pages WHERE Page_ID = @SRFP_Answers_Page_ID)
BEGIN
	INSERT INTO dp_Pages(Page_ID, Display_Name, Singular_Name, View_Order, Table_Name, Default_Field_List, Selected_Record_Expression, Primary_Key, Display_Copy)
	  VALUES(@SRFP_Answers_Page_ID,
	         'SRFP Answers', 
			 'SRFP Answers', 
			 126, 
			 'cr_SRFP_Answer_Lookup', 
			 'SRFP_Answer_Lookup_ID, Answer_Label, Answer_Value, Answer_Order', 
			 'SRFP_Answer_Lookup_ID',  
			 'SRFP_Answer_Lookup_ID',
			 1);
END

SET IDENTITY_INSERT dp_pages OFF

-- security
IF NOT EXISTS(SELECT * FROM dp_role_pages WHERE Page_ID = @SRFP_Answers_Page_ID and Role_ID = @Staff_Role_Id )
BEGIN
	INSERT INTO dp_role_pages(Role_ID, Page_ID, Access_Level, Scope_All, Approver, File_Attacher, Data_Importer, Data_Exporter, Secure_Records, Allow_Comments, Quick_Add)
	            VALUES(@Staff_Role_Id, @SRFP_Answers_Page_ID, 1 , 0,0,0,0,1,0,0,1)
END

GO

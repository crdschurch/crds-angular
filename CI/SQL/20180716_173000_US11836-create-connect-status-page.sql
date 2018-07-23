USE MinistryPlatform

DECLARE @PageID INT = 645
DECLARE @PageSectionID INT = 4; --Lookup Values

DECLARE @RoleIdSysAdmin INTEGER = 85;
DECLARE @RoleIdAllStaff INTEGER = 100;
DECLARE @RoleIdGroupManager INTEGER = 94;

IF NOT EXISTS(SELECT * FROM dp_Pages WHERE Page_ID = @PageID )
BEGIN
  SET IDENTITY_INSERT dp_Pages ON
  INSERT INTO dp_Pages(Page_ID, Display_Name, Singular_Name, View_Order, Table_Name, Default_Field_List, Selected_Record_Expression, Primary_Key, Display_Copy)
	  VALUES(@PageID,
	         'Connect Status', 
			 'Connect Status', 
			 127, 
			 'cr_Connect_Status', 
			 'Connect_Status', 
			 'Connect_Status',  
			 'Connect_Status_ID',
			 1);
  SET IDENTITY_INSERT dp_Pages OFF
END


--Page Section
IF(NOT EXISTS(SELECT * FROM dp_Page_Section_Pages WHERE Page_ID = @PageID AND Page_Section_ID = @PageSectionID))
BEGIN
	INSERT INTO dp_Page_Section_Pages(Page_ID, Page_Section_ID) VALUES(@PageID, @PageSectionID);
END

--Security
IF(NOT EXISTS(SELECT * FROM dp_Role_Pages WHERE Page_ID = @PageID AND Role_ID = @RoleIdSysAdmin))
BEGIN
	INSERT INTO dp_Role_Pages(Role_ID,Page_ID) VALUES(@RoleIdSysAdmin, @PageID);
END

IF(NOT EXISTS(SELECT * FROM dp_Role_Pages WHERE Page_ID = @PageID AND Role_ID = @RoleIdAllStaff))
BEGIN
	INSERT INTO dp_Role_Pages(Role_ID,Page_ID) VALUES(@RoleIdAllStaff, @PageID);
END

IF(NOT EXISTS(SELECT * FROM dp_Role_Pages WHERE Page_ID = @PageID AND Role_ID = @RoleIdGroupManager))
BEGIN
	INSERT INTO dp_Role_Pages(Role_ID,Page_ID) VALUES(@RoleIdGroupManager, @PageID);
END
GO

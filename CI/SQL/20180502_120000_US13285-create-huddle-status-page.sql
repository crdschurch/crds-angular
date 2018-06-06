USE MinistryPlatform

DECLARE @PageID INT = 644

DECLARE @RoleIdSysAdmin INTEGER = 85;
DECLARE @RoleIdAllStaff INTEGER = 100;
DECLARE @RoleIdGroupManager INTEGER = 94;

IF NOT EXISTS(SELECT * FROM dp_Pages WHERE Page_ID = @PageID )
BEGIN
  SET IDENTITY_INSERT dp_Pages ON
  INSERT INTO dp_Pages(Page_ID, Display_Name, Singular_Name, View_Order, Table_Name, Default_Field_List, Selected_Record_Expression, Primary_Key, Display_Copy)
	  VALUES(@PageID,
	         'Huddle Status', 
			 'Huddle Status', 
			 127, 
			 'cr_Huddle_Status', 
			 'Huddle_Status', 
			 'Huddle_Status',  
			 'Huddle_Status_ID',
			 1);
  SET IDENTITY_INSERT dp_Pages OFF
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

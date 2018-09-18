USE MinistryPlatform

DECLARE @PageID INT = 647
DECLARE @PageSectionID INT = 4; --Lookup Values

DECLARE @RoleunauthenticatedCreate INTEGER = 62;

IF NOT EXISTS(SELECT * FROM dp_Pages WHERE Page_ID = @PageID )
BEGIN
  SET IDENTITY_INSERT dp_Pages ON
  INSERT INTO dp_Pages(Page_ID, Display_Name, Singular_Name, View_Order, Table_Name, Default_Field_List, Selected_Record_Expression, Primary_Key, Display_Copy)
	  VALUES(@PageID,
	         'Map Audit', 
			 'Map Audit', 
			 9999, 
			 'cr_MapAudit', 
			 'AuditID, Participant_ID,ShowOnMap,Processed,DateProcessed', 
			 'AuditID',  
			 'AuditID',
			 1);
  SET IDENTITY_INSERT dp_Pages OFF
END


--Page Section
IF(NOT EXISTS(SELECT * FROM dp_Page_Section_Pages WHERE Page_ID = @PageID AND Page_Section_ID = @PageSectionID))
BEGIN
	INSERT INTO dp_Page_Section_Pages(Page_ID, Page_Section_ID) VALUES(@PageID, @PageSectionID);
END

--Security
IF(NOT EXISTS(SELECT * FROM dp_Role_Pages WHERE Page_ID = @PageID AND Role_ID = @RoleunauthenticatedCreate))
BEGIN
	INSERT INTO dp_Role_Pages(Role_ID,Page_ID, Access_Level) VALUES(@RoleunauthenticatedCreate, @PageID, 1);
END

GO

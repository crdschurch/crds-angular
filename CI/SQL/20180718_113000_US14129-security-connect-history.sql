USE MinistryPlatform

DECLARE @PageID INT = 646

DECLARE @RoleIdUnauthenticatedCreate INTEGER = 62;


--Security
IF(NOT EXISTS(SELECT * FROM dp_Role_Pages WHERE Page_ID = @PageID AND Role_ID = @RoleIdUnauthenticatedCreate))
BEGIN
	INSERT INTO dp_Role_Pages(Role_ID,Page_ID, Access_Level) VALUES(@RoleIdUnauthenticatedCreate, @PageID, 3);
END

GO

-- This is a Think Ministry stored proc that is called once each time a user
-- logs in to MP or CR.net
--
-- Modified version of dp_Authorize_User that is signficantly faster because
-- it uses the IX_dp_User_Roles__UserID_DomainID_RoleID index on dp_User_Roles:

USE [MinistryPlatform]
GO

ALTER PROCEDURE [dbo].[dp_Authorize_User]
	@UserId int,
	@DomainId int
AS
	-- Retrieve user details
	SELECT
		U.[User_ID]
		,U.[User_Name]
		,U.Contact_ID
		,U.User_GUID
		,C.Display_Name
		,C.Email_Address
		,C.Contact_GUID
		,U.Setup_Admin
		,U.Admin
		,U.Time_Zone
		,U.Locale
		,U.Theme
		,U.Can_Impersonate
		,(SELECT COUNT(Role_ID) FROM dp_User_Roles WHERE [User_ID] = @UserId AND Domain_ID = @DomainId) as Roles_Count
		,U.Read_Permitted
		,U.Create_Permitted
		,U.Update_Permitted
		,U.Delete_Permitted
		,(SELECT MAX(ISNULL(CAST(r.Mass_Email_Quota as int), 0)) FROM dp_Roles r 
			INNER JOIN dp_User_Roles ur ON ur.Role_ID = r.Role_ID AND ur.Domain_ID = r.Domain_ID
			AND ur.[User_ID] = @UserId AND ur.Domain_ID = @DomainId) as Max_Messages
	FROM dp_Users U
	INNER JOIN Contacts C ON C.Contact_ID = U.Contact_ID
	WHERE U.[User_ID] = @UserId AND U.Domain_ID = @DomainId

	-- Retrieve "All Users Role" identifier
	DECLARE @UsersRoleId int;

	SELECT TOP 1 @UsersRoleId = Role_ID
	FROM dp_Roles
	WHERE Domain_ID = @DomainId AND Role_Name = 'All Platform Users';

	-- Retrieve authorized pages	
	SELECT
		RP.Page_ID
		,MAX(CASE RP.Access_Level WHEN 9 THEN -1 ELSE RP.Access_Level END) AS Access_Level
		,MAX(RP.File_Attacher) AS Can_Attach
		,MAX(RP.Data_Importer) AS Can_Import
		,MAX(RP.Data_Exporter) AS Can_Export
		,MAX(RP.Secure_Records) AS Can_Secure
		,MAX(RP.Allow_Comments) AS Can_Comment
		,MAX(RP.Quick_Add) AS On_Quick_Add
	FROM
		dp_Role_Pages RP
	WHERE
		RP.Role_ID = @UsersRoleId
		OR RP.Role_ID IN (SELECT Role_ID FROM dp_User_Roles WHERE [User_ID] = @UserID)
	GROUP BY
		RP.Page_ID
	;

	-- Retrieve restricted columns
	SELECT
		FR.Table_Name,
		FR.Field_Name,
		MAX(FR.Restriction_Level) AS Restriction_Level
	FROM dp_Field_Restrictions FR
	INNER JOIN dp_User_Roles UR ON FR.Role_ID = UR.Role_ID
	WHERE UR.[User_ID] = @UserID
	GROUP BY
		FR.Table_Name,
		FR.Field_Name
	
	-- Retrieve authorized sub-pages
	SELECT
		SP.Sub_Page_ID
		,SP.Page_ID
		,ISNULL(MAX(CASE RSP.Access_Level WHEN 9 THEN -1 ELSE RSP.Access_Level END), 0) AS Access_Level
	FROM
		dp_Role_Sub_Pages RSP
		INNER JOIN dp_Sub_Pages SP ON SP.Sub_Page_ID = RSP.Sub_Page_ID
	WHERE
		RSP.Role_ID = @UsersRoleId
		OR RSP.Role_ID IN (SELECT Role_ID FROM dp_User_Roles WHERE [User_ID] = @UserID)
	GROUP BY
		SP.Sub_Page_ID,
		SP.Page_ID
	;

	-- Retrieve authorized tools
	SELECT DISTINCT
		Tool_ID
	FROM
		dp_Role_Tools
	WHERE
		Role_ID = @UsersRoleId
		OR Role_ID IN (SELECT Role_ID FROM dp_User_Roles WHERE [User_ID] = @UserID)
	;
	
	-- Retrieve authorized reports
	SELECT DISTINCT
		Report_ID
	FROM
		dp_Role_Reports
	WHERE
		Role_ID = @UsersRoleId
		OR Role_ID IN (SELECT Role_ID FROM dp_User_Roles WHERE [User_ID] = @UserID)
	;

	-- Retrieve page views
	SELECT
		Page_View_ID as View_ID
		,View_Title
		,Page_ID
		,[Description]
		,Field_List
		,View_Clause
		,Order_By
		,[User_ID]
		,User_Group_ID
	FROM dp_Page_Views
	WHERE
		([User_ID] IS NULL AND User_Group_ID IS NULL)
		OR [User_ID] = @UserId
		OR User_Group_ID IN (SELECT User_Group_ID FROM dp_User_User_Groups WHERE [User_ID] = @UserId);

	-- Retrieve sub-page views
	SELECT
		Sub_Page_View_ID as View_ID
		,View_Title
		,Sub_Page_ID as Page_ID
		,[Description]
		,Field_List
		,View_Clause
		,Order_By
		,[User_ID]
	FROM dp_Sub_Page_Views
	WHERE
		[User_ID] IS NULL
		OR [User_ID] = @UserId;
	
	-- Retrieve authorized tables
	SELECT
		P.Table_Name
		,P.Filter_Clause
		,MAX(RP.Access_Level) AS Access_Level
		,MAX(RP.File_Attacher) AS Can_Attach
		,MAX(RP.Data_Importer) AS Can_Import
		,MAX(RP.Data_Exporter) AS Can_Export
		,MAX(RP.Secure_Records) AS Can_Secure
		,MAX(RP.Allow_Comments) AS Can_Comment
	FROM dp_Role_Pages RP
	INNER JOIN dp_Pages P ON P.Page_ID = RP.Page_ID
	INNER JOIN dp_User_Roles UR ON UR.Role_ID = RP.Role_ID 
	WHERE UR.[User_ID] = @UserId AND RP.Access_Level < 9
	GROUP BY P.Table_Name, P.Filter_Clause
	UNION
	SELECT
		o.name,
		NULL,
		CAST(0 as tinyint),
		CAST(0 as tinyint),
		CAST(0 as tinyint),
		CAST(0 as tinyint),
		CAST(0 as tinyint),
		CAST(0 as tinyint)
	FROM sys.objects o
	WHERE
		o.type in ('U', 'V') 
		AND o.name NOT LIKE 'dp!_%' ESCAPE '!' 
		AND o.name NOT LIKE 'vw!_dp!_%' ESCAPE '!' 
		AND o.name NOT IN 
		(
			SELECT	P.Table_Name FROM dp_Role_Pages RP
			INNER JOIN dp_Pages P ON P.Page_ID = RP.Page_ID
			INNER JOIN dp_User_Roles UR ON UR.Role_ID = RP.Role_ID 
			WHERE UR.[User_ID] = @UserId
		)
		AND NOT EXISTS (SELECT 1 FROM sys.columns c WHERE c.[object_id] = o.[object_id] AND c.name='Domain_ID')
	ORDER BY P.Table_Name, P.Filter_Clause;

	-- Retrieve authorized routines
	SELECT DISTINCT
		API_Procedure_ID
	FROM
		dp_Role_API_Procedures
	WHERE
		Role_ID = @UsersRoleId
		OR Role_ID IN (SELECT Role_ID FROM dp_User_Roles WHERE [User_ID] = @UserID)
	;

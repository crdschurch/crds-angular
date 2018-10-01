USE [MinistryPlatform];
GO

IF NOT EXISTS (
    SELECT 1
    FROM sys.indexes 
    WHERE object_id = OBJECT_ID('dbo.dp_Authentication_Log')
    AND name='IX_dp_Authentication_Log_User_ID')

    CREATE NONCLUSTERED INDEX [IX_dp_Authentication_Log_User_ID] ON [dbo].[dp_Authentication_Log]
    (
        [User_ID] ASC
    ) WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, SORT_IN_TEMPDB = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, FILLFACTOR = 95) ON [PRIMARY]

GO

DECLARE @PageViewID INT = 1141;

IF NOT EXISTS(SELECT * FROM dp_Page_Views WHERE Page_View_ID = @PageViewID)
BEGIN
	SET IDENTITY_INSERT dp_Page_Views ON
    DECLARE @PageId INT = 401;
    DECLARE @PageViewTitle VARCHAR(50) = 'Users'' Last Login To MP Admin With Security Roles';
    DECLARE @FieldList VARCHAR(2000) = 'dp_Users.Contact_ID as "Contact ID",
    dp_Users.User_ID as "User ID",
    dp_Users.Display_Name as "Display Name",
    dp_Users.User_Name as "User Name",
    (SELECT Max(Date_Time) 
        FROM dp_Authentication_Log AL 
        WHERE AL.User_ID = dp_Users.User_ID
            AND AL.Referrer = ''Platform.web'') AS "Last Login",
    (select top 1
                r.Role_Name
            from
                dp_user_roles ur
                inner join dp_Roles r on r.Role_ID = ur.Role_ID
            where
                ur.User_ID = dp_Users.User_ID and ur.Role_ID <> 39
            order by
                ur.User_Role_ID
    ) as "Security Role",
    (
        select top 1
            al.Date_Time
        from
            dp_user_roles ur
            inner join dp_Audit_Log al on al.Table_Name = ''dp_User_Roles'' and al.Record_ID = ur.User_Role_ID
        where
            ur.User_ID = dp_Users.User_ID and ur.Role_ID <> 39
        order by
            al.Date_Time asc
    ) as "Role Create Date"';
    DECLARE @ViewClause VARCHAR(1000) = 'dp_Users.User_Email NOT LIKE ''%@thinkministry.com'' 
    AND EXISTS (SELECT 1 FROM dp_User_Roles UR WHERE UR.User_ID = dp_Users.User_ID AND UR.Role_ID <> 39)
    ';
    DECLARE @Description VARCHAR(1000) = 'Returns a list of users with security roles other than All Platform Users and the last time that user logged in.';

    INSERT INTO [dbo].[dp_Page_Views]
        ( [Page_View_ID],
            [View_Title],
            [Page_ID],
            [Field_List],
            [View_Clause],
            [Description]
        )
    VALUES( @PageViewID, @PageViewTitle, @PageId, @FieldList, @ViewClause, @Description );

	SET IDENTITY_INSERT dp_Page_Views OFF
END
GO



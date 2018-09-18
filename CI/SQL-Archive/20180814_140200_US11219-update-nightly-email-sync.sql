USE [MinistryPlatform]
GO

CREATE OR ALTER PROCEDURE [dbo].[crds_service_update_email_nightly] 
AS
BEGIN
    DECLARE @StartDate DATETIME = DATEADD(DAY, -1, CAST(GETDATE() AS DATE)) --yesterday at midnight

    -- Find changes to Contacts.Email_Address and try to make dp_Users.User_Name match if possible
    DECLARE @ContactChanges TABLE (
       User_ID int,
       Email_Address nvarchar(255), 
       Old_Email nvarchar(255),
       Old_Login nvarchar(255),
       Changed_By nvarchar(254),
       Changed_When datetime,
       OkToUpdate bit
    );

    -- Query audit log for changes to Contact Email_Address during the past 24 hours
    -- If there are multiple entries for the same Contact, take only the last/latest.
    -- Skip changes where the resulting Contact Email_Address is empty/null.
    WITH Contact_Audit (Record_ID, Changed_By, Changed_When, Row_Num)
    AS (
        SELECT
            l.Record_ID,
            l.User_Name,
            l.Date_Time,
            Row_Num = ROW_NUMBER() OVER (PARTITION BY l.Record_ID ORDER BY l.Audit_Item_ID DESC)
        FROM
            dp_Audit_Log l 
            INNER JOIN dp_Audit_Detail d ON d.Audit_Item_ID = l.Audit_Item_ID
        WHERE
            l.Table_Name = 'Contacts'
            AND l.Date_Time > @StartDate
            AND l.Audit_Description = 'Updated'
            AND d.field_name = 'Email_Address' 
    )
    INSERT INTO @ContactChanges
    SELECT
        u.User_ID,
        c.Email_Address,
        u.User_Email,
        u.User_Name,
        ca.Changed_By,
        ca.Changed_When,
        0
    FROM
        Contact_Audit ca
        INNER JOIN Contacts c ON c.Contact_ID = ca.Record_ID
        INNER JOIN dp_Users u ON u.User_ID = c.User_Account
    WHERE
        Row_Num = 1
        AND LEN(COALESCE(c.Email_Address, '')) > 0
        AND (c.Email_Address <> u.User_Email OR c.Email_Address <> u.User_Name)
    ;

    --CHECK TO SEE IF THERE IS ALREADY A USER ACCOUNT WITH THE NEW EMAIL
	UPDATE @ContactChanges 
	SET OkToUpdate = 1
	FROM @ContactChanges c
	LEFT JOIN dp_Users u ON c.Email_Address = u.User_Name
	WHERE u.User_ID IS NULL 

	--Update the users we can

		DECLARE 
		 @AuditItemID INT      --SETS to 0
		,@UserName Varchar(50) --SETS to 'Svc Mngr'
		,@UserID INT			--SETS to 0
		,@TableName Varchar(50)
		,@Update_ID  INT
		,@CurrentEmail NVARCHAR(max)
		,@CurrentLogin NVARCHAR(max)
		,@New NVARCHAR(max)

		SET @AuditItemID = 0
		SET @UserName = 'Svc Mngr'
		SET @UserID = 0
		SET @TableName = 'dp_Users'

		DECLARE CursorPUTT CURSOR FAST_FORWARD FOR
		SELECT User_ID,Email_Address, Old_Email, Old_Login FROM @ContactChanges WHERE OkToUpdate = 1

		OPEN CursorPUTT
		FETCH NEXT FROM CursorPUTT INTO @Update_ID, @New, @CurrentEmail, @CurrentLogin
			WHILE @@FETCH_STATUS = 0
			BEGIN

		
				UPDATE dp_Users 
				SET [User_Email] = @New, 
					[User_Name]  = @New 
				WHERE User_ID = @Update_ID 		
			
				--Audit Log the Change
				EXEC [dbo].[crds_Add_Audit] 
					 @TableName 
					,@Update_ID
					,'Mass Updated'
					,@UserName
					,@UserID
					,'User_Email'
					,'User Email'
					,@CurrentEmail
					,@New

				 EXEC [dbo].[crds_Add_Audit] 
					 @TableName 
					,@Update_ID
					,'Mass Updated'
					,@UserName
					,@UserID
					,'User_Name'
					,'User Name'
					,@CurrentLogin
					,@New
	
				FETCH NEXT FROM CursorPUTT INTO @Update_ID, @New, @CurrentEmail, @CurrentLogin
			
			END
		CLOSE CursorPUTT
		DEALLOCATE CursorPUTT

	--Email about the users we can't update
	IF (SELECT count(*) FROM  @ContactChanges WHERE OkToUpdate = 0) > 0
	BEGIN

	DECLARE @xml NVARCHAR(MAX)
	DECLARE @body NVARCHAR(MAX)

	SET @xml = CAST(( SELECT [User_ID] AS 'td','',[Email_Address] AS 'td','',
		   [Changed_By] AS 'td','', Changed_When AS 'td'
	FROM  @ContactChanges 
	WHERE OkToUpdate = 0
	FOR XML PATH('tr'), ELEMENTS ) AS NVARCHAR(MAX))

	SET @body ='<html><body><H3>Unable to update user with new contact email because the login is already in use.</H3>
	<table border = 1> 
	<tr>
	<th> User ID </th> <th> New Contact Email </th> <th> Changed By </th> <th> Changed Date </th></tr>'    

	SET @body = @body + @xml +'</table></body></html>'

	EXEC msdb.dbo.sp_send_dbmail
	@profile_name = 'MinistryPlatform',
	@recipients = 'accounts@crossroads.net',
	@subject = 'Update_Email_Nightly: Unable to update user id with new Contact email',
	@body = @body,
	@body_format ='HTML'

	END

	--Changes to dp_user user_name should propogate to contact
    DECLARE @LoginChanges TABLE (
       Contact_ID int,
       Email_Address nvarchar(255), 
       Old_Email nvarchar(255)
    );

    WITH User_Audit (Record_ID, Row_Num)
    AS (
        SELECT
            l.Record_ID,
            Row_Num = ROW_NUMBER() OVER (PARTITION BY l.Record_ID ORDER BY l.Audit_Item_ID DESC)
        FROM
            dp_Audit_Log l 
            INNER JOIN dp_Audit_Detail d ON d.Audit_Item_ID = l.Audit_Item_ID
        WHERE
            l.Table_Name = 'dp_Users'
            AND l.Date_Time > @StartDate
            AND l.Audit_Description = 'Updated'
            AND d.field_name = 'User_Name' 
    )
    INSERT INTO @LoginChanges
    SELECT
        c.Contact_ID, u.User_Name, c.Email_Address
    FROM
        User_Audit ua
        INNER JOIN dp_Users u ON u.User_ID = ua.Record_ID
        INNER JOIN Contacts c ON c.User_Account = u.User_ID
    WHERE
        Row_Num = 1
        AND COALESCE(c.Email_Address, '') <> u.User_Name
    ;

	SET @TableName = 'Contacts'

	DECLARE CursorPUTT CURSOR FAST_FORWARD FOR
	SELECT Contact_ID ,Email_Address, Old_Email FROM @LoginChanges 

	OPEN CursorPUTT
	FETCH NEXT FROM CursorPUTT INTO @Update_ID, @New, @CurrentEmail
		WHILE @@FETCH_STATUS = 0
		BEGIN

		
			UPDATE Contacts 
			SET [Email_Address] = @New
			WHERE Contact_ID = @Update_ID 		
			
			--Audit Log the Change
			EXEC [dbo].[crds_Add_Audit] 
					@TableName 
				,@Update_ID
				,'Mass Updated'
				,@UserName
				,@UserID
				,'Email_Address'
				,'Email Address'
				,@CurrentEmail
				,@New

				
			FETCH NEXT FROM CursorPUTT INTO @Update_ID, @New, @CurrentEmail
			
		END
	CLOSE CursorPUTT
	DEALLOCATE CursorPUTT

END
GO

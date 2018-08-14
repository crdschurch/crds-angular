USE [MinistryPlatform]
GO

CREATE PROCEDURE [dbo].[crds_service_update_email_nightly] 
AS
BEGIN

	IF OBJECT_ID('tempdb..#ContactChanges') IS NOT NULL
	  /*Then it exists*/
	  DROP TABLE #ContactChanges
  
  
	CREATE TABLE #ContactChanges
	(
	   User_ID int,
	   Email_Address nvarchar(max), 
	   Old_Email nvarchar(max),
	   Old_Login nvarchar(max),
	   Changed_By nvarchar(254),
	   Changed_When datetime,
	   OkToUpdate bit
	)

	INSERT INTO #ContactChanges
	SELECT User_Account, c.Email_Address, u.User_Email, u.User_Name, l.User_Name, Date_Time, 0
	FROM dp_audit_log l 
	INNER JOIN dp_audit_detail d ON l.Audit_Item_ID = d.Audit_Item_ID
	INNER JOIN Contacts c ON l.record_Id = c.contact_id
	INNER JOIN dp_Users u ON u.user_id = c.user_account
	WHERE 
		  d.field_name = 'Email_Address' 
	  AND l.Table_Name = 'Contacts'
	  AND l.Audit_Description = 'Updated'
	  AND l.Date_Time > DATEADD(day,-1, cast(GETDATE() as date)) --yesterday at midnight
	  AND (c.Email_Address <> u.User_Email OR c.Email_Address <> u.User_Name)
	  AND c.Email_Address IS NOT NULL

	  	--CHECK TO SEE IF THERE IS ALREADY A USER ACCOUNT WITH THE NEW EMAIL
	UPDATE #ContactChanges 
	SET OkToUpdate = 1
	FROM #ContactChanges c
	LEFT JOIN dp_Users u ON c.Email_Address = u.User_Name
	WHERE u.User_ID IS NULL 

	--Update the users we can

		DECLARE 
		 @AuditItemID INT      --SETS to 0
		,@UserName Varchar(50) --SETS to 'Svc Mngr'
		,@UserID INT			--SETS to 0
		,@TableName Varchar(50) --SETS to 'Group_Participants'
		,@Update_ID  INT
		,@CurrentEmail NVARCHAR(max)
		,@CurrentLogin NVARCHAR(max)
		,@New NVARCHAR(max)

		SET @AuditItemID = 0
		SET @UserName = 'Svc Mngr'
		SET @UserID = 0
		SET @TableName = 'dp_Users'

		DECLARE CursorPUTT CURSOR FAST_FORWARD FOR
		SELECT User_ID,Email_Address, Old_Email, Old_Login FROM #ContactChanges WHERE OkToUpdate = 1

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
	IF (SELECT count(*) FROM  #ContactChanges WHERE OkToUpdate = 0) > 0
	BEGIN

	DECLARE @xml NVARCHAR(MAX)
	DECLARE @body NVARCHAR(MAX)

	SET @xml = CAST(( SELECT [User_ID] AS 'td','',[Email_Address] AS 'td','',
		   [Changed_By] AS 'td','', Changed_When AS 'td'
	FROM  #ContactChanges 
	WHERE OkToUpdate = 0
	FOR XML PATH('tr'), ELEMENTS ) AS NVARCHAR(MAX))

	SET @body ='<html><body><H3>Unable to update user with new contact email because the login is already in use.</H3>
	<table border = 1> 
	<tr>
	<th> User ID </th> <th> New Contact Email </th> <th> Changed By </th> <th> Changed Date </th></tr>'    

	SET @body = @body + @xml +'</table></body></html>'

	EXEC msdb.dbo.sp_send_dbmail
	@profile_name = 'MinistryPlatform',
	@recipients = 'katie.dwyer@ingagepartners.com; drye@crossroads.net; mike.fuhrman@crossroads.net', --mike fuhrman, dan rye
	@subject = 'Unable to update user id with new Contact email',
	@body = @body,
	@body_format ='HTML'

	END

	--Changes to dp_user user_name should propogate to contact
	IF OBJECT_ID('tempdb..#LoginChanges') IS NOT NULL
	  /*Then it exists*/
	  DROP TABLE #LoginChanges

	CREATE TABLE #LoginChanges
	(
	   Contact_ID int,
	   Email_Address nvarchar(max), 
	   Old_Email nvarchar(max)
	)

	INSERT INTO #LoginChanges
	SELECT c.Contact_ID, u.User_Name, c.Email_Address
	FROM dp_audit_log l 
	INNER JOIN dp_audit_detail d ON l.Audit_Item_ID = d.Audit_Item_ID	
	INNER JOIN dp_Users u ON u.User_ID = l.record_Id 
	INNER JOIN Contacts c ON c.user_account = u.User_ID
	WHERE 
	  d.field_name = 'User_Name' 
	  AND l.Table_Name = 'dp_Users'
	  AND l.Audit_Description = 'Updated'
	  AND l.Date_Time > DATEADD(day,-1, cast(GETDATE() as date)) --yesterday at midnight
	  AND (c.Email_Address <> u.User_Name OR c.Email_Address IS NULL) --apparently we can't use scalar operators on NULL values
	GROUP BY c.Contact_ID, u.User_Name, c.Email_Address  --only do it once


	SET @TableName = 'Contacts'

	DECLARE CursorPUTT CURSOR FAST_FORWARD FOR
	SELECT Contact_ID ,Email_Address, Old_Email FROM #LoginChanges 

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

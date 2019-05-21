USE [MinistryPlatform]
GO

DECLARE @UserID int = 1004;
DECLARE @ContactID int = 55;
DECLARE @Email char(32) = 'webteam+directemail@crossroads.net';

IF NOT EXISTS(SELECT 1 FROM [dbo].[Contacts] WHERE Contact_ID = @ContactID)
BEGIN
        SET IDENTITY_INSERT [dbo].[Contacts] ON
        INSERT INTO [dbo].[Contacts]
            ([Contact_ID]
            ,[Company]
            ,[Display_Name]
            ,[Contact_Status_ID]
            ,[Email_Address]
            ,[Bulk_Email_Opt_Out]
            ,[Bulk_SMS_Opt_Out]
            ,[Domain_ID])
     VALUES
            (@ContactID
            ,0
            ,'Direct Email Contact'
            ,1
            ,@Email
            ,1
            ,1
            ,1)
        SET IDENTITY_INSERT [dbo].[Contacts] OFF
END

IF NOT EXISTS(SELECT 1 FROM [dbo].[dp_Users] WHERE User_ID = @UserID)
BEGIN
        SET IDENTITY_INSERT [dbo].[dp_Users] ON
        INSERT INTO [dbo].[dp_Users]
            ([User_ID]
            ,[User_Name]
            ,[User_Email]
            ,[Display_Name]
            ,[Admin]
            ,[Domain_ID]
            ,[Publications_Manager]
            ,[Contact_ID]
            ,[Can_Impersonate]
            ,[Keep_For_Go_Live]
            ,[Read_Permitted]
            ,[Create_Permitted]
            ,[Update_Permitted]
            ,[Delete_Permitted])
     VALUES
            (@UserID
            ,@Email
            ,@Email
            ,'Direct Email User'
            ,0
            ,1
            ,0
            ,@ContactID
            ,1
            ,1
            ,0
            ,0
            ,0
            ,0)
        SET IDENTITY_INSERT [dbo].[dp_Users] OFF
END

INSERT INTO dp_API_Clients
	(Domain_ID, Display_Name, Client_ID, Client_User_ID)
VALUES
	(1, 'CRDS Direct Email Client', 'CRDS.DirectEmail', @UserID)
;

-- add roles: UnauthenticatedCreate, Email Quota 5000
IF NOT EXISTS (SELECT 1 FROM [dbo].[dp_user_roles]
			   WHERE USER_ID = (SELECT USER_ID FROM dp_users WHERE user_name = @Email)
			   AND ROLE_ID = (SELECT ROLE_ID FROM dp_roles WHERE ROLE_NAME = 'unauthenticatedCreate'))
BEGIN
	INSERT INTO dp_user_roles
	(User_ID                                                     , Role_ID                                                      , DOMAIN_ID) VALUES
	((SELECT USER_ID from dp_users where user_name = @Email), (select role_id from dp_roles where role_name = 'unauthenticatedCreate'), 1        );
END

IF NOT EXISTS (SELECT 1 FROM [dbo].[dp_user_roles]
			   WHERE USER_ID = (SELECT USER_ID FROM dp_users WHERE user_name = @Email)
			   AND ROLE_ID = (SELECT ROLE_ID FROM dp_roles WHERE ROLE_NAME = 'Email Quota 5000'))
BEGIN
	INSERT INTO dp_user_roles
	(User_ID                                                     , Role_ID                                                      , DOMAIN_ID) VALUES
	((SELECT USER_ID from dp_users where user_name = @Email), (select role_id from dp_roles where role_name = 'Email Quota 5000'), 1        );
END
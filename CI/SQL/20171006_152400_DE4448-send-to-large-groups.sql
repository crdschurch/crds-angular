USE MinistryPlatform
GO

DECLARE @api_user_id INT;
DECLARE @role_id INT = 13; -- email quota 5000

-- get the api user
SELECT @api_user_id = u.User_ID
FROM [dbo].[dp_Users] u
WHERE u.User_Name = 'register_api' and u.User_Email = 'updates@crossroads.net'

IF NOT Exists(SELECT User_ID FROM [dbo].[dp_User_Roles] WHERE User_ID = @api_user_id AND Role_ID = @role_id)
BEGIN
	INSERT INTO [dbo].[dp_User_Roles] ([User_ID], [Role_ID], [Domain_ID]) VALUES (@api_user_id, @role_id, 1)
END

USE [MinistryPlatform]
GO

-- Get James contact ID
DECLARE @contactID as int
set @contactID = (select contact_id from contacts where Email_Address = 'mpcrds+auto+captainslow@gmail.com' and Last_Name = 'May');

--- Add the KC Admin role to James May
DECLARE @userID as int
set @userID = (select user_account from contacts where contact_id= @contactID);

INSERT INTO [dbo].[dp_user_roles]
(User_ID,Role_ID,Domain_ID) VALUES
(@userId,112      ,1        ); --Kids Club Tools

INSERT INTO [dbo].[dp_user_roles]
(User_ID,Role_ID,Domain_ID) VALUES
(@userId,92      ,1        ); --Contact Administrator
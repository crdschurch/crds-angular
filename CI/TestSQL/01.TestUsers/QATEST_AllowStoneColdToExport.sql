USE [MinistryPlatform]
GO

-- Get Stone Cold's contact ID
DECLARE @contactID as int
set @contactID = (select contact_id from contacts where Email_Address = 'mpcrds+auto+stone+cold@gmail.com' and Last_Name = 'Austin');

--- Add the right roles to Stone Cold Steve Austin
DECLARE @userID as int
set @userID = (select user_account from contacts where contact_id= @contactID);

INSERT INTO [dbo].dp_user_roles 
(User_ID,Role_ID,Domain_ID) VALUES
(@userId,105      ,1        );

INSERT INTO [dbo].dp_user_roles 
(User_ID,Role_ID,Domain_ID) VALUES
(@userId,111      ,1        );

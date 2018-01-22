USE [MinistryPlatform]
GO

-- Get Stone Cold's contact ID
DECLARE @userID as int
set @userID = (select User_ID from [dbo].[dp_Users] where User_Name = 'mpcrds+auto+stone+cold@gmail.com');

INSERT INTO [dbo].[dp_user_roles] 
(User_ID,Role_ID,Domain_ID) VALUES
(@userId,105      ,1        ); --Finance Donation MGR

INSERT INTO [dbo].[dp_user_roles] 
(User_ID,Role_ID,Domain_ID) VALUES
(@userId,111      ,1        ); --Finance Tools

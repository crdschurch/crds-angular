USE [MinistryPlatform]
GO

-- Get Stone Cold's contact ID
DECLARE @userID as int
set @userID = (select User_ID from [dbo].[dp_Users] where User_Email = 'mpcrds+auto+stone+cold@gmail.com' and Display_Name = 'Austin');

INSERT INTO [dbo].[dp_user_roles] 
(User_ID,Role_ID,Domain_ID) VALUES
(@userId,105      ,1        );

INSERT INTO [dbo].[dp_user_roles] 
(User_ID,Role_ID,Domain_ID) VALUES
(@userId,111      ,1        );

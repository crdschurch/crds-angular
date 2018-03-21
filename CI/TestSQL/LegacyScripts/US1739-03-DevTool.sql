USE [MinistryPlatform]
GO
SET IDENTITY_INSERT [dbo].[dp_Tools] ON

INSERT INTO [dbo].[dp_Tools]
           ([Tool_ID]
		   ,[Tool_Name]
           ,[Description]
           ,[Launch_Page])
     VALUES
           (43
		   ,'Trip Participants - Developmen'
           ,'Development Trip Participants Tool'
           ,'http://localhost:3000/mptools/tripParticipants')

SET IDENTITY_INSERT [dbo].[dp_Tools] OFF
GO
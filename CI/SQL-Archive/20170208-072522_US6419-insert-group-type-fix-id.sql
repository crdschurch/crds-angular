USE [MinistryPlatform]
GO

IF EXISTS (SELECT * 
                 FROM [dbo].[Group_Types]
                 WHERE Group_Type = N'Anywhere Gathering'
				 AND Group_Type_ID != 30
                 )
BEGIN
  DELETE FROM [dbo].[Group_Types]
         WHERE Group_Type = N'Anywhere Gathering'		
END

IF NOT EXISTS (SELECT * 
                 FROM [dbo].[Group_Types]
                 WHERE Group_Type = N'Anywhere Gathering'
				 AND [Group_Type_ID] = 30
                 )
BEGIN
  SET IDENTITY_INSERT [dbo].[Group_Types] ON

  INSERT INTO [dbo].[Group_Types]
		  ( [Group_Type_ID]
		  ,[Group_Type]
		  ,[Description]
		  ,[Domain_ID]
		  ,[Default_Role])
      VALUES
		  ( 30
		  ,N'Anywhere Gathering'
		  ,N'Groups created, administered, and led by an approved Host in the Anywhere community'
		  ,1
		  ,16);

  SET IDENTITY_INSERT [dbo].[Group_Types] OFF
END




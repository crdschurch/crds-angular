USE [MinistryPlatform]
GO

INSERT INTO [dbo].[dp_Users]
           ([User_Name]
           ,[User_Email]
           ,[Display_Name]
           ,[Password]
           ,[Admin]
           ,[Domain_ID]
           ,[Publications_Manager]
           ,[Contact_ID]
           ,[Supervisor]
           ,[User_GUID]
           ,[Can_Impersonate]
           ,[In_Recovery]
           ,[Keep_For_Go_Live]
           ,[Time_Zone]
           ,[Locale]
           ,[Theme]
           ,[Setup_Admin]
           ,[__ExternalPersonID]
           ,[__ExternalUserID]
           ,[Data_Service_Permissions]
           ,[Read_Permitted]
           ,[Create_Permitted]
           ,[Update_Permitted]
           ,[Delete_Permitted]
           ,[PasswordResetToken]
           ,[Login_Attempts])
     VALUES
           ('giving@crossroads.net'
           ,'giving@crossroads.net'
           ,'Giving'
           , 0x73AA4D928FF3D260C1CDA474A3967256
           ,0
           ,1
           ,0
           ,5396574
           ,NULL
           , '0AE9160A-D53D-4D8B-9508-488B15200828'
           ,NULL
           ,NULL
           ,0
           ,NULL
           ,NULL
           ,NULL
           ,0
           ,NULL
           ,NULL
           ,NULL
           ,0
           ,0
           ,0
           ,0
           ,NULL
           ,0)
GO

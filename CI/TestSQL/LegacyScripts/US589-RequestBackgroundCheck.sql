USE [MinistryPlatform]
GO

DELETE FROM [dbo].[dp_Communications] 
       WHERE [Subject] = 'Background Check Request' AND [Template] = 1;
GO

INSERT INTO [dbo].[dp_Communications]
           ([Author_User_ID]
           ,[Subject]
           ,[Body]
           ,[Domain_ID]
           ,[Start_Date]
           ,[Expire_Date]
           ,[Communication_Status_ID]
           ,[From_Contact]
           ,[Reply_to_Contact]
           ,[_Sent_From_Task]
           ,[Selection_ID]
           ,[Template]
           ,[Active]
           ,[To_Contact])
     VALUES
           (5
           ,'Background Check Request'
           ,'Hello there, <br /><br /> Thank you for your continued interest in serving. As part of our policy, we are requesting that you fill out a background check. Please visit https://www.employmentreports.net/ApplicationAlpha/Subject/DisclosureForm?cra=525VER&username=CrossroadsPDF&application=CrossroadsDisclosure.pdf&externalId=[Contact_ID]&redirectUrl=http%3A%2F%2Fint.crossroads.net%2F%23%2Fbackgroundcheck-thanks%2F and fill out the required information.<br /><br /> Please note that when you click the link, you will be taken to a page outside of Crossroads.net. Our background check is handled by VeriData.  <br /><br /> Thank you,'
           ,1
           ,GETDATE()
           ,NULL
           ,NULL
           ,7
           ,7
           ,NULL
           ,NULL
           ,1
           ,1
           ,NULL)
GO


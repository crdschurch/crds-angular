USE [MinistryPlatform]
GO

IF NOT EXISTS(SELECT * FROM [dbo].[dp_Communications] WHERE Communication_ID = 17554)
BEGIN
  SET IDENTITY_INSERT [dbo].[dp_Communications] ON

  UPDATE [dbo].[dp_Pages]
    SET [Default_Field_List] = 'dp_Users.[Display_Name],dp_Users.User_Name,Contact_ID_Table.Display_Name AS Contact_Name,dp_Users.User_Email,Contact_ID_Table.Email_Address AS [Contact Email],Supervisor_Table.Display_Name AS Supervisor_Name,dp_Users.keep_for_go_live,dp_Users.PasswordResetToken'
  WHERE Page_ID = 401

  INSERT INTO [dbo].[dp_Communications]
           ([Communication_ID]
           ,[Author_User_ID]
           ,[Subject]
           ,[Body]
           ,[Domain_ID]
           ,[Start_Date]
           ,[From_Contact]
           ,[Reply_to_Contact]
           ,[Template]
           ,[Active])
     VALUES
           (17554
           ,5
           ,'Welcome to Crossroads'
           ,'<span id="docs-internal-guid-dbf31dde-d50b-4a80-05cb-7601d6b01558"><p dir="ltr" style="line-height:1.38;margin-top:0pt;margin-bottom:0pt;"><span style="font-size: 10pt; font-family: Arial; color: rgb(34, 34, 34); background-color: rgb(255, 255, 255); vertical-align: baseline; white-space: pre-wrap;">Congrats (firstname)! You have successfully registered your family to Crossroads Kids’ Club check in.  Here are some ways to stay updated on all the latest news, events and exclusive content:</span></p><p dir="ltr" style="line-height:1.38;margin-top:0pt;margin-bottom:0pt;"> </p><ol style="margin-top:0pt;margin-bottom:0pt;"><li dir="ltr" style="list-style-type: decimal; font-size: 10pt; font-family: Arial; background-color: rgb(255, 255, 255); vertical-align: baseline;"><p dir="ltr" style="line-height:1.38;margin-top:0pt;margin-bottom:0pt;"><span style="font-size: 10pt; color: rgb(34, 34, 34); vertical-align: baseline; white-space: pre-wrap;">We’d love for you to update your profile on crossroads.net. Use this </span><a href="https://crossroads.net/reset-password?token=[PasswordResetToken]" target="_self">link</a><span style="font-size: 10pt; color: rgb(34, 34, 34); vertical-align: baseline; white-space: pre-wrap;"> </span><span style="font-size: 10pt; color: rgb(51, 51, 51); vertical-align: baseline; white-space: pre-wrap;">to reset your password, login and then update your profile on </span><a href="http://crossroads.net" style="text-decoration-line: none;"><span style="font-size: 10pt; color: rgb(102, 17, 204); text-decoration-line: underline; vertical-align: baseline; white-space: pre-wrap;">crossroads.net</span></a></p></li><li dir="ltr" style="list-style-type: decimal; font-size: 10pt; font-family: Arial; background-color: rgb(255, 255, 255); vertical-align: baseline;"><p dir="ltr" style="line-height:1.38;margin-top:0pt;margin-bottom:0pt;"><span style="font-size: 10pt; color: rgb(34, 34, 34); vertical-align: baseline; white-space: pre-wrap;">Once logged in, go to &quot;Profile&quot; and tell us more about you–like your address, phone number and favorite coffee flavor (kidding). This will keep you updated on upcoming events and special announcements. And don’t worry–all of your personal information is kept confidential and is stored securely.</span></p></li><li dir="ltr" style="list-style-type: decimal; font-size: 10pt; font-family: Arial; color: rgb(34, 34, 34); background-color: rgb(255, 255, 255); vertical-align: baseline;"><p dir="ltr" style="line-height:1.38;margin-top:0pt;margin-bottom:0pt;"><span style="font-size: 10pt; vertical-align: baseline; white-space: pre-wrap;">Want to stay up to date on what’s happening around here on a daily basis? Follow us on </span><a href="https://www.facebook.com/crdschurchkc/?ref=aymt_homepage_panel" style="text-decoration-line: none;"><span style="font-size: 10pt; color: rgb(17, 85, 204); text-decoration-line: underline; vertical-align: baseline; white-space: pre-wrap;">Facebook</span></a><span style="font-size: 10pt; vertical-align: baseline; white-space: pre-wrap;">, </span><a href="https://twitter.com/crdschurch?ref_src=twsrc%5Egoogle%7Ctwcamp%5Eserp%7Ctwgr%5Eauthor" style="text-decoration-line: none;"><span style="font-size: 10pt; color: rgb(17, 85, 204); text-decoration-line: underline; vertical-align: baseline; white-space: pre-wrap;">Twitter</span></a><span style="font-size: 10pt; vertical-align: baseline; white-space: pre-wrap;">, </span><a href="https://www.instagram.com/crdschurchkc/" style="text-decoration-line: none;"><span style="font-size: 10pt; color: rgb(17, 85, 204); text-decoration-line: underline; vertical-align: baseline; white-space: pre-wrap;">Instagram</span></a><span style="font-size: 10pt; vertical-align: baseline; white-space: pre-wrap;"> (if you’re into that sort of thing) and download the Crossroads Anywhere App. The app is free, and it’s available for both iOS and Android devices. Just search for “Crossroads Anywhere” in your app store. You’ll also receive Kids’ Club emails with special updates and exclusive content! </span></p></li><li dir="ltr" style="list-style-type: decimal; font-size: 10pt; font-family: Arial; color: rgb(34, 34, 34); background-color: rgb(255, 255, 255); vertical-align: baseline;"><p dir="ltr" style="line-height:1.38;margin-top:0pt;margin-bottom:0pt;"><span style="font-size: 10pt; vertical-align: baseline; white-space: pre-wrap;">If you’re looking for activities to enjoy as a family during the week, we’ve got you covered. Check out </span><a href="http://crossroadskidsclub.net/" style="text-decoration-line: none;"><span style="font-size: 10pt; color: rgb(17, 85, 204); text-decoration-line: underline; vertical-align: baseline; white-space: pre-wrap;">crossroadskidsclub.net</span></a><span style="font-size: 10pt; vertical-align: baseline; white-space: pre-wrap;"> to watch videos, listen to free music and see what kids experienced each week in Kids’ Club.</span></p></li></ol><p dir="ltr" style="line-height:1.38;margin-top:0pt;margin-bottom:0pt;"> </p><p dir="ltr" style="line-height:1.38;margin-top:0pt;margin-bottom:0pt;"><span style="font-size: 10pt; font-family: Arial; color: rgb(34, 34, 34); background-color: rgb(255, 255, 255); vertical-align: baseline; white-space: pre-wrap;">We hope this helps you stay connected as you explore Crossroads. If you have any other questions, please feel free to contact us.</span><span style="font-size: 10pt; font-family: Arial; color: rgb(34, 34, 34); background-color: rgb(255, 255, 255); vertical-align: baseline; white-space: pre-wrap;"><img height="1" src="https://lh6.googleusercontent.com/GNBZSs4Nayw390PFZpLUL6asO4UtLFmVMnPJvL48Vj5npiEWueBvnVnUJs4Fr17J3bhbPnJl51S6s26TJ7-NzlMAgow0kcZ0RGq_hx3qW58tFN4K1bfTd1ghM1qFDXkk4yFWS1Wc" style="border: none; transform: rotate(0.00rad); -webkit-transform: rotate(0.00rad);" width="1" /></span></p><p dir="ltr" style="line-height:1.38;margin-top:0pt;margin-bottom:0pt;"> </p><p dir="ltr" style="line-height:1.38;margin-top:0pt;margin-bottom:0pt;"><span style="font-size: 10pt; font-family: Arial; color: rgb(34, 34, 34); background-color: rgb(255, 255, 255); vertical-align: baseline; white-space: pre-wrap;">Crossroads Kids’ Club Team</span></p></span>'
           ,1
           ,'2017-06-23 00:00:00.000'
           ,1519180
           ,1519180
           ,1
           ,1)

		SET IDENTITY_INSERT [dbo].[dp_Communications] OFF
END

IF NOT EXISTS(SELECT * FROM [dbo].[dp_Processes] where Process_ID = 52)
BEGIN
	SET IDENTITY_INSERT [dbo].[dp_Processes] ON
	
	UPDATE [dbo].[dp_Processes]
	   SET [Dependent_Condition] = 'EXISTS(SELECT * FROM Contacts c INNER JOIN Households h ON h.Household_ID = c.Household_ID WHERE c.Contact_ID = dp_Users.Contact_ID AND h.Household_Source_ID !=48)'
	WHERE Process_ID = 23

    INSERT INTO [dbo].[dp_Processes]
           ([Process_ID]
           ,[Process_Name]
           ,[Process_Manager]
           ,[Active]
           ,[Description]
           ,[Record_Type]
           ,[Domain_ID]
           ,[Trigger_Fields]
           ,[Dependent_Condition]
           ,[Trigger_On_Create]
           ,[Trigger_On_Update])
     VALUES
           (52
           ,'Send Welcom Email From Kids Club'
           ,3009216
           ,1
           ,'Send a welcome email when a new user is registered through Kids Club'
           ,401
           ,1
           ,'Contact_ID'
           ,'EXISTS(SELECT * FROM Contacts c INNER JOIN Households h ON h.Household_ID = c.Household_ID WHERE c.Contact_ID = dp_Users.Contact_ID AND h.Household_Source_ID = 48)'
           ,1
           ,1)

    INSERT INTO [dbo].[dp_Process_Steps]
           ([Step_Name]
           ,[Process_Step_Type_ID]
           ,[Escalation_Only]
           ,[Order]
           ,[Process_ID]
           ,[Supervisor_User]
           ,[Domain_ID]
           ,[Email_Template]
           ,[Email_To_Contact_Field])
     VALUES
           ('Send Welcome Email Kids Club'
           ,4
           ,0
           ,1
           ,52
           ,0
           ,1
           ,17554
           ,'Contact_ID_Table.[Contact_ID]')

	SET IDENTITY_INSERT [dbo].[dp_Processes] OFF
END

GO

USE [MinistryPlatform]
GO

-- =============================================
-- Author:      John Cleaver
-- Create date: 2017-05-23
-- Description:	Adds process to send an email any time a new
-- Invoice Detail is created
-- (Note: IDs created via Identity Maintenance proc)
-- =============================================

IF NOT EXISTS(SELECT 1 FROM [dbo].[dp_Processes] WHERE Process_ID = 51)
BEGIN
	SET IDENTITY_INSERT [dbo].[dp_Processes] ON
	INSERT INTO [dbo].[dp_Processes]
			   ([Process_ID]
			   ,[Process_Name]
			   ,[Process_Manager]
			   ,[Active]
			   ,[Description]
			   ,[Record_Type]
			   ,[Domain_ID]
			   ,[On_Submit]
			   ,[On_Complete]
			   ,[Trigger_Fields]
			   ,[Dependent_Condition])
		 VALUES
			   (51
			   ,'InvoiceDetailNotification'
			   ,5
			   ,0
			   ,'Send email on Invoice Detail creation'
			   ,272
			   ,1
			   ,NULL
			   ,NULL
			   ,'Invoice_Detail_ID'
			   ,'EXISTS(select 1 from Invoices I where I.Invoice_ID = Invoice_Detail.Invoice_ID)')
	SET IDENTITY_INSERT [dbo].[dp_Processes] OFF
END

IF NOT EXISTS(SELECT 1 FROM [dbo].[dp_Process_Steps] WHERE Step_Name = 'Send Generic Invoice Notification')
BEGIN
	SET IDENTITY_INSERT [dbo].[dp_Process_Steps] ON
	INSERT INTO [dbo].[dp_Process_Steps]
			   ([Process_Step_ID]
			   ,[Step_Name]
			   ,[Instructions]
			   ,[Process_Step_Type_ID]
			   ,[Escalation_Only]
			   ,[Order]
			   ,[Process_ID]
			   ,[Specific_User]
			   ,[Supervisor_User]
			   ,[Lookup_User_Field]
			   ,[Domain_ID]
			   ,[Escalate_to_Step]
			   ,[Task_Deadline]
			   ,[Email_Template]
			   ,[To_Specific_Contact]
			   ,[Email_To_Contact_Field]
			   ,[SQL_Statement])
		 VALUES
			   (86
			   ,'Send Generic Invoice Notification'
			   ,NULL
			   ,4
			   ,0
			   ,1
			   ,51
			   ,NULL
			   ,0
			   ,'Recipient_Contact_ID_Table_User_Account_Table.User_ID'
			   ,1
			   ,NULL
			   ,NULL
			   ,2019
			   ,NULL
			   ,'Recipient_Contact_ID'
			   ,NULL)
	SET IDENTITY_INSERT [dbo].[dp_Process_Steps] ON
END
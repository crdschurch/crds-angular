USE MINISTRYPLATFORM
GO

IF EXISTS (SELECT 1 FROM [dbo].[dp_Sub_Pages] where [Sub_Page_ID] = 267)
BEGIN
	UPDATE [dbo].[dp_Sub_Pages]
	SET [Default_Field_List] = 'Date_Submitted ,(ISNULL(([dp_Created].[User_Name]), ''Unknown'')) AS [Submitted By] ,Entry_Title ,Feedback_Type_ID_Table.Feedback_Type ,Assigned_To_Table.Display_Name AS Assigned_To ,Care_Outcome_ID_Table.Care_Outcome ,Feedback_Entries.Description'
	WHERE [Sub_Page_ID] = 267
END

USE MINISTRYPLATFORM
GO

IF EXISTS (SELECT 1 FROM [dbo].[dp_Sub_Pages] where [Sub_Page_ID] = 541)
BEGIN
	UPDATE [dbo].[dp_Sub_Pages]
	SET [Default_Field_List] = 'Restriction_Date, [dp_Created].[User_Name] AS [Created By], Ministry_ID_Table.Ministry_Name, Serve_Restrictions_Reason_ID_Table.Serve_Restrictions_Reason, Comments'
	WHERE [Sub_Page_ID] = 541
END
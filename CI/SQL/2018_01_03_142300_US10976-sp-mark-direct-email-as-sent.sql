USE [MinistryPlatform]
GO
/****** Object:  StoredProcedure [dbo].[crds_Mark_Direct_Email_As_Sent]    Script Date: 1/3/2018 11:59:45 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

IF OBJECT_ID('dbo.crds_Mark_Direct_Email_As_Sent') IS NULL -- Check if SP Exists
        EXEC('CREATE PROCEDURE dbo.crds_Mark_Direct_Email_As_Sent AS SET NOCOUNT ON;') -- Create dummy/empty SP
GO
-- =================================================================================
-- Author:      Ken Baum
-- Create date: 01/03/2018
-- Description: When a direct email is sent, the record in the dp_Communications
--				table has a status of 'draft' while the record in the
--				dp_Communications_Messages table has a status of 'Ready to Send'.
--				This stored proc is called after the email has been sent. It sets
--				the status of both records to 'Sent'.
-- =================================================================================
ALTER PROCEDURE [dbo].[crds_Mark_Direct_Email_As_Sent]
    @Communication_ID INT,
	@Communication_Message_ID INT
AS
BEGIN
    SET NOCOUNT ON

	BEGIN TRY
		
		BEGIN TRANSACTION
	
		UPDATE dbo.dp_Communications
		SET Communication_Status_ID = 4 -- 4='Sent'
		WHERE Communication_ID = @Communication_ID

		UPDATE dbo.dp_Communication_Messages
		SET Action_Status_ID = 3, -- 3='Sent'
			Action_Status_Time = SYSDATETIME()
		WHERE Communication_Message_ID = @Communication_Message_ID

		COMMIT TRANSACTION
		PRINT 'crds_Mark_Direct_Email_As_Sent completed successfully.'
	END TRY

	BEGIN CATCH
		PRINT 'crds_Mark_Direct_Email_As_Sent failed: ' + ERROR_MESSAGE()
		IF @@TRANCOUNT > 0
            ROLLBACK
	END CATCH

END
GO

-- setup permissions for API User in MP
DECLARE @procName nvarchar(100) = N'crds_Mark_Direct_Email_As_Sent'
DECLARE @procDescription nvarchar(100) = N'Marks direct emails as sent.'

IF NOT EXISTS (SELECT 1 FROM [dbo].[dp_API_Procedures] WHERE [Procedure_Name] = @procName)
BEGIN
        INSERT INTO [dbo].[dp_API_Procedures] (
                 Procedure_Name
                ,Description
        ) VALUES (
                 @procName
                ,@procDescription
        )
END

DECLARE @API_ROLE_ID int = 62;
DECLARE @API_ID int;

SELECT @API_ID = API_Procedure_ID FROM [dbo].[dp_API_Procedures] WHERE [Procedure_Name] = @procName;

IF NOT EXISTS (SELECT 1 FROM [dbo].[dp_Role_API_Procedures] WHERE [Role_ID] = @API_ROLE_ID AND [API_Procedure_ID] = @API_ID)
BEGIN
        INSERT INTO [dbo].[dp_Role_API_Procedures] (
                 [Role_ID]
                ,[API_Procedure_ID]
                ,[Domain_ID]
        ) VALUES (
                 @API_ROLE_ID
                ,@API_ID
                ,1
        )
END
GO

USE [MinistryPlatform]
GO
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

-- =============================================
-- Author:		Henney, Sarah
-- Create date: 01/05/2018
-- Description:	Miscellaneous, independent stored procedures used by multiple other stored procs in the cr_QA_Delete suite
-- =============================================

-- Defines cr_QA_Delete_Childcare_Request
IF NOT EXISTS ( SELECT  *
	FROM    sys.objects
	WHERE   object_id = OBJECT_ID(N'cr_QA_Delete_Childcare_Request')
			AND type IN ( N'P', N'PC' ) )
	EXEC('CREATE PROCEDURE dbo.cr_QA_Delete_Childcare_Request
	@childcare_request_id int AS SET NOCOUNT ON;')
GO
ALTER PROCEDURE [dbo].[cr_QA_Delete_Childcare_Request] 
	@childcare_request_id int
AS
BEGIN
	SET NOCOUNT ON;

	IF @childcare_request_id is null
		RETURN;
	
	--Delete foreign key entries that can't be nullified
	DELETE [dbo].cr_Childcare_Request_Dates WHERE Childcare_Request_ID = @childcare_request_id;

	DELETE [dbo].cr_Childcare_Requests WHERE Childcare_Request_ID = @childcare_request_id;
END
GO


-- Defines cr_QA_Delete_Recurring_Gift
IF NOT EXISTS ( SELECT  *
	FROM    sys.objects
	WHERE   object_id = OBJECT_ID(N'cr_QA_Delete_Recurring_Gift')
			AND type IN ( N'P', N'PC' ) )
	EXEC('CREATE PROCEDURE dbo.cr_QA_Delete_Recurring_Gift
	@recurring_gift_id int AS SET NOCOUNT ON;')
GO
ALTER PROCEDURE [dbo].[cr_QA_Delete_Recurring_Gift] 
	@recurring_gift_id int
AS
BEGIN
	SET NOCOUNT ON;

	IF @recurring_gift_id is null
		RETURN;
	
	--Nullify foreign keys
	UPDATE [dbo].Donations SET Recurring_Gift_ID = null WHERE Recurring_Gift_ID = @recurring_gift_id;

	DELETE [dbo].Recurring_Gifts WHERE Recurring_Gift_ID = @recurring_gift_id;	
END
GO


-- Defines cr_QA_Delete_Communication
IF NOT EXISTS ( SELECT  *
	FROM    sys.objects
	WHERE   object_id = OBJECT_ID(N'cr_QA_Delete_Communication')
			AND type IN ( N'P', N'PC' ) )
	EXEC('CREATE PROCEDURE dbo.cr_QA_Delete_Communication
	@communication_id int AS SET NOCOUNT ON;')
GO
ALTER PROCEDURE [dbo].[cr_QA_Delete_Communication] 
	@communication_id int
AS
BEGIN
	SET NOCOUNT ON;

	IF @communication_id is null
		RETURN;

	--Delete foreign key entries that can't be nullified
	DELETE [dbo].cr_Donation_Communications WHERE Communication_ID = @communication_id;
	DELETE [dbo].dp_Communication_Messages WHERE Communication_ID = @communication_id;
	DELETE [dbo].dp_Communication_Publications WHERE Communication_ID = @communication_id;
	DELETE [dbo].dp_Communication_User_Groups WHERE Communication_ID = @communication_id;

	--Nullify foreign keys
	UPDATE [dbo].dp_Commands SET Communication_ID = null WHERE Communication_ID = @communication_id;
	UPDATE [dbo].dp_Notifications SET Template_ID = null WHERE Template_ID = @communication_id;
	UPDATE [dbo].dp_Process_Steps SET Email_Template = null WHERE Email_Template = @communication_id;
	UPDATE [dbo].Events SET Registrant_Message = null WHERE Registrant_Message = @communication_id;
	UPDATE [dbo].Events SET Optional_Reminder_Message = null WHERE Optional_Reminder_Message = @communication_id;
	UPDATE [dbo].Opportunities SET Reminder_Template = null WHERE Reminder_Template = @communication_id;
	UPDATE [dbo].Programs SET Communication_ID = null WHERE Communication_ID = @communication_id;
		
	DELETE [dbo].dp_Communications WHERE Communication_ID = @communication_id;
END
GO


-- Defines cr_QA_Delete_Form_Response
IF NOT EXISTS ( SELECT  *
	FROM    sys.objects
	WHERE   object_id = OBJECT_ID(N'cr_QA_Delete_Form_Response')
			AND type IN ( N'P', N'PC' ) )
	EXEC('CREATE PROCEDURE dbo.cr_QA_Delete_Form_Response
	@form_response_id int AS SET NOCOUNT ON;')
GO
ALTER PROCEDURE [dbo].[cr_QA_Delete_Form_Response] 
	@form_response_id int
AS
BEGIN
	SET NOCOUNT ON;

	IF @form_response_id is null
		RETURN;

	--Delete foreign key entries that can't be nullified
	DELETE [dbo].cr_Form_Response_Submissions WHERE Form_Response_ID = @form_response_id;
	DELETE [dbo].Form_Response_Answers WHERE Form_Response_ID = @form_response_id;

	--Nullify foreign keys
	UPDATE [dbo].cr_Submissions SET Form_Response_ID = null WHERE Form_Response_ID = @form_response_id;
	UPDATE [dbo].Invoices SET Form_Response_ID = null WHERE Form_Response_ID = @form_response_id;
	
	DELETE [dbo].Form_Responses WHERE Form_Response_ID = @form_response_id;
END
GO


-- Defines cr_QA_Delete_Attribute
IF NOT EXISTS ( SELECT  *
	FROM    sys.objects
	WHERE   object_id = OBJECT_ID(N'cr_QA_Delete_Attribute')
			AND type IN ( N'P', N'PC' ) )
	EXEC('CREATE PROCEDURE dbo.cr_QA_Delete_Attribute
	@attribute_id int AS SET NOCOUNT ON;')
GO
ALTER PROCEDURE [dbo].[cr_QA_Delete_Attribute] 
	@attribute_id int
AS
BEGIN
	SET NOCOUNT ON;

	IF @attribute_id is null
		RETURN;
	
	--Delete foreign key entries that can't be nullified
	DELETE [dbo].Contact_Attributes WHERE Attribute_ID = @attribute_id;
	DELETE [dbo].cr_Go_Volunteer_Skills WHERE Attribute_ID = @attribute_id;
	DELETE [dbo].cr_Registration_Children_Attributes WHERE Attribute_ID = @attribute_id;
	DELETE [dbo].cr_Registration_Equipment_Attributes WHERE Attribute_ID = @attribute_id;
	DELETE [dbo].cr_Registration_PrepWork_Attributes WHERE Attribute_ID = @attribute_id;
	DELETE [dbo].Group_Attributes WHERE Attribute_ID = @attribute_id;
	DELETE [dbo].Group_Participant_Attributes WHERE Attribute_ID = @attribute_id;
	DELETE [dbo].Group_Role_Attributes WHERE Attribute_ID = @attribute_id OR Group_Role_Attribute_ID = @attribute_id;
	DELETE [dbo].Opportunity_Attributes WHERE Attribute_ID = @attribute_id;
	DELETE [dbo].Response_Attributes WHERE Attribute_ID = @attribute_id;

	DELETE [dbo].Attributes WHERE Attribute_ID = @attribute_id;
END
GO


-- Defines cr_QA_Delete_Attribute_By_Name
IF NOT EXISTS ( SELECT  *
	FROM    sys.objects
	WHERE   object_id = OBJECT_ID(N'cr_QA_Delete_Attribute_By_Name')
			AND type IN ( N'P', N'PC' ) )
	EXEC('CREATE PROCEDURE dbo.cr_QA_Delete_Attribute_By_Name
	@attribute_name nvarchar(100) AS SET NOCOUNT ON;')
GO
ALTER PROCEDURE [dbo].[cr_QA_Delete_Attribute_By_Name] 
	@attribute_name nvarchar(100)
AS
BEGIN
	SET NOCOUNT ON;
	
	IF @attribute_name is null
		RETURN;

	DECLARE @attributes_to_delete TABLE
	(
		attribute_id int
	);
	INSERT INTO @attributes_to_delete (attribute_id) SELECT Attribute_ID FROM [dbo].Attributes WHERE Attribute_Name = @attribute_name;
	
	DECLARE @cur_entry_id int = 0;

	WHILE @cur_entry_id is not null
	BEGIN
		--Get top item in list
		SET @cur_entry_id = (SELECT TOP 1 attribute_id 
			FROM @attributes_to_delete
			WHERE attribute_id > @cur_entry_id
			ORDER BY attribute_id ASC);

		--Delete using the stored proc
		IF @cur_entry_id is not null
		BEGIN
			EXEC [dbo].[cr_QA_Delete_Attribute] @cur_entry_id;
		END
	END		
END
GO
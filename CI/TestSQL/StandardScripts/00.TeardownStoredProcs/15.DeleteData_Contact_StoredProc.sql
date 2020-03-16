USE [MinistryPlatform]
GO
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

-- =============================================
-- Author:		Henney, Sarah
-- Create date: 01/09/2018
-- Description:	Stored procedure declarations for deleting contact data
-- =============================================

-- Defines cr_QA_Delete_Medical_Information
IF NOT EXISTS ( SELECT  *
	FROM    sys.objects
	WHERE   object_id = OBJECT_ID(N'cr_QA_Delete_Medical_Information')
			AND type IN ( N'P', N'PC' ) )
	EXEC('CREATE PROCEDURE dbo.cr_QA_Delete_Medical_Information
	@medical_information_id int AS SET NOCOUNT ON;')
GO
ALTER PROCEDURE [dbo].[cr_QA_Delete_Medical_Information] 
	@medical_information_id int
AS
BEGIN
	SET NOCOUNT ON;

	IF @medical_information_id is null
		RETURN;

	--Delete foreign key entries that can't be nullified
	DELETE [dbo].cr_Medical_Information_Allergies WHERE Medical_Information_ID = @medical_information_id;
	DELETE [dbo].cr_Medical_Information_Medications WHERE MedicalInformation_ID = @medical_information_id;

	--Nullify foreign keys
	UPDATE [dbo].Contacts SET MedicalInformation_ID = null WHERE MedicalInformation_ID = @medical_information_id;

	DELETE [dbo].cr_Medical_Information WHERE MedicalInformation_ID = @medical_information_id;
END
GO


-- Defines cr_QA_Delete_Contact
IF NOT EXISTS ( SELECT  *
	FROM    sys.objects
	WHERE   object_id = OBJECT_ID(N'cr_QA_Delete_Contact')
			AND type IN ( N'P', N'PC' ) )
	EXEC('CREATE PROCEDURE dbo.cr_QA_Delete_Contact
	@contact_id int AS SET NOCOUNT ON;')
GO
ALTER PROCEDURE [dbo].[cr_QA_Delete_Contact]
	@contact_id int
AS
BEGIN
	SET NOCOUNT ON;

	IF @contact_id is null
		RETURN;

	--Delete foreign key entries that can't be nullified
	DELETE [dbo].Activity_Log WHERE Contact_ID = @contact_id;		
	DELETE [dbo].Background_Checks WHERE Contact_ID = @contact_id;		
	DELETE [dbo].Congregations WHERE Contact_ID = @contact_id;		
	DELETE [dbo].Contact_Attributes WHERE Contact_ID = @contact_id;		
	DELETE [dbo].Contact_Households WHERE Contact_ID = @contact_id;
	DELETE [dbo].Contact_Identifier_Log WHERE Contact_ID = @contact_id;
	DELETE [dbo].Contact_Log WHERE Contact_ID = @contact_id;
	DELETE [dbo].Counseling_Engagements WHERE Contact_ID = @contact_id;		
	DELETE [dbo].cr_Campaign_Age_Exception WHERE Contact_ID = @contact_id;
	DELETE [dbo].cr_Coaches WHERE Leader_Contact_ID = @contact_id;		
	DELETE [dbo].cr_Coaches WHERE Coach_Contact_ID = @contact_id;		
	DELETE [dbo].cr_Connect_Communications WHERE From_Contact_ID = @contact_id;		
	DELETE [dbo].cr_Connect_Communications WHERE To_Contact_ID = @contact_id;		
	DELETE [dbo].cr_Mentors WHERE Mentor_Contact_ID = @contact_id;		
	DELETE [dbo].cr_Mentors WHERE Coach_Contact_ID = @contact_id;		
	DELETE [dbo].cr_Organizations WHERE Primary_Contact = @contact_id;		
	DELETE [dbo].cr_Override_Email_Prevention WHERE Contact_ID = @contact_id;		
	DELETE [dbo].cr_Serve_Restrictions WHERE Contact_ID = @contact_id;
	DELETE [dbo].dp_Contact_Publications WHERE Contact_ID = @contact_id;		
	DELETE [dbo].dp_Impersonate_Contacts WHERE Contact_ID = @contact_id;
	DELETE [dbo].Feedback_Entries WHERE Contact_ID = @contact_id;
	DELETE [dbo].Household_Care_Log WHERE Provided_By = @contact_id;

	--Delete foreign key entries that can't be nullified using another stored proc
	--Delete Childcare Requests
	DECLARE @childcare_requests_to_delete TABLE
	(
		childcare_request_id int
	)
	INSERT INTO @childcare_requests_to_delete (childcare_request_id) SELECT Childcare_Request_ID 
		FROM [dbo].cr_Childcare_Requests WHERE Requester_ID = @contact_id;

	DECLARE @cur_entry_id int = 0;

	WHILE @cur_entry_id is not null
	BEGIN
		--Get top item in list
		SET @cur_entry_id = (SELECT TOP 1 childcare_request_id 
			FROM @childcare_requests_to_delete
			WHERE childcare_request_id > @cur_entry_id
			ORDER BY childcare_request_id ASC);

		--Delete using the stored proc
		IF @cur_entry_id is not null
		BEGIN
			EXEC [dbo].[cr_QA_Delete_Childcare_Request] @cur_entry_id;
		END
	END		

	--Delete Medical Information
	DECLARE @medical_information_to_delete TABLE
	(
		medical_information_id int
	)
	INSERT INTO @medical_information_to_delete (medical_information_id) SELECT MedicalInformation_ID 
		FROM [dbo].cr_Medical_Information WHERE Contact_ID = @contact_id;

	SET @cur_entry_id = 0;

	WHILE @cur_entry_id is not null
	BEGIN
		--Get top item in list
		SET @cur_entry_id = (SELECT TOP 1 medical_information_id 
			FROM @medical_information_to_delete
			WHERE medical_information_id > @cur_entry_id
			ORDER BY medical_information_id ASC);

		--Delete using the stored proc
		IF @cur_entry_id is not null
		BEGIN
			EXEC [dbo].[cr_QA_Delete_Medical_Information] @cur_entry_id;
		END
	END	

	--Delete Donor
	DECLARE @donor_to_delete TABLE
	(
		donor_id int
	)
	INSERT INTO @donor_to_delete (donor_id) SELECT Donor_ID 
		FROM [dbo].Donors WHERE Contact_ID = @contact_id;

	SET @cur_entry_id = 0;

	WHILE @cur_entry_id is not null
	BEGIN
		--Get top item in list
		SET @cur_entry_id = (SELECT TOP 1 donor_id 
			FROM @donor_to_delete
			WHERE donor_id > @cur_entry_id
			ORDER BY donor_id ASC);

		--Delete using the stored proc
		IF @cur_entry_id is not null
		BEGIN
			EXEC [dbo].[cr_QA_Delete_Donor] @cur_entry_id;
		END
	END	

	--Delete Communications
	DECLARE @communications_to_delete TABLE
	(
		communication_id int
	)
	INSERT INTO @communications_to_delete (communication_id) SELECT Communication_ID 
		FROM [dbo].dp_Communications WHERE From_Contact = @contact_id;
	INSERT INTO @communications_to_delete (communication_id) SELECT Communication_ID 
		FROM [dbo].dp_Communications WHERE Reply_to_Contact = @contact_id;

	SET @cur_entry_id = 0;

	WHILE @cur_entry_id is not null
	BEGIN
		--Get top item in list
		SET @cur_entry_id = (SELECT TOP 1 communication_id 
			FROM @communications_to_delete
			WHERE communication_id > @cur_entry_id
			ORDER BY communication_id ASC);

		--Delete using the stored proc
		IF @cur_entry_id is not null
		BEGIN
			EXEC [dbo].[cr_QA_Delete_Communication] @cur_entry_id;
		END
	END

	--Delete Users
	DECLARE @users_to_delete TABLE
	(
		user_id int
	)
	INSERT INTO @users_to_delete (user_id) SELECT User_ID 
		FROM [dbo].dp_Users WHERE Contact_ID = @contact_id;

	SET @cur_entry_id = 0;

	WHILE @cur_entry_id is not null
	BEGIN
		--Get top item in list
		SET @cur_entry_id = (SELECT TOP 1 user_id 
			FROM @users_to_delete
			WHERE user_id > @cur_entry_id
			ORDER BY user_id ASC);

		--Delete using the stored proc
		IF @cur_entry_id is not null
		BEGIN
			EXEC [dbo].[cr_QA_Delete_User] @cur_entry_id;
		END
	END
		
	--Delete Events
	DECLARE @events_to_delete TABLE
	(
		event_id int
	)
	INSERT INTO @events_to_delete (event_id) SELECT Event_ID 
		FROM [dbo].Events WHERE Primary_Contact = @contact_id;

	SET @cur_entry_id = 0;

	WHILE @cur_entry_id is not null
	BEGIN
		--Get top item in list
		SET @cur_entry_id = (SELECT TOP 1 event_id 
			FROM @events_to_delete
			WHERE event_id > @cur_entry_id
			ORDER BY event_id ASC);

		--Delete using the stored proc
		IF @cur_entry_id is not null
		BEGIN
			EXEC [dbo].[cr_QA_Delete_Event] @cur_entry_id;
		END
	END
	
	--Delete Groups
	DECLARE @groups_to_delete TABLE
	(
		group_id int
	)
	INSERT INTO @groups_to_delete (group_id) SELECT Group_ID 
		FROM [dbo].Groups WHERE Primary_Contact = @contact_id;

	SET @cur_entry_id = 0;

	WHILE @cur_entry_id is not null
	BEGIN
		--Get top item in list
		SET @cur_entry_id = (SELECT TOP 1 group_id 
			FROM @groups_to_delete
			WHERE group_id > @cur_entry_id
			ORDER BY group_id ASC);

		--Delete using the stored proc
		IF @cur_entry_id is not null
		BEGIN
			EXEC [dbo].[cr_QA_Delete_Group] @cur_entry_id;
		END
	END

	--Delete Invoice Details
	DECLARE @invoice_details_to_delete TABLE
	(
		invoice_detail_id int
	)
	INSERT INTO @invoice_details_to_delete (invoice_detail_id) SELECT Invoice_Detail_ID 
		FROM [dbo].Invoice_Detail WHERE Recipient_Contact_ID = @contact_id;

	SET @cur_entry_id = 0;

	WHILE @cur_entry_id is not null
	BEGIN
		--Get top item in list
		SET @cur_entry_id = (SELECT TOP 1 invoice_detail_id 
			FROM @invoice_details_to_delete
			WHERE invoice_detail_id > @cur_entry_id
			ORDER BY invoice_detail_id ASC);

		--Delete using the stored proc
		IF @cur_entry_id is not null
		BEGIN
			EXEC [dbo].[cr_QA_Delete_Invoice_Detail] @cur_entry_id;
		END
	END

	--Delete Invoices
	DECLARE @invoices_to_delete TABLE
	(
		invoice_id int
	)
	INSERT INTO @invoices_to_delete (invoice_id) SELECT Invoice_ID 
		FROM [dbo].Invoices WHERE Purchaser_Contact_ID = @contact_id;

	SET @cur_entry_id = 0;

	WHILE @cur_entry_id is not null
	BEGIN
		--Get top item in list
		SET @cur_entry_id = (SELECT TOP 1 invoice_id 
			FROM @invoices_to_delete
			WHERE invoice_id > @cur_entry_id
			ORDER BY invoice_id ASC);

		--Delete using the stored proc
		IF @cur_entry_id is not null
		BEGIN
			EXEC [dbo].[cr_QA_Delete_Invoice] @cur_entry_id;
		END
	END

	--Delete Ministries
	DECLARE @ministries_to_delete TABLE
	(
		ministry_id int
	)
	INSERT INTO @ministries_to_delete (ministry_id) SELECT Ministry_ID 
		FROM [dbo].Ministries WHERE Primary_Contact = @contact_id;
		
	SET @cur_entry_id = 0;

	WHILE @cur_entry_id is not null
	BEGIN
		--Get top item in list
		SET @cur_entry_id = (SELECT TOP 1 ministry_id 
			FROM @ministries_to_delete
			WHERE ministry_id > @cur_entry_id
			ORDER BY ministry_id ASC);

		--Delete using the stored proc
		IF @cur_entry_id is not null
		BEGIN
			EXEC [dbo].[cr_QA_Delete_Ministry] @cur_entry_id;
		END
	END

	--Delete Opportunities
	DECLARE @oppurtunities_to_delete TABLE
	(
		oppurtunity_id int
	)
	INSERT INTO @oppurtunities_to_delete (oppurtunity_id) SELECT Opportunity_ID 
		FROM [dbo].Opportunities WHERE Contact_Person = @contact_id;
		
	SET @cur_entry_id = 0;

	WHILE @cur_entry_id is not null
	BEGIN
		--Get top item in list
		SET @cur_entry_id = (SELECT TOP 1 oppurtunity_id 
			FROM @oppurtunities_to_delete
			WHERE oppurtunity_id > @cur_entry_id
			ORDER BY oppurtunity_id ASC);

		--Delete using the stored proc
		IF @cur_entry_id is not null
		BEGIN
			EXEC [dbo].[cr_QA_Delete_Opportunity] @cur_entry_id;
		END
	END

	--Delete Participants
	DECLARE @participants_to_delete TABLE
	(
		participant_id int
	)
	INSERT INTO @participants_to_delete (participant_id) SELECT Participant_ID 
		FROM [dbo].Participants WHERE Contact_ID = @contact_id;

	SET @cur_entry_id = 0;

	WHILE @cur_entry_id is not null
	BEGIN
		--Get top item in list
		SET @cur_entry_id = (SELECT TOP 1 participant_id 
			FROM @participants_to_delete
			WHERE participant_id > @cur_entry_id
			ORDER BY participant_id ASC);

		--Delete using the stored proc
		IF @cur_entry_id is not null
		BEGIN
			EXEC [dbo].[cr_QA_Delete_Participant] @cur_entry_id;
		END
	END

	--Delete Payments
	DECLARE @payments_to_delete TABLE
	(
		payment_id int
	)
	INSERT INTO @payments_to_delete (payment_id) SELECT Payment_ID 
		FROM [dbo].Payments WHERE Contact_ID = @contact_id;

	SET @cur_entry_id = 0;

	WHILE @cur_entry_id is not null
	BEGIN
		--Get top item in list
		SET @cur_entry_id = (SELECT TOP 1 payment_id 
			FROM @payments_to_delete
			WHERE payment_id > @cur_entry_id
			ORDER BY payment_id ASC);

		--Delete using the stored proc
		IF @cur_entry_id is not null
		BEGIN
			EXEC [dbo].[cr_QA_Delete_Payment] @cur_entry_id;
		END
	END
	
	--Delete Programs
	DECLARE @programs_to_delete TABLE
	(
		program_id int
	)
	INSERT INTO @programs_to_delete (program_id) SELECT Program_ID 
		FROM [dbo].Programs WHERE Primary_Contact = @contact_id;

	SET @cur_entry_id = 0;

	WHILE @cur_entry_id is not null
	BEGIN
		--Get top item in list
		SET @cur_entry_id = (SELECT TOP 1 program_id 
			FROM @programs_to_delete
			WHERE program_id > @cur_entry_id
			ORDER BY program_id ASC);

		--Delete using the stored proc
		IF @cur_entry_id is not null
		BEGIN
			EXEC [dbo].[cr_QA_Delete_Program] @cur_entry_id;
		END
	END

	--Delete Form Responses (deletion not required, but should be for cleaner data)
	DECLARE @form_responses_to_delete TABLE
	(
		form_response_id int
	)
	INSERT INTO @form_responses_to_delete (form_response_id) SELECT Form_Response_ID 
		FROM [dbo].Form_Responses WHERE Contact_ID = @contact_id;

	SET @cur_entry_id = 0;

	WHILE @cur_entry_id is not null
	BEGIN
		--Get top item in list
		SET @cur_entry_id = (SELECT TOP 1 form_response_id 
			FROM @form_responses_to_delete
			WHERE form_response_id > @cur_entry_id
			ORDER BY form_response_id ASC);

		--Delete using the stored proc
		IF @cur_entry_id is not null
		BEGIN
			EXEC [dbo].[cr_QA_Delete_Form_Response] @cur_entry_id;
		END
	END

	--Delete contact household if current contact is only member (deletion not required, but should be for cleaner data)
	DECLARE @household_id int = (SELECT Household_ID FROM [dbo].Contacts WHERE Contact_ID = @contact_id);
	IF @household_id is not null
	BEGIN
		--Get # members in household
		DECLARE @num_household_members int = (SELECT COUNT(Contact_ID)
			FROM [dbo].Contacts
			WHERE Household_ID = @household_id);

		--Delete household
		IF @num_household_members = 1
		BEGIN
			EXEC [dbo].[cr_QA_Delete_Household] @household_id;
		END
	END

	--Nullify foreign keys
	UPDATE [dbo].Accounting_Companies SET Company_Contact_ID = null WHERE Company_Contact_ID = @contact_id;
	UPDATE [dbo].Buildings SET Building_Coordinator = null WHERE Building_Coordinator = @contact_id;
	UPDATE [dbo].Care_Cases SET Contact_ID = null WHERE Contact_ID = @contact_id;
	UPDATE [dbo].Congregations SET Childcare_Contact = null WHERE Childcare_Contact = @contact_id;
	UPDATE [dbo].Contact_Staging SET Existing_Contact_Record = null WHERE Existing_Contact_Record = @contact_id;
	UPDATE [dbo].Counseling_Engagements SET Counselor = null WHERE Counselor = @contact_id;
	UPDATE [dbo].cr_Event_Participant_Waivers SET Signee_Contact_ID = null WHERE Signee_Contact_ID = @contact_id;
	UPDATE [dbo].dp_Communication_Messages SET Contact_ID = null WHERE Contact_ID = @contact_id;
	UPDATE [dbo].dp_Communications SET To_Contact = null WHERE To_Contact = @contact_id;
	UPDATE [dbo].dp_Domains SET Company_Contact = null WHERE Company_Contact = @contact_id;
	UPDATE [dbo].dp_Process_Steps SET To_Specific_Contact = null WHERE To_Specific_Contact = @contact_id;
	UPDATE [dbo].Feedback_Entries SET Assigned_To = null WHERE Assigned_To = @contact_id;
	UPDATE [dbo].Forms SET Primary_Contact = null WHERE Primary_Contact = @contact_id;
	UPDATE [dbo].Group_Inquiries SET Contact_ID = null WHERE Contact_ID = @contact_id;
	UPDATE [dbo].Household_Care_Log SET Paid_To = null WHERE Paid_To = @contact_id;
	UPDATE [dbo].Households SET Care_Person = null WHERE Care_Person = @contact_id;
	UPDATE [dbo].Participant_Milestones SET Witness = null WHERE Witness = @contact_id;
	UPDATE [dbo].Servicing SET Contact_ID = null WHERE Contact_ID = @contact_id;

	--Note that there are triggers on the Contact_Relationships table that keep reciprocal relationships in sync. If there are
	--issues related to deleting Contact_Relationships it may be related to these triggers.
	DELETE [dbo].Contact_Relationships WHERE Related_Contact_ID = @contact_id OR Contact_ID = @contact_id;

	DELETE [dbo].Contacts WHERE Contact_ID = @contact_id;
END
GO
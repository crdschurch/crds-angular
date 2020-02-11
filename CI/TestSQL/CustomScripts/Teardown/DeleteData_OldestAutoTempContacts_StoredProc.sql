USE [MinistryPlatform]
GO
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

-- =============================================
-- Author:		Henney, Sarah
-- Create date:	02/10/2020
-- Description:	Stored procedure declaration for deleting oldest x temp test users
-- =============================================

-- Defines cr_QA_Delete_Temp_Auto_Users
IF NOT EXISTS ( SELECT  *
	FROM    sys.objects
	WHERE   object_id = OBJECT_ID(N'cr_QA_Delete_Temp_Auto_Users')
			AND type IN ( N'P', N'PC' ) )
	EXEC('CREATE PROCEDURE dbo.cr_QA_Delete_Temp_Auto_Users
	@count_to_delete int AS SET NOCOUNT ON;')
GO
ALTER PROCEDURE [dbo].[cr_QA_Delete_Temp_Auto_Users] 
	@count_to_delete int
AS
BEGIN
	SET NOCOUNT ON;

	--This takes between 4-5 sec to delete a contact fully
	DECLARE @auto_temp_contacts TABLE (contact_id int);

	--SELECT only one person per household so we can accurately delete newly empty households
	INSERT INTO @auto_temp_contacts (contact_id)
	(
	SELECT TOP (@count_to_delete)
	MIN(Contact_ID) as "Contact_ID"
	FROM [dbo].Contacts
	WHERE Email_Address LIKE 'mpcrds+auto+temp+%'
	GROUP BY Household_ID
	)
	ORDER BY Contact_ID ASC;

	DECLARE @auto_temp_data TABLE
	(
		contact_id int,
		participant_id int,
		donor_id int,
		dp_users_id int,
		household_id int,
		event_participant_id int,
		invoice_detail_id int,
		form_response_id int,
		invoice_id int,
		payment_id int,
		donation_id int
	)
	INSERT INTO @auto_temp_data (
	contact_id, 
	participant_id, 
	donor_id, 
	dp_users_id,
	household_id,
	event_participant_id,
	invoice_detail_id,
	form_response_id,
	invoice_id,
	payment_id,
	donation_id
	)
	(
	SELECT
	c.Contact_ID,
	p.Participant_ID,
	d.Donor_ID,
	u.User_ID,
	hh.Household_ID,
	ep.Event_Participant_ID,
	id.Invoice_Detail_ID,
	fr.Form_Response_ID,
	i.Invoice_ID,
	pay.Payment_ID,
	don.Donation_ID
	FROM [dbo].Contacts c
	LEFT JOIN Donors d on c.Contact_ID=d.Contact_ID
	LEFT JOIN Households hh on c.Household_ID=hh.Household_ID
	LEFT JOIN Participants p on c.Contact_ID=p.Contact_ID
	LEFT JOIN dp_Users u on c.Contact_ID=u.Contact_ID
	LEFT JOIN Event_Participants ep on p.Participant_ID=ep.Participant_ID
	LEFT JOIN Form_Responses fr on c.Contact_ID=fr.Contact_ID
	LEFT JOIN Invoices i on fr.Form_Response_ID=i.Form_Response_ID or fr.Invoice_ID=i.Invoice_ID
	LEFT JOIN Invoice_Detail id on ep.Event_Participant_ID=id.Event_Participant_ID or i.Invoice_ID=id.Invoice_ID or c.Contact_ID=id.Recipient_Contact_ID
	LEFT JOIN Payments pay on c.Contact_ID=pay.Contact_ID
	LEFT JOIN Donations don on d.Donor_ID=don.Donor_ID
	WHERE c.Contact_ID in (SELECT contact_id FROM @auto_temp_contacts)
	)
	ORDER BY Contact_ID ASC;

	--DEBUG - print ids of what we're about to delete
	SELECT * FROM @auto_temp_data ORDER BY contact_id ASC;

	--delete things

	--participant related
	--unlink Participant
	UPDATE [dbo].Contacts SET Participant_Record = null WHERE Contact_ID in (SELECT contact_id FROM @auto_temp_data);

	DELETE FROM [dbo].Payment_Detail WHERE Invoice_Detail_ID in (SELECT invoice_detail_id FROM @auto_temp_data WHERE invoice_detail_id IS NOT NULL);
	DELETE FROM [dbo].Invoice_Detail WHERE Invoice_Detail_ID in (SELECT invoice_detail_id FROM @auto_temp_data WHERE invoice_detail_id IS NOT NULL);

	DELETE FROM [dbo].cr_EventParticipant_Documents WHERE Event_Participant_ID in (SELECT event_participant_id FROM @auto_temp_data WHERE event_participant_id IS NOT NULL);
	DELETE FROM [dbo].cr_Event_Participant_Waivers WHERE Event_Participant_ID in (SELECT event_participant_id FROM @auto_temp_data WHERE event_participant_id IS NOT NULL);
	DELETE FROM [dbo].Event_Participants WHERE Event_Participant_ID in (SELECT event_participant_id FROM @auto_temp_data WHERE event_participant_id IS NOT NULL);

	DELETE FROM [dbo].Group_Participants WHERE Participant_ID in (SELECT participant_id FROM @auto_temp_data WHERE participant_id IS NOT NULL);
	DELETE FROM [dbo].Participants WHERE Participant_ID in (SELECT participant_id FROM @auto_temp_data WHERE participant_id IS NOT NULL);

	--payment related
	UPDATE [dbo].Form_Responses SET Invoice_ID = null WHERE Invoice_ID in (SELECT invoice_id FROM @auto_temp_data WHERE invoice_id IS NOT NULL);
	UPDATE [dbo].Invoices SET Form_Response_ID = null WHERE Form_Response_ID in (SELECT form_response_id FROM @auto_temp_data WHERE form_response_id IS NOT NULL);

	DELETE FROM [dbo].Invoices WHERE Invoice_ID in (SELECT invoice_id FROM @auto_temp_data WHERE invoice_id IS NOT NULL)
	OR Purchaser_Contact_ID in (SELECT contact_id FROM @auto_temp_data);

	DELETE FROM [dbo].Form_Response_Answers WHERE Form_Response_ID in (SELECT form_response_id FROM @auto_temp_data WHERE form_response_id IS NOT NULL);
	DELETE FROM [dbo].cr_Form_Response_Submissions WHERE Form_Response_ID in (SELECT form_response_id FROM @auto_temp_data WHERE form_response_id IS NOT NULL);
	DELETE FROM [dbo].cr_Submissions WHERE Form_Response_ID in (SELECT form_response_id FROM @auto_temp_data WHERE form_response_id IS NOT NULL);
	DELETE FROM [dbo].Form_Responses WHERE Form_Response_ID in (SELECT form_response_id FROM @auto_temp_data WHERE form_response_id IS NOT NULL);

	DELETE FROM [dbo].Payment_Detail WHERE Payment_ID in (SELECT payment_id FROM @auto_temp_data WHERE payment_id IS NOT NULL)
	or Invoice_Detail_ID in (SELECT invoice_detail_id FROM @auto_temp_data WHERE invoice_detail_id IS NOT NULL);

	DELETE FROM [dbo].Payments WHERE Payment_ID in (SELECT payment_id FROM @auto_temp_data WHERE payment_id IS NOT NULL);

	--donor related
	--unlink donor
	UPDATE [dbo].Contacts SET Donor_Record = null WHERE Contact_ID in (SELECT contact_id FROM @auto_temp_data);

	DELETE [dbo].cr_Distribution_Adjustments WHERE Donation_Distribution_ID 
	IN (SELECT Donation_Distribution_ID FROM [dbo].Donation_Distributions WHERE Donation_ID 
	IN (SELECT donation_id FROM @auto_temp_data WHERE donation_id IS NOT NULL));
	DELETE FROM [dbo].Donation_Distributions WHERE Donation_ID in (SELECT donation_id FROM @auto_temp_data WHERE donation_id IS NOT NULL);
	DELETE FROM [dbo].Donations WHERE Donation_ID in (SELECT donation_id FROM @auto_temp_data WHERE donation_id IS NOT NULL);

	DELETE FROM [dbo].Pledges WHERE Donor_ID in (SELECT donor_id FROM @auto_temp_data WHERE donor_id IS NOT NULL);
	DELETE FROM [dbo].Donors WHERE Donor_ID in (SELECT donor_id FROM @auto_temp_data WHERE donor_id IS NOT NULL);

	--user related
	--unlink user
	UPDATE [dbo].Contacts SET User_Account = null WHERE Contact_ID in (SELECT contact_id FROM @auto_temp_data);

	DELETE FROM [dbo].dp_User_Roles WHERE User_ID in (SELECT dp_users_id FROM @auto_temp_data WHERE dp_users_id IS NOT NULL);
	DELETE FROM [dbo].dp_Users WHERE User_ID in (SELECT dp_users_id FROM @auto_temp_data WHERE dp_users_id IS NOT NULL);

	--contact related
	DELETE FROM [dbo].Contact_Attributes WHERE Contact_ID in (SELECT contact_id FROM @auto_temp_data);
	DELETE FROM [dbo].cr_Connect_Communications WHERE To_Contact_ID in (SELECT contact_id FROM @auto_temp_data);
	DELETE FROM [dbo].dp_Communication_Messages WHERE Contact_ID in (SELECT contact_id FROM @auto_temp_data);
	DELETE FROM [dbo].dp_Communications WHERE To_Contact in (SELECT contact_id FROM @auto_temp_data);
	DELETE FROM [dbo].dp_Contact_Publications WHERE Contact_ID in (SELECT contact_id FROM @auto_temp_data);
	DELETE FROM [dbo].Activity_Log WHERE Contact_ID in (SELECT contact_id FROM @auto_temp_data);
	DELETE FROM [dbo].Contact_Relationships WHERE Contact_ID in (SELECT contact_id FROM @auto_temp_data) 
	OR Related_Contact_ID in (SELECT contact_id FROM @auto_temp_data);
	
	--Delete contacts
	DELETE FROM [dbo].Contacts WHERE Contact_ID in (SELECT contact_id FROM @auto_temp_data);

	--households related
	--unlink households
	UPDATE [dbo].Contacts SET Household_ID = null WHERE Contact_ID in (SELECT contact_id FROM @auto_temp_data);

	--DEBUG these households should not be deleted
	SELECT Household_ID as 'Non-empty households' FROM contacts where Household_ID in (SELECT household_id FROM @auto_temp_data WHERE household_id IS NOT NULL)

	--delete household if empty
	DECLARE @households_to_delete TABLE ( household_id int);
	INSERT INTO @households_to_delete (household_id) 
	(
	SELECT h.Household_ID FROM Households h
	WHERE h.Household_ID NOT IN (SELECT C.Household_ID FROM Contacts C WHERE Household_ID IS NOT NULL) 
	AND h.Household_ID NOT IN (SELECT CH.Household_ID FROM Contact_Households CH) 
	AND h.Household_ID NOT IN (SELECT CL.Household_ID FROM Household_Care_Log CL WHERE CL.Household_ID = h.Household_ID)
	AND h.Household_ID IN (SELECT household_id FROM @auto_temp_data WHERE household_id IS NOT NULL)
	);

	--DEBUG these households will be deleted
	SELECT household_id as 'Households to delete' FROM @households_to_delete;

	DELETE FROM [dbo].Activity_Log WHERE Household_ID in (SELECT household_id FROM @households_to_delete);
	DELETE FROM [dbo].Households WHERE Household_ID in (SELECT household_id FROM @households_to_delete);
END
GO
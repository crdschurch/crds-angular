use [MinistryPlatform]
GO

SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO


-- Called nightly to find and update Contacts whose Household_Position_ID is set
-- to 7 (Head of Household Spouse) and change it to 1 (Head of Household).
CREATE OR ALTER PROCEDURE [dbo].[crds_service_update_household_position_id]
AS
BEGIN

	BEGIN TRY
		BEGIN TRAN

			DECLARE @Audit_Records dbo.crds_Audit_Item

			UPDATE dbo.Contacts
			SET Household_Position_ID = 1
			OUTPUT
				'Contacts',
				INSERTED.Contact_ID,
				'Updated',
				'Household_Position_ID',
				'Household Position',
				'Head of Household Spouse',
				'Head of Household',
				DELETED.Household_Position_ID,
				INSERTED.Household_Position_ID
				INTO @Audit_Records
			WHERE Household_Position_ID = 7


			DECLARE @date DATETIME = GETDATE();
			EXEC crds_Add_Audit_Items @Audit_Records, @date, 'Svc Mngr', 0;

		COMMIT TRAN
	END TRY
	BEGIN CATCH
		IF @@TRANCOUNT > 0
			ROLLBACK TRAN
		PRINT 'crds_service_update_household_position_id failed: ' + COALESCE(ERROR_MESSAGE(), '');
	END CATCH

END

GO

CREATE OR ALTER   PROCEDURE [dbo].[service_church_specific]
    @DomainID INT
AS
BEGIN
    EXEC crds_service_assign_pledges_nightly @DomainID
    EXEC crds_service_clean_room_reservations_nightly
    EXEC crds_service_clean_donation_emails_nightly
    EXEC crds_service_update_donor_statement_parameters
	EXEC crds_service_update_household_position_id
    EXEC crds_service_update_email_nightly
END

GO


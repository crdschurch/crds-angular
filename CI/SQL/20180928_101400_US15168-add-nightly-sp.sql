USE [MinistryPlatform]
GO

CREATE OR ALTER PROCEDURE [dbo].[service_church_specific]
    @DomainID INT
AS
BEGIN
    EXEC crds_service_assign_pledges_nightly @DomainID
    EXEC crds_service_clean_room_reservations_nightly
    EXEC crds_service_clean_donation_emails_nightly
    EXEC crds_service_update_donor_statement_parameters
    EXEC crds_service_update_household_position_id
    EXEC crds_service_create_missing_households
    EXEC crds_service_update_email_nightly
END
GO

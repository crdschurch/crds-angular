USE [MinistryPlatform]
GO
/****** Object:  StoredProcedure [dbo].[crds_service_remove_empty_households]    Script Date: 9/21/2018 9:23:33 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

-- Called nightly to find and remove empty households
CREATE OR ALTER   PROCEDURE [dbo].[crds_service_remove_empty_households]
AS
BEGIN

	BEGIN TRY

		BEGIN TRAN

			/*
			 * Delete empty households from the database. An empty household has no primary residents
			 * and no residents with the household listed as their secondary residence either.
			 */
			select
				house.Household_ID
			into #NoPrimaryResidents
			from
				dbo.Households house with (nolock)
				left join dbo.Contacts cont with (nolock) on cont.Household_ID = house.Household_ID
			where
				cont.Household_ID is null

			select
				house.Household_ID
			into #NoSecondaryResidents
			from
				dbo.Households house with (nolock)
				left join dbo.Contact_Households cont with (nolock) on cont.Household_ID = house.Household_ID
			where
				cont.Household_ID is null

			select *
			from #NoPrimaryResidents npr
				inner join #NoSecondaryResidents nsr on npr.Household_ID = nsr.Household_ID

			DECLARE @Audit_Records_Activity_Log dbo.crds_Audit_Item

			delete from dbo.Activity_Log
			OUTPUT
				'Activity_Log',
				DELETED.Activity_Log_ID,
				'Deleted',
				'Household_ID',
				'Household ID',
				null,
				null,
				DELETED.Household_ID,
				null
				INTO @Audit_Records_Activity_Log
			where
				Household_ID in 
					(	select Household_ID 
						from #NoPrimaryResidents npr
							inner join #NoSecondaryResidents nsr on npr.Household_ID = nsr.Household_ID
					)

			DECLARE @date DATETIME = GETDATE();
			EXEC crds_Add_Audit_Items @Audit_Records_Activity_Log, @date, 'Svc Mngr', 0;

			DECLARE @Audit_Records_Households dbo.crds_Audit_Item

			delete from dbo.Households
			OUTPUT
				'Households',
				DELETED.Household_ID,
				'Deleted',
				'Household_ID',
				'Household ID',
				null,
				null,
				DELETED.Household_ID,
				null
				INTO @Audit_Records_Households
			where
				Household_ID in 
					(	select Household_ID 
						from #NoPrimaryResidents npr
							inner join #NoSecondaryResidents nsr on npr.Household_ID = nsr.Household_ID
					)

			EXEC crds_Add_Audit_Items @Audit_Records_Households, @date, 'Svc Mngr', 0;

		COMMIT TRAN
	END TRY
	BEGIN CATCH
		IF @@TRANCOUNT > 0
			ROLLBACK TRAN
		PRINT 'crds_service_remove_empty_households failed: ' + COALESCE(ERROR_MESSAGE(), '');
	END CATCH

	drop table #NoPrimaryResidents
	drop table #NoSecondaryResidents

END

GO

CREATE OR ALTER     PROCEDURE [dbo].[service_church_specific]
    @DomainID INT
AS
BEGIN
    EXEC crds_service_assign_pledges_nightly @DomainID
    EXEC crds_service_clean_room_reservations_nightly
    EXEC crds_service_clean_donation_emails_nightly
    EXEC crds_service_update_donor_statement_parameters
	EXEC crds_service_update_household_position_id
	EXEC crds_service_update_remove_empty_households
    EXEC crds_service_update_email_nightly
END

GO

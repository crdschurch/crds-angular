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
				h.Household_ID
			into #EmptyHouseholds
			from
				dbo.Households h with (nolock)
				left join dbo.Contacts c with (nolock) on c.Household_ID = h.Household_ID
				left join dbo.Contact_Households ch with (nolock) on ch.Household_ID = h.Household_ID
			where
				c.Household_ID is null
				and ch.Household_ID is null
			;

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
						from #EmptyHouseholds
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
						from #EmptyHouseholds
					)

			EXEC crds_Add_Audit_Items @Audit_Records_Households, @date, 'Svc Mngr', 0;

		COMMIT TRAN
	END TRY
	BEGIN CATCH
		IF @@TRANCOUNT > 0
			ROLLBACK TRAN
		PRINT 'crds_service_remove_empty_households failed: ' + COALESCE(ERROR_MESSAGE(), '');
	END CATCH

	drop table #EmptyHouseholds

END


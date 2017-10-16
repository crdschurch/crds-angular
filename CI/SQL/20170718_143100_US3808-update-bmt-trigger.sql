USE [MinistryPlatform]
GO

-- ================================================
-- Author:		Lakshmi Nair	
-- Create date: 3/24/2017
-- Description:	updating donation status to 
--              'Deposited' for BMT finalized batch
-- ================================================

ALTER TRIGGER [dbo].[crds_tr_Update_Donations_Status_for_Finalized_BMT_Batch]
  ON  [dbo].[Batches]
  AFTER UPDATE
AS
BEGIN
	IF UPDATE(Finalize_Date)
	BEGIN
		-- Sending Pending donations to Deposited
		UPDATE d
		SET Donation_Status_ID = 2
		FROM
			Batches b 
			INNER JOIN INSERTED ON INSERTED.Batch_ID = b.Batch_ID
			INNER JOIN Donations d ON d.Batch_ID = b.Batch_ID
		WHERE
			b.Batch_Entry_Type_ID = 12 AND
			d.Donation_Status_ID = 1
		;

		-- Reset the congregation ID on all distributions in the batch to match the
		-- current donor (BMT may have changed the donor)
		UPDATE dd
		SET
			HC_Donor_Congregation_ID = COALESCE(h.Congregation_Id, 5),
			Congregation_ID = COALESCE(h.Congregation_Id, 5)
		FROM
			Batches b 
			INNER JOIN INSERTED ON INSERTED.Batch_ID = b.Batch_ID
			INNER JOIN Donations d ON d.Batch_ID = b.Batch_ID
			INNER JOIN Donation_Distributions dd ON dd.Donation_ID = d.Donation_ID
			INNER JOIN [dbo].[Donors] do ON do.donor_id = d.donor_id
			INNER JOIN [dbo].[Contacts] c ON c.contact_id = do.contact_id
			LEFT JOIN [dbo].[Households] h ON h.household_id = c.household_id
		WHERE
			b.Batch_Entry_Type_ID = 12
		;
	END
END
GO

USE [MinistryPlatform]
GO

/****** Object:  Trigger [dbo].[crds_tr_Update_With_Congregation_Id]    Script Date: 5/22/2017 4:58:14 PM ******/
SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO


-- =============================================
-- Author:		Matt Brewer
-- Create date: 6/10/2015
-- Description:	Update with Congregation ID on every INSERT KD: The original trigger was for update as well
-- Updates: KD 5/23/17 
--			1. Only update congregation on Update donation id (since the donor comes from the donation and the congregation comes from the donor)
--				and only if soft_credit_donor is null
--			2. Update HC_Congregation_ID to the donor's congregation id also anytime the donation id is updated
--			2. Save the soft credit donor's congregation in congregation id 
--			   On update SC donor
--				1. If the SC Donor is null (we removed it), then the Congregation ID is set to the HC Donor Congregation field 
--				2. If the SC Donor is not null (we added or changed it), then Congregation ID is set to SC donor congregation id
--			3. Add audit logs for congregation_id updates
-- =============================================
ALTER TRIGGER [dbo].[crds_tr_Update_With_Congregation_Id] 
   ON  [dbo].[Donation_Distributions] 
   AFTER INSERT, UPDATE
AS 
BEGIN
	-- SET NOCOUNT ON added to prevent extra result sets from
	-- interfering with SELECT statements.
	SET NOCOUNT ON;

	DECLARE @Distribution_Id INT;
	DECLARE @Household_Id INT;
	DECLARE @Current_Congregation_Id INT;
	DECLARE @Soft_Credit_Updated INT;	

	IF UPDATE(Donation_ID) --IF UPDATE returns the TRUE value in INSERT actions because the columns have either explicit values or implicit (NULL) values inserted.
	BEGIN 
		DECLARE @Congregation_Id INT;

		SELECT @Distribution_Id = dd.Donation_Distribution_ID, @Household_Id = c.household_id, 
			   @Congregation_Id = h.congregation_id,@Current_Congregation_Id=dd.congregation_id,
			   @Soft_Credit_Updated = dd.Soft_Credit_Donor
			FROM [dbo].[Donation_Distributions] dd
			JOIN [dbo].[Donations] d ON d.donation_id = dd.donation_id
			JOIN [dbo].[Donors] do ON do.donor_id = d.donor_id
			JOIN [dbo].[Contacts] c ON c.contact_id = do.contact_id
			LEFT JOIN [dbo].[Households] h ON h.household_id = c.household_id
			JOIN INSERTED ON INSERTED.Donation_Distribution_ID = dd.Donation_Distribution_ID; 

		-- Set to 'No Site Specified' if Household has no Congregation		
		IF (@Household_Id IS NULL) OR (@Congregation_Id IS NULL)
			SET @Congregation_Id = 5;

		UPDATE [dbo].[Donation_Distributions]
			SET HC_Donor_Congregation_ID = @Congregation_Id
			WHERE Donation_Distribution_ID = @Distribution_Id;

		IF @Soft_Credit_Updated IS NULL --don't update congregation_id unless soft credit donor is null
		BEGIN		 
			UPDATE [dbo].[Donation_Distributions]
			SET Congregation_ID = @Congregation_Id
			WHERE Donation_Distribution_ID = @Distribution_Id;
			      
			EXEC crds_Add_Audit 
				 @TableName='Donation_Distributions'
				,@Record_ID=@Distribution_Id
				,@Audit_Description='Updated'
				,@UserName='Svc Mngr'
				,@UserID =0
				,@FieldName='Congregation_ID'
				,@FieldLabel='Congregation'
				,@PreviousValue=@Current_Congregation_Id
				,@NewValue=@Congregation_Id	
		END
	END --end update donation id 
	

    --If the soft_credit_donor was updated, removed, or added
	IF UPDATE(Soft_Credit_Donor)  --IF UPDATE returns the TRUE value in INSERT actions because the columns have either explicit values or implicit (NULL) values inserted.
	BEGIN
	   
			
		DECLARE @Soft_Credit_Congregation_Id INT;	
		DECLARE @HC_Congregation_Id INT;	
			
		SELECT @Distribution_Id = INSERTED.Donation_Distribution_ID, @Household_Id = c.household_id, 
			@Soft_Credit_Congregation_Id = h.congregation_id, @Soft_Credit_Updated=dd.soft_credit_donor, 
			@Current_Congregation_Id=dd.congregation_id, @HC_Congregation_Id=dd.HC_Donor_Congregation_ID
			FROM [dbo].[Donation_Distributions] dd		
			JOIN INSERTED ON INSERTED.Donation_Distribution_ID = dd.Donation_Distribution_ID --need this join up here in case the soft_credit_donor was removed
			LEFT JOIN [dbo].[Donors] do ON do.donor_id = dd.soft_credit_donor
			LEFT JOIN [dbo].[Contacts] c ON c.contact_id = do.contact_id
			LEFT JOIN [dbo].[Households] h ON h.household_id = c.household_id ;
				
			
		IF (@Soft_Credit_Updated IS NOT NULL) --it is set to something
		BEGIN
			-- Set to 'No Site Specified' if Household has no Congregation		
			IF (@Household_Id IS NULL) OR (@Soft_Credit_Congregation_Id IS NULL)
				SET @Soft_Credit_Congregation_Id = 5;

			IF @HC_Congregation_ID IS NULL
			BEGIN
				UPDATE [dbo].[Donation_Distributions]
				SET HC_Donor_Congregation_ID = @Current_Congregation_Id
				WHERE Donation_Distribution_ID = @Distribution_Id;
			END 

			UPDATE [dbo].[Donation_Distributions]
			SET Congregation_ID = @Soft_Credit_Congregation_Id
			WHERE Donation_Distribution_ID = @Distribution_Id;

			EXEC crds_Add_Audit 
				 @TableName='Donation_Distributions'
				,@Record_ID=@Distribution_Id
				,@Audit_Description='Updated'
				,@UserName='Svc Mngr'
				,@UserID =0
				,@FieldName='Congregation_ID'
				,@FieldLabel='Congregation'
				,@PreviousValue=@Current_Congregation_Id
				,@NewValue=@Soft_Credit_Congregation_Id
		END --end only update if they actually set it
		ELSE IF (SELECT Soft_Credit_Donor FROM DELETED) IS NOT NULL --they must've deleted it because it existed before
		BEGIN 
				
			UPDATE [dbo].[Donation_Distributions]
			SET Congregation_ID = @HC_Congregation_Id --put it back to the hard credit one if they remove the soft credit donor
			WHERE Donation_Distribution_ID = @Distribution_Id;

			EXEC crds_Add_Audit 
				 @TableName='Donation_Distributions'
				,@Record_ID=@Distribution_Id
				,@Audit_Description='Updated'
				,@UserName='Svc Mngr'
				,@UserID =0
				,@FieldName='Congregation_ID'
				,@FieldLabel='Congregation'
				,@PreviousValue=@Current_Congregation_Id
				,@NewValue=@HC_Congregation_Id				
		END			

	END --end if soft credit 
	 
END --end update trigger


GO



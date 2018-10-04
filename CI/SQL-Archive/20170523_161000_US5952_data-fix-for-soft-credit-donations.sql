USE [MinistryPlatform]
GO

SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO

DECLARE @dd_id INT, @HC_Donor_Congregation_ID INT, @Old_Congregation_ID INT, @Congregation_ID INT

SELECT 
    Donation_Distribution_id,
    HC_Donor_Congregation_ID = ISNULL(HC_Donor_Congregation_ID,dd.Congregation_ID),
	Old_Congregation_ID = dd.congregation_id,
	Congregation_ID = 
	  CASE
		WHEN Soft_Credit_Donor IS NOT NULL THEN ISNULL(h.congregation_Id, dd.Congregation_ID) --if it's null then leave it alone
		ELSE dd.Congregation_ID
	  END
 INTO #NeedToUpdate
 FROM Donation_Distributions dd
 JOIN Donors d ON dd.Soft_Credit_Donor = d.donor_id
 JOIN Contacts c on d.contact_id = c.contact_id
 LEFT JOIN Households h on c.household_id = h.household_id
WHERE Soft_Credit_Donor IS NOT NULL 


DECLARE CursorPUTT CURSOR FAST_FORWARD FOR 
SELECT Donation_Distribution_id,HC_Donor_Congregation_ID, Old_Congregation_ID, Congregation_ID FROM #NeedToUpdate

OPEN CursorPUTT
FETCH NEXT FROM CursorPUTT INTO @dd_id, @HC_Donor_Congregation_ID, @Old_Congregation_ID, @Congregation_ID
	WHILE @@FETCH_STATUS = 0
	BEGIN

	UPDATE Donation_Distributions
	SET Congregation_ID = @Congregation_Id,
		HC_Donor_Congregation_ID = @HC_Donor_Congregation_ID
	WHERE Donation_Distribution_ID = @dd_id

	 IF (@Old_Congregation_ID <> @Congregation_Id)
		EXEC crds_Add_Audit 
			 @TableName='Donation_Distributions'
			,@Record_ID=@dd_id
			,@Audit_Description='Mass Updated'
			,@UserName='Svc Mngr'
			,@UserID =0
			,@FieldName='Congregation_ID'
			,@FieldLabel='Congregation'
			,@PreviousValue=@Old_Congregation_ID
			,@NewValue=@Congregation_Id	

					
		FETCH NEXT FROM CursorPUTT INTO @dd_id, @HC_Donor_Congregation_ID, @Old_Congregation_ID, @Congregation_ID
			
	END
CLOSE CursorPUTT
DEALLOCATE CursorPUTT



DROP TABLE #NeedToUpdate
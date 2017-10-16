USE [MinistryPlatform]
GO

/****** Object:  StoredProcedure [dbo].[api_crds_Get_Contact_Donor]    Script Date: 7/13/2017 10:34:39 AM ******/
SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO



CREATE PROCEDURE [dbo].[api_crds_Get_Contact_Donor] (
	@Contact_ID INT
)
AS
BEGIN
	SET NOCOUNT ON;

	SELECT c.Contact_ID AS ContactId,
	       d.Donor_ID AS DonorId, 
		   d.Processor_ID AS ProcessorId,
		   sf.Statement_Frequency AS StatementFreq,
		   st.statement_type AS StatementType,
		   sm.statement_method AS StatementMethod, 
		   c.email_address AS Email, 
		   d.statement_type_id AS StatementTypeId,
		   c.first_name AS FirstName, 
		   c.last_name AS LastName,
		   c.household_id AS HouseholdId,
		   1 AS RegisteredUser
	FROM 
		   Contacts c
		   JOIN Donors d on c.contact_id = d.contact_id
		   JOIN Statement_Frequencies sf on d.statement_frequency_id = sf.Statement_Frequency_id
		   JOIN Statement_Types st ON d.Statement_Type_ID=st.Statement_Type_ID
		   JOIN Statement_Methods sm ON d.Statement_Method_ID = sm.Statement_Method_ID

	WHERE 
		   c.Contact_ID = @Contact_ID


END


GO



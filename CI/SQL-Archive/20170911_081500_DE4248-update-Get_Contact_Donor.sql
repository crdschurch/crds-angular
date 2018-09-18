USE [MinistryPlatform]
GO

ALTER PROCEDURE [dbo].[api_crds_Get_Contact_Donor] (
	@Contact_ID INT
)
AS
BEGIN
	SET NOCOUNT ON;

	SELECT c.Contact_ID AS ContactId,
	       COALESCE(d.Donor_ID, 0) AS DonorId, 
		   d.Processor_ID AS ProcessorId,
		   sf.Statement_Frequency AS StatementFreq,
		   st.statement_type AS StatementType,
		   sm.statement_method AS StatementMethod, 
		   c.email_address AS Email, 
		   COALESCE(d.statement_type_id, 0) AS StatementTypeId,
		   c.first_name AS FirstName, 
		   c.last_name AS LastName,
		   c.household_id AS HouseholdId,
		   c.Display_Name AS DisplayName,
		   1 AS RegisteredUser
	FROM 
		   Contacts c
		   LEFT JOIN Donors d on c.contact_id = d.contact_id
		   LEFT JOIN Statement_Frequencies sf on d.statement_frequency_id = sf.Statement_Frequency_id
		   LEFT JOIN Statement_Types st ON d.Statement_Type_ID = st.Statement_Type_ID
		   LEFT JOIN Statement_Methods sm ON d.Statement_Method_ID = sm.Statement_Method_ID
	WHERE 
		   c.Contact_ID = @Contact_ID

END
GO

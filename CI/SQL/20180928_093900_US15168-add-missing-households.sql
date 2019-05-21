USE [MinistryPlatform]
GO

-- Called nightly to ensure that all Contacts (except Guest Givers) have a Household record
-- This was inspired by Mike Fuhrman's data cleanup scripts [9/28/2018 - US15168]
CREATE OR ALTER PROCEDURE [dbo].[crds_service_create_missing_households]
AS
BEGIN
	BEGIN TRY
		BEGIN TRAN

        DECLARE @NewHouseholds TABLE (
            Contact_ID INT NOT NULL,
            Household_ID INT NOT NULL,
            Household_Name NVARCHAR(75) NOT NULL,

            PRIMARY KEY(Contact_ID)
        );

        -- Create Household records for all Contacts who do not already have a Household record
        -- Using MERGE instead of a simple INSERT so we have access to Contact_ID in the OUTPUT clause
        MERGE Households AS h
        USING Contacts AS c ON c.Household_ID = h.Household_ID
        WHEN NOT MATCHED AND c.Display_Name <> 'Guest Giver' AND c.Contact_ID > 100 THEN
            INSERT (Household_Name, Domain_ID, Congregation_ID)
            VALUES (COALESCE(c.Last_Name, c.Display_Name), c.Domain_ID, 5)
        OUTPUT
            c.Contact_ID, INSERTED.Household_ID, INSERTED.Household_Name
        INTO @NewHouseholds
            (Contact_ID, Household_ID, Household_Name)
        ;

        -- Update Contact records to link to newly created Households
        UPDATE c
        SET
            c.Household_ID = h.Household_ID
        FROM
            Contacts c
            INNER JOIN @NewHouseholds h ON h.Contact_ID = c.Contact_ID
        ;

        DECLARE @Audit_Records dbo.crds_Audit_Item

        -- Audit entries for new Households
        INSERT INTO @Audit_Records
            (Table_Name, Record_ID, Audit_Description)
        SELECT
            'Households', Household_ID, 'Created'
        FROM
            @NewHouseholds
        ORDER BY
            Household_ID
        ;

        -- Audit entries for updated Contacts
        INSERT INTO @Audit_Records
            (Table_Name, Record_ID, Audit_Description, Field_Name, Field_Label, New_Value, New_ID)
        SELECT
            'Contacts', Contact_ID, 'Updated', 'Household_ID', 'Household', Household_Name, Household_ID
        FROM
            @NewHouseholds
        ORDER BY
            Household_ID
        ;

        DECLARE @date DATETIME = GETDATE();
        EXEC crds_Add_Audit_Items @Audit_Records, @date, 'Svc Mngr', 0;

		COMMIT TRAN

        DECLARE @NumHouses INT;
        SELECT @NumHouses = COUNT(*) FROM @NewHouseholds;
        PRINT 'crds_service_create_missing_households: created ' + CONVERT(VARCHAR(20), @NumHouses) + ' households';
	END TRY
	BEGIN CATCH
		IF @@TRANCOUNT > 0
			ROLLBACK TRAN
		PRINT 'crds_service_create_missing_households failed: ' + COALESCE(ERROR_MESSAGE(), '');
	END CATCH
END
GO

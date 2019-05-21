USE [MinistryPlatform]
GO

-- Called nightly to find and update Donors who have made a recent donation, but have
-- Statement Frequency = Never.  We reset Statement Frequency and Statement Method
-- to ensure these Donors will get a statement.  This resolves issues where someone
-- first gives as a Guest Giver (Donor record will be set to Statement Frequency = Never),
-- then the Guest Giver record is later merged with another Contact via the Combine Contacts
-- tool.  The logic below is based on a query provided by Mike Fuhrman [September 2018]
CREATE PROCEDURE crds_service_update_donor_statement_parameters
AS
BEGIN
    DECLARE @Min_Date DATETIME = GETDATE() - 180;       -- look back 6 months

    DECLARE @Donor_Changes TABLE (
        Donor_ID INT NOT NULL,
        PRIMARY KEY(Donor_ID)
    );

    -- list of Donors whose Statement Type is "Never" but have made at least one donation recently
    INSERT INTO @Donor_Changes
        (Donor_ID)
    SELECT
        do.Donor_ID
    FROM
        Donors do
        INNER JOIN Contacts c ON c.Contact_ID = do.Contact_ID
    WHERE
        do.Statement_Frequency_ID = 3           -- Never
        AND c.Contact_Status_ID = 1             -- Active
        AND c.Display_Name <> 'Guest Giver'
        AND c.Company <> 1
        AND EXISTS (
            SELECT 1
            FROM
                Donations d
                INNER JOIN Donation_Distributions dd ON dd.Donation_ID = d.Donation_ID
                INNER JOIN Programs p ON p.Program_ID = dd.Program_ID
            WHERE
                d.Donor_ID = do.Donor_ID
                AND p.Program_Type_ID = 1     -- Fuel
                AND d.Donation_Date >= @Min_Date
        )
    ;

    BEGIN TRY
        BEGIN TRAN

        DECLARE @Data_Changes TABLE (
            Donor_ID INT NOT NULL,
            New_Statement_Frequency_ID INT NOT NULL,
            New_Statement_Method_ID INT NOT NULL,
            Old_Statement_Frequency_ID INT NOT NULL,
            Old_Statement_Method_ID INT NOT NULL,

            PRIMARY KEY(Donor_ID)
        );

        -- Update Donor records to modify Statement Frequency and Statement Method
        UPDATE Donors
        SET
            Statement_Frequency_ID = 1,
            Statement_Method_ID = 2
        OUTPUT
            INSERTED.Donor_ID,
            INSERTED.Statement_Frequency_ID,
            INSERTED.Statement_Method_ID,
            DELETED.Statement_Frequency_ID,
            DELETED.Statement_Method_ID
        INTO @Data_Changes
        WHERE
            Donor_ID IN (SELECT Donor_ID FROM @Donor_Changes)
        ;

        DECLARE @Audit_Records crds_Audit_Item;

        -- Generate audit log entries for changes to Statement Method and/or Statement Frequency
        INSERT INTO @Audit_Records
            (Table_Name, Record_ID, Audit_Description, Field_Name, Field_Label, Previous_Value, New_Value, Previous_ID, New_ID)
        SELECT
            'Donors',
            dc.Donor_ID,
            'Updated',
            'Statement_Frequency_ID',
            'Stmt Frequency',
            sf1.Statement_Frequency,
            sf2.Statement_Frequency,
            dc.Old_Statement_Frequency_ID,
            dc.New_Statement_Frequency_ID
        FROM
            @Data_Changes dc
            LEFT JOIN Statement_Frequencies sf1 on sf1.Statement_Frequency_ID = dc.Old_Statement_Frequency_ID
            LEFT JOIN Statement_Frequencies sf2 on sf2.Statement_Frequency_ID = dc.New_Statement_Frequency_ID
        WHERE
            dc.Old_Statement_Frequency_ID <> dc.New_Statement_Frequency_ID
        UNION
        SELECT
            'Donors',
            dc.Donor_ID,
            'Updated',
            'Statement_Method_ID',
            'Stmt Method',
            sm1.Statement_Method,
            sm2.Statement_Method,
            dc.Old_Statement_Method_ID,
            dc.New_Statement_Method_ID
        FROM
            @Data_Changes dc
            LEFT JOIN Statement_Methods sm1 on sm1.Statement_Method_ID = dc.Old_Statement_Method_ID
            LEFT JOIN Statement_Methods sm2 on sm2.Statement_Method_ID = dc.New_Statement_Method_ID
        WHERE
            dc.Old_Statement_Method_ID <> dc.New_Statement_Method_ID
        ORDER BY
            dc.Donor_ID
        ;

        DECLARE @date DATETIME = GETDATE();
        EXEC crds_Add_Audit_Items @Audit_Records, @date, 'Svc Mngr', 0;

        COMMIT TRAN
    END TRY
    BEGIN CATCH
        IF @@TRANCOUNT > 0
            ROLLBACK TRAN

        PRINT 'crds_service_update_donor_statement_parameters failed: ' + COALESCE(ERROR_MESSAGE(), '');
    END CATCH
END
GO

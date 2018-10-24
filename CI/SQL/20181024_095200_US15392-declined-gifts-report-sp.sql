USE [MinistryPlatform]
GO

CREATE OR ALTER PROCEDURE [dbo].[report_CRDS_Declined_Recurring_Gifts] 
     @NumDaysOfHistory INT = 365
AS
BEGIN
    SET NOCOUNT ON;

    DECLARE @StartDate DATETIME = CAST(GETDATE() - @NumDaysOfHistory AS DATE);

    DECLARE @DeclinedGifts TABLE (
        Recurring_Gift_ID INT NOT NULL,
        Donor_ID INT NOT NULL,
        Num_Consecutive_Declines INT NOT NULL,

        PRIMARY KEY(Recurring_Gift_ID)
    );

    -- NOTE: The queries below only consider Deposited and Declined.  Donations with other statuses
    -- such as Pending are ignored when determining consecutive declines.
    WITH DepositsAndDeclines (Recurring_Gift_ID, Donor_ID, Donation_Date, Donation_Status_ID, Row_Num)
    AS (
        SELECT
            rg.Recurring_Gift_ID,
            rg.Donor_ID,
            d.Donation_Date,
            d.Donation_Status_ID,
            ROW_NUMBER() OVER (PARTITION BY rg.Recurring_Gift_ID ORDER BY d.Donation_Date DESC) Row_Num
        FROM
            Recurring_Gifts rg
            INNER JOIN Donations d ON d.Donor_ID = rg.Donor_ID AND d.Recurring_Gift_ID = rg.Recurring_Gift_ID
        WHERE
            COALESCE(rg.End_Date, '9999-12-31') > GETDATE()     -- exclude end dated
            AND d.Donation_Date >= @StartDate
            AND d.Donation_Status_ID IN (2, 3)	-- deposited, declined
    ),
    DepositsOnly (Recurring_Gift_ID, Row_Num, Deposit_Row_Num)
    AS (
        SELECT
            Recurring_Gift_ID,
            Row_Num,
            ROW_NUMBER() OVER (PARTITION BY Recurring_Gift_ID ORDER BY Row_Num) AS Deposit_Row_Num
        FROM
            DepositsAndDeclines
        WHERE
            Donation_Status_ID = 2		-- deposited
    ),
    CountDonations (Recurring_Gift_ID, Num_Donations)
    AS (
        SELECT
            Recurring_Gift_ID,
            Num_Donations = COUNT(*) 
        FROM
            DepositsAndDeclines
        GROUP BY
            Recurring_Gift_ID
    )

    INSERT INTO @DeclinedGifts
        (Recurring_Gift_ID, Donor_ID, Num_Consecutive_Declines)
    SELECT
        dd.Recurring_Gift_ID,
        dd.Donor_ID,
        Num_Consecutive_Declines = CASE WHEN d.Row_Num IS NOT NULL THEN d.Row_Num - 1 ELSE c.Num_Donations END
    FROM
        DepositsAndDeclines dd
        LEFT JOIN DepositsOnly d ON d.Recurring_Gift_ID = dd.Recurring_Gift_ID AND dd.Row_Num = d.Deposit_Row_Num
        LEFT JOIN CountDonations c ON c.Recurring_Gift_ID = dd.Recurring_Gift_ID
    WHERE
        dd.Row_Num = 1
        AND dd.Donation_Status_ID = 3
    ;

    SELECT
        c.Display_Name,
        c.Email_Address,
        rg.Donor_ID,
        rg.Recurring_Gift_ID,
        rg.Start_Date,
        rgf.Frequency,
        rg.Amount,
        rgs.Recurring_Gift_Status,
        dg.Num_Consecutive_Declines,
        rg.Updated_On,
        rg.Vendor_Admin_Detail_Url
    FROM
        @DeclinedGifts dg
        INNER JOIN Recurring_Gifts rg ON rg.Recurring_Gift_ID = dg.Recurring_Gift_ID
        INNER JOIN cr_Recurring_Gift_Status rgs ON rgs.Recurring_Gift_Status_ID = rg.Recurring_Gift_Status_ID
        INNER JOIN Recurring_Gift_Frequencies rgf ON rgf.Frequency_ID = rg.Frequency_ID
        LEFT JOIN Contacts c ON c.Donor_Record = dg.Donor_ID
    ORDER BY
        c.Display_Name,
        rg.Recurring_Gift_ID
    ;
END
GO

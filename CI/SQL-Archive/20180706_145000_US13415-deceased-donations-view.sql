USE [MinistryPlatform]
GO

CREATE OR ALTER VIEW [dbo].[vw_crds_Deceased_Donations]
AS
        -- regular donors
        SELECT
            do.Donor_ID,
            c.Display_Name,
            do.Notes AS Donor_Notes,
            c2.Display_Name AS [Co-Giver_Name],
            d.Donation_ID,
            d.Donation_Date,
            d.Donation_Amount,
            dd.Amount AS Distribution_Amount,
            pt.Payment_Type,
            d.Is_Recurring_Gift,
            do.Domain_ID
        FROM
            Contacts c
            inner join Donors do on do.Donor_ID = c.Donor_Record
            inner join Donations d on d.Donor_ID = do.Donor_ID and d.Donation_Date >= c.Date_Of_Death
            inner join Donation_Distributions dd on dd.Donation_ID = d.Donation_ID
            inner join Payment_Types pt on pt.Payment_Type_ID = d.Payment_Type_ID
            left join Contact_Relationships cr on cr.Contact_ID = c.Contact_ID and cr.Relationship_ID = 42
                and cr.Start_Date < GETDATE() and COALESCE(cr.End_Date, '9999-12-31') > GETDATE()
            left join Contacts c2 on c2.Contact_ID = cr.Related_Contact_ID
        WHERE
            c.Contact_Status_ID = 3		-- deceased
            and c.Date_Of_Death is not null
            and d.Donation_Date >= DATEADD(month, -13, GETDATE())       -- look back only 13 months
    UNION
        -- soft credit donors
        SELECT
            do.Donor_ID,
            c.Display_Name,
            do.Notes AS Donor_Notes,
            c2.Display_Name AS [Co-Giver_Name],
            d.Donation_ID,
            d.Donation_Date,
            d.Donation_Amount,
            dd.Amount AS Distribution_Amount,
            pt.Payment_Type,
            d.Is_Recurring_Gift,
            do.Domain_ID
        FROM
            Donation_Distributions dd
            inner join Donations d on d.Donation_ID = dd.Donation_ID
            inner join Donors do on do.Donor_ID = dd.Soft_Credit_Donor
            inner join Contacts c on c.Contact_ID = do.Contact_ID
            inner join Payment_Types pt on pt.Payment_Type_ID = d.Payment_Type_ID
            left join Contact_Relationships cr on cr.Contact_ID = c.Contact_ID and cr.Relationship_ID = 42
                and cr.Start_Date < GETDATE() and COALESCE(cr.End_Date, '9999-12-31') > GETDATE()
            left join Contacts c2 on c2.Contact_ID = cr.Related_Contact_ID
        WHERE
            c.Contact_Status_ID = 3		-- deceased
            and c.Date_Of_Death is not null
            and d.Donation_Date >= c.Date_Of_Death
            and d.Donation_Date >= DATEADD(month, -13, GETDATE())       -- look back only 13 months
        ;
GO

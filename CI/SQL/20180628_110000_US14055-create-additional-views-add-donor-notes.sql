USE [MinistryPlatform]
GO

SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO

UPDATE [dbo].[dp_Pages]
   SET [Default_Field_List] = 'ContactID, Relationship, DonorId, DisplayName, StatementMethod, EmailAddress, Congregation, DonorNotes'
 WHERE Page_ID in (1018, 1020, 1021, 1022)
GO

ALTER FUNCTION [dbo].[crds_QuarterlyGivingStatementDonors](
						@Quarter		int
					)
RETURNS @DonorsTable TABLE(		ContactId	int,
								Relationship varchar(10),
								DonorId		int,
								DisplayName	nvarchar(75),
								StatementMethod  nvarchar(50),
								EmailAddress varchar(255),
								Congregation nvarchar(50),
								DonorNotes nvarchar(500)
						 )
AS
     BEGIN
        DECLARE @CurrentDate DATETIME = GETDATE();
        DECLARE @Year INT = YEAR(@CurrentDate);

        -- for Q4, show last year's data during the 1st half of the year
        IF @Quarter = 4 AND MONTH(@CurrentDate) < 7
        BEGIN
            SET @Year = @Year - 1;
        END

        DECLARE @BeginDate DATETIME = DATEFROMPARTS(@Year, 1, 1);
        DECLARE @EndDate DATETIME =
            CASE WHEN @Quarter = 1 THEN DATEADD(DAY, 1, DATEFROMPARTS(@Year, 3, 31))
                 WHEN @Quarter = 2 THEN DATEADD(DAY, 1, DATEFROMPARTS(@Year, 6, 30))
                 WHEN @Quarter = 3 THEN DATEADD(DAY, 1, DATEFROMPARTS(@Year, 9, 30))
                 WHEN @Quarter = 4 THEN DATEADD(DAY, 1, DATEFROMPARTS(@Year, 12, 31))
            END;

        DECLARE @DonorIDs TABLE (
            Contact_ID INT NOT NULL,
            Donor_ID INT NOT NULL,
            PRIMARY KEY(Contact_ID)
        );

        -- list of unique donor IDs (with contact ID) that have given in the desired timeframe
        INSERT INTO @DonorIDs
            (Contact_ID, Donor_ID)
        SELECT DISTINCT
            do.Contact_ID, do.Donor_ID
        FROM
            dbo.Donors do
            INNER JOIN dbo.Donations d on d.Donor_ID = do.Donor_ID
        WHERE
            d.Donation_Status_ID IN (2)
            AND d.Donation_Date >= @BeginDate
            AND d.Donation_Date < @EndDate
        ;

        -- givers
        INSERT INTO @DonorsTable
        SELECT
            c.Contact_ID as ContactID,
			'Giver',
            do.Donor_ID as DonorId,
            c.Display_Name as DisplayName,
            sm.Statement_Method as StatementMethod,
            c.Email_Address as EmailAddress,
            co.Congregation_Name as Congregation,
			do.Notes as DonorNotes
        FROM
            Contacts c
            INNER JOIN @DonorIDs di on di.Contact_ID = c.Contact_ID
            INNER JOIN Donors do on do.Donor_ID = di.Donor_ID
            INNER JOIN Statement_Methods sm on sm.Statement_Method_ID = do.Statement_Method_ID
            LEFT OUTER JOIN Households h on c.Household_ID = h.Household_ID
            LEFT OUTER JOIN Congregations co on co.Congregation_ID = h.Congregation_ID
        ;

        -- co-givers who are not already in the givers list (i.e., did not donate)
        INSERT INTO @DonorsTable
        SELECT
            c.Contact_ID as ContactID, 
			'Cogiver',
            do.Donor_ID as DonorId,
            c.Display_Name as DisplayName,
            sm.Statement_Method as StatementMethod,
            c.Email_Address as EmailAddress,
            co.Congregation_Name as Congregation,
			do.Notes as DonorNotes
        FROM
            @DonorIDs di
            INNER JOIN Contact_Relationships cr on cr.Contact_ID = di.Contact_ID and cr.Relationship_ID = 42
			INNER JOIN Contacts c on c.Contact_ID = cr.Related_Contact_ID
            LEFT OUTER JOIN Donors do on do.Donor_ID = c.Donor_Record
            LEFT OUTER JOIN Statement_Methods sm on sm.Statement_Method_ID = do.Statement_Method_ID
            LEFT OUTER JOIN Households h on c.Household_ID = h.Household_ID
            LEFT OUTER JOIN Congregations co on co.Congregation_ID = h.Congregation_ID
        WHERE
			@BeginDate < coalesce(cr.End_Date, '9999-12-31') 
			and @EndDate > cr.Start_Date
            and NOT EXISTS (SELECT 1 FROM @DonorIDs di WHERE c.Contact_ID = di.Contact_ID)
        ;	

    RETURN;
    END;
    GO

ALTER VIEW [dbo].[vw_crds_Q1_Giving_Statements]
AS
select ContactID, Relationship, DonorId, DisplayName, StatementMethod, EmailAddress, Congregation, DonorNotes, 1 AS Domain_ID 
from crds_QuarterlyGivingStatementDonors(1)
GO

ALTER VIEW [dbo].[vw_crds_Q2_Giving_Statements]
AS
select ContactID, Relationship, DonorId, DisplayName, StatementMethod, EmailAddress, Congregation, DonorNotes, 1 AS Domain_ID 
from crds_QuarterlyGivingStatementDonors(2)
GO

ALTER VIEW [dbo].[vw_crds_Q3_Giving_Statements]
AS
select ContactID, Relationship, DonorId, DisplayName, StatementMethod, EmailAddress, Congregation, DonorNotes, 1 AS Domain_ID 
from crds_QuarterlyGivingStatementDonors(3)
GO

ALTER VIEW [dbo].[vw_crds_Q4_Giving_Statements]
AS
select ContactID, Relationship, DonorId, DisplayName, StatementMethod, EmailAddress, Congregation, DonorNotes, 1 AS Domain_ID 
from crds_QuarterlyGivingStatementDonors(4)
GO
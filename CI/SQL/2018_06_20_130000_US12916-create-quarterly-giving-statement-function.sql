USE [MinistryPlatform]
GO

/****** Object:  UserDefinedFunction [dbo].[crds_QuarterlyGivingStatementDonors]    Script Date: 6/19/2018 1:07:02 PM ******/
SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO

CREATE FUNCTION [dbo].[crds_QuarterlyGivingStatementDonors](
						@Quarter		int
					)
RETURNS @DonorsTable TABLE(		ContactId	int,
								DonorId		int,
								DisplayName	nvarchar(75),
								StatementMethod  nvarchar(50),
								EmailAddress varchar(255),
								Congregation nvarchar(50)
						 )
AS
     BEGIN
	 	DECLARE @BeginDate DATETIME = 
			CASE WHEN @Quarter < 4 THEN DATEFROMPARTS (YEAR(getdate()), 1, 1 )--Q1-Q3 always show current year
				 WHEN @Quarter = 4 AND MONTH(getdate()) >= 7 THEN DATEFROMPARTS (YEAR(getdate()), 1, 1 )--show current year
				 WHEN @Quarter = 4 AND MONTH(getdate()) < 7 THEN DATEFROMPARTS (YEAR(getdate())-1, 1, 1 )--show previous year
			END
		DECLARE @EndDate DATETIME =
			CASE WHEN @Quarter = 1 THEN DATEFROMPARTS (YEAR(getdate()), 3, 31 )
				 WHEN @Quarter = 2 THEN DATEFROMPARTS (YEAR(getdate()), 6, 30 )
			     WHEN @Quarter = 3 THEN DATEFROMPARTS (YEAR(getdate()), 9, 30 )
				 WHEN @Quarter = 4 AND MONTH(getdate()) >=7 THEN DATEFROMPARTS (YEAR(getdate()), 12, 31 )--show current year
				 WHEN @Quarter = 4 AND MONTH(getdate()) < 7 THEN DATEFROMPARTS (YEAR(getdate())-1, 12, 31 )--show previous year
			END;

		INSERT INTO @DonorsTable
		SELECT c.Contact_ID as ContactID, 
				d.Donor_ID as DonorId,
				c.Display_Name as DisplayName,
				sm.Statement_Method as StatementMethod,
				c.Email_Address as EmailAddress,
				co.Congregation_Name as Congregation
		FROM Contacts c
			inner join Donors d on d.Donor_ID = c.Donor_Record
			inner join Statement_Methods sm on sm.Statement_Method_ID = d.Statement_Method_ID
			left outer join Households h on c.Household_ID = h.Household_ID
			left outer join Congregations co on co.Congregation_ID = h.Congregation_ID
		WHERE c.Contact_ID in
		(
			SELECT c.Contact_ID
			FROM dbo.Contacts c
				inner join dbo.Donors d on d.Donor_ID = c.Donor_Record
			WHERE Exists (SELECT 1 FROM dbo.Donations ds						
							WHERE ds.Donor_ID = d.Donor_ID
								and ds.Donation_Status_ID in (2)
								and ds.Donation_Date >= @BeginDate 
								and ds.Donation_Date <= @EndDate
							)
			)

		INSERT INTO @DonorsTable
		SELECT c.Contact_ID as ContactID, 
				d.Donor_ID as DonorId,
				c.Display_Name as DisplayName,
				sm.Statement_Method as StatementMethod,
				c.Email_Address as EmailAddress,
				co.Congregation_Name as Congregation
		FROM Contacts c
			inner join Donors d on d.Donor_ID = c.Donor_Record
			inner join Statement_Methods sm on sm.Statement_Method_ID = d.Statement_Method_ID
			inner join dbo.Contact_Relationships cr on cr.Contact_ID = c.Contact_ID and cr.Relationship_ID = 42
			left outer join Households h on c.Household_ID = h.Household_ID
			left outer join Congregations co on co.Congregation_ID = h.Congregation_ID
		where c.Contact_ID not in
			(
				select c.Contact_ID
				from dbo.Contacts c
					inner join dbo.Donors d on d.Donor_ID = c.Donor_Record
				where Exists (select 1 from dbo.Donations ds						
							where ds.Donor_ID = d.Donor_ID
								and ds.Donation_Status_ID in (2)
								and ds.Donation_Date >= @BeginDate 
								and ds.Donation_Date <= @EndDate
					)
		)		

    RETURN;
    END;

GO



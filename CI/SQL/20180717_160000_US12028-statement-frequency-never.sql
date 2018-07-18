USE [MinistryPlatform]
GO
/****** Object:  StoredProcedure [dbo].[report_CRDS_Donor_Contribution_Statement]    Script Date: 7/17/2018 9:43:57 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

-- Created 4/6/2017 for Paper Statements

CREATE OR ALTER PROCEDURE [dbo].[report_CRDS_Donor_Contribution_Statement] (
    @UserID varchar(40), 
	@FromDate DateTime,
	@ToDate DateTime,
	@AccountingCompanyID Int,
	@Congregations varchar(max),
	@Campaigns varchar(max),
	@PageID Int,
    @SelectionID Int = 0
)
AS

/* 
   Overview: 
     .5 #Selected contains the Statement_ID for the selected donors (this has to work for co-givers)
	 1. #Donations is all deposited donations, filter here for fromdate and todate
	 2. #D is the list of all the donors from #Donations, filter here for congregation (can't filter by selection here in case they selected a co-giver)
	 3. #Donors is #D with duplicates removed, and filtered here for selected donors
	 4. #Pledges is the basic info for all pledges, filter here by Campaigns 
	 5. #PledgeTotals will contain the sum of all contributions for pledges in #Pledges
	 6. #PledgeDataFilter is #PledgeTotals filtered to only include data for donors in #Donors 
	 7. #Gifts prettifies the data
	 8  And then we select from #Gifts union #PledgeTotals 
   
   Modifications:
	6/5 KD: Added page id and selection id to run for specific donors. Unfortunately we can't do this on donations - it has to be on donors
	6/6 KD: Remove all the soft credit donor stuff
	6/21 NT: Added sorting by postcode
	6/28 NT: Taking out the issue with a combination of family and individual statements in one household with pledges
	8/5 CB: Fix issues DE4021, DE4027, DE4028, and DE4042
	8/18 CB: Fix inconsistencies with Mail_Name (US9567)
*/

--For Testing 
/*
Declare @UserID varchar(40) = '00000000-0000-0000-0000-000000000000';
Declare @FromDate DateTime = '20170101'
Declare	@ToDate DateTime = '20170630'
Declare	@AccountingCompanyID Int = 1 --Crossroads
Declare @Campaigns varchar(max) = '1103,60' --Oakley
Declare @Congregations varchar(max) = '1' --Oakley
Declare @PageID INT = 299 --Donors
DECLARE @SelectionID INT = 0
*/
BEGIN
	DECLARE @DomainID INT = 1

	SET NOCOUNT ON
	SET FMTONLY OFF

	-- Normalize dates to remove time and ensure end date is inclusive
	SET @FromDate = CONVERT(DATE, @FromDate)
	SET @ToDate = CONVERT(DATE, @ToDate + 1)

	CREATE TABLE #Congregations (Congregation_ID INT)
	INSERT INTO #Congregations SELECT item FROM dp_Split(@Congregations, ',')

	CREATE TABLE #Campaigns (Campaign_ID INT)
	INSERT INTO #Campaigns SELECT item FROM dp_Split(@Campaigns, ',')

	CREATE TABLE #Selections (Statement_ID VARCHAR(15))
	INSERT INTO #Selections
	        SELECT CASE WHEN C.Household_ID IS NULL OR D.Statement_Type_ID = 1 THEN 'C' + CONVERT(VARCHAR(10),C.Contact_ID) ELSE 'F' + CONVERT(VARCHAR(10),C.Household_ID) END
			FROM dp_Selected_Records SR 
			INNER JOIN dp_Selections S ON S.Selection_ID = SR.Selection_ID  
			INNER JOIN dbo.dp_Users U ON U.[User_ID] = S.[User_ID] AND U.User_GUID = @UserID 
			INNER JOIN Donors D ON SR.record_id = D.Donor_ID
			INNER JOIN Contacts C ON D.Contact_ID = C.Contact_ID
			WHERE S.Page_ID = @PageID 
				AND (    (S.Selection_ID = @SelectionID AND @SelectionID > 0) 
						OR (S.Selection_Name = 'dp_DEFAULT' AND @SelectionID < 1 AND S.Sub_Page_ID IS NULL)
					)
					
	--creating #Donations to use indexing
	--#Donations holds all deposited donations within the specified time range
	CREATE TABLE #Donations (
		Donation_ID INT
		, Donor_ID INT 
		, Donation_Amount Money
		, Donation_Date Datetime
		, Payment_Type_ID INT
		, Item_Number VARCHAR(15)		
		, Is_Recurring_Gift BIT
		, Notes NVARCHAR(500));

	INSERT INTO #Donations (
		Donation_ID 
		, Donor_ID 
		, Donation_Amount 
		, Donation_Date
		, Payment_Type_ID
		, Item_Number		
		, Is_Recurring_Gift
		, Notes )
	SELECT 
		Donation_ID
		, Donor_ID
		, Donation_Amount
		, Donation_Date
		, Payment_Type_ID
		, Item_Number
		, Is_Recurring_Gift
		, Notes
	FROM
		Donations
	WHERE 
		(Donation_Date >= @FromDate AND Donation_Date < @ToDate)
		AND Donation_Status_ID = 2;  --deposited

	CREATE INDEX IX_Donations_DonationID ON #Donations(Donation_ID); --#Donation table Index

	--#D is going to hold a list of donors. The statement id will be the same for donors within families and diff for individuals. 
	CREATE TABLE #D (
		Donor_ID INT
		, Contact_ID INT
		, Statement_ID VARCHAR(15)
		, Contact_Or_Household_ID INT
		, Statement_Type_ID INT
		, Statement_Method_ID INT
		, Statement_Frequency_ID INT
	);

	INSERT INTO #D (
		Donor_ID
		, Contact_ID
		, Statement_ID
		, Contact_Or_Household_ID
		, Statement_Type_ID 
		, Statement_Method_ID
		, Statement_Frequency_ID
	)
	SELECT   -- Finds Donor with atleast 1 Donation made between from date and To date
		Do.Donor_ID
		, Do.Contact_ID
		, Statement_ID = CASE WHEN C.Household_ID IS NULL OR Do.Statement_Type_ID = 1 THEN 'C' + CONVERT(VARCHAR(10),C.Contact_ID) ELSE 'F' + CONVERT(VARCHAR(10),C.Household_ID) END
		, Contact_Or_Household_ID = CASE WHEN C.Household_ID IS NULL OR Do.Statement_Type_ID = 1 THEN C.Contact_ID ELSE C.Household_ID END
		, Do.Statement_Type_ID 
		, Do.Statement_Method_ID
		, Do.Statement_Frequency_ID
	FROM
		Donors Do
		INNER JOIN Contacts C ON C.Contact_ID = Do.Contact_ID
		LEFT JOIN Households H ON C.Household_ID = H.Household_ID
		INNER JOIN #Congregations cong ON COALESCE(H.Congregation_ID, 5) = cong.Congregation_ID   -- 5 = Not site specific
	WHERE --Using EXISTS instead of JOIN because we only want one row per donor and #Donations will have one row for each donation
		EXISTS ( 
			SELECT 1
			FROM #Donations D
			WHERE D.Donor_ID = Do.Donor_ID 
		)



    /*  UNION  US8143 No more soft credit

	SELECT -- Finds soft credit donors with at least 1 soft credit donation between from date and to date
		DD.Soft_Credit_Donor
		, Do.Contact_ID		
		, Statement_ID = CASE WHEN C.Household_ID IS NULL OR Do.Statement_Type_ID = 1 THEN 'C' + CONVERT(VARCHAR(10),C.Contact_ID) ELSE 'F' + CONVERT(VARCHAR(10),C.Household_ID) END
		, Contact_Or_Household_ID = CASE WHEN C.Household_ID IS NULL OR Do.Statement_Type_ID = 1 THEN C.Contact_ID ELSE C.Household_ID END	
		, Do.Statement_Type_ID 
		, Do.Statement_Method_ID
		, Do.Statement_Frequency_ID
	FROM
		Donation_Distributions DD
		INNER JOIN Donors Do ON Do.Donor_ID = DD.Soft_Credit_Donor
		INNER JOIN Contacts C ON C.Contact_ID = Do.Contact_ID
		INNER JOIN Households H ON C.Household_ID = H.Household_ID
		INNER JOIN #Congregations cong ON H.Congregation_ID = cong.Congregation_ID
	WHERE
		 EXISTS ( --Using EXISTS instead of JOIN because we only want one row per donor and #Donations will have one row for each donation
			SELECT 1
			FROM #Donations D
			JOIN Donation_Distributions DD ON DD.Donation_ID = D.Donation_ID
			WHERE DD.Soft_Credit_Donor IS NOT NULL AND DD.Soft_Credit_Donor = Do.Donor_ID 
		);
	*/

	CREATE INDEX IX_D_DonorID ON #D(Donor_ID);

	--#DONORS is the same as #D but removing duplicates (if you donated regular and soft credit) and removing donors who don't get a statement
	CREATE TABLE #DONORS (
		Donor_ID INT,
		Contact_ID INT,
		Statement_ID VARCHAR(15),
		Statement_Type_ID INT,
		Contact_Or_Household_ID INT,
		Mail_Name VARCHAR(1000),
		Address_Line_1 VARCHAR(1000),
		Apt_or_Suite  VARCHAR(1000),
		City VARCHAR(50),
		[State] VARCHAR(50),
		Postal_Code VARCHAR(15)
	);

	INSERT INTO #DONORS
	SELECT DISTINCT
		#D.Donor_ID
		, #D.Contact_ID
		, #D.Statement_ID
		, #D.Statement_Type_ID
		, #D.Contact_Or_Household_ID
		, Mail_Name = CASE
			WHEN C.Company = 1 THEN C.Display_Name
			WHEN #D.Statement_Type_ID = 1 THEN C.First_Name + SPACE(1) + C.Last_Name
			WHEN #D.Statement_Type_ID = 2 AND Count_HoH.Total = 2 AND Count_LastName.Total = 1 THEN
				-- exactly 2 HoH with same last name (e.g., husband and wife): combine last names
				CONCAT(
					STUFF(
						REPLACE(
							(SELECT
								'#!' + LTRIM(RTRIM(S.First_Name)) AS 'data()'
							FROM
								Contacts S
							WHERE
								#D.Statement_Type_ID = 2
								AND S.Household_ID = C.Household_ID
								AND S.Household_Position_ID = 1
							ORDER BY
								S.Gender_ID,
								S.Contact_ID
							FOR XML PATH(''), TYPE).value('(./text())[1]', 'nvarchar(max)'),
							' #!',
							' & '
						),
						1, 2, ''
					),
					' ',
					(SELECT TOP 1
						Last_Name
					FROM
						Contacts S
					WHERE
						#D.Statement_Type_ID = 2			-- Family statement only
						AND S.Household_ID = C.Household_ID
						AND S.Household_Position_ID = 1
					)
				)
			WHEN #D.Statement_Type_ID = 2 AND Count_HoH.Total > 0 THEN
				-- 1, 2 with different last names, or 3+ heads of household: show full names for all
				STUFF(
					REPLACE(
						(SELECT '#!' + LTRIM(RTRIM(S.First_Name + ' ' + S.Last_Name)) AS 'data()'
						FROM
							Contacts S
						WHERE
							#D.Statement_Type_ID = 2		-- Family statement only
							AND S.Household_ID = C.Household_ID
							AND S.Household_Position_ID = 1
						ORDER BY
							S.Gender_ID,
							S.Contact_ID
						FOR XML PATH(''), TYPE).value('(./text())[1]', 'nvarchar(max)'),
						' #!',
						' & '
					),
					1, 2, ''
				)
			ELSE C.First_Name + SPACE(1) + C.Last_Name	-- fallback when all else fails
			END
		, A.Address_Line_1
		, A.Address_Line_2 Apt_or_Suite
		, A.City
		, A.[State/Region] AS [State]
		, A.Postal_Code
	FROM #D 
		INNER JOIN Contacts C ON C.Contact_ID = #D.Contact_ID
		OUTER APPLY (
			-- total head of households (may or may not include current row)
			SELECT
				COUNT(*) AS Total
			FROM
				Contacts S
			WHERE
				#D.Statement_Type_ID = 2		-- Family statement only
				AND S.Household_ID = C.Household_ID
				AND S.Household_Position_ID = 1
		) AS Count_HoH
		OUTER APPLY (
			-- total number of unique last names for all head of households (e.g., 1 means they all share the same last name)
			SELECT
				COUNT(DISTINCT S.Last_Name) AS Total
			FROM
				Contacts S
			WHERE
				#D.Statement_Type_ID = 2		-- Family statement only
				AND S.Household_ID = C.Household_ID
				AND S.Household_Position_ID = 1
		 ) AS Count_LastName
		LEFT OUTER JOIN Households H ON H.Household_ID = C.Household_ID
		LEFT OUTER JOIN Addresses A ON A.Address_ID = H.Address_ID
	WHERE 
		 Statement_Method_ID <> 4		-- No Statement Needed
		 AND (@SelectionID > 0 OR  Statement_Frequency_ID <> 3)		-- Include statement frequency of never only if selection report
		 AND (@SelectionID = 0 
		      OR EXISTS ( 
		       SELECT 1
			   FROM #Selections S
			   WHERE S.Statement_ID = #D.Statement_ID 
		       )
			)
	ORDER BY Donor_ID;

	CREATE INDEX IX_DONORS_DonorID ON #DONORS(Donor_ID);

	--Handle Campaign Pledges 
	--#PledgeData will hold all information about pledges for any statement_id in the list
	CREATE TABLE #PledgeData (
		Statement_ID VARCHAR(15),
		Postal_Code NVARCHAR(15),
		Pledge_ID INT,
		Total_Pledge money,
		Sum_Donations_For_Pledge money,
		Campaign_Name varchar (50)
	)
	
	--If they selected none in the list we aren't going to give them any even if they selected other ones
	IF NOT EXISTS (Select 1 from #Campaigns where campaign_id = 0)
	BEGIN
		WITH Statement_Pledges (Pledge_ID, Total_Pledge, Campaign_Name, Statement_ID, Postal_Code)
		AS (
			SELECT
				p.Pledge_ID,
				p.Total_Pledge,
				pc.Campaign_Name,
				Statement_ID =
					--get the statement_id for the pledge the same way as we got it for the #Gifts
					CASE
						WHEN c.Household_ID IS NULL OR do.Statement_Type_ID = 1 THEN 'C' + CONVERT(VARCHAR(10), c.Contact_ID)
						ELSE 'F' + CONVERT(VARCHAR(10), c.Household_ID)
					END,
				a.Postal_Code
			FROM
				Pledges p
				INNER JOIN Pledge_Campaigns pc on p.Pledge_Campaign_ID = pc.Pledge_Campaign_ID
				INNER JOIN Donors do ON Do.Donor_ID = p.Donor_ID
				INNER JOIN Contacts c ON c.Contact_ID = do.Contact_ID
				LEFT JOIN Households h ON h.Household_ID = c.Household_ID
				LEFT JOIN Addresses a ON a.Address_ID = h.Address_ID 
			WHERE
				p.Pledge_Campaign_ID IN (SELECT Campaign_ID FROM #Campaigns) 
				AND p.Pledge_Status_ID IN (1, 2) --only report on active or completed pledges
		)

		INSERT INTO #PledgeData
			(Statement_ID, Postal_Code, Pledge_ID, Total_Pledge, Sum_Donations_For_Pledge, Campaign_Name)
		SELECT
			p.Statement_ID,
			p.Postal_Code,
			p.Pledge_ID,
			p.Total_Pledge,
			Sum_Donations_For_Pledge = SUM (dd.amount),
			p.Campaign_Name
		FROM
			Statement_Pledges p
			LEFT JOIN Donation_Distributions dd ON dd.Pledge_ID = p.Pledge_ID
			LEFT JOIN Donations d ON dd.Donation_ID = d.Donation_ID
		WHERE
			(d.Donation_Status_ID = 2 OR d.Donation_Status_ID IS NULL) --deposited only or there are no donations
			AND EXISTS (SELECT 1 FROM #DONORS WHERE Statement_ID = p.Statement_ID)
		GROUP BY
			p.Statement_ID,
			p.Postal_Code,
			p.Pledge_ID,
			p.Total_Pledge,
			p.Campaign_Name
		;
	END 

	--#Gifts will contain all of the donations for the time period with the information setup for the statements
	CREATE TABLE #Gifts(
	    Donor_ID INT
		, Donation_ID INT
		, Donation_Distribution_ID INT
		, Donation_Date DATETIME
		, Amount MONEY
		, Non_Deductible_Amount MONEY
		, Deductible_Amount MONEY
		, Payment_Type VARCHAR(50)
		, Payment_Type_ID INT	
		, Item_Number VARCHAR(15)
		, Fund_Name VARCHAR(50)		
		, Is_Soft_Credit_Donor BIT
		, Donation_Detail VARCHAR(1000)
		, Mail_Name VARCHAR(1000)
		, Address_Line_1 VARCHAR(1000)
		, Apt_or_Suite  VARCHAR(1000)
		, City VARCHAR(50)
		, [State] VARCHAR(50)
		, Postal_Code VARCHAR(15)
		, Statement_ID VARCHAR(15)
		, Is_Recurring_Gift BIT
		, Section_Sort INT
	);

	INSERT INTO #Gifts
		(Donation_ID
		, Donor_ID
		, Donation_Distribution_ID
		, Donation_Date
		, Amount
		, Non_Deductible_Amount
		, Deductible_Amount
		, Payment_Type
		, Payment_Type_ID
		, Item_Number
		, Fund_Name			
		, Is_Soft_Credit_Donor 
		, Donation_Detail 
		, Mail_Name
		, Address_Line_1
		, Apt_or_Suite 
		, City
		, [State]
		, Postal_Code
		, Statement_ID
		, Is_Recurring_Gift
		, Section_Sort
	)
		-- Tax deductible only (pull from Donation Distributions)
		SELECT Do.Donor_ID
			, D.Donation_ID
			, DD.Donation_Distribution_ID
			, D.Donation_Date
			, Amount = DD.Amount
			, Non_Deductible_Amount = 0
			, Deductible_Amount = DD.Amount 
			, PT.Payment_Type
			, D.Payment_Type_ID
			, D.Item_Number
			, Prog.Statement_Title AS Fund_Name	
			, Is_Soft_Credit_Donor = 0 --CASE WHEN DD.Soft_Credit_Donor IS NULL THEN 0 ELSE 1 END	
			, Donation_Detail = ISNULL(D.Notes, 'No Description Given')
			, Do.Mail_Name
			, Do.Address_Line_1
			, Do.Apt_or_Suite
			, Do.City
			, Do.[State]
			, Do.Postal_Code
			, Do.Statement_ID --The RDL will group by this
			, D.Is_Recurring_Gift
			, Section_Sort = 1 --Tax Deductible
		FROM #Donations D
			INNER JOIN Donation_Distributions DD ON DD.Donation_ID = D.Donation_ID
			INNER JOIN #DONORS Do ON Do.Donor_ID = D.Donor_ID --CASE WHEN DD.Soft_Credit_Donor IS NULL THEN D.Donor_ID ELSE DD.Soft_Credit_Donor END  --use Donation distribution soft credit donor id as donor id for SCDonations else use donor id in donations as donor id 
			INNER JOIN Programs Prog ON Prog.Program_ID = DD.Program_ID
			INNER JOIN Congregations Cong ON Cong.Congregation_ID = Prog.Congregation_ID AND Cong.Accounting_Company_ID = ISNULL(@AccountingCompanyID, Cong.Accounting_Company_ID)
			INNER JOIN Payment_Types PT ON PT.Payment_Type_ID = D.Payment_Type_ID
		WHERE
			D.Payment_Type_ID <> 6 AND		-- Omit Non-Cash/Asset (e.g., stock)
			ISNULL(Prog.Tax_Deductible_Donations, 0) = 1 --Omit Non Deductible
	UNION ALL
		-- Non-tax deductible only (pull from Donations instead of Donation Distributions so stock donations that have been split into multiple distributions are listed only once)
		SELECT Do.Donor_ID
			, D.Donation_ID
			, 0
			, D.Donation_Date
			, Amount = 0   -- amount is not used for stock giving
			, Non_Deductible_Amount = 0   -- amount is not used for stock giving
			, Deductible_Amount = 0
			, PT.Payment_Type
			, D.Payment_Type_ID
			, D.Item_Number
			, Fund_Name = null -- program name is not used for stock giving
			, Is_Soft_Credit_Donor = 0 --CASE WHEN DD.Soft_Credit_Donor IS NULL THEN 0 ELSE 1 END	
			, Donation_Detail = ISNULL(D.Notes, 'No Description Given')
			, Do.Mail_Name
			, Do.Address_Line_1
			, Do.Apt_or_Suite
			, Do.City
			, Do.[State]
			, Do.Postal_Code
			, Do.Statement_ID --The RDL will group by this
			, D.Is_Recurring_Gift
			, Section_Sort = 2 --Stock Giving
		FROM #Donations D
			INNER JOIN #DONORS Do ON Do.Donor_ID = D.Donor_ID --CASE WHEN DD.Soft_Credit_Donor IS NULL THEN D.Donor_ID ELSE DD.Soft_Credit_Donor END  --use Donation distribution soft credit donor id as donor id for SCDonations else use donor id in donations as donor id 
			INNER JOIN Payment_Types PT ON PT.Payment_Type_ID = D.Payment_Type_ID
		WHERE 
			D.Payment_Type_ID = 6 AND		-- Omit everything except Non-Cash/Asset (e.g., stock)
			EXISTS (
				SELECT
					1
				FROM
					Donation_Distributions dd
					INNER JOIN Programs Prog ON Prog.Program_ID = dd.Program_ID
					INNER JOIN Congregations Cong ON Cong.Congregation_ID = Prog.Congregation_ID AND Cong.Accounting_Company_ID = ISNULL(@AccountingCompanyID, Cong.Accounting_Company_ID)
				WHERE
					dd.Donation_ID = d.Donation_ID AND
					ISNULL(Prog.Tax_Deductible_Donations, 0) = 1
			)
	;

	CREATE INDEX IX_Gifts_DonorID ON #Gifts(Donor_ID)

	

	SELECT
		Statement_ID
		, Donation_ID
		, Donation_Date
		, Amount
		, Non_Deductible_Amount
		, Deductible_Amount
		, Payment_Type = CASE
							WHEN Is_Soft_Credit_Donor = 1 THEN Donation_Detail
							WHEN Payment_Type_ID = 0 THEN Donation_Detail
							WHEN Payment_Type_ID = 1 THEN CONCAT('Check #', Item_Number)
							WHEN Payment_Type_ID = 2 THEN 'Cash'
							WHEN Payment_Type_ID = 4 AND Is_Recurring_Gift = 1 THEN 'Recurring Credit Card'
							WHEN Payment_Type_ID = 4 THEN 'One-time Credit Card'
							WHEN Payment_Type_ID = 5 AND Is_Recurring_Gift = 1 THEN 'Recurring ACH'
							WHEN Payment_Type_ID = 5 THEN 'One-time ACH'
							WHEN Payment_Type_ID = 6 THEN ISNULL(Donation_Detail, 'Non-cash')
							ELSE ISNULL(Item_Number, Payment_Type)
						END
		, Fund_Name AS [Donation_Description]
		, Mail_Name
		, Address_Line_1
		, Apt_or_Suite
		, City
		, [State]
		, Postal_Code
		, Section_Sort
		, NULL AS Total_Pledge
 		, NULL AS Given_to_Pledge
 		, NULL AS Campaign_Name
	FROM #Gifts 
		UNION ALL
 	--The campaign pledge data
 	SELECT 
 	      Statement_ID
 		, NULL
 		, NULL
 		, NULL
 		, NULL
 		, NULL
 		, NULL
 		, NULL
 		, NULL
 		, NULL
 		, NULL
 		, NULL
 		, NULL
 		, Postal_Code
 		, 4 --Campaign Contributions
 		, Total_Pledge		
 		, ISNULL(Sum_Donations_For_Pledge, 0) AS Given_to_Pledge
 		, Campaign_Name
	FROM #PledgeData
	--Sorting by postal code US7181
	ORDER BY postal_code, statement_id, section_sort
	
	
	DROP TABLE #Donations;
	DROP TABLE #D;
	DROP TABLE #DONORS;
	DROP TABLE #Gifts;
	DROP TABLE #Congregations;

	DROP TABLE #Campaigns;
	DROP TABLE #PledgeData; 
	DROP TABLE #Selections;

END

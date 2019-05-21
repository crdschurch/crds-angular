USE [MinistryPlatform]
GO

/****** Object:  StoredProcedure [dbo].[report_CRDS_Donor_Contribution_Statement]    Script Date: 6/6/2017 3:23:59 PM ******/
SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO




-- Created 4/6/2017 for Paper Statements

ALTER PROCEDURE [dbo].[report_CRDS_Donor_Contribution_Statement] (
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
*/

--For Testing 
/*
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
		INNER JOIN Households H ON C.Household_ID = H.Household_ID
		INNER JOIN #Congregations cong ON H.Congregation_ID = cong.Congregation_ID
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
	CREATE TABLE #DONORS (Donor_ID INT, Contact_ID INT, Statement_ID VARCHAR(15), Statement_Type_ID INT, Contact_Or_Household_ID INT);

	INSERT INTO #DONORS SELECT DISTINCT Donor_ID, Contact_ID, Statement_ID, Statement_Type_ID, Contact_Or_Household_ID 
	FROM #D 
	WHERE 
		 Statement_Method_ID <> 4		-- No Statement Needed
		 AND Statement_Frequency_ID <> 3		-- Never
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
	--getting all of them so we can handle spouse, children, etc
	--#Pledges will hold all information about pledges all campaigns in the list
	CREATE TABLE #Pledges (Donor_ID INT, Pledge_ID INT, Pledge_Campaign_ID INT, Total_Pledge money, Campaign_Name varchar (50) )
	INSERT INTO #Pledges
	SELECT Donor_ID, p.Pledge_ID, p.Pledge_Campaign_id, p.Total_Pledge, c.Campaign_Name
	FROM Pledges p
	INNER JOIN Pledge_Campaigns c on p.Pledge_Campaign_ID = c.Pledge_Campaign_ID
	WHERE p.Pledge_Campaign_ID in (SELECT Campaign_ID FROM #Campaigns) 
	AND p.Pledge_Status_ID in (1,2) --only report on active or completed pledges

	--#PledgeTotals holds all pledge totals for the campaign
	CREATE TABLE #PledgeTotals (Pledge_ID INT, Pledge_Owner INT, Total_Pledge money, Sum_Donations_ForPledge money,Campaign_Name varchar (50))	  
	--#PledgeData will hold all information about pledges for any statement_id in the list
	CREATE TABLE #PledgeData (Pledge_ID INT, Total_Pledge money, Sum_Donations_ForPledge money, Statement_ID VARCHAR(15),Campaign_Name varchar (50))
	
	
	--If they selected none in the list we aren't going to give them any even if they selected other ones
	IF NOT EXISTS (Select 1 from #Campaigns where campaign_id = 0)
	BEGIN

	  --We can't join on donors yet because we want all donations for the pledge regardless of the donor
	
	  INSERT INTO #PledgeTotals 
	  SELECT p.Pledge_ID, p.Donor_ID, p.Total_Pledge, SUM (dd.amount),p.campaign_name
	  FROM #Pledges p
	  LEFT JOIN Donation_Distributions dd ON dd.Pledge_ID =p.Pledge_ID
	  LEFT JOIN Donations d ON dd.Donation_ID = d.Donation_ID
	  WHERE Pledge_Campaign_ID in (SELECT Campaign_ID from #Campaigns)
	  AND (d.Donation_Status_ID = 2 OR d.Donation_Status_ID IS NULL) --deposited only or there are no donations
	  GROUP BY p.Pledge_ID, p.Donor_ID, p.Total_Pledge, p.campaign_name
	  
	  	--select * from #PledgeTotals where pledge_owner = 7557276

	  --get the statement_id for the pledge the same way as we got it for the #Gifts and sum the donations for the pledge
	  --using distinct instead of exists because it's waaaayyy faster 21 seconds vs > 3 minutes when I killed it
	  
	  INSERT INTO #PledgeData
	  SELECT distinct
	    pt.Pledge_ID,
		pt.Total_Pledge, 
		pt.Sum_Donations_ForPledge, 
		Statement_ID = CASE WHEN C.Household_ID IS NULL OR d.Statement_Type_ID = 1 THEN 'C' + CONVERT(VARCHAR(10),C.Contact_ID) ELSE 'F' + CONVERT(VARCHAR(10),C.Household_ID) END,
		pt.Campaign_Name
      FROM #PledgeTotals pt
      INNER JOIN Donors d ON d.Donor_ID = pt.Pledge_Owner
	  INNER JOIN Contacts c ON d.Contact_ID = c.Contact_ID
	  INNER JOIN #Donors fd ON fd.Contact_Or_Household_ID = 
		     CASE WHEN fd.Statement_Type_ID = 1 THEN c.Contact_ID
	              WHEN fd.Statement_Type_ID = 2 THEN c.Household_ID
			      ELSE 'x' --leave them off
			      END
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
		, Section_Title VARCHAR(1000)
		, Header_Title NVARCHAR(1000)
		, Section_Sort INT
		, Row_No INT
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
		, Section_Title
		, Header_Title
		, Section_Sort
	)
	SELECT Do.Donor_ID
	    , D.Donation_ID
		, DD.Donation_Distribution_ID
		, D.Donation_Date
		, Amount = DD.Amount
		, Non_Deductible_Amount = CASE WHEN D.Payment_Type_ID = 6 OR Prog.Tax_Deductible_Donations = 0 THEN DD.Amount ELSE 0 END
		, Deductible_Amount = CASE WHEN D.Payment_Type_ID <> 6 AND Prog.Tax_Deductible_Donations = 1 THEN DD.Amount ELSE 0 END 
		, PT.Payment_Type
		, D.Payment_Type_ID
		, D.Item_Number
		, Prog.Statement_Title AS Fund_Name	
		, Is_Soft_Credit_Donor = 0 --CASE WHEN DD.Soft_Credit_Donor IS NULL THEN 0 ELSE 1 END	
		, Donation_Detail =  ISNULL(D.Notes, 'No Descriptions Given')
		/*, Donation_Detail = CASE WHEN DD.Soft_Credit_Donor IS NOT NULL THEN (select display_name from contacts where contact_id = (select contact_id from donors where donor_id = (select donor_id from donations where donation_id = d.donation_id)))
		                      ELSE ISNULL(D.Notes, 'No Descriptions Given') END  -- Display trust/foundation name if Soft credit donation or Donation notes for Stock
		*/
		, Mail_Name = CASE WHEN C.Company = 1 THEN C.Display_Name
						WHEN CNT.num < 2 AND C.Household_Position_id = 2  THEN SP.First_Name + Space(1) + SP.Last_Name
						WHEN SP.Last_Name <> C.Last_Name AND CNT.num < 2 THEN C.First_Name + SPACE(1) + C.Last_Name + ' & ' + SP.First_Name + SPACE(1) + Sp.Last_Name
						WHEN SP.Gender_ID < C.Gender_ID AND CNT.num < 2 THEN ISNULL(SP.First_Name + ' & ','') + C.First_Name + Space(1) + C.Last_Name
						WHEN CNT.num >= 2 AND C.Household_Position_ID = 1 THEN STUFF(REPLACE((SELECT '#!' + LTRIM(RTRIM(co.first_name + ' '+ co.last_name)) AS 'data()' FROM contacts co WHERE Do.Statement_Type_ID = 2 AND C.Household_ID = co.Household_ID AND co.Household_Position_ID =1 FOR XML PATH('')),' #!',' & '), 1, 2, '')
	    				WHEN CNT.num = 2 AND CountLast.sameLast = 2 AND C.Household_Position_id = 2 THEN CONCAT((STUFF(REPLACE((SELECT '#!' + LTRIM(RTRIM(co.first_name)) AS 'data()' FROM contacts co WHERE Do.Statement_Type_ID = 2 AND C.Household_ID = co.Household_ID AND co.Household_Position_ID =1 FOR XML PATH('')),' #!',' & '), 1, 2, '')),' ',(SELECT TOP 1 Last_Name FROM Contacts con WHERE Do.Statement_Type_ID = 2 AND C.Household_ID = con.Household_ID AND con.Household_Position_ID =1))
						ELSE ISNULL(C.First_Name + ISNULL(' & ' + SP.First_Name,'') + SPACE(1) + C.Last_Name, C.Display_Name) END
		, A.Address_Line_1
		, A.Address_Line_2 Apt_or_Suite
		, A.City
		, A.[State/Region] AS [State]
		, A.Postal_Code
		, Do.Statement_ID --The RDL will group by this
		, D.Is_Recurring_Gift
		--Payment_Type_ID = 6 => Non-Cash/Asset
		, Section_Title = CAST(CASE WHEN D.Payment_Type_ID <> 6 AND Prog.Tax_Deductible_Donations = 1 AND DD.Soft_Credit_Donor IS NULL THEN 'Tax-Deductible Information:' 
		                            WHEN D.Payment_Type_ID = 6 THEN 'Stock Giving:' 
							        ELSE '' END AS VARCHAR(100))  
		, Header_Title = CAST(CASE WHEN D.Payment_Type_ID <> 6 AND Prog.Tax_Deductible_Donations = 1 AND DD.Soft_Credit_Donor IS NULL THEN ''
		                            WHEN D.Payment_Type_ID = 6 THEN 'Please contact your accountant to determine your stock donation value and add to your tax deductible total.' 
							        ELSE '' END AS VARCHAR(300))  
		, Section_Sort = CASE WHEN D.Payment_Type_ID <> 6 AND Prog.Tax_Deductible_Donations = 1 AND DD.Soft_Credit_Donor IS NULL THEN 1 --Tax-Deductible Information
							  WHEN D.Payment_Type_ID = 6 THEN 2 --Stock Giving
							  ELSE 3 END --Trust and Foundation Giving (should no longer be there)

		FROM #Donations D
			 INNER JOIN Donation_Distributions DD ON DD.Donation_ID = D.Donation_ID
			 INNER JOIN #DONORS Do ON Do.Donor_ID = D.Donor_ID --CASE WHEN DD.Soft_Credit_Donor IS NULL THEN D.Donor_ID ELSE DD.Soft_Credit_Donor END  --use Donation distribution soft credit donor id as donor id for SCDonations else use donor id in donations as donor id 
			 INNER JOIN Contacts C ON C.Contact_ID = Do.Contact_ID
			 OUTER APPLY (SELECT Count(*) as num FROM Contacts S WHERE Do.Statement_Type_ID = 2 AND S.Household_ID = C.Household_ID AND C.Contact_ID <> S.Contact_ID AND S.Household_Position_ID = 1) CNT -- head of house holds count
		     OUTER APPLY (SELECT Top 1 Contact_ID, Household_ID, Last_Name, Nickname, First_Name, Gender_ID FROM Contacts S WHERE Do.Statement_Type_ID = 2 AND S.Household_ID = C.Household_ID AND C.Contact_ID <> S.Contact_ID AND S.Household_Position_ID = 1) SP --Select top 1 other HoH that's not donor
		     OUTER APPLY (SELECT Count(Last_Name) as sameLast From Contacts CTS WHERE CTS.Last_Name in (SELECT TS.Last_Name from Contacts TS group by TS.Last_Name having count(TS.Last_Name ) > 1) AND CTS.Household_ID = C.Household_ID AND  CTS.household_position_id = 1) CountLast -- head of households with same last name count
		     LEFT OUTER JOIN Households H ON H.Household_ID = C.Household_ID
			 LEFT OUTER JOIN Addresses A ON A.Address_ID = H.Address_ID
			 INNER JOIN Programs Prog ON Prog.Program_ID = DD.Program_ID
			 INNER JOIN Congregations Cong ON Cong.Congregation_ID = Prog.Congregation_ID AND Cong.Accounting_Company_ID = ISNULL(@AccountingCompanyID, Cong.Accounting_Company_ID)
			 INNER JOIN Statement_Headers SH ON SH.Statement_Header_ID = Prog.Statement_Header_ID
			 INNER JOIN Payment_Types PT ON PT.Payment_Type_ID = D.Payment_Type_ID
		WHERE 
			 ISNULL(Prog.Tax_Deductible_Donations,0) = 1 --Omit Non Deductible
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
		, Section_Title
		, Header_Title
		, Section_Sort
		, Row_No
		, NULL AS Total_Pledge
 		, NULL AS Given_to_Pledge
 		, NULL AS Campaign_Name
	FROM #Gifts 
		UNION 
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
 		, NULL
 		, NULL
 		, NULL
 		, 4 --Camapaign Contributions
 		, NULL
 		, Total_Pledge		
 		, ISNULL(Sum_Donations_ForPledge,0)  AS Given_to_Pledge
 		, Campaign_Name
	FROM #PledgeData
	ORDER BY statement_id, section_sort
	
	
	DROP TABLE #Donations;
	DROP TABLE #D;
	DROP TABLE #DONORS;
	DROP TABLE #Gifts;
	DROP TABLE #Congregations;

	DROP TABLE #Campaigns;
	DROP TABLE #Pledges;
	DROP TABLE #PledgeData; 
	DROP TABLE #PledgeTotals;
	DROP TABLE #Selections;

END






GO



USE [MinistryPlatform]
GO
-- =====================================================================================================
-- Author:      John Cleaver
-- Create date: 2017-04-27
-- Description:	Alter report_CRDS_Checkin_New_Families to add address so new
-- families can get a sweet, sweet mailer
-- Modified:	Shakila Rajaiah
-- Modified Date : 2019-06-10
-- Description : New fields Added: Household positions, First names, 
--               Household email address, Household phone, Tag special needs, display allergies.
--		         Corrected: Count#OfChildren, count#OfHouseholds
-- ========================================================================================================
CREATE OR ALTER PROCEDURE [dbo].[report_CRDS_Checkin_New_Families] 
	@StartDate DATETIME
  , @EndDate DATETIME
  , @EventCongregations NVARCHAR(1000)
AS
BEGIN

IF OBJECT_ID('tempdb..#familycheck') IS NOT NULL   
    DROP TABLE #familycheck

IF OBJECT_ID('tempdb..#TempAll') IS NOT NULL   
    DROP TABLE #TempAll

CREATE TABLE #familycheck
(
    etitle varchar(100),
	edate	datetime,
	hid	int,
	hname varchar(150),
	clname varchar(50),
    cfname varchar(50),
	cid	int,
	--aid int,
    email varchar(500),
    hphone varchar(20),
    hpos	varchar(150),
	minorchild int,
    sroom varchar(50),
	allergies varchar(150),
	address varchar(5000)
 )

 -- add records of families and children who are in the event
 insert into #familycheck
	SELECT 
		T1.[Event_Title], 
		T1.[Event_Start_Date], 
		T1.Household_ID,
		T1.HouseholdName ,
		C.Last_Name, 
		C.First_Name , 
		c.contact_ID ,
		C.Email_Address ,
		C.Mobile_Phone,
		CASE
			WHEN C.Household_Position_ID = 1 THEN 'Head of Household'
			WHEN C.Household_Position_ID = 2 THEN 'Minor Child'
		END ,
	   MAX(CASE
			WHEN T1.Contact_ID = c.Contact_ID -- only count the kids in KC
			THEN 1
			ELSE 0
		END),
		null,
		null,
		T1.Address
	FROM 
		(SELECT 
				E.Event_Title AS "Event_Title"
				,E.Event_Start_Date
				,H.Household_Name AS "HouseholdName"
				, H.Household_ID
				, P.Contact_ID
				, CONVERT(time, MIN(P.Participant_Start_Date)) AS "Time Created"
				,(SELECT CONCAT(
						s_A.Address_Line_1 + ' ',
						s_A.Address_Line_2 + ' ',
						s_A.City + ', ', 
						s_A.[State/Region] + ' ',
						s_A.Postal_Code + ' ',
						s_A.Foreign_Country)
				FROM  
						Addresses s_A 
				WHERE s_A.Address_ID = H.Address_ID) AS "Address"
		FROM
				Events E INNER JOIN Event_Participants EP ON E.Event_ID = EP.Event_ID
				INNER JOIN Participants P ON EP.Participant_ID = P.Participant_ID
				INNER JOIN Contacts C ON C.Contact_ID = P.Contact_ID
				INNER JOIN Households H ON H.Household_ID = C.Household_ID
		WHERE
				CONVERT(date, P.Participant_Start_Date) = CONVERT(DATE, E.Event_Start_Date)
				AND H.Household_Source_ID = 48 -- Kids Club Registration
				AND E.Congregation_ID IN (SELECT * FROM dbo.fnSplitString(@EventCongregations,','))
				AND CONVERT(date, E.Event_Start_Date) >= @StartDate
				AND CONVERT(date, E.Event_Start_Date) <= @EndDate
		GROUP BY
				E.Event_ID
				, E.Event_Title
				, E.Event_Start_Date
				, H.Household_Name
				, H.Household_ID
				, P.Contact_ID
				, H.Address_ID) AS T1
	INNER JOIN Contacts as C on C.Household_ID = T1.Household_ID
	AND  C.Contact_Status_ID = 1 --active
	AND  C.Category_ID > 1 --attendees, businesses
	GROUP BY 
	T1.[Event_Title], 
    T1.[Event_Start_Date], 
	T1.Household_ID,
	T1.HouseholdName,
	C.contact_ID,
    C.Last_Name,
    C.First_Name,
    C.Email_Address,
    C.Mobile_Phone,
    CASE
        WHEN C.Household_Position_ID = 1 THEN 'Head of Household'
        WHEN C.Household_Position_ID = 2 THEN 'Minor Child'
    END,
    T1.Address
	order by T1.HouseholdName

	-- update records if the minor child had special needs (Attribute_ID = 7058)
	UPDATE 
		#familycheck 
	SET 
		#familycheck.sroom = 'Y'
	FROM 
		Contact_Attributes CA 
	WHERE 
		#familycheck.cid = CA.Contact_ID
		AND CA.Attribute_ID=7058 --special needs
		AND #familycheck.hpos = 'Minor Child'

	-- update records if the minor child has food allergies (Attribute_ID = 3971)
	UPDATE 
		#familycheck 
	SET 
		#familycheck.allergies = 'Y'
	FROM 
		Contact_Attributes CA 
	WHERE 
		#familycheck.cid = CA.Contact_ID
		AND CA.Attribute_ID=3971 -- food allergies
		AND #familycheck.hpos = 'Minor Child'

	SELECT 
			etitle AS "Event_Title"
			,edate as Event_Start_Date
			, hid AS Household_ID
			, hname AS "HouseholdName"
			, clname  AS "LastName"
			, cfname  AS "FirstName"
			, cid as Contact_ID
			, email AS "HouseholdEmail"
			, hphone AS "HouseholdPhone"
			, hpos AS "HouseholdPosition"
			, minorchild  AS "MinorChildrenInEventCount"
			, sroom AS "SensoryRoom"
			, allergies AS "Allergies"
			,address AS "Address"
	FROM 
		#familycheck

END
GO



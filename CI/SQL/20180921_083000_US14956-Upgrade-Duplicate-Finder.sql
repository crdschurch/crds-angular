USE [MinistryPlatform]
GO

CREATE  PROCEDURE [dbo].[service_duplicate_finder]

	@DomainID INT

AS
BEGIN

--Internal Parameters
DECLARE @DupeRelationshipID INT, @PossDupeRelationshipID INT, @DefaultContactID INT, @UnassignedContactID INT, @UnassignedDonorID INT
SET @DupeRelationshipID = (SELECT Relationship_ID FROM Relationships WHERE Relationship_Name LIKE 'Duplicate%')
SET @PossDupeRelationshipID = (SELECT Relationship_ID FROM Relationships WHERE Relationship_Name LIKE 'Possible%Duplicate%')
SELECT @DefaultContactID = Value FROM dp_Configuration_Settings WHERE Domain_ID = @DomainID AND Key_Name = 'DefaultContactID' AND Application_Code = 'Common'
SELECT @UnassignedDonorID = Value FROM dp_Configuration_Settings WHERE Domain_ID = @DomainID AND Key_Name = 'UnassignedDonorID' AND Application_Code = 'Common'
SELECT @UnassignedContactID = Contact_ID FROM Donors WHERE Donor_ID = @UnassignedDonorID

--Clean-up scripts
UPDATE Contacts SET Email_Address = NULL WHERE Email_Address = ''
UPDATE Contacts SET Mobile_Phone = NULL WHERE Mobile_Phone = ''
UPDATE Households SET Home_Phone = NULL WHERE Home_Phone = ''

--Create Temp Table
CREATE TABLE #Dupes (Contact_ID INT, Related_Contact_ID INT, Gender_ID INT, Related_Gender_ID INT, Date_of_Birth DateTime, Related_Date_of_Birth DateTime, First_Name Varchar(50), Related_First_Name Varchar(50), Nickname Varchar(50), Related_Nickname Varchar(50), Last_Name Varchar(50), Related_Last_Name Varchar(50), Domain_ID INT, Notes Varchar(255))

--Populate Temp Table
INSERT INTO #Dupes (Contact_ID, Related_Contact_ID, Gender_ID, Related_Gender_ID, Date_of_Birth, Related_Date_of_Birth, First_Name, Related_First_Name, Nickname, Related_Nickname, Last_Name, Related_Last_Name, Domain_ID)

--Email Same
SELECT C.Contact_ID, RC.Contact_ID, C.Gender_ID, RC.Gender_ID AS Related_Gender_ID, C.Date_of_Birth, RC.Date_of_Birth AS Related_Date_of_Birth, C.First_Name, RC.First_Name AS Related_First_Name, C.Nickname, RC.Nickname AS Related_Nickname, C.Last_Name, RC.Last_Name AS Related_Last_Name, C.Domain_ID
FROM Contacts C
 INNER JOIN Contacts RC ON C.Contact_ID > RC.Contact_ID
  AND C.Email_Address = RC.Email_Address 
  AND C.Email_Address <> 'support@thinkministry.com'	
WHERE (C.Suffix_ID = RC.Suffix_ID or (C.Suffix_ID is null and RC.Suffix_ID is null))
 AND C.Domain_ID = RC.Domain_ID	
 AND C.Company = 0
 AND (LEFT(C.Last_Name,4) = LEFT(RC.Last_Name,4) OR RIGHT(C.Last_Name,4) = RIGHT(RC.Last_Name,4))
 AND LEFT(C.Nickname,4) = LEFT(RC.Nickname,4)
	 	 
UNION 
--INSERT INTO #Dupes (Contact_ID, Related_Contact_ID, Gender_ID, Related_Gender_ID, Date_of_Birth, Related_Date_of_Birth, Domain_ID)
--DOB and Last Name and First or Nickname same
SELECT C.Contact_ID, RC.Contact_ID, C.Gender_ID, RC.Gender_ID AS Related_Gender_ID, C.Date_of_Birth, RC.Date_of_Birth AS Related_Date_of_Birth, C.First_Name, RC.First_Name AS Related_First_Name, C.Nickname, RC.Nickname AS Related_Nickname, C.Last_Name, RC.Last_Name AS Related_Last_Name, C.Domain_ID
FROM Contacts C
 INNER JOIN Contacts RC ON C.Contact_ID > RC.Contact_ID
  AND C.Date_of_Birth = RC.Date_of_Birth
WHERE (C.Suffix_ID = RC.Suffix_ID or (C.Suffix_ID is null and RC.Suffix_ID is null))	
 AND C.Domain_ID = RC.Domain_ID	 	 
 AND C.Company = 0
 AND (LEFT(C.Last_Name,4) = LEFT(RC.Last_Name,4) OR RIGHT(C.Last_Name,4) = RIGHT(RC.Last_Name,4))
 AND LEFT(C.Nickname,4) = LEFT(RC.Nickname,4)
 
/* AND (C.Last_Name LIKE RC.Last_Name + '%'
	OR RC.Last_Name LIKE C.Last_Name + '%')
 AND (LEFT(RC.First_Name, 4) = LEFT(C.First_Name,4)
	OR RC.Nickname LIKE C.Nickname+'%' 
	OR RC.First_Name LIKE C.Nickname + '%' 
	OR RC.First_Name LIKE C.First_Name + '%' 
	OR RC.Nickname LIKE C.First_Name + '%')
*/
UNION 
--INSERT INTO #Dupes (Contact_ID, Related_Contact_ID, Gender_ID, Related_Gender_ID, Date_of_Birth, Related_Date_of_Birth, Domain_ID)
--Mobile_Phone and Last_Name and First_Name or Nickname same
SELECT C.Contact_ID, RC.Contact_ID, C.Gender_ID, RC.Gender_ID AS Related_Gender_ID, C.Date_of_Birth, RC.Date_of_Birth AS Related_Date_of_Birth, C.First_Name, RC.First_Name AS Related_First_Name, C.Nickname, RC.Nickname AS Related_Nickname, C.Last_Name, RC.Last_Name AS Related_Last_Name, C.Domain_ID
FROM Contacts C
 INNER JOIN Contacts RC ON C.Contact_ID > RC.Contact_ID
  AND REPLACE(REPLACE(REPLACE(REPLACE(C.Mobile_Phone,' ',''),'-',''),')',''),'(','') = REPLACE(REPLACE(REPLACE(REPLACE(RC.Mobile_Phone,' ',''),'-',''),')',''),'(','')
WHERE (C.Suffix_ID = RC.Suffix_ID or (C.Suffix_ID is null and RC.Suffix_ID is null))
 AND C.Domain_ID = RC.Domain_ID	 	 
 AND C.Company = 0
 AND (LEFT(C.Last_Name,4) = LEFT(RC.Last_Name,4) OR RIGHT(C.Last_Name,4) = RIGHT(RC.Last_Name,4))
 AND LEFT(C.Nickname,4) = LEFT(RC.Nickname,4)

UNION 
--INSERT INTO #Dupes (Contact_ID, Related_Contact_ID, Gender_ID, Related_Gender_ID, Date_of_Birth, Related_Date_of_Birth, Domain_ID)
--Home_Phone and Last_Name and First_Name or Nickname same
SELECT C.Contact_ID, RC.Contact_ID, C.Gender_ID, RC.Gender_ID AS Related_Gender_ID, C.Date_of_Birth, RC.Date_of_Birth AS Related_Date_of_Birth, C.First_Name, RC.First_Name AS Related_First_Name, C.Nickname, RC.Nickname AS Related_Nickname, C.Last_Name, RC.Last_Name AS Related_Last_Name, C.Domain_ID
FROM Contacts C
 INNER JOIN Households H ON C.Household_ID = H.Household_ID
 INNER JOIN Contacts RC ON C.Contact_ID > RC.Contact_ID
 INNER JOIN Households RH ON RH.Household_ID = RC.Household_ID							
  AND REPLACE(REPLACE(REPLACE(REPLACE(H.Home_Phone,' ',''),'-',''),')',''),'(','') = REPLACE(REPLACE(REPLACE(REPLACE(RH.Home_Phone,' ',''),'-',''),')',''),'(','')
WHERE (C.Suffix_ID = RC.Suffix_ID or (C.Suffix_ID is null and RC.Suffix_ID is null))	
 AND C.Domain_ID = RC.Domain_ID	 	 
 AND C.Company = 0
 AND (LEFT(C.Last_Name,4) = LEFT(RC.Last_Name,4) OR RIGHT(C.Last_Name,4) = RIGHT(RC.Last_Name,4))
 AND LEFT(C.Nickname,4) = LEFT(RC.Nickname,4)
 
UNION 
--INSERT INTO #Dupes (Contact_ID, Related_Contact_ID, Gender_ID, Related_Gender_ID, Date_of_Birth, Related_Date_of_Birth, Domain_ID)
--Home_Phone to Mobile_Phone and Last_Name and First_Name or Nickname same
SELECT C.Contact_ID, RC.Contact_ID, C.Gender_ID, RC.Gender_ID AS Related_Gender_ID, C.Date_of_Birth, RC.Date_of_Birth AS Related_Date_of_Birth, C.First_Name, RC.First_Name AS Related_First_Name, C.Nickname, RC.Nickname AS Related_Nickname, C.Last_Name, RC.Last_Name AS Related_Last_Name, C.Domain_ID
FROM Contacts C
 INNER JOIN Households H ON C.Household_ID = H.Household_ID
 INNER JOIN Contacts RC ON C.Contact_ID > RC.Contact_ID
 INNER JOIN Households RH ON RH.Household_ID = RC.Household_ID							
  AND REPLACE(REPLACE(REPLACE(REPLACE(H.Home_Phone,' ',''),'-',''),')',''),'(','') = REPLACE(REPLACE(REPLACE(REPLACE(RC.Mobile_Phone,' ',''),'-',''),')',''),'(','')
WHERE (C.Suffix_ID = RC.Suffix_ID or (C.Suffix_ID is null and RC.Suffix_ID is null))
 AND C.Domain_ID = RC.Domain_ID	 	 
 AND C.Company = 0
 AND (LEFT(C.Last_Name,4) = LEFT(RC.Last_Name,4) OR RIGHT(C.Last_Name,4) = RIGHT(RC.Last_Name,4))
 AND LEFT(C.Nickname,4) = LEFT(RC.Nickname,4)
 
UNION 
--INSERT INTO #Dupes (Contact_ID, Related_Contact_ID, Gender_ID, Related_Gender_ID, Date_of_Birth, Related_Date_of_Birth, Domain_ID)
--Home_Phone to Mobile_Phone and Last_Name and First_Name or Nickname same
SELECT C.Contact_ID, RC.Contact_ID, C.Gender_ID, RC.Gender_ID AS Related_Gender_ID, C.Date_of_Birth, RC.Date_of_Birth AS Related_Date_of_Birth, C.First_Name, RC.First_Name AS Related_First_Name, C.Nickname, RC.Nickname AS Related_Nickname, C.Last_Name, RC.Last_Name AS Related_Last_Name, C.Domain_ID
FROM Contacts C
 INNER JOIN Households H ON C.Household_ID = H.Household_ID
 INNER JOIN Contacts RC ON C.Contact_ID > RC.Contact_ID
 INNER JOIN Households RH ON RH.Household_ID = RC.Household_ID							
 AND REPLACE(REPLACE(REPLACE(REPLACE(RH.Home_Phone,' ',''),'-',''),')',''),'(','') = REPLACE(REPLACE(REPLACE(REPLACE(C.Mobile_Phone,' ',''),'-',''),')',''),'(','')
WHERE (C.Suffix_ID = RC.Suffix_ID or (C.Suffix_ID is null and RC.Suffix_ID is null))	
 AND C.Domain_ID = RC.Domain_ID	 	 
 AND C.Company = 0
 AND (LEFT(C.Last_Name,4) = LEFT(RC.Last_Name,4) OR RIGHT(C.Last_Name,4) = RIGHT(RC.Last_Name,4))
 AND LEFT(C.Nickname,4) = LEFT(RC.Nickname,4)
 
UNION 
--INSERT INTO #Dupes (Contact_ID, Related_Contact_ID, Gender_ID, Related_Gender_ID, Date_of_Birth, Related_Date_of_Birth, Domain_ID)
--Address_Line_1 and Last_Name and First_Name or Nickname same
SELECT C.Contact_ID, RC.Contact_ID, C.Gender_ID, RC.Gender_ID AS Related_Gender_ID, C.Date_of_Birth, RC.Date_of_Birth AS Related_Date_of_Birth, C.First_Name, RC.First_Name AS Related_First_Name, C.Nickname, RC.Nickname AS Related_Nickname, C.Last_Name, RC.Last_Name AS Related_Last_Name, C.Domain_ID
FROM Contacts C
 INNER JOIN Households H ON C.Household_ID = H.Household_ID
 INNER JOIN Addresses A ON A.Address_ID = H.Address_ID
 INNER JOIN Contacts RC ON C.Contact_ID > RC.Contact_ID
 INNER JOIN Households RH ON RH.Household_ID = RC.Household_ID	
 INNER JOIN Addresses RA ON RA.Address_ID = RH.Address_ID 
  AND LEFT(A.Address_Line_1,10) = LEFT(RA.Address_Line_1,10) 
  AND A.City = RA.City 						
WHERE (C.Suffix_ID = RC.Suffix_ID or (C.Suffix_ID is null and RC.Suffix_ID is null))	
 AND C.Domain_ID = RC.Domain_ID	 	 
 AND C.Company = 0
 AND (LEFT(C.Last_Name,4) = LEFT(RC.Last_Name,4) OR RIGHT(C.Last_Name,4) = RIGHT(RC.Last_Name,4))
 AND LEFT(C.Nickname,4) = LEFT(RC.Nickname,4)

UNION 
--INSERT INTO #Dupes (Contact_ID, Related_Contact_ID, Gender_ID, Related_Gender_ID, Date_of_Birth, Related_Date_of_Birth, Domain_ID)
--Donor Account and Last_Name and First_Name or Nickname same
SELECT DISTINCT C.Contact_ID, RC.Contact_ID, C.Gender_ID, RC.Gender_ID AS Related_Gender_ID, C.Date_of_Birth, RC.Date_of_Birth AS Related_Date_of_Birth, C.First_Name, RC.First_Name AS Related_First_Name, C.Nickname, RC.Nickname AS Related_Nickname, C.Last_Name, RC.Last_Name AS Related_Last_Name, C.Domain_ID
FROM Contacts C
 INNER JOIN Donor_Accounts DA ON DA.Donor_ID = C.Donor_Record
 INNER JOIN Contacts RC ON C.Contact_ID > RC.Contact_ID
 INNER JOIN Donor_Accounts RDA ON RDA.Donor_ID = RC.Donor_Record	
  AND DA.Routing_Number = RDA.Routing_Number
  AND DA.Account_Number = RDA.Account_Number 			
WHERE (C.Suffix_ID = RC.Suffix_ID or (C.Suffix_ID is null and RC.Suffix_ID is null))	
 AND C.Domain_ID = RC.Domain_ID	 	 
 AND C.Company = 0
 AND (LEFT(C.Last_Name,4) = LEFT(RC.Last_Name,4) OR RIGHT(C.Last_Name,4) = RIGHT(RC.Last_Name,4))
 AND LEFT(C.Nickname,4) = LEFT(RC.Nickname,4)

 /*
-- Not using this criteria anymore as of 7/19/2016
-- Find contacts where one contact has a start date after 2/1/2016 and the other has the same display name.
declare @cutoffdate datetime = datefromparts(2016,2,1)
INSERT INTO #Dupes (Contact_ID, Related_Contact_ID, Gender_ID, Related_Gender_ID, Date_of_Birth, Related_Date_of_Birth, 
					First_Name, Related_First_Name, Nickname, Related_Nickname, Last_Name, Related_Last_Name, Notes, Domain_ID)
select
	cont.Contact_ID, cont2.Contact_ID,
	cont.Gender_ID, cont2.Gender_ID,
	cont.Date_of_Birth, cont2.Date_of_Birth,
	cont.First_Name, cont2.First_Name,
	cont.Nickname, cont2.Nickname,
	cont.Last_Name, cont2.Last_Name,
	'Participant Start Date after 2/1/2016 with same display name',
	cont.Domain_ID
from
	dbo.Participants part
	inner join dbo.Contacts cont  (nolock) on part.Contact_ID = cont.Contact_ID
	inner join dbo.Contacts cont2 (nolock) on cont.Display_Name = cont2.Display_Name
where
	cont.Contact_ID <> cont2.Contact_ID
	and part.Participant_Start_Date > @cutoffdate
	and cont.Display_Name <> 'Guest Giver'
	and cont.Display_Name <> ''
*/

 INSERT INTO #Dupes (Contact_ID, Related_Contact_ID, Gender_ID, Related_Gender_ID, Date_of_Birth, Related_Date_of_Birth, First_Name, Related_First_Name, Nickname, Related_Nickname, Last_Name, Related_Last_Name, Notes, Domain_ID)
select
	cont.Contact_ID, cont2.Contact_ID,
	cont.Gender_ID, cont2.Gender_ID,
	cont.Date_of_Birth, cont2.Date_of_Birth,
	cont.First_Name, cont2.First_Name,
	cont.Nickname, cont2.Nickname,
	cont.Last_Name,cont2.Last_Name,
	'Active contact and inactive contact with same display name',
	cont.Domain_ID
from
	dbo.Contacts cont (nolock)
	inner join dbo.Contacts cont2 (nolock) on cont.Display_Name = cont2.Display_Name
where
	cont.Contact_ID <> cont2.Contact_ID
	and cont.Contact_Status_ID = 1
	and cont2.Contact_Status_ID = 2
	and cont.Display_Name = cont2.Display_Name
	and not exists (select 1 from #Dupes d where cont.Contact_ID in (d.Contact_ID,d.Related_Contact_ID))
	 
--Index on temp table
CREATE INDEX IX_Dupes_ContactID ON #Dupes(Contact_ID)
CREATE INDEX IX_Dupes_RelatedContactID ON #Dupes(Related_Contact_ID)

--Remove if not target domain
DELETE 
FROM #Dupes
WHERE Domain_ID <> @DomainID

--Delete reciprocal records for any new records being added.
DELETE FROM #Dupes
WHERE Exists (select 1 from #Dupes d2
				where d2.Related_Contact_ID = #Dupes.Contact_ID)

--Remove if already related
DELETE FROM #Dupes 
WHERE EXISTS (SELECT 1 
			  FROM Contact_Relationships CR 
			  WHERE CR.Contact_ID IN (#Dupes.Contact_ID,#Dupes.Related_Contact_ID)
			   AND CR.Related_Contact_ID IN (#Dupes.Contact_ID, #Dupes.Related_Contact_ID))

--Remove if Gender or DOB inconsistent
DELETE FROM #Dupes
WHERE Gender_ID <> Related_Gender_ID 
 OR Date_of_Birth <> Related_Date_of_Birth

--Remove if system contact
DELETE FROM #Dupes
WHERE Contact_ID IN (@DefaultContactID)
 OR Related_Contact_ID IN (@DefaultContactID,@UnassignedContactID)

 select
	*
from
	#Dupes


--INSERT New Relationships
INSERT INTO Contact_Relationships (Contact_ID, Relationship_ID, Related_Contact_ID, Start_Date, Notes, Domain_ID)
SELECT DISTINCT 
	Contact_ID, 
	@DupeRelationshipID AS Relationship_ID, 
	Related_Contact_ID, 
	GETDATE() AS Start_Date,
	-- If the routine generated a note, go ahead and use it, otherwise just say dupefinder created it.
	Notes = isnull(#Dupes.Notes,'Created by Dupe Finder Service'), 
	@DomainID AS Domain_ID--, First_Name, Related_First_Name, Nickname, Related_Nickname, Last_Name, Related_Last_Name
FROM #Dupes


DROP TABLE #Dupes
END
GO

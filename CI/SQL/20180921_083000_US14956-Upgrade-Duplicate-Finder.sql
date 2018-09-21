USE [MinistryPlatform]
GO

CREATE OR ALTER PROCEDURE [dbo].[service_duplicate_finder]

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
CREATE TABLE #Dupes (Contact_ID INT, Related_Contact_ID INT, Gender_ID INT, Related_Gender_ID INT, Date_of_Birth DateTime, Related_Date_of_Birth DateTime, First_Name Varchar(50), Related_First_Name Varchar(50), Nickname Varchar(50), Related_Nickname Varchar(50), Last_Name Varchar(50), Related_Last_Name Varchar(50), Domain_ID INT)

--Populate Temp Table
INSERT INTO #Dupes (Contact_ID, Related_Contact_ID, Gender_ID, Related_Gender_ID, Date_of_Birth, Related_Date_of_Birth, First_Name, Related_First_Name, Nickname, Related_Nickname, Last_Name, Related_Last_Name, Domain_ID)

--Email Same
SELECT C.Contact_ID, RC.Contact_ID, C.Gender_ID, RC.Gender_ID AS Related_Gender_ID, C.Date_of_Birth, RC.Date_of_Birth AS Related_Date_of_Birth, C.First_Name, RC.First_Name AS Related_First_Name, C.Nickname, RC.Nickname AS Related_Nickname, C.Last_Name, RC.Last_Name AS Related_Last_Name, C.Domain_ID
FROM Contacts C
 INNER JOIN Contacts RC ON C.Contact_ID > RC.Contact_ID
  AND C.Email_Address = RC.Email_Address 
  AND C.Email_Address <> 'support@thinkministry.com'	
WHERE C.Household_ID <> RC.Household_ID	
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
WHERE C.Household_ID <> RC.Household_ID	
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
WHERE C.Household_ID <> RC.Household_ID	
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
WHERE C.Household_ID <> RC.Household_ID	
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
WHERE C.Household_ID <> RC.Household_ID	
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
WHERE C.Household_ID <> RC.Household_ID	
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
WHERE C.Household_ID <> RC.Household_ID	
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
WHERE C.Household_ID <> RC.Household_ID	
 AND C.Domain_ID = RC.Domain_ID	 	 
 AND C.Company = 0
 AND (LEFT(C.Last_Name,4) = LEFT(RC.Last_Name,4) OR RIGHT(C.Last_Name,4) = RIGHT(RC.Last_Name,4))
 AND LEFT(C.Nickname,4) = LEFT(RC.Nickname,4)
	 
--Index on temp table
CREATE INDEX IX_Dupes_ContactID ON #Dupes(Contact_ID)
CREATE INDEX IX_Dupes_RelatedContactID ON #Dupes(Related_Contact_ID)

--Remove if not target domain
DELETE 
FROM #Dupes
WHERE Domain_ID <> @DomainID
 
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

--INSERT New Relationships
INSERT INTO Contact_Relationships (Contact_ID, Relationship_ID, Related_Contact_ID, Start_Date, Notes, Domain_ID)
SELECT DISTINCT Contact_ID, @DupeRelationshipID AS Relationship_ID, Related_Contact_ID, GETDATE() AS Start_Date, Notes = 'Created by Dupe Finder Service', @DomainID AS Domain_ID--, First_Name, Related_First_Name, Nickname, Related_Nickname, Last_Name, Related_Last_Name
FROM #Dupes

END
GO

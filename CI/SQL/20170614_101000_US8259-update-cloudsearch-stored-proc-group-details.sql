USE [MinistryPlatform]
GO

SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO

IF EXISTS(SELECT name FROM sys.objects WHERE name = N'api_crds_Get_Connect_AWS_Data_v2')
  DROP PROCEDURE dbo.api_crds_Get_Connect_AWS_Data_v2;
GO


CREATE PROCEDURE [dbo].[api_crds_Get_Connect_AWS_Data_v2] 
AS 
BEGIN
SET nocount ON;

DECLARE @anywhereGroupTypeId INTEGER = 30; 
DECLARE @smallGroupTypeId INTEGER = 1;
DECLARE @onsiteGroupTypeId INTEGER = 8;

DECLARE @ageRangeAttributeTypeId INTEGER = 91;
DECLARE @groupTypeAttributeTypeId INTEGER = 73;

DECLARE @participantPinType INTEGER = 1;
DECLARE @gatheringPinType   INTEGER = 2;
DECLARE @sitePinType        INTEGER = 3;
DECLARE @smallGroupPinType  INTEGER = 4;

DECLARE @approvedStatusId   INTEGER = 3;

-- PARTICIPANTS
SELECT
    C.Nickname AS firstName,
	C.Last_Name AS lastname,
	null AS siteName,
	C.Email_Address AS emailAddress,
	C.Contact_ID AS contactId,
	P.Participant_ID AS participantId,
	A.Address_ID AS addressId,
	A.City AS city,
	A.[State/Region] AS state,
	A.Postal_Code AS zip,
	A.Latitude AS latitude,
	A.Longitude AS longitude,
	P.Host_Status_ID AS hostStatus,
	null AS groupId,
	null AS groupName,
	null AS groupStartDate,
	null AS groupDescription,
	null AS primarycontactId,
	null AS primaryContactEmail,
	null AS participantCount,
	null AS groupTypeId,
	H.Household_ID AS householdId,
	@participantPinType AS pinType,
	null AS groupStartDate,
	null AS groupCategory, 
	null AS groupType,  
	null AS groupAgeRange, 
	null AS groupMeetingDay,
	null AS groupMeetingTime,
	null AS groupVirtual,  
	null AS groupMeetingFrequency,
	0    AS groupKidsWelcome,
	null AS groupPrimaryContactFirstName,
	null AS groupPrimaryContactLastName,
	null AS groupPrimaryContactCongregation
FROM Participants P 
JOIN Contacts C ON C.Contact_ID = P.Contact_ID
LEFT JOIN Households H ON H.Household_ID = C.Household_ID
LEFT JOIN Addresses A ON A.Address_ID = H.Address_ID
WHERE P.Show_On_Map = 1
UNION 
--GATHERINGS / GROUPS
SELECT
    C.NickName AS firstName,
	C.Last_Name AS lastname,
	null As siteName,
	C.Email_Address AS emailAddress,
	C.Contact_ID AS contactId,
	C.Participant_Record AS participantId,
	G.Offsite_Meeting_Address AS addressId,
	A.City AS city,
	A.[State/Region] AS state,
	A.Postal_Code AS zip,
	A.Latitude AS latitude,
	A.Longitude AS longitude,
	null AS hostStatus,
	G.Group_ID AS groupId,
	G.Group_Name AS groupName,
	G.Start_Date as groupStartDate,
	G.Description AS groupDescription,
	G.Primary_Contact AS primarycontactId,
	C.Email_Address AS primaryContactEmail,
	(SELECT count(*) FROM group_participants gp WHERE gp.group_id = G.Group_id AND (GP.End_Date IS NULL OR GP.END_DATE > GETDATE())) AS participantCount,
	G.Group_Type_ID AS groupTypeId,
	C.Household_ID AS householdId,
	(IIF(G.Group_Type_ID = @anywhereGroupTypeId, @gatheringPinType, @smallGroupPinType)) AS pinType,
	G.Start_Date AS groupStartDate,
	(SELECT dbo.crds_GetGroupCategoryStringForAWS(G.Group_ID)) AS groupCategory, --function
	(SELECT dbo.crds_GetAtrributeStringForAWS(G.Group_Id,@groupTypeAttributeTypeId,0)) AS groupType,  --function
	(SELECT dbo.crds_GetAtrributeStringForAWS(G.Group_Id,@ageRangeAttributeTypeId,1)) AS groupAgeRange, --function
	MD.Meeting_Day AS groupMeetingDay,
	CONVERT(VARCHAR(10), G.Meeting_Time, 100) AS groupMeetingTime,
	-- CAST(G.Meeting_Time AS VARCHAR(16)) AS groupMeetingTime,
	(IIF(G.Offsite_Meeting_Address IS NULL, 1, 0)) AS groupVirtual,     -- sub select
	MF.Meeting_Frequency AS groupMeetingFrequency,
	(IIF(G.Kids_Welcome IS NULL, 0, G.Kids_Welcome)) AS groupKidsWelcome,
	C.Nickname AS groupPrimaryContactFirstName,
	C.Last_Name AS groupPrimaryContactLastName,
	CON.Congregation_Name AS groupPrimaryContactCongregation
	--function to return attribute properly as string for cloudsearch array
FROM Groups G
LEFT JOIN Addresses A ON A.Address_ID = G.Offsite_Meeting_Address
LEFT JOIN Contacts C ON C.Contact_ID = G.Primary_Contact
LEFT JOIN Participants P ON P.Contact_ID = C.Contact_ID
LEFT JOIN Meeting_Days MD ON MD.Meeting_Day_ID = G.Meeting_Day_ID
LEFT JOIN Meeting_Frequencies MF ON MF.Meeting_Frequency_ID = G.Meeting_Frequency_ID
LEFT JOIN Households H ON H.Household_ID = C.Household_ID
  LEFT JOIN Congregations CON ON CON.Congregation_ID = H.Congregation_ID
WHERE (G.Group_Type_ID IN (@anywhereGroupTypeId) AND G.Available_Online = 1 AND P.Host_Status_ID = @approvedStatusId AND (G.End_Date IS NULL OR G.END_DATE > GETDATE())) OR --ANYWHERE GATHERINGS
      (G.Group_Type_ID IN (@smallGroupTypeId) AND G.Available_Online = 1 AND G.Group_Is_Full = 0 AND (G.End_Date IS NULL OR G.END_DATE > GETDATE()))                            --SMALL GROUPS
UNION
--SITES
SELECT
    null AS firstName,
	null AS lastname,
	L.Location_Name AS siteName,
	null AS emailAddress,
	null AS contactId,
	null AS participantId,
	A.Address_ID AS addressId,
	A.City AS city,
	A.[State/Region] AS state,
	A.Postal_Code AS zip,
	A.Latitude AS latitude,
	A.Longitude AS longitude,
	null AS hostStatus,
	null AS groupId,
	null AS groupName,
	null AS groupStartDate,
	null AS groupDescription,
	null AS primarycontactId,
	null AS primaryContactEmail,
	null AS participantCount, 
	null AS groupTypeId,
	null AS householdId,
	@sitePinType AS pinType,
	null AS groupStartDate,
	null AS groupCategory, 
	null AS groupType,  
	null AS groupAgeRange, 
	null AS groupMeetingDay,
	null AS groupMeetingTime,
	null AS groupVirtual,  
	null AS groupMeetingFrequency,
	0    AS groupKidsWelcome,
	null AS groupPrimaryContactFirstName,
	null AS groupPrimaryContactLastName,
	null AS groupPrimaryContactCongregation
FROM Congregations C
JOIN Locations L ON L.Location_ID = C.Location_ID
LEFT JOIN Addresses A ON A.Address_ID = L.Address_ID
WHERE C.Congregation_ID NOT IN (2,5,15) AND (C.End_Date IS NULL OR C.END_DATE > GETDATE())

END


GO



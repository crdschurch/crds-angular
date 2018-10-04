USE [MinistryPlatform]
GO

IF EXISTS(SELECT 1
          FROM   INFORMATION_SCHEMA.ROUTINES
          WHERE  ROUTINE_NAME = 'api_crds_Get_Finder_AWS_Data_For_Single_Group'
                 AND SPECIFIC_SCHEMA = 'dbo')
BEGIN
    DROP PROCEDURE api_crds_Get_Finder_AWS_Data_For_Single_Group;
END
GO


CREATE PROCEDURE [dbo].[api_crds_Get_Finder_AWS_Data_For_Single_Group] @GroupId INTEGER
AS 
BEGIN
SET nocount ON;

DECLARE @anywhereGroupTypeId INTEGER = 30; 
DECLARE @smallGroupTypeId INTEGER = 1;

DECLARE @ageRangeAttributeTypeId INTEGER = 91;
DECLARE @groupTypeAttributeTypeId INTEGER = 73;

DECLARE @gatheringPinType   INTEGER = 2;
DECLARE @smallGroupPinType  INTEGER = 4;

DECLARE @approvedStatusId   INTEGER = 3;

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
	CAST(G.Meeting_Time AS VARCHAR(16)) AS groupMeetingTime,
	(IIF(G.Offsite_Meeting_Address IS NULL, 1, 0)) AS groupVirtual,     -- sub select
	MF.Meeting_Frequency AS groupMeetingFrequency,
	(IIF(G.Kids_Welcome IS NULL, 0, G.Kids_Welcome)) AS groupKidsWelcome,
	C.Nickname AS groupPrimaryContactFirstName,
	C.Last_Name AS groupPrimaryContactLastName,
	CON.Congregation_Name AS groupPrimaryContactCongregation,
   (IIF(G.Available_Online IS NULL, 0, G.Available_Online)) AS groupavailableonline
FROM Groups G
LEFT JOIN Addresses A ON A.Address_ID = G.Offsite_Meeting_Address
LEFT JOIN Contacts C ON C.Contact_ID = G.Primary_Contact
LEFT JOIN Participants P ON P.Contact_ID = C.Contact_ID
LEFT JOIN Meeting_Days MD ON MD.Meeting_Day_ID = G.Meeting_Day_ID
LEFT JOIN Meeting_Frequencies MF ON MF.Meeting_Frequency_ID = G.Meeting_Frequency_ID
LEFT JOIN Households H ON H.Household_ID = C.Household_ID
  LEFT JOIN Congregations CON ON CON.Congregation_ID = H.Congregation_ID
WHERE ((G.Group_Type_ID IN (@anywhereGroupTypeId) AND G.Available_Online = 1 AND P.Host_Status_ID = @approvedStatusId AND (G.End_Date IS NULL OR G.END_DATE > GETDATE())) OR --ANYWHERE GATHERINGS
      (G.Group_Type_ID IN (@smallGroupTypeId) AND G.Group_Is_Full = 0 AND (G.End_Date IS NULL OR G.END_DATE > GETDATE()))) AND G.Group_ID = @GroupId                            --SMALL GROUPS

END


GO


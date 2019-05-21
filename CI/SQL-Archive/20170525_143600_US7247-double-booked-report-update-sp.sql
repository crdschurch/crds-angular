USE [MinistryPlatform]
GO
/* Modifications: 
	KD 5/25: Added start and end date filter
*/


/****** Object:  StoredProcedure [dbo].[report_Event_Double_Booked_ByCongregation]    Script Date: 5/25/2017 2:18:20 PM ******/
SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO



ALTER PROCEDURE [dbo].[report_Event_Double_Booked_ByCongregation]

	@DomainID varchar(40)
	,@UserID varchar(40)
	,@PageID INT
	,@MyConflictsOnly BIT = 0
	,@CongregationID INT = NULL
	,@BeginDate DATETIME
	,@EndDate DATETIME


AS
BEGIN

DECLARE @ContactID INT
SELECT @ContactID = Contact_ID FROM dp_Users WHERE User_GUID = @UserID

SET @BeginDate =  DATEADD(day, DATEDIFF(day, 0, @BeginDate), '00:00:00');
SET @EndDate =  DATEADD(day, DATEDIFF(day, 0, @EndDate), '23:59:00');

SELECT ER.Event_Room_ID
, E.Event_Title
, E.Event_ID
, Contacts.Display_Name
, Contacts.Email_Address
, ISNULL(R.Room_Number + ' - ','') + R.Room_Name AS DisplayRoom
, E.Event_Start_Date
, E.Event_End_Date
, RIGHT(CONVERT(Varchar, E.Event_Start_Date, 100), 7) AS StartTime
, RIGHT(CONVERT(Varchar, E.Event_End_Date, 100), 7) AS EndTime
, RIGHT(CONVERT(Varchar, DATEADD(n, - (1 * E.Minutes_for_Setup), E.Event_Start_Date), 100), 7) AS Resource_Hold_Start
, RIGHT(CONVERT(Varchar, DATEADD(n, E.Minutes_for_Cleanup, E.Event_End_Date), 100), 7) AS Resource_Hold_End
, R.Room_Name
, Conflict.*

FROM         Event_Rooms ER
 INNER JOIN dp_Domains Dom ON Dom.Domain_ID = ER.Domain_ID
 INNER JOIN Rooms R ON R.Room_ID = ER.Room_ID
 INNER JOIN Events E ON E.Event_ID = ER.Event_ID AND Event_Start_Date >= Getdate() AND ISNULL(E.Congregation_ID,0) = ISNULL(@CongregationID,ISNULL(E.Congregation_ID,0)) 
 INNER JOIN Contacts ON E.Primary_Contact = Contacts.Contact_ID
 OUTER APPLY (SELECT     ERC.Event_Room_ID AS Confli_Event_Room_ID, EC.Event_Title AS Confli_Event_Title, EC.Event_ID AS Confli_Event_ID, RIGHT(CONVERT(Varchar, DATEADD(n, 
                                                   - (1 * EC.Minutes_for_Setup), EC.Event_Start_Date), 100), 7) AS Conflict_Hold_Start, RIGHT(CONVERT(Varchar, DATEADD(n, EC.Minutes_for_Cleanup, 
                                                   EC.Event_End_Date), 100), 7) AS Conflict_Hold_End, EC.Event_Start_Date AS Confli_Event_Start_Date, RC.Room_Name AS Confli_Room_Name
                            FROM          Event_Rooms ERC INNER JOIN
                                                   Rooms RC ON RC.Room_ID = ERC.Room_ID INNER JOIN
                                                   Events EC ON EC.Event_ID = ERC.Event_ID
                            WHERE      ERC.Cancelled = 0 AND RC.Room_ID = R.Room_ID AND ER.Event_ID <> ERC.Event_ID AND (EC.__Reservation_Start BETWEEN 
                                                   E.__Reservation_Start AND E.__Reservation_End OR
                                                   EC.__Reservation_End BETWEEN E.__Reservation_Start AND E.__Reservation_End OR
                                                   E.__Reservation_Start BETWEEN EC.__Reservation_Start AND EC.__Reservation_End OR
                                                   E.__Reservation_End BETWEEN EC.__Reservation_Start AND EC.__Reservation_End)) Conflict
WHERE     ER.Cancelled = 0
 AND Conflict.Confli_Event_Room_ID IS NOT NULL
 AND Dom.Domain_GUID = @DomainID
 AND (@MyConflictsOnly = 0 OR Contacts.Contact_ID = @ContactID)
 AND E.Event_Start_Date > @BeginDate
 AND E.Event_Start_Date < @EndDate

ORDER BY E.Event_Start_Date, E.Event_Title, R.Room_Name

END




GO



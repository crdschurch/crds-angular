USE [MinistryPlatform]
GO

-- =======================================================================================================================
-- Author:      Shakila Rajaiah 
-- Created Date: 10/8/2019
-- Description: This stored procedure generates a list of events, in a site for a given date range. 
--				It also includes events like camps which do not have a room associated with it. 
--				It indicates if there is childcare associated with the event based on the groups they are under.	
-- User Story : US17315 - Event Listing Reports for Community Care
--	
-- ======================================================================================================================================

CREATE OR ALTER procedure [dbo].[report_CRDS_Event_Listing_For_CommunityCare]
    @DomainID VARCHAR(40)
   ,@UserID VARCHAR(40)
   ,@PageID INT
   ,@FromDate DATETIME
   ,@ToDate DATETIME
   ,@CongregationID as varchar (MAX)

AS
BEGIN

DECLARE @ShowCancelled BIT
DECLARE @ShowNoRoomsSet BIT
DECLARE @ListTime INT
DECLARE @ShowBldgRm INT
DECLARE @ExcludeRejectedRoomReservation BIT 
DECLARE @EventTypes varchar(25)

   --set @DomainID = '0FDE7F32-37E3-4E0B-B020-622E0EBD6BF0'
   set @ShowCancelled = 1
   set @ShowNoRoomsSet = 1
   set @ListTime = 1
   set @ShowBldgRm = 1
   set @EventTypes = '0'
   set @ExcludeRejectedRoomReservation = 1;

   IF OBJECT_ID('tempdb..#ReportData') IS NOT NULL   
   DROP TABLE #ReportData

--create a temp table to hold event related information.
CREATE TABLE #ReportData
(
    event_id int,
    event_title	varchar(150),
	event_type_ID int,
	event_type varchar(50),	
    event_date	datetime,
	event_start_date datetime,
	event_end_date	datetime,
	participants_expected int,
	contact_person varchar(500),
	contact_email varchar(500),
	contact_phone varchar(500),
	congregation_name varchar(500),
	childcare_available int,
	Room_1 varchar(500),
	Room_2 varchar(500),
	Room_3 varchar(500),
	Room_4 varchar(500),
	Room_5 varchar(500),
	Room_6 varchar(500),
	Rooms varchar(500),
	ministry_name varchar(500),
	program_name varchar(150),
	cancelled int,
	description	varchar(2000),
	registration_form varchar(10),
	external_registration_url varchar(4000),
	reservation_start datetime,
	reservation_end datetime
)

--insert data into temp table #ReportData 
    INSERT INTO #ReportData
                (Event_ID
                ,Event_Title
				,Event_Type_ID
				,Event_Type             
                ,Event_Date
                ,Event_Start_Date
                ,Event_End_Date
                ,Participants_Expected			
                ,Contact_Person
                ,Contact_Email
                ,Contact_Phone
                ,Congregation_Name
				,Childcare_Available
                ,Room_1
                ,Room_2
                ,Room_3
                ,Room_4
                ,Room_5
                ,Room_6
                ,Rooms
                ,Ministry_Name
                ,[Program_Name]
                ,Cancelled
				,[Description]
				,Registration_Form
				,External_Registration_URL
                ,Reservation_Start
                ,Reservation_End
                )

      SELECT  E.Event_ID
           ,E.Event_Title
		   ,E.Event_Type_ID
		   ,et.Event_Type         
           ,CASE WHEN @FromDate >= E.Event_Start_Date THEN @FromDate ELSE E.Event_Start_Date END AS Event_Date
           ,Event_Start_Date
           ,Event_End_Date
           ,Participants_Expected		   
           ,ISNULL(C.Display_Name, '') AS Contact_Person
           ,ISNULL(C.Email_Address,'') AS Contact_Email
           ,ISNULL(C.Company_Phone,'') AS Contact_Phone
           ,ISNULL(Congregation_Name, 'Congregation Not Set') AS Congregation_Name
		   ,'' -- childcare available
           ,ISNULL(Room_1, 'No Rooms Set') AS Room_1
		   ,ISNULL(Room_2, '') AS Room_2
		   ,ISNULL(Room_3, '') AS Room_3
		   ,ISNULL(Room_4, '') AS Room_4
		   ,ISNULL(Room_5, '') AS Room_5
		   ,ISNULL(Room_6, '') AS Room_6
           ,Rooms = ISNULL(Room_1, 'No Rooms Set') + ISNULL(' | ' + Room_2, '') + ISNULL(' | ' + Room_3, '') + ISNULL(' | ' + Room_4, '')
            + ISNULL(' | ' + Room_5, '') + ISNULL(' | ' + Room_6, '')
           ,M.Ministry_Name
           ,Prog.[Program_Name]
           ,E.Cancelled
		   ,ISNULL(E.[Description],'') AS Description
		   ,Registration_Form AS Form
		   ,ISNULL(External_Registration_URL, '') AS External_Registration_URL
           ,E.__Reservation_Start AS Reservation_Start
           ,E.__Reservation_End AS Reservation_End
    FROM    dbo.Events E
	        INNER JOIN dbo.dp_Domains Dom
				ON Dom.Domain_ID = E.Domain_ID
            INNER JOIN dbo.Contacts C
                ON C.Contact_ID = E.Primary_Contact
			INNER JOIN	dbo.Event_Types et
				ON E.Event_Type_ID = et.Event_Type_ID 
            INNER JOIN dbo.Programs Prog
                ON Prog.Program_ID = E.Program_ID
            INNER JOIN dbo.Ministries M
                ON M.Ministry_ID = Prog.Ministry_ID
            LEFT OUTER JOIN dbo.Congregations Cong
                ON Cong.Congregation_ID = E.Congregation_ID
            LEFT OUTER JOIN (SELECT Event_ID, _Approved
                                   ,Room_1 = MAX(CASE WHEN Position = 1 THEN Room_Name
                                                 END)
                                   ,Room_2 = MAX(CASE WHEN Position = 2 THEN Room_Name
                                                 END)
                                   ,Room_3 = MAX(CASE WHEN Position = 3 THEN Room_Name
                                                 END)
                                   ,Room_4 = MAX(CASE WHEN Position = 4 THEN Room_Name
                                                 END)
                                   ,Room_5 = MAX(CASE WHEN Position = 5 THEN Room_Name
                                                 END)
                                   ,Room_6 = MAX(CASE WHEN Position = 6 THEN Room_Name
                                                 END)
                                   ,Rooms = COUNT(*)
                             FROM   (SELECT Event_ID, _Approved
                                           ,Position = row_number() over (partition by Event_ID ORDER BY Maximum_Capacity DESC)
                                           ,R.Room_Name + ' RM' + ISNULL(R.Room_Number, '') AS Room_Name
                                           ,B.Building_Name + '/' + R.Room_Name + ' RM' + ISNULL(R.Room_Number, '') AS Room_Name_Building
                                     FROM   Event_Rooms ER
                                            INNER JOIN Rooms R
                                                ON R.Room_ID = ER.Room_ID
                                            INNER JOIN Buildings B
                                                ON B.Building_ID = R.Building_ID) ER1
                             GROUP BY ER1.Event_ID, ER1._Approved) ER2
                ON ER2.Event_ID = E.Event_ID
    WHERE   
			Dom.Domain_GUID = @DomainID
            AND 
            E.Congregation_ID IN (SELECT Item FROM dbo.dp_Split(@CongregationID, ','))
			
			AND (
					(E.Event_Start_Date >= @FromDate AND E.Event_Start_Date < @ToDate + 1)
					OR @FromDate between E.Event_Start_Date AND E.Event_End_Date
				)
            AND (
					ISNULL(@EventTypes,'0') = '0'
					OR E.Event_Type_ID is NULL
					OR E.Event_Type_ID in (SELECT Item FROM dp_Split(@EventTypes, ','))
				)
          AND (
            @ExcludeRejectedRoomReservation = 0 OR (@ExcludeRejectedRoomReservation = 1 AND ER2._Approved = 1)
        )
	-- add events that are camps and don't have a room associated with it.
	UNION
		SELECT  
			E.Event_ID
           ,E.Event_Title
		   ,E.Event_Type_ID
		   ,et.Event_Type         
           ,CASE WHEN @FromDate >= E.Event_Start_Date THEN @FromDate ELSE E.Event_Start_Date END AS Event_Date
           ,Event_Start_Date
           ,Event_End_Date
           ,Participants_Expected		   
           ,ISNULL(C.Display_Name, '') AS Contact_Person
           ,ISNULL(C.Email_Address,'') AS Contact_Email
           ,ISNULL(C.Company_Phone,'') AS Contact_Phone
           ,ISNULL(Congregation_Name, 'Congregation Not Set') AS Congregation_Name
		   ,'' -- childcare available
           ,'' -- Room_1
		   ,''-- Room_2
		   ,'' -- Room_3
		   ,'' -- Room_4
		   ,'' -- Room_5
		   ,'' --Room_6
           ,'' --Rooms
           ,M.Ministry_Name
           ,Prog.[Program_Name]
           ,E.Cancelled
		   ,ISNULL(E.[Description],'') AS Description
		   ,Registration_Form AS Form
		   ,ISNULL(External_Registration_URL, '') AS External_Registration_URL
		   ,E.__Reservation_Start AS Reservation_Start
           ,E.__Reservation_End AS Reservation_End
    FROM    dbo.Events E
	        INNER JOIN dbo.dp_Domains Dom
				ON Dom.Domain_ID = E.Domain_ID
            INNER JOIN dbo.Contacts C
                ON C.Contact_ID = E.Primary_Contact
			INNER JOIN	dbo.Event_Types et
				ON E.Event_Type_ID = et.Event_Type_ID 
            INNER JOIN dbo.Programs Prog
                ON Prog.Program_ID = E.Program_ID
            INNER JOIN dbo.Ministries M
                ON M.Ministry_ID = Prog.Ministry_ID
            LEFT OUTER JOIN dbo.Congregations Cong
                ON Cong.Congregation_ID = E.Congregation_ID
    WHERE   
			Dom.Domain_GUID = @DomainID
            AND 
            E.Congregation_ID IN (SELECT Item FROM dbo.dp_Split(@CongregationID, ','))
			
			AND (
					(E.Event_Start_Date >= @FromDate AND E.Event_Start_Date < @ToDate + 1)
					OR @FromDate between E.Event_Start_Date AND E.Event_End_Date
				)
			AND
				E.Event_Type_ID IN (6,8,9)
	ORDER BY Congregation_Name
           ,Event_Start_Date
           ,Event_Title

--- For Child care available, drop if temp table already exists
	IF (OBJECT_ID('tempdb..#TempChild') IS NOT NULL) DROP TABLE #TempChild

-- create a temp table for child care
	CREATE TABLE #TempChild
	(
		event_id int,
		event_title	varchar(150),
		childcare_available int, 
		event_type_ID int,
		event_type varchar(50)	
	)

--insert data into temporary table to see if child care is available.
    INSERT INTO #TempChild
                (Event_ID
                ,Event_Title
				,ChildCare_Available
				,Event_Type_ID
				,Event_Type)  
	SELECT DISTINCT 
		rd.Event_ID,  rd.Event_Title, 
		g.Child_Care_Available, 
		eg.Group_ID,
		rd.Event_Type
	FROM
		#ReportData rd, dbo.Event_Groups eg, dbo.groups g  
	WHERE
		rd.event_id = eg.Event_ID
	AND eg.Group_ID = G.Group_ID
	AND g.Child_Care_Available = 1
	order by Event_Title 

	-- Update the events with Childcare available information
	UPDATE r
	SET r.childcare_available = c.childcare_available
	FROM #ReportData r 
		INNER JOIN #TempChild c
		ON r.event_id = c.event_id 

	--Add any dates between date ranges to print the individual date
    SELECT DATEDIFF("dd"
                    ,CONVERT(DATE,CASE WHEN CONVERT(DATE,@FromDate,101)>= CONVERT(DATE,event_start_date,101) THEN CONVERT(DATE,@FromDate,101) ELSE CONVERT(DATE,event_start_date,101) END,101)
                    ,CONVERT(DATE,CASE WHEN CONVERT(DATE,event_end_date,101) <= CONVERT(DATE,@ToDate,101) THEN CONVERT(DATE,event_end_date,101) ELSE CONVERT(DATE,@ToDate,101) END,101)
                    ) NumDaysToAdd
                ,Event_ID
                ,Event_Title
				,Event_Type_ID
				,Event_Type               
                ,Event_Date
                ,Event_Start_Date
                ,Event_End_Date
                ,Participants_Expected			
                ,Contact_Person
                ,Contact_Email
                ,Contact_Phone
                ,Congregation_Name
                ,ChildCare_Available
                ,Room_1
                ,Room_2
                ,Room_3
                ,Room_4
                ,Room_5
                ,Room_6
                ,Rooms
                ,Ministry_Name
                ,[Program_Name]
                ,Cancelled
				,[Description]
				 ,Registration_Form
				,External_Registration_URL
                ,Reservation_Start
                ,Reservation_End
    INTO #RoomsAddDate
    FROM #ReportData
    WHERE 
		CONVERT(DATE,Event_Start_Date,101) <> CONVERT(DATE,Event_End_Date,101)
	AND 
		#ReportData.event_type_ID NOT IN (6,8,9)

    DECLARE @i INT = 1
            ,@NumDays INT = (SELECT MAX(NumDaysToAdd) FROM #RoomsAddDate)+1

    WHILE @i < @NumDays
    BEGIN
    INSERT INTO #ReportData
                (Event_ID
                ,Event_Title
				,Event_Type_ID
				,Event_Type
                ,Event_Date
                ,Event_Start_Date
                ,Event_End_Date
                ,Participants_Expected			
                ,Contact_Person
                ,Contact_Email
                ,Contact_Phone
                ,Congregation_Name
				,ChildCare_Available
                ,Room_1
                ,Room_2
                ,Room_3
                ,Room_4
                ,Room_5
                ,Room_6
                ,Rooms
                ,Ministry_Name
                ,[Program_Name]
                ,Cancelled
				,[Description]
				,Registration_Form
				,External_Registration_URL
                ,Reservation_Start
                ,Reservation_End
                )
            SELECT Event_ID
                ,Event_Title
				,Event_Type_ID
				,Event_Type
                ,DATEADD(DAY,@i,Event_Date) AS Event_Date
                ,Event_Start_Date
                ,Event_End_Date
                ,Participants_Expected		
                ,Contact_Person
                ,Contact_Email
                ,Contact_Phone
                ,Congregation_Name
				,ChildCare_Available
                ,Room_1
                ,Room_2
                ,Room_3
                ,Room_4
                ,Room_5
                ,Room_6
                ,Rooms
                ,Ministry_Name
                ,[Program_Name]
                ,Cancelled
				,[Description]
				,Registration_Form
				,External_Registration_URL
                ,Reservation_Start
                ,Reservation_End
            FROM #RoomsAddDate
            WHERE NumDaysToAdd >= @i

    SET @i= @i + 1
    END

    SELECT      Congregation_Name AS 'Site'
				,Event_ID
                ,Event_Title
				,Event_Type_ID	
				,Event_Type
				,CONVERT(datetime,CONVERT(VARCHAR(10),Event_Date,101)) AS Event_Date
                ,Event_Start_Date
                ,Event_End_Date
                ,Participants_Expected
				,Contact_Person
                ,Contact_Email
                ,Contact_Phone              
                --,ChildCare_Available
				,ChildCare_Available = CASE WHEN Childcare_Available = 1 THEN 'Yes' ELSE '' END 
                ,Room_1
                ,Room_2
                ,Room_3
                ,Room_4
                ,Room_5
                ,Room_6
                ,Rooms
                ,Ministry_Name
                ,[Program_Name]
                ,Cancelled
				,[Description]
				,Registration_Form = CASE WHEN Registration_Form IS NOT NULL THEN 'Yes' ELSE 'No' END 
				,External_Registration_URL AS 'Social Link'
                ,Reservation_Start
                ,Reservation_End
    FROM #ReportData
    WHERE Cancelled IN (@ShowCancelled,0)
        AND (
				(@ShowNoRoomsSet = 0 AND @ShowBldgRm = 2 AND ISNULL(Rooms,'No Rooms Set') NOT LIKE '%No Rooms Set')
                OR (@ShowNoRoomsSet = 0 AND @ShowBldgRm = 1 )
                OR @ShowNoRoomsSet = 1
            )
    ORDER BY Congregation_Name
			,Event_Title
            ,Event_Date
            ,CASE WHEN @ListTime = 1 THEN CONVERT(TIME,Event_Start_Date) ELSE CONVERT(TIME,Reservation_Start) END
            ,CASE WHEN @ListTime = 1 THEN CONVERT(TIME,Event_End_Date) ELSE CONVERT(TIME,Reservation_End) END
            

--    --CLEAN UP
    DROP TABLE #ReportData
    DROP TABLE #RoomsAddDate

END
GO
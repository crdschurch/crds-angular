USE [MinistryPlatform]
GO

/****** Object:  StoredProcedure [dbo].[report_CRDS_Event_Listing_For_CommunityCare_Email_XML_Report]    Script Date: 1/13/2020 5:05:55 PM ******/
SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO
-- =======================================================================================================================
-- Author:      Shakila Rajaiah 
-- Created Date: 12/3/2019
--	User Story : US18674 - Event_Listing_For_CommunityCare_Email_XML_Report

--	Description: This stored procedure generates a list of events, 
--	grouped by sites for the next seven days from today.  
--	User Group: Hello Team at Crossroads..
--	Congregations: List events from all congregations
--	Execution: This has to be run daily through a SQL Agent job, daily at 5 AM
--	Emailed TO: and be emailed to the group 'xroadshelloeventlisting@crossroads.net' 
--	Contact First Name:  Helloteam
--	Contact LastName:  Eventlistingreports
--	ToContact/Email: This is based on the Contact_ID for the last name 'EventlistingReports' in the Contacts table.
--	The user has been alterted to not chage the last name of the contact.
--	Modified Date:  2/18/2020 by SRajaiah
--	The parameters for the emails- @DomainID was removed (always 1) and @UserID is instead retrieved from the user table.
--	Hardcoded values for  @ToContactID , @ReplyToContact_ID , @CommUser_ID are now retrieved from the tables.
--	Modified Date:  2/21/2020 by SRajaiah
--  Made changes to lines 481 and 482: Changed Select to select TOP 1 based on the first and last names and not their email ID. 
--
-- ======================================================================================================================================

CREATE OR ALTER   procedure [dbo].[report_CRDS_Event_Listing_For_CommunityCare_Email_XML_Report]

AS
BEGIN

DECLARE @ShowCancelled BIT
DECLARE @ShowNoRoomsSet BIT
DECLARE @ListTime INT
DECLARE @ShowBldgRm INT
DECLARE @ExcludeRejectedRoomReservation BIT 
DECLARE @EventTypes varchar(25)
DECLARE @FromDate datetime
declare @ToDate datetime

   set @ShowCancelled = 0
   set @ShowNoRoomsSet = 1
   set @ListTime = 1
   set @ShowBldgRm = 1
   set @EventTypes = '0'
   set @ExcludeRejectedRoomReservation = 1;
   set @FromDate = getdate()
   set @ToDate = dateadd(day,7,@FromDate) -- one week from today

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
	childcare_available varchar(10),
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
           ,ISNULL(Participants_Expected, '') AS Participants	   
           ,ISNULL(C.Display_Name, '') AS Contact_Person
           ,ISNULL(C.Email_Address,'') AS Contact_Email
           ,ISNULL(C.Company_Phone,'') AS Contact_Phone
           ,ISNULL(Congregation_Name, 'Congregation Not Set') AS Congregation_Name
		   ,'' -- childcare_available
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
		   --,Registration_Form AS Form
		   ,CASE WHEN Registration_Form IS NOT NULL THEN 'Yes' ELSE 'No' END AS Form
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
			E.Congregation_ID IN (select Congregation_ID from dbo.Congregations)
			
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
           ,ISNULL(Participants_Expected, '') AS Participants		   
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
		   ,CASE WHEN Registration_Form IS NOT NULL THEN 'Yes' ELSE 'No' END AS Form
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
			E.Congregation_ID IN (select Congregation_ID from dbo.Congregations)
			
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
		childcare_available varchar(10), 
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
		'Yes', 
		eg.Group_ID,
		rd.Event_Type
	FROM
		#ReportData rd, dbo.Event_Groups eg, dbo.groups g  
	WHERE
		rd.event_id = eg.Event_ID
	AND eg.Group_ID = G.Group_ID
	AND g.Child_Care_Available = 1
	order by Event_Title 

--update records where childcare is available...

	UPDATE r
	SET r.childcare_available = c.childcare_available
	FROM #ReportData r 
		INNER JOIN #TempChild c
		ON r.event_id = c.event_id 


----Add any dates between date ranges to print the individual date
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
    WHERE CONVERT(DATE,Event_Start_Date,101) <> CONVERT(DATE,Event_End_Date,101)

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

-- Create the XML for the body of the report
DECLARE @Body NVARCHAR(MAX)
DECLARE @Title NVARCHAR(500)
SET @Title= N' <h1>Event Listing - Community Care. </h1>
</br>'

SET     @Body = @Title + N'<table width=''100%'' border=''1'' cellspacing=''0'' cellpadding=''5''>'
    + N'<tr><th>Congregation_Name</th><th>Event_Title</th><th>Event_Type</th><th>Event_Date</th><th>Event_Start_Date</th><th>Event_End_Date</th><th>Participants</th><th>Contact_Person</th><th>Contact_Email</th><th>Contact_Phone</th><th>ChildCare</th><th>Rooms</th><th>Ministry_Name</th><th>Program_Name</th><th>Cancelled</th><th>Description</th><th>Registration_Form</th><th>Social_Link</th><th>Reservation_Start</th><th>Reservation_End</th></tr>'
    + CAST((
 SELECT 
		Congregation_Name  as td ,
		Event_Title     as td ,
		Event_Type    as td,
		CONVERT(datetime,CONVERT(VARCHAR(10),Event_Date,101)) AS td,--Event_Date
		Event_Start_Date as td,
        Event_End_Date as td,
        Participants_Expected as td,
		Contact_Person as td,
        Contact_Email as td,
        Contact_Phone  as td,                           
		ChildCare_Available as td,
		Rooms  as td,
        Ministry_Name as td,
        [Program_Name] as td,
        Cancelled as td,
		[Description] as td,
		Registration_Form as td,
		--Registration_Form = CASE WHEN Registration_Form IS NOT NULL THEN 'Yes' ELSE 'No' END,  --AS td,
		External_Registration_URL AS td, --'Social Link',
        Reservation_Start  as td,
        Reservation_End as td
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

	FOR XML RAW('tr'), ELEMENTS
	--FOR XML RAW, elements, root('tr') gives whole result set as one line terrible!
    ) AS NVARCHAR(MAX))
    + N'</table>'

	select @Body

	----Create the communications records for inserting into dp_Communications table...

--	SET NOCOUNT ON;
	DECLARE @ReplyToContact_ID int
	DECLARE @ToContactID int
	DECLARE @Subject varchar(MAX)
	DECLARE @Start datetime
	DECLARE @CommunicationStatus int
	DECLARE @NewMessageID int
	DECLARE @FromEmail varchar(256)
	DECLARE @ToEmail varchar(256)
	DECLARE @NewCommMsgID int
	DECLARE @CommUser_ID int

	-- 2/18/2020: Don't hardcode the values, get them from the respective tables based on their last and first names - not their email.
    --SET @ToContactID = 7837576 -- Hello team, don't hard code this as the  contact ID may change for Demo and prod.
	--SET @ReplyToContact_ID = 1519180, where C.Last_Name = 'Communications' and C.First_Name = 'Crossroads'
	SELECT @ToContactID = (SELECT TOP 1 C.Contact_ID FROM Contacts C WHERE (C.Last_Name = 'EventlistingReports'))
	SELECT @ReplyToContact_ID = (SELECT TOP 1 C.Contact_ID FROM Contacts C WHERE (C.Last_Name = 'Communications' and C.First_Name = 'Crossroads'))	
	SELECT @CommUser_ID = (SELECT TOP 1 U.User_ID FROM dp_Users U WHERE (U.Contact_ID = @ReplyToContact_ID)) 
		
	SET @Subject = 'Event Listing Report for Community Care - All Sites'
	SET @Start = GETDATE()
	SET @CommunicationStatus = 3 --ready to send
	SELECT @FromEmail = C.Email_Address FROM Contacts C WHERE (C.Contact_ID = @ReplyToContact_ID)
	SELECT @ToEmail = C.Email_Address FROM Contacts C WHERE (C.Contact_ID = @ToContactID)

--Insert into the communications tables to email this message to the users 
	INSERT INTO dp_Communications
		(Author_User_ID, Domain_ID, [Subject], Body, [Start_Date], Communication_Status_ID, From_Contact, Reply_to_Contact, To_Contact) --, Expire_Date
	VALUES
		(@CommUser_ID, 1, @Subject, @Body, @Start, @CommunicationStatus, @ReplyToContact_ID, @ReplyToContact_ID, @ToContactID) --, @Expire

	SET @NewMessageID = SCOPE_IDENTITY()

	INSERT INTO dp_Communication_Messages
		(Communication_ID, Action_Status_ID, Action_Status_Time, Contact_ID, [From], [To], Reply_To, [Subject], Body, Domain_ID, Deleted)
	VALUES
		(@NewMessageID, 2, GetDate(), @ToContactID, @FromEmail, @ToEmail, @FromEmail, @Subject, @Body, 1, 0)

-- the job will be picked up, emails sent and appropriate statuses changed...

------    --CLEAN UP
	DROP TABLE #ReportData
    DROP TABLE #RoomsAddDate

END
GO



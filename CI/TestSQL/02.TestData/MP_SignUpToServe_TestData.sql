USE [MinistryPlatform]
GO

--Sign Up to Serve Data set up
DECLARE @groupIdSB AS INT
DECLARE @groupIdKC AS INT
DECLARE @eventTypeIdSB1 AS INT
DECLARE @eventTypeIdSB2 AS INT
DECLARE @eventTypeIdKC1 AS INT
DECLARE @eventTypeIdKC2 AS INT
DECLARE @opportunityIdKC1 AS INT
DECLARE @opportunityIdKC2 AS INT
DECLARE @opportunityIdKC3 AS INT
DECLARE @opportunityIdKC4 AS INT
DECLARE @opportunityIdSB1 AS INT
DECLARE @opportunityIdSB2 AS INT
DECLARE @opportunityIdSB3 AS INT
DECLARE @opportunityIdSB4 AS INT
DECLARE @eventId AS INT
DECLARE @eventDate AS DATE
DECLARE @eventStartDate1 AS DATETIME
DECLARE @eventEndDate1 AS DATETIME
DECLARE @eventStartDate2 AS DATETIME
DECLARE @eventEndDate2 AS DATETIME


SET @groupIdSB = (SELECT top 1 group_id from Groups where group_name = '(t) Superbowl Oakley Group');
SET @groupIdKC = (SELECT top 1 group_id from Groups where group_name = '(t) KidsClub Oakley Group');

--Create new Sign Up to Serve Evet Types
SET @eventTypeIdSB1 = (SELECT top 1 Event_Type_ID FROM Event_Types where Event_Type = '(t) Superbowl Oakley daily 10:00');

SET @eventTypeIdSB2 = (SELECT top 1 Event_Type_ID FROM Event_Types where Event_Type = '(t) Superbowl Oakley daily 3:00');

SET @eventTypeIdKC1 = (SELECT top 1 Event_Type_ID FROM Event_Types where Event_Type = '(t) KC Nursery Oakley weekly 11:00');

SET @eventTypeIdKC2 = (SELECT top 1 Event_Type_ID FROM Event_Types where Event_Type = '(t) KC Nursery Oakley weekly 1:00');

--Create new Sign Up to Serve Events

DECLARE @eventflag AS INT;
SET @eventflag = 1;
SET @eventDate = DATEADD(DAY, -7,GETDATE());

WHILE (@eventflag <=6)
BEGIN
SET IDENTITY_INSERT [dbo].[Events] ON;

SET @eventDate = DATEADD(DAY, 7, @eventDate);
SET @eventId = (SELECT MAX(Event_ID) FROM Events) + 1 ;
SET @eventStartDate1 = (CAST(CAST(@eventDate AS DATE) AS DATETIME) + CAST('10:00' AS TIME)) ;
SET @eventEndDate1 = (CAST(CAST(@eventDate AS DATE) AS DATETIME) + CAST('11:00' AS TIME));

INSERT INTO Events
(Event_ID , Event_Title                  , Event_Type_ID  , Congregation_ID, Location_ID, Meeting_Instructions, Description, Program_ID, Primary_Contact, Participants_Expected, Minutes_for_Setup, Event_Start_Date           , Event_End_Date, Minutes_for_Cleanup, Cancelled, _Approved, Public_Website_Settings, Visibility_Level_ID, Featured_On_Calendar, Online_Registration_Product, Registration_Form, Registration_Start, Registration_End, Registration_Active, _Web_Approved, [Check-in_Information], [Allow_Check-in], Ignore_Program_Groups, Prohibit_Guests, [Early_Check-in_Period], [Late_Check-in_Period], Other_Event_Information, Parent_Event_ID, Priority_ID, Domain_ID, On_Connection_Card, External_Registration_URL, On_Donation_Batch_Tool, __ExternalEventID, __ExternalOrganizerUserID, __ExternalGroupID, __ExternalRoomID, __ExternalContactUserID, Project_Code, Participant_Reminder_Settings, Send_Reminder, Reminder_Sent, Reminder_Days_Prior_ID, __ExternalTripID, __ExternalTripLegID ) VALUES
( @eventId, '(t) Superbowl Oakley 10:00 ', @eventTypeIdSB1, 1              , 3          , null                , null       , 109       , 2562428        , null                 , 0                , @eventStartDate1           , @eventEndDate1, 0                  , 0        , 1        , null                   , 4                  , 0                   , null                       , null             , null              , null            , null               , 1            , null                  , 0               , 0                    , 0              , null                   , null                  , null                   , null           , null       , 1        , null              , null                     , 0                     , null             , null                     , null             , null            , null                   , null        , null                         , 1            , null         , 2                     , null            , null );   

SET IDENTITY_INSERT [dbo].[Events] OFF;

INSERT INTO Event_Groups
(Event_ID, Group_ID, Room_ID, Domain_ID, [__Secure_Check-in], Closed) VALUES
(@eventId, @groupIdSB, null   , 1        , null               , null  )

SET @eventFlag = @eventFlag+1;
END

SET @eventflag = 1;
SET @eventDate = DATEADD(DAY, -7,GETDATE());

WHILE (@eventflag <=6)
BEGIN
SET IDENTITY_INSERT [dbo].[Events] ON;

SET @eventDate = DATEADD(DAY, 7, @eventDate);
SET @eventId = (SELECT MAX(Event_ID) FROM Events) + 1 ;
SET @eventStartDate1 = (CAST(CAST(@eventDate AS DATE) AS DATETIME) + CAST('3:00' AS TIME)) ;
SET @eventEndDate1 = (CAST(CAST(@eventDate AS DATE) AS DATETIME) + CAST('4:00' AS TIME));

INSERT INTO Events
(Event_ID , Event_Title                  , Event_Type_ID  , Congregation_ID, Location_ID, Meeting_Instructions, Description, Program_ID, Primary_Contact, Participants_Expected, Minutes_for_Setup, Event_Start_Date           , Event_End_Date, Minutes_for_Cleanup, Cancelled, _Approved, Public_Website_Settings, Visibility_Level_ID, Featured_On_Calendar, Online_Registration_Product, Registration_Form, Registration_Start, Registration_End, Registration_Active, _Web_Approved, [Check-in_Information], [Allow_Check-in], Ignore_Program_Groups, Prohibit_Guests, [Early_Check-in_Period], [Late_Check-in_Period], Other_Event_Information, Parent_Event_ID, Priority_ID, Domain_ID, On_Connection_Card, External_Registration_URL, On_Donation_Batch_Tool, __ExternalEventID, __ExternalOrganizerUserID, __ExternalGroupID, __ExternalRoomID, __ExternalContactUserID, Project_Code, Participant_Reminder_Settings, Send_Reminder, Reminder_Sent, Reminder_Days_Prior_ID, __ExternalTripID, __ExternalTripLegID ) VALUES
( @eventId, '(t) Superbowl Oakley 3:00 ' , @eventTypeIdSB2, 1              , 3          , null                , null       , 109       , 2562428        , null                 , 0                , @eventStartDate1           , @eventEndDate1, 0                  , 0        , 1        , null                   , 4                  , 0                   , null                       , null             , null              , null            , null               , 1            , null                  , 0               , 0                    , 0              , null                   , null                  , null                   , null           , null       , 1        , null              , null                     , 0                     , null             , null                     , null             , null            , null                   , null        , null                         , 1            , null         , 2                     , null            , null );   

SET IDENTITY_INSERT [dbo].[Events] OFF;

INSERT INTO Event_Groups
(Event_ID, Group_ID, Room_ID, Domain_ID, [__Secure_Check-in], Closed) VALUES
(@eventId, @groupIdSB, null   , 1        , null               , null  );

SET @eventFlag = @eventFlag+1;
END

SET @eventflag = 1;
SET @eventDate = DATEADD(DAY, -7,GETDATE());

WHILE (@eventflag <=6)
BEGIN
SET IDENTITY_INSERT [dbo].[Events] ON;

SET @eventDate = DATEADD(DAY, 7, @eventDate);
SET @eventId = (SELECT MAX(Event_ID) FROM Events) + 1 ;
SET @eventStartDate1 = (CAST(CAST(@eventDate AS DATE) AS DATETIME) + CAST('11:00' AS TIME)) ;
SET @eventEndDate1 = (CAST(CAST(@eventDate AS DATE) AS DATETIME) + CAST('12:00' AS TIME));

INSERT INTO Events
(Event_ID , Event_Title                  , Event_Type_ID  , Congregation_ID, Location_ID, Meeting_Instructions, Description, Program_ID, Primary_Contact, Participants_Expected, Minutes_for_Setup, Event_Start_Date           , Event_End_Date, Minutes_for_Cleanup, Cancelled, _Approved, Public_Website_Settings, Visibility_Level_ID, Featured_On_Calendar, Online_Registration_Product, Registration_Form, Registration_Start, Registration_End, Registration_Active, _Web_Approved, [Check-in_Information], [Allow_Check-in], Ignore_Program_Groups, Prohibit_Guests, [Early_Check-in_Period], [Late_Check-in_Period], Other_Event_Information, Parent_Event_ID, Priority_ID, Domain_ID, On_Connection_Card, External_Registration_URL, On_Donation_Batch_Tool, __ExternalEventID, __ExternalOrganizerUserID, __ExternalGroupID, __ExternalRoomID, __ExternalContactUserID, Project_Code, Participant_Reminder_Settings, Send_Reminder, Reminder_Sent, Reminder_Days_Prior_ID, __ExternalTripID, __ExternalTripLegID ) VALUES
( @eventId, '(t) Kids Club Oakley 11:00 ', @eventTypeIdKC1, 1              , 3          , null                , null       , 109       , 2562428        , null                 , 0                , @eventStartDate1           , @eventEndDate1, 0                  , 0        , 1        , null                   , 4                  , 0                   , null                       , null             , null              , null            , null               , 1            , null                  , 0               , 0                    , 0              , null                   , null                  , null                   , null           , null       , 1        , null              , null                     , 0                     , null             , null                     , null             , null            , null                   , null        , null                         , 1            , null         , 2                     , null            , null );   

SET IDENTITY_INSERT [dbo].[Events] OFF;

INSERT INTO Event_Groups
(Event_ID, Group_ID, Room_ID, Domain_ID, [__Secure_Check-in], Closed) VALUES
(@eventId, @groupIdKC, null   , 1        , null               , null  );

SET @eventFlag = @eventFlag+1;
END

SET @eventflag = 1;
SET @eventDate = DATEADD(DAY, -7,GETDATE());

WHILE (@eventflag <=6)
BEGIN
SET IDENTITY_INSERT [dbo].[Events] ON;


SET @eventDate = DATEADD(DAY, 7, @eventDate);
SET @eventId = (SELECT MAX(Event_ID) FROM Events) + 1 ;
SET @eventStartDate1 = (CAST(CAST(@eventDate AS DATE) AS DATETIME) + CAST('1:00' AS TIME)) ;
SET @eventEndDate1 = (CAST(CAST(@eventDate AS DATE) AS DATETIME) + CAST('3:00' AS TIME));

INSERT INTO Events
(Event_ID , Event_Title                  , Event_Type_ID  , Congregation_ID, Location_ID, Meeting_Instructions, Description, Program_ID, Primary_Contact, Participants_Expected, Minutes_for_Setup, Event_Start_Date           , Event_End_Date, Minutes_for_Cleanup, Cancelled, _Approved, Public_Website_Settings, Visibility_Level_ID, Featured_On_Calendar, Online_Registration_Product, Registration_Form, Registration_Start, Registration_End, Registration_Active, _Web_Approved, [Check-in_Information], [Allow_Check-in], Ignore_Program_Groups, Prohibit_Guests, [Early_Check-in_Period], [Late_Check-in_Period], Other_Event_Information, Parent_Event_ID, Priority_ID, Domain_ID, On_Connection_Card, External_Registration_URL, On_Donation_Batch_Tool, __ExternalEventID, __ExternalOrganizerUserID, __ExternalGroupID, __ExternalRoomID, __ExternalContactUserID, Project_Code, Participant_Reminder_Settings, Send_Reminder, Reminder_Sent, Reminder_Days_Prior_ID, __ExternalTripID, __ExternalTripLegID ) VALUES
( @eventId, '(t) Kids Club Oakley 1:00 ' , @eventTypeIdKC1, 1              , 3          , null                , null       , 109       , 2562428        , null                 , 0                , @eventStartDate1           , @eventEndDate1, 0                  , 0        , 1        , null                   , 4                  , 0                   , null                       , null             , null              , null            , null               , 1            , null                  , 0               , 0                    , 0              , null                   , null                  , null                   , null           , null       , 1        , null              , null                     , 0                     , null             , null                     , null             , null            , null                   , null        , null                         , 1            , null         , 2                     , null            , null );   

SET IDENTITY_INSERT [dbo].[Events] OFF;

INSERT INTO Event_Groups
(Event_ID, Group_ID, Room_ID, Domain_ID, [__Secure_Check-in], Closed) VALUES
(@eventId, @groupIdKC, null   , 1        , null               , null  );

SET @eventFlag = @eventFlag+1;
END

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


--Create a new Sign Up to Serve Group
SET IDENTITY_INSERT [dbo].[Groups] ON;

SET @groupIdSB = (SELECT IDENT_CURRENT('Groups')) + 1 ;

INSERT INTO Groups
(Group_ID, Group_Name                     , Group_Type_ID, Ministry_ID, Congregation_ID, Primary_Contact, Description, Start_Date      , End_Date, Target_Size, Parent_Group, Priority_ID, Enable_Waiting_List, Small_Group_Information, Offsite_Meeting_Address, Group_Is_Full, Available_Online, Life_Stage_ID, Group_Focus_ID, Meeting_Time, Meeting_Day_ID, Descended_From, Reason_Ended, Domain_ID, Check_in_Information, [Secure_Check-in], Suppress_Care_Note, On_Classroom_Manager, Promotion_Information, Promote_to_Group, Age_in_Months_to_Promote , Promote_Weekly , __ExternalGroupID , __ExternalParentGroupID , __IsPublic , __ISBlogEnabled , __ISWebEnabled , Group_Notes , Sign_Up_To_Serve , Deadline_Passed_Message_ID , Notifications , Send_Attendance_Notification , Send_Service_Notification , Child_Care_Available , Meeting_Frequency_ID , Meeting_Duration_ID , Required_Book ) VALUES
(@groupIdSB, '(t) Superbowl Oakley Group' , 1            , 8          ,  1             , 2562428        , null       , {d '2015-11-01'}, null    , null       , null        , null       , null               , null                   , null                   , 0            , 1               , null         , null          , null        , null          , null          , null        , 1        , null                , null             , null              , null                , null                 , null            , null                     , null           , null              , null                    , null       , null            , null           , null        , null             , 58                         , null          , 0                            , 0                         , 0                    , null                 , null                , null) ;

SET @groupIdKC = (SELECT IDENT_CURRENT('Groups')) + 1 ;

INSERT INTO Groups
(Group_ID, Group_Name                     , Group_Type_ID, Ministry_ID, Congregation_ID, Primary_Contact, Description, Start_Date      , End_Date, Target_Size, Parent_Group, Priority_ID, Enable_Waiting_List, Small_Group_Information, Offsite_Meeting_Address, Group_Is_Full, Available_Online, Life_Stage_ID, Group_Focus_ID, Meeting_Time, Meeting_Day_ID, Descended_From, Reason_Ended, Domain_ID, Check_in_Information, [Secure_Check-in], Suppress_Care_Note, On_Classroom_Manager, Promotion_Information, Promote_to_Group, Age_in_Months_to_Promote , Promote_Weekly , __ExternalGroupID , __ExternalParentGroupID , __IsPublic , __ISBlogEnabled , __ISWebEnabled , Group_Notes , Sign_Up_To_Serve , Deadline_Passed_Message_ID , Notifications , Send_Attendance_Notification , Send_Service_Notification , Child_Care_Available , Meeting_Frequency_ID , Meeting_Duration_ID , Required_Book ) VALUES
(@groupIdKC, '(t) KidsClub Oakley Group'  , 9            , 2          ,  1             , 2562428        , null       , {d '2015-11-01'}, null    , null       , null        , null       , null               , null                   , null                   , 0            , 1               , null         , null          , null        , null          , null          , null        , 1        , null                , null             , null              , null                , null                 , null            , null                     , null           , null              , null                    , null       , null            , null           , null        , null             , 58                         , null          , 0                            , 0                         , 0                    , null                 , null                , null) ;

SET IDENTITY_INSERT [dbo].[Groups] OFF;

--Create new Sign Up to Serve Evet Types
SET IDENTITY_INSERT [dbo].[Event_Types] ON;

SET @eventTypeIdSB1 = (SELECT MAX(Event_Type_ID) FROM Event_Types) + 1 ;

INSERT INTO Event_Types
(Event_Type_ID, Event_Type                       , Description, Domain_ID , __ExternalEventTypeID , __ExternalSiteServiceTimeID ) VALUES
(@eventTypeIdSB1 , '(t) Superbowl Oakley daily 10:00', null       , 1          ,  null                , null) ;

SET @eventTypeIdSB2 = (SELECT MAX(Event_Type_ID) FROM Event_Types) + 1 ;

INSERT INTO Event_Types
(Event_Type_ID, Event_Type                           , Description, Domain_ID  , __ExternalEventTypeID , __ExternalSiteServiceTimeID ) VALUES
(@eventTypeIdSB2 , '(t) Superbowl Oakley daily 3:00' , null       , 1          ,  null                 , null) ;

SET @eventTypeIdKC1 = (SELECT MAX(Event_Type_ID) FROM Event_Types) + 1 ;

INSERT INTO Event_Types
(Event_Type_ID, Event_Type                             , Description, Domain_ID , __ExternalEventTypeID , __ExternalSiteServiceTimeID ) VALUES
(@eventTypeIdKC1, '(t) KC Nursery Oakley weekly 11:00' , null       , 1          ,  null                , null) ;

SET @eventTypeIdKC2 = (SELECT MAX(Event_Type_ID) FROM Event_Types) + 1 ;

INSERT INTO Event_Types
(Event_Type_ID, Event_Type                            , Description, Domain_ID  , __ExternalEventTypeID , __ExternalSiteServiceTimeID ) VALUES
(@eventTypeIdKC2, '(t) KC Nursery Oakley weekly 1:00' , null       , 1          ,  null                 , null) ;

SET IDENTITY_INSERT [dbo].[Event_Types] OFF;

--Create new Sign Up to Serve Opportunities

SET IDENTITY_INSERT [dbo].[Opportunities] ON;

SET @opportunityIdKC1 = (SELECT MAX(Opportunity_ID) FROM Opportunities) + 1 ;

INSERT INTO Opportunities
(Opportunity_ID   , Opportunity_Title    ,Description, Group_Role_ID, Program_ID, Visibility_Level_ID, Contact_Person, Publish_Date    , Opportunity_Date, Duration_in_Hours, Add_to_Group, Add_to_Event, Required_Gender, Minimum_Age, Minimum_Needed, Maximum_Needed, Domain_ID, Letter_Body, Letter_From, On_Connection_Card, Shift_Start       , Shift_End         , Custom_Form, Event_Type_ID, Sign_Up_Deadline_ID, Room, __ExternalCalendarEventServingTimeID, __ExternalRecurrenceSubsetCode) VALUES
(@opportunityIdKC1, '(t) KC Oakley 11:00',null       , 22           , 109       , 4                  , 2562428       , {d '2015-11-01'}, null            , null             , @groupIdKC   , null        , null           , null       , 1             , 1             , 1        , null       , null       , null              , '10:45:00.1234567', '12:15:00.1234567', null       , @eventTypeIdKC1, 3                  , null, null                                , null     );                             

SET @opportunityIdKC2 = (SELECT MAX(Opportunity_ID) FROM Opportunities) + 1 ;

INSERT INTO Opportunities
(Opportunity_ID   , Opportunity_Title    ,Description, Group_Role_ID, Program_ID, Visibility_Level_ID, Contact_Person, Publish_Date    , Opportunity_Date, Duration_in_Hours, Add_to_Group, Add_to_Event, Required_Gender, Minimum_Age, Minimum_Needed, Maximum_Needed, Domain_ID, Letter_Body, Letter_From, On_Connection_Card, Shift_Start       , Shift_End         , Custom_Form, Event_Type_ID, Sign_Up_Deadline_ID, Room, __ExternalCalendarEventServingTimeID, __ExternalRecurrenceSubsetCode) VALUES
(@opportunityIdKC2, '(t) KC Oakley 1:00' ,null       , 16           , 109       , 4                  , 2562428       , {d '2015-11-01'}, null            , null             , @groupIdKC   , null        , null           , null       , 5             , 10             , 1        , null       , null       , null              , '12:45:00.1234567', '15:15:00.1234567', null       , @eventTypeIdKC1, 3                  , null, null                                , null     );                             

SET @opportunityIdKC3 = (SELECT MAX(Opportunity_ID) FROM Opportunities) + 1 ;

INSERT INTO Opportunities
(Opportunity_ID   , Opportunity_Title    ,Description, Group_Role_ID, Program_ID, Visibility_Level_ID, Contact_Person, Publish_Date    , Opportunity_Date, Duration_in_Hours, Add_to_Group, Add_to_Event, Required_Gender, Minimum_Age, Minimum_Needed, Maximum_Needed, Domain_ID, Letter_Body, Letter_From, On_Connection_Card, Shift_Start       , Shift_End         , Custom_Form, Event_Type_ID, Sign_Up_Deadline_ID, Room, __ExternalCalendarEventServingTimeID, __ExternalRecurrenceSubsetCode) VALUES
(@opportunityIdKC3, '(t) KC Oakley 11:00',null       , 22           , 109       , 4                  , 2562428       , {d '2015-11-01'}, null            , null             , @groupIdKC    , null        , null           , null       , 1            , 1             , 1        , null       , null       , null              , '10:45:00.1234567', '12:15:00.1234567', null       , @eventTypeIdKC2, 3                  , null, null                                , null     );                             

SET @opportunityIdKC4 = (SELECT MAX(Opportunity_ID) FROM Opportunities) + 1 ;

INSERT INTO Opportunities
(Opportunity_ID   , Opportunity_Title     ,Description, Group_Role_ID, Program_ID, Visibility_Level_ID, Contact_Person, Publish_Date    , Opportunity_Date, Duration_in_Hours, Add_to_Group, Add_to_Event, Required_Gender, Minimum_Age, Minimum_Needed, Maximum_Needed, Domain_ID, Letter_Body, Letter_From, On_Connection_Card, Shift_Start       , Shift_End         , Custom_Form, Event_Type_ID, Sign_Up_Deadline_ID, Room, __ExternalCalendarEventServingTimeID, __ExternalRecurrenceSubsetCode) VALUES
(@opportunityIdKC4, '(t) KC Oakley 1:00'  ,null       , 16           , 109       , 4                  , 2562428       , {d '2015-11-01'}, null            , null             , @groupIdKC    , null        , null           , null       , 5             , 10            , 1        , null       , null       , null             ,'12:45:00.1234567', '15:15:00.1234567', null     , @eventTypeIdKC2, 3                  , null, null                                , null     );                             

SET @opportunityIdSB1 = (SELECT MAX(Opportunity_ID) FROM Opportunities) + 1 ;

INSERT INTO Opportunities
(Opportunity_ID   , Opportunity_Title           ,Description, Group_Role_ID, Program_ID, Visibility_Level_ID, Contact_Person, Publish_Date    , Opportunity_Date, Duration_in_Hours, Add_to_Group, Add_to_Event, Required_Gender, Minimum_Age, Minimum_Needed, Maximum_Needed, Domain_ID, Letter_Body, Letter_From, On_Connection_Card, Shift_Start       , Shift_End         , Custom_Form, Event_Type_ID, Sign_Up_Deadline_ID, Room, __ExternalCalendarEventServingTimeID, __ExternalRecurrenceSubsetCode) VALUES
(@opportunityIdSB1, '(t) Superbowl Oakley 10:00',null       , 22           , 111       , 4                  , 2562428       , {d '2015-11-01'}, null            , null             , @groupIdSB   , null        , null           , null       , 1             , 1             , 1        , null       , null       , null             , '9:45:00.1234567', '12:15:00.1234567', null      , @eventTypeIdSB1, 3                  , null, null                                , null     );                             

SET @opportunityIdSB2 = (SELECT MAX(Opportunity_ID) FROM Opportunities) + 1 ;

INSERT INTO Opportunities
(Opportunity_ID   , Opportunity_Title           ,Description, Group_Role_ID, Program_ID, Visibility_Level_ID, Contact_Person, Publish_Date    , Opportunity_Date, Duration_in_Hours, Add_to_Group, Add_to_Event, Required_Gender, Minimum_Age, Minimum_Needed, Maximum_Needed, Domain_ID, Letter_Body, Letter_From, On_Connection_Card, Shift_Start       , Shift_End         , Custom_Form, Event_Type_ID, Sign_Up_Deadline_ID, Room, __ExternalCalendarEventServingTimeID, __ExternalRecurrenceSubsetCode) VALUES
(@opportunityIdSB2, '(t) Superbowl Oakley 3:00' ,null       , 16           , 111       , 4                  , 2562428       , {d '2015-11-01'}, null            , null             , @groupIdSB   , null        , null           , null       , 5             , 10             , 1        , null       , null       , null              , '14:45:00.1234567', '16:15:00.1234567', null     , @eventTypeIdSB1, 3                  , null, null                                , null     );                             

SET @opportunityIdSB3 = (SELECT MAX(Opportunity_ID) FROM Opportunities) + 1 ;

INSERT INTO Opportunities
(Opportunity_ID   , Opportunity_Title           ,Description, Group_Role_ID, Program_ID, Visibility_Level_ID, Contact_Person, Publish_Date    , Opportunity_Date, Duration_in_Hours, Add_to_Group, Add_to_Event, Required_Gender, Minimum_Age, Minimum_Needed, Maximum_Needed, Domain_ID, Letter_Body, Letter_From, On_Connection_Card, Shift_Start       , Shift_End         , Custom_Form, Event_Type_ID, Sign_Up_Deadline_ID, Room, __ExternalCalendarEventServingTimeID, __ExternalRecurrenceSubsetCode) VALUES
(@opportunityIdSB3, '(t) Superbowl Oakley 10:00',null       , 22           , 111       , 4                  , 2562428       , {d '2015-11-01'}, null            , null             , @groupIdSB    , null        , null           , null       , 1            , 1             , 1        , null       , null       , null              , '9:45:00.1234567', '15:15:00.1234567', null      , @eventTypeIdSB2, 3                  , null, null                                , null     );                             

SET @opportunityIdSB4 = (SELECT MAX(Opportunity_ID) FROM Opportunities) + 1 ;

INSERT INTO Opportunities
(Opportunity_ID   , Opportunity_Title             ,Description, Group_Role_ID, Program_ID, Visibility_Level_ID, Contact_Person, Publish_Date    , Opportunity_Date, Duration_in_Hours, Add_to_Group, Add_to_Event, Required_Gender, Minimum_Age, Minimum_Needed, Maximum_Needed, Domain_ID, Letter_Body, Letter_From, On_Connection_Card, Shift_Start       , Shift_End         , Custom_Form, Event_Type_ID, Sign_Up_Deadline_ID, Room, __ExternalCalendarEventServingTimeID, __ExternalRecurrenceSubsetCode) VALUES
(@opportunityIdSB4, '(t) Superbowl Oakley 3:00'   ,null       , 16           , 111       , 4                  , 2562428       , {d '2015-11-01'}, null            , null             , @groupIdSB    , null        , null           , null       , 5             , 10            , 1        , null       , null       , null              , '14:45:00.1234567', '16:15:00.1234567', null     , @eventTypeIdSB2, 3                  , null, null                                , null     );                             
 
SET IDENTITY_INSERT [dbo].[Opportunities] OFF;

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

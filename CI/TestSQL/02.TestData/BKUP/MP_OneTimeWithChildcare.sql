Use [MinistryPlatform]
GO

SET IDENTITY_INSERT [dbo].[Events] OFF;

DECLARE @groupId AS INT
DECLARE @eventId AS INT
DECLARE @eventTypeId AS INT


SET IDENTITY_INSERT [dbo].[Groups] ON;

--Store the current identity value so we can reset it.
SET @groupId = IDENT_CURRENT('Groups');

INSERT INTO Groups
(Group_ID , Group_Name                            , Group_Type_ID, Ministry_ID, Congregation_ID, Primary_Contact, Description, Start_Date      , End_Date, Target_Size, Parent_Group, Priority_ID, Enable_Waiting_List, Small_Group_Information, Offsite_Meeting_Address, Group_Is_Full, Available_Online, Life_Stage_ID, Group_Focus_ID, Meeting_Time, Meeting_Day_ID, Descended_From, Reason_Ended, Domain_ID, Check_in_Information, [Secure_Check-in], Suppress_Care_Note, On_Classroom_Manager, Promotion_Information, Promote_to_Group, Age_in_Months_to_Promote , Promote_Weekly , __ExternalGroupID , __ExternalParentGroupID , __IsPublic , __ISBlogEnabled , __ISWebEnabled , Group_Notes , Sign_Up_To_Serve , Deadline_Passed_Message_ID , Notifications , Send_Attendance_Notification , Send_Service_Notification , Child_Care_Available , Meeting_Frequency_ID , Meeting_Duration_ID , Required_Book ) VALUES
(10000000, '(t) 1Time Mason Group with ChildCare', 8            , 2          ,  6             , 2186211        , null       , {d '2015-11-01'}, null    , null       , null        , null       , null               , null                   , null                   , 0            , 1               , null         , null          , null        , null          , null          , null        , 1        , null                , null             , null              , null                , null                 , null            , null                     , null           , null              , null                    , null       , null            , null           , null        , null             , 58                         , null          , 0                            , 0                         , 1                    , null                 , null                , null) ;

SET IDENTITY_INSERT [dbo].[Groups] OFF;

--This command resets the identity value so that if someone adds groups through the UI it won't use a big ID. 
DBCC CHECKIDENT (Groups, reseed, @groupID);

SET IDENTITY_INSERT [dbo].[Events] ON;

SET @eventId = (SELECT MAX(Event_ID) FROM Events) + 1 ;

DISABLE TRIGGER [dbo].[crds_tr_create_childcare_events] 
ON  Event_Groups; 

INSERT INTO Events
(Event_ID , Event_Title                     , Event_Type_ID, Congregation_ID, Location_ID, Meeting_Instructions, Description, Program_ID, Primary_Contact, Participants_Expected, Minutes_for_Setup, Event_Start_Date          , Event_End_Date            , Minutes_for_Cleanup, Cancelled, _Approved, Public_Website_Settings, Visibility_Level_ID, Featured_On_Calendar, Online_Registration_Product, Registration_Form, Registration_Start, Registration_End, Registration_Active, _Web_Approved, [Check-in_Information], [Allow_Check-in], Ignore_Program_Groups, Prohibit_Guests, [Early_Check-in_Period], [Late_Check-in_Period], Other_Event_Information, Parent_Event_ID, Priority_ID, Domain_ID, On_Connection_Card, External_Registration_URL, On_Donation_Batch_Tool, __ExternalEventID, __ExternalOrganizerUserID, __ExternalGroupID, __ExternalRoomID, __ExternalContactUserID, Project_Code, Send_Reminder, Reminder_Sent, Reminder_Days_Prior_ID, __ExternalTripID, __ExternalTripLegID ) VALUES
( @eventId, '(t) 1Time Mason with ChildCare', 80           , 6              , 5          , null                , null       , 109       , 2186211        , null                 , 0                , DATEADD(DAY, 14,GETDATE()), DATEADD(DAY, 17,GETDATE()), 0                  , 0        , 1        , null                   , 4                  , 0                   , null                       , null             , null              , null            , null               , 1            , null                  , 0               , 0                    , 0              , null                   , null                  , null                   , null           , null       , 1        , null              , null                     , 0                     , null             , null                     , null             , null            , null                   , null        , 1            , null         , 2                     , null            , null );   

SET IDENTITY_INSERT [dbo].[Events] OFF;

INSERT INTO Event_Groups
(Event_ID, Group_ID, Room_ID, Domain_ID, [__Secure_Check-in], Closed) VALUES
(@eventId, 10000000, null   , 1        , null               , null  );

SET IDENTITY_INSERT [dbo].[Events] ON;

SET @eventId = (SELECT MAX(Event_ID) FROM Events) + 1 ;

INSERT INTO Events
(Event_ID , Event_Title                                 , Event_Type_ID, Congregation_ID, Location_ID, Meeting_Instructions, Description, Program_ID, Primary_Contact, Participants_Expected, Minutes_for_Setup, Event_Start_Date          , Event_End_Date            , Minutes_for_Cleanup, Cancelled, _Approved, Public_Website_Settings, Visibility_Level_ID, Featured_On_Calendar, Online_Registration_Product, Registration_Form, Registration_Start, Registration_End, Registration_Active, _Web_Approved, [Check-in_Information], [Allow_Check-in], Ignore_Program_Groups, Prohibit_Guests, [Early_Check-in_Period], [Late_Check-in_Period], Other_Event_Information, Parent_Event_ID, Priority_ID, Domain_ID, On_Connection_Card, External_Registration_URL, On_Donation_Batch_Tool, __ExternalEventID, __ExternalOrganizerUserID, __ExternalGroupID, __ExternalRoomID, __ExternalContactUserID, Project_Code, Send_Reminder, Reminder_Sent, Reminder_Days_Prior_ID, __ExternalTripID, __ExternalTripLegID ) VALUES
( @eventId, '(t) 1Time Mason with ChildCare - Childcare', 243          , 6              , 5          , null                , null       , 109       , 2186211        , null                 , 0                , DATEADD(DAY, 14,GETDATE()), DATEADD(DAY, 17,GETDATE()), 0                  , 0        , 1        , null                   , 4                  , 0                   , null                       , null             , null              , null            , null               , 1            , null                  , 0               , 0                    , 0              , null                   , null                  , null                   , null           , null       , 1        , null              , null                     , 0                     , null             , null                     , null             , null            , null                   , null        , 1            , null         , 2                     , null            , null );   

SET IDENTITY_INSERT [dbo].[Events] OFF;

INSERT INTO Event_Groups
(Event_ID, Group_ID, Room_ID, Domain_ID, [__Secure_Check-in], Closed) VALUES
(@eventId, 10000000, null   , 1        , null               , null  );

ENABLE TRIGGER [dbo].[crds_tr_create_childcare_events] 
ON  Event_Groups; 

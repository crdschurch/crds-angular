USE [MinistryPlatform]
GO

update dp_page_views
set Field_List = 'Event_Rooms.[Event_Room_ID] 
, Event_ID_Table.[Event_ID] 
, Room_ID_Table.[Room_ID] 
, Event_Rooms.[Notes] 
, Room_Layout_ID_Table.[Room_Layout_ID] 
, Event_Rooms.[Hidden] 
, Event_Rooms.[Cancelled] , Room_ID_Table.[Room_Name], Event_Rooms.[Label], Event_Rooms.[Capacity], Event_Rooms.[Allow_Checkin], Event_Rooms.[Volunteers]
, Event_Rooms.[_Approved]
, (SELECT CAST(COUNT(*) AS BIT) from dp_Tasks WHERE _record_id = Event_Rooms.[Event_Room_ID] AND _Rejected=1 AND _page_id = 384) AS Rejected'
where page_view_id = 1108
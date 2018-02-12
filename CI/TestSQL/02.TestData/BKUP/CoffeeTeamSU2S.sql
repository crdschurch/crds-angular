USE [MinistryPlatform]
GO

INSERT INTO Groups 
(Group_Name                 ,Group_Type_ID,Ministry_ID ,Congregation_ID,Primary_Contact,Description,Start_Date                ,Target_Size,Group_Is_Full,Available_Online,Domain_ID,[Secure_Check-in],Suppress_Nametag,Suppress_Care_Note,On_Classroom_Manager,Promote_Weekly,__IsPublic,__ISBlogEnabled,__ISWebEnabled,Group_Notes,Sign_Up_To_Serve,Deadline_Passed_Message_ID,Send_Attendance_Notification,Send_Service_Notification,Child_Care_Available,Kids_Welcome) VALUES
('(t) FI Oakley Coffee Team',9            ,11          ,1              ,7654100        ,'Test data',{ts '2004-09-21 00:00:00'},0          ,0            ,1               ,1        ,0                ,0               ,0                 ,0                   ,0             ,'N'       ,'Y'            ,'Y'           ,''         ,null            ,null                      ,0                           ,0                        ,0                   ,0           );

--Saturday 4:30
--Setup
INSERT INTO Opportunities 
(Opportunity_Title          ,Description,Group_Role_ID,Program_ID,Visibility_Level_ID,Contact_Person,Publish_Date              ,Add_to_Group                                                                ,Minimum_Needed,Maximum_Needed,[Domain_ID],On_Connection_Card,Shift_Start   ,Shift_End     ,Event_Type_ID,Sign_Up_Deadline_ID,Room           ,Send_Reminder,Reminder_Template,Reminder_Days_Prior) VALUES
('(t) Coffee Setup Sat 2:00','Test Data',16           ,106       ,4                  ,7654100       ,{ts '2015-10-27 14:55:00'},(select group_id from Groups where Group_Name = '(t) FI Oakley Coffee Team'),1             ,1             ,1          ,0                 ,{t '14:00:00'},{t '16:00:00'},94           ,7                  ,'Oakley Atrium',1            ,108700           ,3                  );

--Coffee Brewing
INSERT INTO Opportunities 
(Opportunity_Title         ,Description,Group_Role_ID,Program_ID,Visibility_Level_ID,Contact_Person,Publish_Date              ,Add_to_Group                                                                ,Minimum_Needed,Maximum_Needed,[Domain_ID],On_Connection_Card,Shift_Start   ,Shift_End     ,Event_Type_ID,Sign_Up_Deadline_ID,Room                       ,Send_Reminder,Reminder_Template,Reminder_Days_Prior) VALUES
('(t) Coffee Team Sat 4:00','Test Data',16           ,106       ,4                  ,7654100       ,{ts '2015-10-27 14:55:00'},(select group_id from Groups where Group_Name = '(t) FI Oakley Coffee Team'),1             ,1             ,1          ,0                 ,{t '16:00:00'},{t '18:00:00'},94           ,7                  ,'Main Coffee Station Alpha',1            ,108700           ,3                  );


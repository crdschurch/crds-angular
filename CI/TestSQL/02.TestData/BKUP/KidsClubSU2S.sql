USE [MinistryPlatform]
GO

INSERT INTO Groups 
(Group_Name                  ,Group_Type_ID,Ministry_ID,Congregation_ID,Primary_Contact,Description,Start_Date                ,Target_Size,Parent_Group,Group_Is_Full,Available_Online,Domain_ID,[Secure_Check-in],Suppress_Nametag,Suppress_Care_Note,On_Classroom_Manager,Promote_Weekly,__IsPublic,__ISBlogEnabled,__ISWebEnabled,Group_Notes,Sign_Up_To_Serve,Deadline_Passed_Message_ID,Send_Attendance_Notification,Send_Service_Notification,Child_Care_Available,Kids_Welcome) VALUES
('(t) KC Oakley Kindergarten',9            ,2          ,1              ,7680240        ,'Test data',{ts '2004-09-21 00:00:00'},0          ,166602      ,0            ,1               ,1        ,0                ,0               ,0                 ,0                   ,0             ,'N'       ,'Y'            ,'Y'           ,''         ,null            ,58                        ,0                           ,0                        ,0                   ,0           );

--Sunday 11:45 Kindergarten
INSERT INTO Opportunities 
(Opportunity_Title                ,Description,Group_Role_ID,Program_ID,Visibility_Level_ID,Contact_Person,Publish_Date              ,Add_to_Group                                                                 ,Minimum_Needed,Maximum_Needed,[Domain_ID],On_Connection_Card,Shift_Start   ,Shift_End     ,Event_Type_ID,Sign_Up_Deadline_ID,Room             ,Send_Reminder,Reminder_Template,Reminder_Days_Prior) VALUES
('(t) Kindergarten K213 Sun 11:45','Test Data',16           ,83        ,4                  ,7680240       ,{ts '2015-10-27 14:55:00'},(select group_id from Groups where Group_Name = '(t) KC Oakley Kindergarten'),3             ,10            ,1          ,0                 ,{t '11:20:00'},{t '13:20:00'},97           ,7                  ,'OAK KC Room 213',1            ,108700           ,3                  );

--Sunday 10:05 Kindergarten
INSERT INTO Opportunities 
(Opportunity_Title                ,Description,Group_Role_ID,Program_ID,Visibility_Level_ID,Contact_Person,Publish_Date              ,Add_to_Group                                                                 ,Minimum_Needed,Maximum_Needed,[Domain_ID],On_Connection_Card,Shift_Start   ,Shift_End     ,Event_Type_ID,Sign_Up_Deadline_ID,Room             ,Send_Reminder,Reminder_Template,Reminder_Days_Prior) VALUES
('(t) Kindergarten K213 Sun 10:05','Test Data',16           ,83        ,4                  ,7680240       ,{ts '2015-10-27 14:55:00'},(select group_id from Groups where Group_Name = '(t) KC Oakley Kindergarten'),3             ,10            ,1          ,0                 ,{t '09:40:00'},{t '11:20:00'},96           ,7                  ,'OAK KC Room 213',1            ,108700           ,3                  );

--Sunday 8:30
INSERT INTO Opportunities 
(Opportunity_Title               ,Description,Group_Role_ID,Program_ID,Visibility_Level_ID,Contact_Person,Publish_Date              ,Add_to_Group                                                                 ,Minimum_Needed,Maximum_Needed,[Domain_ID],On_Connection_Card,Shift_Start   ,Shift_End     ,Event_Type_ID,Sign_Up_Deadline_ID,Room             ,Send_Reminder,Reminder_Template,Reminder_Days_Prior) VALUES
('(t) Kindergarten K213 Sun 8:30','Test Data',16           ,83        ,4                  ,7680240       ,{ts '2015-10-27 14:55:00'},(select group_id from Groups where Group_Name = '(t) KC Oakley Kindergarten'),3             ,10            ,1          ,0                 ,{t '08:05:00'},{t '09:50:00'},95           ,7                  ,'OAK KC Room 213',1            ,108700           ,3                  );

--Saturday 4:30
INSERT INTO Opportunities 
(Opportunity_Title               ,Description,Group_Role_ID,Program_ID,Visibility_Level_ID,Contact_Person,Publish_Date              ,Add_to_Group                                                                 ,Minimum_Needed,Maximum_Needed,[Domain_ID],On_Connection_Card,Shift_Start   ,Shift_End     ,Event_Type_ID,Sign_Up_Deadline_ID,Room             ,Send_Reminder,Reminder_Template,Reminder_Days_Prior) VALUES
('(t) Kindergarten K213 Sat 4:30','Test Data',16           ,83        ,4                  ,7680240       ,{ts '2015-10-27 14:55:00'},(select group_id from Groups where Group_Name = '(t) KC Oakley Kindergarten'),3             ,10            ,1          ,0                 ,{t '16:05:00'},{t '17:50:00'},94           ,7                  ,'OAK KC Room 213',1            ,108700           ,3                  );

--Saturday 6:15
INSERT INTO Opportunities 
(Opportunity_Title               ,Description,Group_Role_ID,Program_ID,Visibility_Level_ID,Contact_Person,Publish_Date              ,Add_to_Group                                                                 ,Minimum_Needed,Maximum_Needed,[Domain_ID],On_Connection_Card,Shift_Start   ,Shift_End     ,Event_Type_ID,Sign_Up_Deadline_ID,Room             ,Send_Reminder,Reminder_Template,Reminder_Days_Prior) VALUES
('(t) Kindergarten K213 Sat 6:15','Test Data',16           ,83        ,4                  ,7680240       ,{ts '2015-10-27 14:55:00'},(select group_id from Groups where Group_Name = '(t) KC Oakley Kindergarten'),3             ,10            ,1          ,0                 ,{t '17:50:00'},{t '19:30:00'},101           ,7                  ,'OAK KC Room 213',1            ,108700           ,3                  );

--Nursery Group
INSERT INTO Groups 
(Group_Name             ,Group_Type_ID,Ministry_ID,Congregation_ID,Primary_Contact,Description,Start_Date                ,Target_Size,Parent_Group,Group_Is_Full,Available_Online,Domain_ID,[Secure_Check-in],Suppress_Nametag,Suppress_Care_Note,On_Classroom_Manager,Promote_Weekly,__IsPublic,__ISBlogEnabled,__ISWebEnabled,Group_Notes,Sign_Up_To_Serve,Deadline_Passed_Message_ID,Send_Attendance_Notification,Send_Service_Notification,Child_Care_Available,Kids_Welcome) VALUES
('(t) KC Oakley Nursery',9            ,2          ,1              ,7680240        ,'Test data',{ts '2004-09-21 00:00:00'},0          ,166602      ,0            ,1               ,1        ,0                ,0               ,0                 ,0                   ,0             ,'N'       ,'Y'            ,'Y'           ,''         ,null            ,58                        ,0                           ,0                        ,0                   ,0           );

--Nursery A
--Sunday 11:45 Nursery A
INSERT INTO Opportunities 
(Opportunity_Title             ,Description,Group_Role_ID,Program_ID,Visibility_Level_ID,Contact_Person,Publish_Date              ,Add_to_Group                                                            ,Minimum_Needed,Maximum_Needed,[Domain_ID],On_Connection_Card,Shift_Start   ,Shift_End     ,Event_Type_ID,Sign_Up_Deadline_ID,Room             ,Send_Reminder,Reminder_Template,Reminder_Days_Prior) VALUES
('(t) Nursery A K112 Sun 11:45','Test Data',16           ,83        ,4                  ,7680240       ,{ts '2015-10-27 14:55:00'},(select group_id from Groups where Group_Name = '(t) KC Oakley Nursery'),3             ,5             ,1          ,0                 ,{t '11:20:00'},{t '13:20:00'},97           ,7                  ,'OAK KC Room 112',1            ,108700           ,3                  );

--Sunday 10:05 Nursery A
INSERT INTO Opportunities 
(Opportunity_Title             ,Description,Group_Role_ID,Program_ID,Visibility_Level_ID,Contact_Person,Publish_Date              ,Add_to_Group                                                            ,Minimum_Needed,Maximum_Needed,[Domain_ID],On_Connection_Card,Shift_Start   ,Shift_End     ,Event_Type_ID,Sign_Up_Deadline_ID,Room             ,Send_Reminder,Reminder_Template,Reminder_Days_Prior) VALUES
('(t) Nursery A K112 Sun 10:05','Test Data',16           ,83        ,4                  ,7680240       ,{ts '2015-10-27 14:55:00'},(select group_id from Groups where Group_Name = '(t) KC Oakley Nursery'),3             ,5             ,1          ,0                 ,{t '09:40:00'},{t '11:20:00'},96           ,7                  ,'OAK KC Room 112',1            ,108700           ,3                  );

--Sunday 8:30
INSERT INTO Opportunities 
(Opportunity_Title            ,Description,Group_Role_ID,Program_ID,Visibility_Level_ID,Contact_Person,Publish_Date              ,Add_to_Group                                                            ,Minimum_Needed,Maximum_Needed,[Domain_ID],On_Connection_Card,Shift_Start   ,Shift_End     ,Event_Type_ID,Sign_Up_Deadline_ID,Room             ,Send_Reminder,Reminder_Template,Reminder_Days_Prior) VALUES
('(t) Nursery A K112 Sun 8:30','Test Data',16           ,83        ,4                  ,7680240       ,{ts '2015-10-27 14:55:00'},(select group_id from Groups where Group_Name = '(t) KC Oakley Nursery'),3             ,5             ,1          ,0                 ,{t '08:05:00'},{t '09:50:00'},95           ,7                  ,'OAK KC Room 112',1            ,108700           ,3                  );

--Saturday 4:30
INSERT INTO Opportunities 
(Opportunity_Title            ,Description,Group_Role_ID,Program_ID,Visibility_Level_ID,Contact_Person,Publish_Date              ,Add_to_Group                                                            ,Minimum_Needed,Maximum_Needed,[Domain_ID],On_Connection_Card,Shift_Start   ,Shift_End     ,Event_Type_ID,Sign_Up_Deadline_ID,Room             ,Send_Reminder,Reminder_Template,Reminder_Days_Prior) VALUES
('(t) Nursery A K112 Sat 4:30','Test Data',16           ,83        ,4                  ,7680240       ,{ts '2015-10-27 14:55:00'},(select group_id from Groups where Group_Name = '(t) KC Oakley Nursery'),3             ,5            ,1          ,0                 ,{t '16:05:00'},{t '17:50:00'},94           ,7                  ,'OAK KC Room 112',1            ,108700           ,3                  );

--Saturday 6:15
INSERT INTO Opportunities 
(Opportunity_Title            ,Description,Group_Role_ID,Program_ID,Visibility_Level_ID,Contact_Person,Publish_Date              ,Add_to_Group                                                              ,Minimum_Needed,Maximum_Needed,[Domain_ID],On_Connection_Card,Shift_Start   ,Shift_End     ,Event_Type_ID,Sign_Up_Deadline_ID,Room             ,Send_Reminder,Reminder_Template,Reminder_Days_Prior) VALUES
('(t) Nursery A K112 Sat 6:15','Test Data',16           ,83        ,4                  ,7680240       ,{ts '2015-10-27 14:55:00'},(select group_id from Groups where Group_Name = '(t) KC Oakley Nursery'),3             ,5             ,1          ,0                 ,{t '17:50:00'},{t '19:30:00'},101           ,7                  ,'OAK KC Room 112',1            ,108700           ,3                  );

--Nursery B
--Sunday 11:45 Nursery B
INSERT INTO Opportunities 
(Opportunity_Title             ,Description,Group_Role_ID,Program_ID,Visibility_Level_ID,Contact_Person,Publish_Date              ,Add_to_Group                                                            ,Minimum_Needed,Maximum_Needed,[Domain_ID],On_Connection_Card,Shift_Start   ,Shift_End     ,Event_Type_ID,Sign_Up_Deadline_ID,Room             ,Send_Reminder,Reminder_Template,Reminder_Days_Prior) VALUES
('(t) Nursery B K114 Sun 11:45','Test Data',16           ,83        ,4                  ,7680240       ,{ts '2015-10-27 14:55:00'},(select group_id from Groups where Group_Name = '(t) KC Oakley Nursery'),3             ,5             ,1          ,0                 ,{t '11:20:00'},{t '13:20:00'},97           ,7                  ,'OAK KC Room 114',1            ,108700           ,3                  );

--Sunday 10:05 Nursery B
INSERT INTO Opportunities 
(Opportunity_Title             ,Description,Group_Role_ID,Program_ID,Visibility_Level_ID,Contact_Person,Publish_Date              ,Add_to_Group                                                            ,Minimum_Needed,Maximum_Needed,[Domain_ID],On_Connection_Card,Shift_Start   ,Shift_End     ,Event_Type_ID,Sign_Up_Deadline_ID,Room             ,Send_Reminder,Reminder_Template,Reminder_Days_Prior) VALUES
('(t) Nursery B K114 Sun 10:05','Test Data',16           ,83        ,4                  ,7680240       ,{ts '2015-10-27 14:55:00'},(select group_id from Groups where Group_Name = '(t) KC Oakley Nursery'),3             ,5             ,1          ,0                 ,{t '09:40:00'},{t '11:20:00'},96           ,7                  ,'OAK KC Room 114',1            ,108700           ,3                  );

--Sunday 8:30
INSERT INTO Opportunities 
(Opportunity_Title            ,Description,Group_Role_ID,Program_ID,Visibility_Level_ID,Contact_Person,Publish_Date              ,Add_to_Group                                                            ,Minimum_Needed,Maximum_Needed,[Domain_ID],On_Connection_Card,Shift_Start   ,Shift_End     ,Event_Type_ID,Sign_Up_Deadline_ID,Room             ,Send_Reminder,Reminder_Template,Reminder_Days_Prior) VALUES
('(t) Nursery B K114 Sun 8:30','Test Data',16           ,83        ,4                  ,7680240       ,{ts '2015-10-27 14:55:00'},(select group_id from Groups where Group_Name = '(t) KC Oakley Nursery'),3             ,5             ,1          ,0                 ,{t '08:05:00'},{t '09:50:00'},95           ,7                  ,'OAK KC Room 114',1            ,108700           ,3                  );

--Saturday 4:30
INSERT INTO Opportunities 
(Opportunity_Title            ,Description,Group_Role_ID,Program_ID,Visibility_Level_ID,Contact_Person,Publish_Date              ,Add_to_Group                                                            ,Minimum_Needed,Maximum_Needed,[Domain_ID],On_Connection_Card,Shift_Start   ,Shift_End     ,Event_Type_ID,Sign_Up_Deadline_ID,Room             ,Send_Reminder,Reminder_Template,Reminder_Days_Prior) VALUES
('(t) Nursery B K114 Sat 4:30','Test Data',16           ,83        ,4                  ,7680240       ,{ts '2015-10-27 14:55:00'},(select group_id from Groups where Group_Name = '(t) KC Oakley Nursery'),3             ,5            ,1          ,0                 ,{t '16:05:00'},{t '17:50:00'},94           ,7                  ,'OAK KC Room 114',1            ,108700           ,3                  );

--Saturday 6:15
INSERT INTO Opportunities 
(Opportunity_Title            ,Description,Group_Role_ID,Program_ID,Visibility_Level_ID,Contact_Person,Publish_Date              ,Add_to_Group                                                              ,Minimum_Needed,Maximum_Needed,[Domain_ID],On_Connection_Card,Shift_Start   ,Shift_End     ,Event_Type_ID,Sign_Up_Deadline_ID,Room             ,Send_Reminder,Reminder_Template,Reminder_Days_Prior) VALUES
('(t) Nursery B K114 Sat 6:15','Test Data',16           ,83        ,4                  ,7680240       ,{ts '2015-10-27 14:55:00'},(select group_id from Groups where Group_Name = '(t) KC Oakley Nursery'),3             ,5             ,1          ,0                 ,{t '17:50:00'},{t '19:30:00'},101           ,7                  ,'OAK KC Room 114',1            ,108700           ,3                  );

--Nursery C
--Sunday 11:45 Nursery C
INSERT INTO Opportunities 
(Opportunity_Title             ,Description,Group_Role_ID,Program_ID,Visibility_Level_ID,Contact_Person,Publish_Date              ,Add_to_Group                                                            ,Minimum_Needed,Maximum_Needed,[Domain_ID],On_Connection_Card,Shift_Start   ,Shift_End     ,Event_Type_ID,Sign_Up_Deadline_ID,Room             ,Send_Reminder,Reminder_Template,Reminder_Days_Prior) VALUES
('(t) Nursery C K116 Sun 11:45','Test Data',16           ,83        ,4                  ,7680240       ,{ts '2015-10-27 14:55:00'},(select group_id from Groups where Group_Name = '(t) KC Oakley Nursery'),3             ,5             ,1          ,0                 ,{t '11:20:00'},{t '13:20:00'},97           ,7                  ,'OAK KC Room 116',1            ,108700           ,3                  );

--Sunday 10:05 Nursery C
INSERT INTO Opportunities 
(Opportunity_Title             ,Description,Group_Role_ID,Program_ID,Visibility_Level_ID,Contact_Person,Publish_Date              ,Add_to_Group                                                            ,Minimum_Needed,Maximum_Needed,[Domain_ID],On_Connection_Card,Shift_Start   ,Shift_End     ,Event_Type_ID,Sign_Up_Deadline_ID,Room             ,Send_Reminder,Reminder_Template,Reminder_Days_Prior) VALUES
('(t) Nursery C K116 Sun 10:05','Test Data',16           ,83        ,4                  ,7680240       ,{ts '2015-10-27 14:55:00'},(select group_id from Groups where Group_Name = '(t) KC Oakley Nursery'),3             ,5             ,1          ,0                 ,{t '09:40:00'},{t '11:20:00'},96           ,7                  ,'OAK KC Room 116',1            ,108700           ,3                  );

--Sunday 8:30
INSERT INTO Opportunities 
(Opportunity_Title            ,Description,Group_Role_ID,Program_ID,Visibility_Level_ID,Contact_Person,Publish_Date              ,Add_to_Group                                                            ,Minimum_Needed,Maximum_Needed,[Domain_ID],On_Connection_Card,Shift_Start   ,Shift_End     ,Event_Type_ID,Sign_Up_Deadline_ID,Room             ,Send_Reminder,Reminder_Template,Reminder_Days_Prior) VALUES
('(t) Nursery C K116 Sun 8:30','Test Data',16           ,83        ,4                  ,7680240       ,{ts '2015-10-27 14:55:00'},(select group_id from Groups where Group_Name = '(t) KC Oakley Nursery'),3             ,5             ,1          ,0                 ,{t '08:05:00'},{t '09:50:00'},95           ,7                  ,'OAK KC Room 116',1            ,108700           ,3                  );

--Saturday 4:30
INSERT INTO Opportunities 
(Opportunity_Title            ,Description,Group_Role_ID,Program_ID,Visibility_Level_ID,Contact_Person,Publish_Date              ,Add_to_Group                                                            ,Minimum_Needed,Maximum_Needed,[Domain_ID],On_Connection_Card,Shift_Start   ,Shift_End     ,Event_Type_ID,Sign_Up_Deadline_ID,Room             ,Send_Reminder,Reminder_Template,Reminder_Days_Prior) VALUES
('(t) Nursery C K116 Sat 4:30','Test Data',16           ,83        ,4                  ,7680240       ,{ts '2015-10-27 14:55:00'},(select group_id from Groups where Group_Name = '(t) KC Oakley Nursery'),3             ,5            ,1          ,0                 ,{t '16:05:00'},{t '17:50:00'},94           ,7                  ,'OAK KC Room 116',1            ,108700           ,3                  );

--Saturday 6:15
INSERT INTO Opportunities 
(Opportunity_Title            ,Description,Group_Role_ID,Program_ID,Visibility_Level_ID,Contact_Person,Publish_Date              ,Add_to_Group                                                              ,Minimum_Needed,Maximum_Needed,[Domain_ID],On_Connection_Card,Shift_Start   ,Shift_End     ,Event_Type_ID,Sign_Up_Deadline_ID,Room             ,Send_Reminder,Reminder_Template,Reminder_Days_Prior) VALUES
('(t) Nursery C K116 Sat 6:15','Test Data',16           ,83        ,4                  ,7680240       ,{ts '2015-10-27 14:55:00'},(select group_id from Groups where Group_Name = '(t) KC Oakley Nursery'),3             ,5             ,1          ,0                 ,{t '17:50:00'},{t '19:30:00'},101           ,7                  ,'OAK KC Room 116',1            ,108700           ,3                  );

--Nursery D
--Sunday 11:45 Nursery D
INSERT INTO Opportunities 
(Opportunity_Title             ,Description,Group_Role_ID,Program_ID,Visibility_Level_ID,Contact_Person,Publish_Date              ,Add_to_Group                                                            ,Minimum_Needed,Maximum_Needed,[Domain_ID],On_Connection_Card,Shift_Start   ,Shift_End     ,Event_Type_ID,Sign_Up_Deadline_ID,Room             ,Send_Reminder,Reminder_Template,Reminder_Days_Prior) VALUES
('(t) Nursery D K126 Sun 11:45','Test Data',16           ,83        ,4                  ,7680240       ,{ts '2015-10-27 14:55:00'},(select group_id from Groups where Group_Name = '(t) KC Oakley Nursery'),3             ,5             ,1          ,0                 ,{t '11:20:00'},{t '13:20:00'},97           ,7                  ,'OAK KC Room 126',1            ,108700           ,3                  );

--Sunday 10:05 Nursery D
INSERT INTO Opportunities 
(Opportunity_Title             ,Description,Group_Role_ID,Program_ID,Visibility_Level_ID,Contact_Person,Publish_Date              ,Add_to_Group                                                            ,Minimum_Needed,Maximum_Needed,[Domain_ID],On_Connection_Card,Shift_Start   ,Shift_End     ,Event_Type_ID,Sign_Up_Deadline_ID,Room             ,Send_Reminder,Reminder_Template,Reminder_Days_Prior) VALUES
('(t) Nursery D K126 Sun 10:05','Test Data',16           ,83        ,4                  ,7680240       ,{ts '2015-10-27 14:55:00'},(select group_id from Groups where Group_Name = '(t) KC Oakley Nursery'),3             ,5             ,1          ,0                 ,{t '09:40:00'},{t '11:20:00'},96           ,7                  ,'OAK KC Room 126',1            ,108700           ,3                  );

--Sunday 8:30
INSERT INTO Opportunities 
(Opportunity_Title            ,Description,Group_Role_ID,Program_ID,Visibility_Level_ID,Contact_Person,Publish_Date              ,Add_to_Group                                                            ,Minimum_Needed,Maximum_Needed,[Domain_ID],On_Connection_Card,Shift_Start   ,Shift_End     ,Event_Type_ID,Sign_Up_Deadline_ID,Room             ,Send_Reminder,Reminder_Template,Reminder_Days_Prior) VALUES
('(t) Nursery D K126 Sun 8:30','Test Data',16           ,83        ,4                  ,7680240       ,{ts '2015-10-27 14:55:00'},(select group_id from Groups where Group_Name = '(t) KC Oakley Nursery'),3             ,5             ,1          ,0                 ,{t '08:05:00'},{t '09:50:00'},95           ,7                  ,'OAK KC Room 126',1            ,108700           ,3                  );

--Saturday 4:30
INSERT INTO Opportunities 
(Opportunity_Title            ,Description,Group_Role_ID,Program_ID,Visibility_Level_ID,Contact_Person,Publish_Date              ,Add_to_Group                                                            ,Minimum_Needed,Maximum_Needed,[Domain_ID],On_Connection_Card,Shift_Start   ,Shift_End     ,Event_Type_ID,Sign_Up_Deadline_ID,Room             ,Send_Reminder,Reminder_Template,Reminder_Days_Prior) VALUES
('(t) Nursery D K126 Sat 4:30','Test Data',16           ,83        ,4                  ,7680240       ,{ts '2015-10-27 14:55:00'},(select group_id from Groups where Group_Name = '(t) KC Oakley Nursery'),3             ,5            ,1          ,0                 ,{t '16:05:00'},{t '17:50:00'},94           ,7                  ,'OAK KC Room 126',1            ,108700           ,3                  );

--Saturday 6:15
INSERT INTO Opportunities 
(Opportunity_Title            ,Description,Group_Role_ID,Program_ID,Visibility_Level_ID,Contact_Person,Publish_Date              ,Add_to_Group                                                              ,Minimum_Needed,Maximum_Needed,[Domain_ID],On_Connection_Card,Shift_Start   ,Shift_End     ,Event_Type_ID,Sign_Up_Deadline_ID,Room             ,Send_Reminder,Reminder_Template,Reminder_Days_Prior) VALUES
('(t) Nursery D K126 Sat 6:15','Test Data',16           ,83        ,4                  ,7680240       ,{ts '2015-10-27 14:55:00'},(select group_id from Groups where Group_Name = '(t) KC Oakley Nursery'),3             ,5             ,1          ,0                 ,{t '17:50:00'},{t '19:30:00'},101           ,7                  ,'OAK KC Room 126',1            ,108700           ,3                  );

--Nursery E
--Sunday 11:45 Nursery E
INSERT INTO Opportunities 
(Opportunity_Title             ,Description,Group_Role_ID,Program_ID,Visibility_Level_ID,Contact_Person,Publish_Date              ,Add_to_Group                                                            ,Minimum_Needed,Maximum_Needed,[Domain_ID],On_Connection_Card,Shift_Start   ,Shift_End     ,Event_Type_ID,Sign_Up_Deadline_ID,Room             ,Send_Reminder,Reminder_Template,Reminder_Days_Prior) VALUES
('(t) Nursery E K124 Sun 11:45','Test Data',16           ,83        ,4                  ,7680240       ,{ts '2015-10-27 14:55:00'},(select group_id from Groups where Group_Name = '(t) KC Oakley Nursery'),3             ,5             ,1          ,0                 ,{t '11:20:00'},{t '13:20:00'},97           ,7                  ,'OAK KC Room 124',1            ,108700           ,3                  );

--Sunday 10:05 Nursery E
INSERT INTO Opportunities 
(Opportunity_Title             ,Description,Group_Role_ID,Program_ID,Visibility_Level_ID,Contact_Person,Publish_Date              ,Add_to_Group                                                            ,Minimum_Needed,Maximum_Needed,[Domain_ID],On_Connection_Card,Shift_Start   ,Shift_End     ,Event_Type_ID,Sign_Up_Deadline_ID,Room             ,Send_Reminder,Reminder_Template,Reminder_Days_Prior) VALUES
('(t) Nursery E K124 Sun 10:05','Test Data',16           ,83        ,4                  ,7680240       ,{ts '2015-10-27 14:55:00'},(select group_id from Groups where Group_Name = '(t) KC Oakley Nursery'),3             ,5             ,1          ,0                 ,{t '09:40:00'},{t '11:20:00'},96           ,7                  ,'OAK KC Room 124',1            ,108700           ,3                  );

--Sunday 8:30
INSERT INTO Opportunities 
(Opportunity_Title            ,Description,Group_Role_ID,Program_ID,Visibility_Level_ID,Contact_Person,Publish_Date              ,Add_to_Group                                                            ,Minimum_Needed,Maximum_Needed,[Domain_ID],On_Connection_Card,Shift_Start   ,Shift_End     ,Event_Type_ID,Sign_Up_Deadline_ID,Room             ,Send_Reminder,Reminder_Template,Reminder_Days_Prior) VALUES
('(t) Nursery E K124 Sun 8:30','Test Data',16           ,83        ,4                  ,7680240       ,{ts '2015-10-27 14:55:00'},(select group_id from Groups where Group_Name = '(t) KC Oakley Nursery'),3             ,5             ,1          ,0                 ,{t '08:05:00'},{t '09:50:00'},95           ,7                  ,'OAK KC Room 124',1            ,108700           ,3                  );

--Saturday 4:30
INSERT INTO Opportunities 
(Opportunity_Title            ,Description,Group_Role_ID,Program_ID,Visibility_Level_ID,Contact_Person,Publish_Date              ,Add_to_Group                                                            ,Minimum_Needed,Maximum_Needed,[Domain_ID],On_Connection_Card,Shift_Start   ,Shift_End     ,Event_Type_ID,Sign_Up_Deadline_ID,Room             ,Send_Reminder,Reminder_Template,Reminder_Days_Prior) VALUES
('(t) Nursery E K124 Sat 4:30','Test Data',16           ,83        ,4                  ,7680240       ,{ts '2015-10-27 14:55:00'},(select group_id from Groups where Group_Name = '(t) KC Oakley Nursery'),3             ,5            ,1          ,0                 ,{t '16:05:00'},{t '17:50:00'},94           ,7                  ,'OAK KC Room 124',1            ,108700           ,3                  );

--Saturday 6:15
INSERT INTO Opportunities 
(Opportunity_Title            ,Description,Group_Role_ID,Program_ID,Visibility_Level_ID,Contact_Person,Publish_Date              ,Add_to_Group                                                              ,Minimum_Needed,Maximum_Needed,[Domain_ID],On_Connection_Card,Shift_Start   ,Shift_End     ,Event_Type_ID,Sign_Up_Deadline_ID,Room             ,Send_Reminder,Reminder_Template,Reminder_Days_Prior) VALUES
('(t) Nursery E K124 Sat 6:15','Test Data',16           ,83        ,4                  ,7680240       ,{ts '2015-10-27 14:55:00'},(select group_id from Groups where Group_Name = '(t) KC Oakley Nursery'),3             ,5             ,1          ,0                 ,{t '17:50:00'},{t '19:30:00'},101           ,7                  ,'OAK KC Room 124',1            ,108700           ,3                  );

USE [MinistryPlatform]
GO

-- getting the ID for the group
declare @GroupID as int
set @GroupID = (select top 1 group_id from Groups where Group_Name = '(t+auto) Avalanche');

--Saturday Attack Shinra Mako reactor 1
--Setup
INSERT INTO Opportunities 
(Opportunity_Title                      ,Description,Group_Role_ID,Program_ID,Visibility_Level_ID,Contact_Person,Publish_Date              ,Add_to_Group ,Minimum_Needed,Maximum_Needed,[Domain_ID],On_Connection_Card,Shift_Start   ,Shift_End     ,Event_Type_ID,Sign_Up_Deadline_ID,Room           ,Send_Reminder,Reminder_Template,Reminder_Days_Prior) VALUES
('(t+auto) Attack Shinra Mako reactor 1','Test Data',16           ,106       ,4                  ,7654100       ,{ts '2015-11-27 14:55:00'},@GroupID     ,1             ,250           ,1          ,0                 ,{t '14:00:00'},{t '16:00:00'},95           ,7                  ,'Oakley Atrium',1            ,108700           ,3                  );

--Attack Shinra Mako reactor 5
INSERT INTO Opportunities 
(Opportunity_Title                      ,Description,Group_Role_ID,Program_ID,Visibility_Level_ID,Contact_Person,Publish_Date              ,Add_to_Group ,Minimum_Needed,Maximum_Needed,[Domain_ID],On_Connection_Card,Shift_Start   ,Shift_End     ,Event_Type_ID,Sign_Up_Deadline_ID,Room                       ,Send_Reminder,Reminder_Template,Reminder_Days_Prior) VALUES
('(t+auto) Attack Shinra Mako reactor 5','Test Data',16           ,106       ,4                  ,7654100       ,{ts '2015-11-27 14:55:00'},@GroupID     ,1             ,250           ,1          ,0                 ,{t '16:00:00'},{t '18:00:00'},94           ,7                  ,'Main Coffee Station Alpha',1            ,108700           ,3                  );


Use [MinistryPlatform]
GO

--Store group ids
DECLARE @superbowl_group_id int;
SET @superbowl_group_id = (SELECT TOP 1 group_id FROM groups WITH (NOLOCK) WHERE group_name = '(t) Superbowl Oakley Group');

DECLARE @kidsclub_group_id int;
SET @kidsclub_group_id = (SELECT TOP 1 group_id FROM groups WITH (NOLOCK) WHERE group_name = '(t) KidsClub Oakley Group');

DECLARE @shinra_group_id int;
SET @shinra_group_id = (SELECT TOP 1 group_id FROM groups WITH (NOLOCK) WHERE group_name = '(t+auto) Serve Shinra Electric Power Company');

DECLARE @avalanche_group_id int;
SET @avalanche_group_id = (SELECT TOP 1 group_id FROM groups WITH (NOLOCK) WHERE group_name = '(t+auto) Avalanche');

INSERT INTO [dbo].Group_Participants
(Group_ID            ,Participant_ID                                                                                                   ,Group_Role_ID,Domain_ID,Start_Date                ,End_Date,Employee_Role,Hours_Per_Week,Notes,__ExternalPersonGroupRoleID,__ExternalGroupRoleID,__CanManageEvents,__CanMANageMembers,__EmailOptOut,__ISAnonymous,__ServiceTimeID,_First_Attendance,_Second_Attendance,_Third_Attendance,_Last_Attendance) VALUES
(@superbowl_group_id ,(select Participant_record FROM contacts WITH (NOLOCK) WHERE email_address = 'mpcrds+tremplay.richard@gmail.com'),22           ,1        ,{ts '2015-05-01 00:00:00'},null    ,0            ,null          ,null ,null                       ,null                 ,null             ,null              ,null         ,null         ,null           ,null             ,null              ,null             ,null            );

INSERT INTO [dbo].Group_Participants
(Group_ID            ,Participant_ID                                                                                                 ,Group_Role_ID,Domain_ID,Start_Date                ,End_Date,Employee_Role,Hours_Per_Week,Notes,__ExternalPersonGroupRoleID,__ExternalGroupRoleID,__CanManageEvents,__CanMANageMembers,__EmailOptOut,__ISAnonymous,__ServiceTimeID,_First_Attendance,_Second_Attendance,_Third_Attendance,_Last_Attendance) VALUES
(@superbowl_group_id ,(select Participant_record FROM contacts WITH (NOLOCK) WHERE email_address = 'mpcrds+tremplay.mary@gmail.com') ,22           ,1        ,{ts '2015-05-01 00:00:00'},null    ,0            ,null          ,null ,null                       ,null                 ,null             ,null              ,null         ,null         ,null           ,null             ,null              ,null             ,null            );

INSERT INTO [dbo].Group_Participants
(Group_ID            ,Participant_ID                                                                                                 ,Group_Role_ID,Domain_ID,Start_Date                ,End_Date,Employee_Role,Hours_Per_Week,Notes,__ExternalPersonGroupRoleID,__ExternalGroupRoleID,__CanManageEvents,__CanMANageMembers,__EmailOptOut,__ISAnonymous,__ServiceTimeID,_First_Attendance,_Second_Attendance,_Third_Attendance,_Last_Attendance) VALUES
(@superbowl_group_id ,(select Participant_record FROM contacts WITH (NOLOCK) WHERE email_address = 'mpcrds+tremplay.johan@gmail.com'),16           ,1        ,{ts '2015-05-01 00:00:00'},null    ,0            ,null          ,null ,null                       ,null                 ,null             ,null              ,null         ,null         ,null           ,null             ,null              ,null             ,null            );

INSERT INTO [dbo].Group_Participants
(Group_ID            ,Participant_ID                                                                                                   ,Group_Role_ID,Domain_ID,Start_Date                ,End_Date,Employee_Role,Hours_Per_Week,Notes,__ExternalPersonGroupRoleID,__ExternalGroupRoleID,__CanManageEvents,__CanMANageMembers,__EmailOptOut,__ISAnonymous,__ServiceTimeID,_First_Attendance,_Second_Attendance,_Third_Attendance,_Last_Attendance) VALUES
(@superbowl_group_id ,(select Participant_record FROM contacts WITH (NOLOCK) WHERE email_address = 'mpcrds+tremplay.Josina@gmail.com') ,16           ,1        ,{ts '2015-05-01 00:00:00'},null    ,0            ,null          ,null ,null                       ,null                 ,null             ,null              ,null         ,null         ,null           ,null             ,null              ,null             ,null            );

INSERT INTO [dbo].Group_Participants
(Group_ID           ,Participant_ID                                                                                                    ,Group_Role_ID,Domain_ID,Start_Date                ,End_Date,Employee_Role,Hours_Per_Week,Notes,__ExternalPersonGroupRoleID,__ExternalGroupRoleID,__CanManageEvents,__CanMANageMembers,__EmailOptOut,__ISAnonymous,__ServiceTimeID,_First_Attendance,_Second_Attendance,_Third_Attendance,_Last_Attendance) VALUES
(@superbowl_group_id,(select Participant_record FROM contacts WITH (NOLOCK) WHERE email_address = 'mpcrds+tremplay.vincent@gmail.com') ,16           ,1        ,{ts '2015-05-01 00:00:00'},null    ,0            ,null          ,null ,null                       ,null                 ,null             ,null              ,null         ,null         ,null           ,null             ,null              ,null             ,null            );

INSERT INTO [dbo].Group_Participants
(Group_ID            ,Participant_ID                                                                                                   ,Group_Role_ID,Domain_ID,Start_Date                ,End_Date,Employee_Role,Hours_Per_Week,Notes,__ExternalPersonGroupRoleID,__ExternalGroupRoleID,__CanManageEvents,__CanMANageMembers,__EmailOptOut,__ISAnonymous,__ServiceTimeID,_First_Attendance,_Second_Attendance,_Third_Attendance,_Last_Attendance) VALUES
(@superbowl_group_id ,(select Participant_record FROM contacts WITH (NOLOCK) WHERE email_address = 'mpcrds+tremplay.sophia@gmail.com') ,16           ,1        ,{ts '2015-05-01 00:00:00'},null    ,0            ,null          ,null ,null                       ,null                 ,null             ,null              ,null         ,null         ,null           ,null             ,null              ,null             ,null            );

INSERT INTO [dbo].Group_Participants
(Group_ID            ,Participant_ID                                                                                                   ,Group_Role_ID,Domain_ID,Start_Date                ,End_Date,Employee_Role,Hours_Per_Week,Notes,__ExternalPersonGroupRoleID,__ExternalGroupRoleID,__CanManageEvents,__CanMANageMembers,__EmailOptOut,__ISAnonymous,__ServiceTimeID,_First_Attendance,_Second_Attendance,_Third_Attendance,_Last_Attendance) VALUES
(@superbowl_group_id ,(select Participant_record FROM contacts WITH (NOLOCK) WHERE email_address = 'mpcrds+tremplay.george@gmail.com') ,22           ,1        ,{ts '2015-05-01 00:00:00'},null    ,0            ,null          ,null ,null                       ,null                 ,null             ,null              ,null         ,null         ,null           ,null             ,null              ,null             ,null            );

INSERT INTO [dbo].Group_Participants
(Group_ID            ,Participant_ID                                                                                                     ,Group_Role_ID,Domain_ID,Start_Date                ,End_Date,Employee_Role,Hours_Per_Week,Notes,__ExternalPersonGroupRoleID,__ExternalGroupRoleID,__CanManageEvents,__CanMANageMembers,__EmailOptOut,__ISAnonymous,__ServiceTimeID,_First_Attendance,_Second_Attendance,_Third_Attendance,_Last_Attendance) VALUES
(@superbowl_group_id ,(select Participant_record FROM contacts WITH (NOLOCK) WHERE email_address = 'mpcrds+tremplay.margaret@gmail.com') ,16           ,1        ,{ts '2015-05-01 00:00:00'},null    ,0            ,null          ,null ,null                       ,null                 ,null             ,null              ,null         ,null         ,null           ,null             ,null              ,null             ,null            );


INSERT INTO [dbo].Group_Participants
(Group_ID           ,Participant_ID                                                                                                   ,Group_Role_ID,Domain_ID,Start_Date                ,End_Date,Employee_Role,Hours_Per_Week,Notes,__ExternalPersonGroupRoleID,__ExternalGroupRoleID,__CanManageEvents,__CanMANageMembers,__EmailOptOut,__ISAnonymous,__ServiceTimeID,_First_Attendance,_Second_Attendance,_Third_Attendance,_Last_Attendance) VALUES
(@kidsclub_group_id ,(select Participant_record FROM contacts WITH (NOLOCK) WHERE email_address = 'mpcrds+tremplay.richard@gmail.com'),22           ,1        ,{ts '2015-05-01 00:00:00'},null    ,0            ,null          ,null ,null                       ,null                 ,null             ,null              ,null         ,null         ,null           ,null             ,null              ,null             ,null            );

INSERT INTO [dbo].Group_Participants
(Group_ID           ,Participant_ID                                                                                                 ,Group_Role_ID,Domain_ID,Start_Date                ,End_Date,Employee_Role,Hours_Per_Week,Notes,__ExternalPersonGroupRoleID,__ExternalGroupRoleID,__CanManageEvents,__CanMANageMembers,__EmailOptOut,__ISAnonymous,__ServiceTimeID,_First_Attendance,_Second_Attendance,_Third_Attendance,_Last_Attendance) VALUES
(@kidsclub_group_id ,(select Participant_record FROM contacts WITH (NOLOCK) WHERE email_address = 'mpcrds+tremplay.mary@gmail.com') ,22           ,1        ,{ts '2015-05-01 00:00:00'},null    ,0            ,null          ,null ,null                       ,null                 ,null             ,null              ,null         ,null         ,null           ,null             ,null              ,null             ,null            );

INSERT INTO [dbo].Group_Participants
(Group_ID           ,Participant_ID                                                                                                 ,Group_Role_ID,Domain_ID,Start_Date                ,End_Date,Employee_Role,Hours_Per_Week,Notes,__ExternalPersonGroupRoleID,__ExternalGroupRoleID,__CanManageEvents,__CanMANageMembers,__EmailOptOut,__ISAnonymous,__ServiceTimeID,_First_Attendance,_Second_Attendance,_Third_Attendance,_Last_Attendance) VALUES
(@kidsclub_group_id ,(select Participant_record FROM contacts WITH (NOLOCK) WHERE email_address = 'mpcrds+tremplay.johan@gmail.com'),16           ,1        ,{ts '2015-05-01 00:00:00'},null    ,0            ,null          ,null ,null                       ,null                 ,null             ,null              ,null         ,null         ,null           ,null             ,null              ,null             ,null            );

INSERT INTO [dbo].Group_Participants
(Group_ID           ,Participant_ID                                                                                                   ,Group_Role_ID,Domain_ID,Start_Date                ,End_Date,Employee_Role,Hours_Per_Week,Notes,__ExternalPersonGroupRoleID,__ExternalGroupRoleID,__CanManageEvents,__CanMANageMembers,__EmailOptOut,__ISAnonymous,__ServiceTimeID,_First_Attendance,_Second_Attendance,_Third_Attendance,_Last_Attendance) VALUES
(@kidsclub_group_id ,(select Participant_record FROM contacts WITH (NOLOCK) WHERE email_address = 'mpcrds+tremplay.Josina@gmail.com') ,16           ,1        ,{ts '2015-05-01 00:00:00'},null    ,0            ,null          ,null ,null                       ,null                 ,null             ,null              ,null         ,null         ,null           ,null             ,null              ,null             ,null            );

INSERT INTO [dbo].Group_Participants
(Group_ID           ,Participant_ID                                                                                                   ,Group_Role_ID,Domain_ID,Start_Date                ,End_Date,Employee_Role,Hours_Per_Week,Notes,__ExternalPersonGroupRoleID,__ExternalGroupRoleID,__CanManageEvents,__CanMANageMembers,__EmailOptOut,__ISAnonymous,__ServiceTimeID,_First_Attendance,_Second_Attendance,_Third_Attendance,_Last_Attendance) VALUES
(@kidsclub_group_id ,(select Participant_record FROM contacts WITH (NOLOCK) WHERE email_address = 'mpcrds+tremplay.vincent@gmail.com') ,16           ,1        ,{ts '2015-05-01 00:00:00'},null    ,0            ,null          ,null ,null                       ,null                 ,null             ,null              ,null         ,null         ,null           ,null             ,null              ,null             ,null            );

INSERT INTO [dbo].Group_Participants
(Group_ID           ,Participant_ID                                                                                                   ,Group_Role_ID,Domain_ID,Start_Date                ,End_Date,Employee_Role,Hours_Per_Week,Notes,__ExternalPersonGroupRoleID,__ExternalGroupRoleID,__CanManageEvents,__CanMANageMembers,__EmailOptOut,__ISAnonymous,__ServiceTimeID,_First_Attendance,_Second_Attendance,_Third_Attendance,_Last_Attendance) VALUES
(@kidsclub_group_id ,(select Participant_record FROM contacts WITH (NOLOCK) WHERE email_address = 'mpcrds+tremplay.sophia@gmail.com') ,16           ,1        ,{ts '2015-05-01 00:00:00'},null    ,0            ,null          ,null ,null                       ,null                 ,null             ,null              ,null         ,null         ,null           ,null             ,null              ,null             ,null            );

INSERT INTO [dbo].Group_Participants
(Group_ID           ,Participant_ID                                                                                                   ,Group_Role_ID,Domain_ID,Start_Date                ,End_Date,Employee_Role,Hours_Per_Week,Notes,__ExternalPersonGroupRoleID,__ExternalGroupRoleID,__CanManageEvents,__CanMANageMembers,__EmailOptOut,__ISAnonymous,__ServiceTimeID,_First_Attendance,_Second_Attendance,_Third_Attendance,_Last_Attendance) VALUES
(@kidsclub_group_id ,(select Participant_record FROM contacts WITH (NOLOCK) WHERE email_address = 'mpcrds+tremplay.george@gmail.com') ,22           ,1        ,{ts '2015-05-01 00:00:00'},null    ,0            ,null          ,null ,null                       ,null                 ,null             ,null              ,null         ,null         ,null           ,null             ,null              ,null             ,null            );

INSERT INTO [dbo].Group_Participants
(Group_ID           ,Participant_ID                                                                                                     ,Group_Role_ID,Domain_ID,Start_Date                ,End_Date,Employee_Role,Hours_Per_Week,Notes,__ExternalPersonGroupRoleID,__ExternalGroupRoleID,__CanManageEvents,__CanMANageMembers,__EmailOptOut,__ISAnonymous,__ServiceTimeID,_First_Attendance,_Second_Attendance,_Third_Attendance,_Last_Attendance) VALUES
(@kidsclub_group_id ,(select Participant_record FROM contacts WITH (NOLOCK) WHERE email_address = 'mpcrds+tremplay.margaret@gmail.com') ,16           ,1        ,{ts '2015-05-01 00:00:00'},null    ,0            ,null          ,null ,null                       ,null                 ,null             ,null              ,null         ,null         ,null           ,null             ,null              ,null             ,null            );

INSERT INTO [dbo].Group_Participants
(Group_ID         ,Participant_ID                                                                                                     ,Group_Role_ID,Domain_ID,Start_Date                ,End_Date,Employee_Role,Hours_Per_Week,Notes,__ExternalPersonGroupRoleID,__ExternalGroupRoleID,__CanManageEvents,__CanMANageMembers,__EmailOptOut,__ISAnonymous,__ServiceTimeID,_First_Attendance,_Second_Attendance,_Third_Attendance,_Last_Attendance) VALUES
(@shinra_group_id ,(select Participant_record FROM contacts WITH (NOLOCK) WHERE email_address = 'mpcrds+auto+barretwallace@gmail.com'),16           ,1        ,{ts '2015-05-01 00:00:00'},null    ,0            ,null          ,null ,null                       ,null                 ,null             ,null              ,null         ,null         ,null           ,null             ,null              ,null             ,null            );


INSERT INTO [dbo].Group_Participants
(Group_ID            ,Participant_ID                                                                                                     ,Group_Role_ID,Domain_ID,Start_Date                ,End_Date,Employee_Role,Hours_Per_Week,Notes,__ExternalPersonGroupRoleID,__ExternalGroupRoleID,__CanManageEvents,__CanMANageMembers,__EmailOptOut,__ISAnonymous,__ServiceTimeID,_First_Attendance,_Second_Attendance,_Third_Attendance,_Last_Attendance) VALUES
(@avalanche_group_id ,(select Participant_record FROM contacts WITH (NOLOCK) WHERE email_address = 'mpcrds+auto+barretwallace@gmail.com'),16           ,1        ,{ts '2015-05-01 00:00:00'},null    ,0            ,null          ,null ,null                       ,null                 ,null             ,null              ,null         ,null         ,null           ,null             ,null              ,null             ,null            );

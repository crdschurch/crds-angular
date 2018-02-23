--Create participant records for Smith children and add them to Kids Club groups
USE [MinistryPlatform]
GO

--Get participant id
DECLARE @child8ParticipantId INT;
SET @child8ParticipantId = (Select Participant_Record from contacts where contact_id in (SELECT Contact_ID FROM [dbo].dp_Users WHERE User_Name = 'mpcrds+auto+franksmith@gmail.com'));

DECLARE @child4ParticipantId INT;
SET @child4ParticipantId = (Select Participant_Record from contacts where contact_id in (SELECT Contact_ID FROM [dbo].dp_Users WHERE User_Name = 'mpcrds+auto+zoesmith@gmail.com'));

DECLARE @child2ParticipantId INT;
SET @child2ParticipantId = (Select Participant_Record from contacts where contact_id in (SELECT Contact_ID FROM [dbo].dp_Users WHERE User_Name = 'mpcrds+auto+jimsmith@gmail.com'));

  --Create Group Participants for the children
DECLARE @KCGrade3 INT = (SELECT TOP 1 group_id FROM Groups WHERE Group_Name = 'Kids Club Grade 3');
INSERT INTO Group_Participants
(Group_ID       ,Participant_ID          ,Group_Role_ID  ,Domain_ID,Start_Date   ,End_Date,Employee_Role,Hours_Per_Week,Notes,__ExternalPersonGroupRoleID,__ExternalGroupRoleID,__CanManageEvents,__CanMANageMembers,__EmailOptOut,__ISAnonymous,__ServiceTimeID,_First_Attendance,_Second_Attendance,_Third_Attendance,_Last_Attendance,Child_Care_Requested,Share_With_Group,Email_Opt_Out,Need_Book,Preferred_Serving_Time_ID,Enrolled_By,Auto_Promote) VALUES
(@KCGrade3      ,@child8ParticipantId    ,16             ,1        ,'01/01/2015' , null   ,0            ,null          ,null ,null                       ,null                 ,null             ,null              ,null         ,null         ,null           ,null             ,null              ,null             ,null            ,0                   ,null            ,null         ,0        ,null                     ,null       ,1           );

DECLARE @KC4YearOlds INT = (SELECT TOP 1 group_id FROM Groups WHERE Group_Name = 'Kids Club 4 Year Old March');
INSERT INTO Group_Participants
(Group_ID        ,Participant_ID        ,Group_Role_ID  ,Domain_ID,Start_Date   ,End_Date,Employee_Role,Hours_Per_Week,Notes,__ExternalPersonGroupRoleID,__ExternalGroupRoleID,__CanManageEvents,__CanMANageMembers,__EmailOptOut,__ISAnonymous,__ServiceTimeID,_First_Attendance,_Second_Attendance,_Third_Attendance,_Last_Attendance,Child_Care_Requested,Share_With_Group,Email_Opt_Out,Need_Book,Preferred_Serving_Time_ID,Enrolled_By,Auto_Promote) VALUES
(@KC4YearOlds    ,@child4ParticipantId  ,16             ,1       ,'01/01/2015'  ,null    ,0            ,null          ,null ,null                       ,null                 ,null             ,null              ,null         ,null         ,null           ,null             ,null              ,null             ,null            ,0                   ,null            ,null         ,0        ,null                     ,null       ,1           );

DECLARE @KC2YearOlds INT = (SELECT TOP 1 group_id FROM Groups WHERE Group_Name = 'Kids Club 2 Year Old March');
INSERT INTO Group_Participants
(Group_ID       ,Participant_ID          ,Group_Role_ID  ,Domain_ID,Start_Date   ,End_Date,Employee_Role,Hours_Per_Week,Notes,__ExternalPersonGroupRoleID,__ExternalGroupRoleID,__CanManageEvents,__CanMANageMembers,__EmailOptOut,__ISAnonymous,__ServiceTimeID,_First_Attendance,_Second_Attendance,_Third_Attendance,_Last_Attendance,Child_Care_Requested,Share_With_Group,Email_Opt_Out,Need_Book,Preferred_Serving_Time_ID,Enrolled_By,Auto_Promote) VALUES
(@KC2YearOlds   ,@child2ParticipantId    ,16             ,1        ,'01/01/2015' , null   ,0            ,null          ,null ,null                       ,null                 ,null             ,null              ,null         ,null         ,null           ,null             ,null              ,null             ,null            ,0                   ,null            ,null         ,0        ,null                     ,null       ,1           );

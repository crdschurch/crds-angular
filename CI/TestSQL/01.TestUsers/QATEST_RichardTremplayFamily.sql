--Create participan accounts for some Tremplay family members
USE [MinistryPlatform]
GO

--Get contact ids trough User account
DECLARE @georgeContactId INT;
SET @georgeContactId = (SELECT Contact_ID FROM [dbo].dp_Users WHERE User_Name = 'mpcrds+tremplay.george@gmail.com');

DECLARE @margaretContactId INT;
SET @margaretContactId = (SELECT Contact_ID FROM [dbo].dp_Users WHERE User_Name = 'mpcrds+tremplay.margaret@gmail.com');

DECLARE @johanContactId INT;
SET @johanContactId = (SELECT Contact_ID FROM [dbo].dp_Users WHERE User_Name = 'mpcrds+tremplay.johan@gmail.com');

DECLARE @josinaContactId INT;
SET @josinaContactId = (SELECT Contact_ID FROM [dbo].dp_Users WHERE User_Name = 'mpcrds+tremplay.Josina@gmail.com');

DECLARE @vincentContactId INT;
SET @vincentContactId = (SELECT Contact_ID FROM [dbo].dp_Users WHERE User_Name = 'mpcrds+tremplay.vincent@gmail.com');

DECLARE @sophiaContactId INT;
SET @sophiaContactId = (SELECT Contact_ID FROM [dbo].dp_Users WHERE User_Name = 'mpcrds+tremplay.sophia@gmail.com');


--Create participant records
INSERT INTO Participants 
(Contact_ID       , Participant_Type_ID,Participant_Start_Date, Participant_End_Date, Notes, Domain_ID, __ExternalPersonID, _First_Attendance_Ever, _Second_Attendance_Ever, _Third_Attendance_Ever, _Last_Attendance_Ever) VALUES
(@georgeContactId, 1                  ,'01/01/2015'          , null                , null , 1        , null              , null                  ,  null                  ,  null                 ,null                  );

INSERT INTO Participants 
(Contact_ID      , Participant_Type_ID, Participant_Start_Date, Participant_End_Date, Notes, Domain_ID, __ExternalPersonID, _First_Attendance_Ever, _Second_Attendance_Ever, _Third_Attendance_Ever, _Last_Attendance_Ever) VALUES
(@margaretContactId, 1                  , '01/01/2015'          , null                , null , 1        , null              , null                  ,  null                  ,  null                 ,null                  );

INSERT INTO Participants 
(Contact_ID      , Participant_Type_ID, Participant_Start_Date, Participant_End_Date, Notes, Domain_ID, __ExternalPersonID, _First_Attendance_Ever, _Second_Attendance_Ever, _Third_Attendance_Ever, _Last_Attendance_Ever) VALUES
(@johanContactId, 1                  , '01/01/2017'          , null                , null , 1        , null              , null                  ,  null                  ,  null                 ,null                  );

INSERT INTO Participants 
(Contact_ID      , Participant_Type_ID, Participant_Start_Date, Participant_End_Date, Notes, Domain_ID, __ExternalPersonID, _First_Attendance_Ever, _Second_Attendance_Ever, _Third_Attendance_Ever, _Last_Attendance_Ever) VALUES
(@josinaContactId, 1                  , '01/01/2017'          , null                , null , 1        , null              , null                  ,  null                  ,  null                 ,null                  );

INSERT INTO Participants 
(Contact_ID      , Participant_Type_ID, Participant_Start_Date, Participant_End_Date, Notes, Domain_ID, __ExternalPersonID, _First_Attendance_Ever, _Second_Attendance_Ever, _Third_Attendance_Ever, _Last_Attendance_Ever) VALUES
(@vincentContactId, 1                  , '01/01/2017'          , null                , null , 1        , null              , null                  ,  null                  ,  null                 ,null                  );

INSERT INTO Participants 
(Contact_ID      , Participant_Type_ID, Participant_Start_Date, Participant_End_Date, Notes, Domain_ID, __ExternalPersonID, _First_Attendance_Ever, _Second_Attendance_Ever, _Third_Attendance_Ever, _Last_Attendance_Ever) VALUES
(@sophiaContactId, 1                  , '01/01/2017'          , null                , null , 1        , null              , null                  ,  null                  ,  null                 ,null                  );
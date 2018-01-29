--(t) Go Midgar participant records
USE [MinistryPlatform]
GO

DECLARE @cloudPartId as int
set @cloudPartId = (select participant_record from Contacts where Email_address = 'mpcrds+cloudstrife@gmail.com');

DECLARE @cloudDonorId as int
set @cloudDonorId = (select donor_record from Contacts where Email_Address = 'mpcrds+cloudstrife@gmail.com');

DECLARE @thisyear as VARCHAR(4)
set @thisyear = CONVERT(VARCHAR(4), datepart(year, getdate()));

DECLARE @tripName AS VARCHAR(18)
set @tripName = '(t) GO Midgar '+@thisyear;

DECLARE @startYear as VARCHAR(19)
set @startYear = @thisyear+'0101';

--Add Cloud Strife to the GO Midgar child GROUP
--do we really need to do this?
DECLARE @subGroupID as int
SET @subGroupID = (select GROUP_ID from groups where group_name = @tripName + ' (Trip Participants)');

INSERT INTO [dbo].Group_Participants 
(Group_ID   ,Participant_ID,Group_Role_ID,Domain_ID,[Start_Date] ,End_Date,Employee_Role,Hours_Per_Week,Notes,__ExternalPersonGroupRoleID,__ExternalGroupRoleID,__CanManageEvents,__CanMANageMembers,__EmailOptOut,__ISAnonymous,__ServiceTimeID,_First_Attendance,_Second_Attendance,_Third_Attendance,_Last_Attendance) VALUES
(@subGroupID,@cloudPartId  ,16           ,1        ,@startYear   ,null    ,0            ,null          ,null ,null                       ,null                 ,null             ,null              ,null         ,null         ,null           ,null             ,null              ,null             ,null            );

--Add Cloud Strife to Event_Participant list
INSERT INTO [dbo].Event_Participants 
(Event_ID                                                             ,Participant_ID,Participation_Status_ID,Time_In   ,Time_Confirmed ,Time_Out,Notes,Domain_ID,Group_Participant_ID,[Check-in_Station],_Setup_Date               ,Group_ID,Room_ID,Call_Parents,Group_Role_ID,Response_ID,__ExternalCalendarServingtimePersonID,Opportunity_ID) VALUES
((select event_id from events where Event_Title like '(t) GO Midgar%'),@cloudPartId  ,2                      ,@startYear,@startYear    ,null    ,null ,1        ,null                ,null              ,{ts '2015-09-09 19:03:37'},null    ,null   ,null        ,null         ,null       ,null                                 ,null          );
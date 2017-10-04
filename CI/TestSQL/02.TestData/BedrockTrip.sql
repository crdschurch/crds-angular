--Test Go Trips
USE [MinistryPlatform]
GO

--Declaring variables to use for the Name and start of trips/pledging. 
--The trip will always be the last week of whatever year the script was run in. 
DECLARE @thisyear as VARCHAR(4)
set @thisyear = CONVERT(VARCHAR(4), datepart(year, getdate()));

DECLARE @tripName AS VARCHAR(50)
set @tripName = '(t+auto) GO Bedrock '+@thisyear;

DECLARE @startYear as VARCHAR(19)
set @startYear = @thisyear+'0101';

DECLARE @endYear as varchar(19)
set @endYear = @thisyear+'1231';

DECLARE @tripStart as varchar(19)
set @tripStart = @thisyear+'1225';

--Group for (t+auto) GO Bedrock
--http://crossroads.knowledgeowl.com/help/create-edit-go-trip-groups 
INSERT INTO [dbo].groups 
(Group_Name,Group_Type_ID,Ministry_ID,Congregation_ID,Primary_Contact,[Description],[Start_Date]                     ,End_Date,Target_Size,Parent_Group,Priority_ID,Enable_Waiting_List,Small_Group_Information,Offsite_Meeting_Address,Group_Is_Full,Available_Online,Life_Stage_ID,Group_Focus_ID,Meeting_Time,Meeting_Day_ID,Descended_From,Reason_Ended,Domain_ID,Check_in_Information,[Secure_Check-in],Suppress_Nametag,Suppress_Care_Note,On_Classroom_Manager,Promotion_Information,Promote_to_Group,Age_in_Months_to_Promote,Promote_Weekly,__ExternalGroupID,__ExternalParentGroupID,__IsPublic,__ISBlogEnabled,__ISWebEnabled,Group_Notes,Sign_Up_To_Serve,Deadline_Passed_Message_ID) VALUES
(@tripName ,6            ,20         ,5              ,2562378        ,null         ,CAST(@startYear as smalldatetime),null    ,null       ,null        ,null       ,null               ,null                   ,null                   ,0            ,1               ,null         ,null          ,null        ,null          ,null          ,null        ,1        ,null                ,0                ,null            ,0                 ,null                ,null                 ,null            ,null                    ,null          ,null             ,null                   ,null      ,null           ,null          ,null       ,null            ,null                      );

--Group for (t+auto) GO Bedrock Participants, Go Bedrock Group as parent
declare @parentGroup as int
set @parentGroup = (select Group_ID from Groups where Group_Name = @tripName);

INSERT INTO [dbo].groups 
(Group_Name                         ,Group_Type_ID,Ministry_ID,Congregation_ID,Primary_Contact,[Description],[Start_Date]                     ,End_Date,Target_Size,Parent_Group,Priority_ID,Enable_Waiting_List,Small_Group_Information,Offsite_Meeting_Address,Group_Is_Full,Available_Online,Life_Stage_ID,Group_Focus_ID,Meeting_Time,Meeting_Day_ID,Descended_From,Reason_Ended,Domain_ID,Check_in_Information,[Secure_Check-in],Suppress_Nametag,Suppress_Care_Note,On_Classroom_Manager,Promotion_Information,Promote_to_Group,Age_in_Months_to_Promote,Promote_Weekly,__ExternalGroupID,__ExternalParentGroupID,__IsPublic,__ISBlogEnabled,__ISWebEnabled,Group_Notes,Sign_Up_To_Serve,Deadline_Passed_Message_ID) VALUES
(@tripName + ' (Trip Participants)' ,6            ,20         ,5              ,2562378        ,null         ,CAST(@startYear as smalldatetime),null    ,null       ,@parentGroup,null       ,null               ,null                   ,null                   ,0            ,1               ,null         ,null          ,null        ,null          ,null          ,null        ,1        ,null                ,0                ,null            ,0                 ,null                ,null                 ,null            ,null                    ,null          ,null             ,null                   ,null      ,null           ,null          ,null       ,null            ,null                      );

--Pledge Campaign for (t+auto) GO Bedrock
SET IDENTITY_INSERT [dbo].[Pledge_Campaigns] ON;

--Store the current identity value so we can reset it.
DECLARE @pledge_id as int
set @pledge_id = IDENT_CURRENT('Pledge_Campaigns');

--Insert large ID so we always know the id for this trip. Trip name is also the nickname as the data on the form is driven by nickname.
--http://crossroads.knowledgeowl.com/help/create-edit-go-trip-pledge-campaign
DECLARE @formID as int
SET @formID = (select form_id from Forms where form_title = 'GO Trip Application');

INSERT INTO [dbo].Pledge_Campaigns 
(Pledge_Campaign_ID,Campaign_Name,Nickname        ,Pledge_Campaign_Type_ID,[Description]                  ,Campaign_Goal,[Start_Date]                     ,End_Date                         ,Domain_ID,Event_ID,Program_ID,Destination_id,Registration_Details,Registration_Start               ,Registration_End                 ,Maximum_Registrants,Youngest_Age_Allowed,Registration_Deposit,Fundraising_Goal,Registration_Form ,Online_Pledge_Details,Allow_Online_Pledge,Online_Thank_You_Message,Pledge_Beyond_End_Date,Show_On_My_Pledges,__ExternalTripID,__ExternalFundID) VALUES
(10000030          ,@tripName    ,@tripName       ,2                      ,'Church Mobilization - Bedrock',5000.00      ,CAST(@startYear as smalldatetime),CAST(@tripStart as smalldatetime),1        ,null    ,null      ,null          ,null                ,CAST(@startYear as smalldatetime),CAST(@tripStart as smalldatetime),null               ,17                  ,100.0000            ,1000.0000       ,@formID           ,null                 ,0                  ,null                    ,0                     ,0                 ,40              ,99              );

SET IDENTITY_INSERT [dbo].[Pledge_Campaigns] OFF;

--This command resets the identity value so that if someone adds pledge_campaigns through the UI it won't use a big ID. 
DBCC CHECKIDENT (Pledge_Campaigns, reseed, @pledge_id);

-- This is the Communication_ID for the email related to giving to a trip.
DECLARE @comm_ID as int
SET @comm_ID = 121611

--Program for (t+auto) GO Bedrock
INSERT INTO [dbo].programs 
(Program_Name,Congregation_ID,Ministry_ID,[Start_Date]                     ,End_Date                       ,Program_Type_ID,Leadership_Team,Primary_Contact,Priority_ID,On_Connection_Card,Stewardship_Information,Tax_Deductible_Donations,Statement_Title,Statement_Header_ID,Allow_Online_Giving,Online_Sort_Order,Pledge_Campaign_ID,Account_Number,Default_Target_Event,On_Donation_Batch_Tool,Domain_ID,Available_Online,__ExternalFundID,Communication_ID) VALUES
(@tripname   ,5              ,20         ,CAST(@startYear as smalldatetime),CAST(@endYear as smalldatetime),3              ,null           ,2562378        ,null       ,null              ,null                   ,1                       ,@tripName      ,2                  ,1                  ,null             ,null              ,null          ,null                ,0                     ,1        ,null            ,null            ,@comm_ID        );

--Actual Event for (t+auto) GO Bedrock
INSERT INTO [dbo].events 
(Event_Title,Event_Type_ID,Congregation_ID,Location_ID,Meeting_Instructions,[Description],Program_ID                                                    ,Primary_Contact,Participants_Expected,Minutes_for_Setup,Event_Start_Date                 ,Event_End_Date                 ,Minutes_for_Cleanup,Cancelled,_Approved,Public_Website_Settings,Visibility_Level_ID,Featured_On_Calendar,Online_Registration_Product,Registration_Form,Registration_Start,Registration_End,Registration_Active,_Web_Approved,[Check-in_Information],[Allow_Check-in],Ignore_Program_Groups,Prohibit_Guests,[Early_Check-in_Period],[Late_Check-in_Period],Other_Event_Information,Parent_Event_ID,Priority_ID,Domain_ID,On_Connection_Card,External_Registration_URL,On_Donation_Batch_Tool,__ExternalEventID,__ExternalOrganizerUserID,__ExternalGroupID,__ExternalRoomID,__ExternalContactUserID,Project_Code,Send_Reminder,Reminder_Sent,Reminder_Days_Prior_ID) VALUES
(@tripName  ,6            ,5              ,null       ,null                ,null       ,(select program_id from programs where program_name = @tripName),2562378         ,null                 ,0                ,CAST(@tripStart as smalldatetime),CAST(@endYear as smalldatetime),0                  ,0        ,1        ,null                   ,4                  ,0                   ,null                       ,null             ,null              ,null            ,null               ,1            ,null                  ,0               ,0                    ,0              ,null                   ,null                  ,null                   ,null           ,null       ,1        ,null              ,null                     ,0                     ,null             ,null                     ,null             ,null            ,null                   ,null       ,0            ,0            ,2                     );
GO

--Updates
DECLARE @tripName AS VARCHAR(24)
set @tripName = '(t+auto) GO Bedrock '+CONVERT(VARCHAR(4), datepart(year, getdate()));

update [dbo].Pledge_Campaigns set program_id = (select program_id from programs where program_name = @tripName) where campaign_name = @tripName;
update [dbo].programs set pledge_campaign_id = (select pledge_campaign_id from pledge_campaigns where campaign_name = @tripName) where program_name = @tripName;
update [dbo].Pledge_Campaigns set Event_ID = (select Event_ID from Events where Event_Title = @tripName) where campaign_name = @tripName;

--link the group to the event.
DECLARE @subGroupID as int
SET @subGroupID = (select GROUP_ID from groups where group_name = @tripName + ' (Trip Participants)');

INSERT INTO [dbo].EVENT_GROUPS
(EVENT_ID                                                   , GROUP_ID   ,Room_ID, Domain_ID) VALUES
((select event_id from events where event_title = @tripName), @subGroupID,null   , 1        );
GO
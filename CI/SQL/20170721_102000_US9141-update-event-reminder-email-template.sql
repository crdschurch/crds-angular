USE [MinistryPlatform]
GO

update dp_communications
set Subject  = 'Your Event Reminder for [Event_Title] [Action_Needed]'
where Communication_ID = 14909
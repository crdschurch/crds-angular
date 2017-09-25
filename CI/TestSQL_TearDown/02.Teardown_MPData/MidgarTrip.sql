USE [MinistryPlatform]
GO

--Retrieve name of trip in case create occurred in a year prior to teardown
DECLARE @tripName AS VARCHAR(18)
set @tripName = (select top 1 program_name from programs where program_name like '(t) GO Midgar%');

DECLARE @pledgeCampaignId as int
set @pledgeCampaignId = (select top 1 pledge_campaign_id from Pledge_Campaigns where Campaign_name = @tripName);

delete from form_response_answers where form_response_id in (select form_response_id from form_responses where event_id in (select event_id from events where event_title = @tripName));

delete from form_response_answers where form_response_id in (select form_response_id from form_responses where pledge_campaign_id  = @pledgeCampaignId);

delete from form_responses where event_id in (select event_id from events where event_title = @tripName);

delete from form_responses where pledge_campaign_id  = @pledgeCampaignId;

delete from cr_EventParticipant_documents where event_participant_ID in (select Event_Participant_ID from Event_Participants where event_id in (select event_id from events where event_title = @tripName));

delete from event_participants where event_id in (select event_id from events where event_title = @tripName);

delete from event_groups where event_id in (select event_id from events where event_title = @tripName);

delete from event_groups where group_id in (select group_id from groups where group_name = @tripName);

update Pledge_Campaigns set EVENT_ID = null, Program_id = null where Campaign_Name = @tripName;

delete from event_equipment where event_id in (select event_id from events where event_title = @tripName);

delete from events where event_title = @tripName;

--Local table to store records for cleanup
DECLARE @donationsTable table
(
	donation_id int
)

--store results in our local table
insert into @donationsTable (donation_id) (select donation_id from donation_distributions where program_id in (select program_id from programs where program_name like '(t) GO Midgar%'));

--Delete all donations matching the donation_id(s) stored in our local table
delete from donation_distributions where donation_id in (select donation_id from @donationsTable);
delete from donations where donation_id in (select donation_id from @donationsTable);

--Empty local table
delete from @donationsTable;

--store results in our local table
insert into @donationsTable (donation_id) (select donation_id from [dbo].donation_distributions where pledge_id in (select pledge_id from [dbo].pledges where pledge_campaign_id = @pledgeCampaignId));

--Delete all donations matching the donation_id(s) stored in our local table
delete from donation_distributions where donation_id in (select donation_id from @donationsTable);
delete from donations where donation_id in (select donation_id from @donationsTable);

delete from cr_Campaign_Age_Exception where pledge_campaign_id = @pledgeCampaignId;

delete from cr_Campaign_Private_Invitation where pledge_campaign_id = @pledgeCampaignId;

delete from pledges where pledge_campaign_id = @pledgeCampaignId;

delete from Group_Participants where group_id in (select group_id from groups where group_name like @tripName + '%');

delete from Program_groups where program_id in (Select program_id from programs where program_name = @tripName);

update Groups set parent_group = NULL where group_id in (select group_id from groups where group_name like @tripName + '%');

delete from Groups where group_id in (select group_id from groups where group_name like @tripName + '%');

delete from GL_Account_Mapping where program_id in (select program_id from programs where program_name = @tripName);

delete from event_rooms where event_id in (select event_id from events where program_id in (select program_id from programs where program_name = @tripName));

delete from event_groups where event_id in (select event_id from events where program_id in (select program_id from programs where program_name = @tripName));

delete from event_equipment where event_id in (select event_id from events where program_id in (select program_id from programs where program_name = @tripName));

delete from event_participants where event_id in (select event_id from events where program_id in (select program_id from programs where program_name = @tripName)); --delete any participants manually added to a related event

delete from events where program_id in (select program_id from programs where program_name = @tripName);

delete from Opportunities where Program_ID in (select Program_ID from Programs where Program_Name = @tripName);

delete from Events where program_id in (select program_id from programs where program_name = @tripName); --delete any events manually added to the program

delete from programs where program_name = @tripName;

delete from pledge_campaigns where pledge_campaign_id = @pledgeCampaignId;

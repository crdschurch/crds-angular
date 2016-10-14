﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using Crossroads.Utilities.Extensions;
using Crossroads.Utilities.Interfaces;
using MinistryPlatform.Translation.Extensions;
using MinistryPlatform.Translation.Models;
using MinistryPlatform.Translation.Repositories.Interfaces;

namespace MinistryPlatform.Translation.Repositories
{
    public class GroupParticipantRepository : IGroupParticipantRepository
    {
        public const string GetOpportunitiesForTeamStoredProc = "api_crds_Get_Opportunities_For_Team";
        private readonly IConfigurationWrapper _configurationWrapper;
        private readonly IMinistryPlatformService _ministryPlatformService;
        private readonly IApiUserRepository _apiUserService;
        private readonly IMinistryPlatformRestRepository _ministryPlatformRest;
        private readonly IGroupRepository _groupRepository;

        public GroupParticipantRepository(IConfigurationWrapper configurationWrapper,
                                       IMinistryPlatformService ministryPlatformService,
                                       IApiUserRepository apiUserService,
                                       IMinistryPlatformRestRepository ministryPlatformRest,
                                       IGroupRepository groupRepository)

        {
            _configurationWrapper = configurationWrapper;
            _ministryPlatformService = ministryPlatformService;
            _apiUserService = apiUserService;
            _ministryPlatformRest = ministryPlatformRest;
            _groupRepository = groupRepository;
        }

        public int Get(int groupId, int participantId)
        {
            var searchString = string.Format(",{0},{1}", groupId, participantId);
            var token = _apiUserService.GetToken();
            var groupParticipant = _ministryPlatformService.GetPageViewRecords("GroupParticipantsById", token, searchString).FirstOrDefault();
            return groupParticipant != null ? groupParticipant.ToInt("Group_Participant_ID") : 0;
        }

        private string MpRestEncode(string data)
        {
            return WebUtility.UrlEncode(data)?.Replace("+", "%20");
        }

        public List<MpGroupServingParticipant> GetServingParticipants(List<int> participants, long from, long to, int loggedInContactId)
        {

            var defaultDeadlinePassedMessage = _configurationWrapper.GetConfigIntValue("DefaultDeadlinePassedMessage");
            var searchFilter = "(";

            for (int i = 0; i <= participants.Count - 1; i++)
            {
                searchFilter += (i == participants.Count - 1)
                    ? "(Participant_ID=" + participants[i] + ")"
                    : "(Participant_ID=" + participants[i] + ") OR ";
            }

            var fromDate = from == 0 ? DateTime.Today : from.FromUnixTime();
            var toDate = to == 0 ? DateTime.Today.AddDays(29) : to.FromUnixTime();

            searchFilter += $") AND Event_Start_Date >= '{fromDate:yyyy-MM-dd}' AND Event_Start_Date <= '{toDate:yyyy-MM-dd}' " +
                "AND Event_Start_Date >= Participant_Start_Date AND (Event_Start_Date <= Participant_End_Date OR Participant_End_Date IS NULL)";

            //Finish out search string and call the rest backend
            var groupServingParticipants = _ministryPlatformRest.UsingAuthenticationToken(_apiUserService.GetToken()).Search<MpGroupServingParticipant>(MpRestEncode(searchFilter));

            groupServingParticipants.ForEach(p =>
                                             {
                                                 p.RowNumber = groupServingParticipants.IndexOf(p) + 1;
                                                 if (p.ContactId == loggedInContactId)
                                                     p.LoggedInUser = true;
                                                 if (p.DeadlinePassedMessage == null)
                                                     p.DeadlinePassedMessage = defaultDeadlinePassedMessage;
                                             });

            return groupServingParticipants.OrderBy(g => g.EventStartDateTime)
                .ThenBy(g => g.GroupName)
                .ThenBy(g => g.LoggedInUser == false)
                .ThenBy(g => g.ParticipantNickname)
                .ToList();
        }

        public List<MpRsvpMember> GetRsvpMembers(int groupId, int eventId)
        {
            const string COLUMNS = "Responses.opportunity_id,Responses.participant_id,Responses.event_id, opportunity_ID_Table.Group_Role_ID, Participant_ID_Table_Contact_ID_Table.NickName, Participant_ID_Table_Contact_ID_table.Last_Name,Responses.Response_Result_Id";
            string search = $"Responses.Event_ID = {eventId} And Opportunity_ID_Table.Add_To_Group = {groupId}";

            var opportunityResponse = _ministryPlatformRest.UsingAuthenticationToken(_apiUserService.GetToken()).Search<MpRsvpMember>(MpRestEncode(search), MpRestEncode(COLUMNS));

            return opportunityResponse;
        }

        public List<MpSU2SOpportunity> GetListOfOpportunitiesByEventAndGroup(int groupId, int eventId)
        {
            var parms = new Dictionary<string, object>
            {
                {"@GroupID", groupId},
                {"@EventID", eventId }
            };

            var results = _ministryPlatformRest.UsingAuthenticationToken(_apiUserService.GetToken()).GetFromStoredProc<MpSU2SOpportunity>(GetOpportunitiesForTeamStoredProc, parms);
            return results?.FirstOrDefault();
        }

        public int GetRsvpYesCount(int groupId, int eventId)
        {
            const string COLUMNS = "Count(*) As RsvpYesCount";
            string search = $"Responses.Event_ID = {eventId} And Opportunity_ID_Table.Add_To_Group = {groupId} AND Response_Result_Id = 1";

            var response = _ministryPlatformRest.UsingAuthenticationToken(_apiUserService.GetToken()).Search<MpResponse>(MpRestEncode(search), MpRestEncode(COLUMNS));

            return response[0]?.RsvpYesCount ?? 0;
        }

        public List<MpGroup> GetAllGroupsLeadByParticipant(int participantId)
        {
            const string COLUMNS = "Group_Participants.group_participant_id, Group_Participants.participant_id,  Group_Participants.group_id, Group_Participants.group_role_id";
            string search = $"Group_Participants.participant_id = {participantId}";

            var groupParticipantRecords = _ministryPlatformRest.UsingAuthenticationToken(_apiUserService.GetToken()).Search<MpGroupParticipant>(MpRestEncode(search), MpRestEncode(COLUMNS));

            List<MpGroup> groups = new List<MpGroup>();
            foreach (var groupParticipant in groupParticipantRecords)
            {
                if (groupParticipant.GroupRoleId == 22)
                {
                    var group = _groupRepository.getGroupDetails(groupParticipant.GroupId);
                    groups.Add(group);
                }
            }

            //TODO: store 22 somewhere
            return groups;
        }
    }
}
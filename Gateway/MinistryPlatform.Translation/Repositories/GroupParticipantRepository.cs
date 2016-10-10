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
        private readonly IConfigurationWrapper _configurationWrapper;
        private readonly IMinistryPlatformService _ministryPlatformService;
        private readonly IApiUserRepository _apiUserService;
        private readonly IMinistryPlatformRestRepository _ministryPlatformRest;

        public GroupParticipantRepository(IConfigurationWrapper configurationWrapper,
                                       IMinistryPlatformService ministryPlatformService,
                                       IApiUserRepository apiUserService,
                                       IMinistryPlatformRestRepository ministryPlatformRest)

        {
            _configurationWrapper = configurationWrapper;
            _ministryPlatformService = ministryPlatformService;
            _apiUserService = apiUserService;
            _ministryPlatformRest = ministryPlatformRest;
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

            groupServingParticipants.ForEach(p => p.RowNumber = groupServingParticipants.IndexOf(p) + 1);
            groupServingParticipants.Where(p => p.ContactId == loggedInContactId).All(c => c.LoggedInUser = true);

            foreach (var mpGroupServingParticipant in groupServingParticipants.Where(p => p.DeadlinePassedMessage == null))
            {
                mpGroupServingParticipant.DeadlinePassedMessage = defaultDeadlinePassedMessage;
            }

            return groupServingParticipants.OrderBy(g => g.EventStartDateTime)
                .ThenBy(g => g.GroupName)
                .ThenBy(g => g.LoggedInUser == false)
                .ThenBy(g => g.ParticipantNickname)
                .ToList();
        }
    }
}
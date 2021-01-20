using System;
using System.Collections.Generic;
using System.Linq;
using Crossroads.Utilities.Extensions;
using Crossroads.Web.Common.Configuration;
using Crossroads.Web.Common.MinistryPlatform;
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
        private readonly int _groupRoleLeader;
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
            _groupRoleLeader = _configurationWrapper.GetConfigIntValue("GroupRoleLeader");
        }

        public int Get(int groupId, int participantId)
        {
            var searchString = string.Format(",{0},{1}", groupId, participantId);
            var token = _apiUserService.GetDefaultApiClientToken();
            var groupParticipant = _ministryPlatformService.GetPageViewRecords("GroupParticipantsById", token, searchString).FirstOrDefault();
            return groupParticipant != null ? groupParticipant.ToInt("Group_Participant_ID") : 0;
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
            var toDate = to == 0 ? DateTime.Today.AddDays(43) : to.FromUnixTime();

            searchFilter += $") AND Event_Start_Date >= '{fromDate:yyyy-MM-dd}' AND Event_Start_Date <= '{toDate:yyyy-MM-dd}' " +
                            "AND Event_Start_Date >= Participant_Start_Date AND (Event_Start_Date <= Participant_End_Date OR Participant_End_Date IS NULL)";

            //Finish out search string and call the rest backend
            var groupServingParticipants = _ministryPlatformRest.UsingAuthenticationToken(_apiUserService.GetDefaultApiClientToken()).Search<MpGroupServingParticipant>(searchFilter);
            var rownum = 0;
            groupServingParticipants.ForEach(p =>
            {
                rownum++;
                p.RowNumber = rownum;
                if (p.ContactId == loggedInContactId)
                    p.LoggedInUser = true;
                if (p.DeadlinePassedMessage == null)
                    p.DeadlinePassedMessage = defaultDeadlinePassedMessage;
            });

            var returnList =  groupServingParticipants.OrderBy(g => g.EventStartDateTime)
                .ThenBy(g => g.GroupName)
                .ThenBy(g => g.LoggedInUser == false)
                .ThenBy(g => g.ParticipantNickname)
                .ToList();

            //KD
            //If we are getting the defaults (from ==0), then we should try and send the least number of weeks 
            //that we can that still has data. Up to 6 weeks
            //if we don't have anything at all or just one result then don't bother filtering out the rest
            if (to == 0 && returnList.Count > 1) 
            {
                int foundIndex = -1;
                int checkDays = 8;
                while (foundIndex == -1 && checkDays < 43)
                {
                    foundIndex = returnList.FindLastIndex(g => g.EventStartDateTime < DateTime.Today.AddDays(checkDays));
                    if (foundIndex != -1 && ++foundIndex < returnList.Count) //if found index is last then do nothing
                        returnList.RemoveRange(foundIndex, returnList.Count - foundIndex ); //keep the up to and including the found index, remove the rest
                    else
                        checkDays += 7; //check another week
                }
            }

            return returnList;
        }

        public List<MpRsvpMember> GetRsvpMembers(int groupId, int eventId)
        {
            const string COLUMNS =
                "Responses.opportunity_id,Responses.participant_id,Responses.event_id, opportunity_ID_Table.Group_Role_ID, Participant_ID_Table_Contact_ID_Table.NickName, Participant_ID_Table_Contact_ID_table.Last_Name,Responses.Response_Result_Id,Participant_ID_Table_Contact_ID_Table.__Age AS Age,Participant_ID_Table_Contact_ID_Table.Contact_ID";
            string search = $"Responses.Event_ID = {eventId} And Opportunity_ID_Table.Add_To_Group = {groupId}";

            var opportunityResponse = _ministryPlatformRest.UsingAuthenticationToken(_apiUserService.GetDefaultApiClientToken()).Search<MpRsvpMember>(search, COLUMNS);

            return opportunityResponse;
        }

        public List<MpSU2SOpportunity> GetListOfOpportunitiesByEventAndGroup(int groupId, int eventId)
        {
            var parms = new Dictionary<string, object>
            {
                {"@GroupID", groupId},
                {"@EventID", eventId}
            };

            var results = _ministryPlatformRest.UsingAuthenticationToken(_apiUserService.GetDefaultApiClientToken()).GetFromStoredProc<MpSU2SOpportunity>(GetOpportunitiesForTeamStoredProc, parms);
            return results?.FirstOrDefault();
        }

        public int GetRsvpYesCount(int groupId, int eventId)
        {
            const string COLUMNS = "Count(*) As RsvpYesCount";
            string search = $"Responses.Event_ID = {eventId} And Opportunity_ID_Table.Add_To_Group = {groupId} AND Response_Result_Id = 1";

            var response = _ministryPlatformRest.UsingAuthenticationToken(_apiUserService.GetDefaultApiClientToken()).Search<MpResponse>(search, COLUMNS);

            return response[0]?.RsvpYesCount ?? 0;
        }

        public List<MpGroup> GetAllGroupNamesLeadByParticipant(int participantId, int? groupType)
        {
            const string COLUMNS =
                "Group_ID_Table.Group_Name, Group_Participants.group_participant_id, Group_Participants.participant_id,  Group_Participants.group_id, Group_Participants.group_role_id";
            string search = $"Group_Participants.participant_id = {participantId}" +
                            $" AND Group_Role_ID = {_groupRoleLeader}" +
                            $" AND (Group_ID_Table.End_Date > '{DateTime.Now:yyyy-MM-dd H:mm:ss}' OR Group_ID_Table.End_Date Is Null)" +
                            $" AND (Group_Participants.End_Date > '{DateTime.Now:yyyy-MM-dd H:mm:ss}' OR Group_Participants.End_Date Is Null)";

            if (groupType != null)
            {
                search += $" AND Group_ID_Table.Group_Type_ID = {groupType}";
            }

            var groupParticipantRecords = _ministryPlatformRest.UsingAuthenticationToken(_apiUserService.GetDefaultApiClientToken()).Search<MpGroupParticipant>(search, COLUMNS);

            List<MpGroup> groups = new List<MpGroup>();
            foreach (var groupParticipant in groupParticipantRecords)
            {
                var group = new MpGroup()
                {
                    GroupId = groupParticipant.GroupId,
                    Name = groupParticipant.GroupName,
                };

                groups.Add(group);
            }
            return groups;
        }

        public bool GetIsLeader(int participantId, int? groupType = null, int? groupId = null)
        {
            string COLUMNS = "Group_Participants.group_role_id";
            string search = $"Group_Participants.participant_id = {participantId}" +
                            $" AND Group_Role_ID = {_groupRoleLeader}" +
                            $" AND (Group_ID_Table.End_Date > '{DateTime.Now:yyyy-MM-dd H:mm:ss}' OR Group_ID_Table.End_Date Is Null)" +
                            $" AND (Group_Participants.End_Date > '{DateTime.Now:yyyy-MM-dd H:mm:ss}' OR Group_Participants.End_Date Is Null)";

            if (groupType != null)
            {
                search += $" AND Group_ID_Table.Group_Type_ID = {groupType}";
            }

            if (groupId != null)
            {
                search += $" AND Group_Participants.GROUP_ID = {groupId}";
            }

            var mpGroupParticipants = _ministryPlatformRest.UsingAuthenticationToken(_apiUserService.GetDefaultApiClientToken()).Search<MpGroupParticipant>(search, COLUMNS);

            return mpGroupParticipants.Any();
        }

        public List<MpGroupParticipant> GetAllParticipantsForLeaderGroups(int participantId, int? groupType, int? groupId)
        {
            string csvGroupIds = "";
            if (groupId == null)
            {
                var groupIds = GetLeadersGroupIds(participantId, groupType);
                if (groupIds.Count > 0)
                    csvGroupIds = String.Join(",", groupIds.Select(g => g.GroupId.ToString()).ToArray());
            }
            else
                csvGroupIds = groupId.ToString();

            if (csvGroupIds != "")
            {
                const string columns =
                    "group_participants.participant_id, group_participants.group_role_id, Participant_ID_table_contact_id_table.Nickname," +
                    " Participant_ID_table_contact_id_table.Display_Name, Group_Role_ID_table.Role_Title, Participant_ID_table_contact_id_table.Last_name," +
                    " Participant_ID_table_contact_id_table.email_address, Participant_ID_table.contact_id";
                string search = $"group_participants.group_id in ({csvGroupIds})" +
                                $" AND (Group_Participants.End_Date > GetDate() OR Group_Participants.End_Date Is Null)";

                string orderBy = "Participant_ID_table_contact_id_table.Last_name";
                bool distinct = true;

                var mpGroupParticipants = _ministryPlatformRest.UsingAuthenticationToken(_apiUserService.GetDefaultApiClientToken())
                    .Search<MpGroupParticipant>(search, columns, orderBy, distinct);

                return mpGroupParticipants.DistinctBy(x => x.Email).ToList();
            }

            return new List<MpGroupParticipant>();
        }

        public List<MpGroupParticipant> GetLeadersGroupIds(int participantId, int? groupType)
        {
            const string groupIdColumns = "group_participants.group_id";
            string groupIdSearch = $"group_participants.participant_id = {participantId}" +
                                   $" AND group_participants.group_role_id = {_groupRoleLeader}" +
                                   $" AND (Group_ID_Table.End_Date > '{DateTime.Now:yyyy-MM-dd H:mm:ss}' OR Group_ID_Table.End_Date Is Null)" +
                                   $" AND (Group_Participants.End_Date > '{DateTime.Now:yyyy-MM-dd H:mm:ss}' OR Group_Participants.End_Date Is Null)";

            if (groupType != null)
                groupIdSearch += $" AND Group_ID_Table.Group_Type_ID = {groupType}";

            return _ministryPlatformRest.UsingAuthenticationToken(_apiUserService.GetDefaultApiClientToken()).Search<MpGroupParticipant>(groupIdSearch, groupIdColumns);
        }

        public List<MpGroupParticipant> GetGroupParticipantsRecordsForParticipant(int participantId, int groupId)
        {
            const string groupParticipantColumns = "Participant_ID, Group_Participant_ID, Group_ID";

            string groupParticipantSearch = $"Participant_ID = {participantId}" +
                                            $"AND Group_ID = {groupId}" +
                                            $"AND End_Date IS NULL";

            return _ministryPlatformRest.UsingAuthenticationToken(_apiUserService.GetDefaultApiClientToken()).Search<MpGroupParticipant>(groupParticipantSearch, groupParticipantColumns);
        }

        public void EndDateGroupParticipantRecords(List<MpGroupParticipant> groupParticipants)
        {
            var token = _apiUserService.GetDefaultApiClientToken();

            // update individually, as we don't have a batch update available
            foreach (var groupParticipant in groupParticipants)
            {
                Dictionary<string, object> updateValues = new Dictionary<string, object>()
                {
                    { "Group_Participant_ID", groupParticipant.GroupParticipantId },
                    {"End_Date", groupParticipant.EndDate }
                };

                //The second parameter is not used, you must include the PK in the dictionary
                _ministryPlatformRest.UsingAuthenticationToken(token).UpdateRecord("Group_Participants", -1, updateValues);
            }
        }
    }
}
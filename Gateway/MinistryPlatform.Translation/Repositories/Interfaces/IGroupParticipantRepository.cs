using System.Collections.Generic;
using MinistryPlatform.Translation.Models;
using MinistryPlatform.Translation.Models.Opportunities;

namespace MinistryPlatform.Translation.Repositories.Interfaces
{
    public interface IGroupParticipantRepository
    {
        int Get(int groupId, int participantId); 
        List<MpGroupServingParticipant> GetServingParticipants(List<int> participants, long from, long to, int loggedInContactId);
        List<MpGroup> GetAllGroupNamesLeadByParticipant(int participantId, int? groupType = -1);
        List<MpRsvpMember> GetRsvpMembers(int groupId, int eventId);
        List<MpSU2SOpportunity> GetListOfOpportunitiesByEventAndGroup(int groupId, int eventId);
        int GetRsvpYesCount(int groupId, int eventId);
        bool GetIsLeader(int participantId, int? groupType = null, int? groupId = null);
        List<MpGroupParticipant> GetAllParticipantsForLeaderGroups(int participantId, int? groupType, int? groupId);
        List<MpGroupParticipant> GetLeadersGroupIds(int participantId, int? groupType = -1);
        List<MpGroupParticipant> GetGroupParticipantsRecordsForParticipant(int participantId, int groupId);
        void EndDateGroupParticipantRecords(List<MpGroupParticipant> groupParticipants);
    }
}
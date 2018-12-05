using crds_angular.Models.Crossroads;
using System.Collections.Generic;
using Event = crds_angular.Models.Crossroads.Events.Event;
using crds_angular.Models.Crossroads.Groups;
using crds_angular.Models.Crossroads.Profile;
using MinistryPlatform.Translation.Models;

namespace crds_angular.Services.Interfaces
{
    public interface IGroupService
    {
        GroupDTO GetGroupDetails(int groupId);

        GroupDTO getGroupDetails(int groupId, int contactId, MpParticipant participant, string authUserToken);

        GroupDTO GetGroupDetailsByInvitationGuid(string token, string invitationGuid);

        void addParticipantToGroupNoEvents(int groupId, ParticipantSignup participant);

        void addParticipantsToGroup(int groupId, List<ParticipantSignup> participants);

        int addContactToGroup(int groupId, int contactId, int roleId);

        void endDateGroupParticipant(int groupId, int groupParticipantId);

        List<Event> GetGroupEvents(int groupId, string token = null);

        List<GroupContactDTO> GetGroupMembersByEvent(int groupId, int eventId, string recipients);
		
        GroupDTO CreateGroup(GroupDTO group);

        List<GroupDTO> GetGroupsForParticipant(string token, int participantId);

        List<GroupDTO> GetGroupsByTypeForParticipant(string token, int participantId, int groupTypeId);

        Participant GetParticipantRecord(string token);

        void SendJourneyEmailInvite(EmailCommunicationDTO email, string token);

        List<GroupParticipantDTO> GetGroupParticipants(int groupId, bool active = true);

        void LookupParticipantIfEmpty(string token, List<ParticipantSignup> partId);

        List<GroupDTO> GetGroupsForAuthenticatedUser(string token, int[] groupTypeIds);

        List<GroupDTO> GetGroupByIdForAuthenticatedUser(string token, int groupId);

        GroupDTO UpdateGroup(GroupDTO @group);

        void EndDateGroup(int groupId, int? reasonEndedId = null);

        void UpdateGroupParticipantRole(GroupParticipantDTO participant);
        void UpdateGroupParticipantRole(int groupId, int participantId, int roleId);

        void SendParticipantsEmail(string token, List<GroupParticipantDTO> participants, string subject, string body);

        List<GroupDTO> RemoveOnsiteParticipantsIfNotLeader(List<GroupDTO> groups, string token);
        List<GroupDTO> GetGroupsByTypeOrId(string token, int? participantId = null, int[] groupTypeIds = null, int? groupId = null, bool? withParticipants = true, bool? withAttributes = true);

        int GetPrimaryContactParticipantId(int groupId);

        List<GroupParticipantDTO> GetGroupParticipantsWithoutAttributes(int groupId);

        void RemoveParticipantFromGroup(string token, int groupId, int groupParticipantId);

        void SendAllGroupLeadersMemberRemovedEmail(string token, int groupId);

        void UpdateHuddleGroupParticipantStatus();

        GroupDTO GetGroupDetailsWithAttributes(int groupId);
    }
}

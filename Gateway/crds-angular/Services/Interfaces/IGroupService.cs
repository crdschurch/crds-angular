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

        GroupDTO getGroupDetails(int groupId, int contactId, MpParticipant participant);

        GroupDTO GetGroupDetailsByInvitationGuid(string invitationGuid);

        void addParticipantToGroupNoEvents(int groupId, ParticipantSignup participant);

        void addParticipantsToGroup(int groupId, List<ParticipantSignup> participants);

        int addContactToGroup(int groupId, int contactId, int roleId);

        void endDateGroupParticipant(int groupId, int groupParticipantId);

        List<Event> GetGroupEvents(int groupId);

        List<GroupContactDTO> GetGroupMembersByEvent(int groupId, int eventId, string recipients);
		
        GroupDTO CreateGroup(GroupDTO group);

        List<GroupDTO> GetGroupsForParticipant(int participantId);

        List<GroupDTO> GetGroupsByTypeForParticipant(int participantId, int groupTypeId);

  
        Participant GetParticipantRecord(int contactId);

        void SendJourneyEmailInvite(EmailCommunicationDTO email, int contactId);

        List<GroupParticipantDTO> GetGroupParticipants(int groupId, bool active = true);

        void LookupParticipantIfEmpty(int loggedInPartId, List<ParticipantSignup> partId);

        List<GroupDTO> GetGroupByIdForAuthenticatedUser(int contactId, int groupId);

        GroupDTO UpdateGroup(GroupDTO @group);

        void EndDateGroup(int groupId, int? reasonEndedId = null);

        void UpdateGroupParticipantRole(GroupParticipantDTO participant);
        void UpdateGroupParticipantRole(int groupId, int participantId, int roleId);

        void SendParticipantsEmail(int contactId, List<GroupParticipantDTO> participants, string subject, string body);

        List<GroupDTO> RemoveOnsiteParticipantsIfNotLeader(List<GroupDTO> groups, int contactId);
        List<GroupDTO> GetGroupsByTypeOrId(int contactId, int? participantId = null, int[] groupTypeIds = null, int? groupId = null, bool? withParticipants = true, bool? withAttributes = true);

        int GetPrimaryContactParticipantId(int groupId);

        List<GroupParticipantDTO> GetGroupParticipantsWithoutAttributes(int groupId);

        void RemoveParticipantFromGroup(int contactId, int groupId, int groupParticipantId);

        void SendAllGroupLeadersMemberRemovedEmail(int contactId, int groupId);

        void UpdateHuddleGroupParticipantStatus();

        GroupDTO GetGroupDetailsWithAttributes(int groupId);
    }
}

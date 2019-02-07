using System;
using System.Collections.Generic;
using Crossroads.Utilities.FunctionalHelpers;
using MinistryPlatform.Translation.Models;

namespace MinistryPlatform.Translation.Repositories.Interfaces
{
    public interface IGroupRepository
    {
        int CreateGroup(MpGroup group);

        int AddParticipantToGroup(int participantId,
                                  int groupId,
                                  int groupRoleId,
                                  bool childCareNeeded,
                                  bool employeeRole,
                                  DateTime startDate,
                                  DateTime? endDate = null,
                                  int? enrolledBy = null);

        IList<MpEvent> getAllEventsForGroup(int groupId, DateTime? minEndDate = null, bool includeCancelledEvents = false);

        MpGroup getGroupDetails(int groupId);

        bool checkIfUserInGroup(int participantId, IList<MpGroupParticipant> participants);

        bool checkIfRelationshipInGroup(int relationshipId, IList<int> currRelationshipList);

        List<MpGroupSignupRelationships> GetGroupSignupRelations(int groupType);

        bool ParticipantQualifiedServerGroupMember(int groupId, int participantId);

        bool ParticipantGroupMember(int groupId, int participantId);

        int GetParticipantGroupMemberId(int groupId, int participantId);

        List<MpGroup> GetGroupsForEvent(int eventId);

        void SendCommunityGroupConfirmationEmail(int participantId, int groupId, bool waitlist, bool childcareNeeded);

        List<MpGroupParticipant> getEventParticipantsForGroup(int groupId, int eventId);

        IList<string> GetEventTypesForGroup(int groupId, string token = null);

        List<MpGroup> GetGroupsForParticipant(int participantId);

        List<MpGroup> GetGroupsByTypeForParticipant(int participantId, int groupTypeId);

        void UpdateGroupRemainingCapacity(MpGroup group);

        List<MpGroupParticipant> GetGroupParticipants(int groupId, Boolean active);

        List<MpGroupSearchResult> GetSearchResults(int groupTypeId);

        void endDateGroupParticipant(int participantId, int groupId, DateTime? endDate = null);

        void UpdateGroupInquiry(int groupId, int inquiryId, bool approved);

        List<MpGroup> GetMyGroupParticipationByType(string token, int[] groupTypeId = null, int? groupId = null);

        void EndDateGroup(int groupId, DateTime? endDate, int? reasonEndedId);

        MpGroup GetSmallGroupDetailsById(int groupId);

        void SendNewStudentMinistryGroupAlertEmail(List<MpGroupParticipant> leaders, MpGroup group);

        int UpdateGroup(MpGroup mpGroup);
        int UpdateGroupParticipant(List<MpGroupParticipant> participants);

        int GetParticipantIdFromGroup(int groupId, string apiToken);

        void CreateGroupInquiry(MpInquiry inquiry);

        MpGroupParticipant GetAuthenticatedUserParticipationByGroupID(string token, int groupId);

        bool ParticipantGroupHasStudents(string token, int participantId, int groupParticipantId);

        bool IsMemberOfEventGroup(int contactId, int eventId, string token);

        Result<MpGroupParticipant> GetGradeGroupForContact(int contactId, string apiToken);

        List<MpGroup> GetGroupsForParticipantByTypeOrID(int participantId, string token = null, int[] groupTypeIds = null, int? groupId = null);

        List<MpGroup> GetGroupsByGroupType(int groupTypeId);
        bool GetDoesUserLeadSomeGroup(int participantId);
    }
}

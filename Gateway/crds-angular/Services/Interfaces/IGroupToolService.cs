using crds_angular.Models.Crossroads;
using System.Collections.Generic;
using System.Device.Location;
using crds_angular.Models.Crossroads.Groups;
using crds_angular.Models.Finder;
using MinistryPlatform.Translation.Models;

namespace crds_angular.Services.Interfaces
{
    public interface IGroupToolService
    {
        List<Invitation> GetInvitations(int sourceId, int invitationType, string token);
        List<Inquiry> GetInquiries(int groupId, string token);

        void RemoveParticipantFromMyGroup(int contactId, int groupId, int groupParticipantId, string message = null);
        void AcceptDenyGroupInvitation(int contactId, int groupId, string invitationGuid, bool approve);

        void SendGroupParticipantEmail(int groupId,
                                       GroupDTO group,
                                       int emailTemplateId,
                                       MpParticipant toParticipant = null,
                                       string subjectTemplateContentBlockTitle = null,
                                       string emailTemplateContentBlockTitle = null,
                                       string message = null,
                                       MpParticipant fromParticipant = null);

        MyGroup VerifyCurrentUserIsGroupLeader(string token, int groupId);
	    void SendAllGroupParticipantsEmail(string token, int groupId, int groupTypeId, string subject, string message);
        void SendAllGroupLeadersEmail(string token, int groupId, GroupMessageDTO message);
        void SubmitInquiry(int contactId, int groupId, bool sendEmail);
        void EndGroup(int groupId, int reasonEndedId);
        int SendSingleGroupParticipantEmail(GroupParticipantDTO participant, int templateId, Dictionary<string, object> mergeData);
        MyGroup GetMyGroupInfo(int contactId, int groupId);
        void SendSmallGroupPendingInquiryReminderEmails();
        List<AttributeCategoryDTO> GetGroupCategories();
        void ArchivePendingGroupInquiriesOlderThan90Days();
        List<GroupDTO> GetGroupToolGroups(string token);
        Inquiry GetGroupInquiryForContactId(int groupId, int contactId);
        string GetCurrentJourney();
    }
}

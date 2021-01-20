using System.Collections.Generic;
using MinistryPlatform.Translation.Models;

namespace MinistryPlatform.Translation.Repositories.Interfaces
{
    public interface IGroupToolRepository
    {
        List<MpInvitation> GetInvitations(int sourceId, int invitationTypeId);
        List<MpInquiry> GetInquiries(int? groupId = null);
        void ArchivePendingGroupInquiriesOlderThan90Days();
        string GetCurrentJourney();
    }
}
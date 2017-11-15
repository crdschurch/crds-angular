using System.Collections.Generic;
using MinistryPlatform.Translation.Models;
using MinistryPlatform.Translation.Models.DTO;

namespace MinistryPlatform.Translation.Repositories.Interfaces
{
    public interface IGroupToolRepository
    {
        List<MpInvitation> GetInvitations(int sourceId, int invitationTypeId);
        List<MpInquiry> GetInquiries(int? groupId = null);
        List<MpGroupSearchResultDto> SearchGroups(int[] groupTypeIds, string[] keywords = null, int? groupId = null);
        void ArchivePendingGroupInquiriesOlderThan90Days();
        string GetCurrentJourney();
    }
}
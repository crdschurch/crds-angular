using crds_angular.Models.Crossroads.Campaign;

namespace crds_angular.Services.Interfaces
{
    public interface ICampaignService
    {
        List<PledgeCampaignSummaryDto> GetSummary(int pledgeCampaignId);
    }
}

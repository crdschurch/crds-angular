using System;
using crds_angular.Services.Interfaces;
using crds_angular.Models.Crossroads.Campaign;
using crds_angular.Util.Interfaces;
using Crossroads.Web.Common.MinistryPlatform;
using MinistryPlatform.Translation.Repositories.Interfaces;

namespace crds_angular.Services
{
    public class CampaignService : ICampaignService
    {
        private readonly IApiUserRepository _apiUserService;
        private readonly ICampaignRepository _campaignRepository;
        private readonly IDateTime _dateTimeWrapper;

        public CampaignService(IApiUserRepository apiUserService, ICampaignRepository campaignRepository, IDateTime dateTimeWrapper)
        {
            _apiUserService = apiUserService;
            _campaignRepository = campaignRepository;
            _dateTimeWrapper = dateTimeWrapper;
        }

        public List<PledgeCampaignSummaryDto> GetSummary(int pledgeCampaignId)
        {
            var token = _apiUserService.GetDefaultApiClientToken();
            var campaignSummary = _campaignRepository.GetPledgeCampaignSummary(token, pledgeCampaignId);
            var pledgeCampaignSummaries = new List<PledgeCampaignSummaryDto>();

            int totalDays = DaysInRange(campaignSummary.StartDate, campaignSummary.EndDate);
            int currentDay = DaysInRange(campaignSummary.StartDate, _dateTimeWrapper.Now);

            // clip to end date
            currentDay = Math.Min(currentDay, totalDays);

            foreach(var summary in campaignSummary)
            {
                pledgeCampaignSummaries.add(
                    new PledgeCampaignSummaryDto
                    {
                        PledgeCampaignId = pledgeCampaignId,
                        TotalGiven = campaignSummary.TotalGiven + campaignSummary.NoCommitmentAmount,
                        TotalCommitted = campaignSummary.TotalCommitted,
                        CurrentDays = currentDay,
                        TotalDays = totalDays,
                        NotStartedPercent = ToPercentage(campaignSummary.NotStartedCount, campaignSummary.TotalCount),
                        BehindPercent = ToPercentage(campaignSummary.BehindCount, campaignSummary.TotalCount),
                        OnPacePercent = ToPercentage(campaignSummary.OnPaceCount, campaignSummary.TotalCount),
                        AheadPercent = ToPercentage(campaignSummary.AheadCount, campaignSummary.TotalCount),
                        CompletedPercent = ToPercentage(campaignSummary.CompletedCount, campaignSummary.TotalCount),
                    }
                    );
            }
            return pledgeCampaignSummaries;
        }

        private int DaysInRange(DateTime startDate, DateTime endDate)
        {
            int totalDays = (int) (endDate.Date - startDate.Date).TotalDays + 1;
            return Math.Max(0, totalDays);
        }

        private int ToPercentage(int numerator, int denominator)
        {
            int percent = (int) Math.Round(100.0 * numerator / denominator);
            return Math.Max(0, Math.Min(percent, 100));
        }
    }
}

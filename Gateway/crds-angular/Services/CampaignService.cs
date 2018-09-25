using System;
using System.Collections.Generic;
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

            foreach(var summary in campaignSummary)
            {
                int totalDays = DaysInRange(summary.StartDate, summary.EndDate);
                int currentDay = DaysInRange(summary.StartDate, _dateTimeWrapper.Now);

                // clip to end date
                currentDay = Math.Min(currentDay, totalDays);
                pledgeCampaignSummaries.add(
                    new PledgeCampaignSummaryDto
                    {
                        PledgeCampaignId = pledgeCampaignId,
                        TotalGiven = summary.TotalGiven + summary.NoCommitmentAmount,
                        TotalCommitted = summary.TotalCommitted,
                        CurrentDays = currentDay,
                        TotalDays = totalDays,
                        NotStartedPercent = ToPercentage(summary.NotStartedCount, summary.TotalCount),
                        BehindPercent = ToPercentage(summary.BehindCount, summary.TotalCount),
                        OnPacePercent = ToPercentage(summary.OnPaceCount, summary.TotalCount),
                        AheadPercent = ToPercentage(summary.AheadCount, summary.TotalCount),
                        CompletedPercent = ToPercentage(summary.CompletedCount, summary.TotalCount),
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

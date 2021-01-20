using Newtonsoft.Json;

namespace crds_angular.Models.Crossroads.Campaign
{
    public class PledgeCampaignSummaryDto
    {
        [JsonProperty(PropertyName = "pledgeCampaignId")]
        public int PledgeCampaignId { get; set; }

        [JsonProperty(PropertyName = "totalGiven")]
        public decimal TotalGiven { get; set; }

        [JsonProperty(PropertyName = "totalCommitted")]
        public decimal TotalCommitted { get; set; }

        [JsonProperty(PropertyName = "currentDay")]
        public int CurrentDays { get; set; }

        [JsonProperty(PropertyName = "totalDays")]
        public int TotalDays { get; set; }

        [JsonProperty(PropertyName = "notStartedPercent")]
        public int NotStartedPercent { get; set; }

        [JsonProperty(PropertyName = "behindPercent")]
        public int BehindPercent { get; set; }

        [JsonProperty(PropertyName = "onPacePercent")]
        public int OnPacePercent { get; set; }

        [JsonProperty(PropertyName = "aheadPercent")]
        public int AheadPercent { get; set; }

        [JsonProperty(PropertyName = "completedPercent")]
        public int CompletedPercent { get; set; }
    }
}

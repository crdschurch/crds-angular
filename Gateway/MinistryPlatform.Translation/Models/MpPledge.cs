using System;
using Crossroads.Web.Common.MinistryPlatform;
using Newtonsoft.Json;

namespace MinistryPlatform.Translation.Models
{
    [MpRestApiTable(Name = "Pledges")]
    public class MpPledge
    {
        [JsonProperty(PropertyName = "Pledge_ID")]
        public int PledgeId { get; set; }

        [JsonProperty(PropertyName = "Pledge_Campaign_ID")]
        public int PledgeCampaignId { get; set; }

        [JsonProperty(PropertyName = "Donor_ID")]
        public int DonorId { get; set; }

        [JsonProperty(PropertyName = "Pledge_Status_ID")]
        public int PledgeStatusId { get; set; }

        [JsonProperty(PropertyName = "Pledge_Status")]
        public string PledgeStatus { get; set; }

        [JsonProperty(PropertyName = "Campaign_Name")]
        public string CampaignName { get; set; }

        [JsonProperty(PropertyName = "Pledge_Total")]
        public decimal PledgeTotal { get; set; }

        [JsonProperty(PropertyName = "Pledge_Donations")]
        public decimal PledgeDonations { get; set; }

        [JsonProperty(PropertyName = "Campaign_Start_Date")]
        public DateTime CampaignStartDate { get; set; }

        [JsonProperty(PropertyName = "Campaign_End_Date")]
        public DateTime CampaignEndDate { get; set; }

        [JsonProperty(PropertyName = "Campaign_Type_ID")]
        public int CampaignTypeId { get; set; }

        [JsonProperty(PropertyName = "Campaign_Type_Name")]
        public string CampaignTypeName { get; set; }
    }
}
using Crossroads.Web.Common.MinistryPlatform;
using Newtonsoft.Json;

namespace MinistryPlatform.Translation.Models
{
    [MpRestApiTable(Name = "MpTripRecord")]
    public class MpTripRecord
    {
        [JsonProperty(PropertyName = "Destination_ID")]
        public int CampaignDestinationId { get; set; }

        [JsonProperty(PropertyName = "Fundraising_Goal")]
        public decimal CampaignFundRaisingGoal { get; set; }

        [JsonProperty(PropertyName = "Group_ID")]
        public int GroupId { get; set; }

        [JsonProperty(PropertyName = "Registration_Deposit")]
        public decimal RegistrationDeposit { get; set; }
    }
}

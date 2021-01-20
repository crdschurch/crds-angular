using Newtonsoft.Json;

namespace crds_angular.Controllers.API
{
    public class UpdateDonorDTO
    {
        [JsonProperty(PropertyName = "stripe_token_id")]
        public string StripeTokenId { get; set; }
        [JsonProperty(PropertyName = "donor_id")]
        public string DonorId { get; set; }
        [JsonProperty(PropertyName = "email_address")]
        public string EmailAddress { get; set; }
    }
}

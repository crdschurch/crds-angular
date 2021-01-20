using MinistryPlatform.Translation.Models;
using Newtonsoft.Json;

namespace crds_angular.Models.Crossroads.Stewardship
{
    public class EZScanDonorDetails
    {
        [JsonProperty(PropertyName = "DisplayName")]
        public string DisplayName { get; set; }
        [JsonProperty(PropertyName = "DonorId")]
        public string DonorId { get; set; }
        [JsonProperty(PropertyName = "PostalAddress")]
        public MpPostalAddress Address { get; set; }
    }
}
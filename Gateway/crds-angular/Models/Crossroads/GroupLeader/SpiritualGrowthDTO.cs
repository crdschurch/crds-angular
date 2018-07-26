using Newtonsoft.Json;

namespace crds_angular.Models.Crossroads.GroupLeader
{
    public class SpiritualGrowthDTO
    {
        [JsonProperty(PropertyName = "contactId")]
        public int ContactId { get; set; }

        [JsonProperty(PropertyName = "email")]
        public string EmailAddress { get; set; }

        [JsonProperty(PropertyName = "openResponse1")]
        public string OpenResponse1 { get; set; }

        [JsonProperty(PropertyName = "openResponse2")]
        public string OpenResponse2 { get; set; }

        [JsonProperty(PropertyName = "openResponse3")]
        public string OpenResponse3 { get; set; }

        [JsonProperty(PropertyName = "openResponse4")]
        public string OpenResponse4 { get; set; }
    }
}
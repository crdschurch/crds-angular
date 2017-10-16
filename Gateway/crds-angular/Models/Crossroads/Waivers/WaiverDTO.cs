using System;
using Newtonsoft.Json;

namespace crds_angular.Models.Crossroads.Waivers
{
    public class WaiverDTO
    {
        [JsonProperty(PropertyName = "waiverId")]
        public int WaiverId { get; set; }

        [JsonProperty(PropertyName = "waiverName")]
        public string WaiverName { get; set; }

        [JsonProperty(PropertyName = "waiverText")]
        public string WaiverText { get; set; }

        [JsonProperty(PropertyName = "waiverRequired")]
        public bool Required { get; set; }
        
        [JsonProperty(PropertyName = "accepted")]
        public bool Accepted { get; set; }

        [JsonProperty(PropertyName = "signee")]
        public int SigneeContactId { get; set; }

        [JsonProperty(PropertyName = "waiverStartDate")]
        public DateTime? WaiverStartDate { get; set; }

        [JsonProperty(PropertyName = "waiverEndDate")]
        public DateTime? WaiverEndDate { get; set; }
    }
}
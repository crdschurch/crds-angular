using crds_angular.Models.Crossroads;
using Newtonsoft.Json;

namespace crds_angular.Models.Finder
{
    public class MeDTO
    {
        [JsonProperty("address")]
        public AddressDTO Address { get; set; }

        [JsonProperty("congregationid")]
        public int? CongregationId { get; set; }

        [JsonProperty("showonmap")]
        public bool ShowOnMap { get; set; }

        [JsonProperty("participantid")]
        public int? ParticipantId { get; set; }
    }
}


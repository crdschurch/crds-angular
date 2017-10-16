using Newtonsoft.Json;

namespace crds_angular.Models.Crossroads.Trip
{
    public class TripDocument
    {
        [JsonProperty(PropertyName = "eventParticipantDocumentId")]
        public int EventParticipantDocumentId { get; set; }

        [JsonProperty(PropertyName = "tripName")]
        public string TripName { get; set; }

        [JsonProperty(PropertyName = "documentId")]
        public int DocumentId { get; set; }

        [JsonProperty(PropertyName = "eventParticipantId")]
        public int EventParticipantId { get; set; }

        [JsonProperty(PropertyName = "documentReceived")]
        public bool Received { get; set; }

        [JsonProperty(PropertyName = "documentNotes")]
        public string Notes { get; set; }
    }
}
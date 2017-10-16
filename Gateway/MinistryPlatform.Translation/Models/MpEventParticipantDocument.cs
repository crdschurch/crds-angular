using Crossroads.Web.Common.MinistryPlatform;
using Newtonsoft.Json;

namespace MinistryPlatform.Translation.Models
{
    [MpRestApiTable(Name = "cr_EventParticipant_Documents")]
    public class MpEventParticipantDocument
    {
        [JsonProperty(PropertyName = "EventParticipant_Document_ID")]
        public int EventParticipantDocumentId { get; set; }

        [JsonProperty(PropertyName = "Event_Title")]
        public string EventTitle { get; set; }

        [JsonProperty(PropertyName = "Event_Participant_ID")]
        public int EventParticipantId { get; set; }

        [JsonProperty(PropertyName = "Document_ID")]
        public int DocumentId { get; set; }

        [JsonProperty(PropertyName = "Received")]
        public bool Received { get; set; }

        [JsonProperty(PropertyName = "Notes")]
        public string Notes { get; set; }
    }
}

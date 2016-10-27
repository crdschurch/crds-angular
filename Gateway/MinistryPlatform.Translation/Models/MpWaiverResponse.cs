﻿using MinistryPlatform.Translation.Models.Attributes;
using Newtonsoft.Json;

namespace MinistryPlatform.Translation.Models
{
    [MpRestApiTable(Name = "cr_Event_Participant_Waivers")]
    public class MpWaiverResponse
    {
        [JsonProperty(PropertyName = "Waiver_ID")]
        public int WaiverId { get; set; }

        [JsonProperty(PropertyName = "Event_Participant_ID")]
        public int EventParticipantId { get; set; }

        [JsonProperty(PropertyName = "Accepted")]
        public bool Accepted { get; set; }

        [JsonProperty(PropertyName = "Signee_Contact_ID")]
        public int SigneeContactId { get; set; }
    }
}

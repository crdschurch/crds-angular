using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Crossroads.Web.Common.MinistryPlatform;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;

namespace MinistryPlatform.Translation.Models
{
    [MpRestApiTable(Name = "cr_Event_Participant_Waivers")]
    public class MpEventParticipantWaiver
    {
        [JsonProperty(PropertyName = "Event_Participant_Waiver_ID")]
        public int EventParticipantWaiverId { get; set; }

        [JsonProperty(PropertyName = "Event_Participant_ID")]
        public int EventParticipantId { get; set; }

        [JsonProperty(PropertyName = "Waiver_ID")]
        public int WaiverId { get; set; }

        [JsonProperty(PropertyName = "Accepted")]
        public bool Accepted { get; set; }

        [JsonProperty(PropertyName = "Signee_Contact_ID")]
        public int SignerId { get; set; }
    }
}

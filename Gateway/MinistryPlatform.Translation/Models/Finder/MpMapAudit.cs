using Crossroads.Web.Common.MinistryPlatform;
using Newtonsoft.Json;
using System;

namespace MinistryPlatform.Translation.Models.Finder
{
    
    [MpRestApiTable(Name = "cr_MapAudit")]
    public class MpMapAudit
    {
        [JsonProperty(PropertyName = "AuditID")]
        public int AuditId { get; set; }

        [JsonProperty(PropertyName = "Participant_ID")]
        public int ParticipantId { get; set; }

        [JsonProperty(PropertyName = "ShowOnMap")]
        public bool showOnMap { get; set; }

        [JsonProperty(PropertyName = "Processed")]
        public bool processed { get; set; }

        [JsonProperty(PropertyName = "DateProcessed")]
        public DateTime? dateProcessed { get; set; }

        [JsonProperty(PropertyName = "PinType")]
        public string pinType { get; set; }
    }
    
}

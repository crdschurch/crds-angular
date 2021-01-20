using System;
using Crossroads.Web.Common.MinistryPlatform;
using Newtonsoft.Json;

namespace MinistryPlatform.Translation.Models
{
    [MpRestApiTable(Name = "Responses")]
    public class MpResponse
    {
        public int Response_ID { get; set; }
        public DateTime Response_Date { get; set; }
        public int Opportunity_ID { get; set; }
        public int Participant_ID { get; set; }
        public bool Closed { get; set; }
        public string Comments { get; set; }
        public int? Response_Result_ID { get; set; }
        [JsonProperty(PropertyName = "Event_ID")]
        public int Event_ID { get; set; }
        //public DateTime Opportunity_Date { get; set; }

        [JsonProperty(PropertyName = "RsvpYesCount")]
        public int RsvpYesCount { get; set; } = 0;
    }
}

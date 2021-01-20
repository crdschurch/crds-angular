using Crossroads.Web.Common.MinistryPlatform;
using Newtonsoft.Json;

namespace MinistryPlatform.Translation.Models
{
    [MpRestApiTable(Name = "cr_Event_Waivers")]
    public class MpEventWaivers
    {
        [JsonProperty(PropertyName = "Waiver_ID")]
        public int WaiverId { get; set; }

        [JsonProperty(PropertyName = "Waiver_Name")]
        public string WaiverName { get; set; }

        [JsonProperty(PropertyName = "Waiver_Text")]
        public string WaiverText { get; set; }

        [JsonProperty(PropertyName = "Required")]
        public bool Required { get; set; }

        [JsonProperty(PropertyName = "Accepted")]
        public bool Accepted { get; set; }

        [JsonProperty(PropertyName = "Signee_Contact_ID")]
        public int SigneeContactId { get; set; }
    }
}

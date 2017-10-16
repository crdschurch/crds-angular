using System;
using Crossroads.Web.Common.MinistryPlatform;
using Newtonsoft.Json;

namespace MinistryPlatform.Translation.Models
{
    [MpRestApiTable(Name = "cr_Waivers")]
    public class MpWaivers
    {
        [JsonProperty(PropertyName = "Waiver_ID")]
        public int WaiverId { get; set; }

        [JsonProperty(PropertyName = "Waiver_Name")]
        public string WaiverName { get; set; }

        [JsonProperty(PropertyName = "Waiver_Text")]
        public string WaiverText { get; set; }

        [JsonProperty(PropertyName = "Waiver_Start_Date")]
        public DateTime? WaiverStartDate { get; set; }

        [JsonProperty(PropertyName = "Waiver_End_Date")]
        public DateTime? WaiverEndDate { get; set; }
    }
}

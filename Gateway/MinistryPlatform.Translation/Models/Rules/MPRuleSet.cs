using System;
using Crossroads.Web.Common.MinistryPlatform;
using Newtonsoft.Json;

namespace MinistryPlatform.Translation.Models.Rules
{
    [MpRestApiTable(Name = "cr_Ruleset")]
    public class MPRuleSet
    {
        [JsonProperty(PropertyName = "Ruleset_ID")]
        public int Id { get; set; }

        [JsonProperty(PropertyName = "Ruleset_Name")]
        public string Name { get; set; }

        [JsonProperty(PropertyName = "Description")]
        public string Description { get; set; }

        [JsonProperty(PropertyName = "Ruleset_Start_Date")]
        public DateTime StartDate { get; set; }

        [JsonProperty(PropertyName = "Ruleset_End_Date")]
        public DateTime EndDate { get; set; }
    }
}

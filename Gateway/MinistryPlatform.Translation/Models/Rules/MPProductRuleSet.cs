using System;
using Crossroads.Web.Common.MinistryPlatform;
using Newtonsoft.Json;

namespace MinistryPlatform.Translation.Models.Rules
{
    [MpRestApiTable(Name = "cr_Product_Ruleset")]
    public class MPProductRuleSet
    {
        [JsonProperty(PropertyName = "Product_ID")]
        public int ProductId { get; set; }

        [JsonProperty(PropertyName = "Ruleset_ID")]
        public int RulesetId { get; set; }

        [JsonProperty(PropertyName = "Start_Date")]
        public DateTime? StartDate { get; set; }

        [JsonProperty(PropertyName = "End_Date")]
        public DateTime? EndDate { get; set; }
    }
}
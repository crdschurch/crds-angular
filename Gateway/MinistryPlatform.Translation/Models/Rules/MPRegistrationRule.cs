﻿using MinistryPlatform.Translation.Models.Attributes;
using Newtonsoft.Json;

namespace MinistryPlatform.Translation.Models.Rules
{
    [MpRestApiTable(Name = "cr_Rule_Registrations")]
    public class MPRegistrationRule : MPRuleBase
    {
        [JsonProperty(PropertyName = "Minimum_Registrants")]
        public int? MinimumRegistrants { get; set; }

        [JsonProperty(PropertyName = "Maximum_Registrants")]
        public int MaximumRegistrants { get; set; }
    }
}

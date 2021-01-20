using Crossroads.Web.Common.MinistryPlatform;
using Newtonsoft.Json;

namespace MinistryPlatform.Translation.Models.Rules
{
    [MpRestApiTable(Name = "cr_Rule_Genders")]
    public class MPGenderRule : MPRuleBase
    {
        [JsonProperty(PropertyName = "Gender_ID")]
        public int AllowedGenderId { get; set; }
    }
}

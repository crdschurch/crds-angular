using Crossroads.Web.Common.MinistryPlatform;
using Newtonsoft.Json;

namespace MinistryPlatform.Translation.Models
{
    [MpRestApiTable(Name = "Congregations")]
    public class MpCongregation
    {
        [JsonProperty(PropertyName = "Congregation_ID")]
        public int CongregationId { get; set; }

        [JsonProperty(PropertyName = "Congregation_Name")]
        public string Name { get; set; }

        [JsonProperty(PropertyName = "Location_ID")]
        public int LocationId { get; set; }
    }

}

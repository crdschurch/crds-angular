using Crossroads.Web.Common.MinistryPlatform;
using Newtonsoft.Json;

namespace MinistryPlatform.Translation.Models
{
    [MpRestApiTable(Name = "Attributes")]
    public class MpRestAttribute
    {
        [JsonProperty(PropertyName = "Attribute_ID")]
        public int AttributeId { get; set; }

        [JsonProperty(PropertyName = "Attribute_Name")]
        public string Name { get; set; }

        [JsonProperty(PropertyName = "Description")]
        public string Description { get; set; }

        [JsonProperty(PropertyName = "Attribute_Category_ID")]
        public int? CategoryId { get; set; }

        [JsonProperty(PropertyName = "Attribute_Type_ID")]
        public int AttributeTypeId { get; set; }

        [JsonProperty(PropertyName = "Sort_Order")]
        public int SortOrder { get; set; }
    }
}

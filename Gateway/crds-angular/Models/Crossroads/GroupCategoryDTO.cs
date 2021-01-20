using Newtonsoft.Json;
using crds_angular.Models.Crossroads.Attribute;

namespace crds_angular.Models.Crossroads
{
    public class AttributeCategoryDTO
    {
        [JsonProperty(PropertyName = "categoryId")]
        public int CategoryId { get; set; }

        [JsonProperty(PropertyName = "attribute")]
        public AttributeDTO Attribute { get; set; }

        [JsonProperty(PropertyName = "desc")]
        public string Description { get; set; }

        [JsonProperty(PropertyName = "exampleText")]
        public string ExampleText { get; set; }

        [JsonProperty(PropertyName = "requiresActiveAttribute")]
        public bool RequiresActiveAttribute { get; set; }

        [JsonProperty(PropertyName = "name")]
        public string AttributeCategory { get; set; }
    }
}
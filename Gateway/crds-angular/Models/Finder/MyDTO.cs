using Newtonsoft.Json;

namespace crds_angular.Models.Finder
{
    public class MyDTO
    {
        [JsonProperty("internalid")]
        public int InternalId { get; set; }

        [JsonProperty("pintypeid")]
        public int PinTypeId { get; set; }
    }
}
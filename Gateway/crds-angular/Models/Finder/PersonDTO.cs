using Newtonsoft.Json;

namespace crds_angular.Models.Finder
{
    public class PersonDTO
    {
        [JsonProperty("name")]
        public string Name { get; set; }

        [JsonProperty("location")]
        public string Location { get; set; }
    }
}
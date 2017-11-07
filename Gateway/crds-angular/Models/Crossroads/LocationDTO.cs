using Newtonsoft.Json;

namespace crds_angular.Models.Crossroads
{
    public class LocationDTO
    {
        [JsonProperty(PropertyName = "locationId")]
        public int LocationId { get; set; }

        [JsonProperty(PropertyName = "location")]
        public string LocationName { get; set; }

        [JsonProperty(PropertyName = "imageUrl")]
        public string ImageUrl { get; set; }

        [JsonProperty(PropertyName = "address")]
        public AddressDTO Address { get; set; }
    }
}
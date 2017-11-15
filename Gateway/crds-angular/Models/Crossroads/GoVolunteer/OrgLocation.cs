using MinistryPlatform.Translation.Models;
using Newtonsoft.Json;

namespace crds_angular.Models.Crossroads.GoVolunteer
{
    public class OrgLocation
    {
        [JsonProperty(PropertyName = "locationId")]
        public int LocationId { get; set; }

        [JsonProperty(PropertyName = "location")]
        public string LocationName { get; set; }

        [JsonProperty(PropertyName = "address")]
        public string Address { get; set; }

        [JsonProperty(PropertyName = "city")]
        public string City { get; set; }

        [JsonProperty(PropertyName = "state")]
        public string State { get; set; }

        [JsonProperty(PropertyName = "zip")]
        public string Zip { get; set; }

        [JsonProperty(PropertyName = "imageUrl")]
        public string ImageUrl { get; set; }

        public OrgLocation FromMpLocation(MpOrgLocation loc)
        {
            return new OrgLocation
            {
                LocationId = loc.LocationId,
                LocationName = loc.LocationName,
                Address = loc.Address,
                City = loc.City,
                State = loc.State,
                Zip = loc.Zip,
                ImageUrl = loc.ImageUrl
            };
            
        }
    }
}
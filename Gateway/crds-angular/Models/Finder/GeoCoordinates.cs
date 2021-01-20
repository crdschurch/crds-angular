using Newtonsoft.Json;

namespace crds_angular.Models.Finder
{
    public class GeoCoordinates
    {
        [JsonProperty("lat")]
        public double? Lat { get; set; }

        [JsonProperty("lng")]
        public double? Lng { get; set; }

        public GeoCoordinates(double? lat, double? lng)
        {
            this.Lat = lat;
            this.Lng = lng; 
        }
    }
}
using Crossroads.Web.Common.MinistryPlatform;
using Newtonsoft.Json;

namespace MinistryPlatform.Translation.Models
{
    [MpRestApiTable(Name = "Locations")]
    public class MpLocation
    {
        [JsonProperty(PropertyName = "Location_ID")]
        public int LocationId { get; set; }

        [JsonProperty(PropertyName = "Location_Name")]
        public string LocationName { get; set; }

        [JsonProperty(PropertyName = "Location_Type_ID")]
        public int LocationTypeId { get; set; }

        [JsonProperty(PropertyName = "Address_ID")]
        public int AddressId { get; set; }

        [JsonProperty(PropertyName = "Address_Line_1")]
        public string AddressLine1 { get; set; }

        [JsonProperty(PropertyName = "Address_Line_2")]
        public string AddressLine2 { get; set; }

        [JsonProperty(PropertyName = "City")]
        public string City { get; set; }

        [JsonProperty(PropertyName = "State/Region")]
        public string State { get; set; }

        [JsonProperty(PropertyName = "Postal_Code")]
        public string Zip { get; set; }

        [JsonProperty(PropertyName = "Foreign_Country")]
        public string ForeignCountry { get; set; }

        [JsonProperty(PropertyName = "County")]
        public string County { get; set; }

        [JsonProperty(PropertyName = "Image_URL")]
        public string ImageUrl { get; set; }

        [JsonProperty(PropertyName = "Longitude")]
        public double? Longitude { get; set; }

        [JsonProperty(PropertyName = "Latitude")]
        public double? Latitude { get; set; }
        

    }
}
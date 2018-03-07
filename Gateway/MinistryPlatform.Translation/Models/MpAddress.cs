using Crossroads.Web.Common.MinistryPlatform;
using Newtonsoft.Json;

namespace MinistryPlatform.Translation.Models
{
    [MpRestApiTable(Name = "Addresses")]
    public class MpAddress
    {
        public int? Address_ID { get; set; }
        public string Address_Line_1 { get; set; }
        public string Address_Line_2 { get; set; }
        public string City { get; set; }

        [JsonProperty(PropertyName = "State/Region")]
        public string State { get; set; }

        public string Postal_Code { get; set; }
        public string Foreign_Country { get; set; }
        public string County { get; set; }
        public double? Longitude { get; set; }
        public double? Latitude { get; set; }
    }
}
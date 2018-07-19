using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Newtonsoft.Json;

namespace MinistryPlatform.Translation.Models.Finder
{
    public class SpPinDto
    {
        [JsonProperty("first_Name")]
        public string First_Name { get; set; }

        [JsonProperty("last_Name")]
        public string Last_Name { get; set; }

        [JsonProperty("contact_Id")]
        public int? Contact_ID { get; set; }

        [JsonProperty("participant_Id")]
        public int? Participant_ID { get; set; }

        [JsonProperty("hostStatusId")]
        public int? Host_Status_ID { get; set; }

        [JsonProperty("gathering")]
        public string Gathering { get; set; }

        [JsonProperty("household_Id")]
        public int? Household_ID { get; set; }

        [JsonProperty("site_Name")]
        public string Site_Name{ get; set; }

        [JsonProperty("address_line_1")]
        public string Address_Line_1 { get; set; }

        [JsonProperty("address_line_2")]
        public string Address_Line_2 { get; set; }

        [JsonProperty("city")]
        public string City { get; set; }

        [JsonProperty("state")]
        public string State { get; set; }

        [JsonProperty("postal_Code")]
        public string Postal_Code { get; set; }

        [JsonProperty("latitude")]
        public double? Latitude { get; set; }

        [JsonProperty("Longitude")]
        public double? Longitude { get; set; }

        [JsonProperty("pin_Type")]
        public int Pin_Type { get; set; }
    }
}

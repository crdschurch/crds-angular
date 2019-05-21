using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Newtonsoft.Json;

namespace crds_angular.Models.Crossroads
{
    public class User
    {
        [JsonProperty(PropertyName = "firstname")]
        public string firstName { get; set; }

        [JsonProperty(PropertyName = "lastname")]
        public string lastName { get; set; }

        [JsonProperty(PropertyName = "email")]
        public string email { get; set; }

        [JsonProperty(PropertyName = "password")]
        public string password { get; set; }

        [JsonProperty(PropertyName = "contactId")]
        public int? contactId { get; set; }
    }
}
﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using crds_angular.Models.Crossroads.GoVolunteer;
using Newtonsoft.Json;

namespace crds_angular.Models.Crossroads
{
    public class LocationProximityDto
    {
        [JsonProperty("origin")]
        public string Origin { get; set; }

        [JsonProperty("location")]
        public OrgLocation Location { get; set; }

        [JsonProperty("distance")]
        public decimal? Distance { get; set; }

    }
}
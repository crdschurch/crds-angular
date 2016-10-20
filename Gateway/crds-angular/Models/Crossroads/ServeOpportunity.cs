﻿using System.Collections.Generic;
using System.Security.Cryptography.X509Certificates;
using Newtonsoft.Json;

namespace crds_angular.Models.Crossroads
{
    public class ServeOpportunity
    {
        [JsonProperty(PropertyName = "Opportunity_ID")]
        public int OpportunityId { get; set; }

        [JsonProperty(PropertyName = "Opportunity_Title")]
        public string OpportunityTitle { get; set; }

        [JsonProperty(PropertyName = "rsvpMembers")]
        public List<RsvpMembers> RsvpMembers { get; set; }

        [JsonProperty(PropertyName = "Group_Role_ID")]
        public int Group_Role_ID { get; set; }

        [JsonProperty(PropertyName = "roleTitle")]
        public string RoleTitle { get; set; }

        public ServeOpportunity()
        {
            RsvpMembers = new List<RsvpMembers>(); 
        }
    }
}
using System.Collections.Generic;
using Newtonsoft.Json;

namespace crds_angular.Models.Crossroads
{
    public class ServeOpportunity
    {
        [JsonProperty(PropertyName = "opportunityId")]
        public int OpportunityId { get; set; }

        [JsonProperty(PropertyName = "opportunityTitle")]
        public string OpportunityTitle { get; set; }

        [JsonProperty(PropertyName = "rsvpMembers")]
        public List<RsvpMember> RsvpMembers { get; set; }

        [JsonProperty(PropertyName = "groupRoleId")]
        public int Group_Role_ID { get; set; }

        [JsonProperty(PropertyName = "roleTitle")]
        public string RoleTitle { get; set; }

        [JsonProperty(PropertyName = "shiftStartTime")]
        public string ShiftStartTime { get; set; }

        [JsonProperty(PropertyName = "shiftEndTime")]
        public string ShiftEndTime { get; set; }

        [JsonProperty(PropertyName = "room")]
        public string Room { get; set; }

        [JsonProperty(PropertyName = "minimum")]
        public int? MinimumNeeded { get; set; }

        [JsonProperty(PropertyName = "maximum")]
        public int? MaximumNeeded { get; set; }
 
        public ServeOpportunity()
        {
            RsvpMembers = new List<RsvpMember>(); 
        }
    }
}
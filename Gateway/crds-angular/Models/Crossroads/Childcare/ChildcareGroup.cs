using System;
using System.Collections.Generic;
using Newtonsoft.Json;

namespace crds_angular.Models.Crossroads.Childcare
{
    public class ChildcareGroup
    {
        [JsonProperty(PropertyName = "eventStartTime")]
        public DateTime EventStartTime { get; set; }

        [JsonProperty(PropertyName = "eventEndTime")]
        public DateTime EventEndTime { get; set; }

        [JsonProperty(PropertyName = "groupMemberName")]
        public string GroupMemberName { get; set; }

        [JsonProperty(PropertyName = "groupName")]
        public string GroupName { get; set; }

        [JsonProperty(PropertyName = "congregationId")]
        public int CongregationId { get; set; }

        [JsonProperty(PropertyName = "maxAge")]
        public int MaximumAge { get; set; }

        [JsonProperty(PropertyName = "remainingCapacity")]
        public int RemainingCapacity { get; set; }

        [JsonProperty(PropertyName = "eligibleChildren")]
        public List<ChildcareRsvp> EligibleChildren { get; set; }

        [JsonProperty(PropertyName = "childcareGroupId")]
        public int ChildcareGroupId { get; set; }

        [JsonProperty(PropertyName = "groupParticipantId")]
        public int GroupParticipantId { get; set; }

        public ChildcareGroup()
        {
            EligibleChildren = new List<ChildcareRsvp>();
        }
    }
}
using System.Collections.Generic;
using crds_angular.Models.Crossroads.Attribute;
using Newtonsoft.Json;

namespace crds_angular.Models.Crossroads
{
    public class ParticipantSignup
    {
        [JsonProperty(PropertyName = "participantId")]
        public int? particpantId { get; set; }

        [JsonProperty(PropertyName = "childCareNeeded")]
        public bool childCareNeeded { get; set; }

        [JsonProperty(PropertyName = "groupRoleId")]
        public int? groupRoleId { get; set; }

        [JsonProperty(PropertyName = "capacityNeeded")]
        public int capacityNeeded { get; set; }

        [JsonProperty(PropertyName = "sendConfirmationEmail")]
        public bool SendConfirmationEmail { get; set; }

        [JsonProperty(PropertyName = "attributeTypes")]
        public Dictionary<int, ObjectAttributeTypeDTO> AttributeTypes { get; set; }

        [JsonProperty(PropertyName = "singleAttributes")]
        public Dictionary<int, ObjectSingleAttributeDTO> SingleAttributes { get; set; }

        [JsonProperty(PropertyName = "enrolledBy")]
        public int? EnrolledBy { get; set; }
    }

}
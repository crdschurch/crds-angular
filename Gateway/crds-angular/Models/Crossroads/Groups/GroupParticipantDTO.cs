using System;
using System.Collections.Generic;
using crds_angular.Models.Crossroads.Attribute;
using Newtonsoft.Json;

namespace crds_angular.Models.Crossroads.Groups
{

    public class GroupParticipantDTO
    {
        [JsonProperty(PropertyName = "participantId")]
        public int ParticipantId { get; set; }

        [JsonProperty(PropertyName = "contactId")]
        public int ContactId { get; set; }

        [JsonProperty(PropertyName = "groupParticipantId")]
        public int GroupParticipantId { get; set; }

        [JsonProperty(PropertyName = "nickName")]
        public string NickName { get; set; }

        [JsonProperty(PropertyName = "lastName")]
        public string LastName { get; set; }

        [JsonProperty(PropertyName = "groupRoleId")]
        public int GroupRoleId { get; set; }

        [JsonProperty(PropertyName = "groupRoleTitle")]
        public string GroupRoleTitle { get; set; }

        [JsonProperty(PropertyName = "email")]
        public string Email { get; set; }

        [JsonProperty(PropertyName = "startDate")]
        public DateTime? StartDate { get; set; }

        [JsonProperty(PropertyName = "congregation")]
        public string Congregation { get; set; }

        [JsonProperty(PropertyName="isApprovedLeader")]
        public bool IsApprovedSmallGroupLeader { get; set; }

        [JsonProperty(PropertyName = "attributeTypes")]
        public Dictionary<int, ObjectAttributeTypeDTO> AttributeTypes { get; set; }

        [JsonProperty(PropertyName = "singleAttributes")]        
        public Dictionary<int, ObjectSingleAttributeDTO> SingleAttributes { get; set; }

        [JsonProperty(PropertyName = "displayName")]
        public string DisplayName { get; set; }

    }
}

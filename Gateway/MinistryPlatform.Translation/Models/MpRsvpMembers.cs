using Crossroads.Web.Common.MinistryPlatform;
using Newtonsoft.Json;

namespace MinistryPlatform.Translation.Models
{
    [MpRestApiTable(Name = "Responses")]
    public class MpRsvpMember
    {
        [JsonProperty(PropertyName = "Contact_ID")]
        public long ContactId { get; set; }

        [JsonProperty(PropertyName = "Event_ID")]
        public long EventId { get; set; }

        [JsonProperty(PropertyName = "Participant_ID")]
        public long ParticipantId { get; set; }

        [JsonProperty(PropertyName = "Opportunity_ID")]
        public long Opportunity { get; set; }

        [JsonProperty(PropertyName = "Group_Role_ID")]
        public int GroupRoleId { get; set; }

        [JsonProperty(PropertyName = "NickName")]
        public string Name { get; set; }

        [JsonProperty(PropertyName = "Last_Name")]
        public string LastName { get; set; }

        [JsonProperty(PropertyName = "Response_Result_ID")]
        public int ResponseResultId { get; set; } = 0;

        [JsonProperty(PropertyName = "Age")]
        public int? Age { get; set; }
    }
}
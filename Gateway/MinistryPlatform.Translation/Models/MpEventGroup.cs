using Crossroads.Web.Common.MinistryPlatform;
using Newtonsoft.Json;

namespace MinistryPlatform.Translation.Models
{
    [MpRestApiTable(Name = "Event_Groups")]
    public class MpEventGroup
    {
        [JsonProperty(PropertyName = "Event_Group_ID")]
        public int EventGroupId { get; set; }

        [JsonProperty(PropertyName = "Event_ID")]
        public int EventId { get; set; }

        [JsonProperty(PropertyName = "Group_ID")]
        public int GroupId { get; set; }

        [JsonProperty(PropertyName = "Group_Name")]
        public string GroupName { get; set; }

        [JsonProperty(PropertyName = "Room_ID")]
        public int? RoomId { get; set; }

        [JsonProperty(PropertyName = "Domain_ID")]
        public int DomainId { get; set; }

        [JsonProperty(PropertyName = "Closed")]
        public bool Closed { get; set; }

        [JsonProperty(PropertyName = "Event_Room_ID")]
        public int? EventRoomId { get; set; }

        [JsonProperty(PropertyName = "Created")]
        public bool Created { get; set; }

        [JsonProperty(PropertyName = "Group_Type_ID")]
        public int GroupTypeId { get; set; }
    }
}

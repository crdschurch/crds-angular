using Crossroads.Web.Common;
using Crossroads.Web.Common.MinistryPlatform;
using Newtonsoft.Json;

namespace MinistryPlatform.Translation.Models
{
    [MpRestApiTable(Name = "Event_Rooms")]
    public class MpEventRooms
    {
        [JsonProperty(PropertyName = "Event_Room_ID")]
        public int EventRoomId { get; set; }

        [JsonProperty(PropertyName = "Event_ID")]
        public int EventId { get; set; }

        [JsonProperty(PropertyName = "Room_ID")]
        public int RoomId { get; set; }

        [JsonProperty(PropertyName = "Room_Layout_ID")]
        public int? RoomLayoutId { get; set; }

        [JsonProperty(PropertyName = "Notes")]
        public string Notes { get; set; }

        [JsonProperty(PropertyName = "_Approved")]
        public bool? Approved { get; set; }

        [JsonProperty(PropertyName = "Cancelled")]
        public bool? Cancelled { get; set; }

        [JsonProperty(PropertyName = "Hidden")]
        public bool Hidden { get; set; }

        [JsonProperty(PropertyName = "Capacity")]
        public int? Capacity { get; set; }

        [JsonProperty(PropertyName = "Label")]
        public string Label { get; set; }

        [JsonProperty(PropertyName = "Allow_Checkin")]
        public bool? AllowCheckin { get; set; }

        [JsonProperty(PropertyName = "Volunteers")]
        public int? Volunteers { get; set; }

        [JsonProperty(PropertyName = "Group_ID")]
        public int? GroupId { get; set; }

        [JsonProperty(PropertyName = "Default_Group_Room")]
        public bool? DefaultGroupRoom { get; set; }

        [JsonProperty(PropertyName = "Balance_Priority")]
        public int BalancePriority { get; set; }

        [JsonProperty(PropertyName = "Closed")]
        public bool Closed { get; set; }

        [JsonProperty(PropertyName = "Auto_Close_At_Capacity")]
        public bool AutoCloseAtCapacity { get; set; }
    }
}

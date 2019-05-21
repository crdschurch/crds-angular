using System;
using Crossroads.Web.Common.MinistryPlatform;
using Newtonsoft.Json;

namespace MinistryPlatform.Translation.Models
{
    [MpRestApiTable(Name = "Participants")]
    public class MpParticipant
    {
        [JsonProperty(PropertyName = "Contact_ID")]
        public int ContactId { get; set; }

        [JsonProperty(PropertyName = "Participant_ID")]
        public int ParticipantId { get; set;}

        [JsonProperty(PropertyName = "Email_Address")]
        public string EmailAddress { get; set; }

        [JsonProperty(PropertyName = "NickName")]
        public string PreferredName { get; set; }

        [JsonProperty(PropertyName = "Display_Name")]
        public string DisplayName { get; set; }

        [JsonProperty(PropertyName = "Age")]
        public int? Age { get; set; }

        [JsonProperty(PropertyName = "Attendance_Start_Date")]
        public DateTime? AttendanceStart { get; set; }

        public bool ApprovedSmallGroupLeader { get; set; }

        [JsonProperty(PropertyName = "Participant_Type")]
        public string Role { get; set; }

        public string GroupName { get; set; }

        [JsonProperty(PropertyName = "Nickname")]
        public string Nickname { get; set; }

        [JsonProperty(PropertyName = "Group_Leader_Status_ID")]
        public int GroupLeaderStatus { get; set; }

        [JsonProperty(PropertyName = "Host_Status_ID")]
        public int HostStatus { get; set; }

        [JsonProperty(PropertyName = "Show_On_Map")]
        public bool ShowOnMap { get; set; }
    }
}

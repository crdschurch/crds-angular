using System;
using Crossroads.Web.Common.MinistryPlatform;
using Newtonsoft.Json;

namespace MinistryPlatform.Translation.Models
{
    [MpRestApiTable(Name = "Event_Participants")]
    public class MpEventParticipant
    {
        [JsonProperty(PropertyName = "Childcare_Required")]
        public bool ChildcareRequired { get; set; }

        [JsonProperty(PropertyName = "Contact_ID")]
        public int ContactId { get; set; }

        [JsonProperty(PropertyName = "Event_ID")]
        public int EventId { get; set; }

        [JsonProperty(PropertyName = "Event_Participant_ID")]
        public int EventParticipantId { get; set; }

        [JsonProperty(PropertyName = "Event_Start_Date")]
        public DateTime EventStartDateTime { get; set; }

        [JsonProperty(PropertyName = "Event_Title")]
        public string EventTitle { get; set; }

        [JsonProperty(PropertyName = "Group_ID")]
        public int? GroupId { get; set; }

        [JsonProperty(PropertyName = "Group_Name")]
        public string GroupName { get; set; }

        [JsonProperty(PropertyName = "Group_Participant_ID")]
        public int? GroupParticipantId { get; set; }

        [JsonProperty(PropertyName = "Participant_ID")]
        public int ParticipantId { get; set; }

        [JsonProperty(PropertyName = "Email_Address")]
        public string ParticipantEmail { get; set; }

        [JsonProperty(PropertyName = "Participation_Status_ID")]
        public int ParticipantStatus { get; set; }

        [JsonProperty(PropertyName = "Room_ID")]
        public int? RoomId { get; set; }

        [JsonProperty(PropertyName = "Setup_Date")]
        public DateTime? SetupDate { get; set; }

        [JsonProperty(PropertyName = "End_Date")]
        public DateTime? EndDate { get; set; }
    }
}
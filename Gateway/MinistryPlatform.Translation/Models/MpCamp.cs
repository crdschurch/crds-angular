using System;
using System.Collections.Generic;
using Crossroads.Web.Common.MinistryPlatform;
using Newtonsoft.Json;

namespace MinistryPlatform.Translation.Models
{
    [MpRestApiTable(Name = "Events")]
    public class MpCamp
    {
        [JsonProperty(PropertyName = "Event_ID")]
        public int EventId { get; set; }

        [JsonProperty(PropertyName = "Event_Title")]
        public string EventTitle { get; set; }

        [JsonProperty(PropertyName = "Event_Start_Date")]
        public DateTime StartDate { get; set; }

        [JsonProperty(PropertyName = "Event_End_Date")]
        public DateTime EndDate { get; set; }

        [JsonProperty(PropertyName = "Event_Type_ID")]
        public int EventType { get; set; }

        [JsonProperty(PropertyName = "Program_ID")]
        public int ProgramId { get; set; }

        [JsonProperty(PropertyName = "Online_Registration_Product")]
        public int OnlineProductId { get; set; }

        [JsonProperty(PropertyName = "Email_Address")]
        public string PrimaryContactEmail { get; set; }

        [JsonProperty(PropertyName = "Registration_Start")]
        public DateTime RegistrationStartDate { get; set; }

        [JsonProperty(PropertyName = "Registration_End")]
        public DateTime RegistrationEndDate { get; set; }

        [JsonProperty(PropertyName = "Group_Name")]
        public List<MpEventGroup> CampGradesList { get; set; }

    }
}

using System;
using System.Collections.Generic;
using Crossroads.Web.Common.MinistryPlatform;
using Newtonsoft.Json;

namespace MinistryPlatform.Translation.Models
{
    [MpRestApiTable(Name = "Events")]
    public class MpEvent
    {    
        [JsonProperty(PropertyName = "Event_ID")]
        public int EventId { get; set; }

        [JsonProperty(PropertyName = "Event_Title")]
        public string EventTitle { get; set; }

        [JsonProperty(PropertyName = "Event_Start_Date")]
        public DateTime EventStartDate { get; set; }

        [JsonProperty(PropertyName = "Event_End_Date")]
        public DateTime EventEndDate { get; set; }

        [JsonProperty(PropertyName = "Event_Type_ID")]
        public string EventType { get; set; }

        [JsonProperty(PropertyName = "Program_ID")]
        public int? ProgramId { get; set; }

        [JsonProperty(PropertyName = "Online_Registration_Product")]
        public int? OnlineProductId { get; set; }

        [JsonProperty(PropertyName = "Registration_Start")]
        public DateTime? RegistrationStartDate { get; set; }

        [JsonProperty(PropertyName = "Registration_End")]
        public DateTime? RegistrationEndDate { get; set; }

        [JsonProperty(PropertyName = "External_Registration_URL")]
        public string RegistrationURL { get; set; }

        private IList<int> participants = new List<int>();

        [JsonProperty(PropertyName = "Congregation_Name")]
        public string Congregation { get; set; }

        public string Site { get; set; }                      
        
        public int? ParentEventId { get; set; }

        public MpContact PrimaryContact { get; set; }

        [JsonProperty(PropertyName = "On_Donation_Batch_Tool")]
        public bool DonationBatchTool { get; set; }

        [JsonProperty(PropertyName = "Primary_Contact")]
        public int PrimaryContactId { get; set; }
        public IList<int> Participants
        {
            get { return (participants); }
        }
        [JsonProperty(PropertyName = "Congregation_ID")]
        public int CongregationId { get; set; }
        [JsonProperty(PropertyName = "Reminder_Days_Prior_ID")]
        public int? ReminderDaysPriorId { get; set; }

        [JsonProperty(PropertyName = "Send_Reminder")]
        public bool SendReminder { get; set; }

        [JsonProperty(PropertyName = "Template")]
        public bool? Template { get; set; }
        [JsonProperty(PropertyName = "Cancelled")]
        public bool Cancelled { get; set; }

        [JsonProperty(PropertyName = "Minutes_Until_Timeout")]
        public int? MinutesUntilTimeout { get; set; }
        
        [JsonProperty(PropertyName = "Participants_Expected")]
        public int? ParticipantsExpected { get; set; }

        public string Description { get; set; }

        [JsonProperty(PropertyName="Meeting_Instructions")]
        public string MeetingInstructions { get; set; }

        [JsonProperty(PropertyName = "Minutes_For_Setup")]
        public int MinutesSetup { get; set; }

        [JsonProperty(PropertyName = "Minutes_For_Cleanup")]
        public int MinutesTeardown { get; set; }
    }
}
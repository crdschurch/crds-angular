﻿using System;
using System.Collections.Generic;
using MinistryPlatform.Translation.Models.Attributes;
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

        public string Congregation { get; set; }

        public string Site { get; set; }                      
        
        public int? ParentEventId { get; set; }

        public MpContact PrimaryContact { get; set; }

        [JsonProperty(PropertyName = "Primary_Contact")]
        public int PrimaryContactId { get; set; }
        public IList<int> Participants
        {
            get { return (participants); }
        }
        [JsonProperty(PropertyName = "Congregation_ID")]
        public int CongregationId { get; set; }
        [JsonProperty(PropertyName = "Reminder_Days_Prior_ID")]
        public int ReminderDaysPriorId { get; set; }
        [JsonProperty(PropertyName = "Template")]
        public bool? Template { get; set; }
        [JsonProperty(PropertyName = "Cancelled")]
        public bool Cancelled { get; set; }

        [JsonProperty(PropertyName = "Minutes_Until_Timeout")]
        public int? MinutesUntilTimeout { get; set; }
    }
}
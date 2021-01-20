using System;
using Crossroads.Web.Common.MinistryPlatform;
using Newtonsoft.Json;

namespace MinistryPlatform.Translation.Models
{
    [MpRestApiTable(Name = "vw_crds_Serving_Participants")]
    public class MpGroupServingParticipant
    {
        [JsonProperty(PropertyName = "Contact_ID")]
        public int ContactId { get; set; }

        [JsonProperty(PropertyName = "Domain_ID")]
        public int DomainId { get; set; }

        [JsonProperty(PropertyName = "Event_Type")]
        public string EventType { get; set; }

        [JsonProperty(PropertyName = "Event_Type_ID")]
        public int EventTypeId { get; set; }

        [JsonProperty(PropertyName = "Event_ID")]
        public int EventId { get; set; }

        [JsonProperty(PropertyName = "Event_Start_Date")]
        public DateTime EventStartDateTime { get; set; }

        [JsonProperty(PropertyName = "Event_Title")]
        public string EventTitle { get; set; }

        [JsonProperty(PropertyName = "Group_ID")]
        public int GroupId { get; set; }

        [JsonProperty(PropertyName = "Group_Name")]
        public string GroupName { get; set; }

        [JsonProperty(PropertyName = "Group_Role_ID")]
        public int GroupRoleId { get; set; }

        [JsonProperty(PropertyName = "Primary_Contact_Email")]
        public string GroupPrimaryContactEmail { get; set; }

        [JsonProperty(PropertyName = "Opportunity_ID")]
        public int OpportunityId { get; set; }

        [JsonProperty(PropertyName = "Maximum_Needed")]
        public int? OpportunityMaximumNeeded { get; set; }

        [JsonProperty(PropertyName = "Minimum_Needed")]
        public int? OpportunityMinimumNeeded { get; set; }

        [JsonProperty(PropertyName = "Role_Title")]
        public string OpportunityRoleTitle { get; set; }

        [JsonProperty(PropertyName = "Shift_End")]
        public TimeSpan? OpportunityShiftEnd { get; set; }

        [JsonProperty(PropertyName = "Shift_Start")]
        public TimeSpan? OpportunityShiftStart { get; set; }

        private int opportunitySignUpDeadLine;

        [JsonProperty(PropertyName = "Sign_Up_Deadline")]
        public int? OpportunitySignUpDeadline {
            get { return opportunitySignUpDeadLine; }
            set { opportunitySignUpDeadLine = value ?? 0; }
        }

        [JsonProperty(PropertyName = "Deadline_Passed_Message_ID")]
        public int? DeadlinePassedMessage { get; set; }

        [JsonProperty(PropertyName = "Opportunity_Title")]
        public string OpportunityTitle { get; set; }

        [JsonProperty(PropertyName = "Nickname")]
        public string ParticipantNickname { get; set; }

        [JsonProperty(PropertyName = "Email_Address")]
        public string ParticipantEmail { get; set; }

        [JsonProperty(PropertyName = "Participant_ID")]
        public int ParticipantId { get; set; }

        [JsonProperty(PropertyName = "Last_Name")]
        public string ParticipantLastName { get; set; }

        [JsonProperty(PropertyName = "Room")]
        public string Room { get; set; }

        private int? _rsvpResponse;
        [JsonProperty(PropertyName = "Rsvp")]
        public int? RsvpResponse
        {
            get { return _rsvpResponse; }

            set
            {
                _rsvpResponse = value;
                
                if (RsvpResponse == 1)
                {
                    Rsvp = true;
                }
                else if (RsvpResponse == 2)
                {
                    Rsvp = false;
                }
            } }

        public long RowNumber { get; set; }
        [JsonProperty(PropertyName = "NotRsvp")]
        public bool? Rsvp { get; set; }
        public bool LoggedInUser { get; set; }
    }
}
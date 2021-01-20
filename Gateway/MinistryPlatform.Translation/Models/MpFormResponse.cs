using System.Collections.Generic;
using Crossroads.Web.Common.MinistryPlatform;
using Newtonsoft.Json;

namespace MinistryPlatform.Translation.Models
{
    [MpRestApiTable(Name = "Form_Responses")]
    public class MpFormResponse
    {
        [JsonProperty(PropertyName = "Form_Response_ID")]
        public int FormResponseId { get; set; }
        [JsonProperty(PropertyName = "Form_ID")]
        public int FormId { get; set; }
        [JsonProperty(PropertyName = "Contact_ID")]
        public int ContactId { get; set; }
        [JsonProperty(PropertyName = "Opportunity_ID")]
        public int? OpportunityId { get; set; }
        [JsonProperty(PropertyName = "Opportunity_Response")]
        public int? OpportunityResponseId { get; set; }
        [JsonProperty(PropertyName = "Pledge_Campaign_ID")]
        public int? PledgeCampaignId { get; set; }
        [JsonProperty(PropertyName = "Event_ID")]
        public int? EventId { get; set; }
        public List<MpFormAnswer> FormAnswers { get; set; }

        public MpFormResponse()
        {
            FormAnswers = new List<MpFormAnswer>();
        }
    }

    [MpRestApiTable(Name = "Form_Response_Answers")]
    public class MpFormAnswer
    {
        [JsonProperty(PropertyName = "Form_Response_Answer_ID")]
        public int FormResponseAnswerId { get; set; }
        [JsonProperty(PropertyName = "Form_Field_ID")]
        public int FieldId { get; set; }
        [JsonProperty(PropertyName = "Form_Response_ID")]
        public int FormResponseId { get; set; }
        [JsonProperty(PropertyName = "Response")]
        public string Response { get; set; }
        [JsonProperty(PropertyName = "Opportunity_Response")]
        public int? OpportunityResponseId { get; set; }
        [JsonProperty(PropertyName = "Event_Participant_ID")]
        public int? EventParticipantId { get; set; }
    }
}
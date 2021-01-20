using System;
using System.Collections.Generic;
using MinistryPlatform.Translation.Models.Childcare;
using Newtonsoft.Json;

namespace crds_angular.Models.Crossroads.Childcare
{
    public class ChildcareRequestDto
    {
        [JsonProperty(PropertyName = "requester")]
        public int RequesterId { get; set; }

        [JsonProperty(PropertyName = "site")]
        public int LocationId { get; set; }

        [JsonProperty(PropertyName = "ministry")]
        public int MinistryId { get; set; }

        [JsonProperty(PropertyName = "group")]
        public int GroupId { get; set; }

        [JsonProperty(PropertyName = "startDate")]
        public DateTime StartDate { get; set; }

        [JsonProperty(PropertyName = "endDate")]
        public DateTime EndDate { get; set; }

        [JsonProperty(PropertyName = "frequency")]
        public string Frequency { get; set; }

        [JsonProperty(PropertyName = "timeframe")]
        public string PreferredTime { get; set; }

        [JsonProperty(PropertyName = "notes")]
        public string Notes { get; set; }

        [JsonProperty(PropertyName = "dates")]
        public List<DateTime> DatesList { get; set; }

        [JsonProperty(PropertyName="decisionNotes")]
        public string DecisionNotes { get; set; }

        [JsonProperty(PropertyName = "childcareRequestId")]
        public int ChildcareRequestId { get; set; }

        public MpChildcareRequest ToMPChildcareRequest()
        {
            var mpReq = new MpChildcareRequest
            {
                RequesterId = this.RequesterId,
                LocationId = this.LocationId,
                MinistryId = this.MinistryId,
                GroupId = this.GroupId,
                StartDate = this.StartDate,
                EndDate = this.EndDate,
                Frequency = this.Frequency,
                PreferredTime = this.PreferredTime,
                DatesList = this.DatesList,
                Notes = this.Notes,
                DecisionNotes = this.DecisionNotes,
                ChildcareRequestId = this.ChildcareRequestId
            };

            return mpReq;
        }
    }
}

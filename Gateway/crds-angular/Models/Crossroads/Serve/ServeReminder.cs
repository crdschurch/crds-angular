using System;

namespace crds_angular.Models.Crossroads.Serve
{
    public class ServeReminder
    {
        public string OpportunityTitle { get; set; }

        public int SignedupContactId { get; set; }

        public string SignedupEmailAddress { get; set; }

        public string EventTitle { get; set; }

        public DateTime EventStartDate { get; set; }

        public DateTime EventEndDate { get; set; }

        public int? TemplateId { get; set; }

        public int OpportunityContactId { get; set; }

        public string OpportunityEmailAddress { get; set; }

        public TimeSpan? ShiftStart { get; set; }

        public TimeSpan? ShiftEnd { get; set; }
    }
}
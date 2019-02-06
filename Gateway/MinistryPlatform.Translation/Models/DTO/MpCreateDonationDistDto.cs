using System;
using Crossroads.Web.Common.MinistryPlatform;
using Newtonsoft.Json;

namespace MinistryPlatform.Translation.Models.DTO
{
    [MpRestApiTable(Name = "Recurring_Gifts")]
    public class MpCreateDonationDistDto
    {
        [JsonProperty(PropertyName = "Amount")]
        public Decimal Amount { get; set; }

        [JsonProperty(PropertyName = "Donor_ID")]
        public int DonorId { get; set; }

        [JsonProperty(PropertyName = "Congregation_ID")]
        public int CongregationId { get; set; }
        
        [JsonProperty(PropertyName = "Account_Type_ID")]
        public string PaymentType { get; set; }

        [JsonProperty(PropertyName = "Program_ID")]
        public string ProgramId { get; set; }

        [JsonProperty(PropertyName = "Program_Name")]
        public string ProgramName { get; set; }

        [JsonProperty(PropertyName = "Recurring_Gift_ID")]
        public int? RecurringGiftId { get; set; }

        [JsonProperty(PropertyName = "Donor_Account_ID")]
        public int? DonorAccountId { get; set; }

        [JsonProperty(PropertyName = "Frequency_ID")]
        public int Frequency { get; set; }

        [JsonProperty(PropertyName = "Recurrence")]
        public string Recurrence { get; set; }

        [JsonProperty(PropertyName = "Day_Of_Week_ID")]
        public int? DayOfWeek { get; set; }

        [JsonProperty(PropertyName = "Day_Of_Month_ID")]
        public int? DayOfMonth { get; set; }

        [JsonProperty(PropertyName = "Start_Date")]
        public DateTime? StartDate { get; set; }

        [JsonProperty(PropertyName = "Subscription_ID")]
        public string SubscriptionId { get; set; }

        [JsonProperty(PropertyName = "Consecutive_Failure_Count")]
        public int ConsecutiveFailureCount { get; set; }

        [JsonProperty(PropertyName = "Processor_ID")]
        public string StripeCustomerId { get; set; }

        [JsonProperty(PropertyName = "Processor_Account_ID")]
        public string StripeAccountId { get; set; }
    }
}

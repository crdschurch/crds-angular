using System;
using crds_angular.Models.Json;
using Newtonsoft.Json;

namespace crds_angular.Models.Crossroads.Stewardship
{
    public class StripeInvoice : StripeObject
    {
        [JsonProperty("subscription")]
        public string Subscription { get; set; }

        [JsonProperty("customer")]
        public string Customer { get; set; }

        [JsonProperty("charge")]
        public string Charge { get; set; }

        [JsonProperty("amount_due")]
        public int Amount { get; set; }

        [JsonProperty("date")]
        [JsonConverter(typeof(StripeDateTimeConverter))]
        public DateTime Date { get; set; }

        [JsonProperty("paid")]
        public bool Paid { get; set; }
    }
}

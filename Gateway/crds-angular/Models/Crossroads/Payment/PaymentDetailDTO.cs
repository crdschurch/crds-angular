using Newtonsoft.Json;

namespace crds_angular.Models.Crossroads.Payment
{
    public class PaymentDetailDTO
    {
        [JsonProperty(PropertyName = "paymentAmount")]
        public decimal PaymentAmount { get; set; }

        [JsonProperty(PropertyName = "paymentLeft")]
        public decimal TotalToPay { get; set; }

        [JsonProperty(PropertyName = "recipientEmail")]
        public string RecipientEmail { get; set; }

        [JsonProperty(PropertyName = "invoiceTotal")]
        public decimal InvoiceTotal { get; set; }

        [JsonProperty(PropertyName = "recentPaymentId")]
        public int? RecentPaymentId { get; set; }

        [JsonProperty(PropertyName = "recentPaymentAmount")]
        public decimal? RecentPaymentAmount { get; set; }

        [JsonProperty(PropertyName = "recentPaymentLastFour")]
        public string RecentPaymentLastFour { get; set; }
    }
}
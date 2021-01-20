using Crossroads.Web.Common.MinistryPlatform;
using Newtonsoft.Json;

namespace MinistryPlatform.Translation.Models.Payments
{
    [MpRestApiTable(Name = "Payment_Types")]
    public class MpPaymentType
    {
        [JsonProperty(PropertyName = "Payment_Type_ID")]
        public int PaymentTypeId { get; set; }

        [JsonProperty(PropertyName = "Payment_Type")]
        public string PaymentType { get; set; }

    }
}

using Crossroads.Web.Common.MinistryPlatform;
using Newtonsoft.Json;

namespace MinistryPlatform.Translation.Models.Payments
{
    [MpRestApiTable(Name = "Invoice_Detail")]
    public class MpNestedInvoiceDetail
    {
        [JsonProperty(PropertyName = "Invoice_Detail_ID")]
        public int InvoiceDetailId { get; set; }

        [JsonProperty(PropertyName = "Invoice_ID")]
        public MpInvoice Invoice { get; set; }

        [JsonProperty(PropertyName = "Recipient_Contact_ID")]
        public int RecipientContactId { get; set; }

        [JsonProperty(PropertyName = "Event_Participant_ID")]
        public int? EventParticipantId { get; set; }

        [JsonProperty(PropertyName = "Item_Quantity")]
        public int Quantity { get; set; }

        [JsonProperty(PropertyName = "Line_Total")]
        public decimal LineTotal { get; set; }

        [JsonProperty(PropertyName = "Product_ID")]
        public int ProductId { get; set; }

        [JsonProperty(PropertyName = "Product_Option_Price_ID")]
        public int? ProductOptionPriceId { get; set; }

        [JsonProperty(PropertyName = "Item_Note")]
        public string ItemNote { get; set; }

        [JsonProperty(PropertyName = "Recipient_Name")]
        public string RecipientName { get; set; }

        [JsonProperty(PropertyName = "Recipient_Address")]
        public string RecipientAddress { get; set; }

        [JsonProperty(PropertyName = "Recipient_Email")]
        public string RecipientEmail { get; set; }

        [JsonProperty(PropertyName = "Recipient_Phone")]
        public string RecipientPhone { get; set; }
    }
}

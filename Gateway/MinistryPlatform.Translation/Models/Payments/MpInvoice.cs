using System;
using Crossroads.Web.Common.MinistryPlatform;
using Newtonsoft.Json;

namespace MinistryPlatform.Translation.Models.Payments
{
    [MpRestApiTable(Name = "Invoices")]
    public class MpInvoice
    {
        [JsonProperty(PropertyName = "Invoice_ID")]
        public int InvoiceId { get; set; }
        [JsonProperty(PropertyName = "Purchaser_Contact_ID")]
        public int PurchaserContactId { get; set; }
        [JsonProperty(PropertyName = "Invoice_Status_ID")]
        public int InvoiceStatusId { get; set; }
        [JsonProperty(PropertyName = "Invoice_Total")]
        public decimal InvoiceTotal { get; set; }
        [JsonProperty(PropertyName = "Invoice_Date")]
        public DateTime InvoiceDate { get; set; }
        [JsonProperty(PropertyName = "Notes")]
        public string Notes { get; set; }
        [JsonProperty(PropertyName = "Currency")]
        public string Currency { get; set; }
    }
}

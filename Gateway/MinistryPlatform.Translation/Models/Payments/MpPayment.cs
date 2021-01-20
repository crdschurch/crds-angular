using System;
using Crossroads.Web.Common.MinistryPlatform;
using Newtonsoft.Json;

namespace MinistryPlatform.Translation.Models.Payments
{
    [MpRestApiTable(Name = "Payments")]
    public class MpPayment
    {
        [JsonProperty(PropertyName = "Payment_ID")]
        public int PaymentId { get; set; }

        [JsonProperty(PropertyName = "Payment_Total")]
        public decimal PaymentTotal { get; set; }

        [JsonProperty(PropertyName = "Contact_ID")]
        public int ContactId { get; set; }
        
        [JsonProperty(PropertyName = "Payment_Date")]
        public DateTime PaymentDate { get; set; }
        
        [JsonProperty(PropertyName = "Gateway_Response")]
        public string GatewayResponse { get; set; }
    
        [JsonProperty(PropertyName = "Transaction_Code")]
        public string TransactionCode { get; set; }
    
        [JsonProperty(PropertyName = "Notes")]
        public string Notes { get; set; }
    
        [JsonProperty(PropertyName = "Merchant_Batch")]
        public string MerchantBatch { get; set; }
        
        [JsonProperty(PropertyName = "Payment_Type_ID")]
        public int PaymentTypeId { get; set; }
    
        [JsonProperty(PropertyName = "Item_Number")]
        public string ItemNumber { get; set; }

        [JsonProperty(PropertyName = "Currency")]
        public string Currency { get; set; }
    
        [JsonProperty(PropertyName = "Invoice_Number")]
        public string InvoiceNumber { get; set; }

        [JsonProperty(PropertyName = "Batch_ID")]
        public int? BatchId { get; set; }

        [JsonProperty(PropertyName = "Payment_Status_ID")]
        public int PaymentStatus { get; set; }

        [JsonProperty(PropertyName = "Processor_Fee_Amount")]
        public decimal? ProcessorFeeAmount { get; set; }
    }
}

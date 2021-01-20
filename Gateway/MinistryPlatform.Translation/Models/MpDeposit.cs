using System;
using Crossroads.Web.Common.MinistryPlatform;
using Newtonsoft.Json;

namespace MinistryPlatform.Translation.Models
{
    [MpRestApiTable(Name = "Deposits")]
    public class MpDeposit
    {
        [JsonProperty(PropertyName = "Deposit_ID")]
        public int Id { get; set; }
        [JsonProperty(PropertyName = "Deposit_Name")]
        public string DepositName { get; set; }
        [JsonProperty(PropertyName = "Deposit_Total")]
        public decimal DepositTotalAmount { get; set; }
        [JsonProperty(PropertyName = "Deposit_Date")]
        public DateTime DepositDateTime { get; set; }
        [JsonProperty(PropertyName = "Batch_Count")]
        public int BatchCount { get; set; }
        [JsonProperty(PropertyName = "Exported")]
        public bool Exported { get; set; }
        [JsonProperty(PropertyName = "Processor_Transfer_ID")]
        public string ProcessorTransferId { get; set; }
    }
}
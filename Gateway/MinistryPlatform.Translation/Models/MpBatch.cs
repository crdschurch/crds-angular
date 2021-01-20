using System;
using Crossroads.Web.Common.MinistryPlatform;
using Newtonsoft.Json;

namespace MinistryPlatform.Translation.Models
{
    [MpRestApiTable(Name = "Batches")]
    public class MpBatch
    {
        [JsonProperty(PropertyName = "Batch_ID")]
        public int BatchId { get; set; }

        [JsonProperty(PropertyName = "Batch_Name")]
        public string BatchName { get; set; }

        [JsonProperty(PropertyName = "Setup_Date")]
        public DateTime SetupDate { get; set; }

        [JsonProperty(PropertyName = "Batch_Total")]
        public decimal BatchTotal { get; set; }

        [JsonProperty(PropertyName = "Item_Count")]
        public int ItemCount { get; set; }

        [JsonProperty(PropertyName = "Batch_Entry_Type_ID")]
        public int BatchEntryTypeId { get; set; }

        [JsonProperty(PropertyName = "Deposit_ID")]
        public int? DepositId { get; set; }

        [JsonProperty(PropertyName = "Finalize_Date")]
        public DateTime FinalizeDate { get; set; }

        [JsonProperty(PropertyName = "Processor_Transfer_ID")]
        public string ProcessorTransferId { get; set; }

    }
}

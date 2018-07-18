using System;
using Crossroads.Web.Common.MinistryPlatform;
using Newtonsoft.Json;

namespace MinistryPlatform.Translation.Models
{
    [MpRestApiTable(Name = "cr_Connect_History")]
    public class MpConnectHistory
    {
        [JsonProperty(PropertyName = "Connect_History_ID")]
        public int ConnectHistoryId { get; set; }
        [JsonProperty(PropertyName = "Participant_ID")]
        public int ParticipantId { get; set; }
        [JsonProperty(PropertyName = "Connect_Status_ID")]
        public int ConnectStatusId { get; set; }
        [JsonProperty(PropertyName = "Transaction_Date")]
        public DateTime TransactionDate { get; set; }
    }

}
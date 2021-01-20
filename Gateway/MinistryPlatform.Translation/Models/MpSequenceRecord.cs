using Crossroads.Web.Common.MinistryPlatform;
using Newtonsoft.Json;

namespace MinistryPlatform.Translation.Models
{
    [MpRestApiTable(Name = "dp_Sequence_Records")]
    public class MpSequenceRecord
    {    
        [JsonProperty(PropertyName = "Record_ID")]
        public int RecordId { get; set; }

        [JsonProperty(PropertyName = "Table_Name")]
        public string TableName { get; set; }

        [JsonProperty(PropertyName = "Domain_ID")]
        public int DomainId { get; set; }

        [JsonProperty(PropertyName = "Sequence_ID")]
        public int SequenceId { get; set; }
    }
}
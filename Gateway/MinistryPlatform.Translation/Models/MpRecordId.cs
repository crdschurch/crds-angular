using Crossroads.Web.Common.MinistryPlatform;
using Newtonsoft.Json;

namespace MinistryPlatform.Translation.Models
{
    namespace MinistryPlatform.Translation.Models
    {
        [MpRestApiTable(Name = "MpRecordID")]
        public class MpRecordID
        {
            [JsonProperty(PropertyName = "RecordID")]
            public int RecordId { get; set; }

        }
    }
}

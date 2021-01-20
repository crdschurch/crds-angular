using Newtonsoft.Json;

namespace crds_angular.Models.MailChimp
{
    public class BulkEmailSubscriberOptDTO
    {
        [JsonProperty("id")]
        public string ThirdPartySystemID { get; set; }
        [JsonProperty("email_address")]
        public string EmailAddress { get; set; }
        [JsonProperty("status")]
        public string Status { get; set; }
        [JsonIgnore]
        public int PublicationID { get; set; }
    }
}
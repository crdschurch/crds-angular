using crds_angular.Models.Crossroads;
using crds_angular.Models.Crossroads.Groups;
using Newtonsoft.Json;

namespace crds_angular.Models.Finder
{
    public enum PinType
    {
        PERSON = 1,
        GATHERING = 2,
        SITE = 3,
        SMALL_GROUP = 4
    };

    public class PinDto
    {
        [JsonProperty("firstName")]
        public string FirstName { get; set; }

        [JsonProperty("lastName")]
        public string LastName { get; set; }

        [JsonProperty("emailAddress")]
        public string EmailAddress { get; set; }

        [JsonProperty("contactId")]
        public int? Contact_ID { get; set; }

        [JsonProperty("participantId")]
        public int? Participant_ID { get; set; }

        [JsonProperty("address")]
        public AddressDTO Address { get; set; }

        [JsonProperty("hostStatus")]
        public int? Host_Status_ID { get; set; }

        [JsonProperty("gathering")]
        public FinderGroupDto Gathering { get; set; }

        [JsonProperty("householdId")]
        public int? Household_ID { get; set; }

        [JsonProperty("pinType")]
        public PinType PinType { get; set;  }

        [JsonProperty("proximity")]
        public decimal? Proximity { get; set; }

        [JsonProperty("siteName")]
        public string SiteName { get; set; }

        [JsonProperty("Show_On_Map")]
        public bool ShowOnMap { get; set; }

        [JsonProperty("updateHomeAddress")]
        public bool ShouldUpdateHomeAddress { get; set; }

        [JsonProperty("iconUrl")]
        public string IconUrl { get; set; }

        [JsonProperty("title")]
        public string Title { get; set; }

        [JsonProperty("congregationId")]
        public int congregationId { get; set; }
    }
}
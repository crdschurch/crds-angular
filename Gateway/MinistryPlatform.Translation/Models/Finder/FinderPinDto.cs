using Crossroads.Web.Common.MinistryPlatform;
using Newtonsoft.Json;

namespace MinistryPlatform.Translation.Models.Finder
{
    [MpRestApiTable(Name = "Contacts")]
    public class FinderPinDto
    {
        public int Contact_ID { get; set; }
        public int Participant_ID { get; set; }
        public string FirstName { get; set; }

        public string LastName { get; set; }
        
        [JsonProperty("Host_Status_ID")]
        public int Host_Status_ID { get; set; }

        [JsonProperty("Email_Address")]
        public string EmailAddress { get; set; }

        [JsonProperty("Show_On_Map")]
        public bool ShowOnMap { get; set; }

        public int Household_ID { get; set; }

        public MpAddress Address { get; set; }

    }
}

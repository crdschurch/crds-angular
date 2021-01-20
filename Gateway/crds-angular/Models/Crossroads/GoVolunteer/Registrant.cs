using System.ComponentModel.DataAnnotations;
using Newtonsoft.Json;

namespace crds_angular.Models.Crossroads.GoVolunteer
{
    public class Registrant
    {
        [JsonProperty(PropertyName = "contactId")]
        public int ContactId { get; set; }

        [JsonProperty(PropertyName = "dob")]
        public string DateOfBirth { get; set; }

        [JsonProperty(PropertyName = "emailAddress")]
        public string EmailAddress { get; set; }

        [JsonProperty(PropertyName = "firstName")]
        public string FirstName { get; set; }

        [JsonProperty(PropertyName = "lastName")]
        public string LastName { get; set; }

        [JsonProperty(PropertyName = "mobile")]
        [RegularExpression(@"^\d{3}-\d{3}-\d{4}$")]
        public string MobilePhone { get; set; }
    }
}
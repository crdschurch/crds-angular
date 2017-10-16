using System;
using Crossroads.Web.Common.MinistryPlatform;
using Newtonsoft.Json;


namespace MinistryPlatform.Translation.Models
{
    [MpRestApiTable(Name = "Contacts")]
    public class MpSimpleContact
    {
        [JsonProperty(PropertyName = "Contact_ID" )]
        public int ContactId { get; set; }

        [JsonProperty(PropertyName = "Email_Address")]
        public string EmailAddress { get; set; }

        [JsonProperty(PropertyName = "First_Name")]
        public string FirstName { get; set; }

        [JsonProperty(PropertyName = "Last_Name")]
        public string LastName { get; set; }

        [JsonProperty(PropertyName = "Nickname")]
        public string Nickname { get; set; }

        [JsonProperty(PropertyName = "Date_of_Birth")]
        public DateTime DateOfBirth { get; set; }
    }
}
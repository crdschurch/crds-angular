using System;
using Crossroads.Web.Common.MinistryPlatform;
using Newtonsoft.Json;

namespace MinistryPlatform.Translation.Models
{
    [MpRestApiTable(Name = "Contacts")]
    public class MpContact
    {
        [JsonProperty(PropertyName = "Contact_ID")]
        public int ContactId { get; set; }

        [JsonProperty(PropertyName = "First_Name")]
        public string FirstName { get; set; }

        [JsonProperty(PropertyName = "Nickname")]
        public string Nickname { get; set; }

        [JsonProperty(PropertyName = "Last_Name")]
        public string LastName { get; set; }

        [JsonProperty(PropertyName = "Email_Address")]
        public string EmailAddress { get; set; }

        [JsonProperty(PropertyName = "Middle_Name")]
        public string MiddleName { get; set; }

        [JsonProperty(PropertyName = "Display_Name")]
        public string PreferredName { get; set; }

        [JsonProperty(PropertyName = "Date_Of_Birth")]
        public DateTime BirthDate { get; set; }

        [JsonProperty(PropertyName = "Gender_ID")]
        public int? Gender { get; set; }

        [JsonProperty(PropertyName = "Household_ID")]
        public int HouseholdId { get; set; }

        [JsonProperty(PropertyName = "Household_Position_ID")]
        public int HouseholdPositionId { get; set; }

        [JsonProperty(PropertyName = "Current_School")]
        public string SchoolAttending { get; set; }

        [JsonProperty(PropertyName = "Mobile_Phone")]
        public string MobilePhone { get; set; }

        [JsonProperty(PropertyName = "Household_Position")]
        public String HouseholdPosition { get; set; }
    }
}
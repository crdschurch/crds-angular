using System;
using Newtonsoft.Json;

namespace crds_angular.Models.AwsCloudsearch
{
    public class AwsConnectDto
    {
        [JsonProperty(PropertyName = "firstname")]
        public string FirstName { get; set; }

        [JsonProperty(PropertyName = "lastname")]
        public string LastName { get; set; }

        [JsonProperty(PropertyName = "sitename")]
        public string SiteName { get; set; }

        [JsonProperty(PropertyName = "contactid")]
        public int? ContactId { get; set; }

        [JsonProperty(PropertyName = "participantid")]
        public int? ParticipantId { get; set; }

        [JsonProperty(PropertyName = "addressid")]
        public int? AddressId { get; set; }

        [JsonProperty(PropertyName = "city")]
        public string City { get; set; }

        [JsonProperty(PropertyName = "state")]
        public string State { get; set; }

        [JsonProperty(PropertyName = "zip")]
        public string Zip { get; set; }

        [JsonIgnore]
        public double? Latitude { get; set; }

        [JsonIgnore]
        public double? Longitude { get; set; }

        [JsonProperty(PropertyName = "latlong")]
        public string LatLong { get; set; }

        [JsonProperty(PropertyName = "hoststatus")]
        public int? HostStatus { get; set; }

        [JsonProperty(PropertyName = "groupid")]
        public int? GroupId { get; set; }

        [JsonProperty(PropertyName = "groupname")]
        public string GroupName { get; set; }

        [JsonProperty(PropertyName = "groupstartdate")]
        public DateTime? GroupStartDate { get; set; } = null;

        [JsonProperty(PropertyName = "groupdescription")]
        public string GroupDescription { get; set; }

        [JsonProperty(PropertyName = "primarycontactid")]
        public int? PrimaryContactId { get; set; }

        [JsonProperty(PropertyName = "primarycontactemail")]
        public string PrimaryContactEmail { get; set; }

        [JsonProperty(PropertyName = "participantcount")]
        public int? ParticipantCount { get; set; }

        [JsonProperty(PropertyName = "grouptypeid")]
        public int? GroupTypeId { get; set; }

        [JsonProperty(PropertyName = "householdid")]
        public int? HouseholdId { get; set; }

        [JsonProperty(PropertyName = "pintype")]
        public int PinType { get; set; }

        // fields below added for finder-grouptool
        [JsonProperty(PropertyName = "groupcategory")]
        public string[] GroupCategory { get; set; }

        [JsonProperty(PropertyName = "grouptype")]
        public string GroupType { get; set; }

        [JsonProperty(PropertyName = "groupagerange")]
        public string[] GroupAgeRange { get; set; }

        [JsonProperty(PropertyName = "groupmeetingday")]
        public string GroupMeetingDay { get; set; }

        [JsonProperty(PropertyName = "groupmeetingtime")]
        public string GroupMeetingTime { get; set; }

        [JsonProperty(PropertyName = "groupvirtual")]
        public int GroupVirtual { get; set; }

        [JsonProperty(PropertyName = "grouplocation")]
        public string GroupLocation { get; set; }

        [JsonProperty(PropertyName = "groupmeetingfrequency")]
        public string GroupMeetingFrequency { get; set; }

        [JsonProperty(PropertyName = "groupkidswelcome")]
        public int GroupKidsWelcome { get; set; }

        [JsonProperty(PropertyName = "groupprimarycontactfirstname")]
        public string GroupPrimaryContactFirstName { get; set; }

        [JsonProperty(PropertyName = "groupprimarycontactlastname")]
        public string GroupPrimaryContactLastName { get; set; }

        [JsonProperty(PropertyName = "groupprimarycontactcongregation")]
        public string GroupPrimaryContactCongregation { get; set; }

        [JsonProperty(PropertyName = "groupavailableonline")]
        public int GroupAvailableOnline { get; set; }
    }
}
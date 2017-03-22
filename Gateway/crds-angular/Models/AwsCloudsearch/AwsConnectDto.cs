﻿using System;
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

        [JsonProperty(PropertyName = "emailaddress")]
        public string EmailAddress { get; set; }

        [JsonProperty(PropertyName = "contactid")]
        public int? ContactId { get; set; }

        [JsonProperty(PropertyName = "participantid")]
        public int? ParticipantId { get; set; }

        [JsonProperty(PropertyName = "addressid")]
        public int AddressId { get; set; }

        [JsonProperty(PropertyName = "city")]
        public string City { get; set; }

        [JsonProperty(PropertyName = "state")]
        public string State { get; set; }

        [JsonProperty(PropertyName = "zip")]
        public string Zip { get; set; }

        [JsonProperty(PropertyName = "latlong")]
        public string LatLong { get; set; }

        [JsonProperty(PropertyName = "hoststatus")]
        public int? HostStatus { get; set; }

        [JsonProperty(PropertyName = "groupid")]
        public int? GroupId { get; set; }

        [JsonProperty(PropertyName = "groupname")]
        public string GroupName { get; set; }

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
    }
}
﻿using System;
using Newtonsoft.Json;

namespace crds_angular.Models.Crossroads.Camp
{
    public class CampFamilyMember
    {
        [JsonProperty(PropertyName = "contactId")]
        public int ContactId { get; set; }

        [JsonProperty(PropertyName = "preferredName")]
        public string PreferredName { get; set; }

        [JsonProperty(PropertyName = "lastName")]
        public string LastName { get; set; }

        [JsonProperty(PropertyName = "isEligible")]
        public bool IsEligible { get; set; }

        [JsonProperty(PropertyName = "signedUpDate")]
        public DateTime? SignedUpDate { get; set; }

        [JsonProperty(PropertyName = "isSignedUp")]
        public bool IsSignedUp { get; set; }

        [JsonProperty(PropertyName = "isPending")]
        public bool IsPending { get; set; }

        [JsonProperty(PropertyName = "isExpired")]
        public bool IsExpired { get; set; }

        [JsonProperty(PropertyName = "isCancelled")]
        public bool IsCancelled { get; set; }

        [JsonProperty(PropertyName = "endDate")]
        public DateTime? EndDate { get; set; }

    }
}
﻿using System.Collections.Generic;
using Newtonsoft.Json;
using System;

namespace crds_angular.Models.Crossroads
{
    public class EmailCommunicationDTO
    {
        [JsonProperty(PropertyName = "fromContactId")]
        public int? FromContactId { get; set; }

        [JsonProperty(PropertyName = "replyToContact")]
        public int? ReplyToContactId { get; set; }

        [JsonProperty(PropertyName = "fromUserId", NullValueHandling = NullValueHandling.Ignore)]
        public int? FromUserId { get; set; }

        [JsonProperty(PropertyName = "toContactId")]
        public int? ToContactId { get; set; }

        [JsonProperty(PropertyName = "templateId")]
        public int TemplateId { get; set; }

        [JsonProperty(PropertyName = "mergeData")]
        public Dictionary<string, object> MergeData { get; set; }

        [JsonProperty(PropertyName = "groupId", NullValueHandling = NullValueHandling.Ignore)]
        public int? groupId { get; set; }

        [JsonProperty(PropertyName = "emailAddress")]
        public string emailAddress { get; set; }

        [JsonProperty(PropertyName = "startDate", NullValueHandling = NullValueHandling.Ignore)]
        public DateTime? StartDate { get; set; }
    }
}
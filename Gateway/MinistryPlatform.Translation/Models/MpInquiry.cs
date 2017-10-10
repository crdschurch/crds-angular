using System;
using Crossroads.Web.Common.MinistryPlatform;
using Newtonsoft.Json;

namespace MinistryPlatform.Translation.Models
{
    [MpRestApiTable(Name = "Group_Inquiries")]
    public class MpInquiry
    {
        [JsonProperty(PropertyName = "Group_Inquiry_ID")]
        public int InquiryId { get; set; }
        [JsonProperty(PropertyName = "Group_ID")]
        public int GroupId { get; set; }
        [JsonProperty(PropertyName = "Contact_ID")]
        public int ContactId { get; set; }
        [JsonProperty(PropertyName = "Inquiry_Date")]
        public DateTime RequestDate { get; set; }
        [JsonProperty(PropertyName = "First_Name")]
        public string FirstName { get; set; }
        [JsonProperty(PropertyName = "Last_Name")]
        public string LastName { get; set; }
        [JsonProperty(PropertyName = "Phone")]
        public string PhoneNumber { get; set; }
        [JsonProperty(PropertyName = "Email")]
        public string EmailAddress { get; set; }
        [JsonProperty(PropertyName = "Placed")]
        public bool? Placed { get; set; }
    }

}
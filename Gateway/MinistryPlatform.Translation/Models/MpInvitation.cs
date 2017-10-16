using System;
using Crossroads.Web.Common.MinistryPlatform;
using Newtonsoft.Json;

namespace MinistryPlatform.Translation.Models
{
    [MpRestApiTable(Name = "cr_Invitations")]
    public class MpInvitation
    {
        [JsonProperty(PropertyName = "Invitation_ID")]
        public int InvitationId { get; set; }

        [JsonProperty(PropertyName = "Source_ID")]
        public int SourceId { get; set; }

        [JsonProperty(PropertyName = "Group_Role_ID")]
        public int? GroupRoleId { get; set; }

        [JsonProperty(PropertyName = "Email_Address")]
        public string EmailAddress { get; set; }

        [JsonProperty(PropertyName = "Recipient_Name")]
        public string RecipientName { get; set; }

        [JsonProperty(PropertyName = "Invitation_Date")]
        public DateTime RequestDate { get; set; }

        [JsonProperty(PropertyName = "Invitation_Type_ID")]
        public int InvitationType { get; set; }

        [JsonProperty(PropertyName = "Invitation_GUID")]
        public string InvitationGuid { get; set; }

        public string CustomMessage { get; set; }
    }

}
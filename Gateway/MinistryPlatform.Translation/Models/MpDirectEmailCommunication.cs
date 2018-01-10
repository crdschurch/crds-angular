using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Crossroads.Web.Common.MinistryPlatform;
using Newtonsoft.Json;

namespace MinistryPlatform.Translation.Models
{
    [MpRestApiTable(Name = "dp_Communication_Messages")]
    public class MpDirectEmailCommunication
    {
        [JsonProperty(PropertyName = "Communication_Message_ID")] 
        public int CommunicationMessageId { get; set; }
        public string From { get; set; }
        public string To { get; set; }
        [JsonProperty(PropertyName = "Reply_To")]
        public string ReplyTo { get; set; }
        public string Subject { get; set; }
        public string Body { get; set; }
    }
}

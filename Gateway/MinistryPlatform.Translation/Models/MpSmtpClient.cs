using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Crossroads.Web.Common.MinistryPlatform;
using Newtonsoft.Json;

namespace MinistryPlatform.Translation.Models
{
    [MpRestApiTable(Name = "dp_Domains")]
    public class MpSmtpClient
    {
        [JsonProperty(PropertyName = "SMTP_Server_Port")]
        public int Port { get; set; }
        [JsonProperty(PropertyName = "SMTP_Server_Name")]
        public string Host { get; set; }
        [JsonProperty(PropertyName = "SMTP_Enable_SSL")]
        public bool EnableSsl { get; set; }
    }
}

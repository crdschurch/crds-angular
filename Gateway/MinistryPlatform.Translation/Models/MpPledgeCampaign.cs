using System;
using System.Collections.Generic;
using Crossroads.Web.Common.MinistryPlatform;
using Newtonsoft.Json;

namespace MinistryPlatform.Translation.Models
{
    [MpRestApiTable(Name = "Pledge_Campaigns")]
    public class MpPledgeCampaign
    {
        [JsonProperty(PropertyName = "Pledge_Campaign_ID")]
        public int Id { get; set; }

        [JsonProperty(PropertyName = "Campaign_Name")]
        public string Name { get; set; }

        [JsonProperty(PropertyName = "Campaign_Type")]
        public string Type { get; set; }

        [JsonProperty(PropertyName = "Start_Date")]
        public DateTime StartDate { get; set; }

        [JsonProperty(PropertyName = "End_Date")]
        public DateTime EndDate { get; set; }

        [JsonProperty(PropertyName = "Campaign_Goal")]
        public double Goal { get; set; }

        [JsonProperty(PropertyName = "Form_ID")]
        public int FormId { get; set; }

        [JsonProperty(PropertyName = "Nickname")]
        public string Nickname { get; set; }

        [JsonProperty(PropertyName = "Youngest_Age_Allowed")]
        public int YoungestAgeAllowed { get; set; }

        [JsonProperty(PropertyName = "Registration_Start")]
        public DateTime RegistrationStart { get; set; }

        [JsonProperty(PropertyName = "Registration_End")]
        public DateTime RegistrationEnd { get; set; }

        [JsonProperty(PropertyName = "Registration_Deposit")]
        public string RegistrationDeposit { get; set; }


        public List<int> AgeExceptions { get; set; }

        [JsonProperty(PropertyName = "Event_ID")]
        public int EventId { get; set; }

        [JsonProperty(PropertyName = "Event_Start_Date")]
        public DateTime EventStart { get; set; }

        [JsonProperty(PropertyName = "Program_ID")]
        public int ProgramId { get; set; }

        [JsonProperty(PropertyName = "Maximum_Registrants")]
        public int? MaximumRegistrants { get; set; }
    }
}
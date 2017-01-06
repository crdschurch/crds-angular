﻿using MinistryPlatform.Translation.Models.Attributes;
using Newtonsoft.Json;

namespace MinistryPlatform.Translation.Models
{
    [MpRestApiTable(Name = "cr_Medical_Information")]
    public class MpMedicalInformation
    {
        [JsonProperty(PropertyName = "Contact_ID")]
        public int ContactId { get; set; }

        [JsonProperty(PropertyName = "MedicalInformation_ID")]
        public int MedicalInformationId { get; set; }

        [JsonProperty(PropertyName = "InsuranceCompany")]
        public string InsuranceCompany { get; set; }

        [JsonProperty(PropertyName = "PolicyHolderName")]
        public string PolicyHolder { get; set; }

        [JsonProperty(PropertyName = "PhysicianName")]
        public string PhysicianName { get; set; }

        [JsonProperty(PropertyName = "PhysicianPhone")]
        public string PhysicianPhone { get; set; }

        [JsonProperty(PropertyName = "Allowed_To_Administer_Medications")]
        public string MedicationsAdministered { get; set; }
    }
    
}


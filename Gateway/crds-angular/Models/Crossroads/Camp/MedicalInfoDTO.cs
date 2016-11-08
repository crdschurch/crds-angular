﻿using System.Collections.Generic;
using Newtonsoft.Json;

namespace crds_angular.Models.Crossroads.Camp
{
    public class MedicalInfoDTO
    {
        [JsonProperty(PropertyName = "contactId")]
        public int ContactId { get; set; }

        [JsonProperty(PropertyName = "medicalInformationId")]
        public int MedicalInformationId { get; set; }

        [JsonProperty(PropertyName = "insuranceCompany")]
        public string InsuranceCompany { get; set; }

        [JsonProperty(PropertyName = "policyHolder")]
        public string PolicyHolder { get; set; }

        [JsonProperty(PropertyName = "physicianName")]
        public string PhysicianName { get; set; }

        [JsonProperty(PropertyName = "physicianPhone")]
        public string PhysicianPhone { get; set; }

        [JsonProperty(PropertyName = "showAllergies")]
        public bool ShowAllergies { get; set; }

        [JsonProperty(PropertyName = "allergies")]
        public List<Allergy> Allergies  { get; set; }
    }

    public class Allergy
    {
        [JsonProperty(PropertyName = "allergyType")]
        public string AllergyType { get; set; }

        [JsonProperty(PropertyName = "allergyDescription")]
        public string AllergyDescription { get; set; }
    }
   
}
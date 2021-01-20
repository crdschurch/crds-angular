using Crossroads.Web.Common.MinistryPlatform;
using Newtonsoft.Json;

namespace MinistryPlatform.Translation.Models
{
    [MpRestApiTable(Name = "cr_Medical_Information_Allergies")]
    public class MpMedical
    {
        [JsonProperty(PropertyName = "Allergy_ID")]
        public int AllergyId { get; set; }

        [JsonProperty(PropertyName = "Allergy_Type_ID")]
        public int AllergyTypeId { get; set; }
            
        [JsonProperty(PropertyName = "Allergy_Type")]
        public string AllergyType { get; set; }

        [JsonProperty(PropertyName = "Description")]
        public string AllergyDescription { get; set; }

        [JsonProperty(PropertyName = "MedicalInformationId")]
        public int MedicalInformationId { get; set; }

        [JsonProperty(PropertyName = "InsuranceCompany")]
        public string InsuranceCompany { get; set; }

        [JsonProperty(PropertyName = "PolicyHolderName")]
        public string PolicyHolderName { get; set; }

        [JsonProperty(PropertyName = "PhysicianName")]
        public string PhysicianName { get; set; }

        [JsonProperty(PropertyName = "PhysicianPhone")]
        public string PhysicianPhone { get; set; }

        [JsonProperty(PropertyName = "Medical_Information_Allergy_ID")]
        public int MedicalInfoAllergyId { get; set; }
    }
}
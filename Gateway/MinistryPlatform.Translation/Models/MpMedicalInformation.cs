using Crossroads.Web.Common.MinistryPlatform;
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

        [JsonProperty(PropertyName = "Insurance_Company")]
        public string InsuranceCompany { get; set; }

        [JsonProperty(PropertyName = "Policy_Holder_Name")]
        public string PolicyHolder { get; set; }

        [JsonProperty(PropertyName = "Physician_Name")]
        public string PhysicianName { get; set; }

        [JsonProperty(PropertyName = "Physician_Phone")]
        public string PhysicianPhone { get; set; }

        [JsonProperty(PropertyName = "Allowed_To_Administer_Medications")]
        public string MedicationsAdministered { get; set; }
    }
    
}


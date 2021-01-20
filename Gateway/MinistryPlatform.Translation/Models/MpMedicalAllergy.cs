using Crossroads.Web.Common.MinistryPlatform;
using Newtonsoft.Json;

namespace MinistryPlatform.Translation.Models
{
    [MpRestApiTable(Name = "cr_Medical_Information_Allergies")]
    public class MpMedicalAllergy
    {
        [JsonProperty(PropertyName = "Medical_Information_Allergy_ID")]
        public int MedicalInfoAllergyId { get; set; }

        [JsonProperty(PropertyName = "Medical_Information_ID")]
        public int MedicalInformationId { get; set; }

        [JsonProperty(PropertyName = "Allergy_ID")]
        public MpAllergy Allergy { get; set; }
    }
}

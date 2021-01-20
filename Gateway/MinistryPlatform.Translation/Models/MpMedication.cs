using Crossroads.Web.Common.MinistryPlatform;
using Newtonsoft.Json;

namespace MinistryPlatform.Translation.Models
{
    [MpRestApiTable(Name = "cr_Medical_Information_Medications")]
    public class MpMedication
    {
        [MpRestApiPrimaryKey("MedicalInformationMedication_ID")]
        [JsonProperty(PropertyName = "MedicalInformationMedication_ID")]
        public int MedicalInformationMedicationId { get; set; }

        [JsonProperty(PropertyName = "MedicalInformation_ID")]
        public int MedicalInformationId { get; set; }

        [JsonProperty(PropertyName = "Medication_Name")]
        public string MedicationName { get; set; }

        [JsonProperty(PropertyName = "Medication_Type_ID")]
        public int MedicationTypeId { get; set; }

        [JsonProperty(PropertyName = "Dosage_Time")]
        public string DosageTimes { get; set; }

        [JsonProperty(PropertyName = "Dosage_Amount")]
        public string DosageAmount { get; set; }

        public bool Deleted { get; set; }
    }

    [MpRestApiTable(Name = "cr_Medication_Types")]
    public class MpMedicationType
    {
        [JsonProperty(PropertyName = "Medication_Type_ID")]
        public int MedicationTypeId { get; set; }

        [JsonProperty(PropertyName = "Medication_Type")]
        public string MedicationType { get; set; }
    }
}

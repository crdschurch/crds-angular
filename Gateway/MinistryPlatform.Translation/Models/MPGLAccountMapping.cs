using Crossroads.Web.Common.MinistryPlatform;
using Newtonsoft.Json;

namespace MinistryPlatform.Translation.Models
{
    [MpRestApiTable(Name = "GL_Account_Mapping")]
    public class MPGLAccountMapping
    {
        [JsonProperty(PropertyName = "Program_ID")]
        public int ProgramId { get; set; }

        [JsonProperty(PropertyName = "Congregation_ID")]
        public int CongregationId { get; set; }

        [JsonProperty(PropertyName = "GL_Account")]
        public string GLAccount { get; set; }

        [JsonProperty(PropertyName = "Checkbook_ID")]
        public string CheckbookId { get; set; }

        [JsonProperty(PropertyName = "Cash_Account")]
        public string CashAccount { get; set; }

        [JsonProperty(PropertyName = "Receivable_Account")]
        public string ReceivableAccount { get; set; }

        [JsonProperty(PropertyName = "Distribution_Account")]
        public string DistributionAccount { get; set; }

        [JsonProperty(PropertyName = "Document_Type")]
        public string DocumentType { get; set; }

        [JsonProperty(PropertyName = "Customer_ID")]
        public string CustomerId { get; set; }

        [JsonProperty(PropertyName = "Scholarship_Expense_Account")]
        public string ScholarshipExpenseAccount { get; set; }

        [JsonProperty(PropertyName = "Processor_Fee_Mapping_ID")]
        public int? ProcessorFeeMappingId { get; set; }
    }
}

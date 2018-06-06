using Crossroads.Web.Common.MinistryPlatform;
using Newtonsoft.Json;

namespace MinistryPlatform.Translation.Models.Product
{
    [MpRestApiTable(Name = "Products")]
    public class MpProduct
    {
        [JsonProperty(PropertyName = "Product_ID")]
        public int ProductId { get; set; }

        [JsonProperty(PropertyName = "Product_Name")]
        public string ProductName { get; set; }

        [JsonProperty(PropertyName = "Base_Price")]
        public decimal BasePrice { get; set; }

        [JsonProperty(PropertyName = "Deposit_Price")]
        public decimal? DepositPrice { get; set; }

        [JsonProperty(PropertyName = "Program_Id")]
        public int? ProgramId { get; set; }
    }
}

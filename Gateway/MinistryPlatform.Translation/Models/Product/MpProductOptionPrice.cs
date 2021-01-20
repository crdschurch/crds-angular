using Crossroads.Web.Common.MinistryPlatform;
using Newtonsoft.Json;

namespace MinistryPlatform.Translation.Models.Product
{
    [MpRestApiTable(Name = "Product_Option_Prices")]
    public class MpProductOptionPrice
    {
        [JsonProperty(PropertyName = "Product_Option_Price_ID")]
        public int ProductOptionPriceId { get; set; }

        [JsonProperty(PropertyName = "Option_Title")]
        public string OptionTitle { get; set; }

        [JsonProperty(PropertyName = "Option_Price")]
        public decimal OptionPrice { get; set; }

        [JsonProperty(PropertyName = "Days_Out_To_Hide")]
        public int? DaysOutToHide { get; set; }
    }
}

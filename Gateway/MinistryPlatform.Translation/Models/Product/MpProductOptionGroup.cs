using Crossroads.Web.Common.MinistryPlatform;
using Newtonsoft.Json;

namespace MinistryPlatform.Translation.Models.Product
{
    [MpRestApiTable(Name = "Product_Option_Groups")]
    public class MpProductOptionGroup
    {
        [JsonProperty(PropertyName = "Product_Option_Group_ID")]
        public int ProductOptionGroupId { get; set; }

        [JsonProperty(PropertyName = "Product_ID")]
        public int ProductId { get; set; }
    }
}


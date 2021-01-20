using Crossroads.Web.Common.MinistryPlatform;

namespace MinistryPlatform.Translation.Models
{

    [MpRestApiTable(Name = "Attribute_Categories")]
    public class MpAttributeCategory
    {
        public int Attribute_Category_ID { get; set; }
        public MpAttribute Attribute { get; set; }
        public string Description { get; set; }
        public string Example_Text { get; set; }
        public bool Requires_Active_Attribute { get; set; }
        public string Attribute_Category { get; set; }
        public int? Sort_Order { get; set; }
    }
}
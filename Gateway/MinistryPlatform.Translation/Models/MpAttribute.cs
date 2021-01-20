using System;
using Crossroads.Web.Common.MinistryPlatform;
using Newtonsoft.Json;

namespace MinistryPlatform.Translation.Models
{
    [MpRestApiTable(Name = "Attributes")]
    public class MpAttribute
    {
        [JsonProperty(PropertyName = "Attribute_ID")]
        public int AttributeId { get; set; }
        [JsonProperty(PropertyName = "Attribute_Name")]
        public string Name { get; set; }
        public string Description { get; set; }
        [JsonProperty(PropertyName = "Attribute_Category")]
        public string Category { get; set; }
        [JsonProperty(PropertyName = "Attribute_Category_ID")]
        public int? CategoryId { get; set; }
        [JsonProperty(PropertyName = "Attribute_Category_Description")]
        public string CategoryDescription { get; set; }
        public bool? Selected { get; set; }
        [JsonProperty(PropertyName = "Attribute_Type_ID")]
        public int AttributeTypeId { get; set; }
        [JsonProperty(PropertyName = "Attribute_Type")]
        public string AttributeTypeName { get; set; }
        [JsonProperty(PropertyName = "Prevent_Multiple_Selection")]
        public bool PreventMultipleSelection { get; set; }
        [JsonProperty(PropertyName = "Sort_Order")]
        public int SortOrder { get; set; }
        [JsonProperty(PropertyName = "End_Date")]
        public DateTime? EndDate { get; set; }
        [JsonProperty(PropertyName = "Start_Date")]
        public DateTime? StartDate { get; set; }
    }
}
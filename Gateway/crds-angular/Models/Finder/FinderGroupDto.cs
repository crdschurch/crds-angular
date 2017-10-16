using System.Collections.Generic;
using crds_angular.Models.Crossroads.Groups;
using Newtonsoft.Json;

namespace crds_angular.Models.Finder
{
    public class FinderGroupDto : GroupDTO
    {
        [JsonProperty("categories")]
        public List<string> GroupCategoriesList { get; set; }

        [JsonProperty("ageRanges")]
        public List<string> GroupAgesRangeList { get; set; }

        [JsonProperty("groupType")]
        public string GroupType { get; set; }

        [JsonProperty("isVirtualGroup")]
        public bool VirtualGroup { get; set; }

        [JsonProperty("primaryContactFirstName")]
        public string PrimaryContactFirstName { get; set; }

        [JsonProperty("primaryContactLastName")]
        public string PrimaryContactLastName { get; set; }

        [JsonProperty("primaryContactCongregation")]
        public string PrimaryContactCongregation { get; set; }
    }
}
using Newtonsoft.Json;

namespace crds_angular.Models.Crossroads.GoVolunteer
{
    public class ProjectType
    {
        [JsonProperty(PropertyName = "projectTypeId")]
        public int ProjectTypeId { get; set; }

        [JsonProperty(PropertyName="desc")]
        public string Description { get; set; }

        [JsonProperty(PropertyName = "minAge")]
        public int MinAge { get; set; }

        [JsonProperty(PropertyName = "sortOrder")]
        public int SortOrder { get; set; }

        [JsonProperty(PropertyName = "imageUrl")]
        public string ImageUrl { get; set; }
    }
}
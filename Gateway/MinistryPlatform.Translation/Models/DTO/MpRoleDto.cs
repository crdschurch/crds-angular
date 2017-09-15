using Crossroads.Web.Common.MinistryPlatform;
using Newtonsoft.Json;

namespace MinistryPlatform.Translation.Models.DTO
{
    [MpRestApiTable(Name = "dp_User_Roles")]
    public class MpRoleDto
    {
        [JsonProperty(PropertyName = "Role_ID")]
        public int Id { get; set; }

        [JsonProperty(PropertyName = "Role_Name")]
        public string Name { get; set; }

    }
}
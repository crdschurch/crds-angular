using Newtonsoft.Json;
namespace crds_angular.Models.Finder
{
    public class GroupParticipantRemovalDto
    {
        [JsonProperty("groupId")]
        public int GroupId;
        [JsonProperty("groupParticipantId")]
        public int GroupParticipantId;
        [JsonProperty("message")]
        public string Message;
    }
}
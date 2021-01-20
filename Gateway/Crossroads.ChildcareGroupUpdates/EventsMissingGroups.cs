using Crossroads.Web.Common.MinistryPlatform;
using Newtonsoft.Json;

namespace Crossroads.ChildcareGroupUpdates
{
    [MpRestApiTable(Name = "EventsMissingGroups")]
    public class MpEventsMissingGroups
    {
        [JsonProperty(PropertyName = "Event_ID")]
        public int EventId;

        [JsonProperty(PropertyName = "Group_ID")]
        public int GroupId;
    }

    [MpRestApiTable(Name = "OrphanEventsMissingGroups")]
    public class MpOrphanEventsMissingGroups
    {
        [JsonProperty(PropertyName = "Event_ID")]
        public int EventId;
    }
}

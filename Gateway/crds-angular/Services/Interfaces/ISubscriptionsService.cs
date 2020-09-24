using System.Collections.Generic;

namespace crds_angular.Services.Interfaces
{
    public interface ISubscriptionsService
    {
        int SetSubscriptions(Dictionary<string, object> subscription, int contactId);
    }
}
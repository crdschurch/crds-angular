using crds_angular.Models.Crossroads.Subscription;

namespace crds_angular.Util.Interfaces
{
    public interface IEmailListHandler
    {
        OptInResponse AddListSubscriber(string email, string listName, string token);
    }
}

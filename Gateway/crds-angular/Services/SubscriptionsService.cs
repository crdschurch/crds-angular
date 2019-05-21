using System;
using System.Collections.Generic;
using crds_angular.Services.Interfaces;
using MPInterfaces = MinistryPlatform.Translation.Repositories.Interfaces;
using crds_angular.Models.Crossroads.Subscription;
using Crossroads.Web.Common;
using Crossroads.Web.Common.MinistryPlatform;

namespace crds_angular.Services
{
    public class SubscriptionsService : MinistryPlatformBaseService, ISubscriptionsService
    {
        private readonly MPInterfaces.IMinistryPlatformService _ministryPlatformService;
        private readonly Util.Interfaces.IEmailListHandler _emailListHandler;
        private readonly IApiUserRepository _apiUserService;
        public SubscriptionsService(MPInterfaces.IMinistryPlatformService ministryPlatformService, Util.Interfaces.IEmailListHandler emailListHandler, IApiUserRepository apiUserService)
        {
            _ministryPlatformService = ministryPlatformService;
            _emailListHandler = emailListHandler;
            _apiUserService = apiUserService;
        }
		public List<Dictionary<string, object>> GetSubscriptions(int contactId)
        {
			var token = _apiUserService.GetDefaultApiClientToken();
            var publications = _ministryPlatformService.GetRecordsDict("Publications", token, ",,,,True", "8 asc");
            var subscriptions = _ministryPlatformService.GetSubPageRecords("SubscriptionsSubPage", contactId, token);
            foreach (var publication in publications)
            {
                object unSub = true;
                foreach (var subscription in subscriptions)
                {
                    object spID;
                    object pID;
                    subscription.TryGetValue("Publication_ID", out spID);
                    publication.TryGetValue("ID", out pID);
                    if ((int)pID == (int)spID)
                    {
                        subscription.TryGetValue("Unsubscribed", out unSub);
                        publication.Add("Subscription", subscription);
                        break;
                    }
                }
                publication.Add("Subscribed", !(bool)unSub);
            }
            return publications;
        }

        public int SetSubscriptions(Dictionary<string, object> subscription, int contactId)
        {
			var token = _apiUserService.GetDefaultApiClientToken();
			object spID;
            if (subscription.TryGetValue("dp_RecordID", out spID))
            {
				subscription.Add("Contact_Publication_ID", spID);
                _ministryPlatformService.UpdateSubRecord("SubscriptionsSubPage", subscription, token);
                return Convert.ToInt32(spID);
            }
			return _ministryPlatformService.CreateSubRecord("SubscriptionsSubPage", contactId, subscription, token);
        }

        public OptInResponse AddListSubscriber(string emailAddress, string listName)
        {
            var token = _apiUserService.GetDefaultApiClientToken();
            return _emailListHandler.AddListSubscriber(emailAddress, listName, token);
        }
    }
}
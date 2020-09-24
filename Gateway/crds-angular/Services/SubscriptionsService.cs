using System;
using System.Collections.Generic;
using crds_angular.Services.Interfaces;
using MPInterfaces = MinistryPlatform.Translation.Repositories.Interfaces;
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
    }
}
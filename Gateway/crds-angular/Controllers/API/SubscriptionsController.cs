using System.Collections.Generic;
using System.Web.Http;
using System.Web.Http.Description;
using crds_angular.Security;
using crds_angular.Services.Interfaces;
using MPInterfaces = MinistryPlatform.Translation.Repositories.Interfaces;
using crds_angular.Models.Crossroads.Subscription;
using Crossroads.ApiVersioning;
using Crossroads.Web.Common;
using Crossroads.Web.Common.Security;

namespace crds_angular.Controllers.API
{
    public class SubscriptionsController : ImpersonateAuthBaseController
    {
        private readonly MPInterfaces.IContactRepository _contactRepository;
        private readonly ISubscriptionsService _subscriptionService;
		
	

        public SubscriptionsController(IAuthTokenExpiryService authTokenExpiryService, 
                                       ISubscriptionsService subscriptionService, 
                                       IAuthenticationRepository authenticationService, 
                                       IUserImpersonationService userImpersonationService, 
                                       MPInterfaces.IContactRepository contactRepository)
			                           
            : base(authTokenExpiryService, userImpersonationService, authenticationService)
        {
            _subscriptionService = subscriptionService;
            _contactRepository = contactRepository;
        }

        [ResponseType(typeof (List<Dictionary<string, object>>))]
        [VersionedRoute(template: "subscriptions", minimumVersion: "1.0.0")]
        [Route("subscriptions")]
        [HttpGet]
        public IHttpActionResult Get()
        {
            return (Authorized(authDto =>
            {
        
              return (Ok(_subscriptionService.GetSubscriptions(authDto.UserInfo.Mp.ContactId)));
            }));
        }

        [VersionedRoute(template: "subscriptions", minimumVersion: "1.0.0")]
        [Route("subscriptions")]
        [HttpPost]
        public IHttpActionResult Post(Dictionary<string, object> subscription)
        {
            return (Authorized(authDto =>
            {
                var recordId = new {dp_RecordID = _subscriptionService.SetSubscriptions(subscription,authDto.UserInfo.Mp.ContactId)};
                return this.Ok(recordId);
            }));
        }

        [VersionedRoute(template: "subscriptions/opt-in", minimumVersion: "1.0.0")]
        [Route("subscriptions/optin")]
        [HttpPost]
        public IHttpActionResult PostOptIn(OptInRequest request)
        {
            try
            {
                var response = _subscriptionService.AddListSubscriber(request.EmailAddress, request.ListName);
                return this.Ok(response);
            }
            catch (System.Exception ex)
            {
                return this.InternalServerError();
            }
        }
    }
}

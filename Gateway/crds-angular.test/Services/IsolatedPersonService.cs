using crds_angular.Services;
using crds_angular.Services.Interfaces;
using Crossroads.Web.Common.MinistryPlatform;
using Crossroads.Web.Common.Security;
using Crossroads.Web.Common.Configuration;
using MPServices = MinistryPlatform.Translation.Repositories.Interfaces;
using System.Net.Http;
using crds_angular.Models.Crossroads.Profile;

namespace crds_angular.test.Services
{
    class IsolatedPersonService : PersonService
    {
        protected override HttpClient client { get; }
        
        public IsolatedPersonService(MPServices.IContactRepository contactService,
            IObjectAttributeService objectAttributeService,
            IApiUserRepository apiUserService,
            MPServices.IParticipantRepository participantService,
            MPServices.IUserRepository userService,
            IAuthenticationRepository authenticationService,
            IAddressService addressService,
            IAnalyticsService analyticsService,
            IConfigurationWrapper configurationWrapper, 
            ILoginService loginService,
            HttpClient fakeClient) : base(contactService, objectAttributeService, apiUserService, participantService, userService, authenticationService, addressService, analyticsService, configurationWrapper, loginService)
        {
            client = fakeClient;
        }
        public void IsolatedUpdateUsernameOrPasswordIfNeeded(Person person, string UserAccessToken)
        {
            UpdateUsernameOrPasswordIfNeeded(person, UserAccessToken);
        }
    }
}

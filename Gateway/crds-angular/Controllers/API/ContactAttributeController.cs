using System.Web.Http;
using crds_angular.Models.Crossroads.Attribute;
using crds_angular.Security;
using crds_angular.Services.Interfaces;
using MinistryPlatform.Translation.Models;
using Crossroads.ApiVersioning;
using Crossroads.Web.Common.Security;

namespace crds_angular.Controllers.API
{
    public class ContactAttributeController : ImpersonateAuthBaseController
    {
        private readonly IObjectAttributeService _objectAttributeService;

        public ContactAttributeController(IAuthTokenExpiryService authTokenExpiryService, 
                                          IPersonService personService, 
                                          IObjectAttributeService objectAttributeService, 
                                          IUserImpersonationService userImpersonationService, 
                                          IAuthenticationRepository authenticationRepository) 
            : base(authTokenExpiryService, userImpersonationService, authenticationRepository)
        {
            _objectAttributeService = objectAttributeService;
        }

        [VersionedRoute(template: "contact/attribute/{contactId}", minimumVersion: "1.0.0")]
        [Route("contact/attribute/{contactId}")]
        public IHttpActionResult Post(int contactId, [FromBody] ObjectAttributeDTO objectAttribute)
        {
            return Authorized(token =>
            {
                var configuration = MpObjectAttributeConfigurationFactory.MyContact();
                _objectAttributeService.SaveObjectMultiAttribute(contactId, objectAttribute, configuration);
                return this.Ok();
            });
        }
    }
}
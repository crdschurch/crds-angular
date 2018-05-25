using System.Collections.Generic;
using System.Web.Http;
using System.Web.Http.Description;
using crds_angular.Models.Crossroads.Attribute;
using crds_angular.Security;
using crds_angular.Services.Interfaces;
using Crossroads.ApiVersioning;
using Crossroads.Web.Common.Security;

namespace crds_angular.Controllers.API
{
    public class AttributeTypeController : MPAuth
    {
        private readonly IAttributeService _attributeService;

        public AttributeTypeController(IAuthTokenExpiryService authTokenExpiryService, 
                                       IAttributeService attributeService, 
                                       IUserImpersonationService userImpersonationService, 
                                       IAuthenticationRepository authenticationRepository) 
            : base(authTokenExpiryService, userImpersonationService, authenticationRepository)
        {
            _attributeService = attributeService;
        }

        [ResponseType(typeof (List<AttributeTypeDTO>))]
        [VersionedRoute(template: "attribute-type", minimumVersion: "1.0.0")]
        [Route("attributetype")]
        public IHttpActionResult Get()
        {
            var attributeTypes = _attributeService.GetAttributeTypes(null);
            return this.Ok(attributeTypes);
        }

        [ResponseType(typeof(AttributeTypeDTO))]
        [VersionedRoute(template: "attribute-type/{attributeTypeId}", minimumVersion: "1.0.0")]
        [Route("attributetype/{attributeTypeId}")]
        public IHttpActionResult Get(int attributeTypeId)
        {
            var attributeTypes = _attributeService.GetAttributeTypes(attributeTypeId);
            return this.Ok(attributeTypes[0]);
        }
    }
}

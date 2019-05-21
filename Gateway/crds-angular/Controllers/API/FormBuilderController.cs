
using System;
using System.Collections.Generic;
using System.Reflection;
using System.Web.Http;
using System.Web.Http.Description;
using crds_angular.Models.Crossroads.Groups;
using crds_angular.Security;
using crds_angular.Services.Interfaces;
using MinistryPlatform.Translation.Repositories.Interfaces;
using log4net;
using Crossroads.ApiVersioning;
using Crossroads.Web.Common.Security;

namespace crds_angular.Controllers.API
{
    public class FormBuilderController : MPAuth
    {
        private readonly ILog _logger = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);
        private readonly IMinistryPlatformService _ministryPlatformService;
        private readonly Services.Interfaces.IFormBuilderService _formBuilderService;

        public FormBuilderController(IAuthTokenExpiryService authTokenExpiryService, 
                                     Services.Interfaces.IFormBuilderService formBuilderService,
                                     IMinistryPlatformService ministryPlatformService, 
                                     IUserImpersonationService userImpersonationService, 
                                     IAuthenticationRepository authenticationRepository) 
            : base(authTokenExpiryService, userImpersonationService, authenticationRepository)
        {
            _formBuilderService = formBuilderService;
            _ministryPlatformService = ministryPlatformService;
        }

        /// <summary>
        /// Get groups associated with a group key passed in
        /// </summary>
        [RequiresAuthorization]
        [ResponseType(typeof(List<GroupDTO>))]
        [VersionedRoute(template: "form-builder/groups/{templateType}", minimumVersion: "1.0.0")]
        [Route("formbuilder/groups/{templateType}")]
        [HttpGet]
        public IHttpActionResult GetGroupsUndivided(string templateType)
        {
            return Authorized(token =>
            {
                try
                {
                    var groupsUndivided = _formBuilderService.GetGroupsUndivided(templateType);
                    return Ok(groupsUndivided);
                }
                catch (Exception e)
                {
                    _logger.Error("Could not get undivided groups", e);
                    return BadRequest();
                }
            });
        }

        /// <summary>
        /// Omnibus endpoint for form builder
        /// </summary>
        [VersionedRoute(template: "form-builder/huge-end-point", minimumVersion: "1.0.0")]
        [Route("formbuilder/hugeEndPoint")]
        [HttpPost]
        public IHttpActionResult SaveFormbuilderForm(dynamic jsonModel)
        {
            _logger.Info("Hit the endpoint");
            return Ok();
        }
    }
}

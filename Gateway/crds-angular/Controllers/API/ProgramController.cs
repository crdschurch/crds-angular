using System;
using System.Collections.Generic;
using System.Web.Http;
using System.Web.Http.Description;
using crds_angular.Exceptions.Models;
using crds_angular.Models.Crossroads;
using crds_angular.Security;
using crds_angular.Services.Interfaces;
using Crossroads.ApiVersioning;
using Crossroads.Web.Common.Security;

namespace crds_angular.Controllers.API
{
    public class ProgramController : MPAuth
    {
        private readonly IProgramService _programService;

        public ProgramController(IAuthTokenExpiryService authTokenExpiryService, 
                                 IProgramService programService, 
                                 IUserImpersonationService userImpersonationService, 
                                 IAuthenticationRepository authenticationRepository) 
            : base(authTokenExpiryService, userImpersonationService, authenticationRepository)
        {
            _programService = programService;
        }

        [ResponseType(typeof(IList<ProgramDTO>))]
        [VersionedRoute(template: "all-programs", minimumVersion: "1.0.0")]
        [Route("all-programs")]
        public IHttpActionResult Get()
        {
            return Authorized(token =>
            {
                try
                {
                    var programs = _programService.GetAllProgramsForReal();
                    return this.Ok(programs);
                }
                catch (Exception exception)
                {
                    var apiError = new ApiErrorDto("Get All Programs", exception);
                    throw new HttpResponseException(apiError.HttpResponseMessage);
                }
            });
        }

        [VersionedRoute(template: "programs", minimumVersion: "1.0.0")]
        [Route("programs")]
        [HttpGet]
        public IHttpActionResult GetAllPrograms([FromUri(Name = "excludeTypes")] int[] excludeTypes = null)
        {
            var result = _programService.GetOnlineGivingPrograms();
            if (excludeTypes == null || excludeTypes.Length == 0)
            {
                return (Ok(result));
            }

            foreach (var t in excludeTypes)
            {
                result.RemoveAll(p => p.ProgramType == t);
            }

            return Ok(result);
        }

        [VersionedRoute(template: "programs/{programType}", minimumVersion: "1.0.0")]
        [Route("programs/{programType}")]
        [HttpGet]
        public IHttpActionResult GetProgramsByType(int programType)
        {
            return Ok(_programService.GetOnlineGivingPrograms(programType));
        }

        [VersionedRoute(template: "programs/event-tool", minimumVersion: "1.0.0")]
        [Route("programs/event-tool")]
        [HttpGet]
        public IHttpActionResult GetProgramsForEventTool()
        {
            var programs = _programService.GetProgramsForEventTool();
            return Ok(programs);
        }
    }
}

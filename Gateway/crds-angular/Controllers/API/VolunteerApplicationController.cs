using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Http;
using System.Web.Http.Description;
using crds_angular.Exceptions.Models;
using crds_angular.Models.Crossroads.Serve;
using crds_angular.Models.Crossroads.VolunteerApplication;
using crds_angular.Security;
using crds_angular.Services.Interfaces;
using Crossroads.ApiVersioning;
using Crossroads.Web.Common.Security;

namespace crds_angular.Controllers.API
{
    public class VolunteerApplicationController : MPAuth
    {
        private readonly IVolunteerApplicationService _volunteerApplicationService;


         public VolunteerApplicationController(IAuthTokenExpiryService authTokenExpiryService, 
                                               IVolunteerApplicationService volunteerApplicationService, 
                                               IUserImpersonationService userImpersonationService, 
                                               IAuthenticationRepository authenticationRepository) 
            : base(authTokenExpiryService, userImpersonationService, authenticationRepository)
        {
            _volunteerApplicationService = volunteerApplicationService;
        }

        [VersionedRoute(template: "volunteer-application/adult", minimumVersion: "1.0.0")]
        [Route("volunteer-application/adult")]
        public IHttpActionResult SaveAdult([FromBody] AdultApplicationDto application)
        {
            if (ModelState.IsValid)
            {
                return Authorized(token =>
                {
                    try
                    {
                        _volunteerApplicationService.SaveAdult(application);
                    }
                    catch (Exception exception)
                    {
                        var apiError = new ApiErrorDto("Volunteer Application POST Failed", exception);
                        throw new HttpResponseException(apiError.HttpResponseMessage);
                    }
                    return Ok();
                });
            }

            var errors = ModelState.Values.SelectMany(val => val.Errors).Aggregate("", (current, err) => current + err.ErrorMessage + " ");
            var dataError = new ApiErrorDto("SaveAdult Data Invalid", new InvalidOperationException("Invalid SaveAdult Data" + errors));
            throw new HttpResponseException(dataError.HttpResponseMessage);
        }

        [VersionedRoute(template: "volunteer-application/student", minimumVersion: "1.0.0")]
        [Route("volunteer-application/student")]
        public IHttpActionResult SaveStudent([FromBody] StudentApplicationDto application)
        {
            if (ModelState.IsValid)
            {
                return Authorized(token =>
                {
                    try
                    {
                        _volunteerApplicationService.SaveStudent(application);
                    }
                    catch (Exception exception)
                    {
                        var apiError = new ApiErrorDto("Volunteer Application POST Failed", exception);
                        throw new HttpResponseException(apiError.HttpResponseMessage);
                    }
                    return Ok();
                });
            }

            var errors = ModelState.Values.SelectMany(val => val.Errors)
                .Aggregate("", (current, err) => current + err.ErrorMessage + " ");
            var dataError = new ApiErrorDto("SaveStudent Data Invalid",
                new InvalidOperationException("Invalid SaveStudent Data" + errors));
            throw new HttpResponseException(dataError.HttpResponseMessage);
        }

        [ResponseType(typeof(List<FamilyMember>))]
        [VersionedRoute(template: "volunteer-application/family/{contactId}", minimumVersion: "1.0.0")]
        [Route("volunteer-application/family/{contactId}")]
        public IHttpActionResult GetFamily(int contactId)
        {//TODO: I don't think you need to pass in contactId, use the token instead
            return Authorized(token =>
            {
                try
                {
                    var family = _volunteerApplicationService.FamilyThatUserCanSubmitFor(token);
                    return Ok(family);
                }
                catch (Exception ex)
                {
                    var apiError = new ApiErrorDto("Volunteer Application GET Family", ex);
                    throw new HttpResponseException(apiError.HttpResponseMessage);
                }

            });
        }
    }
}

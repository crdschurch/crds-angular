using System;
using System.Collections.Generic;
using System.Web.Http;
using System.Web.Http.Description;
using crds_angular.Exceptions.Models;
using crds_angular.Models.Crossroads;
using crds_angular.Security;
using crds_angular.Services.Interfaces;
using MinistryPlatform.Translation.Repositories.Interfaces;
using Crossroads.ApiVersioning;
using Crossroads.Web.Common.Security;

namespace crds_angular.Controllers.API
{
    public class StaffContactController : MPAuth
    {
        private readonly crds_angular.Services.Interfaces.IStaffContactService _contactService;

        public StaffContactController(IAuthTokenExpiryService authTokenExpiryService, 
                                      crds_angular.Services.Interfaces.IStaffContactService contactService, 
                                      IUserImpersonationService userImpersonationService, 
                                      IAuthenticationRepository authenticationRepository) 
            : base(authTokenExpiryService, userImpersonationService, authenticationRepository)
        {
            _contactService = contactService;
        }

        // NOTE 6/16/2017: GetStaffContacts() has been replaced by GetPrimaryContacts() for the Create/Edit Event
        // tool, which is the only consumer I was able to find for GetStaffContacts().  However, it's possible
        // this endpoint is used elsewhere, so I'm leaving it for now.

        [ResponseType(typeof(List<PrimaryContactDto>))]
        [VersionedRoute(template: "staff-contacts", minimumVersion: "1.0.0")]
        [Route("staffcontacts")]
        public IHttpActionResult GetStaffContacts()
        {
            return Authorized(t =>
            {
                try
            {
                var users = _contactService.GetStaffContacts();

                return Ok(users);
            }
                catch (Exception e)
                {
                    var msg = "Error getting staff contacts ";
                    logger.Error(msg, e);
                    var apiError = new ApiErrorDto(msg, e);
                    throw new HttpResponseException(apiError.HttpResponseMessage);
                }
            });
        }

        [ResponseType(typeof(List<PrimaryContactDto>))]
        [VersionedRoute(template: "primary-contacts", minimumVersion: "1.0.0")]
        [Route("primarycontacts")]
        public IHttpActionResult GetPrimaryContacts()
        {
            return Authorized(t =>
            {
                try
                {
                    var users = _contactService.GetPrimaryContacts();

                    return Ok(users);
                }
                catch (Exception e)
                {
                    var msg = "Error getting primary contacts ";
                    logger.Error(msg, e);
                    var apiError = new ApiErrorDto(msg, e);
                    throw new HttpResponseException(apiError.HttpResponseMessage);
                }
            });
        }
    }
}

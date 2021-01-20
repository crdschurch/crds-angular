using System;
using System.Net;
using System.Web.Http;
using crds_angular.Exceptions.Models;
using crds_angular.Models.Json;
using crds_angular.Security;
using crds_angular.Services.Interfaces;
using Crossroads.ApiVersioning;
using Crossroads.Web.Common.Security;

namespace crds_angular.Controllers.API
{
    public class EventParticipantController : MPAuth
    {
        private readonly IEventParticipantService _eventParticipantService;

        public EventParticipantController(IAuthTokenExpiryService authTokenExpiryService, 
                                          IEventParticipantService eventParticipantService, 
                                          IUserImpersonationService userImpersonationService, 
                                          IAuthenticationRepository authenticationRepository) 
            : base(authTokenExpiryService, userImpersonationService, authenticationRepository)
        {
            _eventParticipantService = eventParticipantService;
        }

        [VersionedRoute(template: "contact/{contactId}/interested-in/{eventId}", minimumVersion: "1.0.0")]
        [HttpGet]
        public IHttpActionResult IsParticipantValid(int contactId, int eventId)
        {
            return Authorized((token) =>
            {
                try
                {
                    var participant = _eventParticipantService.GetEventParticipantByContactAndEvent(contactId, eventId);
                    if (_eventParticipantService.IsParticipantInvalid(participant))
                    {
                        return NotFound();
                    }                    
                    return Ok();
                }
                catch (Exception e)
                {
                    var apiError = new ApiErrorDto("Error determining if participant is interested",  e);
                    return (RestHttpActionResult<ApiErrorDto>.WithStatus(HttpStatusCode.InternalServerError, apiError));
                }
            });
        }
    }
}
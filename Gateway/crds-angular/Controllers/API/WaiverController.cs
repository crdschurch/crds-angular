using System;
using System.Collections.Generic;
using System.Reactive.Linq;
using System.Threading.Tasks;
using System.Web.Http;
using System.Web.Http.Description;
using crds_angular.Exceptions.Models;
using crds_angular.Models.Crossroads.Waivers;
using crds_angular.Security;
using crds_angular.Services.Interfaces;
using Crossroads.ApiVersioning;
using Crossroads.Web.Common.Security;

namespace crds_angular.Controllers.API
{
    public class WaiverController : ImpersonateAuthBaseController
    {
        private IAuthTokenExpiryService _authTokenExpiryService;
        public readonly IWaiverService _waiverService;

        public WaiverController(
            IAuthTokenExpiryService authTokenExpiryService,
            IWaiverService waiverService,
            IUserImpersonationService userImpersonationService, 
            IAuthenticationRepository authenticationRepository) 
          : base(authTokenExpiryService, userImpersonationService, authenticationRepository)
        {
            _waiverService = waiverService;
        }

        [VersionedRoute(template: "waivers/event/{eventId}", minimumVersion: "1.0.0")]
        [ResponseType(typeof(List<WaiverDTO>))]
        [HttpGet]
        public async Task<IHttpActionResult> GetEventWaivers(int eventId)
        {
            return Authorized(token =>
            {
                try
                {
                    var waivers = _waiverService.EventWaivers(eventId, token.UserInfo.Mp.ContactId).ToList().Wait();
                    return Ok(waivers);
                }
                catch (Exception e)
                {
                    var apiError = new ApiErrorDto("Failed to get Event Waivers", e);
                    throw new HttpResponseException(apiError.HttpResponseMessage);
                }
            });
        }

        [VersionedRoute(template: "waivers/{waiverId}", minimumVersion: "1.0.0")]
        [ResponseType(typeof(WaiverDTO))]
        [HttpGet]
        public async Task<IHttpActionResult> GetWaiver(int waiverId)
        {
            return Authorized(token =>
            {
                try
                {
                    var waiver = _waiverService.GetWaiver(waiverId).Wait();
                    return Ok(waiver);
                }
                catch (Exception e)
                {
                    var apiError = new ApiErrorDto("Failed to get waiver with id {waiverId}", e);
                    throw new HttpResponseException(apiError.HttpResponseMessage);
                }
            });
        }

        [VersionedRoute(template: "waivers/{waiverId}/send/{eventParticipantId}", minimumVersion: "1.0.0")]
        [HttpPost]
        public async Task<IHttpActionResult> SendAcceptWaiverEmail(int waiverId, int eventParticipantId)
        {
            return Authorized(token =>
            {
                try
                {
                    _waiverService.CreateWaiverInvitation(waiverId, eventParticipantId, token.UserInfo.Mp.ContactId).Select(invite => _waiverService.SendAcceptanceEmail(invite).Subscribe()).Wait();                   
                    return Ok();
                }
                catch (Exception e)
                {
                    var apiError = new ApiErrorDto("Failure to create Invitation", e);
                    throw new HttpResponseException(apiError.HttpResponseMessage);
                }
            });
        }

        [VersionedRoute(template: "waivers/accept/{guid}", minimumVersion: "1.0.0")]
        [HttpPost]
        public async Task<IHttpActionResult> AcceptWaiver(string guid)
        {
            try
            {
                var waiverDto = await _waiverService.AcceptWaiver(guid);
                return Ok(waiverDto);
            }
            catch (Exception e)
            {
                var apiError = new ApiErrorDto("Failure to accept waiver", e);
                throw new HttpResponseException(apiError.HttpResponseMessage);
            }
        }
    }
}
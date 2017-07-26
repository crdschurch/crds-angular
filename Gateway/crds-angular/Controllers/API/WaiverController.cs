﻿using System;
using System.Collections.Generic;
using System.Reactive;
using System.Reactive.Disposables;
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
    public class WaiverController : ReactiveMPAuth
    {
        public readonly IWaiverService _waiverService;

        public WaiverController(
            IWaiverService waiverService,
            IUserImpersonationService userImpersonationService, 
            IAuthenticationRepository authenticationRepository) : base(userImpersonationService, authenticationRepository)
        {
            _waiverService = waiverService;
        }

        [VersionedRoute(template: "waivers/event/{eventId}", minimumVersion: "1.0.0")]
        [ResponseType(typeof(List<WaiverDTO>))]
        [HttpGet]
        public async Task<IHttpActionResult> GetEventWaivers(int eventId)
        {
            return await Authorized(token =>
            {
                var waivers = _waiverService.EventWaivers(eventId).ToList().Wait();
                return Ok(waivers);
            });
        }

        [VersionedRoute(template: "waivers/{waiverId}", minimumVersion: "1.0.0")]
        [ResponseType(typeof(WaiverDTO))]
        [HttpGet]
        public async Task<IHttpActionResult> GetWaiver(int waiverId)
        {
            return await Authorized(token =>
            {
                var waiver = _waiverService.GetWaiver(waiverId).Wait();
                return Ok(waiver);
            });
        }

        [VersionedRoute(template: "waivers/{waiverId}/send/{eventParticipantId}", minimumVersion: "1.0.0")]
        [HttpPost]
        public async Task<IHttpActionResult> SendAcceptWaiverEmail(int waiverId, int eventParticipantId)
        {
            return await Authorized(token =>
            {
                try
                {
                    //TODO: make sure the event participant is the logged in user...
                    _waiverService.CreateWaiverInvitation(waiverId, eventParticipantId, token).Select(invite => _waiverService.SendAcceptanceEmail(invite).Subscribe()).Wait();                   
                    return Ok();
                }
                catch (Exception e)
                {
                    var apiError = new ApiErrorDto("Failure to create Invitation", e);
                    throw new HttpResponseException(apiError.HttpResponseMessage);
                }
            });
        }

    }
}
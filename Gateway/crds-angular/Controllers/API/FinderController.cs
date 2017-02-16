﻿using System;
using System.Web.Http;
using System.Web.Http.Description;
using crds_angular.Exceptions.Models;
using crds_angular.Models.Crossroads;
using crds_angular.Models.Finder;
using crds_angular.Security;
using crds_angular.Services.Interfaces;
using Crossroads.ApiVersioning;
using Crossroads.Web.Common.Security;

namespace crds_angular.Controllers.API
{
    public class FinderController : MPAuth
    {
        private readonly IFinderService _finderService;

        public FinderController(IFinderService finderService, IUserImpersonationService userImpersonationService, IAuthenticationRepository authenticationRepository)
            : base(userImpersonationService, authenticationRepository)
        {
            _finderService = finderService;
        }

        [ResponseType(typeof(PinDto))]
        [VersionedRoute(template: "finder/pin/{participantId}", minimumVersion: "1.0.0")]
        [Route("finder/pin/{participantId}")]
        [HttpGet]
        public IHttpActionResult GetPinDetails([FromUri]int participantId)
        {
            try
            {
                var list = _finderService.GetPinDetails(participantId);
                return Ok(list);
            }
            catch (Exception ex)
            {
                var apiError = new ApiErrorDto("Get Pin Details Failed", ex);
                throw new HttpResponseException(apiError.HttpResponseMessage);
            }
        }

        [ResponseType(typeof(PinDto))]
        [VersionedRoute(template: "finder/pin/contact/{contactId}", minimumVersion: "1.0.0")]
        [Route("finder/pin/contact/{contactId}")]
        [HttpGet]
        public IHttpActionResult GetPinDetailsByContact([FromUri]int contactId)
        {
            try
            {
                var participantId = _finderService.GetParticipantIdFromContact(contactId);
                var list = _finderService.GetPinDetails(participantId);
                return Ok(list);
            }
            catch (Exception ex)
            {
                var apiError = new ApiErrorDto("Get Pin Details by Contact Failed", ex);
                throw new HttpResponseException(apiError.HttpResponseMessage);
            }
        }

        [ResponseType(typeof(AddressDTO))]
        [VersionedRoute(template: "finder/pinbyip", minimumVersion: "1.0.0")]
        [Route("finder/pinbyip")]
        [HttpGet]
        public IHttpActionResult GetPinByIpAddress()
        {
            try
            {
                var address = _finderService.GetAddressForIp();
                return Ok(address);
            }
            catch (Exception ex)
            {
                var apiError = new ApiErrorDto("Get Pin By Ip Failed", ex);
                throw new HttpResponseException(apiError.HttpResponseMessage);
            }
        }
    }
}
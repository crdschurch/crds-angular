﻿using System;
using System.Collections.Generic;
using System.Device.Location;
using System.Linq;
using System.Net;
using System.Reflection;
using System.Web.Http;
using System.Web.Http.Description;
using System.Web.Mvc;
using System.Web.Services.Description;
using crds_angular.Exceptions.Models;
using crds_angular.Models.Crossroads;
using crds_angular.Models.Finder;
using crds_angular.Security;
using crds_angular.Services.Interfaces;
using Crossroads.ApiVersioning;
using Crossroads.Web.Common.Security;
using System.ComponentModel.DataAnnotations;
using crds_angular.Models.AwsCloudsearch;
using crds_angular.Models.Crossroads.Groups;
using log4net;

namespace crds_angular.Controllers.API
{
    public class FinderController : MPAuth
    {
        private readonly IAwsCloudsearchService _awsCloudsearchService;
        private readonly IAddressService _addressService;
        private readonly IFinderService _finderService;
        private readonly IAddressGeocodingService _addressGeocodingService;
        private readonly ILog _logger = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

        public FinderController(IAddressService addressService,
                                IAddressGeocodingService addressGeocodingService, 
                                IFinderService finderService,
                                IUserImpersonationService userImpersonationService,
                                IAuthenticationRepository authenticationRepository,
                                IAwsCloudsearchService awsCloudsearchService)
            : base(userImpersonationService, authenticationRepository)
        {
            _addressService = addressService;
            _finderService = finderService;
            _addressGeocodingService = addressGeocodingService;
            _awsCloudsearchService = awsCloudsearchService;
        }

        [ResponseType(typeof(PinDto))]
        [VersionedRoute(template: "finder/pin/{participantId}", minimumVersion: "1.0.0")]
        [System.Web.Http.Route("finder/pin/{participantId}")]
        [System.Web.Http.HttpGet]
        public IHttpActionResult GetPinDetails([FromUri]int participantId)
        {
            try
            {
                var list = _finderService.GetPinDetailsForPerson(participantId);
                return Ok(list);
            }
            catch (Exception ex)
            {
                var apiError = new ApiErrorDto("Get Pin Details Failed", ex);
                throw new HttpResponseException(apiError.HttpResponseMessage);
            }
        }

        [ResponseType(typeof(PinDto))]
        [VersionedRoute(template: "finder/pinByGroupID/{groupId}", minimumVersion: "1.0.0")]
        [System.Web.Http.Route("finder/pinByGroupID/{groupId}")]
        [System.Web.Http.HttpGet]
        public IHttpActionResult GetPinDetailsByGroup([FromUri]int groupId)
        {
            try
            {
                var list = _finderService.GetPinDetailsForGroup(groupId);
                return Ok(list);
            }
            catch (Exception ex)
            {
                var apiError = new ApiErrorDto("Get Pin Details Failed", ex);
                throw new HttpResponseException(apiError.HttpResponseMessage);
            }
        }

        [ResponseType(typeof(GroupParticipantDTO[]))]
        [VersionedRoute(template: "finder/participants/{groupId}", minimumVersion: "1.0.0")]
        [System.Web.Http.Route("finder/participants/{groupId}")]
        [System.Web.Http.HttpGet]
        public IHttpActionResult GetParticipantsForGroup([FromUri]int groupId)
        {
            try
            {
                var list = _finderService.GetParticipantsForGroup(groupId);
                return Ok(list);
            }
            catch (Exception ex)
            {
                var apiError = new ApiErrorDto("Get Group Participants Failed", ex);
                throw new HttpResponseException(apiError.HttpResponseMessage);
            }
        }

        [ResponseType(typeof(PinDto))]
        [VersionedRoute(template: "finder/pin/contact/{contactId}/{throwOnEmptyCoordinates?}", minimumVersion: "1.0.0")]
        [System.Web.Http.Route("finder/pin/contact/{contactId}/{throwOnEmptyCoordinates?}")]
        [System.Web.Http.HttpGet]
        public IHttpActionResult GetPinDetailsByContact([FromUri]int contactId, [FromUri]bool throwOnEmptyCoordinates = true)
        {
            try
            {
                var participantId = _finderService.GetParticipantIdFromContact(contactId);
                //refactor this to JUST get location;
                var pin = _finderService.GetPinDetailsForPerson(participantId);
                bool pinHasInvalidGeoCoords = ( (pin.Address.Latitude == null || pin.Address.Longitude == null)
                                               || (pin.Address.Latitude == 0 && pin.Address.Longitude == 0));

                if (pinHasInvalidGeoCoords && throwOnEmptyCoordinates)
                {
                   return Content(HttpStatusCode.ExpectationFailed, "Invalid Latitude/Longitude");
                }
                return Ok(pin);
            }
            catch (Exception ex)
            {
                var apiError = new ApiErrorDto("Get Pin Details by Contact Failed", ex);
                throw new HttpResponseException(apiError.HttpResponseMessage);
            }
        }

        [ResponseType(typeof(AddressDTO))]
        [VersionedRoute(template: "finder/pinbyip/{ipAddress}", minimumVersion: "1.0.0")]
        [System.Web.Http.Route("finder/pinbyip/{ipAddress}")]
        [System.Web.Http.HttpGet]
        public IHttpActionResult GetPinByIpAddress([FromUri]string ipAddress)
        {
            try
            {
                var address = _finderService.GetAddressForIp(ipAddress.Replace('$','.'));
                return Ok(address);
            }
            catch (Exception ex)
            {
                var apiError = new ApiErrorDto("Get Pin By Ip Failed", ex);
                throw new HttpResponseException(apiError.HttpResponseMessage);
            }
        }

        /// <summary>
        /// Create Pin with provided address details
        /// </summary>
        [RequiresAuthorization]
        [ResponseType(typeof(PinDto))]
        [VersionedRoute(template: "finder/pin", minimumVersion: "1.0.0")]
        [System.Web.Http.Route("finder/pin")]
        [System.Web.Http.HttpPost]
        public IHttpActionResult PostPin([FromBody] PinDto pin)
        {
            return Authorized(token =>
            {
                try
                {

                    if (pin.Address != null && string.IsNullOrEmpty(pin.Address.AddressLine1) == false)
                    {
                        _finderService.UpdateHouseholdAddress(pin);
                    }

                    if (pin.Participant_ID == 0 || String.IsNullOrEmpty(pin.Participant_ID.ToString()))
                    {
                        pin.Participant_ID =_finderService.GetParticipantIdFromContact((int)pin.Contact_ID);
                    }

                    _finderService.EnablePin((int)pin.Participant_ID);
                    _logger.DebugFormat("Successfully created pin for contact {0} ", pin.Contact_ID);
                    return (Ok(pin));
                }
                catch (Exception e)
                {
                    _logger.Error("Could not create pin", e);
                    var apiError = new ApiErrorDto("Save Pin Failed", e);
                    throw new HttpResponseException(apiError.HttpResponseMessage);
                }
            });
        }

        [ResponseType(typeof(PinSearchResultsDto))]
        [VersionedRoute(template: "finder/findpinsbyaddress/{userSearchAddress}/{lat?}/{lng?}", minimumVersion: "1.0.0")]
        [System.Web.Http.Route("finder/findpinsbyaddress/{userSearchAddress}/{lat?}/{lng?}/{upperleftlat?}/{upperleftlng?}/{bottomrightlat?}/{bottomrightlng?}")]
        [System.Web.Http.HttpGet]
        public IHttpActionResult GetFindPinsByAddress([FromUri]string userSearchAddress, [FromUri]string lat = "0", [FromUri]string lng = "0", [FromUri]string upperleftlat = "0", [FromUri]string upperleftlng = "0", [FromUri]string bottomrightlat = "0", [FromUri]string bottomrightlng = "0")
        {
            try
            {

                AwsBoundingBox boundingBox = null;
                if (upperleftlat != "0" && upperleftlng != "0" && bottomrightlat != "0" && bottomrightlng != "0")
                {
                    boundingBox = _awsCloudsearchService.BuildBoundingBox(upperleftlat, upperleftlng, bottomrightlat, bottomrightlng);
                }
               
                var originCoords = _finderService.GetGeoCoordsFromAddressOrLatLang(userSearchAddress, lat, lng);
                var pinsInRadius = _finderService.GetPinsInBoundingBox(originCoords, userSearchAddress, boundingBox);

                foreach (var pin in pinsInRadius)
                {
                    if (pin.PinType != PinType.SITE)
                    {
                        pin.Address = _finderService.RandomizeLatLong(pin.Address);
                    }
                }

                var result = new PinSearchResultsDto(new GeoCoordinates(originCoords.Latitude, originCoords.Longitude), pinsInRadius);

                return Ok(result);
            }
            catch (Exception ex)
            {
                var apiError = new ApiErrorDto("Get Pin By Address Failed", ex);
                throw new HttpResponseException(apiError.HttpResponseMessage);
            }
        }

        [RequiresAuthorization]
        [ResponseType(typeof(PinSearchResultsDto))]                                   
        [VersionedRoute(template: "finder/findmypinsbycontactid/{contactId}/{lat}/{lng}", minimumVersion: "1.0.0")]
        [System.Web.Http.Route("finder/findmypinsbycontactid/{contactId}/{lat}/{lng}")]
        [System.Web.Http.HttpGet]
        public IHttpActionResult GetMyPinsByContactId([FromUri]int contactId, [FromUri]string lat, [FromUri]string lng)
        {
            return Authorized(token =>
            {
                try
                {
                    var originCoords = _finderService.GetGeoCoordsFromLatLong(lat, lng);

                    var pinsForContact = _finderService.GetMyPins(token, originCoords, contactId);

                    foreach (var pin in pinsForContact)
                    {
                        pin.Address = _finderService.RandomizeLatLong(pin.Address);
                    }

                    var result = new PinSearchResultsDto(new GeoCoordinates(originCoords.Latitude, originCoords.Longitude), pinsForContact);

                    return Ok(result);
                }
                catch (Exception ex)
                {
                    var apiError = new ApiErrorDto("Get Pins for My Stuff Failed", ex);
                    throw new HttpResponseException(apiError.HttpResponseMessage);
                }
            });
        }
        /// <summary>
        /// Logged in user invites a participant to the gathering
        /// </summary>
        [RequiresAuthorization]
        [VersionedRoute(template: "finder/pin/invitetogathering/{gatheringId}", minimumVersion: "1.0.0")]
        [System.Web.Http.Route("finder/pin/invitetogathering/{gatheringId}")]
        [System.Web.Http.HttpPost]
        public IHttpActionResult InviteToGathering([FromUri] int gatheringId, [FromBody] User person)
        {
            if (!ModelState.IsValid)
            {
                var errors = ModelState.Values.SelectMany(val => val.Errors).Aggregate("", (current, err) => current + err.Exception.Message);
                var dataError = new ApiErrorDto("CreateInvitation Data Invalid", new InvalidOperationException("Invalid CreateInvitation Data " + errors));
                throw new HttpResponseException(dataError.HttpResponseMessage);
            }

            return Authorized(token =>
            {
                try
                {
                    _finderService.InviteToGathering(token, gatheringId, person);
                    return (Ok());
                }
                catch (ValidationException e)
                {
                    var error = new ApiErrorDto("Not authorized to send invitations of this type", e, HttpStatusCode.Forbidden);
                    throw new HttpResponseException(error.HttpResponseMessage);
                }
                catch (Exception e)
                {
                    _logger.Error(string.Format("Could not create invitation to recipient {0} ({1}) for group {2}", person.firstName + " " + person.lastName, person.email, 3), e);
                    var apiError = new ApiErrorDto("CreateInvitation Failed", e, HttpStatusCode.InternalServerError);
                    throw new HttpResponseException(apiError.HttpResponseMessage);
                }
            });
        }

        /// <summary>
        /// Logged in user requests to join gathering
        /// </summary>
        [RequiresAuthorization]
        [VersionedRoute(template: "finder/pin/gatheringjoinrequest", minimumVersion: "1.0.0")]
        [System.Web.Http.Route("finder/pin/gatheringjoinrequest")]
        [System.Web.Http.HttpPost]
        public IHttpActionResult GatheringJoinRequest([FromBody]int gatheringId)
        {
            return Authorized(token =>
            {
                try
                {
                    _finderService.GatheringJoinRequest(token, gatheringId);
                    return (Ok());
                }
                catch (Exception e)
                {
                    _logger.Error("Could not generate request", e);
                    if (e.Message == "User is already member or has request")
                    {
                        throw new HttpResponseException(System.Net.HttpStatusCode.Conflict);
                    }
                    else
                    {
                        throw new HttpResponseException(new ApiErrorDto("Gathering request failed", e).HttpResponseMessage);
                    }

                }
            });
        }


        [ResponseType(typeof(PinSearchResultsDto))]
        [VersionedRoute(template: "finder/testawsupload", minimumVersion: "1.0.0")]
        [System.Web.Http.Route("finder/testawsupload")]
        [System.Web.Http.HttpGet]
        public IHttpActionResult TestAwsUpload()
        {
            try
            {
                var response = _awsCloudsearchService.UploadAllConnectRecordsToAwsCloudsearch();

                return Ok(response);
            }
            catch (Exception ex)
            {
                var apiError = new ApiErrorDto("TestAwsUploadFailed", ex);
                throw new HttpResponseException(apiError.HttpResponseMessage);
            }
        }

        [ResponseType(typeof(PinSearchResultsDto))]
        [VersionedRoute(template: "finder/testawssearch", minimumVersion: "1.0.0")]
        [System.Web.Http.Route("finder/testawssearch/{searchstring}")]
        [System.Web.Http.HttpGet]
        public IHttpActionResult TestAwsSearch([FromUri] string searchstring)
        {
            try
            {
                var response = _awsCloudsearchService.SearchConnectAwsCloudsearch(searchstring, "_all_fields");

                return Ok(response);
            }
            catch (Exception ex)
            {
                var apiError = new ApiErrorDto("TestAwsSearch Failed", ex);
                throw new HttpResponseException(apiError.HttpResponseMessage);
            }
        }

        [ResponseType(typeof(PinSearchResultsDto))]
        [VersionedRoute(template: "finder/testawsdelete", minimumVersion: "1.0.0")]
        [System.Web.Http.Route("finder/testawsdelete")]
        [System.Web.Http.HttpGet]
        public IHttpActionResult TestAwsDelete()
        {
            try
            {
                var response = _awsCloudsearchService.DeleteAllConnectRecordsInAwsCloudsearch();

                return Ok(response);
            }
            catch (Exception ex)
            {
                var apiError = new ApiErrorDto("TestAwsDelete Failed", ex);
                throw new HttpResponseException(apiError.HttpResponseMessage);
            }
        }
    }
}

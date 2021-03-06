﻿using crds_angular.Exceptions;
using crds_angular.Exceptions.Models;
using crds_angular.Models.AwsCloudsearch;
using crds_angular.Models.Crossroads;
using crds_angular.Models.Crossroads.Groups;
using crds_angular.Models.Finder;
using crds_angular.Security;
using crds_angular.Services.Analytics;
using crds_angular.Services.Interfaces;
using Crossroads.ApiVersioning;
using Crossroads.Web.Common.Security;
using log4net;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Device.Location;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Reflection;
using System.Web.Http;
using System.Web.Http.Description;
using static NewRelic.Api.Agent.NewRelic;

namespace crds_angular.Models.Finder
{
    public class SayHiDTO
  {
    [JsonProperty("message")]
    public string Message { get; set; }
  }
}

namespace crds_angular.Controllers.API
{
    public class FinderController : ImpersonateAuthBaseController
    {
        private readonly IAwsCloudsearchService _awsCloudsearchService;
        private readonly IFinderService _finderService;
        private readonly IGroupToolService _groupToolService;
        private readonly IAnalyticsService _analyticsService;
        private readonly ILog _logger = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

        public FinderController(IAuthTokenExpiryService authTokenExpiryService,
            IFinderService finderService,
            IGroupToolService groupToolService,
            IUserImpersonationService userImpersonationService,
            IAuthenticationRepository authenticationRepository,
            IAwsCloudsearchService awsCloudsearchService,
            IAnalyticsService analyticsService)
            : base(authTokenExpiryService, userImpersonationService, authenticationRepository)
        {
            _finderService = finderService;
            _groupToolService = groupToolService;
            _awsCloudsearchService = awsCloudsearchService;
            _analyticsService = analyticsService;
        }

        [ResponseType(typeof(PersonDTO))]
        [VersionedRoute(template: "map20/person/{participantId}", minimumVersion: "1.0.0")]
        [Route("map20/person/{participantId}")]
        [HttpGet]
        public IHttpActionResult GetPerson([FromUri]int participantId)
        {
            try
            {
                return Ok(_finderService.GetPerson(participantId));
            }
            catch (Exception ex)
            {
                var apiError = new ApiErrorDto("Get Pin Details Failed", ex);
                throw new HttpResponseException(apiError.HttpResponseMessage);
            }
        }

        [ResponseType(typeof(int))]
        [VersionedRoute(template: "map20/getparticipantid/{contactId}", minimumVersion: "1.0.0")]
        [Route("map20/getparticipantid/{contactId}")]
        [HttpGet]
        public IHttpActionResult GetParticipantId([FromUri]int contactId)
        {
            try
            {
                var participantId = _finderService.GetParticipantIdFromContact(contactId);
                return Ok(participantId);
            }
            catch (Exception ex)
            {
                var apiError = new ApiErrorDto("GetParticipantId Failed", ex);
                throw new HttpResponseException(apiError.HttpResponseMessage);
            }
        }

        [ResponseType(typeof(MyDTO[]))]
        [VersionedRoute(template: "map20/myresultsforpintype/{contactId}/{pintypeId}", minimumVersion: "1.0.0")]
        [Route("map20/myresultsforpintype/{contactId}/{pintypeId}")]
        [HttpGet]
        public IHttpActionResult GetMyResultsForPintype([FromUri]int contactId, [FromUri]int pintypeId)
        {
            try
            {
                List<MyDTO> myList = _finderService.GetMyListForPinType(contactId, pintypeId);
                return Ok(myList.ToArray());
            }
            catch (Exception ex)
            {
                var apiError = new ApiErrorDto("GetMyResultsForPintype Failed", ex);
                throw new HttpResponseException(apiError.HttpResponseMessage);
            }
        }

        [RequiresAuthorization]
        [VersionedRoute(template: "map20/sayhitoparticipant/{toParticipantId}", minimumVersion: "1.0.0")]
        [Route("map20/sayhitoparticipant/{toParticipantId}")]
        [HttpPost]
        public IHttpActionResult SayHiToParticipant( [FromUri]int toParticipantId, [FromBody]SayHiDTO hi)
        {
            return Authorized(token =>
            {
                try
                {
                    _finderService.SayHiToParticipant(token.UserInfo.Mp.ContactId, toParticipantId, hi.Message);
                    return Ok();
                }
                catch (Exception e)
                {
                    var apiError = new ApiErrorDto("Say Hi To Participant Failed", e);
                    throw new HttpResponseException(apiError.HttpResponseMessage);
                }
            });
        }

        [ResponseType(typeof(PinDto))]
        [VersionedRoute(template: "finder/pin/{participantId}", minimumVersion: "1.0.0")]
        [Route("finder/pin/{participantId}")]
        [HttpGet]
        public IHttpActionResult GetPinDetails([FromUri]int participantId)
        {
            try
            {
                var pin = _finderService.GetPinDetailsForPerson(participantId);
                pin.Address = _finderService.RandomizeLatLong(pin.Address);
                return Ok(pin);
            }
            catch (Exception ex)
            {
                var apiError = new ApiErrorDto("Get Pin Details Failed", ex);
                throw new HttpResponseException(apiError.HttpResponseMessage);
            }
        }


        [ResponseType(typeof(PinDto))]
        [VersionedRoute(template: "finder/pinByGroupID/{groupId}/{lat?}/{lng?}", minimumVersion: "1.0.0")]
        [Route("finder/pinByGroupID/{groupId}/{lat?}/{lng?}")]
        [HttpGet]
        public IHttpActionResult GetPinDetailsByGroupId([FromUri]int groupId, [FromUri]string lat = "0", [FromUri]string lng = "0")
        {
            try
            {
                GeoCoordinate centerCoordinate = null;
                if (!lat.Equals("0") && !lat.Equals("0"))
                {
                  centerCoordinate = new GeoCoordinate(double.Parse(lat.Replace('$', '.')), double.Parse(lng.Replace('$', '.')));
                }
                
                var group = _finderService.GetPinDetailsForGroup(groupId, centerCoordinate);
                return Ok(group);
            }
            catch (Exception ex)
            {
                var apiError = new ApiErrorDto("Get Pin Details Failed", ex);
                throw new HttpResponseException(apiError.HttpResponseMessage);
            }
        }

        [ResponseType(typeof(GroupParticipantDTO[]))]
        [VersionedRoute(template: "finder/participants/{groupId}", minimumVersion: "1.0.0")]
        [Route("finder/participants/{groupId}")]
        [HttpGet]
        public IHttpActionResult GetParticipantsForGroup([FromUri]int groupId)
        {
            try
            {
                var groupParticipantList = _finderService.GetParticipantsForGroup(groupId);
                return Ok(groupParticipantList);
            }
            catch (Exception ex)
            {
                var apiError = new ApiErrorDto("Get Group Participants Failed", ex);
                throw new HttpResponseException(apiError.HttpResponseMessage);
            }
        }

        [ResponseType(typeof(bool))]
        [VersionedRoute(template: "finder/getmatch", minimumVersion: "1.0.0")]
        [Route("finder/getmatch")]
        [HttpPost]
        public IHttpActionResult GetPotentialUserMatch([FromBody]User searchUser)
        {
            try
            {
                var rc = _finderService.DoesActiveContactExists(searchUser.email);
                return Ok(rc);
            }
            catch (Exception ex)
            {
                var apiError = new ApiErrorDto("GetPotentialUserMatch Failed", ex);
                throw new HttpResponseException(apiError.HttpResponseMessage);
            }
        }

        [ResponseType(typeof(PinDto))]
        [VersionedRoute(template: "finder/pin/contact/{contactId}/{throwOnEmptyCoordinates?}", minimumVersion: "1.0.0")]
        [Route("finder/pin/contact/{contactId}/{throwOnEmptyCoordinates?}")]
        [HttpGet]
        public IHttpActionResult GetPinDetailsByContact([FromUri]int contactId, [FromUri]bool throwOnEmptyCoordinates = true)
        {
            try
            {
                var participantId = _finderService.GetParticipantIdFromContact(contactId);
                
                //refactor this to JUST get location;
                var pin = _finderService.GetPinDetailsForPerson(participantId);
                var pinHasInvalidGeoCoords = ( (pin.Address == null) || (pin.Address.Latitude == null || pin.Address.Longitude == null)
                                               || (pin.Address.Latitude == 0 && pin.Address.Longitude == 0));

                if (pinHasInvalidGeoCoords && throwOnEmptyCoordinates)
                {
                   return Content(HttpStatusCode.ExpectationFailed, "Invalid Latitude/Longitude");
                }
                pin.Address = _finderService.RandomizeLatLong(pin.Address);
                return Ok(pin);
            }
            catch (Exception ex)
            {
                var apiError = new ApiErrorDto("Get Pin Details by Contact Failed", ex);
                throw new HttpResponseException(apiError.HttpResponseMessage);
            }
        }

        [ResponseType(typeof(bool))]
        [VersionedRoute(template: "finder/doesuserleadsomegroup/{contactid}", minimumVersion: "1.0.0")]
        [Route("finder/doesuserleadsomegroup/{contactid}")]
        [HttpGet]
        public IHttpActionResult GetDoesUserLeadSomeGroup([FromUri]int contactId)
        {
            try
            {
                bool doesUserLeadSomeGroup = _finderService.DoesUserLeadSomeGroup(contactId);
                return Ok(doesUserLeadSomeGroup);
            }
            catch (Exception ex)
            {
                var apiError = new ApiErrorDto("Doesuserleadesomegroup call failed", ex);
                throw new HttpResponseException(apiError.HttpResponseMessage);
            }
        }

        [ResponseType(typeof(bool))]
        [VersionedRoute(template: "finder/isuseronmap/{contactid}", minimumVersion: "1.0.0")]
        [Route("finder/isuseronmap/{contactid}")]
        [HttpGet]
        public IHttpActionResult IsUserOnMap([FromUri]int contactid)
        {
            try
            {
                bool isuseronmap = _finderService.IsUserOnMap(contactid);
                return Ok(isuseronmap);
            }
            catch (Exception ex)
            {
                var apiError = new ApiErrorDto("isuseronmap call failed", ex);
                throw new HttpResponseException(apiError.HttpResponseMessage);
            }
        }

        [RequiresAuthorization]
        [ResponseType(typeof(AddressDTO))]
        [VersionedRoute(template: "finder/group/address/{groupId}", minimumVersion: "1.0.0")]
        [Route("finder/group/address/{groupId}")]
        [HttpGet]
        public IHttpActionResult GetGroupAddress([FromUri] int groupId)
        {
            return Authorized(token =>
            {
                try
                {
                    var address = _finderService.GetGroupAddress(groupId);
                    return (Ok(address));
                }
                catch (Exception e)
                {
                    _logger.Error("Could not get address", e);
                    var apiError = new ApiErrorDto("Get Address Failed", e);
                    throw new HttpResponseException(apiError.HttpResponseMessage);
                }
            });
        }

        /// <summary>
        /// Remove a participant from my group.
        /// </summary>
        /// <param name="groupInformation"></param> Contains Group ID, Participant ID, and message
        /// <returns>An empty response with 200 status code if everything worked, 403 if the caller does not have permission to remove a participant, or another non-success status code on any other failure</returns>
        [RequiresAuthorization]
        [VersionedRoute(template: "finder/group/participant/remove", minimumVersion: "1.0.0")]
        [Route("finder/group/participant/remove")]
        [HttpPost]
        public IHttpActionResult RemoveParticipantFromMyGroup([FromBody] GroupParticipantRemovalDto groupInformation)
        {
            return Authorized(token =>
            {
                try
                {
                    _groupToolService.RemoveParticipantFromMyGroup(token.UserInfo.Mp.ContactId, groupInformation.GroupId, groupInformation.GroupParticipantId, groupInformation.Message);
                    return Ok();
                }
                catch (GroupParticipantRemovalException e)
                {
                    var apiError = new ApiErrorDto(e.Message, null, e.StatusCode);
                    throw new HttpResponseException(apiError.HttpResponseMessage);
                }
                catch (Exception ex)
                {
                    var apiError = new ApiErrorDto($"Error removing group participant {groupInformation.GroupParticipantId} from group {groupInformation.GroupId}", ex);
                    throw new HttpResponseException(apiError.HttpResponseMessage);
                }
            });
        }

        [RequiresAuthorization]
        [ResponseType(typeof(AddressDTO))]
        [VersionedRoute(template: "finder/person/address/{participantId}/{shouldGetFullAddress}", minimumVersion: "1.0.0")]
        [Route("finder/person/address/{participantId}/{shouldGetFullAddress}")]
        [HttpGet]
        public IHttpActionResult GetPersonAddress([FromUri] int participantId, [FromUri] bool shouldGetFullAddress)
        {
            return Authorized(token =>
            {
                try
                {
                    var address = _finderService.GetPersonAddress(token.UserInfo.Mp.ContactId, participantId, shouldGetFullAddress);
                    return (Ok(address));
                }
                catch (Exception e)
                {
                    _logger.Error("Could not get address", e);
                    var apiError = new ApiErrorDto("Get Address Failed", e);
                    throw new HttpResponseException(apiError.HttpResponseMessage);
                }
            });
        }

        [RequiresAuthorization]
        [ResponseType(typeof(MeDTO))]
        [VersionedRoute(template: "finder/person/me", minimumVersion: "1.0.0")]
        [Route("finder/person/me")]
        [HttpGet]
        public IHttpActionResult GetMe()
        {
            return Authorized(token =>
            {
                try
                {
                    return (Ok(_finderService.GetMe(token.UserInfo.Mp.ContactId)));
                }
                catch (Exception e)
                {
                    _logger.Error("Could not get me", e);
                    var apiError = new ApiErrorDto("GetMe Failed", e);
                    throw new HttpResponseException(apiError.HttpResponseMessage);
                }
            });
        }

        [RequiresAuthorization]
        [VersionedRoute(template: "finder/person/me", minimumVersion: "1.0.0")]
        [Route("finder/person/me")]
        [HttpPost]
        public IHttpActionResult PostMe([FromBody] MeDTO medto)
        {
            return Authorized(token =>
            {
                try
                {
                    //only make changes in association with the logged in user.
                    //address changes
                    //congregation
                    //show on map


                    _finderService.SaveMe(token.UserInfo.Mp.ContactId, medto);
                    return(Ok());
                }
                catch (Exception e)
                {
                    _logger.Error("Could not post me", e);
                    var apiError = new ApiErrorDto("PostMe Failed", e);
                    throw new HttpResponseException(apiError.HttpResponseMessage);
                }
            });
        }


        [ResponseType(typeof(PinSearchResultsDto))]
        [VersionedRoute(template: "finder/findpinsbyaddress", minimumVersion: "1.0.0")]
        [Route("finder/findpinsbyaddress/")]
        [HttpPost]
        public  IHttpActionResult GetPinsByAddress(PinSearchQueryParams queryParams)
        {
            try
            {
                // 9/20/2017 Bounding box is NOT being used. This code being left in because there is 
                //           discussion around limiting the number of pins returned.  kdb
                AwsBoundingBox awsBoundingBox = null;
                var areAllBoundingBoxParamsPresent = _finderService.areAllBoundingBoxParamsPresent(queryParams.BoundingBox);
                if (areAllBoundingBoxParamsPresent)
                {
                    awsBoundingBox = _awsCloudsearchService.BuildBoundingBox(queryParams.BoundingBox);
                }

                var stopWatch = Stopwatch.StartNew();
                var originCoords = _finderService.GetMapCenterForResults(queryParams.UserLocationSearchString, queryParams.CenterGeoCoords, queryParams.FinderType);
                stopWatch.Stop();
                RecordMetric("Custom/Time_For_GetMapCenterForResults_Execution", stopWatch.ElapsedMilliseconds);
                RecordCustomNewRelicEvent(" _finderService.GetMapCenterForResults", stopWatch.ElapsedMilliseconds.ToString());
                
                var pinsInRadius = _finderService.GetPinsInBoundingBox(originCoords, queryParams.UserKeywordSearchString, awsBoundingBox, queryParams.FinderType, queryParams.ContactId, queryParams.UserFilterString);
                pinsInRadius = _finderService.RandomizeLatLongForNonSitePins(pinsInRadius);
                var result = new PinSearchResultsDto(new GeoCoordinates(originCoords.Latitude, originCoords.Longitude), pinsInRadius);
           
                return Ok(result);
            }
            catch (InvalidAddressException ex)
            {
                var apiError = new ApiErrorDto("Invalid Address", ex, HttpStatusCode.PreconditionFailed);
                throw new HttpResponseException(apiError.HttpResponseMessage);
            }
            catch (Exception ex)
            {
                var apiError = new ApiErrorDto("Get Pin By Address Failed", ex);
                throw new HttpResponseException(apiError.HttpResponseMessage);
            }
        }

        private static void RecordCustomNewRelicEvent(string methodName,string otherData)
        {
            var eventAttributes = new Dictionary<string, object> { { "MethodName", methodName }, { "Elapsed Time", otherData } };
            RecordCustomEvent("Ape", eventAttributes);
        }

        [RequiresAuthorization]
        [ResponseType(typeof(PinSearchResultsDto))]                                   
        [VersionedRoute(template: "finder/findmypinsbycontactid", minimumVersion: "1.0.0")]
        [Route("finder/findmypinsbycontactid")]
        [HttpPost]
        public IHttpActionResult GetMyPinsByContactId(PinSearchQueryParams queryParams)
        {
            return Authorized(token =>
            {
                try
                {
                    var originCoords = _finderService.GetGeoCoordsFromAddressOrLatLang(queryParams.UserLocationSearchString, queryParams.CenterGeoCoords);
                    var centerLatitude = originCoords.Latitude;
                    var centerLongitude = originCoords.Longitude;

                    var pinsForContact = _finderService.GetMyPins(originCoords, queryParams.ContactId, queryParams.FinderType);

                    if (pinsForContact.Count > 0)
                    {
                        var addressLatitude = pinsForContact[0].Address.Latitude;
                        if (addressLatitude != null) centerLatitude = (double)addressLatitude != 0.0 ? (double)addressLatitude : originCoords.Latitude;

                        var addressLongitude = pinsForContact[0].Address.Longitude;
                        if (addressLongitude != null) centerLongitude = (double)addressLongitude != 0.0 ? (double)addressLongitude : originCoords.Longitude;
                    }

                    var result = new PinSearchResultsDto(new GeoCoordinates(centerLatitude, centerLongitude), pinsForContact);
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
        /// Logged in user invites a participant to the group of types - gathering or small group
        /// </summary>
        [RequiresAuthorization]
        [VersionedRoute(template: "finder/pin/invitetogroup/{groupId}/{finderFlag}", minimumVersion: "1.0.0")]
        [Route("finder/pin/invitetogroup/{groupId}/{finderFlag}")]
        [HttpPost]
        public IHttpActionResult InviteToGroup([FromUri] int groupId, [FromUri]string finderFlag, [FromBody] User person)
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
                    _finderService.InviteToGroup(token.UserInfo.Mp.ContactId, groupId, person, finderFlag);

                    // Call Analytics
                    var props = new EventProperties {{"InvitationToEmail", person.email}};
                    _analyticsService.Track(token.UserInfo.Mp.ContactId.ToString(), "HostInvitationSent", props);

                    return (Ok());
                }
                catch (ValidationException e)
                {
                    var error = new ApiErrorDto("Not authorized to send invitations of this type", e, HttpStatusCode.Forbidden);
                    throw new HttpResponseException(error.HttpResponseMessage);
                }
                catch (Exception e)
                {
                    _logger.Error($"Could not create invitation to recipient {person.firstName + " " + person.lastName} ({person.email}) for group {3}", e);
                    var apiError = new ApiErrorDto("CreateInvitation Failed", e, HttpStatusCode.InternalServerError);
                    throw new HttpResponseException(apiError.HttpResponseMessage);
                }
            });
        }

        /// <summary>
        /// Leader adds a user to their group
        /// </summary>
        [RequiresAuthorization]
        [VersionedRoute(template: "finder/pin/addtogroup/{groupId}/{roleId}", minimumVersion: "1.0.0")]
        [Route("finder/pin/addtogroup/{groupId}")]
        [HttpPost]
        public IHttpActionResult AddToGroup([FromUri] int groupId,  [FromBody] User person, [FromUri] int roleId)
        {
            if (!ModelState.IsValid)
            {
                var errors = ModelState.Values.SelectMany(val => val.Errors).Aggregate("", (current, err) => current + err.Exception.Message);
                var dataError = new ApiErrorDto("AddToGroup Data Invalid", new InvalidOperationException("Invalid AddToGroup Data " + errors));
                throw new HttpResponseException(dataError.HttpResponseMessage);
            }

            return Authorized(authDto =>
            {
                try
                {
                    _finderService.AddUserDirectlyToGroup(person, groupId, roleId, authDto.UserInfo.Mp.ContactId);
                    return Ok();
                }
                catch (DuplicateGroupParticipantException)
                {
                    throw new HttpResponseException(HttpStatusCode.Conflict);
                }
                catch (Exception e)
                {
                    _logger.Error($"Could not add participant {person.firstName + " " + person.lastName} ({person.email}) to group {3}", e);
                    var apiError = new ApiErrorDto("AddToGroup Failed", e, HttpStatusCode.InternalServerError);
                    throw new HttpResponseException(apiError.HttpResponseMessage);
                }
            });
        }

    
        /// <summary>
        /// Logged in user requests to "try a group"
        /// </summary>
        [RequiresAuthorization]
        [VersionedRoute(template: "finder/pin/tryagroup", minimumVersion: "1.0.0")]
        [Route("finder/pin/tryagroup")]
        [HttpPost]
        public IHttpActionResult TryAGroup([FromBody]int groupId)
        {
            return Authorized(token =>
            {
                try
                {
                    _finderService.TryAGroup(token.UserInfo.Mp.ContactId, groupId);
                    return (Ok());
                }
                catch (Exception e)
                {
                    _logger.Error("Could not generate request", e);
                    switch (e.Message)
                    {
                        case "User already has request":
                            throw new HttpResponseException(HttpStatusCode.Conflict);
                        case "User already a member":
                            throw new HttpResponseException(HttpStatusCode.NotAcceptable);
                        default:
                            throw new HttpResponseException(new ApiErrorDto("Try a group request failed", e).HttpResponseMessage);
                    }
                }
            });
        }

        /// <summary>
        /// Leader accepts user requests to "try a group"
        /// </summary>
        [RequiresAuthorization]
        [VersionedRoute(template: "finder/pin/tryagroup/{groupId}/true/{participantId}", minimumVersion: "1.0.0")]
        [Route("finder/pin/tryagroup/{groupId}/true/{participantId}")]
        [HttpPost]
        public IHttpActionResult TryAGroupAccept([FromUri]int groupId, [FromUri]int participantId)
        {
            return Authorized(token =>
            {
                try
                {
                    _finderService.TryAGroupAcceptDeny(groupId, participantId, true);
                    return Ok();
                }
                catch (Exception e)
                {
                    _logger.Error("Could not accept request", e);
                    switch (e.Message)
                    {
                        case "User is already a group member":
                            throw new HttpResponseException(HttpStatusCode.Conflict);
                        default:
                            throw new HttpResponseException(new ApiErrorDto("Try a group accept request failed", e).HttpResponseMessage);
                    }
                }
            });
        }

        /// <summary>
        /// Leader declines user requests to "try a group"
        /// </summary>
        [RequiresAuthorization]
        [VersionedRoute(template: "finder/pin/tryagroup/{groupId}/false/{participantId}", minimumVersion: "1.0.0")]
        [Route("finder/pin/tryagroup/{groupId}/false/{participantId}")]
        [HttpPost]
        public IHttpActionResult TryAGroupDecline([FromUri]int groupId, [FromUri]int participantId)
        {
            return Authorized(token =>
            {
                try
                {
                    _finderService.TryAGroupAcceptDeny(groupId, participantId, false);
                    return Ok();
                }
                catch (Exception e)
                {
                    _logger.Error("Could not deny request", e);
                    switch (e.Message)
                    {
                        case "User is already a group member":
                            throw new HttpResponseException(HttpStatusCode.Conflict);
                        default:
                            throw new HttpResponseException(new ApiErrorDto("Try a group deny request failed", e).HttpResponseMessage);
                    }
                }
            });
        }

        [ResponseType(typeof(PinSearchResultsDto))]
        [VersionedRoute(template: "finder/uploadallcloudsearchrecords", minimumVersion: "1.0.0")]
        [Route("finder/uploadallcloudsearchrecords")]
        [HttpGet]
        public IHttpActionResult UploadAllCloudsearchRecords()
        {
            try
            {
                var response = _awsCloudsearchService.UploadAllConnectRecordsToAwsCloudsearch();
                return Ok(response);
            }
            catch (Exception ex)
            {
                var apiError = new ApiErrorDto("UploadAllCloudsearchRecords", ex);
                throw new HttpResponseException(apiError.HttpResponseMessage);
            }
        }

        [ResponseType(typeof(PinSearchResultsDto))]
        [VersionedRoute(template: "finder/deleteallcloudsearchrecords", minimumVersion: "1.0.0")]
        [Route("finder/deleteallcloudsearchrecords")]
        [HttpGet]
        public IHttpActionResult DeleteAllCloudsearchRecords()
        {
            try
            {
                var response = _awsCloudsearchService.DeleteAllConnectRecordsInAwsCloudsearch();
                return Ok(response);
            }
            catch (Exception ex)
            {
                var apiError = new ApiErrorDto("DeleteAllCloudsearchRecords Failed", ex);
                throw new HttpResponseException(apiError.HttpResponseMessage);
            }
        }



        /// <summary>
        /// Allows an invitee to accept or deny a group invitation.
        /// </summary>
        /// <param name="groupId">An integer identifying the group that the invitation is associated to.</param>
        /// <param name="invitationKey">An string identifying the private invitation.</param>
        /// <param name="accept">A boolean showing if the invitation is being approved or denied.</param>
        [AcceptVerbs("POST")]
        // note - This AcceptVerbs attribute on an entry with the Http* Method attribute causes the
        //        API not to be included in the swagger output. We're doing it because there's a fail
        //        in the swagger code when the body has a boolean in it that breaks in the JS causing
        //        the GroopTool and all subsequent controller APIs not to show on the page. This is a
        //        stupid fix for a defect that is out of our control.
        [RequiresAuthorization]
        [VersionedRoute(template: "finder/group/{groupId}/invitation/{invitationKey}", minimumVersion: "1.0.0")]
        [Route("finder/group/{groupId:int}/invitation/{invitationKey}")]
        [HttpPost]
        public IHttpActionResult ApproveDenyGroupInvitation([FromUri] int groupId, [FromUri] string invitationKey, [FromBody] bool accept)
        {
            return Authorized(token =>
            {
                try
                {
                    _finderService.AcceptDenyGroupInvitation(token.UserInfo.Mp.ContactId, groupId, invitationKey, accept);
                    return Ok();
                }
                catch (GroupParticipantRemovalException)
                {
                    throw new HttpResponseException(HttpStatusCode.NotAcceptable);
                }
                catch (DuplicateGroupParticipantException)
                {
                    throw new HttpResponseException(HttpStatusCode.Conflict);
                }
                catch (Exception ex)
                {
                    var apiError = new ApiErrorDto($"Error when accepting: {accept}, for group {groupId}", ex);
                    throw new HttpResponseException(apiError.HttpResponseMessage);
                }
            });
        }

        /// <summary>
        /// Allows a group leader to accept or deny a group inquirier.
        /// </summary>
        /// <param name="groupTypeId">An integer identifying the type of group.</param>
        /// <param name="groupId">An integer identifying the group that the inquiry is associated to.</param>
        /// <param name="approve">A boolean showing if the inquiry is being approved or denied. It defaults to approved</param>
        /// <param name="inquiry">An Inquiry JSON Object.</param>
        [RequiresAuthorization]
        [VersionedRoute(template: "finder/group-type/{groupTypeId}/group/{groupId}/inquiry/approve/{approve}", minimumVersion: "1.0.0")]
        [Route("finder/grouptype/{groupTypeId:int}/group/{groupId:int}/inquiry/approve/{approve:bool}")]
        [HttpPost]
        public IHttpActionResult ApproveDenyInquiryFromMyGroup([FromUri] int groupTypeId, [FromUri] int groupId, [FromUri] bool approve, [FromBody] Inquiry inquiry)
        {
            return Authorized(token =>
            {
                try
                {
                    _finderService.ApproveDenyGroupInquiry(approve, inquiry);
                    return Ok();
                }
                catch (DuplicateGroupParticipantException)
                {
                    throw new HttpResponseException(HttpStatusCode.Conflict);
                }
                catch (GroupParticipantRemovalException e)
                {
                    var apiError = new ApiErrorDto(e.Message, null, e.StatusCode);
                    throw new HttpResponseException(apiError.HttpResponseMessage);
                }
                catch (Exception ex)
                {
                    var apiError = new ApiErrorDto(
                        $"Error {approve} group inquiry {inquiry.InquiryId} from group {groupId}", ex);
                    throw new HttpResponseException(apiError.HttpResponseMessage);
                }
            });
        }



        [RequiresAuthorization]
        [VersionedRoute(template: "finder/showOnMap/{participantId}/{showOnMap}", minimumVersion: "1.0.0")]
        [Route("finder/showOnMap/{participantId}/{showOnMap}")]
        [HttpPost]
        public IHttpActionResult ShowOnMap([FromUri] int participantId, [FromUri] Boolean showOnMap)
        {
            return Authorized(token =>
            {
                try
                {
                    _finderService.SetShowOnMap(participantId, showOnMap);
                    return Ok();

                }
                catch (Exception e)
                {
                    _logger.Error("Error - ShowOnMap", e);
                    var apiError = new ApiErrorDto("Error - ShowOnMap", e);
                    throw new HttpResponseException(apiError.HttpResponseMessage);
                }
            });
        }
    }
}

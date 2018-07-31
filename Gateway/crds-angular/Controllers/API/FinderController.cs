using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Reflection;
using System.Web.Http;
using System.Web.Http.Description;
using crds_angular.Exceptions.Models;
using crds_angular.Models.Crossroads;
using crds_angular.Models.Finder;
using crds_angular.Security;
using crds_angular.Services.Interfaces;
using Crossroads.ApiVersioning;
using Crossroads.Web.Common.Security;
using System.ComponentModel.DataAnnotations;
using System.Device.Location;
using System.Diagnostics;
using crds_angular.Exceptions;
using crds_angular.Models.AwsCloudsearch;
using crds_angular.Models.Crossroads.Groups;
using crds_angular.Services.Analytics;
using Crossroads.Web.Common.Configuration;
using log4net;
using static NewRelic.Api.Agent.NewRelic;

namespace crds_angular.Controllers.API
{
    public class FinderController : MPAuth
    {
        private readonly IAwsCloudsearchService _awsCloudsearchService;
        private readonly IFinderService _finderService;
        private readonly IGroupToolService _groupToolService;
        private readonly IAuthenticationRepository _authenticationRepo;
        private readonly IAnalyticsService _analyticsService;
        private readonly ILog _logger = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

        public FinderController(IAuthTokenExpiryService authTokenExpiryService, 
                                IFinderService finderService,
                                IGroupToolService groupToolService,
                                IUserImpersonationService userImpersonationService,
                                IAuthenticationRepository authenticationRepository,
                                IAwsCloudsearchService awsCloudsearchService,
                                IAnalyticsService analyticsService,
                                 IConfigurationWrapper configurationWrapper)
            : base(authTokenExpiryService, userImpersonationService, authenticationRepository)
        {
            _finderService = finderService;
            _groupToolService = groupToolService;
            _awsCloudsearchService = awsCloudsearchService;
            _authenticationRepo = authenticationRepository;
            _analyticsService = analyticsService;
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
                var list = _finderService.GetParticipantsForGroup(groupId);
                return Ok(list);
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

        [ResponseType(typeof(AddressDTO))]
        [VersionedRoute(template: "finder/pinbyip/{ipAddress}", minimumVersion: "1.0.0")]
        [Route("finder/pinbyip/{ipAddress}")]
        [HttpGet]
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
                    _groupToolService.RemoveParticipantFromMyGroup(token, groupInformation.GroupId, groupInformation.GroupParticipantId, groupInformation.Message);
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
        [Route("finder/person/address/{participantId}/{getFullAddress}")]
        [HttpGet]
        public IHttpActionResult GetPersonAddress([FromUri] int participantId, [FromUri] bool shouldGetFullAddress)
        {
            return Authorized(token =>
            {
                try
                {
                    var address = _finderService.GetPersonAddress(token, participantId, shouldGetFullAddress);
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
        /// Create Pin with provided address details
        /// </summary>
        [RequiresAuthorization]
        [ResponseType(typeof(PinDto))]
        [VersionedRoute(template: "finder/pin", minimumVersion: "1.0.0")]
        [Route("finder/pin")]
        [HttpPost]
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

                    if (pin.Participant_ID == 0 || string.IsNullOrEmpty(pin.Participant_ID.ToString()))
                    {
                        pin.Participant_ID =_finderService.GetParticipantIdFromContact((int)pin.Contact_ID);
                    }

                    _finderService.EnablePin((int)pin.Participant_ID);
                    _logger.DebugFormat("Successfully created pin for contact {0} ", pin.Contact_ID);

                    //Ensure that address id is available
                    var personPin = _finderService.GetPinDetailsForPerson((int)pin.Participant_ID);

                    //Call  analytics
                    var props = new EventProperties {{"City", pin?.Address?.City}, {"State", pin?.Address?.State}, {"Zip", pin?.Address?.PostalCode}};
                    _analyticsService.Track(pin.Contact_ID.ToString(), "AddedtoMap", props);

                    _awsCloudsearchService.UploadNewPinToAws(personPin); 

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

        /// <summary>
        /// Remove pin from map
        /// </summary>
        [RequiresAuthorization]
        [VersionedRoute(template: "finder/pin/removeFromMap", minimumVersion: "1.0.0")]
        [Route("finder/pin/removeFromMap")]
        [HttpPost]
        public IHttpActionResult RemovePinFromMap([FromBody] int participantId)
        {
            return Authorized(token =>
            {
                try
                {
                    _finderService.DisablePin(participantId);
                    _awsCloudsearchService.DeleteSingleConnectRecordInAwsCloudsearch(participantId, 1);

                    // Call  analytics
                    _analyticsService.Track(AuthenticationRepository.GetContactId(token).ToString(), "RemovedFromMap");

                    return Ok();

                }
                catch (Exception e)
                {
                    _logger.Error("Could not create pin", e);
                    var apiError = new ApiErrorDto("Remove pin from map failed", e);
                    throw new HttpResponseException(apiError.HttpResponseMessage);
                }
            });
        }

        /// <summary>
        /// Create Pin with provided address details
        /// </summary>
        [RequiresAuthorization]
        [ResponseType(typeof(PinDto))]
        [VersionedRoute(template: "finder/gathering/edit", minimumVersion: "1.0.0")]
        [Route("finder/gathering/edit")]
        [HttpPut]
        public IHttpActionResult EditGatheringPin([FromBody] PinDto pin)
        {
            return Authorized(token =>
            {
                try
                {
                    if (pin.Contact_ID != _authenticationRepo.GetContactId(token))
                    {
                        throw new HttpResponseException(HttpStatusCode.Unauthorized);
                    }

                    pin = _finderService.UpdateGathering(pin);
                    _awsCloudsearchService.UploadNewPinToAws(pin);

                    return (Ok(pin));
                }
                catch (Exception e)
                {
                    _logger.Error("Could not update pin", e);
                    var apiError = new ApiErrorDto("Save Pin Failed", e);
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

                    var pinsForContact = _finderService.GetMyPins(token, originCoords, queryParams.ContactId, queryParams.FinderType);

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
                    _finderService.InviteToGroup(token, groupId, person, finderFlag);

                    // Call Analytics
                    var props = new EventProperties {{"InvitationToEmail", person.email}};
                    _analyticsService.Track(AuthenticationRepository.GetContactId(token).ToString(), "HostInvitationSent", props);

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

            return Authorized(token =>
            {
                try
                {
                    _finderService.AddUserDirectlyToGroup(token, person, groupId, roleId);
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
        /// Logged in user requests to join gathering
        /// </summary>
        [RequiresAuthorization]
        [VersionedRoute(template: "finder/pin/gatheringjoinrequest", minimumVersion: "1.0.0")]
        [Route("finder/pin/gatheringjoinrequest")]
        [HttpPost]
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
                    if (e.Message == "User already has request")
                    {
                        throw new HttpResponseException(HttpStatusCode.Conflict);
                    }
                    else if (e.Message == "User already a member")
                    {
                        throw new HttpResponseException(HttpStatusCode.NotAcceptable);
                    }
                    else
                    {
                        throw new HttpResponseException(new ApiErrorDto("Gathering request failed", e).HttpResponseMessage);
                    }

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
                    _finderService.TryAGroup(token, groupId);
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
                    _finderService.TryAGroupAcceptDeny(token, groupId, participantId, true);
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
                    _finderService.TryAGroupAcceptDeny(token, groupId, participantId, false);
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

        /// <summary>
        /// Logged in user says hi
        /// </summary>
        [RequiresAuthorization]
        [VersionedRoute(template: "finder/sayhi/{fromId}/{toId}", minimumVersion: "1.0.0")]
        [Route("finder/sayhi/{fromId}/{toId}")]
        [HttpPost]
        public IHttpActionResult SayHi([FromUri]int fromId, [FromUri]int toId, [FromBody]string message)
        {
            return Authorized(token =>
            {
                try
                {
                    _finderService.SayHi(fromId, toId, message);
                    return Ok();
                }
                catch (Exception e)
                {
                    var apiError = new ApiErrorDto("Say Hi Failed", e);
                    throw new HttpResponseException(apiError.HttpResponseMessage);
                }
            });
        }

        /// <summary>
        /// Logged in user requests to be a host
        /// </summary>
        [RequiresAuthorization]
        [VersionedRoute(template: "finder/pin/requesttobehost", minimumVersion: "1.0.0")]
        [Route("finder/pin/requesttobehost")]
        [HttpPost]
        public IHttpActionResult RequestToBeHost([FromBody]HostRequestDto hostRequest)
        {
            return Authorized(token =>
            {
                try
                {
                    _finderService.RequestToBeHost(token, hostRequest);

                    // Call Analytics
                    var props = new EventProperties {{"City", hostRequest.Address.City}, {"State", hostRequest.Address.State}, {"Zip", hostRequest.Address.PostalCode}};
                    _analyticsService.Track(hostRequest.ContactId.ToString(), "RegisteredAsHost", props);

                    return Ok();
                }
                catch (GatheringException e)
                {
                    _logger.Error("Host already has a gathering at this location.", e);
                    throw new HttpResponseException(HttpStatusCode.NotAcceptable);
                }
                catch (Exception e)
                {
                    _logger.Error("Could not generate request", e);
                    throw new HttpResponseException(new ApiErrorDto("Gathering request failed", e).HttpResponseMessage);
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
                    _finderService.AcceptDenyGroupInvitation(token, groupId, invitationKey, accept);
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
                    _finderService.ApproveDenyGroupInquiry(token, approve, inquiry);
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
    }
}

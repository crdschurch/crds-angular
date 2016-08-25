﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Web.Http;
using System.Web.Http.Description;
using System.Web.Http.Results;
using crds_angular.Exceptions.Models;
using crds_angular.Models.Crossroads.Trip;
using crds_angular.Security;
using crds_angular.Services.Interfaces;
using IPersonService = crds_angular.Services.Interfaces.IPersonService;

namespace crds_angular.Controllers.API
{
    public class TripController : MPAuth
    {
        private readonly ITripService _tripService;
        private readonly IPersonService _personService;

        public TripController(ITripService tripService, IPersonService personService)
        {
            _tripService = tripService;
            _personService = personService;
        }

        [System.Web.Http.AcceptVerbs("GET")]
        [ResponseType(typeof (List<FamilyMemberTripDto>))]
        [System.Web.Http.Route("api/trip/{pledgeCampaignId}/family-members")]
        public IHttpActionResult GetFamilyWithTripInfo(int pledgeCampaignId)
        {
            return Authorized(token =>
            {
                try
                {
                    var familyMembers = _tripService.GetFamilyMembers(pledgeCampaignId, token);
                    return Ok(familyMembers);
                }
                catch (Exception ex)
                {
                    var apiError = new ApiErrorDto("Get Family With Trip Info", ex);
                    throw new HttpResponseException(apiError.HttpResponseMessage);
                }
                
            });         
        }

        [System.Web.Http.AcceptVerbs("GET")]
        [System.Web.Http.Route("api/trip/scholarship/{campaignId}/{contactId}")]
        public IHttpActionResult ContactHasScholarship(int contactId, int campaignId)
        {
            return Authorized(token =>
            {
                try
                {
                    var scholarshipped = _tripService.HasScholarship(contactId, campaignId);
                    if (scholarshipped)
                    {
                        return Ok();
                    }
                    return new StatusCodeResult(HttpStatusCode.ExpectationFailed, this);
                }
                catch (Exception ex)
                {
                    var apiError = new ApiErrorDto("Check for scholarship", ex);
                    throw new HttpResponseException(apiError.HttpResponseMessage);
                }
            });
        }

        [System.Web.Http.AcceptVerbs("GET")]
        [ResponseType(typeof (TripFormResponseDto))]
        [System.Web.Http.Route("api/trip/form-responses/{selectionId}/{selectionCount}/{recordId}")]
        public IHttpActionResult TripFormResponses(int selectionId, int selectionCount, int recordId)
        {
            return Authorized(token =>
            {
                try
                {
                    var groups = _tripService.GetFormResponses(selectionId, selectionCount, recordId);
                    return Ok(groups);
                }
                catch (Exception ex)
                {
                    var apiError = new ApiErrorDto("GetGroupsForEvent Failed", ex);
                    throw new HttpResponseException(apiError.HttpResponseMessage);
                }
            });
        }

        [System.Web.Http.AcceptVerbs("GET")]
        [ResponseType(typeof (TripCampaignDto))]
        [System.Web.Http.Route("api/trip/campaign/{campaignId}")]
        public IHttpActionResult GetCampaigns(int campaignId)
        {
            return Authorized(token =>
            {
                try
                {
                    var campaign = _tripService.GetTripCampaign(campaignId);
                    return Ok(campaign);
                }
                catch (Exception ex)
                {
                    var apiError = new ApiErrorDto("Get Campaign Failed", ex);
                    throw new HttpResponseException(apiError.HttpResponseMessage);
                }
            });
        }

        [System.Web.Http.AcceptVerbs("POST")]
        [System.Web.Http.Route("api/trip/generate-private-invite")]
        public IHttpActionResult GeneratePrivateInvite([FromBody] PrivateInviteDto dto)
        {
            if (!ModelState.IsValid)
            {
                var errors = ModelState.Values.SelectMany(val => val.Errors).Aggregate("", (current, err) => current + err.Exception.Message);
                var dataError = new ApiErrorDto("GeneratePrivateInvite Data Invalid", new InvalidOperationException("Invalid GeneratePrivateInvite Data" + errors));
                throw new HttpResponseException(dataError.HttpResponseMessage);
            }

            return Authorized(token =>
            {
                try
                {
                    _tripService.GeneratePrivateInvite(dto, token);
                    return Ok();
                }
                catch (Exception exception)
                {
                    var apiError = new ApiErrorDto("GeneratePrivateInvite Failed", exception);
                    throw new HttpResponseException(apiError.HttpResponseMessage);
                }
            });
        }

        [System.Web.Http.AcceptVerbs("GET")]
        [ResponseType(typeof (TripParticipantDto))]
        [System.Web.Http.Route("api/trip/search/{query?}")]
        public IHttpActionResult Search(string query)
        {
            try
            {
                var list = _tripService.Search(query);
                return Ok(list);
            }
            catch (Exception ex)
            {
                var apiError = new ApiErrorDto("Trip Search Failed", ex);
                throw new HttpResponseException(apiError.HttpResponseMessage);
            }
        }

        [System.Web.Http.AcceptVerbs("GET")]
        [ResponseType(typeof (TripParticipantDto))]
        [System.Web.Http.Route("api/trip/participant/{tripParticipantId}")]
        public IHttpActionResult TripParticipant(string tripParticipantId)
        {
            try
            {
                // Get Participant
                var searchString = tripParticipantId + ',';
                var participant = _tripService.Search(searchString).FirstOrDefault();
                return Ok(participant);
            }
            catch (Exception ex)
            {
                var apiError = new ApiErrorDto("Trip Search Failed", ex);
                throw new HttpResponseException(apiError.HttpResponseMessage);
            }
        }

        [System.Web.Http.AcceptVerbs("GET")]
        [ResponseType(typeof (MyTripsDto))]
        [System.Web.Http.Route("api/trip/mytrips")]
        public IHttpActionResult MyTrips()
        {
            return Authorized(token =>
            {
                try
                {
                    var trips = _tripService.GetMyTrips(token);
                    return Ok(trips);
                }
                catch (Exception ex)
                {
                    var apiError = new ApiErrorDto("Failed to retrieve My Trips info", ex);
                    throw new HttpResponseException(apiError.HttpResponseMessage);
                }
            });
        }

        [System.Web.Http.AcceptVerbs("GET")]
        [ResponseType(typeof (ValidatePrivateInviteDto))]
        [System.Web.Http.Route("api/trip/validate-private-invite/{pledgeCampaignId}/{guid}")]
        public IHttpActionResult ValidatePrivateInvite(int pledgeCampaignId, string guid)
        {
            return Authorized(token =>
            {
                try
                {
                    var retVal = new ValidatePrivateInviteDto {Valid = _tripService.ValidatePrivateInvite(pledgeCampaignId, guid, token)};
                    return Ok(retVal);
                }
                catch (Exception exception)
                {
                    var apiError = new ApiErrorDto("ValidatePrivateInvite Failed", exception);
                    throw new HttpResponseException(apiError.HttpResponseMessage);
                }
            });
        }
    }
}
using System;
using System.Collections.Generic;
using System.Linq;
using System.Messaging;
using System.Web;
using System.Web.Helpers;
using System.Web.Http;
using System.Web.Http.Description;
using crds_angular.Exceptions.Models;
using crds_angular.Models.Crossroads.Groups;
using crds_angular.Models.Crossroads.Opportunity;
using crds_angular.Models.Crossroads.Serve;
using crds_angular.Security;
using crds_angular.Services.Interfaces;
using Crossroads.Utilities.Interfaces;
using Crossroads.Utilities.Messaging.Interfaces;
using Crossroads.ApiVersioning;
using Crossroads.Web.Common;
using Crossroads.Web.Common.Configuration;
using Crossroads.Web.Common.Security;

namespace crds_angular.Controllers.API
{
    public class ServeController : MPAuth
    {
        private readonly IServeService _serveService;
        private readonly IMessageFactory _messageFactory;
        private readonly MessageQueue _eventQueue;

        public ServeController(IAuthTokenExpiryService authTokenExpiryService, 
                               IServeService serveService, 
                               IConfigurationWrapper configuration, 
                               IMessageFactory messageFactory, 
                               IMessageQueueFactory messageQueueFactory, 
                               IUserImpersonationService userImpersonationService, 
                               IAuthenticationRepository authenticationRepository) 
            : base(authTokenExpiryService, userImpersonationService, authenticationRepository)
        {
            _serveService = serveService;
            _messageFactory = messageFactory;

            var eventQueueName = configuration.GetConfigValue("SignupToServeEventQueue");
            _eventQueue = messageQueueFactory.CreateQueue(eventQueueName, QueueAccessMode.Send);
            _messageFactory = messageFactory;
        }

        /// <summary>
        /// Gets the opportunities for a volunteer and his/her family
        /// Accepts optional parameters from and to that specify the date range to fetch
        /// </summary>
        /// <param name="contactId">The volunteers contactId</param>
        /// <param name="from">Optional- The starting date</param>
        /// <param name="to">Optional- The end date</param>
        /// <returns></returns>
        [ResponseType(typeof(List<ServingDay>))]
        [VersionedRoute(template: "serve/family-serve-days/{contactId}", minimumVersion: "1.0.0")]
        [Route("serve/family-serve-days/{contactId}")]
        public IHttpActionResult GetFamilyServeDays(int contactId, long from = 0, long to = 0)
        {
            return Authorized(token =>
                              {
                                  try
                                  {
                                      var servingDays = _serveService.GetServingDays(token, contactId, from, to);
                                      return Ok(servingDays);
                                  }
                                  catch (Exception exception)
                                  {
                                      var apiError = new ApiErrorDto("Get Family Serve Days Failed", exception);
                                      throw new HttpResponseException(apiError.HttpResponseMessage);
                                  }
                              });
        }

        [ResponseType(typeof(ServingTeam))]
        [VersionedRoute(template: "serve/get-team-rsvps", minimumVersion: "1.0.0")]
        [Route("serve/getTeamRsvps")]
        [HttpPost]
        public IHttpActionResult GetServingTeamRsvps([FromBody] ServingTeam team)
        {
            return Authorized(token =>
                              {
                                  try
                                  {
                                      var rsvpTeam = _serveService.GetServingTeamRsvps(team);
                                      return Ok(rsvpTeam);
                                  }
                                  catch (Exception exception)
                                  {
                                      var apiError = new ApiErrorDto($"Get RSVP for {team.GroupId} group failed", exception);
                                      throw new HttpResponseException(apiError.HttpResponseMessage);
                                  }
                              });
        }


        [ResponseType(typeof (List<FamilyMember>))]
        [VersionedRoute(template: "serve/family/{contactId?}", minimumVersion: "1.0.0")]
        [Route("serve/family/{contactId?}")]
        public IHttpActionResult GetFamily(int contactId = 0)
        {
            //TODO: I don't think you need to pass in contactId here, use the token
            return Authorized(token =>
            {
                try
                {
                    var list = _serveService.GetImmediateFamilyParticipants(token);
                    return Ok(list);
                }
                catch (Exception ex)
                {
                    var apiError = new ApiErrorDto("GetFamily Failed", ex);
                    throw new HttpResponseException(apiError.HttpResponseMessage);
                }
            });
        }

        [ResponseType(typeof (List<QualifiedServerDto>))]
        [VersionedRoute(template: "serve/qualified-servers/{groupId}/{opportunityId}", minimumVersion: "1.0.0")]
        [Route("serve/qualifiedservers/{groupId}/{opportunityId}")]
        public IHttpActionResult GetQualifiedServers(int groupId, int opportunityId)
        {
            return Authorized(token =>
            {
                try
                {
                    var list = _serveService.GetQualifiedServers(groupId, opportunityId, token);
                    return Ok(list);
                }
                catch (Exception ex)
                {
                    var apiError = new ApiErrorDto("GetQualifiedServers Failed", ex);
                    throw new HttpResponseException(apiError.HttpResponseMessage);
                }
            });
        }

        [RequiresAuthorization]
        [ResponseType(typeof(List<GroupDTO>))]
        [VersionedRoute(template: "serve/get-logged-in-leaders-groups", minimumVersion: "1.0.0")]
        [Route("serve/GetLoggedInLeadersGroups")]
        public IHttpActionResult GetLoggedInLeadersGroups()
        {
            return Authorized(token =>
            {
                try
                {
                    var list = _serveService.GetLeaderGroups(token);
                    return Ok(list);
                }
                catch (Exception ex)
                {
                    var apiError = new ApiErrorDto("Get Leaders Groups Failed", ex);
                    throw new HttpResponseException(apiError.HttpResponseMessage);
                }
            });
        }

        [RequiresAuthorization]
        [ResponseType(typeof(List<GroupParticipantDTO>))]
        [VersionedRoute(template: "serve/get-logged-in-leaders-groups-participants", minimumVersion: "1.0.0")]
        [Route("serve/GetLoggedInLeadersGroupsParticipants")]
        public IHttpActionResult GetLoggedInLeadersGroupsParticipants(int? groupId = null)
        {
            return Authorized(token =>
            {
                try
                {
                    var list = _serveService.GetLeaderGroupsParticipants(token, groupId);
                    return Ok(list);
                }
                catch (Exception ex)
                {
                    var apiError = new ApiErrorDto("Get Leaders Groups Participants Failed", ex);
                    throw new HttpResponseException(apiError.HttpResponseMessage);
                }
            });
        }

        [RequiresAuthorization]
        [ResponseType(typeof(object))]
        [VersionedRoute(template: "serve/get-is-leader", minimumVersion: "1.0.0")]
        [Route("serve/GetIsLeader")]
        public IHttpActionResult GetIsLeader(int? groupId = null)
        {
            return Authorized(token =>
            {
                try
                {
                    bool isLeader = _serveService.GetIsLeader(token, groupId);
                    return Ok(new {isLeader = isLeader});
                }
                catch (Exception ex)
                {
                    var apiError = new ApiErrorDto("Get Is Leader failed", ex);
                    throw new HttpResponseException(apiError.HttpResponseMessage);
                }
            });
        }

        [VersionedRoute(template: "serve/save-rsvp", minimumVersion: "1.0.0")]
        [Route("serve/save-rsvp")]
        public IHttpActionResult SaveRsvp([FromBody] SaveRsvpDto saveRsvp)
        {
            if (!ModelState.IsValid)
            {
                var errors = ModelState.Values.SelectMany(val => val.Errors).Aggregate("", (current, err) => current + err.Exception.Message);
                var dataError = new ApiErrorDto("RSVP Data Invalid", new InvalidOperationException("Invalid RSVP Data" + errors));
                throw new HttpResponseException(dataError.HttpResponseMessage);
            }
            //validate request
            if (saveRsvp.StartDateUnix <= 0)
            {
                var dateError = new ApiErrorDto("StartDate Invalid", new InvalidOperationException("Invalid Date"));
                throw new HttpResponseException(dateError.HttpResponseMessage);
            }

            return Authorized(token =>
            {
                var message = _messageFactory.CreateMessage(saveRsvp);
                _eventQueue.Send(message, MessageQueueTransactionType.None);

                // get updated events and return them
                var updatedEvents = new UpdatedEvents();
                try
                {
                    updatedEvents.EventIds.AddRange(_serveService.GetUpdatedOpportunities(token, saveRsvp));
                }
                catch (Exception exception)
                {
                    var apiError = new ApiErrorDto("Save RSVP Failed", exception);
                    throw new HttpResponseException(apiError.HttpResponseMessage);
                }
                return Ok(updatedEvents);
            });
        }

        [ResponseType(typeof (Capacity))]
        [VersionedRoute(template: "serve/opportunity-capacity", minimumVersion: "1.0.0")]
        [Route("serve/opp-capacity")]
        public IHttpActionResult GetOpportunityCapacity([FromUri] OpportunityCapacityDto oppCap)
        {
            return Authorized(token =>
            {
                try
                {
                    var oppCapacity = _serveService.OpportunityCapacity(oppCap.Id, oppCap.EventId, oppCap.Min, oppCap.Max);
                    if (oppCapacity == null)
                    {
                        return Unauthorized();
                    }
                    return Ok(oppCapacity);
                }
                catch (Exception exception)
                {
                    var apiError = new ApiErrorDto("Get Opportunity Capacity Failed", exception);
                    throw new HttpResponseException(apiError.HttpResponseMessage);
                }
            });
        }
    }
}

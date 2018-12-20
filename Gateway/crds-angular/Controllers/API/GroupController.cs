using System;
using System.Collections.Generic;
using System.Net;
using System.Reflection;
using System.Web.Http;
using System.Web.Http.Description;
using crds_angular.Exceptions.Models;
using crds_angular.Models.Crossroads;
using crds_angular.Models.Crossroads.Groups;
using crds_angular.Security;
using log4net;
using MinistryPlatform.Translation.Exceptions;
using MinistryPlatform.Translation.Repositories.Interfaces;
using crds_angular.Services.Interfaces;
using Event = crds_angular.Models.Crossroads.Events.Event;
using Crossroads.ApiVersioning;
using Crossroads.Web.Common;
using Crossroads.Web.Common.Security;

namespace crds_angular.Controllers.API
{
    public class GroupController : MPAuth
    {
        private readonly ILog _logger = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);
        private readonly IGroupService _groupService;
        private readonly IContactRepository _contactRepository;
        private readonly IParticipantRepository _participantService;
        private readonly IAddressService _addressService;
        private readonly IGroupSearchService _groupSearchService;
        private readonly IGroupToolService _groupToolService;

        public GroupController(IAuthTokenExpiryService authTokenExpiryService, 
                               IGroupService groupService,
                               IAuthenticationRepository authenticationService,
                               IContactRepository contactRepository,
                               IParticipantRepository participantService,
                               IAddressService addressService,
                               IGroupSearchService groupSearchService,
                               IGroupToolService groupToolService, 
                               IUserImpersonationService userImpersonationService) 
            : base(authTokenExpiryService, userImpersonationService, authenticationService)
        {
            _groupService = groupService;
            _contactRepository = contactRepository;
            _participantService = participantService;
            _addressService = addressService;
            _groupSearchService = groupSearchService;
            _groupToolService = groupToolService;
        }

        /// <summary>
        /// Create Group with provided details, returns created group with ID
        /// </summary>
        [RequiresAuthorization]
        [ResponseType(typeof (GroupDTO))]
        [VersionedRoute(template: "group", minimumVersion: "1.0.0")]
        [Route("group")]
        [HttpPost]
        public IHttpActionResult PostGroup([FromBody] GroupDTO group)
        {
            return Authorized(token =>
            {
                try
                {
                    if (group.Address != null && string.IsNullOrEmpty(group.Address.AddressLine1) == false)
                    {
                        _addressService.FindOrCreateAddress(group.Address, true);
                    }

                    group = _groupService.CreateGroup(group);
                    _logger.DebugFormat("Successfully created group {0} ", group.GroupId);
                    return (Created(string.Format("api/group/{0}", group.GroupId), group));
                }
                catch (Exception e)
                {
                    _logger.Error("Could not create group", e);
                    return BadRequest();
                }
            });
        }

        /// <summary>
        /// Edit a group for the authenticated user.
        /// </summary>
        /// <param name="group">The data required to edit the group, GroupDTO</param>
        /// <returns>The input GroupDTO</returns>
        [RequiresAuthorization]
        [ResponseType(typeof(GroupDTO))]
        [VersionedRoute(template: "group/edit", minimumVersion: "1.0.0")]
        [Route("group/edit")]
        [HttpPost]
        public IHttpActionResult EditGroup([FromBody] GroupDTO group)
        {
            return Authorized(token =>
            {
                try
                {
                    _groupToolService.VerifyCurrentUserIsGroupLeader(token, @group.GroupId);
                    if (group.Address != null && string.IsNullOrEmpty(group.Address.AddressLine1) == false)
                    {
                        _addressService.FindOrCreateAddress(group.Address, true);
                    }

                    group = _groupService.UpdateGroup(group);
                    _logger.DebugFormat("Successfully updated group {0} ", group.GroupId);
                    return (Created(string.Format("api/group/{0}", group.GroupId), group));

                }
                catch (Exception e)
                {
                    _logger.Error("Could not update group", e);
                    return BadRequest();
                }
            });
        }

        [RequiresAuthorization]
        [ResponseType(typeof (GroupDTO))]
        [VersionedRoute(template: "group/{groupId}", minimumVersion: "1.0.0")]
        [Route("group/{groupId}")]
        public IHttpActionResult Get(int groupId)
        {
            return Authorized(token =>
            {
                try
                {
                    var participant = _participantService.GetParticipantRecord(token);
                    var contactId = _contactRepository.GetContactId(token);

                    var detail = _groupService.getGroupDetails(groupId, contactId, participant, token);

                    return Ok(detail);
                }
                catch (Exception e)
                {
                    var apiError = new ApiErrorDto("Get Group", e);
                    throw new HttpResponseException(apiError.HttpResponseMessage);
                }

            });
        }

        [ResponseType(typeof(object))]
        [VersionedRoute(template: "group/{groupId}/groupType", minimumVersion: "1.0.0")]
        [Route("group/{groupId}/groupType")]
        public IHttpActionResult GetGroupTypeId(int groupId)
        {
            var group = _groupService.GetGroupDetails(groupId);
            return Ok(new { groupTypeId = group.GroupTypeId });
        }


        /// <summary>
        /// Return the group dto for the invitation guid (Private Invitation)
        /// </summary>
        /// <param name="invitationKey">An string representing the unique private invite key</param>
        /// <returns>A list of Group DTO</returns>
        [RequiresAuthorization]
        [ResponseType(typeof(GroupDTO))]
        [VersionedRoute(template: "group/invitation/{invitationKey}", minimumVersion: "1.0.0")]
        [Route("group/invitation/{invitationKey}")]
        [HttpGet]
        public IHttpActionResult GetGroupByInvitationGuid(string invitationKey)
        {
            return Authorized(token =>
            {
                try
                {
                    var group = _groupService.GetGroupDetailsByInvitationGuid(token, invitationKey);

                    return Ok(group);
                }
                catch (Exception e)
                {
                    var apiError = new ApiErrorDto("Get Group", e);
                    throw new HttpResponseException(apiError.HttpResponseMessage);
                }

            });
        }

        [RequiresAuthorization]
        [ResponseType(typeof (List<Event>))]
        [VersionedRoute(template: "group/{groupId}/events", minimumVersion: "1.0.0")]
        [Route("group/{groupId}/events")]
        public IHttpActionResult GetEvents(int groupId)
        {
            return Authorized(token =>
            {
                try
                {
                    var eventList = _groupService.GetGroupEvents(groupId, token);
                    return Ok(eventList);
                }
                catch (Exception e)
                {
                    var apiError = new ApiErrorDto("Error getting events ", e);
                    throw new HttpResponseException(apiError.HttpResponseMessage);
                }
            }
                );
        }

        [RequiresAuthorization]
        [ResponseType(typeof (List<GroupContactDTO>))]
        [VersionedRoute(template: "group/{groupId}/event/{eventId}", minimumVersion: "1.0.0")]
        [Route("group/{groupId}/event/{eventId}")]
        public IHttpActionResult GetParticipants(int groupId, int eventId, string recipients)
        {
            return Authorized(token =>
            {
                try
                {
                    if (recipients != "current" && recipients != "potential")
                    {
                        throw new ApplicationException("Recipients should be 'current' or 'potential'");
                    }
                    var memberList = _groupService.GetGroupMembersByEvent(groupId, eventId, recipients);
                    return Ok(memberList);
                }
                catch (Exception e)
                {
                    var apiError = new ApiErrorDto("Error getting participating group members ", e);
                    throw new HttpResponseException(apiError.HttpResponseMessage);
                }
            }
                );
        }

        /// <summary>
        /// This takes in a Group Type ID and retrieves all groups of that type for the current user.
        /// If one or more groups are found, then the group detail data is returned.
        /// If no groups are found, then an empty list will be returned.
        /// </summary>
        /// <param name="groupTypeId">This is the Ministry Platform Group Type ID for the specific group being requested..</param>
        /// <returns>A list of all groups for the given user based on the Group Type ID passed in.</returns>
        [RequiresAuthorization]
        [ResponseType(typeof (List<GroupContactDTO>))]
        [VersionedRoute(template: "group/group-type/{groupTypeId}", minimumVersion: "1.0.0")]
        [Route("group/groupType/{groupTypeId}")]
        public IHttpActionResult GetGroups(int groupTypeId)
        {
            return Authorized(token =>
            {
                try
                {
                    var participant = _groupService.GetParticipantRecord(token);
                    var groups = _groupService.GetGroupsByTypeForParticipant(token, participant.ParticipantId, groupTypeId);
                    return Ok(groups);
                }
                catch (Exception ex)
                {
                    var apiError = new ApiErrorDto("Error getting groups for group type ID " + groupTypeId, ex);
                    throw new HttpResponseException(apiError.HttpResponseMessage);
                }

            });
        }

        /// <summary>
        /// This takes in a Group Type ID and retrieves all groups of that type for the current user.
        /// If one or more groups are found, then the group detail data is returned.
        /// If no groups are found, then an empty list will be returned.
        /// </summary>
        /// <returns>A list of all small groups for the given user (group type of 1)</returns>
        [RequiresAuthorization]
        [ResponseType(typeof(List<GroupDTO>))]
        [VersionedRoute(template: "group/mine", minimumVersion: "1.0.0")]
        [Route("group/mine")]
        public IHttpActionResult GetMyGroups()
        {
            return Authorized(token =>
            {
                try
                {
                    var groups = _groupToolService.GetGroupToolGroups(token);
                    return Ok(groups);
                }
                catch (Exception ex)
                {
                    var apiError = new ApiErrorDto("Error getting Groups for logged in user.", ex);
                    throw new HttpResponseException(apiError.HttpResponseMessage);
                }

            });
        }

        /// <summary>
        /// This finds groups that match a participants answers for a specific group type.
        /// If one or more groups are found, then a list of the group are returned.
        /// If no groups are found, then an empty list will be returned.
        /// </summary>
        /// <param name="groupTypeId">Group Type ID of the groups to search</param>
        /// <param name="participant">Participants answers to find matching groups</param>
        /// <returns></returns>
        [RequiresAuthorization]
        [ResponseType(typeof(List<GroupDTO>))]
        [VersionedRoute(template: "group/group-type/{groupTypeId}/search", minimumVersion: "1.0.0")]
        [Route("group/groupType/{groupTypeId}/search")]
        [HttpPost]
        public IHttpActionResult GetSearchMatches(int groupTypeId, [FromBody] GroupParticipantDTO participant)
        {
            return Authorized(token =>
            {
                try
                {
                    var matches = _groupSearchService.FindMatches(groupTypeId, participant);
                    return Ok(matches);
                }
                catch (Exception ex)
                {
                    var apiError = new ApiErrorDto("Error searching for matching groups for group type ID " + groupTypeId, ex);
                    throw new HttpResponseException(apiError.HttpResponseMessage);
                }
            });
        }

        /// <summary>
        /// Takes in a Group ID and retrieves all active participants for the group id.
        /// </summary>
        /// <param name="groupId">GroupId of the group.</param>
        /// <returns>A list of active participants for the group id passed in.</returns>
        [RequiresAuthorization]
        [ResponseType(typeof (List<GroupParticipantDTO>))]
        [VersionedRoute(template: "group/{groupId}/participants", minimumVersion: "1.0.0")]
        [Route("group/{groupId}/participants")]
        public IHttpActionResult GetGroupParticipants(int groupId)
        {
            return Authorized(token =>
            {
                try
                {
                    var participants = _groupService.GetGroupParticipants(groupId);
                    return participants == null ? (IHttpActionResult) NotFound() : Ok(participants);
                }
                catch (Exception ex)
                {
                    var apiError = new ApiErrorDto("Error getting participants for group ID " + groupId, ex);
                    throw new HttpResponseException(apiError.HttpResponseMessage);
                }

            });
        }

        /// <summary>
        /// Update the participant for a particular group
        /// </summary>
        [RequiresAuthorization]
        [VersionedRoute(template: "group/update-participants", minimumVersion: "1.0.0")]
        [Route("group/updateParticipantRole")]
        public IHttpActionResult UpdateParticipant([FromBody] GroupParticipantDTO participant)
        {
            return Authorized(token =>
            {
                try
                {
                    _groupService.UpdateGroupParticipantRole(participant);
                    return Ok();
                }
                catch (InvalidOperationException)
                {
                    return NotFound();
                }
                catch (Exception ex)
                {
                    var apiError = new ApiErrorDto("Error updating participant for participant ID " + participant.ParticipantId, ex);
                    throw new HttpResponseException(apiError.HttpResponseMessage);
                }
            });
        }

        /// <summary>
        /// Update the participant for a particular group
        /// </summary>
        [RequiresAuthorization]
        [VersionedRoute(template: "group/updateParticipantRole/{groupId}/{participantId}/{roleId}", minimumVersion: "1.0.0")]
        [Route("group/updateParticipantRole/{groupId}/{participantId}/{roleId}")]
        public IHttpActionResult UpdateParticipant([FromUri]int groupId, [FromUri]int participantId, [FromUri]int roleId)
        {
            return Authorized(token =>
            {
                try
                {
                    _groupService.UpdateGroupParticipantRole(groupId, participantId, roleId);
                    return Ok();
                }
                catch (InvalidOperationException)
                {
                    return NotFound();
                }
                catch (Exception ex)
                {
                    var apiError = new ApiErrorDto("Error updating participant for participant ID " + participantId, ex);
                    throw new HttpResponseException(apiError.HttpResponseMessage);
                }
            });
        }

        /// <summary>	
        /// Enroll the currently logged-in user into a Community Group, and register this user for all events for the CG.	
        /// Also send email confirmation to user if joining a CG	
        /// Or Add Journey/Small Group Participant to a Group	
        /// </summary>	
        [RequiresAuthorization]
        [ResponseType(typeof(GroupDTO))]
        [VersionedRoute(template: "group/{groupId}/participants", minimumVersion: "1.0.0")]
        [Route("group/{groupId}/participants")]
        [HttpPost]
        public IHttpActionResult Post(int groupId, [FromBody] List<ParticipantSignup> partId)
        {
            return Authorized(token =>
            {
                try
                {
                    _groupService.LookupParticipantIfEmpty(token, partId);

                    _groupService.addParticipantsToGroup(groupId, partId);
                    _logger.Debug(String.Format("Successfully added participants {0} to group {1}", partId, groupId));
                    return (Ok());
                }
                catch (GroupFullException e)
                {
                    var responseMessage = new ApiErrorDto("Group Is Full", e).HttpResponseMessage;

                    // Using HTTP Status code 422/Unprocessable Entity to indicate Group Is Full	
                    // http://tools.ietf.org/html/rfc4918#section-11.2	
                    responseMessage.StatusCode = (HttpStatusCode)422;
                    throw new HttpResponseException(responseMessage);
                }
                catch (Exception e)
                {
                    _logger.Error("Could not add user to group", e);
                    return BadRequest();
                }
            });
        }
    }
}

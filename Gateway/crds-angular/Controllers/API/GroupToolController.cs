using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Reflection;
using System.Web.Http;
using System.Web.Http.Description;
using crds_angular.Exceptions;
using crds_angular.Exceptions.Models;
using crds_angular.Models.Crossroads;
using crds_angular.Models.Crossroads.Groups;
using crds_angular.Models.Finder;
using crds_angular.Models.Json;
using crds_angular.Security;
using crds_angular.Services.Analytics;
using crds_angular.Services.Interfaces;
using log4net;
using Crossroads.ApiVersioning;
using Crossroads.Web.Common;
using Crossroads.Web.Common.Configuration;
using Crossroads.Web.Common.Security;

namespace crds_angular.Controllers.API
{
    public class GroupToolController : MPAuth
    {
        private readonly ILog _logger = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);
        private readonly IGroupToolService _groupToolService;
        private readonly IGroupService _groupService;

        private readonly int _defaultGroupTypeId;
        private readonly int _defaultRoleId;
        private readonly IConfigurationWrapper _configurationWrapper;
        private readonly IAnalyticsService _analyticsService;


        public GroupToolController(Services.Interfaces.IGroupToolService groupToolService,
                                   IConfigurationWrapper configurationWrapper, 
                                   IUserImpersonationService userImpersonationService, 
                                   IAuthenticationRepository authenticationRepository,
                                   IAnalyticsService analyticsService,
                                   IGroupService groupService) : base(userImpersonationService, authenticationRepository)
        {
            _groupToolService = groupToolService;
            _groupService = groupService;
            _configurationWrapper = configurationWrapper;
            _analyticsService = analyticsService;
            _defaultGroupTypeId = _configurationWrapper.GetConfigIntValue("SmallGroupTypeId");
            _defaultRoleId = _configurationWrapper.GetConfigIntValue("Group_Role_Default_ID");
        }

        /// <summary>
        /// Return all pending invitations
        /// </summary>
        /// <param name="sourceId">An integer identifying a group or a trip campaign or some entity to be named later</param>
        /// <param name="invitationTypeId">An integer indicating which invitations are to be returned. For example, Groups or Trips or a source to be identified later.</param>
        /// <returns>A list of Invitation DTOs</returns>
        [RequiresAuthorization]
        [ResponseType(typeof(List<Invitation>))]
        [VersionedRoute(template: "group-tool/invitations/{sourceId}/{invitationTypeId}", minimumVersion: "1.0.0")]
        [Route("grouptool/invitations/{sourceId}/{invitationTypeId}")]
        [HttpGet]
        public IHttpActionResult GetInvitations(int sourceId, int invitationTypeId)
        {
            return Authorized(token =>
            {
                try
                {
                    var invitess = _groupToolService.GetInvitations(sourceId, invitationTypeId, token);
                    return Ok(invitess);
                }
                catch (Exception exception)
                {
                    var apiError = new ApiErrorDto("GetInvitations Failed", exception);
                    throw new HttpResponseException(apiError.HttpResponseMessage);
                }
            });
        }

        [AcceptVerbs("GET")]
        [ResponseType(typeof(List<AttributeCategoryDTO>))]
        [VersionedRoute(template: "group-tool/categories", minimumVersion: "1.0.0")]
        [Route("grouptool/categories")]
        public IHttpActionResult GetCategories()
        {
            try
            {
                var cats = _groupToolService.GetGroupCategories();

                return Ok(cats);
            }
            catch (Exception exception)
            {
                var apiError = new ApiErrorDto("Get Group Categories Failed", exception);
                throw new HttpResponseException(apiError.HttpResponseMessage);
            }
        }

        /// <summary>
        /// Return all pending inquiries
        /// </summary>
        /// <param name="groupId">An integer identifying the group that we want the inquires for.</param>
        /// <returns>A list of Invitation DTOs</returns>
        [RequiresAuthorization]
        [ResponseType(typeof(List<Inquiry>))]
        [VersionedRoute(template: "group-tool/inquiries/{groupId}", minimumVersion: "1.0.0")]
        [Route("grouptool/inquiries/{groupId}")]
        [HttpGet]
        public IHttpActionResult GetInquiries(int groupId)
        {
            return Authorized(token =>
            {
                try
                {
                    var requestors = _groupToolService.GetInquiries(groupId, token);
                    return Ok(requestors);
                }
                catch (Exception exception)
                {
                    var apiError = new ApiErrorDto("GetInquires Failed", exception);
                    throw new HttpResponseException(apiError.HttpResponseMessage);
                }
            });
        }

        /// <summary>
        /// Ends a group and emails all participants to let them know
        /// it is over
        /// </summary>
        /// <param name="groupId">The id of a group</param>
        /// <returns>Http Result</returns>
        [RequiresAuthorization]
        [VersionedRoute(template: "grouptool/{groupId}/endsmallgroup", minimumVersion: "1.0.0")]
        [Route("grouptool/{groupId:int}/endsmallgroup")]
        [HttpPost]
        public IHttpActionResult EndSmallGroup([FromUri] int groupId)
        {
            return Authorized(token =>
            {
                try
                {
                    _groupToolService.VerifyCurrentUserIsGroupLeader(token, groupId);
                    _groupToolService.EndGroup(groupId, 4);
                    return Ok();
                }
                catch (Exception e)
                {
                    _logger.Error("Could not end group: " + groupId, e);
                    return BadRequest();
                }
            });
        }

        /// <summary>
        /// Returns the name of the a current journey or null if there isn't a journey going on.
        /// Note: Will only return one journey name.
        /// </summary>
        /// <returns>journey name (string)</returns>
        [VersionedRoute(template: "grouptool/getcurrentjourney", minimumVersion: "1.0.0")]
        [Route("grouptool/getcurrentjourney")]
        [HttpGet]
        public IHttpActionResult getCurrentJourney()
        {
            try
            {
                return Ok(new { journeyName = _groupToolService.GetCurrentJourney()} );
            }
            catch(Exception e)
            {
                _logger.Error("Could not get current journey: " + e.Message);
                return BadRequest();
            }
        }

        /// <summary>
        /// Returns the name of the a current journey or null if there isn't a journey going on.
        /// Note: Will only return one journey name.
        /// </summary>
        /// <returns>journey name (string)</returns>
        [VersionedRoute(template: "grouptool/joetest", minimumVersion: "1.0.0")]
        [Route("grouptool/joetest")]
        [HttpGet]
        public IHttpActionResult JoeTest()
        {
            try
            {
                _groupToolService.SendSmallGroupPendingInquiryReminderEmails();
                return Ok(new { journeyName = _groupToolService.GetCurrentJourney() });
            }
            catch (Exception e)
            {
                _logger.Error("Could not get current journey: " + e.Message);
                return BadRequest();
            }
        }

        /// <summary>
        /// Remove a participant from my group.
        /// </summary>
        /// <param name="groupTypeId">An integer identifying the type of group.</param>
        /// <param name="groupId">An integer identifying the group that the inquiry is associated to.</param>
        /// <param name="groupParticipantId">The ID of the group participant to remove</param>
        /// <param name="removalMessage">An optional message to send to the participant when they are removed.  This is sent along with a boilerplate message.</param>
        /// <returns>An empty response with 200 status code if everything worked, 403 if the caller does not have permission to remove a participant, or another non-success status code on any other failure</returns>
        [RequiresAuthorization]
        [VersionedRoute(template: "grouptool/group/{groupId}/participant/{groupParticipantId}", minimumVersion: "1.0.0")]
        [Route("grouptool/group/{groupId:int}/participant/{groupParticipantId:int}")]
        [HttpDelete]
        public IHttpActionResult RemoveParticipantFromMyGroup([FromUri] int groupId,
                                                              [FromUri] int groupParticipantId,
                                                              [FromUri(Name = "removalMessage")] string removalMessage = null)
        {
            return Authorized(token =>
            {
                try
                {
                    _groupToolService.RemoveParticipantFromMyGroup(token, groupId, groupParticipantId, removalMessage);
                    return Ok();
                }
                catch (GroupParticipantRemovalException e)
                {
                    var apiError = new ApiErrorDto(e.Message, null, e.StatusCode);
                    throw new HttpResponseException(apiError.HttpResponseMessage);
                }
                catch (Exception ex)
                {
                    var apiError = new ApiErrorDto(string.Format("Error removing group participant {0} from group {1}", groupParticipantId, groupId), ex);
                    throw new HttpResponseException(apiError.HttpResponseMessage);
                }
            });
        }

        /// <summary>
        /// Remove self (group participant) from group - end date group participant record and email leaders to inform.
        /// </summary>
        /// <param name="groupInformation"></param> Contains Group ID, Participant ID, and message
        /// <returns>An empty response with 200 status code if everything worked, 403 if the caller does not have permission to remove a participant, or another non-success status code on any other failure</returns>
        [RequiresAuthorization]
        [VersionedRoute(template: "group-tool/group/participant/remove-self", minimumVersion: "1.0.0")]
        [Route("group-tool/group/participant/removeself")]
        [HttpPost]
        public IHttpActionResult RemoveSelfFromGroup([FromBody] GroupParticipantRemovalDto groupInformation)
        {
            return Authorized(token =>
            {
                try
                {
                    _groupService.RemoveParticipantFromGroup(token, groupInformation.GroupId, groupInformation.GroupParticipantId);
                    return Ok();
                }
                catch (GroupParticipantRemovalException e)
                {
                    var apiError = new ApiErrorDto(e.Message, null, e.StatusCode);
                    throw new HttpResponseException(apiError.HttpResponseMessage);
                }
                catch (Exception ex)
                {
                    var apiError = new ApiErrorDto(string.Format("Error removing group participant {0} from group {1}", groupInformation.GroupParticipantId, groupInformation.GroupId), ex);
                    throw new HttpResponseException(apiError.HttpResponseMessage);
                }
            });
        }

        /// <summary>
        /// Allows an invitee to accept or deny a group invitation.
        /// DEPRICATED -- Use the function in the finder controller.
        /// </summary>
        /// <param name="groupId">An integer identifying the group that the invitation is associated to.</param>
        /// <param name="invitationKey">An string identifying the private invitation.</param>
        /// <param name="accept">A boolean showing if the invitation is being approved or denied.</param>
        [AcceptVerbs("POST")]
        // note - This AcceptVerbs attribute on an entry with the Http* Method attribute causes the
        //        API not to be included in the swagger output. We're doing it because there's a fail
        //        in the swagger code when the body has a boolean in it that breaks in the JS causing
        //        the GroopTool and all subsequent controller APIs not to show on the page. This is a
        //        stupid fix for a bug that is out of our control.
        [RequiresAuthorization]
        [VersionedRoute(template: "group-tool/group/{groupId}/invitation/{invitationKey}", minimumVersion: "1.0.0")]
        [Route("grouptool/group/{groupId:int}/invitation/{invitationKey}")]
        [HttpPost]
        public IHttpActionResult ApproveDenyGroupInvitation([FromUri] int groupId, [FromUri] string invitationKey, [FromBody] bool accept)
        {
            return Authorized(token =>
            {
                try
                {
                    _groupToolService.AcceptDenyGroupInvitation(token, groupId, invitationKey, accept);
                    return Ok();
                }
                catch (GroupParticipantRemovalException e)
                {
                    var apiError = new ApiErrorDto(e.Message, null, e.StatusCode);
                    throw new HttpResponseException(apiError.HttpResponseMessage);
                }
                catch (DuplicateGroupParticipantException e)
                {
                    var apiError = new ApiErrorDto(e.Message, null, e.StatusCode);
                    throw new HttpResponseException(apiError.HttpResponseMessage);
                }
                catch (Exception ex)
                {
                    var apiError = new ApiErrorDto(string.Format("Error when accepting: {0}, for group {1}", accept, groupId), ex);
                    throw new HttpResponseException(apiError.HttpResponseMessage);
                }
            });
        }

        /// <summary>
        /// Send an email message to all leaders of a Group
        /// </summary>
        /// <param name="groupId">An integer identifying the group that the inquiry is associated to.</param>
        /// <param name="message">A Group Message DTO that holds the subject and body of the email</param>
        [RequiresAuthorization]
        [VersionedRoute(template: "group-tool/group/{groupId}/leader-message", minimumVersion: "1.0.0")]
        [Route("grouptool/{groupId}/leadermessage")]
        public IHttpActionResult PostGroupLeaderMessage([FromUri()] int groupId, GroupMessageDTO message)
        {
            return Authorized(token =>
            {
                try
                {
                    _groupToolService.SendAllGroupLeadersEmail(token, groupId, message);
                    return Ok();
                }
                catch (InvalidOperationException)
                {
                    return (IHttpActionResult) NotFound();
                }
                catch (Exception ex)
                {
                    var apiError = new ApiErrorDto("Error sending a Leader email to groupID " + groupId, ex);
                    throw new HttpResponseException(apiError.HttpResponseMessage);
                }
            });
        }

        /// <summary>
        /// Send an email message to all members of a Group
        /// Requires the user to be a leader of the Group
        /// Will return a 404 if the user is not a Leader of the group
        /// </summary>
        [RequiresAuthorization]
        [VersionedRoute(template: "group-tool/{groupId}/{groupTypeId}/group-message", minimumVersion: "1.0.0")]
        [Route("grouptool/{groupId}/{groupTypeId}/groupmessage")]
        public IHttpActionResult PostGroupMessage([FromUri()] int groupId, [FromUri()] int groupTypeId, GroupMessageDTO message)
        {
            return Authorized(token =>
            {
                try
                {
                    _groupToolService.SendAllGroupParticipantsEmail(token, groupId, groupTypeId, message.Subject, message.Body);
                    return Ok();
                }
                catch (InvalidOperationException)
                {
                    return (IHttpActionResult) NotFound();
                }
                catch (Exception ex)
                {
                    var apiError = new ApiErrorDto("Error sending a Group email for groupID " + groupId, ex);
                    throw new HttpResponseException(apiError.HttpResponseMessage);
                }
            });
        }

        /// <summary>
        /// Return if the user is a group leader
        /// </summary>
        /// <param name="groupId">An integer identifying the group that we want to check if the user is a Leader of.</param>
        /// <param name="groupTypeId">An integer identifying the group type that we want to check if the user is a Leader of.</param>
        /// <returns>MyGroup</returns>
        [RequiresAuthorization]
        [ResponseType(typeof(MyGroup))]
        [VersionedRoute(template: "group-tool/{groupId}/is-leader", minimumVersion: "1.0.0")]
        [Route("grouptool/{groupId}/isleader")]
        [HttpGet]
        public IHttpActionResult GetIfIsGroupLeader(int groupId)
        {
            return Authorized(token =>
            {
                try
                {
                    var group = _groupToolService.VerifyCurrentUserIsGroupLeader(token, groupId);
                    return Ok(group);
                }
                catch (GroupNotFoundForParticipantException exception)
                {
                    var apiError = new ApiErrorDto("User is not in GroupId: " + groupId, exception);
                    throw new HttpResponseException(apiError.HttpResponseMessage);
                }
                catch (NotGroupLeaderException)
                {
                    //Will return empty group if they are not a group leader
                    return Ok(new MyGroup());
                }
                catch (Exception exception)
                {
                    var apiError = new ApiErrorDto("Error while verify Group Leader of GroupId: " + groupId, exception);
                    throw new HttpResponseException(apiError.HttpResponseMessage);
                }
            });
        }

        /// <summary>
        /// Create a group inquiry (typically to join a small group)
        /// </summary>
        /// <param name="groupId">An integer identifying the group</param>
        /// <param name="inquiry">The inquiry object submitted by a client.</param>
        [RequiresAuthorization]
        [VersionedRoute(template: "group-tool/group/{groupId}/submit-inquiry", minimumVersion: "1.0.0")]
        [Route("grouptool/group/{groupId:int}/submitinquiry")]
        [HttpPost]
        public IHttpActionResult SubmitGroupInquiry([FromUri()] int groupId)
        {
            return Authorized(token =>
            {
                try
                {
                    _groupToolService.SubmitInquiry(token, groupId, true);
                    return Ok();
                }
                catch (ExistingRequestException)
                {
                    return Conflict();
                }
                catch (Exception ex)
                {
                    var apiError = new ApiErrorDto(string.Format("Error when creating inquiry for group {0}", groupId), ex);
                    throw new HttpResponseException(apiError.HttpResponseMessage);
                }
            });
        }
    }
}
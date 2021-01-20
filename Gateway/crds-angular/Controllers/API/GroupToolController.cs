using System;
using System.Collections.Generic;
using System.Reflection;
using System.Web.Http;
using System.Web.Http.Description;
using crds_angular.Exceptions;
using crds_angular.Exceptions.Models;
using crds_angular.Models.Crossroads;
using crds_angular.Models.Crossroads.Groups;
using crds_angular.Models.Finder;
using crds_angular.Security;
using crds_angular.Services.Interfaces;
using log4net;
using Crossroads.ApiVersioning;
using Crossroads.Web.Common.Configuration;
using Crossroads.Web.Common.Security;

namespace crds_angular.Controllers.API
{
    public class GroupToolController : ImpersonateAuthBaseController
    {
        private readonly ILog _logger = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);
        private readonly IGroupToolService _groupToolService;
        private readonly IGroupService _groupService;

        private readonly int _defaultGroupTypeId;
        private readonly int _defaultRoleId;
        private readonly IConfigurationWrapper _configurationWrapper;
        private readonly IAnalyticsService _analyticsService;


        public GroupToolController(IAuthTokenExpiryService authTokenExpiryService, 
                                   Services.Interfaces.IGroupToolService groupToolService,
                                   IConfigurationWrapper configurationWrapper, 
                                   IUserImpersonationService userImpersonationService, 
                                   IAuthenticationRepository authenticationRepository,
                                   IAnalyticsService analyticsService,
                                   IGroupService groupService) 
            : base(authTokenExpiryService, userImpersonationService, authenticationRepository)
        {
            _groupToolService = groupToolService;
            _groupService = groupService;
            _configurationWrapper = configurationWrapper;
            _analyticsService = analyticsService;
            _defaultGroupTypeId = _configurationWrapper.GetConfigIntValue("SmallGroupTypeId");
            _defaultRoleId = _configurationWrapper.GetConfigIntValue("Group_Role_Default_ID");
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

        [AcceptVerbs("GET")]
        [ResponseType(typeof(List<AttributeCategoryDTO>))]
        [VersionedRoute(template: "group-tool/subcategories", minimumVersion: "1.0.0")]
        [Route("grouptool/subcategories")]
        public IHttpActionResult GetSubcategories()
        {
            try
            {
                var cats = _groupToolService.GetGroupSubcategories();

                return Ok(cats);
            }
            catch (Exception exception)
            {
                var apiError = new ApiErrorDto("Get Group Categories Failed", exception);
                throw new HttpResponseException(apiError.HttpResponseMessage);
            }
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
                    _groupToolService.VerifyUserIsGroupLeader(token.UserInfo.Mp.ContactId, groupId);
            
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
                    _groupToolService.SendAllGroupLeadersEmail(token.UserInfo.Mp.ContactId, groupId, message);
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
                    _groupService.RemoveParticipantFromGroup(token.UserInfo.Mp.ContactId, groupInformation.GroupId, groupInformation.GroupParticipantId);
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
                    var requestors = _groupToolService.GetInquiries(groupId, token.UserInfo.Mp.ContactId);
                    return Ok(requestors);
                }
                catch (Exception exception)
                {
                    var apiError = new ApiErrorDto("GetInquires Failed", exception);
                    throw new HttpResponseException(apiError.HttpResponseMessage);
                }
            });
        }
    }
}
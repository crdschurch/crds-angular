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
    }
}
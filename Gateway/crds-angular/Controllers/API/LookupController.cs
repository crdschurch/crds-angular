using crds_angular.Security;
using crds_angular.Services.Interfaces;
using Crossroads.ApiVersioning;
using Crossroads.Web.Common.Security;
using MinistryPlatform.Translation.Repositories;
using MinistryPlatform.Translation.Repositories.Interfaces;
using System;
using System.Collections.Generic;
using System.Web.Http;
using System.Web.Http.Description;

namespace crds_angular.Controllers.API
{
    public class LookupController : ImpersonateAuthBaseController
    {
        private readonly LookupRepository _lookupRepository;
        private readonly IUserRepository _userService;

        public LookupController(IAuthTokenExpiryService authTokenExpiryService,
            LookupRepository lookupRepository,
            IUserImpersonationService userImpersonationService,
            IAuthenticationRepository authenticationRepository,
            IUserRepository userService)
            : base(authTokenExpiryService, userImpersonationService, authenticationRepository)
        {
            _lookupRepository = lookupRepository;
            _userService = userService;
        }


        /// <summary>
        /// Get lookup values for table passed in
        /// </summary>
        [RequiresAuthorization]
        [ResponseType(typeof(List<Dictionary<string, object>>))]
        [VersionedRoute(template: "lookup/{table?}", minimumVersion: "1.0.0")]
        [Route("lookup/{table?}")]
        [HttpGet]
        public IHttpActionResult Lookup(string table)
        {
            return LookupValues(table);
        }

        private IHttpActionResult LookupValues(string table)
        {
            var ret = new List<Dictionary<string, object>>();
            switch (table)
            {
                case "genders":
                    ret = _lookupRepository.Genders();
                    break;
                case "maritalstatus":
                    ret = _lookupRepository.MaritalStatus();
                    break;
                case "serviceproviders":
                    ret = _lookupRepository.ServiceProviders();
                    break;
                case "countries":
                    ret = _lookupRepository.Countries();
                    break;
                case "states":
                    ret = _lookupRepository.States();
                    break;
                case "crossroadslocations":
                    // This returns Crossroads sites and NOT locations!
                    ret = _lookupRepository.CrossroadsLocations();
                    break;
                case "workteams":
                    ret = _lookupRepository.WorkTeams();
                    break;
                case "eventtypes":
                    ret = _lookupRepository.EventTypes();
                    break;
                case "eventtypes-eventtool":
                    ret = _lookupRepository.EventTypesForEventTool();
                    break;
                case "reminderdays":
                    ret = _lookupRepository.ReminderDays();
                    break;
                case "meetingdays":
                    ret = _lookupRepository.MeetingDays();
                    break;
                case "ministries":
                    ret = _lookupRepository.Ministries();
                    break;
                case "childcarelocations":
                    ret = _lookupRepository.ChildcareLocations();
                    break;
                case "groupreasonended":
                    ret = _lookupRepository.GroupReasonEnded();
                    break;
                default:
                    break;
            }
            if (ret.Count == 0)
            {
                return this.BadRequest(string.Format("table: {0}", table));
            }
            return Ok(ret);

        }

        /// <summary>
        /// Get lookup values for genders
        /// </summary>
        [ResponseType(typeof(List<Dictionary<string, object>>))]
        [VersionedRoute(template: "lookup/genders", minimumVersion: "1.0.0")]
        [Route("lookup/genders")]
        [HttpGet]
        public IHttpActionResult LookupGenders()
        {
            return Lookup("genders");
        }

        /// <summary>
        /// Get lookup values for event types
        /// </summary>
        [ResponseType(typeof(List<Dictionary<string, object>>))]
        [VersionedRoute(template: "lookup/event-types", minimumVersion: "1.0.0")]
        [Route("lookup/eventtypes")]
        [HttpGet]
        public IHttpActionResult LookupEventTypes(string filter = null)
        {
            var table = (filter == "event-tool") ? "eventtypes-eventtool" : "eventtypes";
            return LookupValues(table);
        }

        /// <summary>
        /// Get lookup values for event types
        /// </summary>
        [ResponseType(typeof(List<Dictionary<string, object>>))]
        [VersionedRoute(template: "lookup/childcare-locations", minimumVersion: "1.0.0")]
        [Route("lookup/childcarelocations")]
        [HttpGet]
        public IHttpActionResult LookupChildCareLocations()
        {
            return LookupValues("childcarelocations");
        }

        /// <summary>
        /// Get lookup values for group ended reasons
        /// </summary>
        [ResponseType(typeof(List<Dictionary<string, object>>))]
        [VersionedRoute(template: "lookup/group-reason-ended", minimumVersion: "1.0.0")]
        [Route("lookup/groupreasonended")]
        [HttpGet]
        public IHttpActionResult LookupGroupReasonEnded()
        {
            return LookupValues("groupreasonended");
        }

        /// <summary>
        /// Get lookup values for crossroads sites
        /// </summary>
        [ResponseType(typeof(List<Dictionary<string, object>>))]
        [VersionedRoute(template: "lookup/sites", minimumVersion: "1.0.0")]
        [Route("lookup/sites")]
        [HttpGet]
        public IHttpActionResult LookupSites()
        {
            return Lookup("crossroadslocations");
        }

        /// <summary>
        /// Get lookup values for table passed in
        /// </summary>
        [RequiresAuthorization]
        [ResponseType(typeof(List<Dictionary<string, object>>))]
        [VersionedRoute(template: "lookup/group/{congregationId}/{ministryId}", minimumVersion: "1.0.0")]
        [Route("lookup/group/{congregationid}/{ministryid}")]
        [HttpGet]
        public IHttpActionResult FindGroups(string congregationId, string ministryId)
        {
            return Authorized(t =>
            {
                var ret = new List<Dictionary<string, object>>();
                ret = _lookupRepository.GroupsByCongregationAndMinistry(congregationId, ministryId);

                if (ret.Count == 0)
                {
                    return this.BadRequest(string.Format("congregationId: {0} ministryId: {1}", congregationId, ministryId));
                }
                return Ok(ret);
            });
        }

        /// <summary>
        /// Get lookup values for table passed in
        /// </summary>
        [RequiresAuthorization]
        [ResponseType(typeof(List<Dictionary<string, object>>))]
        [VersionedRoute(template: "lookup/childcare-times/{congregationId}", minimumVersion: "1.0.0")]
        [Route("lookup/childcaretimes/{congregationId}")]
        [HttpGet]
        public IHttpActionResult FindChildcareTimes(string congregationId)
        {
            return Authorized(t =>
            {
                var ret = _lookupRepository.ChildcareTimesByCongregation(congregationId);
                return Ok(ret);
            });
        }

        /// <summary>
        /// Checks that a given email does not exist OR (when authorized) that contact record with email address has given contact id
        /// </summary>
        /// <param name="contactId"></param>
        /// <param name="email"></param>
        /// <returns></returns>
        [ResponseType(typeof(Dictionary<string, object>))]
        [VersionedRoute(template: "lookup/{contactId}/find", minimumVersion: "1.0.0")]
        [Route("lookup/{contactId}/find")]
        [HttpGet]
        public IHttpActionResult EmailExists(int contactId, string email)
        {
            var matchingRecords = _lookupRepository.EmailSearch(email);
            var recordNotFound = matchingRecords.Count == 0;

            if (recordNotFound)
            {
                return Ok();
            }

            return Authorized(doWhenAuthorized =>
            {
                var userIdFromRecord = Convert.ToInt32(matchingRecords["dp_RecordID"]);
                var contactIdFromRecord = _userService.GetContactIdByUserId(userIdFromRecord);

                if (contactIdFromRecord == contactId)
                    return Ok();
                return BadRequest();
            }, BadRequest);
        }
    }
}

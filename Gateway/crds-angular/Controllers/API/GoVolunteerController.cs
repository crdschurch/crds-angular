using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Web.Http;
using System.Web.Http.Description;
using crds_angular.Exceptions;
using crds_angular.Exceptions.Models;
using crds_angular.Models.Crossroads.Attribute;
using crds_angular.Models.Crossroads.GoVolunteer;
using crds_angular.Models.Crossroads.Lookups;
using crds_angular.Security;
using crds_angular.Services.Interfaces;
using crds_angular.Util;
using Crossroads.ApiVersioning;
using Crossroads.Web.Common.Configuration;
using Crossroads.Web.Common.Security;

namespace crds_angular.Controllers.API
{
    public class GoVolunteerController : MPAuth
    {
        private readonly IAttributeService _attributeService;
        private readonly IConfigurationWrapper _configurationWrapper;
        private readonly IGatewayLookupService _gatewayLookupService;
        private readonly IGoVolunteerService _goVolunteerService;
        private readonly IGroupConnectorService _groupConnectorService;
        private readonly IOrganizationService _organizationService;
        private readonly IGoSkillsService _skillsService;

        public GoVolunteerController(IAuthTokenExpiryService authTokenExpiryService, 
                                     IOrganizationService organizationService,
                                     IGroupConnectorService groupConnectorService,
                                     IGatewayLookupService gatewayLookupService,
                                     IGoSkillsService skillsService,
                                     IGoVolunteerService goVolunteerService,
                                     IAttributeService attributeService,
                                     IConfigurationWrapper configurationWrapper,
                                     IUserImpersonationService userImpersonationService,
                                     IAuthenticationRepository authenticationRepository)
            : base(authTokenExpiryService, userImpersonationService, authenticationRepository)

        {
            _organizationService = organizationService;
            _gatewayLookupService = gatewayLookupService;
            _goVolunteerService = goVolunteerService;
            _groupConnectorService = groupConnectorService;
            _skillsService = skillsService;
            _attributeService = attributeService;
            _configurationWrapper = configurationWrapper;
        }

        [ResponseType(typeof(List<ChildrenOptions>))]
        [VersionedRoute(template: "go-volunteer/children", minimumVersion: "1.0.0")]
        [Route("govolunteer/children")]
        [HttpGet]
        public IHttpActionResult GetGoChildrenOptions()
        {
            try
            {
                var options = _goVolunteerService.ChildrenOptions();
                return Ok(options);
            }
            catch (Exception e)
            {
                var apiError = new ApiErrorDto("Get Go Volunteer Children Options failed: ", e);
                throw new HttpResponseException(apiError.HttpResponseMessage);
            }
        }

        [ResponseType(typeof(List<DashboardDatum>))]
        [VersionedRoute(template: "go-volunteer/dashboard/{projectId}", minimumVersion: "1.0.0")]
        [Route("go-volunteer/dashboard/{projectId}")]
        [HttpGet]
        public IHttpActionResult GetDashboardData(int projectId)
        {
            try
            {
                return Ok(_goVolunteerService.GetRegistrationsForProject(projectId));
            }
            catch (Exception e)
            {
                var apiError = new ApiErrorDto("Get Dashboard Data failed: ", e);
                throw new HttpResponseException(apiError.HttpResponseMessage);
            }
        }

        [ResponseType(typeof(Project))]
        [VersionedRoute(template: "go-volunteer/project/{projectId}", minimumVersion: "1.0.0")]
        [Route("go-volunteer/project/{projectId}")]
        [HttpGet]
        public IHttpActionResult GetProject(int projectId)
        {
            try
            {
                return Ok(_goVolunteerService.GetProject(projectId));
            }
            catch (Exception e)
            {
                var apiError = new ApiErrorDto("Get Project failed: ", e);
                throw new HttpResponseException(apiError.HttpResponseMessage);
            }
        }

        [VersionedRoute(template: "go-volunteer/prep-times", minimumVersion: "1.0.0")]
        [ResponseType(typeof(List<AttributeDTO>))]
        [Route("govolunteer/prep-times")]
        [HttpGet]
        public IHttpActionResult GetPrepTimes()
        {
            try
            {
                return Ok(GetAttributesByType("PrepWorkAttributeTypeId"));
            }
            catch (Exception e)
            {
                var apiError = new ApiErrorDto("Get Prep Times failed: ", e);
                throw new HttpResponseException(apiError.HttpResponseMessage);
            }
        }

        [ResponseType(typeof(List<AttributeDTO>))]
        [VersionedRoute(template: "go-volunteer/equipment", minimumVersion: "1.0.0")]
        [Route("govolunteer/equipment")]
        [HttpGet]
        public IHttpActionResult GetGoEquipment()
        {
            try
            {
                return Ok(GetAttributesByType("GoCincinnatiEquipmentAttributeType"));
            }
            catch (Exception e)
            {
                var apiError = new ApiErrorDto("Get Go Volunteer Equipment failed: ", e);
                throw new HttpResponseException(apiError.HttpResponseMessage);
            }
        }

        private List<AttributeDTO> GetAttributesByType(string attributeType)
        {
            var attributeTypeId = _configurationWrapper.GetConfigIntValue(attributeType);
            var attributeTypes = _attributeService.GetAttributeTypes(attributeTypeId);
            return attributeTypes.Single().Attributes;
        }

        [ResponseType(typeof(List<GoSkills>))]
        [VersionedRoute(template: "go-volunteer/skills", minimumVersion: "1.0.0")]
        [Route("govolunteer/skills")]
        [HttpGet]
        public IHttpActionResult GetGoSkills()
        {
            return (Authorized(Skills, () => Skills(string.Empty)));
        }

        private IHttpActionResult Skills(string token)
        {
            try
            {
                var skills = _skillsService.RetrieveGoSkills(token);
                return Ok(skills);
            }
            catch (Exception e)
            {
                var apiError = new ApiErrorDto("Get Go Volunteer Skills failed: ", e);
                throw new HttpResponseException(apiError.HttpResponseMessage);
            }
        }

        [ResponseType(typeof(List<GroupConnector>))]
        [VersionedRoute(template: "group-connectors/open-orgs/{initiativeId}", minimumVersion: "1.0.0")]
        [Route("group-connectors/open-orgs/{initiativeId}")]
        [HttpGet]
        public IHttpActionResult GetGetGroupConnectorsOpenOrgs(int initiativeId)
        {
            try
            {
                var groupConnectors = _groupConnectorService.GetGroupConnectorsForOpenOrganizations(initiativeId);

                if (groupConnectors == null)
                {
                    return NotFound();
                }
                return Ok(groupConnectors);
            }
            catch (Exception e)
            {
                const string msg = "GoVolunteerController.GetGetGroupConnectorsOpenOrgs";
                logger.Error(msg, e);
                var apiError = new ApiErrorDto(msg, e);
                throw new HttpResponseException(apiError.HttpResponseMessage);
            }
        }

        [ResponseType(typeof(List<GroupConnector>))]
        [VersionedRoute(template: "group-connectors/{organizationId}/{initiativeId}", minimumVersion: "1.0.0")]
        [Route("group-connectors/{organizationId}/{initiativeId}")]
        [HttpGet]
        public IHttpActionResult GetGroupConnectorsForOrg(int organizationId, int initiativeId)
        {
            try
            {
                var groupConnectors = _groupConnectorService.GetGroupConnectorsByOrganization(organizationId, initiativeId);

                if (groupConnectors == null)
                {
                    return NotFound();
                }
                return Ok(groupConnectors);
            }
            catch (Exception e)
            {
                const string msg = "GoVolunteerController.GetGroupConnectorsForOrg";
                logger.Error(msg, e);
                var apiError = new ApiErrorDto(msg, e);
                throw new HttpResponseException(apiError.HttpResponseMessage);
            }
        }

        [ResponseType(typeof(Organization))]
        [VersionedRoute(template: "organization/{organizationName}", minimumVersion: "1.0.0")]
        [Route("organization/{organizationName}")]
        [HttpGet]
        public IHttpActionResult GetOrganization(string organizationName)
        {
            try
            {
                var org = _organizationService.GetOrganizationByName(organizationName);
                if (org == null)
                {
                    return NotFound();
                }
                return Ok(org);
            }
            catch (Exception e)
            {
                var apiError = new ApiErrorDto("Get Organization failed: ", e);
                throw new HttpResponseException(apiError.HttpResponseMessage);
            }
        }

        [ResponseType(typeof(List<OtherOrganization>))]
        [VersionedRoute(template: "organizations/other", minimumVersion: "1.0.0")]
        [Route("organizations/other")]
        [HttpGet]
        public IHttpActionResult GetOtherOrganizations()
        {
            try
            {
                var Orgs = _gatewayLookupService.GetOtherOrgs();
                return Ok(Orgs);
            }
            catch (Exception e)
            {
                var apiError = new ApiErrorDto("Unable to get other organizations", e);
                throw new HttpResponseException(apiError.HttpResponseMessage);
            }
        }

        [ResponseType(typeof(List<OrgLocation>))]
        [VersionedRoute(template: "organizations/{organizationId}/locations", minimumVersion: "1.0.0")]
        [Route("organizations/{organizationId}/locations")]
        [HttpGet]
        public IHttpActionResult GetLocationsForOrganization(int organizationId)
        {
            try
            {
                var Locs = _organizationService.GetLocationsForOrganization(organizationId);
                return Ok(Locs);
            }
            catch (Exception e)
            {
                var apiError = new ApiErrorDto("Unable to get locations", e);
                throw new HttpResponseException(apiError.HttpResponseMessage);
            }
        }

        [ResponseType(typeof(List<ProjectType>))]
        [VersionedRoute(template: "go-volunteer/project-types", minimumVersion: "1.0.0")]
        [Route("goVolunteer/projectTypes")]
        [HttpGet]
        public IHttpActionResult GetProjectTypes()
        {
            try
            {
                var projectTypes = _goVolunteerService.GetProjectTypes();
                return Ok(projectTypes);
            }
            catch (Exception e)
            {
                var apiError = new ApiErrorDto("Unable to get project types", e);
                throw new HttpResponseException(apiError.HttpResponseMessage);
            }
        }

        [VersionedRoute(template: "go-volunteer/registration", minimumVersion: "1.0.0")]
        [Route("govolunteer/registration")]
        [ResponseType(typeof(Registration))]
        [HttpPost]
        public IHttpActionResult Post([FromBody] CincinnatiRegistration goVolunteerRegistration)
        {
            if (ModelState.IsValid)
            {
                return Authorized(token => SaveRegistration(token, goVolunteerRegistration),
                                  () => SaveRegistration(string.Empty, goVolunteerRegistration));
            }
            var errors = ModelState.Values.SelectMany(val => val.Errors).Aggregate("", (current, err) => current + err.ErrorMessage);
            var dataError = new ApiErrorDto("Registration Data Invalid", new InvalidOperationException("Invalid Registration Data" + errors));
            throw new HttpResponseException(dataError.HttpResponseMessage);
        }

        [VersionedRoute(template: "go-volunteer/registration/{projectId}", minimumVersion: "1.0.0")]
        [Route("go-volunteer/registration/{projectId}")]
        [ResponseType(typeof(AnywhereRegistration))]
        [HttpPost]
        public IHttpActionResult Post([FromBody] AnywhereRegistration goVolunteerRegistration, int projectId)
        {
            try
            {
                if (ModelState.IsValid)
                {
                    return Authorized(token => Ok(_goVolunteerService.CreateAnywhereRegistration(goVolunteerRegistration, projectId, token)));
                }
                var errors = ModelState.Values.SelectMany(val => val.Errors).Aggregate("", (current, err) => current + err.ErrorMessage);
                var dataError = new ApiErrorDto("Registration Data Invalid", new InvalidOperationException("Invalid Registration Data" + errors));
                throw new HttpResponseException(dataError.HttpResponseMessage);
            }
            catch (DuplicateUserException e)
            {
                throw new HttpResponseException(HttpStatusCode.Conflict);
            }
            catch (Exception e)
            {
                var msg = "GoVolunteerRegistrationController: POST " + goVolunteerRegistration;
                logger.Error(msg, e);
                var apiError = new ApiErrorDto(msg, e);
                throw new HttpResponseException(apiError.HttpResponseMessage);
            }
        }

        [VersionedRoute(template: "go-volunteer/cities/{initiativeId}", minimumVersion: "1.0.0")]
        [Route("go-volunteer/cities/{initiativeId}")]
        [ResponseType(typeof (List<ProjectCity>))]
        [HttpGet]
        public IHttpActionResult GetParticipatingCities(int initiativeId)
        {
            try
            {
                var cities = _goVolunteerService.GetParticipatingCities(initiativeId);
                return Ok(cities);
            }
            catch (Exception e)
            {
                var apiError = new ApiErrorDto("Unable to get cities", e);
                throw new HttpResponseException(apiError.HttpResponseMessage);
            }
        }

        [VersionedRoute(template: "go-volunteer/organizations", minimumVersion: "1.0.0")]
        [Route("go-volunteer/organizations")]
        [ResponseType(typeof(List<Organization>))]
        [HttpGet]
        public IHttpActionResult GetOrganizations()
        {
            try
            {
                var orgs = _organizationService.GetOrganizations();
                return Ok(orgs);
            }
            catch (Exception e)
            {
                var apiError = new ApiErrorDto("Unable to get organizations", e);
                throw new HttpResponseException(apiError.HttpResponseMessage);
            }
        }

        [VersionedRoute(template: "go-volunteer/dashboard/export/{projectId}", minimumVersion: "1.0.0")]
        [Route("go-volunteer/dashboard/export/{projectId}")]
        [HttpGet]
        public IHttpActionResult GetGroupLeaderExportFile(int projectId)
        {
            return Authorized(token =>
            {
                try
                {
                    string filename = "groupLeaderExport.csv";
                    var stream = _goVolunteerService.CreateGroupLeaderExport(projectId);
                    return new FileResult(stream, filename);
                }
                catch (Exception e)
                {
                    var apiError = new ApiErrorDto("Unable to get Group Leader Export File", e);
                    throw new HttpResponseException(apiError.HttpResponseMessage);
                }
            });
        }

        private IHttpActionResult SaveRegistration(string token, CincinnatiRegistration goVolunteerRegistration)
        {
            try
            {                
                var reg = _goVolunteerService.CreateRegistration(goVolunteerRegistration, token);
                return Ok(reg);
            }
            catch (Exception e)
            {
                var msg = "GoVolunteerRegistrationController: POST " + goVolunteerRegistration;
                logger.Error(msg, e);
                var apiError = new ApiErrorDto(msg, e);
                throw new HttpResponseException(apiError.HttpResponseMessage);
            }
        }
    }
}

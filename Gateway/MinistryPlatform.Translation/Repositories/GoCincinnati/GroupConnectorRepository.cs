using System;
using System.Collections.Generic;
using System.Linq;
using Crossroads.Utilities.Interfaces;
using log4net;
using Crossroads.Web.Common;
using Crossroads.Web.Common.Configuration;
using Crossroads.Web.Common.MinistryPlatform;
using Crossroads.Web.Common.Security;
using MinistryPlatform.Translation.Extensions;
using MinistryPlatform.Translation.Models.GoCincinnati;
using MinistryPlatform.Translation.Repositories.Interfaces;
using MinistryPlatform.Translation.Repositories.Interfaces.GoCincinnati;
using IGroupConnectorRepository = MinistryPlatform.Translation.Repositories.Interfaces.GoCincinnati.IGroupConnectorRepository;

namespace MinistryPlatform.Translation.Repositories.GoCincinnati
{
    public class GroupConnectorRepository : BaseRepository, Interfaces.GoCincinnati.IGroupConnectorRepository
    {
        private readonly ILog _logger = LogManager.GetLogger(typeof (RoomRepository));
        private readonly IMinistryPlatformService _ministryPlatformService;
        private readonly IMinistryPlatformRestRepository _ministryPlatformRestRepository;
        private readonly string _apiToken;

        public GroupConnectorRepository(
            IMinistryPlatformService ministryPlatformService,
            IMinistryPlatformRestRepository ministryPlatformRestRepository,
            IAuthenticationRepository authenticationService,
            IConfigurationWrapper configuration,
            IApiUserRepository apiUserRepository)
            : base(authenticationService, configuration, apiUserRepository)
        {
            _ministryPlatformService = ministryPlatformService;
            _ministryPlatformRestRepository = ministryPlatformRestRepository;
            _apiToken = ApiLogin();
        }

        public int CreateGroupConnector(int registrationId, bool privateGroup)
        {
            var pageId = _configurationWrapper.GetConfigIntValue("GroupConnectorPageId");
            var dictionary = new Dictionary<string, object>
            {
                {"Primary_Registration", registrationId},
                {"Private", privateGroup}
            };

            try
            {
                var groupConnectorId = _ministryPlatformService.CreateRecord(pageId, dictionary, _apiToken, true);
                CreateGroupConnectorRegistration(groupConnectorId, registrationId);
                return groupConnectorId;
            }
            catch (Exception e)
            {
                var msg = string.Format("Error creating Go Cincinnati Group Connector, registration: {0}", registrationId);
                _logger.Error(msg, e);
                throw (new ApplicationException(msg, e));
            }
        }

        public int CreateGroupConnectorRegistration(int groupConnectorId, int registrationId)
        {
            var dictionary = new Dictionary<string, object>
            {
                {"Registration_ID", registrationId}
            };

            try
            {
                return _ministryPlatformService.CreateSubRecord("GroupConnectorRegistrationPageId", groupConnectorId, dictionary, _apiToken, true);
            }
            catch (Exception e)
            {
                var msg = string.Format("Error creating Go Cincinnati Group Connector Registration, group connector: {0}, registration: {1}", groupConnectorId, registrationId);
                _logger.Error(msg, e);
                throw (new ApplicationException(msg, e));
            }
        }

        public MpGroupConnector GetGroupConnectorById(int groupConnectorId)
        {
            var searchString = string.Format(",,,,,,,,,,,\"{0}\"", groupConnectorId);
            var groupConnectors = GetGroupConnectors(searchString);
            var groupConnector = groupConnectors.SingleOrDefault();
            return groupConnector;
        }

        public List<MpGroupConnector> GetGroupConnectorsForOpenOrganizations(int initiativeId)
        {
            var searchString = string.Format(",,,,,true,{0}", initiativeId);
            return GetGroupConnectors(searchString);
        }

        public List<MpGroupConnector> GetGroupConnectorsForOrganization(int organizationId, int initiativeId)
        {
            var searchString = string.Format(",,,,{0},,{1}", organizationId, initiativeId);
            return GetGroupConnectors(searchString);
        }

        public MpGroupConnector GetGroupConnectorByProjectId(int projectId, string token)
        {
            var groupConnectors = _ministryPlatformRestRepository.UsingAuthenticationToken(token).Search<MpGroupConnector>($"Project_ID = {projectId}");

            return groupConnectors.FirstOrDefault();
        }

        private List<MpGroupConnector> GetGroupConnectors(string searchString)
        {
            var result = _ministryPlatformService.GetPageViewRecords("GroupConnectorPageView", _apiToken, searchString);

            return result.Select(r => new MpGroupConnector
            {
                Id = r.ToInt("Group_Connector_ID"),
                Name = r.ToString("Primary_Registration"),
                PrimaryRegistrationID = r.ToInt("Primary_Registration_Contact_ID"),
                ProjectMaximumVolunteers = r.ToInt("Project_Maximum_Volunteers"),
                ProjectMinimumAge = r.ToInt("_Minimum_Age"),
                ProjectName = r.ToString("Project_Name"),
                ProjectType = r.ToString("Project_Type"),
                PreferredLaunchSite = r.ToString("Preferred_Launch_Site"),
                VolunteerCount = r.ToInt("Volunteer_Count"),
                PreferredLaunchSiteId = r.ToInt("Preferred_Launch_Site_ID"),
                ProjectId = r.ToNullableInt("Project_ID")
            }).ToList();
        }
    }
}
﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Text;
using System.Threading.Tasks;
using Crossroads.Utilities.Interfaces;
using MinistryPlatform.Translation.Models;
using MinistryPlatform.Translation.Repositories.Interfaces;

namespace MinistryPlatform.Translation.Repositories
{
    public class CampRepository : ICampRepository
    {
        private readonly IMinistryPlatformRestRepository _ministryPlatformRest;
        private readonly IConfigurationWrapper _configurationWrapper;
        private readonly IApiUserRepository _apiUserRepository;

        public CampRepository(IConfigurationWrapper configurationWrapper, IMinistryPlatformRestRepository ministryPlatformRest, IApiUserRepository apiUserRepository)
        {
            _configurationWrapper = configurationWrapper;
            _ministryPlatformRest = ministryPlatformRest;
            _apiUserRepository = apiUserRepository;
        }

        public List<MpCampEvent> GetCampEventDetails(int eventId)
        {
            var apiToken = _apiUserRepository.GetToken();
            var parms = new Dictionary<string, object> { { "Event_ID", eventId }, { "Domain_ID", 1 } };
            var campEventData = _ministryPlatformRest.UsingAuthenticationToken(apiToken).GetFromStoredProc<MpCampEvent>(_configurationWrapper.GetConfigValue("CampEventStoredProc"), parms);
            var campEvent = campEventData.FirstOrDefault() ?? new List<MpCampEvent>();
            return campEvent;
        }

        public int CreateMinorContact(MpMinorContact minorContact)
        {
            return 0;
        }
    }
}

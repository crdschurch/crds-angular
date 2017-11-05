using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Crossroads.Utilities.FunctionalHelpers;
using Crossroads.Web.Common.Configuration;
using Crossroads.Web.Common.MinistryPlatform;
using Crossroads.Web.Common.Security;
using MinistryPlatform.Translation.Models;
using MinistryPlatform.Translation.Repositories.Interfaces;

namespace MinistryPlatform.Translation.Repositories
{
    class LocationRepository : BaseRepository, ILocationRepository
    {
        private readonly IMinistryPlatformRestRepository _ministryPlatformRestRepository;

        public LocationRepository(IMinistryPlatformRestRepository ministryPlatformRestRepository, IAuthenticationRepository authenticationService, IConfigurationWrapper configurationWrapper) : base(authenticationService, configurationWrapper)
        {
            _ministryPlatformRestRepository = ministryPlatformRestRepository;
        }

        public List<MpLocation> GetLocations()
        {
            var apiToken = ApiLogin();
            Dictionary<string, object> filter = new Dictionary<string, object>();
            string columns = "Location_ID, Location_Name, Location_Type_ID, Address_ID_Table.*";
            return _ministryPlatformRestRepository.UsingAuthenticationToken(apiToken).SearchTable<MpLocation>("Locations", null, columns);
        }
    }
}

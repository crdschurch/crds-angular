using System.Collections.Generic;
using System.Linq;
using Crossroads.Utilities.FunctionalHelpers;
using Crossroads.Web.Common.Configuration;
using Crossroads.Web.Common.MinistryPlatform;
using Crossroads.Web.Common.Security;
using MinistryPlatform.Translation.Extensions;
using MinistryPlatform.Translation.Models;
using MinistryPlatform.Translation.Repositories.Interfaces;

namespace MinistryPlatform.Translation.Repositories
{
    public class CongregationRepository : BaseRepository, ICongregationRepository
    {
        private readonly IMinistryPlatformService _ministryPlatformService;
        private readonly IMinistryPlatformRestRepository _ministryPlatformRestRepository;

        public CongregationRepository(IMinistryPlatformService ministryPlatformService, IMinistryPlatformRestRepository ministryPlatformRestRepository, IAuthenticationRepository authenticationService, IConfigurationWrapper configuration)
            : base(authenticationService, configuration)
        {
            _ministryPlatformService = ministryPlatformService;
            _ministryPlatformRestRepository = ministryPlatformRestRepository;

        }

        public List<MpCongregation> GetCongregations(string searchString = null)
        {
            var apiToken = ApiLogin();
            string columns = "Congregation_ID, Congregation_Name, Location_ID";
            return _ministryPlatformRestRepository.UsingAuthenticationToken(apiToken).Search<MpCongregation>(searchString, columns);
        }

        public MpCongregation GetCongregationById(int id)
        {
            var token = ApiLogin();
            var pageId = _configurationWrapper.GetConfigIntValue("CrossroadsLocations");
            Dictionary<string, object> recordDict;
            try
            {
                recordDict = _ministryPlatformService.GetRecordDict(pageId, id, token);
            }
            catch (System.ServiceModel.FaultException fault)
            {
                // this is terrible, but can't find another way to handle!!!!
                if (fault.Message.StartsWith("Record is not found"))
                {
                    return null;
                }
                throw;
            }

            var c = new MpCongregation();
            c.CongregationId = recordDict.ToInt("Congregation_ID");
            c.Name = recordDict.ToString("Congregation_Name");
            c.LocationId = recordDict.ToInt("Location_ID");
            return c;
        }

        public Result<MpCongregation> GetCongregationByName(string congregationName, string token)
        {
            var searchString = $"Congregations.[Congregation_Name]='{congregationName}'";
            var result = _ministryPlatformRestRepository.UsingAuthenticationToken(token).Search<MpCongregation>(searchString);
            if (result.Any())
            {
                return new Ok<MpCongregation>(result.FirstOrDefault());
            }
            return new Err<MpCongregation>($"Unable to find congregation named {congregationName}");
        }
    }
}
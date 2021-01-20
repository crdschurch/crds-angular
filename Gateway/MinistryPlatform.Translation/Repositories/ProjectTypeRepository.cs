using System.Collections.Generic;
using System.Linq;
using Crossroads.Web.Common.Configuration;
using Crossroads.Web.Common.Security;
using MinistryPlatform.Translation.Extensions;
using MinistryPlatform.Translation.Models.GoCincinnati;
using MinistryPlatform.Translation.Repositories.Interfaces;
using Crossroads.Web.Common.MinistryPlatform;

namespace MinistryPlatform.Translation.Repositories
{
    public class ProjectTypeRepository : BaseRepository,IProjectTypeRepository
    {
        private readonly IMinistryPlatformService _ministryPlatformService;

        public ProjectTypeRepository(
            IAuthenticationRepository authenticationService,
            IConfigurationWrapper configurationWrapper,
            IMinistryPlatformService ministryPlatformService,
            IApiUserRepository apiUserRepository)
            : base(authenticationService, configurationWrapper, apiUserRepository)
        {
            _ministryPlatformService = ministryPlatformService;
        }

        public List<MpProjectType> GetProjectTypes()
        {
            var token = ApiLogin();
            var result = _ministryPlatformService.GetRecordsDict(_configurationWrapper.GetConfigIntValue("ProjectTypes"), token);
            return MapProjectTypes(result);
        }

        private static List<MpProjectType> MapProjectTypes(IEnumerable<Dictionary<string, object>> records)
        {
            return records.Select(record => new MpProjectType
            {
                ProjectTypeId = record.ToInt("dp_RecordID"),
                Description = record.ToString("Description"),
                MinAge = record.ToInt("Minimum_Age"),
                SortOrder = record.ToInt("Sort_Order"),
                ImageUrl = record.ToString("Image_URL")
            }).ToList();
        }
    }
}

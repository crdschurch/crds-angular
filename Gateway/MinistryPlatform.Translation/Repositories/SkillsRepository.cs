using System.Collections.Generic;
using System.Linq;
using Crossroads.Web.Common.Configuration;
using Crossroads.Web.Common.Security;
using MinistryPlatform.Translation.Extensions;
using MinistryPlatform.Translation.Models;
using MinistryPlatform.Translation.Repositories.Interfaces;
using Crossroads.Web.Common.MinistryPlatform;

namespace MinistryPlatform.Translation.Repositories
{
    public class SkillsRepository : BaseRepository, ISkillsRepository
    {
        private readonly IMinistryPlatformService _ministryPlatformService;

        public SkillsRepository(
            IAuthenticationRepository authenticationService,
            IConfigurationWrapper configurationWrapper,
            IMinistryPlatformService ministryPlatformService,
            IApiUserRepository apiUserRepository)
            : base(authenticationService, configurationWrapper, apiUserRepository)
        {
            _ministryPlatformService = ministryPlatformService;
        }

        public List<MpGoVolunteerSkill> GetGoVolunteerSkills(string token)
        {
            var goSkillsPageId = _configurationWrapper.GetConfigIntValue("GoVolunteerSkills");
            var records = _ministryPlatformService.GetRecordsDict(goSkillsPageId, token);

            return records.Select(record => new MpGoVolunteerSkill(record.ToInt("Go_Volunteer_Skills_ID"),
                                                                   record.ToString("Label"),
                                                                   record.ToString("Attribute_Name"),
                                                                   record.ToInt("Attribute_ID"))).ToList();
        }
    }
}
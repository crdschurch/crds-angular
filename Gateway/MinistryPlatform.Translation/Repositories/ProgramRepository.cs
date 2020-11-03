using System.Collections.Generic;
using System.Linq;
using AutoMapper;
using Crossroads.Web.Common.Configuration;
using Crossroads.Web.Common.MinistryPlatform;
using Crossroads.Web.Common.Security;
using MinistryPlatform.Translation.Models;
using MinistryPlatform.Translation.Repositories.Interfaces;

namespace MinistryPlatform.Translation.Repositories
{
    public class ProgramRepository : BaseRepository, IProgramRepository
    {
        private readonly IMinistryPlatformService _ministryPlatformService;
        private readonly IMinistryPlatformRestRepository _ministryPlatformRest;
        private readonly int _onlineGivingProgramsPageViewId;
        private readonly int _programsPageId;

        public ProgramRepository(IMinistryPlatformService ministryPlatformService,
                                 IAuthenticationRepository authenticationService,
                                 IConfigurationWrapper configurationWrapper,
                                 IMinistryPlatformRestRepository ministryPlatformRest,
                                 IApiUserRepository apiUserRepository)
            : base(authenticationService, configurationWrapper, apiUserRepository)
        {
            _ministryPlatformService = ministryPlatformService;
            _ministryPlatformRest = ministryPlatformRest;
            _onlineGivingProgramsPageViewId = configurationWrapper.GetConfigIntValue("OnlineGivingProgramsPageViewId");
            _programsPageId = configurationWrapper.GetConfigIntValue("Programs");
        }

        public MpProgram GetProgramById(int programId)
        {
            return (WithApiLogin(token => (Mapper.Map<MpProgram>(_ministryPlatformService.GetRecordDict(_programsPageId, programId, token)))));
        }
    }
}
using System.Collections.Generic;
using crds_angular.Models.Crossroads.Serve;
using crds_angular.Services.Interfaces;
using Crossroads.AsyncJobs.Interfaces;
using Crossroads.AsyncJobs.Models;
using Crossroads.Utilities.Extensions;
using Crossroads.Utilities.Interfaces;
using Crossroads.Web.Common;
using Crossroads.Web.Common.Configuration;
using Crossroads.Web.Common.Security;
using MinistryPlatform.Translation.Repositories;
using MinistryPlatform.Translation.Repositories.Interfaces;
using Crossroads.Web.Common.MinistryPlatform;

namespace Crossroads.AsyncJobs.Processors
{
    public class SignupToServeProcessor : BaseRepository, IJobExecutor<SaveRsvpDto>
    {
        private readonly IServeService _serveService;
        private readonly IConfigurationWrapper _configurationWrapper;
        private readonly IAuthenticationRepository _authenticationService;

        public SignupToServeProcessor(
            IServeService serveService,
            IConfigurationWrapper configurationWrapper,
            IAuthenticationRepository authenticationService,
            IApiUserRepository apiUserRepository)
            : base(authenticationService, configurationWrapper, apiUserRepository)
        {
            this._serveService = serveService;
            this._configurationWrapper = configurationWrapper;
            this._authenticationService = authenticationService;
        }

        public void Execute(JobDetails<SaveRsvpDto> details)
        {
            WithApiLogin<List<int>>(token => _serveService.SaveServeRsvp(token, details.Data));            
        }
    }
}
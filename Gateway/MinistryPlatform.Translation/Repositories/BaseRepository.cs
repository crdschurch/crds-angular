using System;
using System.Configuration;
using Crossroads.Utilities.Interfaces;
using Crossroads.Web.Common;
using Crossroads.Web.Common.Configuration;
using Crossroads.Web.Common.Security;
using MinistryPlatform.Translation.Repositories.Interfaces;
using Crossroads.Web.Common.MinistryPlatform;

namespace MinistryPlatform.Translation.Repositories
{
    public class BaseRepository
    {
        protected readonly IAuthenticationRepository _authenticationService;
        protected readonly IConfigurationWrapper _configurationWrapper;
        protected readonly IApiUserRepository _apiUserRepositor;

        public BaseRepository(
            IAuthenticationRepository authenticationService,
            IConfigurationWrapper configurationWrapper,
            IApiUserRepository apiUserRepository)
        {
            this._authenticationService = authenticationService;
            this._configurationWrapper = configurationWrapper;
            this._apiUserRepositor = apiUserRepository;
        }

        protected static int AppSettings(string pageKey)
        {
            int pageId;
            if (!int.TryParse(ConfigurationManager.AppSettings[pageKey], out pageId))
            {
                throw new InvalidOperationException(string.Format("Invalid Page Key: {0}", pageKey));
            }
            return pageId;
        }

        protected T WithApiLogin<T>(Func<string, T> doIt)
        {
            return (doIt(ApiLogin()));
        }

        protected string ApiLogin()
        {
            return this._apiUserRepositor.GetDefaultApiClientToken();
        }



        protected static int AppSetting(string key)
        {
            int value;
            if (!int.TryParse(ConfigurationManager.AppSettings[key], out value))
            {
                throw new InvalidOperationException(string.Format("Invalid Page Key: {0}", key));
            }
            return value;
        }
    }
}
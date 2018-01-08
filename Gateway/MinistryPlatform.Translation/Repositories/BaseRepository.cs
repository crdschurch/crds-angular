using System;
using System.Configuration;
using Crossroads.Utilities.Interfaces;
using Crossroads.Web.Common;
using Crossroads.Web.Common.Configuration;
using Crossroads.Web.Common.Security;
using MinistryPlatform.Translation.Repositories.Interfaces;

namespace MinistryPlatform.Translation.Repositories
{
    public class BaseRepository
    {
        protected readonly IAuthenticationRepository _authenticationService;
        protected readonly IConfigurationWrapper _configurationWrapper;

        public BaseRepository(IAuthenticationRepository authenticationService, IConfigurationWrapper configurationWrapper)
        {
            this._authenticationService = authenticationService;
            this._configurationWrapper = configurationWrapper;
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
            var apiUser = _configurationWrapper.GetEnvironmentVarAsString("API_USER");
            var apiPasword = _configurationWrapper.GetEnvironmentVarAsString("API_PASSWORD");
            var authData = _authenticationService.Authenticate(apiUser, apiPasword);
            var token = authData.AccessToken;

            return (token);
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
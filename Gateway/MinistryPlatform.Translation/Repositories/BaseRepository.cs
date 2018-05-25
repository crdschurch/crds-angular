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
            // TODO: Refactor this to use IApiUserRepository.GetDefaultApiClientToken
            var clientId = _configurationWrapper.GetEnvironmentVarAsString("CRDS_MP_COMMON_CLIENT_ID");
            var clientSecret = _configurationWrapper.GetEnvironmentVarAsString("CRDS_MP_COMMON_CLIENT_SECRET");
            var authData = _authenticationService.AuthenticateClient(clientId, clientSecret);
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
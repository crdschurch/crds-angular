using crds_angular.Services.Interfaces;
using Crossroads.Web.Common.Configuration;

namespace crds_angular.Services
{
    public class ConfigurationService : IConfigurationService
    {
        private readonly IConfigurationWrapper _configurationWrapper;

        public ConfigurationService(IConfigurationWrapper configurationWrapper)
        {
            _configurationWrapper = configurationWrapper;
        }

        public string GetMpConfigValue(string appCode, string key, bool throwIfNotFound = false)
        {
            return _configurationWrapper.GetMpConfigValue(appCode, key, throwIfNotFound);
        }
    }
}

namespace crds_angular.Services.Interfaces
{
    public interface IConfigurationService
    {
        string GetMpConfigValue(string appCode, string key, bool throwIfNotFound = false);
    }
}
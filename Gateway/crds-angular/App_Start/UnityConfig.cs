using Microsoft.Practices.Unity;
using Microsoft.Practices.Unity.Configuration;
using System.Configuration;
using System.Linq;
using System.Web.Http;
using Unity.WebApi;


namespace crds_angular
{
    public static class UnityConfig
    {
        public static void RegisterComponents()
        {
            var container = new UnityContainer();
            var unitySections = new[] { "crossroadsCommonUnity", "crossroadsClientApiKeysUnity", "unity" };

            foreach (var section in unitySections.Select(sectionName => (UnityConfigurationSection) ConfigurationManager.GetSection(sectionName)))
            {
                container.LoadConfiguration(section);
            }

            GlobalConfiguration.Configuration.DependencyResolver = new UnityDependencyResolver(container);
        }
    }
}
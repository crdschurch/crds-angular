using Microsoft.Practices.Unity;
using Microsoft.Practices.Unity.Configuration;
using System.Configuration;
using System.Linq;
using System.Web.Http;
using Crossroads.AsyncJobs.App_Start;
using Crossroads.Web.Common.Configuration;
using Unity.WebApi;


namespace Crossroads.AsyncJobs
{
    public static class UnityConfig
    {
        private readonly static object Lock = new object();

        public static void RegisterComponents()
        {
            lock (Lock)
            {
                // Only initialize once
                if (GlobalConfiguration.Configuration.DependencyResolver != null &&
                    GlobalConfiguration.Configuration.DependencyResolver.GetType() == typeof (UnityDependencyResolver))
                {
                    return;
                }

                var container = new UnityContainer();
                CrossroadsWebCommonConfig.Register(container);

                var unitySections = new[] { "unity", "asyncJobsUnity" };

                foreach (var section in unitySections.Select(sectionName => (UnityConfigurationSection)ConfigurationManager.GetSection(sectionName)))
                {
                    container.LoadConfiguration(section);
                }

                QuartzConfig.Register(container);
                
                GlobalConfiguration.Configuration.DependencyResolver = new UnityDependencyResolver(container);
            }
        }
    }
}
using Quartz;
using System;
using System.Collections.Specialized;
using System.Configuration;
using Microsoft.Practices.Unity;
using Quartz.Unity;

namespace Crossroads.AsyncJobs.App_Start
{
    public class QuartzConfig
    {
        public static NameValueCollection GetConfiguration()
        {
            var quartzConfig = new NameValueCollection((NameValueCollection)ConfigurationManager.GetSection("quartz"));
            ResolveDbConnectionString(quartzConfig);
            return quartzConfig;
        }

        private static void ResolveDbConnectionString(NameValueCollection quartzConfig)
        {
            var connectionString = quartzConfig["quartz.dataSource.default.connectionString"];
            connectionString = connectionString.Replace("%MP_API_DB_USER%", Environment.GetEnvironmentVariable("MP_API_DB_USER"));
            connectionString = connectionString.Replace("%MP_API_DB_PASSWORD%", Environment.GetEnvironmentVariable("MP_API_DB_PASSWORD"));
            quartzConfig.Set("quartz.dataSource.default.connectionString", connectionString);
        }

        public static void Register(UnityContainer container)
        {
            var quartzConfig = GetConfiguration();

            ISchedulerFactory schedulerFactory = new UnitySchedulerFactory(new UnityJobFactory(container));
            ((UnitySchedulerFactory)schedulerFactory).Initialize(quartzConfig);
            
            container.RegisterInstance<ISchedulerFactory>(schedulerFactory);
            
            container.RegisterType<IScheduler>(new InjectionFactory(c => c.Resolve<ISchedulerFactory>().GetScheduler()));
        }
    }
}
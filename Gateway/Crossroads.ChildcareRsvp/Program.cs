using System;
using System.Configuration;
using System.Linq;
using System.Reflection;
using crds_angular.Services;
using Crossroads.Utilities.Services;
using log4net;
using Microsoft.Practices.Unity;
using Microsoft.Practices.Unity.Configuration;
using Crossroads.Web.Common.Configuration;

[assembly: log4net.Config.XmlConfigurator(Watch = true)]

namespace Crossroads.ChildcareRsvp
{
    internal class Program
    {
        private static readonly ILog Log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

        private static void Main()
        {
            var container = new UnityContainer();
            CrossroadsWebCommonConfig.Register(container);

            var unitySections = new[] { "unity" };

            foreach (var section in unitySections.Select(sectionName => (UnityConfigurationSection)ConfigurationManager.GetSection(sectionName)))
            {
                container.LoadConfiguration(section);
            }

            TlsHelper.AllowTls12();
            var exitCode = 0;
            var childcareService = container.Resolve<ChildcareService>();
            try
            {
                Log.Info("Sending notifications for cancellations");
                childcareService.SendChildcareCancellationNotification();                
            }
            catch (Exception ex)
            {
                Log.Error("Childcare Cancellation Notifcation Failed", ex);
                exitCode += 1;
            }

            try
            {
                Log.Info("Sending childcare reminders");
                childcareService.SendChildcareReminders();
            }
            catch (Exception ex)
            {                
                Log.Error("Sending Childcare Reminders failed", ex);
                exitCode += 2;
            }

            Environment.Exit(exitCode);
        }
    }
}
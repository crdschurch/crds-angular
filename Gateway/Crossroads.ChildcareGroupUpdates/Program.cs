using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Reflection;
using crds_angular.App_Start;
using crds_angular.Models.Crossroads.Groups;
using crds_angular.Services;
using Crossroads.Utilities.Services;
using log4net;
using Microsoft.Practices.Unity;
using Microsoft.Practices.Unity.Configuration;
using Crossroads.Web.Common.MinistryPlatform;
using Crossroads.Web.Common.Configuration;
using MinistryPlatform.Translation.Repositories;

[assembly: log4net.Config.XmlConfigurator(Watch = true)]

namespace Crossroads.ChildcareGroupUpdates
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

            var eventService = container.Resolve<EventService>();
            var groupService = container.Resolve<GroupService>();
            var userApiService = container.Resolve<ApiUserRepository>();
            var mpRestRepository = container.Resolve<MinistryPlatformRestRepository>();

            // use childcare grouptype and eventtype
            var groupTypeId = Convert.ToInt32(ConfigurationManager.AppSettings["ChildcareGroupType"]);
            var eventTypeId = Convert.ToInt32(ConfigurationManager.AppSettings["ChildcareEventType"]);
            var defaultMinistryId = Convert.ToInt32(ConfigurationManager.AppSettings["ChildcareDefaultMinistryId"]);

            var configSettingsPageId = Convert.ToInt32(ConfigurationManager.AppSettings["ConfigurationSettingsPageId"]);

            try
            {
                ////////////////////////////////////////////////////////////////////////
                // Update the childcare events that are properly set up in a series
                ////////////////////////////////////////////////////////////////////////
                Log.Info("Updating Childcare events in series.");
                var apiToken = userApiService.GetDefaultApiClientToken();
                AutoMapperConfig.RegisterMappings();

                var parms = new Dictionary<string, object>()
                {
                    {"@Group_Type", groupTypeId},
                    {"@Event_type", eventTypeId}
                };

                //Call Andy's stored proc and loop through
                var results = mpRestRepository.UsingAuthenticationToken(apiToken).GetFromStoredProc<MpEventsMissingGroups>("api_crds_MissingChildcareGroup", parms);
                var eventList = results.FirstOrDefault() ?? new List<MpEventsMissingGroups>();
                Log.Info("Updating " + eventList.Count.ToString() + " series events.");
                foreach (var item in eventList)
                {
                    var groupdto = groupService.GetGroupDetails(item.GroupId);

                    //change the date
                    var mpevent = eventService.GetEvent(item.EventId);
                    groupdto.StartDate = mpevent.EventStartDate;
                    groupdto.Participants?.Clear();
                    groupdto.Events?.Clear();
                    groupdto.MeetingDayId = null;
                    groupdto.MeetingFrequencyID = null;
                    //change the dates
                    
                    var newgroupdto = groupService.CreateGroup(groupdto);

                    //link the new group to the event
                    eventService.AddEventGroup(item.EventId, newgroupdto.GroupId);
                }

                ////////////////////////////////////////////////////////////////////////
                // Update the childcare events that are orphans (Use defaults)
                ////////////////////////////////////////////////////////////////////////

                //get default values 10,25,100
                var maxAgeObject = MinistryPlatformService.GetRecordsDict(configSettingsPageId, apiToken, ",ChildcareMaxAge", string.Empty).FirstOrDefault()?["Value"];
                var defaultMaximumAge = maxAgeObject != null ? Convert.ToInt32(maxAgeObject) : 10;

                var minParticipantsObject = MinistryPlatformService.GetRecordsDict(configSettingsPageId, apiToken, ",ChildcareMinParticipants", string.Empty).FirstOrDefault()?["Value"]; 
                var defaultMinimumParticipants = minParticipantsObject != null ? Convert.ToInt32(minParticipantsObject) : 25;

                var targetSizeObject = MinistryPlatformService.GetRecordsDict(configSettingsPageId, apiToken, ",ChildcareTargetSize", string.Empty).FirstOrDefault()?["Value"]; 
                var defaultTargetSize = targetSizeObject != null ? Convert.ToInt32(targetSizeObject) : 100;

                Log.Info("Updating orphan Childcare events.");
                var orphanresults = mpRestRepository.UsingAuthenticationToken(apiToken).GetFromStoredProc<MpOrphanEventsMissingGroups>("api_crds_GetOrphanChildcareEvents", new Dictionary<string, object>());
                var orphaneventList = orphanresults.FirstOrDefault() ?? new List<MpOrphanEventsMissingGroups>();
                Log.Info("Updating " + eventList.Count.ToString() + " orphan events.");
                foreach (var item in orphaneventList)
                {
                    var groupdto = new GroupDTO();
                    var mpevent = eventService.GetEvent(item.EventId);
                    groupdto.StartDate = mpevent.EventStartDate;
                    groupdto.Participants?.Clear();
                    groupdto.Events?.Clear();
                    groupdto.MeetingDayId = null;
                    groupdto.MeetingFrequencyID = null;
                    groupdto.CongregationId = mpevent.CongregationId;
                    groupdto.Congregation = mpevent.Congregation;
                    groupdto.ContactId = mpevent.PrimaryContact.ContactId;
                    groupdto.MaximumAge = defaultMaximumAge;
                    groupdto.MinimumParticipants = defaultMinimumParticipants;
                    groupdto.TargetSize = defaultTargetSize;
                    groupdto.GroupName = "__childcaregroup";
                    groupdto.GroupTypeId = groupTypeId;
                    groupdto.MinistryId = defaultMinistryId;

                    var newgroupdto = groupService.CreateGroup(groupdto);

                    //link the new group to the event
                    eventService.AddEventGroup(item.EventId, newgroupdto.GroupId);
                }

                Log.Info("Childcare Group update Complete.");
            }
            catch (Exception ex)
            {
                Log.Error("Childcare Group Update Notifcation Failed", ex);
                Environment.Exit(9999);
            }
            Environment.Exit(0);
        }
    }
}
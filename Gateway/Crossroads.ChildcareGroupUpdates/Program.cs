﻿using System;
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
using MinistryPlatform.Translation.Repositories;

[assembly: log4net.Config.XmlConfigurator(Watch = true)]

namespace Crossroads.ChildcareGroupUpdates
{
    internal class Program
    {
        private static readonly ILog Log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

        private static void Main()
        {
            var section = (UnityConfigurationSection) ConfigurationManager.GetSection("unity");
            var container = new UnityContainer();
            section.Configure(container);

            TlsHelper.AllowTls12();

            var eventService = container.Resolve<EventService>();
            var groupService = container.Resolve<GroupService>();
            var userApiService = container.Resolve<ApiUserRepository>();
            var mpRestRepository = container.Resolve<MinistryPlatformRestRepository>();

            // use childcare grouptype and eventtype
            const int groupTypeId = 27;
            const int eventTypeId = 243;
            const int defaultMinistryId = 2;

            try
            {
                ////////////////////////////////////////////////////////////////////////
                // Update the childcare events that are properly set up in a series
                ////////////////////////////////////////////////////////////////////////
                Log.Info("Updating Childcare events in series.");
                var apiToken = userApiService.GetToken();
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
                    eventService.AddEventGroup(item.EventId, newgroupdto.GroupId, apiToken);
                }

                ////////////////////////////////////////////////////////////////////////
                // Update the childcare events that are orphans (Use defaults)
                ////////////////////////////////////////////////////////////////////////

                //get default values 10,25,100
                var maxAgeObject = MinistryPlatformService.GetRecordsDict(420, apiToken, ",ChildcareMaxAge", "").FirstOrDefault()?["Value"];
                int defaultMaximumAge = maxAgeObject != null ? Convert.ToInt32(maxAgeObject) : 10;

                var minParticipantsObject = MinistryPlatformService.GetRecordsDict(420, apiToken, ",ChildcareMinParticipants", "").FirstOrDefault()?["Value"]; 
                int defaultMinimumParticipants = minParticipantsObject != null ? Convert.ToInt32(minParticipantsObject) : 25;

                var targetSizeObject = MinistryPlatformService.GetRecordsDict(420, apiToken, ",ChildcareTargetSize", "").FirstOrDefault()?["Value"]; 
                int defaultTargetSize = targetSizeObject != null ? Convert.ToInt32(targetSizeObject) : 100;

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
                    groupdto.GroupName = "__childcare";
                    groupdto.GroupTypeId = groupTypeId;
                    groupdto.MinistryId = defaultMinistryId;

                    var newgroupdto = groupService.CreateGroup(groupdto);

                    //link the new group to the event
                    eventService.AddEventGroup(item.EventId, newgroupdto.GroupId, apiToken);
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
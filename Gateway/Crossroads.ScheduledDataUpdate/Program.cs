using System;
using System.Configuration;
using System.Reflection;
using Amazon.CloudSearchDomain.Model;
using crds_angular.App_Start;
using crds_angular.Services.Interfaces;
using log4net;
using Microsoft.Practices.Unity;
using Microsoft.Practices.Unity.Configuration;
using CommandLine;
using Crossroads.Utilities.Services;
using Crossroads.Web.Common.Configuration;
using System.Threading;
using System.Collections.Generic;
using System.IO;

namespace Crossroads.ScheduledDataUpdate
{
    public class Program
    {
        private static readonly ILog Log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

        private static void Main(string [] args)
        {
            log4net.Config.XmlConfigurator.Configure();

            TlsHelper.AllowTls12();

            var container = new UnityContainer();
            CrossroadsWebCommonConfig.Register(container);

            var unitySections = new[] { "unity", "scheduledDataUnity" };
            foreach (var sectionName in unitySections)
            {
                var section = (UnityConfigurationSection)ConfigurationManager.GetSection(sectionName);
                container.LoadConfiguration(section);
            }

            var argString = args == null ? string.Empty : string.Join(" ", args);
            Log.Info($"Starting ScheduledDataUpdate with arguments: {argString}");

            var program = container.Resolve<Program>();
            var exitCode = program.Run(args);

            Log.Info($"Completed Scheduled Data Update, exit code {exitCode}");

            Environment.Exit(exitCode);
        }

        private readonly ITaskService _taskService;
        private readonly IGroupToolService _groupToolService;
        private readonly IAwsCloudsearchService _awsService;
        private readonly ICorkboardService _corkboardService;
        private readonly IGroupService _groupService;
        private readonly IFinderService _finderService;
        private readonly IAddressService _addressService;

        public Program(ITaskService taskService, 
                       IGroupToolService groupToolService, 
                       IAwsCloudsearchService awsService, 
                       ICorkboardService corkboardService,
                       IGroupService groupService,
                       IFinderService finderService,
                       IAddressService addressService)
        {
            _taskService = taskService;
            _groupToolService = groupToolService;
            _awsService = awsService;
            _corkboardService = corkboardService;
            _groupService = groupService;
            _finderService = finderService;
            _addressService = addressService;
        }

        public int Run(string[] args)
        {
            var options = new Options();
            if (!Parser.Default.ParseArguments(args, options))
            {
                Log.Error("Invalid Arguments.");
                Log.Error(options.GetUsage());
                return 1;
            }
            
            if (options.HelpMode)
            {
                Log.Error(options.GetUsage());
                return 0;
            }

            var exitCode = 0;
            var modeSelected = false;
            AutoMapperConfig.RegisterMappings();

            if (options.UpdateAddressLatLong)
            {
                modeSelected = true;
                try
                {
                    // get address ids from the file. If no file then geocode 100 addresses with no lat/long
                    var filename = "addressids.txt";
                    var addressids = new List<int>();
                    if (File.Exists(filename))
                    {
                        // load addressids from file
                        List<string> list = new List<string>(File.ReadAllLines(filename));
                        foreach (string item in list)
                        {
                            int id;
                            if (int.TryParse(item, out id))
                            {
                                addressids.Add(id);
                            }
                        }
                    }
                    else
                    {
                        // geocode any participants on the map that have no lat/long
                        addressids = _finderService.GetAddressIdsForMapParticipantWithNoGeoCode();

                        // if we dont have any map participants that need geocoding then lets geocode other addresses
                        if (addressids.Count == 0)
                        {
                            addressids = _finderService.GetAddressIdsWithNoGeoCode();
                        }
                    }

                    foreach(int addressid in addressids)
                    {
                        _addressService.SetGeoCoordinates(addressid);
                    }
                }
                catch (Exception ex)
                {
                    Log.Error("UpdateAddressLatLong failed.", ex);
                    exitCode = 9999;
                }
            }


            if (options.ConnectMapListenForUpdates)
            {
                modeSelected = true;
                try
                {
                    const int fiveminutes = (60 * 5) * 1000;
                    Log.Info("Starting Connect Map Update to Firestore");
                    var conStr = ConfigurationManager.ConnectionStrings["MessageQueueDBAccess"].ToString();
                    conStr = conStr.Replace("%MP_API_DB_QUEUE_USER%", Environment.GetEnvironmentVariable("MP_API_DB_QUEUE_USER"));
                    conStr = conStr.Replace("%MP_API_DB_QUEUE_PASSWORD%", Environment.GetEnvironmentVariable("MP_API_DB_QUEUE_PASSWORD"));
                    string queueName = "MPAppQueue";
                    string query = "select AuditID, Participant_ID, ShowOnMap, processed from dbo.cr_MapAudit where processed=0 ";
                    var watcher = new DbWatcher(conStr, queueName, query, _finderService);
                    watcher.Start();
                    Thread.Sleep(fiveminutes);
                    watcher.Stop();
                    Log.Info("Finished Connect Map Update to Firestore successfully");
                }
                catch (Exception ex)
                {
                    Log.Error("Connect Map Update to Firestore failed.", ex);
                    exitCode = 9999;
                }
            }

            if (options.AutoCompleteTasksMode)
            {
                modeSelected = true;
                try
                {
                    Log.Info("Starting Auto Complete Tasks");

                    _taskService.AutoCompleteTasks();

                    Log.Info("Finished Auto Complete Tasks successfully");
                }
                catch (Exception ex)
                {
                    Log.Error("Auto Complete Tasks failed.", ex);
                    exitCode = 9999;
                }
            }

            if (options.RoomReservationRejectionNotification)
            {
                modeSelected = true;
                try
                {
                    Log.Info("Room Reservation Rejection Notification");

                    _taskService.RoomReservationRejectionNotification();

                    Log.Info("Room Reservation Rejection Notification successfully");
                }
                catch (Exception ex)
                {
                    Log.Error("Room Reservation Rejection Notification failed.", ex);
                    exitCode = 9999;
                }
            }

            if (options.SmallGroupInquiryReminderMode)
            {
                modeSelected = true;
                try
                {
                    Log.Info("Starting Small Group Inquiry Reminder");

                    _groupToolService.SendSmallGroupPendingInquiryReminderEmails();

                    Log.Info("Finished Small Group Inquiry Reminder successfully");
                }
                catch (Exception ex)
                {
                    Log.Error("Small Group Inquiry Reminder failed.", ex);
                    exitCode = 9999;
                }
            }

            if (options.ConnectAwsRefreshMode)
            {
                modeSelected = true;
                try
                {
                    Log.Info("Starting Connect AWS Refresh");
                    AutoMapperConfig.RegisterMappings();
                    _awsService.DeleteAllConnectRecordsInAwsCloudsearch();
                    _awsService.UploadAllConnectRecordsToAwsCloudsearch();

                    Log.Info("Finished Connect AWS Refresh successfully");
                }
                catch (DocumentServiceException ex)
                {
                    Log.Error("Connect AWS error, nothing to delete, empty cloud. Still go ahead and refresh", ex);
                    _awsService.UploadAllConnectRecordsToAwsCloudsearch();
                }
                catch (Exception ex)
                {
                    Log.Error("Connect AWS Refresh failed.", ex);
                    exitCode = 9999;
                }
            }

            if (options.CorkboardAwsRefreshMode)
            {
                modeSelected = true;
                try
                {
                    Log.Info("Starting Corkboard AWS Refresh");
                    _corkboardService.SyncPosts();                    

                    Log.Info("Finished Corkboard AWS Refresh successfully");
                }
                catch (Exception ex)
                {
                    Log.Error("Corkboard AWS Refresh failed.", ex);
                    exitCode = 9999;
                }
            }

            if (options.HuddleStatusParticipantUpdateMode)
            {
                modeSelected = true;
                try
                {
                    Log.Info("Starting Huddle Status Participant Update");
                    _groupService.UpdateHuddleGroupParticipantStatus();
                    Log.Info("Finished Huddle Status Participant Update successfully");
                }
                catch (Exception ex)
                {
                    Log.Error("Huddle Status Participant Update failed.", ex);
                    exitCode = 9999;
                }
            }

            if (options.ArchivePendingGroupInquiriesMode)
            {
                modeSelected = true;
                try
                {
                    Log.Info("Starting group inquiry archival process...");
                    _groupToolService.ArchivePendingGroupInquiriesOlderThan90Days();

                    Log.Info("Groups archival stored proc successful...");
                }
                catch (Exception ex)
                {
                    Log.Error("Group archival stored proc failed.", ex);
                    exitCode = 9999;
                }
            }

            if (!modeSelected)
            {
                Log.Error(options.GetUsage());
                return 0;
            }

            return exitCode;
        }        
    }
}

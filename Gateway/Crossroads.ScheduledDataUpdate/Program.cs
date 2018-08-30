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

        public Program(ITaskService taskService, 
                       IGroupToolService groupToolService, 
                       IAwsCloudsearchService awsService, 
                       ICorkboardService corkboardService,
                       IGroupService groupService,
                       IFinderService finderService)
        {
            _taskService = taskService;
            _groupToolService = groupToolService;
            _awsService = awsService;
            _corkboardService = corkboardService;
            _groupService = groupService;
            _finderService = finderService;
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

            if (options.ConnectMapListenForUpdates)
            {
                modeSelected = true;
                try
                {
                    const int almost24hours = 1430 * 60 * 1000;
                    Log.Info("Starting Connect Map Update to Firestore");
                    string conStr = ConfigurationManager.ConnectionStrings["MessageQueueDBAccess"].ToString();
                    var user = "MPBrokerUser";
                    var pass = "Aw@!ted2015";
                    conStr = $"Data Source=MP-Int-DB.centralus.cloudapp.azure.com;Initial Catalog=MinistryPlatform;Persist Security Info=True;User ID={user};Password={pass}";
                    string queueName = "MPAppQueue";
                    string query = "select AuditID, Participant_ID, ShowOnMap, processed from dbo.cr_MapAudit where processed=0 ";
                    var watcher = new DbWatcher(conStr, queueName, query, _finderService);
                    watcher.Start();
                    Thread.Sleep(almost24hours);
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

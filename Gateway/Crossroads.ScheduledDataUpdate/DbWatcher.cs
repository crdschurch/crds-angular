using crds_angular.Services.Interfaces;
using System.Reflection;
using log4net;
using System;
using System.Data.SqlClient;
namespace Crossroads.ScheduledDataUpdate
{
    class DbWatcher
    {
        private static readonly ILog Log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

        private readonly string connectionString;
        private readonly string sqlQueue;
        private readonly string listenerQuery;
        private SqlDependency dependency;
        private readonly IFirestoreUpdateService _firestoreUpdateService;
        public DbWatcher(string connectionString, string sqlQueue, string listenerQuery, IFirestoreUpdateService firestoreUpdateService)
        {
            this.connectionString = connectionString;
            this.sqlQueue = sqlQueue;
            this.listenerQuery = listenerQuery;
            this.dependency = null;
            _firestoreUpdateService = firestoreUpdateService;
        }
        public void Start()
        {
            SqlDependency.Start(connectionString, sqlQueue);
            ListenForChanges("Insert");
        }
        public void Stop()
        {
            SqlDependency.Stop(this.connectionString, sqlQueue);
        }
        private void ListenForChanges(string dbAction)
        {
            //Remove existing dependency, if necessary
            if (dependency != null)
            {
                dependency.OnChange -= OnDependencyChange;
                dependency = null;

            }
            //Perform this action when SQL notifies of a change
            if (dbAction == "Insert")
            {
                PerformAction();
            }
            SqlConnection connection = new SqlConnection(connectionString);
            connection.Open();
            SqlCommand command = new SqlCommand(listenerQuery, connection);
            dependency = new SqlDependency(command);
            // Subscribe to the SqlDependency event.
            dependency.OnChange += new OnChangeEventHandler(OnDependencyChange);
            SqlDependency.Start(connectionString);
            command.ExecuteReader();
            connection.Close();
        }
        private void OnDependencyChange(Object o, SqlNotificationEventArgs args)
        {
            WriteToConsoleAndLog(System.Environment.NewLine + $"Args: Source={args.Source}, Info={args.Info}, Type={args.Type.ToString()}");
            if (((args.Source.ToString() == "Data") || (args.Source.ToString() == "Timeout")) && args.Info.ToString() == "Insert")
            {
                WriteToConsoleAndLog(System.Environment.NewLine + $"Refreshing data due to {args.Source}");
            }
            else
            {
                WriteToConsoleAndLog(System.Environment.NewLine + $"Data not refreshed due to unexpected SqlNotificationEventArgs: Source={args.Source}, Info={args.Info}, Type={args.Type.ToString()}");
            }
            ListenForChanges(args.Info.ToString());
        }
        private void PerformAction()
        {
            WriteToConsoleAndLog("Performing action - like running the batch");
            SyncPinsToFirestore();
        }
        private void SyncPinsToFirestore()
        {
            WriteToConsoleAndLog("Starting Sync at " + DateTime.Now.ToLongTimeString());
            _firestoreUpdateService.ProcessMapAuditRecords().Wait();
            WriteToConsoleAndLog("Completed Sync at " + DateTime.Now.ToLongTimeString());
        }
        private void WriteToConsoleAndLog(string message)
        {
            Console.WriteLine(message);
            Log.Info(message);
        }
    }
}
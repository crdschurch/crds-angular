using crds_angular.Services.Interfaces;
using System;
using System.Data.SqlClient;
namespace Crossroads.ScheduledDataUpdate
{
    class DbWatcher
    {
        private readonly string connectionString;
        private readonly string sqlQueue;
        private readonly string listenerQuery;
        private SqlDependency dependency;
        private readonly IFinderService _finderService;
        public DbWatcher(string connectionString, string sqlQueue, string listenerQuery, IFinderService finderService)
        {
            this.connectionString = connectionString;
            this.sqlQueue = sqlQueue;
            this.listenerQuery = listenerQuery;
            this.dependency = null;
            _finderService = finderService;
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
            Console.WriteLine(System.Environment.NewLine + "Args: Source={0}, Info={1}, Type={2}", args.Source, args.Info, args.Type.ToString());
            if (((args.Source.ToString() == "Data") || (args.Source.ToString() == "Timeout")) && args.Info.ToString() == "Insert")
            {
                Console.WriteLine(System.Environment.NewLine + "Refreshing data due to {0}", args.Source);
            }
            else
            {
                Console.WriteLine(System.Environment.NewLine + "Data not refreshed due to unexpected SqlNotificationEventArgs: Source={0}, Info={1}, Type={2}", args.Source, args.Info, args.Type.ToString());
            }
            ListenForChanges(args.Info.ToString());
        }
        private void PerformAction()
        {
            Console.WriteLine("Performing action - like running the batch");
            SyncPinsToFirestore();
        }
        private void SyncPinsToFirestore()
        {
            Console.WriteLine("Starting Sync at " + DateTime.Now.ToLongTimeString());
            _finderService.ProcessMapAuditRecords().Wait();
            Console.WriteLine("Completed Sync at " + DateTime.Now.ToLongTimeString());
        }
    }
}
using System;
using System.Web.Hosting;
using Quartz;

namespace Crossroads.AsyncJobs.Application
{
    public class JobProcessor : IRegisteredObject
    {
        private readonly IQueueProcessor[] _queueProcessors;
        private readonly IScheduler _scheduler;
        public bool IsRunning { get; private set; }

        private readonly object _lockObject = new object();


        public JobProcessor(IQueueProcessor[] processors, IScheduler scheduler)
        {
            _queueProcessors = processors;
            _scheduler = scheduler;
        }

        public void Start()
        {
            lock (_lockObject)
            {
                Console.WriteLine("Starting job processor");
                if (IsRunning)
                {
                    return;
                }
                IsRunning = true;

                HostingEnvironment.RegisterObject(this);

                foreach (var p in _queueProcessors)
                {
                    p.Start();
                }

               _scheduler.Start();
            }
        }

        public void Stop()
        {
            lock (_lockObject)
            {
                if (!IsRunning)
                {
                    return;
                }
                IsRunning = false;

                HostingEnvironment.UnregisterObject(this);

                foreach (var p in _queueProcessors)
                {
                    p.Pause();
                }

                _scheduler.Shutdown();
            }
        }

        public void Stop(bool immediate)
        {
            Stop();
        }
    }
}
using System;
using crds_angular.Models.Crossroads;
using Crossroads.AsyncJobs.Interfaces;
using log4net;
using Quartz;
using Quartz.Impl;

namespace Crossroads.AsyncJobs.Processors
{
    public class ScheduledJobProcessor : IJobExecutor<ScheduledJob>
    {
        private ILog _logger = LogManager.GetLogger(typeof(ScheduledJobProcessor));
        private readonly IScheduler _scheduler;
        public ScheduledJobProcessor(IScheduler scheduler)
        {
            _scheduler = scheduler;
        }

        public void Execute(Models.JobDetails<ScheduledJob> details)
        {
            ScheduledJob scheduledJob = details.Data;

            string jobName = scheduledJob.JobType.Name + ":" + Guid.NewGuid() + "@" + scheduledJob.StartDateTime;

            _logger.Info("Scheduling Job: " + jobName + " Scheduled Job queue initiated");

            IJobDetail job = new JobDetailImpl(jobName, scheduledJob.JobType);

            job.JobDataMap.PutAll(scheduledJob.Dto);

            ITrigger trigger = TriggerBuilder.Create()
                .StartAt(scheduledJob.StartDateTime)
                .Build();
            _scheduler.ScheduleJob(job, trigger);

        }
    }
}
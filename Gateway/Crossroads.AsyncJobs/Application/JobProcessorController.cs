using System.Web.Http;

namespace Crossroads.AsyncJobs.Application
{
    /// <summary>
    /// Temporary class here just to bootstrap webapi, to help troubleshoot if auto-start is not working when deployed
    /// </summary>
    public class JobProcessorController : ApiController
    {
        private readonly JobProcessor _jobProcessor;

        public JobProcessorController(JobProcessor jobProcessor)
        {
            _jobProcessor = jobProcessor;
        }

        [Route("api/jobProcessor/{start}")]
        public string Get(bool start)
        {
            if (start)
            {
                _jobProcessor.Start();
            }
            else
            {
                _jobProcessor.Stop();
            }

            return ("JobProcessor Running? " + _jobProcessor.IsRunning);
        }

        [Route("api/jobProcessor")]
        public string Get()
        {
            return ("JobProcessor Running? " + _jobProcessor.IsRunning);
        }
    }
}

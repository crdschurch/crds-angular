using Common.Logging;
using crds_angular.Models.Crossroads.Trip;
using crds_angular.Services.Interfaces;
using Crossroads.AsyncJobs.Interfaces;
using Crossroads.AsyncJobs.Models;

namespace Crossroads.AsyncJobs.Processors
{
    public class TripEventProcessor : IJobExecutor<TripApplicationDto>
    {
        private readonly ITripService _tripService;
        private ILog _logger = LogManager.GetLogger(typeof(TripEventProcessor));

        public TripEventProcessor(ITripService tripService)
        {
            _tripService = tripService;
        }

        public void Execute(JobDetails<TripApplicationDto> tripData)
        {
            _tripService.SaveApplication(tripData.Data);
            _logger.Info("TripEventProcessor Initiated");
        }
    }
}
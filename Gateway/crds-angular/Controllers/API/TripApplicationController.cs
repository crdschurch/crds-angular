using System;
using System.Linq;
using System.Messaging;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web.Http;
using System.Web.Http.Description;
using crds_angular.Exceptions;
using crds_angular.Exceptions.Models;
using crds_angular.Models.Crossroads.Trip;
using crds_angular.Models.Json;
using crds_angular.Security;
using crds_angular.Services.Interfaces;
using Crossroads.Utilities.Messaging.Interfaces;
using Crossroads.ApiVersioning;
using Crossroads.Web.Common.Configuration;
using Crossroads.Web.Common.Security;
using Newtonsoft.Json;
using log4net;

namespace crds_angular.Controllers.API
{
    public class TripApplicationController : MPAuth
    {
        private readonly ITripService _tripService;
        private readonly IMessageFactory _messageFactory;
        private readonly MessageQueue _eventQueue;
        private readonly ILog _logger;

        public TripApplicationController(IAuthTokenExpiryService authTokenExpiryService, 
                                         ITripService tripService, 
                                         IConfigurationWrapper configuration, 
                                         IMessageFactory messageFactory, 
                                         IMessageQueueFactory messageQueueFactory, 
                                         IUserImpersonationService userImpersonationService, 
                                         IAuthenticationRepository authenticationRepository) 
          : base(authTokenExpiryService, userImpersonationService, authenticationRepository)
        {
            _tripService = tripService;
            _messageFactory = messageFactory;

            var eventQueueName = configuration.GetConfigValue("TripApplicationEventQueue");
            _eventQueue = messageQueueFactory.CreateQueue(eventQueueName, QueueAccessMode.Send);
            _messageFactory = messageFactory;
        }

        [ResponseType(typeof(TripParticipantPledgeDto))]
        [VersionedRoute(template: "trip-application/{contactId}/{campaignId}", minimumVersion: "1.0.0")]
        [Route("trip-application/{contactId}/{campaignId}")]
        [HttpGet]
        public IHttpActionResult GetCampaignInfo(int contactId, int campaignId)
        {
            return Authorized(token =>
            {
                try
                {
                    var campaignInfo = _tripService.GetCampaignPledgeInfo(contactId, campaignId);
                    return Ok(campaignInfo);
                }
                catch (Exception exception)
                {
                    var apiError = new ApiErrorDto("GetCampaignInfo", exception);
                    throw new HttpResponseException(apiError.HttpResponseMessage);
                }
            });
        }

        [VersionedRoute(template: "trip-application", minimumVersion: "1.0.0")]
        [Route("trip-application")]
        [HttpPost]
        public IHttpActionResult Save([FromBody] TripApplicationDto dto)
        {
            if (!ModelState.IsValid)
            {              
                var errors = ModelState.Values.SelectMany(val => val.Errors).Aggregate("", (current, err) => current + err.Exception.Message);
                var dataError = new ApiErrorDto("Save Trip Application Data Invalid", new InvalidOperationException("Invalid Save Data" + errors));
                throw new HttpResponseException(dataError.HttpResponseMessage);
            }

            TripApplicationResponseDto response;
            try
            {               
                var participantPledgeInfo = _tripService.CreateTripParticipant(dto.ContactId, dto.PledgeCampaignId);
                var message = _messageFactory.CreateMessage(dto);
                _eventQueue.Send(message, MessageQueueTransactionType.None);
                response = new TripApplicationResponseDto
                {
                    Message = "Queued event for asynchronous processing",
                    DepositAmount = participantPledgeInfo.Deposit,
                    DonorId = participantPledgeInfo.DonorId,
                    ProgramId = participantPledgeInfo.ProgramId,
                    ProgramName = participantPledgeInfo.ProgramName
                };

                new Task(() => { _tripService.SendTripIsFullMessage(dto.PledgeCampaignId); }).Start();

            }
            catch (TripFullException e)
            {
                var json = JsonConvert.SerializeObject(e.Message, Formatting.None);
                var message = new HttpResponseMessage(HttpStatusCode.Conflict);
                message.Content = new StringContent(json);
                throw new HttpResponseException(message);
            }
            catch (Exception e)
            {
                const string msg = "Unexpected error processing Trip Application Save";
                var responseDto = new TripApplicationResponseDto()
                {
                    Exception = new ApplicationException(msg, e),
                    Message = msg
                };
                return (RestHttpActionResult<TripApplicationResponseDto>.ServerError(responseDto));
            }
            return RestHttpActionResult<TripApplicationResponseDto>.Ok(response);
        }

    }
}

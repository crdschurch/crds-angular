using System;
using System.Messaging;
using log4net;
using System.Web.Http;
using crds_angular.Services.Interfaces;
using crds_angular.Models.Crossroads.Stewardship;
using crds_angular.Models.Json;
using crds_angular.Services;
using Crossroads.Utilities.Messaging.Interfaces;
using Crossroads.ApiVersioning;
using Crossroads.ClientApiKeys;
using Crossroads.Web.Common.Configuration;
using MinistryPlatform.Translation.Exceptions;

namespace crds_angular.Controllers.API
{
    // Ignore Client API Keys for all methods, since Stripe cannot send custom headers in webhooks
    [IgnoreClientApiKey]
    public class StripeEventController : ApiController
    {
        private readonly ILog _logger = LogManager.GetLogger(typeof(StripeEventController));
        private readonly bool _liveMode;
        private readonly MessageQueue _eventQueue;
        private readonly IMessageFactory _messageFactory;
        private readonly bool _asynchronous;
        private readonly IStripeEventService _stripeEventService;

        public StripeEventController(IConfigurationWrapper configuration, IStripeEventService stripeEventService = null,
            IMessageQueueFactory messageQueueFactory = null, IMessageFactory messageFactory = null)
        {
            var b = configuration.GetConfigValue("StripeWebhookLiveMode");
            _liveMode = b != null && bool.Parse(b);

            b = configuration.GetConfigValue("StripeWebhookAsynchronousProcessingMode");
            _asynchronous = b != null && bool.Parse(b);
            if (_asynchronous && messageQueueFactory != null)
            {
                var eventQueueName = configuration.GetConfigValue("StripeWebhookEventQueueName");
                _eventQueue = messageQueueFactory.CreateQueue(eventQueueName, QueueAccessMode.Send);
                _messageFactory = messageFactory;
            }
            else
            {
                _stripeEventService = stripeEventService;
            }
        }

        [VersionedRoute(template: "stripe-event", minimumVersion: "1.0.0")]
        [Route("stripe-event")]
        [HttpPost]
        public IHttpActionResult ProcessStripeEvent([FromBody]StripeEvent stripeEvent)
        {
            if (stripeEvent == null || !ModelState.IsValid)
            {
                if (_logger.IsDebugEnabled)
                {
                    _logger.Debug("Received invalid Stripe event " + stripeEvent);
                }
                return (BadRequest(ModelState));
            }

            _logger.Debug("Received Stripe Event " + stripeEvent.Type);
            if (_liveMode != stripeEvent.LiveMode)
            {
                _logger.Debug("Dropping Stripe Event " + stripeEvent.Type + " because LiveMode was " + stripeEvent.LiveMode);
                return (Ok());
            }

            StripeEventResponseDTO response;
            try
            {
                if (_asynchronous)
                {
                    _logger.Info("Enqueueing Stripe event " + stripeEvent.Type + " because AsynchronousProcessingMode was true");
                    var message = _messageFactory.CreateMessage(stripeEvent);
                    _eventQueue.Send(message, MessageQueueTransactionType.None);
                    response = new StripeEventResponseDTO
                    {
                        Message = "Queued event for asynchronous processing"
                    };
                }
                else
                {
                    _logger.Info("Processing Stripe event " + stripeEvent.Type + " because AsynchronousProcessingMode was false");
                    response = _stripeEventService.ProcessStripeEvent(stripeEvent);
                }

            }
            catch (Exception e)
            {
                var msg = "Unexpected error processing Stripe Event " + stripeEvent.Type;

                if (e is DonationNotFoundException)
                {
                    // Sometimes we receive a webhook callback before the donation has been
                    // added to the database.  This is a known issue, so just do minimal
                    // logging without a full stack trace.
                    _logger.Error($"ProcessStripeEvent: Donation not found processing {stripeEvent.Type}: {e.Message}");
                }
                else
                {
                    _logger.Error(msg, e);
                }

                var responseDto = new StripeEventResponseDTO()
                {
                    Exception = new ApplicationException(msg, e),
                    Message = msg
                };
                return (RestHttpActionResult<StripeEventResponseDTO>.ServerError(responseDto));
            }

            return (response == null ? Ok() : (IHttpActionResult)RestHttpActionResult<StripeEventResponseDTO>.Ok(response));
        }
    }
}

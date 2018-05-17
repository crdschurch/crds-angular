using System;
using System.Collections.Generic;
using System.Linq;
using System.Messaging;
using System.Web.Http;
using crds_angular.Security;
using crds_angular.Services.Interfaces;
using crds_angular.Models.Crossroads;
using crds_angular.Exceptions.Models;
using crds_angular.Models;
using crds_angular.Processors;
using Crossroads.Utilities.Messaging.Interfaces;
using Crossroads.Utilities.Messaging;
using log4net;
using Crossroads.Utilities.Interfaces;
using Crossroads.ApiVersioning;
using Crossroads.Web.Common;
using Crossroads.Web.Common.Configuration;
using Crossroads.Web.Common.Security;

namespace crds_angular.Controllers.API
{
    public class TextCommunicationController : MPAuth
    {
        private readonly ILog _logger = LogManager.GetLogger(typeof(TextCommunicationController));

        private readonly IMessageQueue _messageQueue;
        private readonly IMessageFactory _messageFactory;
        private readonly ITextCommunicationService _textCommunicationService;
        private readonly string _eventQueueName;
        private readonly int _streamReminderTemplateId;

        public TextCommunicationController(IAuthTokenExpiryService authTokenExpiryService, 
                                           ITextCommunicationService textCommunicationService, 
                                           IConfigurationWrapper configurationWrapper, 
                                           IUserImpersonationService userImpersonationService,
                                           IAuthenticationRepository authenticationRepository,
                                           IMessageQueueFactory messageQueueFactory = null, 
                                           IMessageFactory messageFactory = null, 
                                           IMessageQueue messageQueue = null) 
            : base(authTokenExpiryService, userImpersonationService, authenticationRepository)
        {
            _textCommunicationService = textCommunicationService;
            _eventQueueName = configurationWrapper.GetConfigValue("ScheduledJobsQueueName");
            _streamReminderTemplateId = configurationWrapper.GetConfigIntValue("StreamReminderTemplate");
            _messageQueue = messageQueue;
            _messageQueue.CreateQueue(_eventQueueName, QueueAccessMode.Send);
            _messageFactory = messageFactory;
        }

        /// <summary>
        /// Schedule a text message to a specific contactId/number at a specific time
        /// </summary>
        [VersionedRoute(template: "send-text-message-reminder", minimumVersion: "1.0.0")]
        [Route("sendTextMessageReminder")]
        public IHttpActionResult PostReminder(TextCommunicationDto textCommunicationData)
        {
            if (textCommunicationData == null || !ModelState.IsValid)
            {
                if (_logger.IsDebugEnabled)
                {
                    _logger.Debug("Received invalid schedule text event " + textCommunicationData);
                }
                return (BadRequest(ModelState));
            }

            _logger.Debug("Received schedule text request " + textCommunicationData);

            try
            {
                // Hardcode template ID for now to mitigate misuse risk
                textCommunicationData.TemplateId = _streamReminderTemplateId;

                ScheduledJob scheduledJob = new ScheduledJob();
                scheduledJob.StartDateTime = textCommunicationData.StartDate.Value;
                scheduledJob.JobType = typeof(SendTextMessageJob);
                scheduledJob.Dto = new Dictionary<string, object>()
                {
                    {"TemplateId", textCommunicationData.TemplateId},
                    {"MergeData", textCommunicationData.MergeData},
                    {"ToPhoneNumber", textCommunicationData.ToPhoneNumber},
                    {"StartDate", textCommunicationData.StartDate}
                };

                var message = _messageFactory.CreateMessage(scheduledJob);

                if (!_messageQueue.Exists(_eventQueueName))
                {
                    _messageQueue.CreateQueue(_eventQueueName, QueueAccessMode.Send);
                }

                _messageQueue.Send(message, MessageQueueTransactionType.None);
            }
            catch (Exception e)
            {
                var msg = "Unexpected error processing schedule text request " + textCommunicationData;
                _logger.Error(msg, e);
                var apiError = new ApiErrorDto("Schedule Text failed", e);
                throw new HttpResponseException(apiError.HttpResponseMessage);
            }

            return this.Ok();

        }

    }
}

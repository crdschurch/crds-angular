﻿using crds_angular.Services.Interfaces;
using Crossroads.Web.Common.Configuration;
using log4net;
using System;
using Twilio;
using Twilio.Exceptions;
using Twilio.Rest.Api.V2010.Account;

namespace crds_angular.Services
{
    public class TwilioService : ITextCommunicationService
    {
        public ILog _logger = LogManager.GetLogger(typeof(TwilioService));

        private readonly string _fromPhoneNumber;

        public TwilioService(IConfigurationWrapper configurationWrapper)
        {
            var accountSid = configurationWrapper.GetConfigValue("TwilioAccountSid");
            var authToken = Environment.GetEnvironmentVariable("TWILIO_AUTH_TOKEN");
            _fromPhoneNumber = configurationWrapper.GetConfigValue("TwilioFromPhoneNumber");
            TwilioClient.Init(accountSid, authToken);
        }

        public void SendTextMessage(string toPhoneNumber, string body)
        {
            _logger.Debug("Sending text message to " + toPhoneNumber);

            try
            {
                var message = MessageResource.Create(
                    from: new Twilio.Types.PhoneNumber(_fromPhoneNumber),
                    to: new Twilio.Types.PhoneNumber(toPhoneNumber),
                    body: body
                );
            }
            catch (ApiException e)
            {
                _logger.Error($"Twilio Error {e.Code} - {e.MoreInfo}");
            }
        }

        public void SetLogger(ILog logger)
        {
            _logger = logger;
        }
    }
}

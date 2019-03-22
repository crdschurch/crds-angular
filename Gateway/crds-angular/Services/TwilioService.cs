using System;
using crds_angular.Services.Interfaces;
using Crossroads.Utilities.Interfaces;
using log4net;
using Crossroads.Web.Common;
using Crossroads.Web.Common.Configuration;
using Twilio.Clients;
using Twilio;
using Twilio.Rest.Api.V2010.Account;
using Twilio.Exceptions;

namespace crds_angular.Services
{
    public class TwilioService : ITextCommunicationService
    {
        public ILog _logger = LogManager.GetLogger(typeof(TwilioService));

        private readonly string _fromPhoneNumber;

        public TwilioService(IConfigurationWrapper configurationWrapper)
        {
            var accountSid = configurationWrapper.GetConfigValue("TwilioAccountSid");
            var authToken = configurationWrapper.GetEnvironmentVarAsString("TWILIO_AUTH_TOKEN");
            _fromPhoneNumber = configurationWrapper.GetConfigValue("TwilioFromPhoneNumber");
            TwilioClient.Init(accountSid, authToken);
        }

        public void SendTextMessage(string toPhoneNumber, string body)
        {
            _logger.Debug("Sending text message to "+ toPhoneNumber);

            try
            {
                Console.WriteLine(body);
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

using System;
using System.Net;
using System.Net.Mail;
using crds_angular.Services.Interfaces;
using crds_angular.Models.Crossroads;
using Crossroads.AsyncJobs.Interfaces;
using Crossroads.AsyncJobs.Models;
using log4net;
using Newtonsoft.Json;

namespace Crossroads.AsyncJobs.Processors
{
    public class DirectEmailProcessor : IJobExecutor<DirectEmailCommunication>
    {
        private readonly IEmailCommunication _emailCommunication;

        private readonly ILog _logger = LogManager.GetLogger(typeof(DirectEmailProcessor));

        private const string SMTP_HEADER = "X-SMTPAPI";

        public DirectEmailProcessor(IEmailCommunication emailCommunication)
        {
            _emailCommunication = emailCommunication;
            System.Net.ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12;
        }

        public void Execute(JobDetails<DirectEmailCommunication> details)
        {
            var communicationId = details.Data.CommunicationId;

            var communication = _emailCommunication.GetDirectEmailMessage(communicationId);

            var messageSent = SendDirectEmail(communication);

            if (messageSent)
            {
                var recordsUpdated = _emailCommunication.UpdateDirectEmailMessagesToSent(communicationId, communication.CommunicationMessageId);
                if (!recordsUpdated)
                {
                    var serializedEmailObject = JsonConvert.SerializeObject(communication, Formatting.Indented);
                    _logger.Warn($"Failed to update messages to 'sent' for email: {serializedEmailObject}");

                }
            }
        }

        private bool SendDirectEmail(DirectEmailCommunication emailDto)
        {

            SmtpClient client = _emailCommunication.GetSmtpCredentials();

            try
            {
                using (MailMessage mail = new MailMessage())
                {
                    mail.From = new MailAddress(emailDto.From);
                    mail.To.Add(new MailAddress(emailDto.To));
                    mail.ReplyToList.Add(new MailAddress(emailDto.ReplyTo));
                    mail.Subject = emailDto.Subject;
                    mail.Body = emailDto.Body;
                    mail.IsBodyHtml = true;
                    mail.Headers.Add(SMTP_HEADER,
                                     string.Format(System.Globalization.CultureInfo.InvariantCulture,
                                                   "{{\"unique_args\": {{\"messageID\":\"{0}\", \"communicationID\":\"{1}\"}}}}",
                                                   new object[]
                                                   {
                                                       emailDto.CommunicationMessageId.ToString(),
                                                       emailDto.CommunicationId.ToString()
                                                   }));
                    client.Send(mail);
                }
            }
            catch (Exception e)
            {
                var serializedEmailObject = JsonConvert.SerializeObject(emailDto, Formatting.Indented);
                _logger.Error($"Could not send direct email: {serializedEmailObject}", e);
                return false;
            }return true;
        }
    }
}

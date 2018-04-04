using System;
using System.Collections.Generic;
using System.Messaging;
using System.Net.Mail;
using crds_angular.Models.Crossroads;
using crds_angular.Services.Interfaces;
using Crossroads.Utilities.Interfaces;
using Crossroads.Utilities.Messaging.Interfaces;
using Crossroads.Web.Common;
using Crossroads.Web.Common.Configuration;
using MinistryPlatform.Translation.Models;
using MinistryPlatform.Translation.Repositories.Interfaces;
using IPersonService = crds_angular.Services.Interfaces.IPersonService;

namespace crds_angular.Services
{
    public class EmailCommunication : IEmailCommunication
    {
        private readonly ICommunicationRepository _communicationService;
        private readonly IPersonService _personService;
        private readonly IContactRepository _contactService;
        private readonly IConfigurationWrapper _configurationWrapper;
        private readonly MessageQueue _directEmailQueue;
        private readonly IMessageFactory _messageFactory;

        private readonly int DefaultContactEmailId;
        private readonly int DomainID;
        private readonly int DefaultAuthorUserId;

        public EmailCommunication(ICommunicationRepository communicationService, 
            IPersonService personService, 
            IContactRepository contactService,
            IConfigurationWrapper configurationWrapper,
            IMessageQueueFactory messageQueueFactory = null,
            IMessageFactory messageFactory = null)
        {
            _communicationService = communicationService;
            _personService = personService;
            _contactService = contactService;
            _configurationWrapper = configurationWrapper;
            DefaultContactEmailId = _configurationWrapper.GetConfigIntValue("DefaultContactEmailId");
            DomainID = _configurationWrapper.GetConfigIntValue("DomainId");
            DefaultAuthorUserId = _configurationWrapper.GetConfigIntValue("DefaultAuthorUser");

            var directEmailQueueName = _configurationWrapper.GetConfigValue("DirectEmailQueueName");
            // ReSharper disable once PossibleNullReferenceException
            _directEmailQueue = messageQueueFactory.CreateQueue(directEmailQueueName, QueueAccessMode.Send);
            _messageFactory = messageFactory;
        }


        public DirectEmailCommunication GetDirectEmailMessage(int communicationId)
        {
            var mpDirectEmail = _communicationService.GetDirectEmail(communicationId);
            var directEmail = new DirectEmailCommunication()
            {
                CommunicationMessageId = mpDirectEmail.CommunicationMessageId,
                From = mpDirectEmail.From,
                To = mpDirectEmail.To,
                ReplyTo = mpDirectEmail.ReplyTo,
                Subject = mpDirectEmail.Subject,
                Body = mpDirectEmail.Body
            };
            return directEmail;
        }

        public bool UpdateDirectEmailMessagesToSent(int communicationId, int communicationMessageId)
        {
            var updated = _communicationService.UpdateDirectEmailMessagesToSent(communicationId, communicationMessageId);
            return updated;
        }

        public SmtpClient GetSmtpCredentials()
        {
            var client = _communicationService.GetSmtpCredentials();
            return client;
        }

        public void SendEmail(EmailCommunicationDTO email, string token)
        {
            var template = _communicationService.GetTemplate(email.TemplateId);

            if (token == null && email.FromUserId == null && template.FromContactId == 0)
            {
                throw (new InvalidOperationException("Must provide either email.FromUserId from  or an authentication token."));
            }

            var replyToContactId = email.ReplyToContactId ?? template.ReplyToContactId;
            var replyTo = new MpContact { ContactId = replyToContactId, EmailAddress = _communicationService.GetEmailFromContactId(replyToContactId) };

            var fromContactId = email.FromContactId ?? template.FromContactId;
            var from = new MpContact { ContactId = fromContactId, EmailAddress = _communicationService.GetEmailFromContactId(fromContactId) };

            MpContact to = GetMpContactFromEmailCommunicationDto(email);
            
            var communication = new MpCommunication
            {
                DomainId = DomainID,
                AuthorUserId = email.FromUserId ?? DefaultAuthorUserId,
                TemplateId = email.TemplateId,
                EmailBody = template.Body,
                EmailSubject = template.Subject,
                ReplyToContact = replyTo,
                FromContact = from,
                StartDate = email.StartDate ?? DateTime.Now,
                MergeData = email.MergeData,
                ToContacts = new List<MpContact>(),             
            };

            communication.ToContacts.Add(to);

            if (!communication.MergeData.ContainsKey("BaseUrl"))
            {
                communication.MergeData.Add("BaseUrl", _configurationWrapper.GetConfigValue("BaseUrl"));
            }

            bool sendImmediately = communication.StartDate <= DateTime.Today;
            if (sendImmediately)
            {
                var communicationId = _communicationService.SendMessage(communication, true);
                var directEmailId = new DirectEmailCommunication()
                {
                    CommunicationId = communicationId
                };
                var message = _messageFactory.CreateMessage(directEmailId);

                _directEmailQueue.Send(message);
            }
            else
            {
                _communicationService.SendMessage(communication);
            }
        }


        private MpContact GetMpContactFromEmailCommunicationDto(EmailCommunicationDTO email)
        {
            MpContact to;

            if (!email.ToContactId.HasValue && email.emailAddress == null)
            {
                throw (new InvalidOperationException("Must provide either ToContactId or emailAddress."));
            }

            if (email.ToContactId.HasValue)
            {
                return new MpContact { ContactId = email.ToContactId.Value, EmailAddress = _communicationService.GetEmailFromContactId(email.ToContactId.Value) };
            }
           
            var contactId = 0;
            try
            {
                contactId = _contactService.GetContactIdByEmail(email.emailAddress);
            }
            catch (Exception)
            {
                //work around incorrectly handled case where multiple contacts exists for a single contact
                contactId = 0;
            }
            if (contactId == 0)
            {
                contactId = DefaultContactEmailId;
            }
            to = new MpContact { ContactId = contactId, EmailAddress = email.emailAddress };

            return to;
        }

        public void SendEmail(CommunicationDTO emailData)
        {
            var replyToContactId = emailData.ReplyToContactId ?? DefaultContactEmailId;
                
            var from = new MpContact { ContactId = DefaultContactEmailId, EmailAddress = _communicationService.GetEmailFromContactId(DefaultContactEmailId) };
            var replyTo = new MpContact { ContactId = replyToContactId, EmailAddress = _communicationService.GetEmailFromContactId(replyToContactId) };

            var comm = new MpCommunication
            {
                AuthorUserId = DefaultAuthorUserId,
                DomainId = DomainID,
                EmailBody = emailData.Body,
                EmailSubject = emailData.Subject,
                FromContact = from,
                ReplyToContact = replyTo,
                MergeData = new Dictionary<string, object>(),
                ToContacts = new List<MpContact>()
            };
            foreach (var to in emailData.ToContactIds)
            {
                var contact  = new MpContact { ContactId = to, EmailAddress = _communicationService.GetEmailFromContactId(to) };
                comm.ToContacts.Add(contact);
            }
            _communicationService.SendMessage(comm);
        }

    }
}

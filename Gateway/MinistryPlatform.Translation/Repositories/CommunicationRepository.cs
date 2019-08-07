using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Net;
using System.Net.Mail;
using System.Reflection;
using System.Text.RegularExpressions;
using System.Web.Configuration;
using Crossroads.Utilities.Interfaces;
using log4net;
using Crossroads.Web.Common;
using Crossroads.Web.Common.Configuration;
using Crossroads.Web.Common.MinistryPlatform;
using Crossroads.Web.Common.Security;
using MinistryPlatform.Translation.Exceptions;
using MinistryPlatform.Translation.Extensions;
using MinistryPlatform.Translation.Models;
using MinistryPlatform.Translation.Repositories.Interfaces;
using MpCommunication = MinistryPlatform.Translation.Models.MpCommunication;

namespace MinistryPlatform.Translation.Repositories
{
    public class CommunicationRepository : BaseRepository, ICommunicationRepository

    {
        private readonly int _messagePageId = Convert.ToInt32(AppSettings("MessagesPageId"));
        private readonly int _recipientsSubPageId = Convert.ToInt32(AppSettings("RecipientsSubpageId"));
        private readonly int _communicationStatusId = Convert.ToInt32(AppSettings("CommunicationStatusId"));
        private readonly int _communicationDraftStatus = Convert.ToInt32(AppSettings("CommunicationDraftId"));
        private readonly int _actionStatusId = Convert.ToInt32(AppSettings("ActionStatusId"));
        private readonly int _contactPageId = Convert.ToInt32(AppSettings("Contacts"));

        private readonly ILog _logger = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);
        private readonly IMinistryPlatformService _ministryPlatformService;
        private readonly IMinistryPlatformRestRepository _ministryPlatformRestRepository;
        private readonly IApiUserRepository _apiUserRepository;

        private string EMAIL_USERNAME = String.Empty;
        private string EMAIL_PASSWORD = String.Empty;

        public CommunicationRepository(
            IMinistryPlatformService ministryPlatformService,
            IMinistryPlatformRestRepository ministryPlatformRestRepository,
            IAuthenticationRepository authenticationService,
            IConfigurationWrapper configurationWrapper,
            IApiUserRepository apiUserRepository)
            : base(authenticationService, configurationWrapper, apiUserRepository)
        {
            _ministryPlatformService = ministryPlatformService;
            _ministryPlatformRestRepository = ministryPlatformRestRepository;
            _apiUserRepository = apiUserRepository;
        }
		
        public int GetUserIdFromContactId(string token, int contactId)
        {
            int pNum = Convert.ToInt32(ConfigurationManager.AppSettings["MyContact"]);
            var profile = MinistryPlatformService.GetRecordDict(pNum, contactId, token);

            return (int) profile["User_Account"];
        }

        public int GetUserIdFromContactId(int contactId)
        {
            var profile = MinistryPlatformService.GetRecordDict(_contactPageId, contactId, ApiLogin());

            return (int)profile["User_Account"];
        }

        public string GetEmailFromContactId(int contactId)
        {
            var contact = _ministryPlatformService.GetRecordDict(_contactPageId, contactId, ApiLogin());
            return contact["Email_Address"].ToString();
        }

        public SmtpClient GetSmtpCredentials()
        {
            FetchEnvironmentVariables();
            var apiToken = _apiUserRepository.GetApiClientToken("CRDS.DirectEmail");
            var searchString = $"dp_Domains.[Domain_ID]='1'";
            string selectColumns = GetColumns();
            var mpClient = _ministryPlatformRestRepository.UsingAuthenticationToken(apiToken).Search<MpSmtpClient>(searchString, selectColumns, null, true).FirstOrDefault();
            var client = new SmtpClient(mpClient.Host, mpClient.Port);
            client.EnableSsl = mpClient.EnableSsl;
            client.Credentials = new NetworkCredential(EMAIL_USERNAME, EMAIL_PASSWORD);
            return client;
        }

        private void FetchEnvironmentVariables()
        {
            if (String.IsNullOrEmpty(EMAIL_USERNAME) || String.IsNullOrEmpty(EMAIL_PASSWORD))
            {
                EMAIL_USERNAME = _configurationWrapper.GetEnvironmentVarAsString("DIRECT_EMAIL_USERNAME");
                EMAIL_PASSWORD = _configurationWrapper.GetEnvironmentVarAsString("DIRECT_EMAIL_PASSWORD");
            }          
        }

        private string GetColumns()
        {
            var columns = "dp_Domains.[SMTP_Server_Port], dp_Domains.[SMTP_Server_Name], dp_Domains.[SMTP_Enable_SSL]";
            return columns;
        }

        public MpDirectEmailCommunication GetDirectEmail(int communicationId)
        {
            try
            {
                var apiToken = _apiUserRepository.GetApiClientToken("CRDS.DirectEmail");
                var searchString = $"dp_Communication_Messages.[Communication_ID]='{communicationId}'";
                const string selectColumns = "dp_Communication_Messages.[Communication_Message_ID], dp_Communication_Messages.[From], dp_Communication_Messages.[To], dp_Communication_Messages.[Reply_To], dp_Communication_Messages.[Subject], dp_Communication_Messages.[Body]";
                var email = _ministryPlatformRestRepository.UsingAuthenticationToken(apiToken).Search<MpDirectEmailCommunication>(searchString, selectColumns, null, true);
                return email.FirstOrDefault();
            }
            catch (Exception e)
            {
                _logger.Error($"Could not retrieve record from dp_Communication_Messages.", e);
            }
            return null;
        }

        public bool UpdateDirectEmailMessagesToSent(int communicationId, int communicationMessageId)
        {
            try
            {
                var apiToken = _apiUserRepository.GetApiClientToken("CRDS.DirectEmail");

                var parms = new Dictionary<string, object>
                {
                    { "@Communication_ID", communicationId },
                    { "@Communication_Message_ID", communicationMessageId }
                };
                _ministryPlatformRestRepository.UsingAuthenticationToken(apiToken).PostStoredProc(_configurationWrapper.GetConfigValue("MarkEmailAsSentProc"), parms);
                return true;
            }
            catch (Exception e)
            {
                return false;
            }
        }

        public MpCommunicationPreferences GetPreferences(String token, int userId)
        {
            int pNum = Convert.ToInt32( ConfigurationManager.AppSettings["MyContact"]);
            int hNum = Convert.ToInt32(ConfigurationManager.AppSettings["MyHousehold"]);
            var profile = _ministryPlatformService.GetRecordDict(pNum, userId, token);
            var household = _ministryPlatformService.GetRecordDict(hNum, (int)profile["Household_ID"], token);
            return new MpCommunicationPreferences
            {
                Bulk_Email_Opt_Out = (bool)profile["Bulk_Email_Opt_Out"],
                Bulk_Mail_Opt_Out = (bool)household["Bulk_Mail_Opt_Out"],
                Bulk_SMS_Opt_Out = (bool)profile["Bulk_SMS_Opt_Out"]
            };
        }

        public bool SetEmailSMSPreferences(String token, Dictionary<string,object> prefs){
            int pId = Convert.ToInt32(ConfigurationManager.AppSettings["MyContact"]);
            _ministryPlatformService.UpdateRecord(pId, prefs, token);
            return true;
        }

        public bool SetMailPreferences(string token, Dictionary<string,object> prefs){
            int pId = Convert.ToInt32(ConfigurationManager.AppSettings["MyHousehold"]);
            _ministryPlatformService.UpdateRecord(pId, prefs, token);
            return true;
        }

        /// <summary>
        /// Creates the correct record in MP so that the mail service can pick it up and send 
        /// it during the scheduled run
        /// </summary>
        /// <param name="communication">The message properties </param>     
        /// <param name="isDraft"> Is this message a draft? Defaults to false </param>   
        public int SendMessage(MpCommunication communication, bool isDraft = false)
        {
            var token = ApiLogin();
            var communicationStatus = isDraft ? _communicationDraftStatus : _communicationStatusId;
            var communicationId = AddCommunication(communication, token, communicationStatus);
            AddCommunicationMessages(communication, communicationId, token);
            return communicationId;
        }

        private int AddCommunication(MpCommunication communication, string token, int communicationStatus)
        {
            if(communication.StartDate == default(DateTime))
                communication.StartDate = DateTime.Now;
            
            var dictionary = new Dictionary<string, object>
            {
                {"Subject", communication.EmailSubject},
                {"Body", communication.EmailBody},
                {"Author_User_Id", communication.AuthorUserId},
                {"Start_Date", communication.StartDate},
                {"From_Contact", communication.FromContact.ContactId},
                {"Reply_to_Contact", communication.ReplyToContact.ContactId},
                {"Communication_Status_ID", communicationStatus}
            };
            var communicationId = _ministryPlatformService.CreateRecord(_messagePageId, dictionary, token);
            return communicationId;
        }

        private void AddCommunicationMessages(MpCommunication communication, int communicationId, string token)
        {
            foreach (MpContact contact in communication.ToContacts)
            {
                var dictionary = new Dictionary<string, object>
                {
                    {"Action_Status_ID", _actionStatusId},
                    {"Action_Status_Time", DateTime.Now},
                    {"Contact_ID", contact.ContactId},
                    {"From", communication.FromContact.EmailAddress},
                    {"To", contact.EmailAddress},
                    {"Reply_To", communication.ReplyToContact.EmailAddress},
                    {"Subject", ParseTemplateBody(communication.EmailSubject, communication.MergeData)},
                    {"Body", ParseTemplateBody(communication.EmailBody, communication.MergeData)}
                };
                if (contact.EmailAddress != "" && contact.EmailAddress != null)
                {
                    _ministryPlatformService.CreateSubRecord(_recipientsSubPageId, communicationId, dictionary, token);
                }
            }
        }

        public MpMessageTemplate GetTemplate(int templateId)
        {
            var pageRecords = _ministryPlatformService.GetRecordDict(_messagePageId, templateId, ApiLogin());

            if (pageRecords == null)
            {
                throw new InvalidOperationException("Couldn't find message template.");
            }

            var template = new MpMessageTemplate
            {
                Body = pageRecords["Body"].ToString(),
                Subject = pageRecords["Subject"].ToString(),
                FromContactId = pageRecords.ToInt("From_Contact"),
                FromEmailAddress = Regex.Replace(pageRecords["From_Contact_Text"].ToString(), "^.*;\\s*", string.Empty),
                ReplyToContactId = pageRecords.ToInt("Reply_to_Contact"),
                ReplyToEmailAddress = Regex.Replace(pageRecords["Reply_to_Contact_Text"].ToString(), "^.*;\\s*", string.Empty)
            };

            return template;
        }

        public MpCommunication GetTemplateAsCommunication(int templateId, int toContactId, string toEmailAddress, Dictionary<string, object> mergeData = null)
        {
            var template = GetTemplate(templateId);
            return new MpCommunication
            {
                AuthorUserId = _configurationWrapper.GetConfigIntValue("DefaultAuthorUser"),
                DomainId = 1,
                EmailBody = template.Body,
                EmailSubject = template.Subject,
                FromContact = new MpContact { ContactId = template.FromContactId, EmailAddress = template.FromEmailAddress },
                ReplyToContact = new MpContact { ContactId = template.ReplyToContactId, EmailAddress = template.ReplyToEmailAddress },
                ToContacts = new List<MpContact> { new MpContact { ContactId = toContactId, EmailAddress = toEmailAddress } },
                TemplateId = templateId,
                MergeData = mergeData
            };
        }

        public MpCommunication GetTemplateAsCommunication(int templateId, int fromContactId, string fromEmailAddress, int replyContactId, string replyEmailAddress, int toContactId, string toEmailAddress, Dictionary<string, object> mergeData = null)
        {
            var template = GetTemplate(templateId);
            return new MpCommunication
            {
                AuthorUserId = _configurationWrapper.GetConfigIntValue("DefaultAuthorUser"),
                DomainId = 1,
                EmailBody = template.Body,
                EmailSubject = template.Subject,
                FromContact = new MpContact {ContactId = fromContactId, EmailAddress = fromEmailAddress},
                ReplyToContact = new MpContact {ContactId = replyContactId, EmailAddress = replyEmailAddress},
                ToContacts = new List<MpContact>{ new MpContact{ContactId = toContactId, EmailAddress = toEmailAddress}},
                TemplateId = templateId,
                MergeData = mergeData
            };
        }

        public string ParseTemplateBody(string templateBody, Dictionary<string, object> record)
        {
            try
            {
                if (record == null)
                {
                    return templateBody;
                }
                return record.Aggregate(templateBody,
                    (current, field) => current.Replace("[" + field.Key + "]", field.Value == null ? string.Empty : field.Value.ToString()));
            }
            catch (Exception ex)
            {
                _logger.Warn("Failed to parse the template", ex);
                throw new TemplateParseException("Failed to parse the template", ex);
            }
        }
    }
}
using System;
using System.Web.Http;
using crds_angular.Models.Crossroads;
using crds_angular.Security;
using crds_angular.Services.Interfaces;
using MPInterfaces = MinistryPlatform.Translation.Repositories.Interfaces;
using crds_angular.Exceptions.Models;
using Crossroads.Utilities.Interfaces;
using Crossroads.ApiVersioning;
using Crossroads.Web.Common;
using Crossroads.Web.Common.Configuration;
using Crossroads.Web.Common.Security;

namespace crds_angular.Controllers.API
{
    public class EmailCommunicationController : ImpersonateAuthBaseController
    {
        private readonly IEmailCommunication _emailCommunication;
        private readonly IConfigurationWrapper _configurationWrapper;

        public EmailCommunicationController(IAuthTokenExpiryService authTokenExpiryService, 
                                            IEmailCommunication emailCommunication, 
                                            IConfigurationWrapper configurationWrapper, 
                                            IUserImpersonationService userImpersonationService, 
                                            IAuthenticationRepository authenticationRepository) 
            : base(authTokenExpiryService, userImpersonationService, authenticationRepository)
        {
            _emailCommunication = emailCommunication;
            _configurationWrapper = configurationWrapper;
        }

        /// <summary>
        /// Send an email to a specific contactId
        /// </summary>
        [RequiresAuthorization]
        [VersionedRoute(template: "send-email", minimumVersion: "1.0.0")]
        [Route("sendemail")]
        public IHttpActionResult Post(EmailCommunicationDTO email)
        {
            return Authorized(token =>
            {
                try
                {
                    _emailCommunication.SendEmail(email);

                    return Ok();
                }
                catch (Exception ex)
                {
                    return InternalServerError(ex);
                }
            });
        }

        /// <summary>
        /// Send an email to a group, takes in a list of contactId's
        /// </summary>
        [RequiresAuthorization]
        [VersionedRoute(template: "send-group-email", minimumVersion: "1.0.0")]
        [Route("sendgroupemail")]
        public IHttpActionResult Post([FromBody] CommunicationDTO communication)
        {
            return Authorized(token =>
            {
                try
                {
                    _emailCommunication.SendEmail(communication);
                    return Ok();
                }
                catch (Exception ex)
                {
                    return InternalServerError(ex);
                }
            });
        }

        /// <summary>
        /// Schedule an email to a specific contactId/emailAddress at a specific time
        /// </summary>
        [VersionedRoute(template: "send-email-reminder", minimumVersion: "1.0.0")]
        [Route("sendEmailReminder")]
        public IHttpActionResult PostReminder(EmailCommunicationDTO email)
        {
            try
            {
                email.TemplateId = _configurationWrapper.GetConfigIntValue("StreamReminderTemplate");
                _emailCommunication.SendEmail(email);
                return this.Ok();
            }
            catch (System.Exception ex)
            {
                var apiError = new ApiErrorDto("Schedule Email failed", ex);
                throw new HttpResponseException(apiError.HttpResponseMessage);
            }
        }
    }
}

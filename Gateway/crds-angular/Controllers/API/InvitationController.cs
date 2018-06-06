using System;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Net;
using System.Reflection;
using System.Web.Http;
using System.Web.Http.Description;
using crds_angular.Exceptions.Models;
using crds_angular.Models.Crossroads;
using crds_angular.Security;
using crds_angular.Services.Interfaces;
using log4net;
using Crossroads.ApiVersioning;
using Crossroads.Web.Common.Security;

namespace crds_angular.Controllers.API
{
    public class InvitationController : MPAuth
    {
        private readonly ILog _logger = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);
        private readonly Services.Interfaces.IInvitationService _invitationService;        


        public InvitationController(IAuthTokenExpiryService authTokenExpiryService, 
                                    Services.Interfaces.IInvitationService invitationService, 
                                    IUserImpersonationService userImpersonationService, 
                                    IAuthenticationRepository authenticationRepository) 
            : base(authTokenExpiryService, userImpersonationService, authenticationRepository)
        {
            _invitationService = invitationService;

        }

        /// <summary>
        /// Create a private invitation for someone to join a Group or Trip.  Returns a 403/Forbidden if the logged-in user (identified by the access token) is not allowed to create the invitation.
        /// </summary>
        /// <param name="invitation">The details of the invitation</param>
        /// <returns>An <see cref="Invitation"/>, with the GUID and ID populated</returns>
        [RequiresAuthorization]
        [ResponseType(typeof(Invitation))]
        [VersionedRoute(template: "invitation", minimumVersion: "1.0.0")]
        [Route("invitation"), HttpPost]
        public IHttpActionResult CreateInvitation([FromBody] Invitation invitation)
        {
            if (!ModelState.IsValid)
            {
                var errors = ModelState.Values.SelectMany(val => val.Errors).Aggregate("", (current, err) => current + err.Exception.Message);
                var dataError = new ApiErrorDto("CreateInvitation Data Invalid", new InvalidOperationException("Invalid CreateInvitation Data " + errors));
                throw new HttpResponseException(dataError.HttpResponseMessage);
            }

            return Authorized(token =>
            {
                try
                {
                    _invitationService.ValidateInvitation(invitation, token);
                    return Ok(_invitationService.CreateInvitation(invitation, token));
                }
                catch (ValidationException e)
                {
                    var error = new ApiErrorDto("Not authorized to send invitations of this type", e, HttpStatusCode.Forbidden);
                    throw new HttpResponseException(error.HttpResponseMessage);
                }
                catch (Exception e)
                {
                    _logger.Error(string.Format("Could not create invitation to recipient {0} ({1}) for group {2}", invitation.RecipientName, invitation.EmailAddress, invitation.SourceId), e);
                    var apiError = new ApiErrorDto("CreateInvitation Failed", e, HttpStatusCode.InternalServerError);
                    throw new HttpResponseException(apiError.HttpResponseMessage);
                }
            });
        }

    }
}
using crds_angular.Exceptions.Models;
using crds_angular.Models.Crossroads;
using crds_angular.Models.Json;
using crds_angular.Security;
using crds_angular.Services.Interfaces;
using Crossroads.ApiVersioning;
using Crossroads.ClientApiKeys;
using Crossroads.Web.Common.Security;
using MinistryPlatform.Translation.Repositories.Interfaces;
using System;
using System.Web.Http;
using System.Web.Http.Description;

namespace crds_angular.Controllers.API
{
    public class LoginController : MPAuth
    {

        private readonly IUserRepository _userService;
        private readonly ILoginService _loginService;

        private readonly IContactRepository _contactRepository;
        private readonly IAnalyticsService _analyticsService;

        public LoginController(IAuthTokenExpiryService authTokenExpiryService, 
                                ILoginService loginService, 
                                IUserRepository userService, 
                                IAnalyticsService analyticsService,
                                IUserImpersonationService userImpersonationService, 
                                IAuthenticationRepository authenticationRepository,
                                IContactRepository contactRepository) 
            : base(authTokenExpiryService, userImpersonationService, authenticationRepository)

        {
            _loginService = loginService;
            _userService = userService;

            _contactRepository = contactRepository;
            _analyticsService = analyticsService;

        }

        [VersionedRoute(template: "request-password-reset", minimumVersion: "1.0.0")]
        [Route("requestpasswordreset")]
        [HttpPost]
        public IHttpActionResult RequestPasswordReset(PasswordResetRequest request)
        {
            try
            {
                var isMobile = false;
                _loginService.PasswordResetRequest(request.Email, isMobile);
                return Ok();
            }
            catch (Exception ex)
            {
                return InternalServerError();
            }
        }

        [VersionedRoute(template: "request-password-reset-mobile", minimumVersion: "1.0.0")]
        [Route("requestpasswordreset/mobile")]
        [HttpPost]
        public IHttpActionResult RequestPasswordResetMobile(PasswordResetRequest request)
        {
            try
            {
                var isMobile = true;
                _loginService.PasswordResetRequest(request.Email, isMobile);
                return Ok();
            }
            catch (Exception ex)
            {
                return InternalServerError();
            }
        }

        [VersionedRoute(template: "verify-reset-token/{token}", minimumVersion: "1.0.0")]
        [Route("verifyresettoken/{token}")]
        [HttpGet]
        public IHttpActionResult VerifyResetTokenRequest(string token)
        {
            try
            {
                ResetTokenStatus status = new ResetTokenStatus
                {
                    TokenValid = _loginService.VerifyResetToken(token)
                };
                return Ok(status);
            }
            catch (Exception ex)
            {
                return InternalServerError();
            }
        }

        [VersionedRoute(template: "reset-password", minimumVersion: "1.0.0")]
        [Route("resetpassword")]
        [HttpPost]
        public IHttpActionResult ResetPassword(PasswordReset request)
        {
            try
            {
                _loginService.ResetPassword(request.Password, request.Token);
                return Ok();
            }
            catch (Exception ex)
            {
                return InternalServerError();
            }
        }

        [ResponseType(typeof (LoginReturn))]
        [VersionedRoute(template: "authenticated", minimumVersion: "1.0.0")]
        [Route("authenticated")]
        [HttpGet]
        public IHttpActionResult isAuthenticated()
        {
            return Authorized(token =>
            {
                try
                {
                    var contact = _contactRepository.GetMyProfile(token);
                    if (contact == null)
                    {
                        return Unauthorized();
                    }

                    var apiToken = _userService.HelperApiLogin();
                    var user = _userService.GetByContactId(contact.Contact_ID, apiToken);
                    var roles = _userService.GetUserRolesRest(user.UserRecordId, apiToken);

                    var loginReturn = new LoginReturn
                    {
                        userId = contact.Contact_ID,
                        userToken = token,
                        username = contact.First_Name,
                        userEmail = contact.Email_Address,
                        userPhone = contact.Mobile_Phone,
                        roles = roles,
                        canImpersonate = user.CanImpersonate
                    };
                    return Ok(loginReturn);
                }
                catch (Exception)
                {
                    return Unauthorized();
                }
            });
        }

        [VersionedRoute(template: "login", minimumVersion: "1.0.0")]
        [Route("login")]
        [ResponseType(typeof (LoginReturn))]
        // TODO - Once Ez-Scan has been updated to send a client API key (US7764), remove the IgnoreClientApiKey attribute
        [IgnoreClientApiKey]
        public IHttpActionResult Post([FromBody] LoginCredentials cred)
        {
            try
            {
                // try to login
                var authData = AuthenticationRepository.AuthenticateUser(cred.username, cred.password, true);
                var token = authData.AccessToken;
                var exp = authData.ExpiresIn+"";
                var refreshToken = authData.RefreshToken;

                if (token == "")
                {
                    return Unauthorized();
                }

                var apiToken = _userService.HelperApiLogin();
                var user = _userService.GetByUserName(cred.username,apiToken); //235 ms _userService.GetByAuthenticationToken(token) was 1.5 seconds 
                var userRoles = _userService.GetUserRolesRest(user.UserRecordId, apiToken);
                var contact = _contactRepository.GetContactByUserRecordId(user.UserRecordId, apiToken);//use a rest call and use the id directly
                var loginReturn = new LoginReturn
                {
                    userToken = token,
                    userTokenExp = exp,
                    refreshToken = refreshToken,
                    userId = contact.Contact_ID,
                    username = contact.First_Name,
                    userEmail = contact.Email_Address,
                    roles = userRoles,
                    age = contact.Age,
                    userPhone = contact.Mobile_Phone,
                    canImpersonate = user.CanImpersonate 
                };


                _loginService.ClearResetToken(user.UserRecordId); //no need to lookup the userid if we already have it
                _contactRepository.UpdateContactToActive(contact.Contact_ID);
                _analyticsService.Track(contact.Contact_ID.ToString(), "SignedIn");

                //Kick off a call to the migration service to create or update an account in Okta on the user's behalf
                OktaMigrationUser oktaMigrationUser = new OktaMigrationUser
                {
                    firstName = contact.First_Name,
                    lastName = contact.Last_Name,
                    email = contact.Email_Address,
                    login = cred.username,
                    password = cred.password,
                    mpContactId = contact.Contact_ID.ToString()
                };
                _loginService.CreateOrUpdateOktaAccount(oktaMigrationUser);

                return Ok(loginReturn);
            }
            catch (Exception e)
            {
                var apiError = new ApiErrorDto("Login Failed", e);
                throw new HttpResponseException(apiError.HttpResponseMessage);
            }
        }

        [VersionedRoute(template: "verify-credentials", minimumVersion: "1.0.0")]
        [Route("verifycredentials")]
        [HttpPost]
        public IHttpActionResult VerifyCredentials([FromBody] LoginCredentials cred)
        {
            return Authorized(token =>
            {
                try
                {
                    var authData = AuthenticationRepository.AuthenticateUser(cred.username, cred.password);

                    // if the username or password is wrong, auth data will be null
                    if (authData == null)
                    {
                        return Unauthorized();
                    }

                    return Ok();
                }
                catch (Exception e)
                {
                    var apiError = new ApiErrorDto("Verify Credentials Failed", e);
                    throw new HttpResponseException(apiError.HttpResponseMessage);
                }
            });
        }

        /// <summary>
        /// Validates the password provided matches the user identified by the token.  This function
        /// can be used in place of VerifyCredentials() to validate the password for the current user
        /// without requiring an email address.
        /// </summary>
        /// <param name="password"></param>
        /// <returns>Ok (200) if the password is valid, or Unauthorized (401) if the password is not
        /// valid or an error occurs</returns>
        [VersionedRoute(template: "verify-password", minimumVersion: "1.0.0")]
        [Route("verifypassword")]
        [HttpPost]
        public IHttpActionResult VerifyPassword([FromBody] string password)
        {
            return Authorized(token =>
            {
                try
                {
                    if (!_loginService.IsValidPassword(token, password))
                        return Unauthorized();

                    return Ok();
                }
                catch (Exception e)
                {
                    var apiError = new ApiErrorDto("Verify Password Failed", e);
                    throw new HttpResponseException(apiError.HttpResponseMessage);
                }
            });
        }
    }
}

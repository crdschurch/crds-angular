using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Http;
using log4net;
using System.Reflection;
using crds_angular.Services.Interfaces;
using crds_angular.Util;
using Crossroads.Web.Common.Security;
using System.Net.Http.Headers;
using Crossroads.Web.Auth.Exceptions;
using Crossroads.Web.Auth.Models;
using Crossroads.Web.Auth.Services;

namespace crds_angular.Security
{
    public class ImpersonateAuthBaseController : ApiController
    {
        protected readonly log4net.ILog logger = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

        private readonly IAuthTokenExpiryService _authTokenExpiryService;
        private readonly IUserImpersonationService _userImpersonationService;
        protected readonly IAuthenticationRepository AuthenticationRepository;

        public ImpersonateAuthBaseController(IAuthTokenExpiryService authTokenExpiryService,
            IUserImpersonationService userImpersonationService,
            IAuthenticationRepository authenticationRepository)
        {
            _authTokenExpiryService = authTokenExpiryService;
            _userImpersonationService = userImpersonationService;
            AuthenticationRepository = authenticationRepository;
        }

        /// <summary>
        /// Ensure that a user is authenticated before executing the given lambda expression.  The expression will
        /// have a reference to the user's authentication token (the value of the "Authorization" cookie).  If
        /// the user is not authenticated, an UnauthorizedResult will be returned.
        /// </summary>
        /// <param name="doIt">A lambda expression to execute if the user is authenticated</param>
        /// <returns>An IHttpActionResult from the "doIt" expression, or UnauthorizedResult if the user is not authenticated.</returns>
        protected IHttpActionResult Authorized(Func<AuthDTO, IHttpActionResult> doIt)
        {
            return (Authorized(doIt, () => { return (Unauthorized()); }));
        }

        /// <summary>
        /// Execute the lambda expression "actionWhenAuthorized" if the user is authenticated, or execute the expression
        /// "actionWhenNotAuthorized" if the user is not authenticated.  If authenticated, the "actionWhenAuthorized"
        /// expression will have a reference to the user's authentication token (the value of the "Authorization" cookie).
        /// </summary>
        /// <param name="actionWhenAuthorized">A lambda expression to execute if the user is authenticated</param>
        /// <param name="actionWhenNotAuthorized">A lambda expression to execute if the user is NOT authenticated</param>
        /// <returns>An IHttpActionResult from the lambda expression that was executed.</returns>
        protected IHttpActionResult Authorized(Func<AuthDTO, IHttpActionResult> actionWhenAuthorized, Func<IHttpActionResult> actionWhenNotAuthorized)
        {
            string accessToken = Request.Headers.GetValues("Authorization").FirstOrDefault();

            AuthDTO auth;
            try
            {
                auth = AuthService.AuthorizeAsync(accessToken).Result;
            }
            catch (AccessTokenNullOrEmptyException ex)
            {
                logger.Debug(ex.Message);
                return actionWhenNotAuthorized();
            }
            catch (RemoteAuthException ex)
            {
                logger.Debug(ex.Message);
                return actionWhenNotAuthorized();
            }
            catch (AuthServiceUrlUndefinedException ex)
            {
                logger.Debug(ex.Message);
                return actionWhenNotAuthorized();
            }

            //If its an mp token we need to perform the token refresh logic:
            if (auth.Authentication.Provider == "mp")
            {
                return RefreshTokensAndHandleImpersonate(actionWhenAuthorized, actionWhenNotAuthorized, auth);
            }
            else // Okta for now but could be another provider in the future
            {
                return actionWhenAuthorized(auth);
            }

            
        }

        private IHttpActionResult RefreshTokensAndHandleImpersonate(Func<AuthDTO, IHttpActionResult> actionWhenAuthorized, 
            Func<IHttpActionResult> actionWhenNotAuthorized,
            AuthDTO authDTO)
        {
            try
            {
                IEnumerable<string> refreshTokens;
                IEnumerable<string> impersonateUserIds;
                bool impersonate = false;
                var authorized = "";

                if (Request.Headers.TryGetValues("ImpersonateUserId", out impersonateUserIds) && impersonateUserIds.Any())
                {
                    impersonate = true;
                }

                bool authTokenCloseToExpiry = _authTokenExpiryService.IsAuthtokenCloseToExpiry(Request.Headers);
                bool isRefreshTokenPresent =
                    Request.Headers.TryGetValues("RefreshToken", out refreshTokens) && refreshTokens.Any();

                HttpRequestHeaders headers = Request.Headers;

                if (authTokenCloseToExpiry && isRefreshTokenPresent)
                {
                    var authData = AuthenticationRepository.RefreshToken(refreshTokens.FirstOrDefault());
                    if (authData != null)
                    {
                        authorized = authData.AccessToken;
                        var refreshToken = authData.RefreshToken;
                        IHttpActionResult result = null;
                        if (impersonate)
                        {
                            result =
                                new HttpAuthResult(
                                    _userImpersonationService.WithImpersonation(authorized, impersonateUserIds.FirstOrDefault(), () => actionWhenAuthorized(authDTO)),
                                    authorized,
                                    refreshToken);
                        }
                        else
                        {
                            result = new HttpAuthResult(actionWhenAuthorized(authDTO), authorized, refreshToken);
                        }
                        return result;
                    }
                }

                
                if (impersonate)
                {
                    return _userImpersonationService.WithImpersonation(authorized, impersonateUserIds.FirstOrDefault(), () => actionWhenAuthorized(authDTO));
                }
                else
                {
                    return actionWhenAuthorized(authDTO);
                }
            }
            catch (System.InvalidOperationException e)
            {
                return actionWhenNotAuthorized();
            }
        }
    }
}
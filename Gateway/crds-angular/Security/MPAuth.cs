using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Http;
using log4net;
using System.Reflection;
using crds_angular.Services.Interfaces;
using crds_angular.Util;
using Crossroads.Web.Common.Security;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Net.Http.Headers;

namespace crds_angular.Security
{
    public class MPAuth : ApiController
    {
        protected readonly log4net.ILog logger = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

        private readonly IUserImpersonationService _userImpersonationService;
        protected readonly IAuthenticationRepository AuthenticationRepository;

        public MPAuth(IUserImpersonationService userImpersonationService, IAuthenticationRepository authenticationRepository)
        {
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
        protected IHttpActionResult Authorized(Func<string,IHttpActionResult> doIt )
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
        protected IHttpActionResult Authorized(Func<string, IHttpActionResult> actionWhenAuthorized, Func<IHttpActionResult> actionWhenNotAuthorized)
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

                bool authTokenCloseToExpiry = isAuthtokenCloseToExpiry(Request.Headers);  
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
                                    _userImpersonationService.WithImpersonation(authorized, impersonateUserIds.FirstOrDefault(), () => actionWhenAuthorized(authorized)),
                                    authorized,
                                    refreshToken);
                        }
                        else
                        {
                            result = new HttpAuthResult(actionWhenAuthorized(authorized), authorized, refreshToken);
                        }
                        return result;
                    }
                }

                authorized = Request.Headers.GetValues("Authorization").FirstOrDefault();   
                if (authorized != null && (authorized != "null" || authorized != ""))
                {
                    if (impersonate)
                    {
                        return _userImpersonationService.WithImpersonation(authorized, impersonateUserIds.FirstOrDefault(), () => actionWhenAuthorized(authorized));
                    }
                    else
                    {
                        return actionWhenAuthorized(authorized);
                    }
                }
                return actionWhenNotAuthorized();
            }
            catch (System.InvalidOperationException e)
            {
                return actionWhenNotAuthorized();
            }
        }

      private bool isAuthtokenCloseToExpiry(HttpRequestHeaders headers)
      {
        const int expirationBufferInSeconds = 60;

        IEnumerable<string> authorizationHeaders;
        headers.TryGetValues("Authorization", out authorizationHeaders);
        string authTokenHeader = authorizationHeaders.FirstOrDefault();
        JwtSecurityToken authToken = new JwtSecurityToken(authTokenHeader);
        DateTime tokenValidTo = authToken.ValidTo;
        var secondsToAuthTokenExpiry = (tokenValidTo - DateTime.UtcNow).TotalSeconds;

        bool authTokenCloseToExpiry = secondsToAuthTokenExpiry < expirationBufferInSeconds;

        return authTokenCloseToExpiry;
    }
    }
}
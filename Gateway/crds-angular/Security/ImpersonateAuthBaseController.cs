﻿using System;
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
using Crossroads.Web.Common.Configuration;
using RestSharp;
using System.Net;
using Newtonsoft.Json;

namespace crds_angular.Security
{
    public class ImpersonateAuthBaseController : ApiController
    {
        protected readonly log4net.ILog logger = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

        private readonly IAuthTokenExpiryService _authTokenExpiryService;
        private readonly IUserImpersonationService _userImpersonationService;
        protected readonly IAuthenticationRepository AuthenticationRepository;

        //Added for Auth test
        private static RestClient client = new RestClient();
        private static string _authUrl = Environment.GetEnvironmentVariable("AUTH_SERVICE_BASE_URL");

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
            logger.Info($"Attempting authorization check...");
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
            string accessToken;
            IEnumerable<string> accessTokens;
            Request.Headers.TryGetValues("Authorization", out accessTokens);
            accessToken = accessTokens == null ? string.Empty : accessTokens.FirstOrDefault();
            bool authTokenCloseToExpiry = _authTokenExpiryService.IsAuthtokenCloseToExpiry(Request.Headers);

            IEnumerable<string> refreshTokens;
            Request.Headers.TryGetValues("RefreshToken", out refreshTokens);
            string refreshToken = refreshTokens == null ? null : refreshTokens.FirstOrDefault();

            bool shouldGetNewAccessToken = authTokenCloseToExpiry && (refreshToken != null);

            if (shouldGetNewAccessToken) // Check if request is an mp token with an mp refresh token, if so we may need to refresh
            {
                logger.Info($"Refreshing access token above with ");
                var authData = AuthenticationRepository.RefreshToken(refreshToken);
                if (authData != null)
                {
                    accessToken = authData.AccessToken;
                    refreshToken = authData.RefreshToken;
                }
            }

            AuthDTO auth;
            try
            {
                logger.Info($"Requesting AuthService to verify access token : {accessToken}");
                //DOING AUTHENTICATION IN THIS CLASS BELOW TO TEST
                //auth = AuthService.Authorize(accessToken);
                auth = Authorize(accessToken);
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
            logger.Info($"Looking to get ImpersonateUserId's");
            IEnumerable<string> impersonateUserIds;
            Request.Headers.TryGetValues("ImpersonateUserId", out impersonateUserIds);
            bool impersonate = (impersonateUserIds != null) && impersonateUserIds.Any();
            logger.Info("Checking if we need to get new refresh token");
            //If its an mp token we need to perform the token refresh logic:
            if (auth.Authentication.Provider == "mp")
            {
                IHttpActionResult result = null;
                if (impersonate)
                {
                    if (shouldGetNewAccessToken)
                    {
                        result =
                        new HttpAuthResult(
                            _userImpersonationService.WithImpersonation(auth, impersonateUserIds.FirstOrDefault(), () => actionWhenAuthorized(auth)),
                            accessToken,
                            refreshToken);
                    }
                    else
                    {
                        result = _userImpersonationService.WithImpersonation(auth, impersonateUserIds.FirstOrDefault(), () => actionWhenAuthorized(auth));
                    }
                }
                else
                {
                    if (shouldGetNewAccessToken)
                    {
                        result = new HttpAuthResult(actionWhenAuthorized(auth), accessToken, refreshToken);
                    }
                    else
                    {
                        result = actionWhenAuthorized(auth);
                    }
                }
                return result;
            }
            else // Okta for now but could be another provider in the future
            {
                return actionWhenAuthorized(auth);
            }
        }

        /*
         * Adding auth code in order to debug gateway
         */
        public AuthDTO Authorize(string accessToken)
        {
            try
            {
                ValidateAccessToken(accessToken);

                var authServiceBaseUrl = _authUrl;
                logger.Info($"Prepping Auth request to : {authServiceBaseUrl}");
                ValidateBaseUrl(authServiceBaseUrl);

                AuthDTO auth = GetAuthorizeResponse(accessToken, authServiceBaseUrl);

                return auth;
            }catch(Exception ex)
            {
                logger.Error($"Error Authorizing : {ex.Message}");
                throw new AccessTokenNullOrEmptyException();
            }
            
        }

        public AuthDTO GetAuthorizeResponse(string accessToken, string baseUrl)
        {
#if FULLFRAMEWORK
            ServicePointManager.SecurityProtocol |= SecurityProtocolType.Tls11 | SecurityProtocolType.Tls12;
#endif
            client.BaseUrl = new Uri(baseUrl);
            var request = new RestRequest("api/authorize", Method.GET);
            request.AddHeader("Authorization", accessToken);
            logger.Info($"Sending auth request...");
            IRestResponse response = client.Execute(request);
            logger.Info($"Auth Response : {response.StatusCode} - {response.Content}");
            string responseString = response.Content;

            AuthDTO auth = BuildAuthFromResponse(response, responseString);

            return auth;
        }

        public static AuthDTO BuildAuthFromResponse(IRestResponse response, string responseString)
        {
            AuthDTO auth = new AuthDTO();

            if (response.StatusCode == HttpStatusCode.OK)
            {
                auth = JsonConvert.DeserializeObject<AuthDTO>(responseString);
            }
            else
            {
                throw new RemoteAuthException(responseString);
            }

            return auth;
        }

        public static void ValidateAccessToken(string accessToken)
        {
            if (String.IsNullOrEmpty(accessToken))
            {
                throw new AccessTokenNullOrEmptyException();
            }
        }

        public static void ValidateBaseUrl(string baseUrl)
        {
            if (baseUrl == null)
            {
                throw new AuthServiceUrlUndefinedException();
            }
        }
    }
}
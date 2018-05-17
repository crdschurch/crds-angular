using crds_angular.Services.Interfaces;
using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Net.Http.Headers;

namespace crds_angular.Services
{
  public class AuthTokenExpiryService : IAuthTokenExpiryService
  {
    public bool IsAuthtokenCloseToExpiry(HttpRequestHeaders headers)
    {
      const int expirationBufferInSeconds = 60;

      IEnumerable<string> authorizationHeaders;
      headers.TryGetValues("Authorization", out authorizationHeaders);

      if (authorizationHeaders == null || !authorizationHeaders.Any())
      {
        return true;
      }

      string authTokenHeader = authorizationHeaders.FirstOrDefault();
      JwtSecurityToken authToken = new JwtSecurityToken(authTokenHeader);
      DateTime tokenValidTo = authToken.ValidTo;
      var secondsToAuthTokenExpiry = (tokenValidTo - DateTime.UtcNow).TotalSeconds;

      bool authTokenCloseToExpiry = secondsToAuthTokenExpiry < expirationBufferInSeconds;

      return authTokenCloseToExpiry;
    }
  }
}
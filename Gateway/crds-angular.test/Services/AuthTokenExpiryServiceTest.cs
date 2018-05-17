using crds_angular.Services;
using Microsoft.IdentityModel.Tokens;
using NUnit.Framework;
using System;
using System.IdentityModel.Tokens.Jwt;
using System.Net.Http;

namespace crds_angular.test.Services
{
  public class AuthTokenExpiryServiceTest
  {
    private AuthTokenExpiryService _fixture;
    private HttpRequestMessage _httpRequest;

    private string _someUrl = "https://www.crossroads.net/";
    private string _jwt;

    [SetUp]
    public void SetUp()
    {
      _fixture = new AuthTokenExpiryService();
    }

    [Test]
    public void ShouldIndicateThatTokenIsNotExpiringSoon()
    {
      int minutesToTokenExpiration = 30;
      _jwt = GenerateToken(minutesToTokenExpiration);

      _httpRequest = new HttpRequestMessage()
      {
        RequestUri = new Uri(_someUrl),
        Method = HttpMethod.Get
      };

      _httpRequest.Headers.Add("Authorization", _jwt);

      var isCloseToExpiry = 
        _fixture.IsAuthtokenCloseToExpiry(_httpRequest.Headers);
      Assert.IsFalse(isCloseToExpiry);
    }

    [Test]
    public void ShouldIndicateThatTokenIsExpiringSoon()
    {
      int minutesToTokenExpiration = 1;
      _jwt = GenerateToken(minutesToTokenExpiration);

      _httpRequest = new HttpRequestMessage()
      {
        RequestUri = new Uri(_someUrl),
        Method = HttpMethod.Get
      };

      _httpRequest.Headers.Add("Authorization", _jwt);

      var isCloseToExpiry = 
        _fixture.IsAuthtokenCloseToExpiry(_httpRequest.Headers);
      Assert.IsTrue(isCloseToExpiry);
    }

    private string GenerateToken(int minutesUntilTokenExpiry)
    {
      const string someSecret = "db3OIsj+BXE9NZDy0t8W3TcNekrF+2d/1sFnWG4HnV" +
                                "8TZY30iTOdtVWJG8abWvB1GlOgJuQZdcF2Luqm/hccMw==";

      byte[] symmetricKey = Convert.FromBase64String(someSecret);
      var tokenHandler = new JwtSecurityTokenHandler();

      var now = DateTime.UtcNow;
      var tokenDescriptor = new SecurityTokenDescriptor
      {
        Expires = now.AddMinutes(Convert.ToInt32(minutesUntilTokenExpiry)),
        SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(symmetricKey), SecurityAlgorithms.HmacSha256Signature)
      };

      var stoken = tokenHandler.CreateToken(tokenDescriptor);
      var token = tokenHandler.WriteToken(stoken);

      return token;
    }
  }
}
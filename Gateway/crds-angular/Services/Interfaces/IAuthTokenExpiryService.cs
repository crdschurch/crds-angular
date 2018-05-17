using System.Net.Http.Headers;

namespace crds_angular.Services.Interfaces
{
  public interface IAuthTokenExpiryService
  {
    bool IsAuthtokenCloseToExpiry(HttpRequestHeaders headers);
  }
}
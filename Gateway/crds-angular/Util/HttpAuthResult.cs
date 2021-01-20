using System;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using System.Web.Http;

namespace crds_angular.Util
{
    class HttpAuthResult : IHttpActionResult
    {
        private readonly String _token;
        private readonly String _refreshToken;
        private readonly IHttpActionResult _result;

        public HttpAuthResult(IHttpActionResult result, string token, string refreshToken)
        {
            _result = result;
            _token = token;
            _refreshToken = refreshToken;
        }

        public Task<HttpResponseMessage> ExecuteAsync(CancellationToken cancellationToken)
        {
            return Task.Run(() =>
            {
                var response = _result.ExecuteAsync(cancellationToken).Result;
                response.Headers.Add("Access-Control-Expose-Headers", "sessionId, refreshToken");
                response.Headers.Add("sessionId", _token);
                response.Headers.Add("refreshToken", _refreshToken);
                return response;
            },
            cancellationToken);
        }
    }
}

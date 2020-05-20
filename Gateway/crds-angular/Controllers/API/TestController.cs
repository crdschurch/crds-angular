using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Reactive;
using System.Reactive.Disposables;
using System.Reactive.Linq;
using System.Reflection;
using System.Threading.Tasks;
using System.Web.Http;
using System.Web.Http.Description;
using crds_angular.Exceptions.Models;
using crds_angular.Models.Crossroads.Waivers;
using crds_angular.Security;
using crds_angular.Services.Interfaces;
using Crossroads.ApiVersioning;
using Crossroads.Web.Common.Security;
using log4net;
using RestSharp;

namespace crds_angular.Controllers.API
{
    public class TestController : ApiController
    {

        protected readonly log4net.ILog logger = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

        [VersionedRoute(template: "test/identityHealth1", minimumVersion: "1.0.0")]
        [HttpGet]
        public string IdentityHealthCheck1()
        {
            ServicePointManager.Expect100Continue = true;
            ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12;
            RestClient client = new RestClient();
                client.BaseUrl = new Uri("https://api-int.crossroads.net/identity");
                var request = new RestRequest("api/health", Method.GET);
                logger.Info("Sending RestClient request to Identity service health endpoint");
            var response = client.Execute(request);
            return $"Response Code : {response.StatusCode} - Message : {response.Content}";

        }

        [VersionedRoute(template: "test/identityHealth2", minimumVersion: "1.0.0")]
        [HttpGet]
        public async Task<string> IdentityHealthCheck2()
        {
            ServicePointManager.Expect100Continue = true;
            ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12;
            var client = new HttpClient();
                var request = new HttpRequestMessage(HttpMethod.Get, "https://api-int.crossroads.net/identity/api/health");
                logger.Info("Sending HttpClient request to Identity service health endpoint");
            logger.Info($"Relevant Headers - Connection:{request.Headers.Connection}, Authorization:{request.Headers.Authorization}");
            var response = await client.SendAsync(request);
            string error = response.Headers.GetValues("X-Error").FirstOrDefault();
            string errorCode = response.Headers.GetValues("X-Error-Code").FirstOrDefault();
            return $"Response Code : {response.StatusCode} - Message : {response.Content} Headers: X-Error - {error}, X-Error-Code - {errorCode} ";
        }

        [VersionedRoute(template: "test/identityHealth3", minimumVersion: "1.0.0")]
        [HttpGet]
        public async Task<string> IdentityHealthCheck3()
        {
            ServicePointManager.Expect100Continue = true;
            ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12;
            var client = new HttpClient();
            var request = new HttpRequestMessage(HttpMethod.Get, "https://api.crossroads.net/identity/api/health");
            logger.Info("Sending HttpClient request to Identity service health endpoint");
            logger.Info($"Relevant Headers - Connection:{request.Headers.Connection}, Authorization:{request.Headers.Authorization}");
            var response = await client.SendAsync(request);
            string error = response.Headers.GetValues("X-Error").FirstOrDefault();
            string errorCode = response.Headers.GetValues("X-Error-Code").FirstOrDefault();
            return $"Response Code : {response.StatusCode} - Message : {response.Content} Headers: X-Error - {error}, X-Error-Code - {errorCode} ";
        }

        [VersionedRoute(template: "test/authHealth1", minimumVersion: "1.0.0")]
        [HttpGet]
        public async Task<string> AuthHealthCheck1()
        {
            ServicePointManager.Expect100Continue = true;
            ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12;
            RestClient client = new RestClient();
                client.BaseUrl = new Uri("https://api-int.crossroads.net/auth");
                var request = new RestRequest("api/health/ready", Method.GET);
                logger.Info("Sending RestClient request to Identity service health endpoint");
            var response = client.Execute(request);
            return $"Response Code : {response.StatusCode} - Message : {response.Content}";
        }

        [VersionedRoute(template: "test/authHealth2", minimumVersion: "1.0.0")]
        [HttpGet]
        public async Task<string> AuthHealthCheck2()
        {
            ServicePointManager.Expect100Continue = true;
            ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12;
            var client = new HttpClient();
                var request = new HttpRequestMessage(HttpMethod.Get, "https://api-int.crossroads.net/auth/api/health/ready");
                logger.Info("Sending HttpClient request to Identity service health endpoint");
            logger.Info($"Relevant Headers - Connection:{request.Headers.Connection}, Authorization:{request.Headers.Authorization}");
            var response = await client.SendAsync(request);
                string error = response.Headers.GetValues("X-Error").FirstOrDefault();
                string errorCode = response.Headers.GetValues("X-Error-Code").FirstOrDefault();
                return $"Response Code : {response.StatusCode} - Message : {response.Content} Headers: X-Error - {error}, X-Error-Code - {errorCode} ";
        }

        [VersionedRoute(template: "test/google", minimumVersion: "1.0.0")]
        [HttpGet]
        public async Task<string> UrlHealthCheck()
        {
            ServicePointManager.Expect100Continue = true;
            ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12;
            try
            {
                var client = new HttpClient();
                var request = new HttpRequestMessage(HttpMethod.Get, "https://google.com");
                logger.Info("Sending HttpClient request to Identity service health endpoint");
                logger.Info($"Relevant Headers - Connection:{request.Headers.Connection}, Authorization:{request.Headers.Authorization}");
                var response = await client.SendAsync(request);
                string error = response.Headers.GetValues("X-Error").FirstOrDefault();
                string errorCode = response.Headers.GetValues("X-Error-Code").FirstOrDefault();
                return $"Response Code : {response.StatusCode} - Message : {response.Content}.  Headers: X-Error - {error}, X-Error-Code - {errorCode} ";
            }
            catch (Exception ex)
            {
                logger.Info($"Error sending request to Identity : {ex.Message}");
                return ex.Message;
            }
        }


    }
}
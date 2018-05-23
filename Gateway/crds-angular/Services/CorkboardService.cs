using System;
using System.Net;
using crds_angular.Services.Interfaces;
using Crossroads.ClientApiKeys;
using Crossroads.Web.Common.Configuration;
using Crossroads.Web.Common.MinistryPlatform;
using RestSharp;

namespace crds_angular.Services
{
    public class CorkboardService: ICorkboardService
    {
        private readonly IRestClient _servicesRestClient;
        private readonly IApiUserRepository _apiUserRepository;
        private readonly IConfigurationWrapper _configuration;

        public CorkboardService(IRestClient servicesRestClient, IApiUserRepository apiUserRepository, IConfigurationWrapper configuration)
        {
            _servicesRestClient = servicesRestClient;
            _apiUserRepository = apiUserRepository;
            _configuration = configuration;
        }

            
        public void SyncPosts()
        {
            var token = _apiUserRepository.GetDefaultApiClientToken();                    
            var request = new RestRequest("corkboard/api/v1.0.0/syncposts", Method.POST);
            request.AddParameter("Authorization",$"Bearer {token}", ParameterType.HttpHeader);

            var serverApiKey = _configuration.GetMpConfigValue("CRDS-COMMON", "ServerApiKey");
            if (!string.IsNullOrWhiteSpace(serverApiKey))
            {
                request.AddHeader("Crds-Api-Key", serverApiKey);
            }

            var response = _servicesRestClient.Execute(request);
            if (response.StatusCode != HttpStatusCode.OK)
            {
                throw new ApplicationException($"Received {response.StatusCode} status code from corkboard api.");
            }
        }
    }
}

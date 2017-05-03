﻿using System;
using System.Net;
using crds_angular.Services.Interfaces;
using Crossroads.Web.Common.MinistryPlatform;
using RestSharp;

namespace crds_angular.Services
{
    public class CorkboardService: ICorkboardService
    {
        private readonly IRestClient _corkboardRestClient;
        private readonly IApiUserRepository _apiUserRepository;

        public CorkboardService(IRestClient corkboardRestClient, IApiUserRepository apiUserRepository)
        {
            _corkboardRestClient = corkboardRestClient;
            _apiUserRepository = apiUserRepository;
        }

            
        public void SyncPosts()
        {
            // TODO: What do we do with Client API token?
            var token = _apiUserRepository.GetToken();                    
            var request = new RestRequest("corkboard/api/v1.0.0/syncposts", Method.POST);
            request.AddParameter("Authorization",$"Bearer {token}", ParameterType.HttpHeader);

            var response = _corkboardRestClient.Execute(request);
            if (response.StatusCode != HttpStatusCode.OK)
            {
                throw new ApplicationException($"Received {response.StatusCode} status code from corkboard api.");
            }
        }
    }
}

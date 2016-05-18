﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using MinistryPlatform.Translation.Extensions;
using MinistryPlatform.Translation.Services.Interfaces;
using Newtonsoft.Json;
using RestSharp;
using RestSharp.Extensions;
using MinistryPlatform.Models.Attributes;

namespace MinistryPlatform.Translation.Services
{
    public class MinistryPlatformRestService : IMinistryPlatformRestService
    {
        private readonly IRestClient _ministryPlatformRestClient;
        private readonly ThreadLocal<string> _authToken = new ThreadLocal<string>();

        public MinistryPlatformRestService(IRestClient ministryPlatformRestClient)
        {
            _ministryPlatformRestClient = ministryPlatformRestClient;
        }

        public IMinistryPlatformRestService UsingAuthenticationToken(string authToken)
        {
            _authToken.Value = authToken;
            return this;
        }

        public T Get<T>(int recordId, string selectColumns = null)
        {
            var url = AddColumnSelection(string.Format("/tables/{0}/{1}", GetTableName<T>(), recordId), selectColumns);
            var request = new RestRequest(url, Method.GET);
            AddAuthorization(request);

            var response = _ministryPlatformRestClient.Execute(request);
            _authToken.Value = null;
            response.CheckForErrors(string.Format("Error getting {0} by ID {1}", GetTableName<T>(), recordId), true);

            var content = JsonConvert.DeserializeObject<List<T>>(response.Content);
            if (content == null || !content.Any())
            {
                return default(T);
            }

            return content.FirstOrDefault();
        }

        public List<T> Search<T>(string searchString = null, string selectColumns = null)
        {
            var search = string.IsNullOrWhiteSpace(searchString) ? string.Empty : string.Format("?$filter={0}", searchString);

            var url = AddColumnSelection(string.Format("/tables/{0}{1}", GetTableName<T>(), search), selectColumns);
            var request = new RestRequest(url, Method.GET);
            AddAuthorization(request);

            var response = _ministryPlatformRestClient.Execute(request);
            _authToken.Value = null;
            response.CheckForErrors(string.Format("Error searching {0}", GetTableName<T>()));

            var content = JsonConvert.DeserializeObject<List<T>>(response.Content);

            return content;
        }

        private void AddAuthorization(IRestRequest request)
        {
            if (_authToken.IsValueCreated)
            {
                request.AddHeader("Authorization", string.Format("Bearer {0}", _authToken.Value));
            }
        }

        private static string GetTableName<T>()
        {
            var table = typeof(T).GetAttribute<RestApiTable>();
            if (table == null)
            {
                throw new NoTableDefinitionException(typeof(T));
            }

            return table.Name;
        }

        private static string AddColumnSelection(string url, string selectColumns)
        {
            return string.IsNullOrWhiteSpace(selectColumns) ? url : string.Format("{0}&$select={1}", url, selectColumns);
        }
    }

    public class NoTableDefinitionException : Exception
    {
        public NoTableDefinitionException(Type t) : base(string.Format("No RestApiTable attribute specified on type {0}", t)) { }
    }
}
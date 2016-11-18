﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Net;
using MinistryPlatform.Translation.Extensions;
using MinistryPlatform.Translation.Models.Attributes;
using MinistryPlatform.Translation.Repositories.Interfaces;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using RestSharp;
using RestSharp.Extensions;

namespace MinistryPlatform.Translation.Repositories
{
    public class MinistryPlatformRestRepository : IMinistryPlatformRestRepository
    {
        private readonly IRestClient _ministryPlatformRestClient;
        private readonly ThreadLocal<string> _authToken = new ThreadLocal<string>();
        private const string DeleteRecordsStoredProcName = "api_crds_Delete_Table_Rows";

        public MinistryPlatformRestRepository(IRestClient ministryPlatformRestClient)
        {
            _ministryPlatformRestClient = ministryPlatformRestClient;
        }

        public IMinistryPlatformRestRepository UsingAuthenticationToken(string authToken)
        {
            _authToken.Value = authToken;
            return this;
        }

        public T Get<T>(int recordId, string selectColumns = null)
        {
            var url = AddGetColumnSelection($"/tables/{GetTableName<T>()}/{recordId}", selectColumns);
            var request = new RestRequest(url, Method.GET);
            AddAuthorization(request);

            var response = _ministryPlatformRestClient.Execute(request);
            _authToken.Value = null;
            response.CheckForErrors($"Error getting {GetTableName<T>()} by ID {recordId}", true);

            var content = JsonConvert.DeserializeObject<List<T>>(response.Content);
            if (content == null || !content.Any())
            {
                return default(T);
            }

            return content.FirstOrDefault();
        }

        public T Get<T>(string tableName, int recordId, string columnName)
        {
            var url = AddGetColumnSelection($"/tables/{tableName}/{recordId}", columnName);
            var request = new RestRequest(url, Method.GET);
            AddAuthorization(request);

            var response = _ministryPlatformRestClient.Execute(request);
            _authToken.Value = null;
            response.CheckForErrors($"Error getting {tableName} by ID {recordId}", true);

            var content = JsonConvert.DeserializeObject<List<T>>(response.Content);
            if (content == null || !content.Any())
            {
                return default(T);
            }

            return content.FirstOrDefault();
        }

        public List<T> Get<T>(string tableName, Dictionary<string,object> filter)
        {
            var url = AddFilter($"/tables/{tableName}", filter);

            var request = new RestRequest(url, Method.GET);
            AddAuthorization(request);

            var response = _ministryPlatformRestClient.Execute(request);
            _authToken.Value = null;
            response.CheckForErrors($"Error getting {tableName} using filter", true);

            var content = JsonConvert.DeserializeObject<List<T>>(response.Content);
           
            return content;
        }

        public List<List<T>> GetFromStoredProc<T>(string procedureName)
        {
            return GetFromStoredProc<T>(procedureName, new Dictionary<string, object>());
        }

        public List<List<T>> GetFromStoredProc<T>(string procedureName, Dictionary<string, object> parameters)
        {
            var url = $"/procs/{procedureName}/{FormatStoredProcParameters(parameters)}";
            var request = new RestRequest(url, Method.GET);
            AddAuthorization(request);

            var response = _ministryPlatformRestClient.ExecuteAsGet(request, "GET");
            _authToken.Value = null;
            response.CheckForErrors($"Error executing procedure {procedureName}", true);

            var content = JsonConvert.DeserializeObject<List<List<T>>>(response.Content);
            if (content == null || !content.Any())
            {
                return default(List<List<T>>);
            }
            return content;
        }   

        public int PostStoredProc(string procedureName, Dictionary<string, object> parameters)
        {
            var url = $"/procs/{procedureName}";
            var request = new RestRequest(url, Method.POST);
            AddAuthorization(request);
          
            request.AddParameter("application/json", FormatStoredProcBody(parameters), ParameterType.RequestBody);
            var response = _ministryPlatformRestClient.Execute(request);
            _authToken.Value = null;
            response.CheckForErrors($"Error executing procedure {procedureName}", true);

            return (int) response.StatusCode;
        }

        public int Post<T>(List<T> records)
        {
            var json = JsonConvert.SerializeObject(records);
            var url = $"/tables/{GetTableName<T>()}";

            var request = new RestRequest(url, Method.POST);
            AddAuthorization(request);

            request.AddParameter("application/json", json, ParameterType.RequestBody);

            var response = _ministryPlatformRestClient.Execute(request);
            _authToken.Value = null;
            response.CheckForErrors($"Error updating {GetTableName<T>()}", true);

            return (int) response.StatusCode;
        }

        public List<T> PostWithReturn<M, T>(List<M> records)
        {
            var json = JsonConvert.SerializeObject(records);
            var url = $"/tables/{GetTableName<M>()}";
            var request = new RestRequest(url, Method.POST);
            AddAuthorization(request);
            request.AddParameter("application/json", json, ParameterType.RequestBody);
            var response = _ministryPlatformRestClient.Execute(request);
            _authToken.Value = null;
            response.CheckForErrors($"Error updating {GetTableName<M>()}", true);            
            if (response.StatusCode == HttpStatusCode.OK)
            {
                var content = JsonConvert.DeserializeObject<List<T>>(response.Content);
                return content;
            }
            return default(List<T>);
        }

        public int Put<T>(List<T> records)
        {
            var json = JsonConvert.SerializeObject(records);
            var url = $"/tables/{GetTableName<T>()}";

            var request = new RestRequest(url, Method.PUT);
            AddAuthorization(request);

            request.AddParameter("application/json", json, ParameterType.RequestBody);

            var response = _ministryPlatformRestClient.Execute(request);
            _authToken.Value = null;
            response.CheckForErrors($"Error updating {GetTableName<T>()}", true);

            return (int)response.StatusCode;
        }

        public int Put(string tableName, List<Dictionary<string, object>> records)
        {
            //build the json
            var json = JsonConvert.SerializeObject(records);
            var url = $"/tables/{tableName}";

            var request = new RestRequest(url, Method.PUT);
            AddAuthorization(request);

            request.AddParameter("application/json", json, ParameterType.RequestBody);

            var response = _ministryPlatformRestClient.Execute(request);
            _authToken.Value = null;
            response.CheckForErrors($"Error updating {tableName}", true);

            return (int)response.StatusCode;
        }

        private static string FormatStoredProcBody(Dictionary<string, object> parameters)
        {
            var parmlist = new List<string>();
            foreach (var item in parameters)
            {
                var parm = "\"" + item.Key + "\":\"" + item.Value + "\"";
                parmlist.Add(parm);
            }
             
            return "{" + string.Join(",", parmlist) + "}";
        }

        private string MpRestEncode(string data)
        {
            return WebUtility.UrlEncode(data)?.Replace("+", "%20");
        }

        private static string FormatStoredProcParameters(Dictionary<string, object> parameters)
        {
            var result = parameters.Aggregate("?", (current, parameter) => current + ((parameter.Key.StartsWith("@") ? parameter.Key : "@" + parameter.Key) + "=" + parameter.Value + "&"));
            return result.TrimEnd('&');
        }


        /// <summary>
        /// this allows us to search one table, and return a type of another
        /// </summary>
        /// <typeparam name="T1">Table/Type to search</typeparam>
        /// <typeparam name="T2">Type to return</typeparam>
        /// <param name="searchString">where clause</param>
        /// <param name="columns">select statement</param>
        /// <param name="orderByString">order by clause</param>
        /// <param name="distinct">should we only return distinct</param>
        /// <returns>List of T2</returns>
        public List<T2> Search<T1, T2>(string searchString = null, string selectColumns = null, string orderByString = null, bool distinct = false)
        {
            var search = string.IsNullOrWhiteSpace(searchString) ? string.Empty : $"?$filter={MpRestEncode(searchString)}";
            var orderBy = string.IsNullOrWhiteSpace(orderByString) ? string.Empty : $"&$orderby={MpRestEncode(orderByString)}";
            var distinctString = $"&$distinct={distinct.ToString()}";

            var url = AddColumnSelection($"/tables/{GetTableName<T1>()}{search}{orderBy}{distinctString}", selectColumns);
            var request = new RestRequest(url, Method.GET);
            AddAuthorization(request);

            var response = _ministryPlatformRestClient.Execute(request);
            _authToken.Value = null;
            response.CheckForErrors($"Error searching {GetTableName<T1>()}");

            var content = JsonConvert.DeserializeObject<List<T2>>(response.Content);

            return content;
        }

        /// <summary>
        /// this allows us to search one table, and return a type of another
        /// </summary>
        /// <typeparam name="T1">Table/Type to search</typeparam>
        /// <typeparam name="T2">Type to return</typeparam>
        /// <param name="searchString">where clause</param>
        /// <param name="columns">select statement</param>
        /// <param name="orderByString">order by clause</param>
        /// <param name="distinct">should we only return distinct</param>
        /// <returns>List of T2</returns>
        public List<T2> Search<T1, T2>(string searchString, List<string> columns, string orderByString = null, bool distinct = false)
        {
            string selectColumns = null;
            if (columns != null)
            {
                selectColumns = string.Join(",", columns);
            }
            return Search<T1, T2>(searchString, selectColumns, orderByString, distinct);
        }

        public List<T> Search<T>(string searchString = null, string selectColumns = null, string orderByString = null, bool distinct = false)
        {
            var search = string.IsNullOrWhiteSpace(searchString) ? string.Empty : $"?$filter={MpRestEncode(searchString)}";
            var orderBy = string.IsNullOrWhiteSpace(orderByString) ? string.Empty : $"&{MpRestEncode($"$orderby={orderByString}")}";
            var distinctString = $"&{MpRestEncode($"$distinct={distinct.ToString()}")}";

            var url = AddColumnSelection(string.Format("/tables/{0}{1}{2}{3}&%24top=0", GetTableName<T>(), search, orderBy, distinctString),selectColumns);

            var request = new RestRequest(url, Method.GET);
            AddAuthorization(request);

            var response = _ministryPlatformRestClient.Execute(request);
            _authToken.Value = null;
            response.CheckForErrors($"Error searching {GetTableName<T>()}");

            var content = JsonConvert.DeserializeObject<List<T>>(response.Content);

            return content;
        }

        public List<T> Search<T>(string searchString, List<string> columns, string orderByString = null, bool distinct = false)
        {
            string selectColumns = null;
            if (columns != null)
            {
                selectColumns = string.Join(",", columns);
            }
            return Search<T>(searchString, selectColumns, orderByString, distinct);
        }

        public T Search<T>(string tableName, string searchString, string column)
        {
            var search = string.IsNullOrWhiteSpace(searchString) ? string.Empty : $"?$filter={MpRestEncode(searchString)}";
            var url = AddColumnSelection($"/tables/{tableName}{search}", column);
            var request = new RestRequest(url, Method.GET);
            AddAuthorization(request);

            var response = _ministryPlatformRestClient.Execute(request);
            _authToken.Value = null;
            response.CheckForErrors($"Error getting {tableName}", true);
            var returnVal = default(T);
            if (response.Content.Length > 2)
            {
                var jsonResponse = JObject.Parse(response.Content.TrimStart('[').TrimEnd(']'));
                returnVal = jsonResponse.Values().FirstOrDefault().Value<T>();
            }
            return returnVal;
        }

        public void UpdateRecord(string tableName, int recordId, Dictionary<string, object> fields)
        {
            var url = $"/tables/{tableName}";
            var request = new RestRequest(url, Method.PUT);
            AddAuthorization(request);
            request.AddParameter("application/json", "[" + FormatStoredProcBody(fields) + "]", ParameterType.RequestBody);

            var response = _ministryPlatformRestClient.Execute(request);
            response.CheckForErrors($"Error updating {tableName}", true);
        }

        public void Delete<T>(int recordId)
        {
            Delete<T>(new[] { recordId });
        }

        public void Delete<T>(IEnumerable<int> recordIds)
        {
            var parms = new Dictionary<string, object>
            {
                {"@TableName", GetTableName<T>()},
                {"@PrimaryKeyColumnName", GetPrimaryKeyColumnName<T>()},
                {"@IdentifiersToDelete", string.Join(",", recordIds)}
            };

            PostStoredProc(DeleteRecordsStoredProcName, parms);
        }


        private void AddAuthorization(IRestRequest request)
        {
            if (_authToken.IsValueCreated)
            {
                request.AddHeader("Authorization", $"Bearer {_authToken.Value}");
            }
        }

        private static string GetTableName<T>()
        {
            var table = typeof(T).GetAttribute<MpRestApiTable>();
            if (table == null)
            {
                throw new NoTableDefinitionException<T>();
            }

            return table.Name;
        }

        private static string GetPrimaryKeyColumnName<T>()
        {
            var primaryKey = typeof(T).GetProperties().ToList().Select(p => p.GetAttribute<MpRestApiPrimaryKey>()).FirstOrDefault();
            if (primaryKey == null)
            {
                throw new NoPrimaryKeyDefinitionException<T>();
            }
            return primaryKey.Name;
        }

        private static string AddColumnSelection(string url, string selectColumns)
        {
            return string.IsNullOrWhiteSpace(selectColumns) ? url : $"{url}&$select={selectColumns}";
        }

        private static string AddGetColumnSelection(string url, string selectColumns)
        {
            return string.IsNullOrWhiteSpace(selectColumns) ? url : $"{url}?$select={selectColumns}";
        }

        private static string AddFilter(string url, Dictionary<string,object> filter)
        {
            var filterString = "";

            foreach (var entry in filter)
            {
                if (filterString.Length > 0)
                {
                    filterString += ",";
                }

                filterString += entry.Key + "=" + entry.Value;
            }

            return string.IsNullOrWhiteSpace(filterString) ? url : $"{url}?$filter={filterString}";
        }
    }

    public class NoTableDefinitionException<T> : Exception
    {
        public NoTableDefinitionException() : base($"No RestApiTable attribute specified on type {typeof(T)}") { }
    }

    public class NoPrimaryKeyDefinitionException<T> : Exception
    {
        public NoPrimaryKeyDefinitionException() : base($"No RestApiPrimaryKey attribute specified on type {typeof(T)}") { }
    }

}
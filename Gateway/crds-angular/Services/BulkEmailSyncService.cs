using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Security.Cryptography;
using System.Timers;
using System.Text;
using System.Threading;
using crds_angular.Models.MailChimp;
using crds_angular.Services.Interfaces;
using Crossroads.Utilities.Serializers;
using log4net;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using RestSharp;
using MPInterfaces = MinistryPlatform.Translation.Repositories.Interfaces;
using AutoMapper;
using Crossroads.Web.Common.Configuration;
using Crossroads.Web.Common.MinistryPlatform;
using MinistryPlatform.Translation.Models;
using RestSharp.Authenticators;

namespace crds_angular.Services
{
    public class BulkEmailSyncService : IBulkEmailSyncService
    {
        private readonly ILog _logger = LogManager.GetLogger(typeof(GroupService));

        private readonly MPInterfaces.IBulkEmailRepository _bulkEmailRepository;
        private readonly IApiUserRepository _apiUserService;
        private readonly IConfigurationWrapper _configWrapper;
        private string _token;
        private System.Timers.Timer _refreshTokenTimer;
        // TODO: Consider changing this to read record count in query to accommodate pagination
        private const int MAX_SYNC_RECORDS = 1000000;

        public BulkEmailSyncService(
            MPInterfaces.IBulkEmailRepository bulkEmailRepository,
            IApiUserRepository apiUserService,
            IConfigurationWrapper configWrapper)
        {
            _bulkEmailRepository = bulkEmailRepository;
            _apiUserService = apiUserService;
            _configWrapper = configWrapper;

            _token = _apiUserService.GetDefaultApiClientToken();
            
            ConfigureRefreshTokenTimer();
        }

        private void ConfigureRefreshTokenTimer()
        {
            // Hack to get around token expiring every 15 minutes when updating large number of Third_Party_Contact_Id, 
            // so fire event every 10 minutes and get a new token
            _refreshTokenTimer = new System.Timers.Timer(10*60*1000);
            _refreshTokenTimer.AutoReset = true;
            _refreshTokenTimer.Elapsed += RefreshTokenTimerElapsed;
        }

        private void RefreshTokenTimerElapsed(object sender, ElapsedEventArgs elapsedEventArgs)
        {
            _logger.Info("Refreshing token");
            _token = _apiUserService.GetDefaultApiClientToken();            
        }


        public void RunService()
        {
            try
            {                
                _refreshTokenTimer.Start();

                var publications = _bulkEmailRepository.GetPublications(_token);
                var publicationOperationIds = new Dictionary<MpBulkEmailPublication, List<string>>();

                foreach (var publication in publications)
                {
                    PullSubscriptionStatusChangesFromThirdParty(publication);

                    var pageViewIds = _bulkEmailRepository.GetPageViewIds(_token, publication.PublicationId);
                    var subscribers = _bulkEmailRepository.GetSubscribers(_token, publication.PublicationId, pageViewIds);

                    var operationIds = CreateAndSendBatches(publication, subscribers);
                    publicationOperationIds.Add(publication, operationIds);
                }

                ProcessSynchronizationResultsWithRetries(publicationOperationIds);
            }
            finally
            {
                _refreshTokenTimer.Stop();
            }
        }

        private List<string> CreateAndSendBatches(MpBulkEmailPublication publication, List<MpBulkEmailSubscriber> subscribers)
        {            
            var batchSize = 10000;
            var batches = Math.Ceiling(subscribers.Count/(decimal) batchSize);
            var operationIds = new List<string>();

            for (var batchIndex = 0; batchIndex < batches; batchIndex++)
            {
                var currentBatch = subscribers.Skip(batchIndex*batchSize).Take(batchSize);
                var operationId = SendBatch(publication, currentBatch.ToList(), batchIndex);
                
                if (!string.IsNullOrEmpty(operationId))
                {
                    operationIds.Add(operationId);
                }
            }

            return operationIds;
        }

        public string SendBatch(MpBulkEmailPublication publication, List<MpBulkEmailSubscriber> subscribers, int batchIndex)
        {
            var client = GetBulkEmailClient();

            var request = new RestRequest("batches", Method.POST);
            request.AddHeader("Content-Type", "application/json");

            var batch = AddSubscribersToBatch(publication, subscribers);

            request.JsonSerializer = new RestsharpJsonNetSerializer();
            request.RequestFormat = DataFormat.Json;
            request.AddBody(batch);

            var response = client.Execute(request);

            if (response.StatusCode != HttpStatusCode.OK)
            {
                // This will be addressed is US2861: MP/MailChimp Synch Error Handling 
                // TODO: Should these be exceptions?
                _logger.ErrorFormat("Failed sending batch {0} for publication {1} with StatusCode = {2}", batchIndex, publication.PublicationId, response.StatusCode);
                return null;
            }

            var content = response.Content;
            
            if (String.IsNullOrEmpty(content))
            {
                // This will be addressed is US2861: MP/MailChimp Synch Error Handling 
                // TODO: Should these be exceptions?
                // TODO: Add logging code here for failure
                _logger.ErrorFormat("Bulk email sync failed for batch {0} for publication {1} empty response", batchIndex, publication.PublicationId);
                return null;
            }

            var responseValues = DeserializeToDictionary(content);

            // this needs to be returned, because we can't guarantee that the operation won't fail after it begins
            if (responseValues["status"].ToString() == "started" || 
                responseValues["status"].ToString() == "pending" || 
                responseValues["status"].ToString() == "finished")
            {
                return responseValues["id"].ToString();
            }
            else
            {
                // This will be addressed is US2861: MP/MailChimp Synch Error Handling
                // TODO: Should these be exceptions?
                // TODO: Add logging code here for failure
                _logger.ErrorFormat("Bulk email sync failed for batch {0} publication {1} Response detail: {2}", batchIndex, publication.PublicationId, content);
                return null;
            }
        }

        private void ProcessSynchronizationResultsWithRetries(Dictionary<MpBulkEmailPublication, List<string>> publicationOperations)
        {
            var configurationWaitHours = _configWrapper.GetConfigIntValue("BulkEmailMaximumWaitHours");
            var waitTime = (int) TimeSpan.FromSeconds(10).TotalSeconds;
            var maximumWaitTime = (int) TimeSpan.FromHours(configurationWaitHours).TotalSeconds;
            var maxRetries = maximumWaitTime / waitTime;
            var attempts = 0;

            do
            {                
                attempts++;
                
                if (attempts > maxRetries)
                {
                    // This will be addressed is US2861: MP/MailChimp Synch Error Handling 
                    // TODO: Should these be exceptions?
                    // Probably an infinite loop so stop processing and log error
                    _logger.ErrorFormat("Failed to LogUpdateStatuses after {0} total retries", attempts);
                    return;
                }

                // pause to allow the operations to complete -- consider switching this to async
                Thread.Sleep(waitTime * 1000);

                // TODO: Think about creating BulkEmailPublicationOperation object and moving away from Dictionary<BulkEmailPublication, List<string>> 
                publicationOperations = ProcessSynchronizationResults(publicationOperations);

            } while (publicationOperations.Any());
        }

        private Dictionary<MpBulkEmailPublication, List<string>> ProcessSynchronizationResults(Dictionary<MpBulkEmailPublication, List<string>> publicationOperations)
        {
            var client = GetBulkEmailClient();

            for (int index = publicationOperations.Count - 1; index >= 0; index--)
            {
                var idPair = publicationOperations.ElementAt(index);
                var publication = idPair.Key;
                var operations = idPair.Value;

                ProcessSynchronizationResultsForPublication(operations, client, publication);

                if (operations.Count == 0)
                {
                    // All operations are complete for this publication
                    _bulkEmailRepository.UpdateLastSyncDate(_token, publication);
                    publicationOperations.Remove(publication);
                }
            }

            return publicationOperations;
        }

        private void ProcessSynchronizationResultsForPublication(List<string> operations, RestClient client, MpBulkEmailPublication publication)
        {
            for (var operationIndex = operations.Count - 1; operationIndex >= 0; operationIndex--)
            {
                var operationId = operations[operationIndex];
                var request = new RestRequest("batches/" + operationId, Method.GET);
                request.AddHeader("Content-Type", "application/json");

                var response = client.Execute(request);
                if (response.StatusCode != HttpStatusCode.OK)
                {
                    // This will be addressed is US2861: MP/MailChimp Synch Error Handling 
                    _logger.WarnFormat("Received StatusCode = {0} when querying for status of publication {1} for operation {2}",
                                       response.StatusCode,
                                       publication.PublicationId,
                                       operationId);
                    continue;
                }

                var responseValues = DeserializeToDictionary(response.Content);

                if (responseValues["status"].ToString() == "started" || responseValues["status"].ToString() == "pending")
                {
                    continue;
                }

                if (responseValues["status"].ToString() == "finished")
                {
                    var total = responseValues["total_operations"];
                    var errors = responseValues["errored_operations"];

                    if (errors.ToString() == "0")
                    {
                        _logger.InfoFormat("Processed {0} total records for publication {1} and operation {2}", total, publication.PublicationId, operationId);
                    }
                    else
                    {
                        _logger.ErrorFormat("Processed {0} total records with {1} errors for publication {2} and operation {3}", total, errors, publication.PublicationId, operationId);
                    }

                    _logger.Info(response.Content);

                    var completedAt = DateTime.Parse(responseValues["completed_at"].ToString());
                    if (publication.LastSuccessfulSync <= completedAt)
                    {
                        publication.LastSuccessfulSync = completedAt;
                    }
                }
                else
                {
                    // This will be addressed is US2861: MP/MailChimp Synch Error Handling 
                    // TODO: Should these be exceptions?
                    // TODO: Add logging code here for failure
                    _logger.ErrorFormat("Bulk email sync failed for publication {0} and operation {1} Response detail: {2}", publication.PublicationId, operationId, response.Content);
                }

                operations.RemoveAt(operationIndex);
            }
        }

        private RestClient GetBulkEmailClient()
        {
            var apiUrl = _configWrapper.GetConfigValue("BulkEmailApiUrl");
            var apiKey = _configWrapper.GetEnvironmentVarAsString("BULK_EMAIL_API_KEY");
           
            var client = new RestClient(apiUrl);
            client.Authenticator = new HttpBasicAuthenticator("noname", apiKey);

            return client;
        }

        private SubscriberBatchDTO AddSubscribersToBatch(MpBulkEmailPublication publication, List<MpBulkEmailSubscriber> subscribers)
        {
            var batch = new SubscriberBatchDTO();
            batch.Operations = new List<SubscriberOperationDTO>();

            foreach (var subscriber in subscribers)
            {
                if (subscriber.EmailAddress != subscriber.ThirdPartyContactId)
                {
                    if (!string.IsNullOrEmpty(subscriber.ThirdPartyContactId))
                    {                        
                        var unsubscribeOperation = UnsubscribeOldEmailAddress(publication, subscriber);
                        batch.Operations.Add(unsubscribeOperation);
                    }

                    //Assuming success here, update the ID to match the emailaddress
                    //TODO: US2782 - need to also recongnize this is an email change and handle this acordingly
                    subscriber.ThirdPartyContactId = subscriber.EmailAddress;
                    _bulkEmailRepository.UpdateSubscriber(_token, subscriber);
                }

                var operation = GetOperation(publication, subscriber);
                batch.Operations.Add(operation);
            }
            
            return batch;
        }

        private SubscriberOperationDTO UnsubscribeOldEmailAddress(MpBulkEmailPublication publication, MpBulkEmailSubscriber subscriber)
        {
            var updatedSubcriber = subscriber.Clone();
            updatedSubcriber.Subscribed = false;
            var updateOperation = GetOperation(publication, updatedSubcriber);
            return updateOperation;
        }

        private SubscriberOperationDTO GetOperation(MpBulkEmailPublication publication, MpBulkEmailSubscriber subscriber)
        {
            var mailChimpSubscriber = new SubscriberDTO();
            
            mailChimpSubscriber.Subscribed = subscriber.Subscribed;
            mailChimpSubscriber.EmailAddress = subscriber.ThirdPartyContactId;
            mailChimpSubscriber.MergeFields = subscriber.MergeFields;

            var hashedEmail = CalculateMD5Hash(mailChimpSubscriber.EmailAddress.ToLower());

            var operation = new SubscriberOperationDTO();
            operation.Method = "PUT";
            operation.Path = string.Format("lists/{0}/members/{1}", publication.ThirdPartyPublicationId, hashedEmail);

            // TODO: Do we need to store this somewhere to verify subscriber processed successfully
            operation.OperationId = Guid.NewGuid().ToString();
            operation.Body = JsonConvert.SerializeObject(mailChimpSubscriber);

            return operation;
        }

        // From http://stackoverflow.com/questions/1207731/how-can-i-deserialize-json-to-a-simple-dictionarystring-string-in-asp-net
        private Dictionary<string, object> DeserializeToDictionary(string jo)
        {
            var values = JsonConvert.DeserializeObject<Dictionary<string, object>>(jo);
            var values2 = new Dictionary<string, object>();

            foreach (KeyValuePair<string, object> d in values)
            {
                // if (d.Value.GetType().FullName.Contains("Newtonsoft.Json.Linq.JObject"))
                if (d.Value is JObject)
                {
                    values2.Add(d.Key, DeserializeToDictionary(d.Value.ToString()));
                }
                else
                {
                    values2.Add(d.Key, d.Value);
                }
            }
            return values2;
        }

        public string CalculateMD5Hash(string input)
        {
            // step 1, calculate MD5 hash from input
            MD5 md5 = MD5.Create();
            byte[] inputBytes = Encoding.ASCII.GetBytes(input);
            byte[] hash = md5.ComputeHash(inputBytes);

            // step 2, convert byte array to hex string
            StringBuilder sb = new StringBuilder();
            for (int i = 0; i < hash.Length; i++)
            {
                sb.Append(hash[i].ToString("X2"));
            }
            return sb.ToString();
        }

        private void PullSubscriptionStatusChangesFromThirdParty(MpBulkEmailPublication publication)
        {
            var client = GetBulkEmailClient();

            // query mailchimp to get list activity         
            var lastSuccessfulSync = publication.LastSuccessfulSync.ToUniversalTime().ToString("u");
            var request = new RestRequest("lists/" + publication.ThirdPartyPublicationId + "/members?since_last_changed=" + lastSuccessfulSync +
                                          "&fields=members.id,members.email_address,members.status&activity=status&count=" + MAX_SYNC_RECORDS,
                                          Method.GET);
            request.AddHeader("Content-Type", "application/json");

            try
            {
                var response = client.Execute(request);

                if (response.StatusCode != HttpStatusCode.OK)
                {
                    // This will be addressed in US2861: MP/MailChimp Synch Error Handling 
                    // TODO: Should these be exceptions?
                    _logger.ErrorFormat("Http failed syncing opts for publication {0} with StatusCode = {1}", publication.PublicationId, response.StatusCode);
                    return;
                }

                var responseContent = response.Content;
                var responseContentJson = JObject.Parse(responseContent);
                List<BulkEmailSubscriberOptDTO> subscribersDTOs = JsonConvert.DeserializeObject<List<BulkEmailSubscriberOptDTO>>(responseContentJson["members"].ToString());

                SetStatuses(publication, subscribersDTOs);
            }
            catch (Exception ex)
            {
                _logger.ErrorFormat("Opt-in sync code failed for publication {0} Detail: {1}", publication.PublicationId, ex);
            }
        }

        public void SetStatuses(MpBulkEmailPublication publication, List<BulkEmailSubscriberOptDTO> subscribersDTOs)
        {
            List<MpBulkEmailSubscriberOpt> subscriberOpts = new List<MpBulkEmailSubscriberOpt>();

            foreach (var subscriberDTO in subscribersDTOs)
            {
                subscriberDTO.PublicationID = publication.PublicationId;
                subscriberOpts.Add(Mapper.Map<MpBulkEmailSubscriberOpt>(subscriberDTO));   
            }

            foreach (var subscriberOpt in subscriberOpts)
            {
                _logger.InfoFormat("Changing subscription status for {0} to {1}", subscriberOpt.EmailAddress, subscriberOpt.Status);
                _bulkEmailRepository.SetSubscriberStatus(_token, subscriberOpt);
            }
        }
    }
}

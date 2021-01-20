using System;
using System.Collections.Generic;
using System.Linq;
using RestSharp;
using System.Net;
using Crossroads.Utilities.Serializers;
using MPInterfaces = MinistryPlatform.Translation.Repositories.Interfaces;
using System.Security.Cryptography;
using System.Text;
using log4net;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using crds_angular.Models.Crossroads.Subscription;
using crds_angular.Models.MailChimp;
using Crossroads.Web.Common.Configuration;

namespace crds_angular.Util
{
    public class MailchimpListHandler : Interfaces.IEmailListHandler
    {
        private readonly ILog _logger = LogManager.GetLogger(typeof(MailchimpListHandler));
        private readonly IConfigurationWrapper _configWrapper;
        private readonly MPInterfaces.IBulkEmailRepository _bulkEmailRepository;
        private readonly IRestClient _restClient;

        public MailchimpListHandler(IConfigurationWrapper configWrapper, MPInterfaces.IBulkEmailRepository bulkEmailRepository, IRestClient mailchimpRestClient)
        {
            _configWrapper = configWrapper;
            _bulkEmailRepository = bulkEmailRepository;
            _restClient = mailchimpRestClient;
        }

        public OptInResponse AddListSubscriber(string email, string listName, string token)
        {
            try
            {
                var publicationId = GetPublicationId(token, listName);

                // query mailchimp to see if the subscriber is present on the list
                var subscriberStatusRequest = new RestRequest("lists/" + publicationId + "/members/" + CalculateMD5Hash(email), Method.GET);
                subscriberStatusRequest.AddHeader("Content-Type", "application/json");

                var subscriberStatusResponse = _restClient.Execute(subscriberStatusRequest);

                if (subscriberStatusResponse.StatusCode == HttpStatusCode.OK)
                {
                    var responseContent = subscriberStatusResponse.Content;
                    var responseContentJson = JObject.Parse(responseContent);

                    var subscriber = JsonConvert.DeserializeObject<BulkEmailSubscriberOptDTO>(responseContentJson.ToString());
                    
                    if (subscriber.Status == "unsubscribed")
                    {
                        var signupSuccessful = SendSubscriberRequest(publicationId, email);

                        if (signupSuccessful == true)
                        {
                            return new OptInResponse
                            {
                                ErrorInSignupProcess = false,
                                UserAlreadySubscribed = false
                            };
                        }
                    }
                    else if (subscriber.Status == "subscribed" || subscriber.Status == "pending")
                    {
                        return new OptInResponse
                        {
                            ErrorInSignupProcess = false,
                            UserAlreadySubscribed = true
                        };
                    }
                }
                else if (subscriberStatusResponse.StatusCode == HttpStatusCode.NotFound)
                {
                    var signupSuccessful = SendSubscriberRequest(publicationId, email);

                    if (signupSuccessful == true)
                    {
                        return new OptInResponse
                        {
                            ErrorInSignupProcess = false,
                            UserAlreadySubscribed = false
                        };
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.ErrorFormat("Failed adding subscriber {0} for list {1} with exception = {2}", email, listName, ex.ToString());
            }

            // if we don't get the correct response, return an error message
            return new OptInResponse
            {
                ErrorInSignupProcess = true,
                UserAlreadySubscribed = false
            };
        }

        public bool SendSubscriberRequest(string publicationId, string email)
        {
            // create opt in request, if the person isn't already subscribed
            var request = new RestRequest("lists/" + publicationId + "/members/" + CalculateMD5Hash(email));
            request.AddHeader("Content-Type", "application/json");

            request.JsonSerializer = new RestsharpJsonNetSerializer();
            request.RequestFormat = DataFormat.Json;
            request.Method = Method.PUT;

            Dictionary<string, string> requestBody = new Dictionary<string, string>();
            requestBody.Add("email_address", email);
            requestBody.Add("status", "pending");
            request.AddBody(requestBody);

            var response = _restClient.Execute(request);

            if (response.StatusCode == HttpStatusCode.OK)
            {
                return true;
            }

            return false;
        }

        private string GetPublicationId(string token, string publicationName)
        {
            var publications = _bulkEmailRepository.GetPublications(token);
            string publicationId = publications.First(r => r.Title == publicationName).ThirdPartyPublicationId;
            return publicationId;
        }

        private string CalculateMD5Hash(string input)
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

        private Dictionary<string, object> DeserializeToDictionary(string jo)
        {
            var values = JsonConvert.DeserializeObject<Dictionary<string, object>>(jo);
            var values2 = new Dictionary<string, object>();

            foreach (KeyValuePair<string, object> d in values)
            {
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
    }
}
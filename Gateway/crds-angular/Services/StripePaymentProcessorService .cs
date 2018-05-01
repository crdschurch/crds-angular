using System;
using System.Collections.Generic;
using System.Linq;
using crds_angular.Exceptions;
using crds_angular.Services.Interfaces;
using Newtonsoft.Json;
using RestSharp;
using System.Net;
using crds_angular.Models.Crossroads.Stewardship;
using crds_angular.Models.Json;
using Crossroads.Utilities;
using Crossroads.Utilities.Extensions;
using Crossroads.Utilities.Interfaces;
using Crossroads.Utilities.Models;
using Crossroads.Utilities.Services;
using Crossroads.Web.Common;
using Crossroads.Web.Common.Extensions;
using Crossroads.Web.Common.Configuration;
using MinistryPlatform.Translation.Models;
using RestSharp.Extensions;
using log4net;
using Constants = Crossroads.Utilities.Constants;

namespace crds_angular.Services
{
    public class StripePaymentProcessorService : IPaymentProcessorService
    {
        private readonly IRestClient _stripeRestClient;

        private const string StripeCustomerDescription = "Crossroads Donor #{0}";

        private const string StripeNetworkErrorResponseCode = "abort";

        private readonly int _maxQueryResultsPerPage;

        private readonly IContentBlockService _contentBlockService;

        private readonly ILog _logger = LogManager.GetLogger(typeof(StripePaymentProcessorService));

        public StripePaymentProcessorService(IRestClient stripeRestClient, IConfigurationWrapper configuration, IContentBlockService contentBlockService)
        {
            _stripeRestClient = stripeRestClient;
            _maxQueryResultsPerPage = configuration.GetConfigIntValue("MaxStripeQueryResultsPerPage");

            var stripeApiVersion = configuration.GetEnvironmentVarAsString("CRDS_STRIPE_API_VERSION", false);
            if (stripeApiVersion != null)
            {
                _stripeRestClient.AddDefaultHeader("Stripe-Version", stripeApiVersion);
            }

            _contentBlockService = contentBlockService;
        }

        private static bool IsBadResponse(IRestResponse response, bool errorNotFound = false)
        {
            return (response.ResponseStatus != ResponseStatus.Completed
                    || (errorNotFound && response.StatusCode == HttpStatusCode.NotFound)
                    || response.StatusCode == HttpStatusCode.Unauthorized
                    || response.StatusCode == HttpStatusCode.BadRequest
                    || response.StatusCode == HttpStatusCode.PaymentRequired);
        }

        private void CheckStripeResponse(string errorMessage, IRestResponse response, bool errorNotFound = false)
        {
            if (!IsBadResponse(response, errorNotFound))
            {
                return;
            }

            var content = JsonConvert.DeserializeObject<Content>(response.Content);
            if (content == null || content.Error == null)
            {
                throw(AddGlobalErrorMessage(new PaymentProcessorException(HttpStatusCode.InternalServerError, errorMessage, StripeNetworkErrorResponseCode,
                    response.ErrorException?.Message, null, null, null)));
            }
            else
            {
                throw(AddGlobalErrorMessage(new PaymentProcessorException(response.StatusCode, errorMessage, content.Error.Type, content.Error.Message, content.Error.Code, content.Error.DeclineCode, content.Error.Param)));
            }
        }

        private PaymentProcessorException AddGlobalErrorMessage(PaymentProcessorException e)
        {
            // This same logic exists on the Angular side in app/give/services/payment_service.js.
            // This is because of the Stripe "tokens" call, which goes directly to Stripe, not via our API.  We
            // are implementing the same here in the interest of keeping our application somewhat agnostic to
            // the underlying payment processor.
            if ("abort".Equals(e.Type) || "abort".Equals(e.Code))
            {
                e.GlobalMessage = GetContentBlock("paymentMethodProcessingError");
            }
            else if ("card_error".Equals(e.Type))
            {
                if (e.Code != null && ("card_declined".Equals(e.Code) || e.Code.Matches("^incorrect") || e.Code.Matches("^invalid")))
                {
                    e.GlobalMessage = GetContentBlock("paymentMethodDeclined");
                }
                else if ("processing_error".Equals(e.Code))
                {
                    e.GlobalMessage = GetContentBlock("paymentMethodProcessingError");
                }
            }
            else if ("bank_account".Equals(e.Param))
            {
                if ("invalid_request_error".Equals(e.Type))
                {
                    e.GlobalMessage = GetContentBlock("paymentMethodDeclined");
                }
            }

            if (e.GlobalMessage == null)
            {
                e.GlobalMessage = GetContentBlock("failedResponse");
            }

            return (e);
        }

        private ContentBlock GetContentBlock(string key)
        {
            ContentBlock contentBlock;
            if (!_contentBlockService.TryGetValue(key, out contentBlock))
            {
                contentBlock = new ContentBlock()
                {
                    Id = 0,
                    Title = key,
                    Content = key,
                    Type = ContentBlockType.Error,
                    Category =  ""
                };
            }

            return contentBlock;
        }

        public StripeCustomer CreateCustomer(string customerToken, string donorDescription = null, string Email = null, string DisplayName = null)
        {
            var request = new RestRequest("customers", Method.POST);
            request.AddParameter("description", string.Format(StripeCustomerDescription, string.IsNullOrWhiteSpace(donorDescription) ? "pending" : donorDescription));
            if (!string.IsNullOrWhiteSpace(customerToken))
            {
                request.AddParameter("source", customerToken);
            }
            request.AddParameterIfSpecified("metadata[email]", Email, ParameterType.GetOrPost);
            request.AddParameterIfSpecified("metadata[display_name]", DisplayName, ParameterType.GetOrPost);

            var response = _stripeRestClient.Execute<StripeCustomer>(request);
            CheckStripeResponse("Customer creation failed", response);
            
            return response.Data;
        }

        public StripeCustomer GetCustomer(string customerId)
        {
            var request = new RestRequest(string.Format("/customers/{0}", customerId));

            var response = _stripeRestClient.Execute<StripeCustomer>(request);
            CheckStripeResponse("Customer creation failed", response);

            return response.Data;
        }

        public StripeCustomer DeleteCustomer(string customerId)
        {
            var request = new RestRequest(string.Format("/customers/{0}", customerId), Method.DELETE);

            var response = _stripeRestClient.Execute<StripeCustomer>(request);
            CheckStripeResponse("Customer delete failed", response);

            return response.Data;
        }

        public StripeToken CreateToken(string accountNumber, string routingNumber, string accountHolderName)
        {
            var request = new RestRequest("tokens", Method.POST);
            request.AddParameter("bank_account[account_number]", accountNumber);
            request.AddParameter("bank_account[routing_number]", routingNumber);
            request.AddParameter("bank_account[country]", "US");
            request.AddParameter("bank_account[currency]", "USD");
            request.AddParameter("bank_account[account_holder_name]", accountHolderName);
            request.AddParameter("bank_account[account_holder_type]", "individual");

            // TODO Should be able to use request.AddJsonBody here, but that seems to ignore the property annotations
            //request.RequestFormat = DataFormat.Json;
            //request.AddJsonBody(new StripeBankAccount
            //{
            //    AccountNumber = accountNumber,
            //    RoutingNumber = routingNumber,
            //    Country = "US",
            //    Currency = "USD"
            //});

            var response = _stripeRestClient.Execute<StripeToken>(request);
            CheckStripeResponse("Token creation failed", response);

            return (response.Data);
        }

        public SourceData UpdateCustomerSource(string customerToken, string cardToken)
        {
            //Passing source will create a new source object, make it the new customer default source, and delete the old customer default if one exist
            var request = new RestRequest("customers/" + customerToken, Method.POST);
            request.AddParameter("source", cardToken);

            var response = _stripeRestClient.Execute<StripeCustomer>(request);
            CheckStripeResponse("Customer update to add source failed", response);

            var defaultSourceId = response.Data.default_source;
            var sources = response.Data.sources.data;
            var defaultSource = MapDefaultSource(sources, defaultSourceId);
            
            return defaultSource;

        }

        public StripeCustomer AddSourceToCustomer(string customerToken, string cardToken)
        {
            //Passing source will create a new source object, make it the new customer default source, and delete the old customer default if one exist
            var request = new RestRequest("customers/" + customerToken + "/sources", Method.POST);
            request.AddParameter("source", cardToken);

            var response = _stripeRestClient.Execute<StripeCustomer>(request);
            CheckStripeResponse("Customer update to add source failed", response);
            var sourceData = response.Data;

            return sourceData;
        }

        public StripeSubscription CancelSubscription(string customerId, string subscriptionId)
        {
            var request = new RestRequest(string.Format("customers/{0}/subscriptions/{1}", customerId, subscriptionId), Method.DELETE);

            var response = _stripeRestClient.Execute<StripeSubscription>(request);
            CheckStripeResponse("Stripe Subscription Cancel failed", response, true);

            return (response.Data);
        }

        public StripePlan CancelPlan(string planId)
        {
            // We need to replace "/" with the URL-encoded "%2F" b/c our plan IDs have slashes in them, but this is
            // part of the URI, which means it will not work properly if not encoded.
            // For example: "2015344 10/13/2015 10:57:17"
            var request = new RestRequest(string.Format("plans/{0}", planId.Replace("/", "%2F")), Method.DELETE);

            var response = _stripeRestClient.Execute<StripePlan>(request);
            CheckStripeResponse("Stripe Plan Cancel failed", response);

            return (response.Data);
        }

        public string UpdateCustomerDescription(string customerToken, int donorId)
        {
            var request = new RestRequest("customers/" + customerToken, Method.POST);
            request.AddParameter("description", string.Format(StripeCustomerDescription, donorId));

            var response = _stripeRestClient.Execute<StripeCustomer>(request);
            CheckStripeResponse("Customer update failed", response);

            return (response.Data.id);
        }

        public SourceData GetDefaultSource(string customerToken)
        {
            var request = new RestRequest("customers/" + customerToken, Method.GET);

            var response = _stripeRestClient.Execute<StripeCustomer>(request);
            CheckStripeResponse("Could not get default source information because customer lookup failed", response);

            var defaultSourceId = response.Data.default_source;
            var sources = response.Data.sources.data;
            var defaultSource = MapDefaultSource(sources, defaultSourceId);

            return defaultSource;
        }

        public SourceData GetSource(string customerId, string sourceId)
        {
            var customer = GetCustomer(customerId);
            if (customer.sources == null || customer.sources.data == null || !customer.sources.data.Any())
            {
                return (null);
            }

            return (customer.sources.data.Find(src => src.id.Equals(sourceId)));
        }

        private static SourceData MapDefaultSource(List<SourceData>sources, string defaultSourceId)
        {
            var defaultSource = new SourceData();

            foreach (var source in sources.Where(source => source.id == defaultSourceId))
            {
                defaultSource.@object = source.@object;
                if (source.@object == "bank_account")
                {
                    defaultSource.routing_number = source.routing_number;
                    defaultSource.bank_last4 = source.last4;
                    defaultSource.account_holder_name = source.account_holder_name;
                    defaultSource.account_holder_type = source.account_holder_type;
                }
                else
                {
                    defaultSource.brand = source.brand;
                    defaultSource.last4 = source.last4;
                    defaultSource.address_zip = source.address_zip;
                    defaultSource.exp_month = source.exp_month.PadLeft(2, '0');
                    defaultSource.exp_year = source.exp_year.Substring(2, 2);
                }
                defaultSource.id = defaultSourceId;
            }

            return defaultSource;
        }

        public StripeCharge ChargeCustomer(string customerToken, decimal amount, int donorId, bool isPayment, string Email= null, string DisplayName = null)
        {
            var request = new RestRequest("charges", Method.POST);
            request.AddParameter("amount", (int)(amount * Constants.StripeDecimalConversionValue));
            request.AddParameter("currency", "usd");
            request.AddParameter("customer", customerToken);
            request.AddParameter("description", "Donor ID #" + donorId);
            request.AddParameter("expand[]", "balance_transaction");

            request.AddParameter("metadata[crossroads_transaction_type]", isPayment ? "payment" : "donation");

            request.AddParameterIfSpecified("metadata[email]", Email, ParameterType.GetOrPost);
            request.AddParameterIfSpecified("metadata[display_name]", DisplayName, ParameterType.GetOrPost);

            var response = _stripeRestClient.Execute<StripeCharge>(request);
            CheckStripeResponse("Invalid charge request", response, true);

            return response.Data;
        }

        public StripeCharge ChargeCustomer(string customerToken, string customerSourceId, decimal amount, int donorId, string checkNumber, string Email = null, string DisplayName = null)
        {
            var request = new RestRequest("charges", Method.POST);
            request.AddParameter("amount",(int)(amount * Constants.StripeDecimalConversionValue));
            request.AddParameter("currency", "usd");
            request.AddParameter("customer", customerToken);
            request.AddParameter("source", customerSourceId);
            request.AddParameter("description", "Donor ID #" + donorId);
            request.AddParameter("expand[]", "balance_transaction");
            request.AddParameter("statement_descriptor", string.Format("CK{0} CONVERTED", (checkNumber ?? string.Empty).TrimStart(' ', '0').Right(5)));

            request.AddParameterIfSpecified("metadata[email]", Email, ParameterType.GetOrPost);
            request.AddParameterIfSpecified("metadata[display_name]", DisplayName, ParameterType.GetOrPost);

            var response = _stripeRestClient.Execute<StripeCharge>(request);
            CheckStripeResponse("Invalid charge request", response, true);

            return response.Data;
        }

        public List<StripeCharge> GetChargesForTransfer(string transferId)
        {
            var url = $"transfers/{transferId}/transactions";
            var request = new RestRequest(url, Method.GET);
            request.AddParameter("count", _maxQueryResultsPerPage);

            var charges = new List<StripeCharge>();
            StripeCharges nextPage;
            do
            {
                var response = _stripeRestClient.Execute<StripeCharges>(request);
                CheckStripeResponse("Could not query transactions", response);

                nextPage = response.Data;
                charges.AddRange(nextPage.Data.Select(charge => charge));

                request = new RestRequest(url, Method.GET);
                request.AddParameter("count", _maxQueryResultsPerPage);
                request.AddParameter("starting_after", charges.Last().Id);
            } while (nextPage.HasMore);

            //get the metadata for all of the charges
            for (int i = charges.Count - 1; i >= 0; --i)
            {
                StripeCharge charge = charges[i];

                // don't let a failure to retrieve data for one charge stop the entire batch
                try
                {
                    if (charge.Type == "payment" || charge.Type == "charge")
                    {
                        var singlecharge = GetCharge(charge.Id);
                        charge.Metadata = singlecharge.Metadata;
                    }
                    else if (charge.Type == "payment_refund") //its a bank account refund
                    {
                        var singlerefund = GetRefund(charge.Id);
                        charge.Metadata = singlerefund.Charge.Metadata;
                    }
                    else // if charge.Type == "refund", it's a credit card charge refund
                    {
                        var singlerefund = GetChargeRefund(charge.Id);
                        charge.Metadata = singlerefund.Data[0].Charge.Metadata;
                    }
                }
                catch (Exception e)
                {
                    // remove from the batch and keep going; the batch will be out of balance, but thats Ok
                    _logger.Error($"GetChargesForTransfer error retrieving metadata for {charge.Type} {charge.Id}", e);
                    charges.RemoveAt(i);
                }
            }

            return (charges);
        }

        public StripeRefund GetChargeRefund(string chargeId)
        {
            var url = string.Format("charges/{0}/refunds", chargeId);
            var request = new RestRequest(url, Method.GET);
            request.AddParameter("expand[]", "data.balance_transaction");
            request.AddParameter("expand[]", "data.charge");

            var response = _stripeRestClient.Execute(request);
            CheckStripeResponse("Could not query charge refund", response);

            // TODO Execute<StripeRefund>() above always gets an error deserializing the response, so using Execute() instead, and manually deserializing here
            var refund = JsonConvert.DeserializeObject<StripeRefund>(response.Content);
            
            return (refund);
        }

        public StripeCharge GetCharge(string chargeId)
        {
            var url = string.Format("charges/{0}", chargeId);
            var request = new RestRequest(url, Method.GET);
            request.AddParameter("expand[]", "balance_transaction");
            request.AddParameter("expand[]", "invoice");

            var response = _stripeRestClient.Execute(request);
            CheckStripeResponse("Could not query charge", response);

            // TODO Execute<StripeCharge>() above always gets an error deserializing the response, so using Execute() instead, and manually deserializing here
            var charge = JsonConvert.DeserializeObject<StripeCharge>(response.Content);

            return (charge);
        }

        public StripeRefundData GetRefund(string refundId)
        {
            var url = string.Format("refunds/{0}", refundId);
            var request = new RestRequest(url, Method.GET);
            request.AddParameter("expand[]", "charge");
            request.AddParameter("expand[]", "balance_transaction");

            var response = _stripeRestClient.Execute(request);
            CheckStripeResponse("Could not query refund", response);

            // TODO Execute<StripeRefundData>() above always gets an error deserializing the response, so using Execute() instead, and manually deserializing here
            var refund = JsonConvert.DeserializeObject<StripeRefundData>(response.Content);

            return refund;
        }

        public StripePlan CreatePlan(RecurringGiftDto recurringGiftDto, MpContactDonor mpContactDonor, string Email = null, string DisplayName = null)
        {
            var request = new RestRequest("plans", Method.POST);

            var interval = EnumMemberSerializationUtils.ToEnumString(recurringGiftDto.PlanInterval);

            request.AddParameter("amount", (int)(recurringGiftDto.PlanAmount * Constants.StripeDecimalConversionValue));
            request.AddParameter("interval", interval);
            request.AddParameter("name", string.Format("Donor ID #{0} {1}ly", mpContactDonor.DonorId, interval));
            request.AddParameter("currency", "usd");
            request.AddParameter("id", mpContactDonor.DonorId + " " + DateTime.Now);

            request.AddParameterIfSpecified("metadata[email]", Email, ParameterType.GetOrPost);
            request.AddParameterIfSpecified("metadata[display_name]", DisplayName, ParameterType.GetOrPost);

            var response = _stripeRestClient.Execute<StripePlan>(request);
            CheckStripeResponse("Invalid plan creation request", response);

            return response.Data;
        }

        public StripeSubscription CreateSubscription(string planName, string customer, DateTime trialEndDate, string Email = null, string DisplayName = null)
        {
            var request = new RestRequest("customers/" + customer +"/subscriptions", Method.POST);
            request.AddParameter("plan", planName);
            if (trialEndDate.ToUniversalTime().Date > DateTime.UtcNow.Date)
            {
                request.AddParameter("trial_end", trialEndDate.ToUniversalTime().Date.AddHours(12).ConvertDateTimeToEpoch());
            }

            request.AddParameterIfSpecified("metadata[email]", Email, ParameterType.GetOrPost);
            request.AddParameterIfSpecified("metadata[display_name]", DisplayName, ParameterType.GetOrPost);


            var response = _stripeRestClient.Execute<StripeSubscription>(request);
            CheckStripeResponse("Invalid subscription creation request", response);

            return response.Data;
        }

        public StripeSubscription GetSubscription(string customerId, string subscriptionId)
        {
            var request = new RestRequest(string.Format("customers/{0}/subscriptions/{1}", customerId, subscriptionId), Method.GET);

            var response = _stripeRestClient.Execute<StripeSubscription>(request);
            CheckStripeResponse("Invalid subscription get request", response);

            return response.Data;
        }

        public StripeSubscription UpdateSubscriptionPlan(string customerId, string subscriptionId, string planId, DateTime? trialEndDate = null)
        {
            var request = new RestRequest(string.Format("customers/{0}/subscriptions/{1}", customerId, subscriptionId), Method.POST);
            request.AddParameter("prorate", false);
            request.AddParameter("plan", planId);
            if (trialEndDate != null && trialEndDate.Value.ToUniversalTime().Date > DateTime.UtcNow.Date)
            {
                request.AddParameter("trial_end", trialEndDate.Value.ToUniversalTime().Date.AddHours(12).ConvertDateTimeToEpoch());
            }

            var response = _stripeRestClient.Execute<StripeSubscription>(request);
            CheckStripeResponse("Invalid subscription update request", response);

            return response.Data;
        }

        public StripeSubscription UpdateSubscriptionTrialEnd(string customerId, string subscriptionId, DateTime? trialEndDate)
        {
            var request = new RestRequest(string.Format("customers/{0}/subscriptions/{1}", customerId, subscriptionId), Method.POST);
            request.AddParameter("prorate", false);
            if (trialEndDate != null && trialEndDate.Value.ToUniversalTime().Date > DateTime.UtcNow.Date)
            {
                request.AddParameter("trial_end", trialEndDate.Value.ToUniversalTime().Date.AddHours(12).ConvertDateTimeToEpoch());
            }
            else
            {
                request.AddParameter("trial_end", "now");
            }

            var response = _stripeRestClient.Execute<StripeSubscription>(request);
            CheckStripeResponse("Invalid subscription update request", response);

            return response.Data;
        }
    }

    public class Error
    {
        [JsonProperty(PropertyName = "type")]
        public string Type { get; set; }
        [JsonProperty(PropertyName = "message")]
        public string Message { get; set; }
        [JsonProperty(PropertyName = "param")]
        public string Param { get; set; }
        [JsonProperty(PropertyName = "code")]
        public string Code { get; set; }
        [JsonProperty(PropertyName = "decline_code")]
        public string DeclineCode { get; set; }
    }

    public class Content
    {
        public Error Error { get; set; }
    }
}

using System;
using crds_angular.Exceptions;
using crds_angular.Services;
using Moq;
using NUnit.Framework;
using RestSharp;
using System.Collections.Generic;
using System.Net;
using crds_angular.Models.Crossroads.Stewardship;
using crds_angular.Models.Json;
using Crossroads.Utilities;
using Crossroads.Utilities.Interfaces;
using Crossroads.Utilities.Models;
using Crossroads.Utilities.Services;
using Crossroads.Web.Common;
using Crossroads.Web.Common.Configuration;
using MinistryPlatform.Translation.Models;
using Newtonsoft.Json;
using Constants = Crossroads.Utilities.Constants;

namespace crds_angular.test.Services
{
    public class StripeServiceTest
    {
        private Mock<IRestClient> _restClient;
        private Mock<IConfigurationWrapper> _configuration;
        private Mock<IContentBlockService> _contentBlockService;
        private StripePaymentProcessorService _fixture;
        private Dictionary<string, ContentBlock> _errors;

        [SetUp]
        public void Setup()
        {
            _errors = new Dictionary<string, ContentBlock>
            {
                {"paymentMethodProcessingError", new ContentBlock { Id = 123 }},
                {"paymentMethodDeclined", new ContentBlock { Id = 456 }},
                {"failedResponse", new ContentBlock { Id = 789 }},
            };

            _restClient = new Mock<IRestClient>(MockBehavior.Strict);
            _configuration = new Mock<IConfigurationWrapper>();
            _configuration.Setup(mocked => mocked.GetConfigIntValue("MaxStripeQueryResultsPerPage")).Returns(42);

            _contentBlockService = new Mock<IContentBlockService>();
            foreach (var e in _errors)
            {
                var e1 = e;
                var value = e1.Value;
                _contentBlockService.SetupGet(mocked => mocked[e1.Key]).Returns(e1.Value);
                _contentBlockService.Setup<bool>(mocked => mocked.TryGetValue(e1.Key, out value)).Returns(true);
            }

            _fixture = new StripePaymentProcessorService(_restClient.Object, _configuration.Object, _contentBlockService.Object);
        }

        [Test]
        public void ShouldGetChargesForTransfer()
        {
            var q = new Queue<StripeCharges>();
            q.Enqueue(new StripeCharges
            {
                HasMore = true,
                Data = new List<StripeCharge>
                {
                    new StripeCharge
                    {
                        Id = "123",
                        Type = "payment_refund"
                    },
                    new StripeCharge
                    {
                        Id = "last_one_in_first_page",
                        Type = "payment"
                    }
                }
            });
            q.Enqueue(new StripeCharges
            {
                HasMore = false,
                Data = new List<StripeCharge>
                {
                    new StripeCharge
                    {
                        Id = "789",
                        Type = "payment"
                    },
                    new StripeCharge
                    {
                        Id = "90210",
                        Type = "payment"
                    }
                }
            });


            var mockContent =
                "{\n  \"id\": \"py_19HJIeDpgPmDp9CAlNl1cvCO\",\n  \"object\": \"charge\",\n  \"amount\": 4300,\n  \"amount_refunded\": 0,\n  \"application\": null,\n  \"application_fee\": null,\n  \"balance_transaction\": {\n    \"id\": \"txn_19HJIeDpgPmDp9CAlCQlXSbu\",\n    \"object\": \"balance_transaction\",\n    \"amount\": 4300,\n    \"available_on\": 1479502172,\n    \"created\": 1479502172,\n    \"currency\": \"usd\",\n    \"description\": null,\n    \"fee\": 25,\n    \"fee_details\": [\n      {\n        \"amount\": 25,\n        \"application\": null,\n        \"currency\": \"usd\",\n        \"description\": \"Stripe processing fees\",\n        \"type\": \"stripe_fee\"\n      }\n    ],\n    \"net\": 4275,\n    \"source\": \"py_19HJIeDpgPmDp9CAlNl1cvCO\",\n    \"sourced_transfers\": {\n      \"object\": \"list\",\n      \"data\": [],\n      \"has_more\": false,\n      \"total_count\": 0,\n      \"url\": \"/v1/transfers?source_transaction=py_19HJIeDpgPmDp9CAlNl1cvCO\"\n    },\n    \"status\": \"available\",\n    \"type\": \"payment\"\n  },\n  \"captured\": true,\n  \"created\": 1479502172,\n  \"currency\": \"usd\",\n  \"customer\": \"cus_9SbXBcUvgoadTL\",\n  \"description\": null,\n  \"destination\": null,\n  \"dispute\": null,\n  \"failure_code\": null,\n  \"failure_message\": null,\n  \"fraud_details\": {},\n  \"invoice\": {\n    \"id\": \"in_19HILHDpgPmDp9CAzg0WAqI0\",\n    \"object\": \"invoice\",\n    \"amount_due\": 4300,\n    \"application_fee\": null,\n    \"attempt_count\": 0,\n    \"attempted\": true,\n    \"charge\": \"py_19HJIeDpgPmDp9CAlNl1cvCO\",\n    \"closed\": true,\n    \"currency\": \"usd\",\n    \"customer\": \"cus_9SbXBcUvgoadTL\",\n    \"date\": 1479498491,\n    \"description\": null,\n    \"discount\": null,\n    \"ending_balance\": 0,\n    \"forgiven\": false,\n    \"lines\": {\n      \"object\": \"list\",\n      \"data\": [\n        {\n          \"id\": \"sub_9SbXhYxWHDv5JP\",\n          \"object\": \"line_item\",\n          \"amount\": 4300,\n          \"currency\": \"usd\",\n          \"description\": null,\n          \"discountable\": true,\n          \"livemode\": false,\n          \"metadata\": {},\n          \"period\": {\n            \"start\": 1479498480,\n            \"end\": 1480103280\n          },\n          \"plan\": {\n            \"id\": \"7727750 10/28/2016 3:48:00 PM\",\n            \"object\": \"plan\",\n            \"amount\": 4300,\n            \"created\": 1477684080,\n            \"currency\": \"usd\",\n            \"interval\": \"week\",\n            \"interval_count\": 1,\n            \"livemode\": false,\n            \"metadata\": {},\n            \"name\": \"Donor ID #7727750 weekly\",\n            \"statement_descriptor\": null,\n            \"trial_period_days\": null\n          },\n          \"proration\": false,\n          \"quantity\": 1,\n          \"subscription\": null,\n          \"type\": \"subscription\"\n        }\n      ],\n      \"has_more\": false,\n      \"total_count\": 1,\n      \"url\": \"/v1/invoices/in_19HILHDpgPmDp9CAzg0WAqI0/lines\"\n    },\n    \"livemode\": false,\n    \"metadata\": {},\n    \"next_payment_attempt\": null,\n    \"paid\": true,\n    \"period_end\": 1479498480,\n    \"period_start\": 1478893680,\n    \"receipt_number\": null,\n    \"starting_balance\": 0,\n    \"statement_descriptor\": null,\n    \"subscription\": \"sub_9SbXhYxWHDv5JP\",\n    \"subtotal\": 4300,\n    \"tax\": null,\n    \"tax_percent\": null,\n    \"total\": 4300,\n    \"webhooks_delivered_at\": 1479498502,\n    \"payment\": \"py_19HJIeDpgPmDp9CAlNl1cvCO\"\n  },\n  \"livemode\": false,\n  \"metadata\": {},\n  \"order\": null,\n  \"outcome\": {\n    \"network_status\": \"approved_by_network\",\n    \"reason\": null,\n    \"risk_level\": \"not_assessed\",\n    \"seller_message\": \"Payment complete.\",\n    \"type\": \"authorized\"\n  },\n  \"paid\": true,\n  \"receipt_email\": null,\n  \"receipt_number\": null,\n  \"refunded\": false,\n  \"refunds\": {\n    \"object\": \"list\",\n    \"data\": [],\n    \"has_more\": false,\n    \"total_count\": 0,\n    \"url\": \"/v1/charges/py_19HJIeDpgPmDp9CAlNl1cvCO/refunds\"\n  },\n  \"review\": null,\n  \"shipping\": null,\n  \"source\": {\n    \"id\": \"ba_199gKZDpgPmDp9CAnsQEZkKG\",\n    \"object\": \"bank_account\",\n    \"account_holder_name\": \"asdf\",\n    \"account_holder_type\": \"individual\",\n    \"bank_name\": \"STRIPE TEST BANK\",\n    \"country\": \"US\",\n    \"currency\": \"usd\",\n    \"customer\": \"cus_9SbXBcUvgoadTL\",\n    \"fingerprint\": \"NV5JYQIEEtmue1aC\",\n    \"last4\": \"6789\",\n    \"metadata\": {},\n    \"routing_number\": \"110000000\",\n    \"status\": \"verified\",\n    \"name\": \"asdf\"\n  },\n  \"source_transfer\": null,\n  \"statement_descriptor\": null,\n  \"status\": \"succeeded\"\n}\n";


            var mockContent2 =
                "{\n  \"id\": \"pyr_19HGYqDpgPmDp9CAmN2rtyf8\",\n  \"object\": \"refund\",\n  \"amount\": 100,\n  \"balance_transaction\": {\n    \"id\": \"txn_19HGYqDpgPmDp9CA4znriuIy\",\n    \"object\": \"balance_transaction\",\n    \"amount\": -100,\n    \"available_on\": 1479427200,\n    \"created\": 1479491644,\n    \"currency\": \"usd\",\n    \"description\": \"REFUND FOR FAILED PAYMENT (Donor ID #7727905)\",\n    \"fee\": 75,\n    \"fee_details\": [\n      {\n        \"amount\": 75,\n        \"application\": null,\n        \"currency\": \"usd\",\n        \"description\": \"Stripe processing fees\",\n        \"type\": \"stripe_fee\"\n      }\n    ],\n    \"net\": -175,\n    \"source\": \"py_19HGYpDpgPmDp9CAwifxCprE\",\n    \"sourced_transfers\": {\n      \"object\": \"list\",\n      \"data\": [],\n      \"has_more\": false,\n      \"total_count\": 0,\n      \"url\": \"/v1/transfers?source_transaction=pyr_19HGYqDpgPmDp9CAmN2rtyf8\"\n    },\n    \"status\": \"available\",\n    \"type\": \"payment_failure_refund\"\n  },\n  \"charge\": {\n    \"id\": \"py_19HGYpDpgPmDp9CAwifxCprE\",\n    \"object\": \"charge\",\n    \"amount\": 100,\n    \"amount_refunded\": 100,\n    \"application\": null,\n    \"application_fee\": null,\n    \"balance_transaction\": \"txn_19HGYpDpgPmDp9CABiCHm768\",\n    \"captured\": true,\n    \"created\": 1479491643,\n    \"currency\": \"usd\",\n    \"customer\": \"cus_9UmcupMgl6tOk9\",\n    \"description\": \"Donor ID #7727905\",\n    \"destination\": null,\n    \"dispute\": null,\n    \"failure_code\": \"account_closed\",\n    \"failure_message\": \"The customer's bank account has been closed.\",\n    \"fraud_details\": {},\n    \"invoice\": null,\n    \"livemode\": false,\n    \"metadata\": {\n      \"crossroads_transaction_type\": \"payment\"\n    },\n    \"order\": null,\n    \"outcome\": {\n      \"network_status\": \"approved_by_network\",\n      \"reason\": null,\n      \"risk_level\": \"not_assessed\",\n      \"seller_message\": \"Payment complete.\",\n      \"type\": \"authorized\"\n    },\n    \"paid\": false,\n    \"receipt_email\": null,\n    \"receipt_number\": null,\n    \"refunded\": true,\n    \"refunds\": {\n      \"object\": \"list\",\n      \"data\": [\n        {\n          \"id\": \"pyr_19HGYqDpgPmDp9CAmN2rtyf8\",\n          \"object\": \"refund\",\n          \"amount\": 100,\n          \"balance_transaction\": \"txn_19HGYqDpgPmDp9CA4znriuIy\",\n          \"charge\": \"py_19HGYpDpgPmDp9CAwifxCprE\",\n          \"created\": 1479491644,\n          \"currency\": \"usd\",\n          \"description\": \"Payment failure refund\",\n          \"metadata\": {},\n          \"reason\": null,\n          \"receipt_number\": null,\n          \"status\": \"pending\"\n        }\n      ],\n      \"has_more\": false,\n      \"total_count\": 1,\n      \"url\": \"/v1/charges/py_19HGYpDpgPmDp9CAwifxCprE/refunds\"\n    },\n    \"review\": null,\n    \"shipping\": null,\n    \"source\": {\n      \"id\": \"ba_19HEUNDpgPmDp9CAh83NHKUY\",\n      \"object\": \"bank_account\",\n      \"account_holder_name\": \"Sara\",\n      \"account_holder_type\": \"individual\",\n      \"bank_name\": \"STRIPE TEST BANK\",\n      \"country\": \"US\",\n      \"currency\": \"usd\",\n      \"customer\": \"cus_9UmcupMgl6tOk9\",\n      \"fingerprint\": \"U5XWN1RP7sNZyG10\",\n      \"last4\": \"1113\",\n      \"metadata\": {},\n      \"routing_number\": \"110000000\",\n      \"status\": \"verified\",\n      \"name\": \"Sara\"\n    },\n    \"source_transfer\": null,\n    \"statement_descriptor\": null,\n    \"status\": \"failed\"\n  },\n  \"created\": 1479491644,\n  \"currency\": \"usd\",\n  \"description\": \"Payment failure refund\",\n  \"metadata\": {},\n  \"reason\": null,\n  \"receipt_number\": null,\n  \"status\": \"pending\"\n}\n";

            var r = new Mock<IRestResponse<StripeCharge>>();
            r.SetupGet(mocked => mocked.ResponseStatus).Returns(ResponseStatus.Completed).Verifiable();
            r.SetupGet(mocked => mocked.StatusCode).Returns(HttpStatusCode.OK).Verifiable();
            r.SetupGet(mocked => mocked.Content).Returns(mockContent2);
           

            var response = new Mock<IRestResponse<StripeCharges>>();
            response.SetupGet(mocked => mocked.ResponseStatus).Returns(ResponseStatus.Completed).Verifiable();
            response.SetupGet(mocked => mocked.StatusCode).Returns(HttpStatusCode.OK).Verifiable();
            response.SetupGet(mocked => mocked.Data).Returns(() => (q.Dequeue())).Verifiable();

            _restClient.Setup(mocked => mocked.Execute<StripeCharges>(It.IsAny<IRestRequest>())).Returns(response.Object);

            _restClient.Setup(m => m.Execute(It.IsAny<RestRequest>())).Returns(r.Object);


            var charges = _fixture.GetChargesForTransfer("tx123");
            _restClient.Verify(mocked => mocked.Execute<StripeCharges>(
                It.Is<RestRequest>(o =>
                    o.Method == Method.GET
                    && o.Resource.Equals("transfers/tx123/transactions")
                    && o.Parameters.Matches("count", 42)
            )));
            _restClient.Verify(mocked => mocked.Execute<StripeCharges>(
                It.Is<RestRequest>(o =>
                    o.Method == Method.GET
                    && o.Resource.Equals("transfers/tx123/transactions")
                    && o.Parameters.Matches("count", 42)
                    && o.Parameters.Matches("starting_after", "last_one_in_first_page")
            )));
            _restClient.VerifyAll();
            response.VerifyAll();

            Assert.IsNotNull(charges);
            Assert.AreEqual(4, charges.Count);
            Assert.AreEqual("123", charges[0].Id);
            Assert.AreEqual("last_one_in_first_page", charges[1].Id);
            Assert.AreEqual("789", charges[2].Id);
            Assert.AreEqual("90210", charges[3].Id);
        }

        [Test]
        public void ShouldGetDefaultSource()
        {
            var cust = new StripeCustomer
            {
                sources = new Sources
                {
                    data = new List<SourceData>
                    {
                        new SourceData
                        {
                            id = "456",
                            @object = "bank_account",
                            last4 = "9876",
                            routing_number = "5432",
                        },
                        new SourceData
                        {
                            id = "123",
                            @object = "bank_account",
                            last4 = "1234",
                            routing_number = "5678",
                        },
                        new SourceData
                        {
                            id = "789",
                            @object = "credit_card",
                            brand = "visa",
                            last4 = "0001",
                            exp_month = "01",
                            exp_year = "2023",
                            address_zip = "20202"
                        },
                        new SourceData
                        {
                            id = "123",
                            @object = "credit_card",
                            brand = "mcc",
                            last4 = "0002",
                            exp_month = "2",
                            exp_year = "2024",
                            address_zip = "10101"
                        },
                    }
                },
                default_source = "123",
            };
            var stripeResponse = new Mock<IRestResponse<StripeCustomer>>(MockBehavior.Strict);
            stripeResponse.SetupGet(mocked => mocked.ResponseStatus).Returns(ResponseStatus.Completed).Verifiable();
            stripeResponse.SetupGet(mocked => mocked.StatusCode).Returns(HttpStatusCode.OK).Verifiable();
            stripeResponse.SetupGet(mocked => mocked.Data).Returns(cust).Verifiable();
            _restClient.Setup(mocked => mocked.Execute<StripeCustomer>(It.IsAny<IRestRequest>())).Returns(stripeResponse.Object);

            var defaultSource = _fixture.GetDefaultSource("token");
            Assert.IsNotNull(defaultSource);

            Assert.AreEqual("5678", defaultSource.routing_number);
            Assert.AreEqual("1234", defaultSource.bank_last4);

            Assert.AreEqual("mcc", defaultSource.brand);
            Assert.AreEqual("0002", defaultSource.last4);
            Assert.AreEqual("02", defaultSource.exp_month);
            Assert.AreEqual("24", defaultSource.exp_year);
            Assert.AreEqual("10101", defaultSource.address_zip);

            _restClient.VerifyAll();
            stripeResponse.VerifyAll();
        }

        [Test]
        public void ShouldThrowExceptionWhenTokenIsInvalid()
        {
            var stripeResponse = new Mock<IRestResponse<StripeCustomer>>(MockBehavior.Strict);
            stripeResponse.SetupGet(mocked => mocked.ResponseStatus).Returns(ResponseStatus.Completed).Verifiable();
            stripeResponse.SetupGet(mocked => mocked.StatusCode).Returns(HttpStatusCode.BadRequest).Verifiable();
            stripeResponse.SetupGet(mocked => mocked.Content).Returns("{error: {message:'Bad Request'}}").Verifiable();
            _restClient.Setup(mocked => mocked.Execute<StripeCustomer>(It.IsAny<IRestRequest>())).Returns(stripeResponse.Object);

            Assert.Throws<PaymentProcessorException>(() => _fixture.CreateCustomer("token"));

            _restClient.Verify(mocked => mocked.Execute<StripeCustomer>(
                It.Is<RestRequest>(o =>
                    o.Method == Method.POST
                    && o.Resource.Equals("customers")
                    && o.Parameters.Matches("description", "Crossroads Donor #pending")
                    && o.Parameters.Matches("source", "token")
                    )));
            _restClient.VerifyAll();
            stripeResponse.VerifyAll();
        }

        [Test]
        public void ShouldThrowAbortExceptionWhenStripeConnectionFails()
        {
            var stripeResponse = new Mock<IRestResponse<StripeCustomer>>(MockBehavior.Strict);
            stripeResponse.SetupGet(mocked => mocked.ResponseStatus).Returns(ResponseStatus.Completed).Verifiable();
            stripeResponse.SetupGet(mocked => mocked.StatusCode).Returns(HttpStatusCode.BadRequest).Verifiable();
            stripeResponse.SetupGet(mocked => mocked.Content).Returns("{}").Verifiable();
            stripeResponse.SetupGet(mocked => mocked.ErrorException).Returns(new Exception("Doh!")).Verifiable();
            _restClient.Setup(mocked => mocked.Execute<StripeCustomer>(It.IsAny<IRestRequest>())).Returns(stripeResponse.Object);

            try
            {
                _fixture.CreateCustomer("token");
                Assert.Fail("Expected exception was not thrown");
            }
            catch (PaymentProcessorException e)
            {
                Assert.AreEqual("abort", e.Type);
                Assert.AreEqual("Doh!", e.DetailMessage);
                Assert.AreEqual(HttpStatusCode.InternalServerError, e.StatusCode);
                Assert.AreEqual(_errors["paymentMethodProcessingError"], e.GlobalMessage);
            }


        }

        [Test]
        public void ShouldReturnSuccessfulCustomerId()
        {
            var customer = new StripeCustomer
            {
                id = "856",
                default_source = "123",
            };

            var stripeResponse = new Mock<IRestResponse<StripeCustomer>>(MockBehavior.Strict);
            stripeResponse.SetupGet(mocked => mocked.ResponseStatus).Returns(ResponseStatus.Completed).Verifiable();
            stripeResponse.SetupGet(mocked => mocked.StatusCode).Returns(HttpStatusCode.OK).Verifiable();
            stripeResponse.SetupGet(mocked => mocked.Data).Returns(customer).Verifiable();

            _restClient.Setup(mocked => mocked.Execute<StripeCustomer>(It.IsAny<IRestRequest>())).Returns(stripeResponse.Object);

            var response = _fixture.CreateCustomer("token");
            _restClient.Verify(mocked => mocked.Execute<StripeCustomer>(
                It.Is<IRestRequest>(o =>
                    o.Method == Method.POST
                    && o.Resource.Equals("customers")
                    && o.Parameters.Matches("description", "Crossroads Donor #pending")
                    && o.Parameters.Matches("source", "token")
                    )));
            _restClient.VerifyAll();
            stripeResponse.VerifyAll();

            Assert.AreEqual(customer, response);
        }

        [Test]
        public void ShouldCreateCustomerWithoutToken()
        {
            var customer = new StripeCustomer
            {
                id = "856",
                default_source = "123",
            };

            var stripeResponse = new Mock<IRestResponse<StripeCustomer>>(MockBehavior.Strict);
            stripeResponse.SetupGet(mocked => mocked.ResponseStatus).Returns(ResponseStatus.Completed).Verifiable();
            stripeResponse.SetupGet(mocked => mocked.StatusCode).Returns(HttpStatusCode.OK).Verifiable();
            stripeResponse.SetupGet(mocked => mocked.Data).Returns(customer).Verifiable();

            _restClient.Setup(mocked => mocked.Execute<StripeCustomer>(It.IsAny<IRestRequest>())).Returns(stripeResponse.Object);

            var response = _fixture.CreateCustomer(null);
            _restClient.Verify(mocked => mocked.Execute<StripeCustomer>(
                It.Is<IRestRequest>(o =>
                    o.Method == Method.POST
                    && o.Resource.Equals("customers")
                    && o.Parameters.Matches("description", "Crossroads Donor #pending")
                    && !o.Parameters.Contains("source")
                    )));
            _restClient.VerifyAll();
            stripeResponse.VerifyAll();

            Assert.AreEqual(customer, response);
        }

        [Test]
        public void ShouldUpdateCustomerDescription()
        {
            var customer = new StripeCustomer
            {
                id = "12345"
            };

            var stripeResponse = new Mock<IRestResponse<StripeCustomer>>(MockBehavior.Strict);
            stripeResponse.SetupGet(mocked => mocked.ResponseStatus).Returns(ResponseStatus.Completed).Verifiable();
            stripeResponse.SetupGet(mocked => mocked.StatusCode).Returns(HttpStatusCode.OK).Verifiable();
            stripeResponse.SetupGet(mocked => mocked.Data).Returns(customer).Verifiable();

            _restClient.Setup(mocked => mocked.Execute<StripeCustomer>(It.IsAny<IRestRequest>())).Returns(stripeResponse.Object);

            var response = _fixture.UpdateCustomerDescription("token", 102030);
            _restClient.Verify(mocked => mocked.Execute<StripeCustomer>(
                It.Is<IRestRequest>(o =>
                    o.Method == Method.POST
                    && o.Resource.Equals("customers/token")
                    && o.Parameters.Matches("description", "Crossroads Donor #102030")
                    )));
            _restClient.VerifyAll();
            stripeResponse.VerifyAll();

            Assert.AreEqual("12345", response);
        }

        [Test]
        public void ShouldThrowExceptionWhenCustomerUpdateFails()
        {
            var stripeResponse = new Mock<IRestResponse<StripeCustomer>>(MockBehavior.Strict);
            stripeResponse.SetupGet(mocked => mocked.ResponseStatus).Returns(ResponseStatus.Completed).Verifiable();
            stripeResponse.SetupGet(mocked => mocked.StatusCode).Returns(HttpStatusCode.BadRequest).Verifiable();
            stripeResponse.SetupGet(mocked => mocked.Content).Returns("{error: {message:'Invalid Request'}}").Verifiable();

            _restClient.Setup(mocked => mocked.Execute<StripeCustomer>(It.IsAny<IRestRequest>())).Returns(stripeResponse.Object);

            try
            {
                _fixture.UpdateCustomerDescription("token", 102030);
                Assert.Fail("Expected exception was not thrown");
            }
            catch (PaymentProcessorException e)
            {
                Assert.AreEqual("Customer update failed", e.Message);
                Assert.IsNotNull(e.DetailMessage);
                Assert.AreEqual("Invalid Request", e.DetailMessage);
                Assert.AreEqual(_errors["failedResponse"], e.GlobalMessage);
            }
        }

        [Test]
        public void ShouldChargeCustomer()
        {
            var charge = new StripeCharge
            {
                Id = "90210",
                BalanceTransaction = new StripeBalanceTransaction
                {
                    Id = "txn_123",
                    Fee = 145
                }
            };
            

            var stripeResponse = new Mock<IRestResponse<StripeCharge>>(MockBehavior.Strict);
            stripeResponse.SetupGet(mocked => mocked.ResponseStatus).Returns(ResponseStatus.Completed).Verifiable();
            stripeResponse.SetupGet(mocked => mocked.StatusCode).Returns(HttpStatusCode.OK).Verifiable();
            stripeResponse.SetupGet(mocked => mocked.Data).Returns(charge).Verifiable();

            _restClient.Setup(mocked => mocked.Execute<StripeCharge>(It.IsAny<IRestRequest>())).Returns(stripeResponse.Object);

            var response = _fixture.ChargeCustomer("cust_token", 9090, 98765, false,"bart_simpson@crossroads.net", "Bart Simpson");

            _restClient.Verify(mocked => mocked.Execute<StripeCharge>(
                It.Is<IRestRequest>(o =>
                    o.Method == Method.POST
                    && o.Resource.Equals("charges")
                    && o.Parameters.Matches("amount", 909000)
                    && o.Parameters.Matches("currency", "usd")
                    && o.Parameters.Matches("customer", "cust_token")
                    && o.Parameters.Matches("description", "Donor ID #98765")
                    && o.Parameters.Matches("expand[]", "balance_transaction")
                    && o.Parameters.Matches("metadata[crossroads_transaction_type]", "donation")
                    )));

            _restClient.VerifyAll();
            stripeResponse.VerifyAll();

            Assert.AreSame(charge, response);
        }

        [Test]
        public void ShouldNotChargeCustomerIfAmountIsInvalid()
        {
            var customer = new StripeCustomer
            {
                id = "12345",
                default_source = "some card",

            };
            
            var getCustomerResponse = new Mock<IRestResponse<StripeCustomer>>(MockBehavior.Strict);

            getCustomerResponse.SetupGet(mocked => mocked.ResponseStatus).Returns(ResponseStatus.Completed).Verifiable();
            getCustomerResponse.SetupGet(mocked => mocked.StatusCode).Returns(HttpStatusCode.OK).Verifiable();
            getCustomerResponse.SetupGet(mocked => mocked.Data).Returns(customer).Verifiable();
            _restClient.Setup(mocked => mocked.Execute<StripeCustomer>(It.IsAny<IRestRequest>())).Returns(getCustomerResponse.Object);


            var chargeResponse = new Mock<IRestResponse<StripeCharge>>(MockBehavior.Strict);
            chargeResponse.SetupGet(mocked => mocked.ResponseStatus).Returns(ResponseStatus.Completed).Verifiable();
            chargeResponse.SetupGet(mocked => mocked.StatusCode).Returns(HttpStatusCode.BadRequest).Verifiable();
            chargeResponse.SetupGet(mocked => mocked.Content).Returns("{error: {message:'Invalid Integer Amount'}}").Verifiable();

            _restClient.Setup(mocked => mocked.Execute<StripeCharge>(It.IsAny<IRestRequest>())).Returns(chargeResponse.Object);
            try
            {
                _fixture.ChargeCustomer("token", -900, 98765, false, "bart_simpson@crossroads.net", "Bart Simpson");
                Assert.Fail("Should have thrown exception");
            }
            catch (PaymentProcessorException e)
            {
                Assert.AreEqual("Invalid charge request", e.Message);
                Assert.IsNotNull(e.DetailMessage);
                Assert.AreEqual("Invalid Integer Amount", e.DetailMessage);
                Assert.AreEqual(_errors["failedResponse"], e.GlobalMessage);
            }

        }

        [Test]
        public void ShouldNotChargeCustomerWith404()
        {
            
            var charge = new StripeCharge
            {
                Id = "90210",
          
                BalanceTransaction = new StripeBalanceTransaction
                {
                    Id = "txn_123",
                    Fee = 145
                }
            };


            var stripeResponse = new Mock<IRestResponse<StripeCharge>>(MockBehavior.Strict);
            stripeResponse.SetupGet(mocked => mocked.ResponseStatus).Returns(ResponseStatus.Completed).Verifiable();
            stripeResponse.SetupGet(mocked => mocked.StatusCode).Returns(HttpStatusCode.NotFound).Verifiable();
            stripeResponse.SetupGet(mocked => mocked.Data).Returns(charge).Verifiable();
            stripeResponse.SetupGet(mocked => mocked.Content).Returns("{error: {message:'Customer does not have a linked source with ID '}}").Verifiable();


            _restClient.Setup(mocked => mocked.Execute<StripeCharge>(It.IsAny<IRestRequest>())).Returns(stripeResponse.Object);

            try
            {
                var response = _fixture.ChargeCustomer("cust_token", "nonexistant source", 9090, 98765, "1111");
                Assert.Fail("Should have thrown exception");
            }
            catch (PaymentProcessorException e)
            {
                Assert.AreEqual("Invalid charge request", e.Message);
                
            }
            
        }

        [Test]
        public void ShouldUpdateCustomerSource()
        {
            var customer = new StripeCustomer
            {
                id = "cus_test0618",
                default_source = "platinum card",
                sources = new Sources()
                {
                    data = new List<SourceData>()
                    {
                        new SourceData()
                        {
                            last4 = "8585",
                            brand = "Visa",
                            address_zip = "45454",
                            id = "platinum card",
                            exp_month = "01",
                            exp_year = "2020"
                        }
                    }
                }
            };


            var stripeResponse = new Mock<IRestResponse<StripeCustomer>>(MockBehavior.Strict);
            stripeResponse.SetupGet(mocked => mocked.ResponseStatus).Returns(ResponseStatus.Completed).Verifiable();
            stripeResponse.SetupGet(mocked => mocked.StatusCode).Returns(HttpStatusCode.OK).Verifiable();
            stripeResponse.SetupGet(mocked => mocked.Data).Returns(customer).Verifiable();

            _restClient.Setup(mocked => mocked.Execute<StripeCustomer>(It.IsAny<IRestRequest>())).Returns(stripeResponse.Object);

            var defaultSource = _fixture.UpdateCustomerSource("customerToken", "cardToken");
            _restClient.Verify(mocked => mocked.Execute<StripeCustomer>(
                It.Is<IRestRequest>(o =>
                    o.Method == Method.POST
                    && o.Resource.Equals("customers/customerToken")
                    && o.Parameters.Matches("source", "cardToken")
                    )));
            _restClient.VerifyAll();
            stripeResponse.VerifyAll();
          
            Assert.AreEqual("Visa", defaultSource.brand);
            Assert.AreEqual("8585", defaultSource.last4);
            Assert.AreEqual("45454", defaultSource.address_zip);
        }

        [Test]
        public void ShouldGetChargeRefund()
        {
            var expectedRefund = new StripeRefund
            {
                Data = new List<StripeRefundData>()
                {
                    new StripeRefundData()
                    {
                        Id = "456",
                        Amount = "987",
                        Charge = new StripeCharge
                        {
                            Id = "ch_123456"
                        }
                    }

                }
            };

            var refundDataJson = JsonConvert.SerializeObject(expectedRefund);
        
            var response = new Mock<IRestResponse>();
            response.SetupGet(mocked => mocked.ResponseStatus).Returns(ResponseStatus.Completed).Verifiable();
            response.SetupGet(mocked => mocked.StatusCode).Returns(HttpStatusCode.OK).Verifiable();
            response.SetupGet(mocked => mocked.Content).Returns(refundDataJson).Verifiable();

            _restClient.Setup(mocked => mocked.Execute(It.IsAny<IRestRequest>())).Returns(response.Object);

            var refund = _fixture.GetChargeRefund("456");
            _restClient.Verify(mocked => mocked.Execute(
                It.Is<RestRequest>(o =>
                    o.Method == Method.GET
                    && o.Resource.Equals("charges/456/refunds")
                    && o.Parameters.Matches("expand[]", "data.balance_transaction")
                    && o.Parameters.Matches("expand[]", "data.charge")
            )));
       
            _restClient.VerifyAll();
            response.VerifyAll();
            Assert.IsNotNull(refund.Data);
        }

        [Test]
        public void ShouldGetRefund()
        {

            const string refundDataJson = "{id: '456', amount: 987, charge: { id: 'ch_123456'}}";

            var response = new Mock<IRestResponse>();

            response.SetupGet(mocked => mocked.ResponseStatus).Returns(ResponseStatus.Completed).Verifiable();
            response.SetupGet(mocked => mocked.StatusCode).Returns(HttpStatusCode.OK).Verifiable();
            response.SetupGet(mocked => mocked.Content).Returns(refundDataJson).Verifiable();

            _restClient.Setup(mocked => mocked.Execute(It.IsAny<IRestRequest>())).Returns(response.Object);

            var refund = _fixture.GetRefund("456");
            _restClient.Verify(mocked => mocked.Execute(
                It.Is<RestRequest>(o =>
                    o.Method == Method.GET
                    && o.Resource.Equals("refunds/456")
            )));

            _restClient.VerifyAll();
            response.VerifyAll();
            Assert.IsNotNull(refund);
            Assert.AreEqual("456", refund.Id);
            Assert.AreEqual("987", refund.Amount);
            Assert.IsNotNull(refund.Charge);
            Assert.AreEqual("ch_123456", refund.Charge.Id);
        }

        [Test]
        public void TestCreatePlan()
        {
            const int expectedTrialDays = 3;
            var recurringGiftDto = new RecurringGiftDto
            {
                StripeTokenId = "tok_123",
                PlanAmount = 123.45M,
                PlanInterval = PlanInterval.Weekly,
                Program = "987",
                StartDate = DateTime.Now.AddDays(expectedTrialDays)
            };
            var interval = EnumMemberSerializationUtils.ToEnumString(recurringGiftDto.PlanInterval);

            var contactDonor = new MpContactDonor
            {
                DonorId = 678,
                ProcessorId = "cus_123"
            };

            var stripePlan = new StripePlan();

            var stripeResponse = new Mock<IRestResponse<StripePlan>>(MockBehavior.Strict);
            stripeResponse.SetupGet(mocked => mocked.ResponseStatus).Returns(ResponseStatus.Completed).Verifiable();
            stripeResponse.SetupGet(mocked => mocked.StatusCode).Returns(HttpStatusCode.OK).Verifiable();
            stripeResponse.SetupGet(mocked => mocked.Data).Returns(stripePlan).Verifiable();

            _restClient.Setup(mocked => mocked.Execute<StripePlan>(It.IsAny<IRestRequest>())).Returns(stripeResponse.Object);

            var response = _fixture.CreatePlan(recurringGiftDto, contactDonor);
            _restClient.Verify(mocked => mocked.Execute<StripePlan>(It.Is<IRestRequest>(o =>
                o.Method == Method.POST
                && o.Resource.Equals("plans")
                && o.Parameters.Matches("amount", (int)(recurringGiftDto.PlanAmount * Constants.StripeDecimalConversionValue))
                && o.Parameters.Matches("interval", interval)
                && o.Parameters.Matches("name", "Donor ID #" + contactDonor.DonorId + " " + interval + "ly")
                && o.Parameters.Matches("currency", "usd")
                && o.Parameters.Matches("id", contactDonor.DonorId + " " + DateTime.Now))));

            Assert.AreSame(stripePlan, response);
        }

        [Test]
        public void TestAddSourceToCustomer()
        {
            var stripeCustomer = new StripeCustomer();

            var stripeResponse = new Mock<IRestResponse<StripeCustomer>>(MockBehavior.Strict);
            stripeResponse.SetupGet(mocked => mocked.ResponseStatus).Returns(ResponseStatus.Completed).Verifiable();
            stripeResponse.SetupGet(mocked => mocked.StatusCode).Returns(HttpStatusCode.OK).Verifiable();
            stripeResponse.SetupGet(mocked => mocked.Data).Returns(stripeCustomer).Verifiable();

            _restClient.Setup(mocked => mocked.Execute<StripeCustomer>(It.IsAny<IRestRequest>())).Returns(stripeResponse.Object);

            var response = _fixture.AddSourceToCustomer("cus_123", "card_123");
            _restClient.Verify(
                mocked =>
                    mocked.Execute<StripeCustomer>(
                        It.Is<IRestRequest>(o => o.Method == Method.POST && o.Resource.Equals("customers/cus_123/sources") && o.Parameters.Matches("source", "card_123"))));

            Assert.AreSame(stripeCustomer, response);
        }

        [Test]
        public void TestCreateSubscription()
        {
            var stripeSubscription = new StripeSubscription();

            var stripeResponse = new Mock<IRestResponse<StripeSubscription>>(MockBehavior.Strict);
            stripeResponse.SetupGet(mocked => mocked.ResponseStatus).Returns(ResponseStatus.Completed).Verifiable();
            stripeResponse.SetupGet(mocked => mocked.StatusCode).Returns(HttpStatusCode.OK).Verifiable();
            stripeResponse.SetupGet(mocked => mocked.Data).Returns(stripeSubscription).Verifiable();

            _restClient.Setup(mocked => mocked.Execute<StripeSubscription>(It.IsAny<IRestRequest>())).Returns(stripeResponse.Object);

            const string plan = "Take over the world.";
            const string customer = "cus_123";
            var trialEndDate = DateTime.Now.AddDays(1);

            var expectedEpochTime = trialEndDate.ToUniversalTime().Date.AddHours(12).ConvertDateTimeToEpoch();

            var response = _fixture.CreateSubscription(plan, customer, trialEndDate);
            _restClient.Verify(
                mocked =>
                    mocked.Execute<StripeSubscription>(
                        It.Is<IRestRequest>(
                            o =>
                                o.Method == Method.POST && o.Resource.Equals("customers/" + customer + "/subscriptions") && o.Parameters.Matches("plan", plan) &&
                                o.Parameters.Matches("trial_end", expectedEpochTime))));

            Assert.AreSame(stripeSubscription, response);
        }

        [Test]
        public void TestCreateSubscriptionNoTrial()
        {
            var stripeSubscription = new StripeSubscription();

            var stripeResponse = new Mock<IRestResponse<StripeSubscription>>(MockBehavior.Strict);
            stripeResponse.SetupGet(mocked => mocked.ResponseStatus).Returns(ResponseStatus.Completed).Verifiable();
            stripeResponse.SetupGet(mocked => mocked.StatusCode).Returns(HttpStatusCode.OK).Verifiable();
            stripeResponse.SetupGet(mocked => mocked.Data).Returns(stripeSubscription).Verifiable();

            _restClient.Setup(mocked => mocked.Execute<StripeSubscription>(It.IsAny<IRestRequest>())).Returns(stripeResponse.Object);

            const string plan = "Take over the world.";
            const string customer = "cus_123";
            var trialEndDate = DateTime.Today;

            var response = _fixture.CreateSubscription(plan, customer, trialEndDate);
            _restClient.Verify(
                mocked =>
                    mocked.Execute<StripeSubscription>(
                        It.Is<IRestRequest>(
                            o =>
                                o.Method == Method.POST && o.Resource.Equals("customers/" + customer + "/subscriptions") && o.Parameters.Matches("plan", plan) &&
                                !o.Parameters.Contains("trial_end"))));

            Assert.AreSame(stripeSubscription, response);
        }

        [Test]
        public void TestCancelSubscription()
        {
            var stripeSubscription = new StripeSubscription();

            var stripeResponse = new Mock<IRestResponse<StripeSubscription>>(MockBehavior.Strict);
            stripeResponse.SetupGet(mocked => mocked.ResponseStatus).Returns(ResponseStatus.Completed).Verifiable();
            stripeResponse.SetupGet(mocked => mocked.StatusCode).Returns(HttpStatusCode.OK).Verifiable();
            stripeResponse.SetupGet(mocked => mocked.Data).Returns(stripeSubscription).Verifiable();

            _restClient.Setup(mocked => mocked.Execute<StripeSubscription>(It.IsAny<IRestRequest>())).Returns(stripeResponse.Object);

            const string sub = "sub_123";
            const string customer = "cus_123";

            var response = _fixture.CancelSubscription(customer, sub);
            _restClient.Verify(
                mocked =>
                    mocked.Execute<StripeSubscription>(
                        It.Is<IRestRequest>(o => o.Method == Method.DELETE && o.Resource.Equals("customers/" + customer + "/subscriptions/" + sub))));

            Assert.AreSame(stripeSubscription, response);
        }

        [Test]
        public void TestUpdateSubscriptionPlan()
        {
            var stripeSubscription = new StripeSubscription();

            var stripeResponse = new Mock<IRestResponse<StripeSubscription>>(MockBehavior.Strict);
            stripeResponse.SetupGet(mocked => mocked.ResponseStatus).Returns(ResponseStatus.Completed).Verifiable();
            stripeResponse.SetupGet(mocked => mocked.StatusCode).Returns(HttpStatusCode.OK).Verifiable();
            stripeResponse.SetupGet(mocked => mocked.Data).Returns(stripeSubscription).Verifiable();

            _restClient.Setup(mocked => mocked.Execute<StripeSubscription>(It.IsAny<IRestRequest>())).Returns(stripeResponse.Object);

            const string sub = "sub_123";
            const string customer = "cus_123";
            const string plan = "plan_123";
            var trialDateTime = DateTime.Now.AddDays(2);
            var expectedTrialEndDate = trialDateTime.ToUniversalTime().Date.AddHours(12).ConvertDateTimeToEpoch();

            var response = _fixture.UpdateSubscriptionPlan(customer, sub, plan, trialDateTime);
            _restClient.Verify(
                mocked =>
                    mocked.Execute<StripeSubscription>(
                        It.Is<IRestRequest>(
                            o =>
                                o.Method == Method.POST && o.Resource.Equals("customers/" + customer + "/subscriptions/" + sub) && o.Parameters.Matches("plan", plan) &&
                                o.Parameters.Matches("prorate", false) && o.Parameters.Matches("trial_end", expectedTrialEndDate))));

            Assert.AreSame(stripeSubscription, response);
        }

        [Test]
        public void TestUpdateSubscriptionPlanWithNoTrial()
        {
            var stripeSubscription = new StripeSubscription();

            var stripeResponse = new Mock<IRestResponse<StripeSubscription>>(MockBehavior.Strict);
            stripeResponse.SetupGet(mocked => mocked.ResponseStatus).Returns(ResponseStatus.Completed).Verifiable();
            stripeResponse.SetupGet(mocked => mocked.StatusCode).Returns(HttpStatusCode.OK).Verifiable();
            stripeResponse.SetupGet(mocked => mocked.Data).Returns(stripeSubscription).Verifiable();

            _restClient.Setup(mocked => mocked.Execute<StripeSubscription>(It.IsAny<IRestRequest>())).Returns(stripeResponse.Object);

            const string sub = "sub_123";
            const string customer = "cus_123";
            const string plan = "plan_123";

            var response = _fixture.UpdateSubscriptionPlan(customer, sub, plan);
            _restClient.Verify(
                mocked =>
                    mocked.Execute<StripeSubscription>(
                        It.Is<IRestRequest>(
                            o =>
                                o.Method == Method.POST && o.Resource.Equals("customers/" + customer + "/subscriptions/" + sub) && o.Parameters.Matches("plan", plan) &&
                                o.Parameters.Matches("prorate", false) && !o.Parameters.Contains("trial_end"))));

            Assert.AreSame(stripeSubscription, response);
        }

        [Test]
        public void TestGetSubscription()
        {
            var stripeSubscription = new StripeSubscription();

            var stripeResponse = new Mock<IRestResponse<StripeSubscription>>(MockBehavior.Strict);
            stripeResponse.SetupGet(mocked => mocked.ResponseStatus).Returns(ResponseStatus.Completed).Verifiable();
            stripeResponse.SetupGet(mocked => mocked.StatusCode).Returns(HttpStatusCode.OK).Verifiable();
            stripeResponse.SetupGet(mocked => mocked.Data).Returns(stripeSubscription).Verifiable();

            _restClient.Setup(mocked => mocked.Execute<StripeSubscription>(It.IsAny<IRestRequest>())).Returns(stripeResponse.Object);

            const string sub = "sub_123";
            const string customer = "cus_123";

            var response = _fixture.GetSubscription(customer, sub);
            _restClient.Verify(
                mocked =>
                    mocked.Execute<StripeSubscription>(
                        It.Is<IRestRequest>(
                            o => o.Method == Method.GET && o.Resource.Equals("customers/" + customer + "/subscriptions/" + sub))));

            Assert.AreSame(stripeSubscription, response);
        }

        [Test]
        public void TestCancelPlan()
        {
            var stripePlan = new StripePlan();

            var stripeResponse = new Mock<IRestResponse<StripePlan>>(MockBehavior.Strict);
            stripeResponse.SetupGet(mocked => mocked.ResponseStatus).Returns(ResponseStatus.Completed).Verifiable();
            stripeResponse.SetupGet(mocked => mocked.StatusCode).Returns(HttpStatusCode.OK).Verifiable();
            stripeResponse.SetupGet(mocked => mocked.Data).Returns(stripePlan).Verifiable();

            _restClient.Setup(mocked => mocked.Execute<StripePlan>(It.IsAny<IRestRequest>())).Returns(stripeResponse.Object);

            const string plan = "plan_123/456/789";

            var response = _fixture.CancelPlan(plan);
            _restClient.Verify(
                mocked =>
                    mocked.Execute<StripePlan>(
                        It.Is<IRestRequest>(o => o.Method == Method.DELETE && o.Resource.Equals("plans/" + plan.Replace("/", "%2F")))));

            Assert.AreSame(stripePlan, response);
        }

        [Test]
        public void TestGetSource()
        {
            var expectedSource = new SourceData
            {
                id = "ba_456"
            };

            var customer = new StripeCustomer
            {
                sources = new Sources
                {
                    data = new List<SourceData>
                    {
                        new SourceData
                        {
                            id = "card_123"
                        },
                        expectedSource,
                        new SourceData
                        {
                            id = "card_789"
                        }
                    }
                }
            };

            var response = new Mock<IRestResponse<StripeCustomer>>();

            response.SetupGet(mocked => mocked.ResponseStatus).Returns(ResponseStatus.Completed).Verifiable();
            response.SetupGet(mocked => mocked.StatusCode).Returns(HttpStatusCode.OK).Verifiable();
            response.SetupGet(mocked => mocked.Data).Returns(customer).Verifiable();

            _restClient.Setup(mocked => mocked.Execute<StripeCustomer>(It.IsAny<IRestRequest>())).Returns(response.Object);

            var result = _fixture.GetSource("cus_123", "ba_456");

            _restClient.Verify(mocked => mocked.Execute<StripeCustomer>(
                It.Is<RestRequest>(o =>
                    o.Method == Method.GET
                    && o.Resource.Equals("/customers/cus_123")
            )));

            _restClient.VerifyAll();
            response.VerifyAll();

            Assert.IsNotNull(result);
            Assert.AreSame(expectedSource, result);
        }

        [Test]
        public void TestGetCustomer()
        {
            var customer = new StripeCustomer();

            var response = new Mock<IRestResponse<StripeCustomer>>();

            response.SetupGet(mocked => mocked.ResponseStatus).Returns(ResponseStatus.Completed).Verifiable();
            response.SetupGet(mocked => mocked.StatusCode).Returns(HttpStatusCode.OK).Verifiable();
            response.SetupGet(mocked => mocked.Data).Returns(customer).Verifiable();

            _restClient.Setup(mocked => mocked.Execute<StripeCustomer>(It.IsAny<IRestRequest>())).Returns(response.Object);

            var result = _fixture.GetCustomer("cus_123");

            _restClient.Verify(mocked => mocked.Execute<StripeCustomer>(
                It.Is<RestRequest>(o =>
                    o.Method == Method.GET
                    && o.Resource.Equals("/customers/cus_123")
            )));

            _restClient.VerifyAll();
            response.VerifyAll();
            Assert.IsNotNull(result);
            Assert.AreSame(customer, result);
        }

        [Test]
        public void TestCreateToken()
        {
            var token = new StripeToken();

            var response = new Mock<IRestResponse<StripeToken>>();

            response.SetupGet(mocked => mocked.ResponseStatus).Returns(ResponseStatus.Completed).Verifiable();
            response.SetupGet(mocked => mocked.StatusCode).Returns(HttpStatusCode.OK).Verifiable();
            response.SetupGet(mocked => mocked.Data).Returns(token).Verifiable();

            _restClient.Setup(mocked => mocked.Execute<StripeToken>(It.IsAny<IRestRequest>())).Returns(response.Object);

            var result = _fixture.CreateToken("123", "456", "AccountHolderFirstName LastName");

            _restClient.Verify(mocked => mocked.Execute<StripeToken>(
                It.Is<RestRequest>(o =>
                    o.Method == Method.POST
                    && o.Resource.Equals("tokens")
                    && o.Parameters.Matches("bank_account[account_number]", "123")
                    && o.Parameters.Matches("bank_account[routing_number]", "456")
                    && o.Parameters.Matches("bank_account[country]", "US")
                    && o.Parameters.Matches("bank_account[account_holder_type]", "individual")
                    && o.Parameters.Matches("bank_account[account_holder_name]", "AccountHolderFirstName LastName")
            )));

            _restClient.VerifyAll();
            response.VerifyAll();
            Assert.IsNotNull(result);
            Assert.AreSame(token, result);
        }
    }

    internal static class ParameterExtension
    {
        public static bool Matches(this List<Parameter> parms, string name, object value)
        {
            return (parms.Find(p => p.Name.Equals(name) && p.Value.Equals(value)) != null);
        }

        public static bool Contains(this List<Parameter> parms, string name)
        {
            return (parms.Find(p => p.Name.Equals(name)) != null);
        }
    }
}

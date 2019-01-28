using crds_angular.Controllers.API;
using crds_angular.Services.Interfaces;
using Moq;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Web.Http;
using System.Web.Http.Controllers;
using System.Web.Http.Hosting;
using System.Web.Http.Results;
using crds_angular.Exceptions;
using crds_angular.Exceptions.Models;
using crds_angular.Models.Crossroads.Stewardship;
using crds_angular.Models.Json;
using Crossroads.Utilities.Models;
using Crossroads.Web.Common;
using Crossroads.Web.Common.Security;
using MinistryPlatform.Translation.Models;
using MinistryPlatform.Translation.Repositories.Interfaces;
using DonationStatus = crds_angular.Models.Crossroads.Stewardship.DonationStatus;
using IDonationService = crds_angular.Services.Interfaces.IDonationService;
using IDonorService = crds_angular.Services.Interfaces.IDonorService;
using MPInterfaces = MinistryPlatform.Translation.Repositories.Interfaces;
using crds_angular.Services.Interfaces;

namespace crds_angular.test.controllers
{
    public class DonorControllerTest
    {
        private DonorController _fixture;

        private Mock<IAuthTokenExpiryService> _authTokenExpiryService;
        private Mock<IDonorService> _donorService;
        private Mock<IDonationService> _donationService;
        private Mock<IPaymentProcessorService> _paymentService;
        private Mock<IAuthenticationRepository> _authenticationService;
        private Mock<MPInterfaces.IDonorRepository> _mpDonorService;
        private Mock<IUserImpersonationService> _impersonationService;
        private Mock<IAnalyticsService> _analyticsService;
        private string _authType;
        private string _authToken;
        private const int ContactId = 8675309;
        private const string ProcessorId = "cus_test123456";
        private const string Email = "automatedtest@crossroads.net";
        private const int DonorId = 394256;
        private const string Last4 = "1234";
        private const string Brand = "Visa";
        private const string AddressZip = "45454";
        private readonly MpContactDonor _donor = new MpContactDonor()
        {
            DonorId = DonorId,
            ProcessorId = ProcessorId,
            ContactId = ContactId,
            Email = Email
        };

        [SetUp]
        public void SetUp()
        {
            _authTokenExpiryService = new Mock<IAuthTokenExpiryService>();
            _donorService = new Mock<IDonorService>();
            _donationService = new Mock<IDonationService>();
            _paymentService = new Mock<IPaymentProcessorService>();
            _authenticationService = new Mock<IAuthenticationRepository>();
            _mpDonorService = new Mock<MPInterfaces.IDonorRepository>();
            _impersonationService = new Mock<IUserImpersonationService>();
            _analyticsService = new Mock<IAnalyticsService>();
            _fixture = new DonorController(_authTokenExpiryService.Object, 
                                           _donorService.Object, 
                                           _paymentService.Object, 
                                           _donationService.Object, 
                                           _mpDonorService.Object, 
                                           _authenticationService.Object, 
                                           _impersonationService.Object, 
                                           _analyticsService.Object);

            _authType = "auth_type";
            _authToken = "auth_token";
            _fixture.Request = new HttpRequestMessage();
            _fixture.Request.Headers.Authorization = new AuthenticationHeaderValue(_authType, _authToken);
            _fixture.RequestContext = new HttpRequestContext();

            // This is needed in order for Request.createResponse to work
            _fixture.Request.Properties.Add(HttpPropertyKeys.HttpConfigurationKey, new HttpConfiguration());
            _fixture.Request.SetConfiguration(new HttpConfiguration());
        }

        [Test]
        public void ShouldPostToSuccessfullyCreateAuthenticatedDonor()
        {
            var createDonorDto = new CreateDonorDTO
            {
                stripe_token_id = "tok_test"
            };
            _authTokenExpiryService.Setup(a => a.IsAuthtokenCloseToExpiry(It.IsAny<HttpRequestHeaders>())).Returns(true);
            _donorService.Setup(mocked => mocked.GetContactDonorForAuthenticatedUser(It.IsAny<string>())).Returns((MpContactDonor)null);
            _donorService.Setup(mocked => mocked.CreateOrUpdateContactDonor(null, string.Empty, string.Empty, string.Empty, string.Empty, "tok_test", It.IsAny<DateTime>())).Returns(_donor);
            
            IHttpActionResult result = _fixture.Post(createDonorDto);

            Assert.IsNotNull(result);
            Assert.IsInstanceOf(typeof(OkNegotiatedContentResult<DonorDTO>), result);
            var okResult = (OkNegotiatedContentResult<DonorDTO>)result;
            Assert.AreEqual(DonorId, okResult.Content.Id);
            Assert.AreEqual(ProcessorId, okResult.Content.ProcessorId);
        }

        [Test]
        public void TestGetSuccessGetDonorAuthenticated()
        {
            var contactDonor = new MpContactDonor
            {
                ContactId = 1,
                DonorId = 394256,
                ProcessorId = ProcessorId
            };

            var defaultSource = new SourceData
            {
                last4 = "1234",
                brand = "Visa",
                address_zip = "45454"
            };
            _authTokenExpiryService.Setup(a => a.IsAuthtokenCloseToExpiry(It.IsAny<HttpRequestHeaders>())).Returns(true);
            _donorService.Setup(mocked => mocked.GetContactDonorForAuthenticatedUser(It.IsAny<string>())).Returns(contactDonor);
            _paymentService.Setup(mocked => mocked.GetDefaultSource(It.IsAny<string>())).Returns(defaultSource);
            IHttpActionResult result = _fixture.Get();
            Assert.IsNotNull(result);
            Assert.IsInstanceOf(typeof(OkNegotiatedContentResult<DonorDTO>), result);
            var okResult = (OkNegotiatedContentResult<DonorDTO>)result;
            Assert.AreEqual(DonorId, okResult.Content.Id);
            Assert.AreEqual(ProcessorId, okResult.Content.ProcessorId);
            Assert.AreEqual(Brand, okResult.Content.DefaultSource.credit_card.brand);
            Assert.AreEqual(Last4, okResult.Content.DefaultSource.credit_card.last4);
            Assert.AreEqual(AddressZip, okResult.Content.DefaultSource.credit_card.address_zip);
        }

        [Test]
        public void TestGetGetDonorAuthenticatedNoPaymentProcessor()
        {
            var contactDonor = new MpContactDonor
            {
                ContactId = 1,
                DonorId = 2,
                ProcessorId = null
            };
            _authTokenExpiryService.Setup(a => a.IsAuthtokenCloseToExpiry(It.IsAny<HttpRequestHeaders>())).Returns(true);
            _donorService.Setup(mocked => mocked.GetContactDonorForAuthenticatedUser(It.IsAny<string>())).Returns(contactDonor);
            IHttpActionResult result = _fixture.Get();
            Assert.IsNotNull(result);
            Assert.IsInstanceOf(typeof(NotFoundResult), result);
        }

        [Test]
        public void TestGetSuccessGetDonorUnauthenticated()
        {
            _fixture.Request.Headers.Authorization = null;
            _donorService.Setup(mocked => mocked.GetContactDonorForEmail(Email)).Returns(_donor);
            IHttpActionResult result = _fixture.Get(Email);
            Assert.IsNotNull(result);
            Assert.IsInstanceOf(typeof(OkNegotiatedContentResult<DonorDTO>), result);
            var okResult = (OkNegotiatedContentResult<DonorDTO>)result;
            Assert.AreEqual(DonorId, okResult.Content.Id);
            Assert.AreEqual(ProcessorId, okResult.Content.ProcessorId);
        }

        [Test]
        public void TestGetGetDonorUnauthenticatedNoPaymentProcessor()
        {
            _fixture.Request.Headers.Authorization = null;
            var contactDonor = new MpContactDonor
            {
                ContactId = 1,
                DonorId = 2,
                ProcessorId = null
            };
            _donorService.Setup(mocked => mocked.GetContactDonorForEmail(Email)).Returns(contactDonor);
            IHttpActionResult result = _fixture.Get(Email);
            Assert.IsNotNull(result);
            Assert.IsInstanceOf(typeof(NotFoundResult), result);
        }

        [Test]
        public void ShouldPostToSuccessfullyCreateGuestDonor()
        {
            _fixture.Request.Headers.Authorization = null;

            var createDonorDto = new CreateDonorDTO
            {
                stripe_token_id = "tok_test",
                email_address = "me@here.com",
                first_name = "",
                last_name = ""
            };

            var lookupDonor = new MpContactDonor
            {
                ContactId = 8675309,
                DonorId = 0,
                ProcessorId = null
            };

            var createDonor = new MpContactDonor
            {
                ContactId = 8675309,
                DonorId = 394256,
                ProcessorId = "jenny_ive_got_your_number"
            };

            _donorService.Setup(mocked => mocked.GetContactDonorForEmail(createDonorDto.email_address)).Returns(lookupDonor);
            _donorService.Setup(mocked => mocked.CreateOrUpdateContactDonor(It.Is<MpContactDonor>(d => d == lookupDonor), string.Empty,string.Empty, string.Empty, createDonorDto.email_address, createDonorDto.stripe_token_id, It.IsAny<DateTime>())).Returns(createDonor);

            IHttpActionResult result = _fixture.Post(createDonorDto);

            _donorService.VerifyAll();

            Assert.IsNotNull(result);
            Assert.IsInstanceOf(typeof(ResponseMessageResult), result);
            Assert.NotNull(((ResponseMessageResult)result).Response);
            Assert.AreEqual(HttpStatusCode.Created, ((ResponseMessageResult)result).Response.StatusCode);
            var content = ((ResponseMessageResult)result).Response.Content;
            Assert.NotNull(content);
            Assert.IsInstanceOf(typeof(ObjectContent<DonorDTO>), content);
            var responseDto = (DonorDTO)((ObjectContent)content).Value;
            Assert.AreEqual(394256, responseDto.Id);
            Assert.AreEqual("jenny_ive_got_your_number", responseDto.ProcessorId);
        }

        [Test]
        public void ShouldPostToSuccessfullyReturnExistingGuestDonor()
        {
            _fixture.Request.Headers.Authorization = null;

            var createDonorDto = new CreateDonorDTO
            {
                stripe_token_id = "tok_test",
                email_address = "me@here.com",
                first_name = "",
                last_name = ""
            };

            var lookupDonor = new MpContactDonor
            {
                ContactId = 8675309,
                DonorId = 90210,
                ProcessorId = "jenny_ive_got_your_number"
            };

            var createDonor = new MpContactDonor
            {
                ContactId = 8675309,
                DonorId = 90210,
                ProcessorId = "jenny_ive_got_your_number"
            };

            _donorService.Setup(mocked => mocked.GetContactDonorForEmail(createDonorDto.email_address)).Returns(lookupDonor);
            _donorService.Setup(mocked => mocked.CreateOrUpdateContactDonor(It.Is<MpContactDonor>(d => d == lookupDonor), string.Empty, string.Empty, string.Empty, createDonorDto.email_address, createDonorDto.stripe_token_id, It.IsAny<DateTime>())).Returns(createDonor);

            IHttpActionResult result = _fixture.Post(createDonorDto);

            _donorService.VerifyAll();

            Assert.IsNotNull(result);
            Assert.IsInstanceOf(typeof(ResponseMessageResult), result);
            Assert.NotNull(((ResponseMessageResult)result).Response);
            Assert.AreEqual(HttpStatusCode.OK, ((ResponseMessageResult)result).Response.StatusCode);
            var content = ((ResponseMessageResult)result).Response.Content;
            Assert.NotNull(content);
            Assert.IsInstanceOf(typeof(ObjectContent<DonorDTO>), content);
            var responseDto = (DonorDTO)((ObjectContent)content).Value;
            Assert.AreEqual(90210, responseDto.Id);
            Assert.AreEqual("jenny_ive_got_your_number", responseDto.ProcessorId);
        }

        [Test]
        public void ShouldThrowExceptionWhenDonorLookupFails()
        {
            _fixture.Request.Headers.Authorization = null;

            var createDonorDto = new CreateDonorDTO
            {
                stripe_token_id = "tok_test",
                email_address = "me@here.com"
            };

            var lookupException = new Exception("Danger, Will Robinson!");

            _donorService.Setup(mocked => mocked.GetContactDonorForEmail(createDonorDto.email_address)).Throws(lookupException);

            try
            {
                _fixture.Post(createDonorDto);
                Assert.Fail("Expected exception was not thrown");
            }
            catch (Exception e)
            {
                Assert.AreEqual(typeof(HttpResponseException), e.GetType());
            }

            _donorService.VerifyAll();

        }

        [Test]
        public void ShouldThrowExceptionWhenDonorCreationFails()
        {
            _fixture.Request.Headers.Authorization = null;

            var createDonorDto = new CreateDonorDTO
            {
                stripe_token_id = "tok_test",
                email_address = "me@here.com",
                first_name = "",
                last_name = ""
            };

            var lookupDonor = new MpContactDonor
            {
                ContactId = 8675309,
                DonorId = 90210,
                ProcessorId = "jenny_ive_got_your_number"
            };

            var createException = new Exception("Danger, Will Robinson!");

            _donorService.Setup(mocked => mocked.GetContactDonorForEmail(createDonorDto.email_address)).Returns(lookupDonor);
            _donorService.Setup(mocked => mocked.CreateOrUpdateContactDonor(It.Is<MpContactDonor>(d => d == lookupDonor), string.Empty, string.Empty, string.Empty, createDonorDto.email_address, createDonorDto.stripe_token_id, It.IsAny<DateTime>())).Throws(createException);

            try
            {
                _fixture.Post(createDonorDto);
                Assert.Fail("Expected exception was not thrown");
            }
            catch (Exception e)
            {
                Assert.AreEqual(typeof(HttpResponseException), e.GetType());
            }

            _donorService.VerifyAll();
        }
       
        [Test]
        public void TestUpdateDonorGuestDonor()
        {
            _fixture.Request.Headers.Authorization = null;
            var dto = new UpdateDonorDTO
            {
                DonorId = "123",
                EmailAddress = "me@here.com",
                StripeTokenId = "456"
            };

            var contactDonor = new MpContactDonor
            {
                DonorId = 123,
                ContactId = 789,
                Email = "me@here.com",
                ProcessorId = "102030",
                RegisteredUser = false,
            };

            var sourceData = new SourceData
            {
                bank_last4 = "5555",
                routing_number = "987654321"
            };

            _donorService.Setup(mocked => mocked.GetContactDonorForEmail("me@here.com")).Returns(contactDonor);
            _paymentService.Setup(mocked => mocked.UpdateCustomerSource(contactDonor.ProcessorId, dto.StripeTokenId))
                .Returns(sourceData);

            var result = _fixture.UpdateDonor(dto);
            _donorService.VerifyAll();
            _paymentService.VerifyAll();

            Assert.NotNull(result);
            Assert.IsInstanceOf<OkNegotiatedContentResult<DonorDTO>>(result);
            var donorDto = ((OkNegotiatedContentResult<DonorDTO>) result).Content;
            Assert.AreEqual(contactDonor.DonorId, donorDto.Id);
            Assert.AreEqual(contactDonor.ProcessorId, donorDto.ProcessorId);
            Assert.AreEqual(contactDonor.RegisteredUser, donorDto.RegisteredUser);

            Assert.IsNotNull(donorDto.DefaultSource.credit_card);
            Assert.IsNotNull(donorDto.DefaultSource.bank_account);
            Assert.AreEqual(sourceData.bank_last4, donorDto.DefaultSource.bank_account.last4);
            Assert.AreEqual(sourceData.routing_number, donorDto.DefaultSource.bank_account.routing);
        }

        [Test]
        public void TestUpdateDonorRegisteredDonor()
        {
            var dto = new UpdateDonorDTO
            {
                DonorId = "123",
                EmailAddress = "me@here.com",
                StripeTokenId = "456"
            };

            var contactDonor = new MpContactDonor
            {
                DonorId = 123,
                ContactId = 789,
                Email = "me@here.com",
                ProcessorId = "102030",
                RegisteredUser = true,
            };

            var sourceData = new SourceData
            {
                brand = "Visa",
                last4 = "5432",
                address_zip = "90210",
                exp_month = "12",
                exp_year = "19"
            };
            _authTokenExpiryService.Setup(a => a.IsAuthtokenCloseToExpiry(It.IsAny<HttpRequestHeaders>())).Returns(true);
            _donorService.Setup(mocked => mocked.GetContactDonorForAuthenticatedUser(It.IsAny<string>())).Returns(contactDonor);
            _paymentService.Setup(mocked => mocked.UpdateCustomerSource(contactDonor.ProcessorId, dto.StripeTokenId))
                .Returns(sourceData);

            var result = _fixture.UpdateDonor(dto);
            _donorService.VerifyAll();
            _paymentService.VerifyAll();

            Assert.NotNull(result);
            Assert.IsInstanceOf<OkNegotiatedContentResult<DonorDTO>>(result);
            var donorDto = ((OkNegotiatedContentResult<DonorDTO>)result).Content;
            Assert.AreEqual(contactDonor.DonorId, donorDto.Id);
            Assert.AreEqual(contactDonor.ProcessorId, donorDto.ProcessorId);
            Assert.AreEqual(contactDonor.RegisteredUser, donorDto.RegisteredUser);
            Assert.IsNotNull(donorDto.DefaultSource.bank_account);
            Assert.IsNotNull(donorDto.DefaultSource.credit_card);
            Assert.AreEqual(sourceData.brand, donorDto.DefaultSource.credit_card.brand);
            Assert.AreEqual(sourceData.last4, donorDto.DefaultSource.credit_card.last4);
            Assert.AreEqual(sourceData.address_zip, donorDto.DefaultSource.credit_card.address_zip);
            Assert.AreEqual(sourceData.exp_month + sourceData.exp_year, donorDto.DefaultSource.credit_card.exp_date);
        }


        [Test]
        public void TestUpdateDonorLookupThrowsApplicationException()
        {
            _fixture.Request.Headers.Authorization = null;
            var dto = new UpdateDonorDTO
            {
                DonorId = "123",
                EmailAddress = "me@here.com",
                StripeTokenId = "456"
            };

            var applicationException = new ApplicationException("whoa nelly");
            _donorService.Setup(mocked => mocked.GetContactDonorForEmail("me@here.com")).Throws(applicationException);

            try
            {
                _fixture.UpdateDonor(dto);
                Assert.Fail("Expected exception was not thrown");
            }
            catch (Exception e)
            {
                Assert.AreEqual(typeof(HttpResponseException), e.GetType());
            }

            _donorService.VerifyAll();
            _paymentService.VerifyAll();
        }

        [Test]
        public void TestUpdateDonorStripeUpdateThrowsStripeException()
        {
            _fixture.Request.Headers.Authorization = null;
            var dto = new UpdateDonorDTO
            {
                DonorId = "123",
                EmailAddress = "me@here.com",
                StripeTokenId = "456"
            };

            var contactDonor = new MpContactDonor
            {
                DonorId = 123,
                ContactId = 789,
                Email = "me@here.com",
                ProcessorId = "102030",
                RegisteredUser = false,
            };

            _donorService.Setup(mocked => mocked.GetContactDonorForEmail("me@here.com")).Returns(contactDonor);

            var stripeException = new PaymentProcessorException(HttpStatusCode.PaymentRequired, "auxMessage", "type", "message", "code", "decline", "param");
            _paymentService.Setup(mocked => mocked.UpdateCustomerSource(contactDonor.ProcessorId, dto.StripeTokenId))
                .Throws(stripeException);

            var response = _fixture.UpdateDonor(dto);
            Assert.AreEqual(typeof(RestHttpActionResult<PaymentProcessorErrorResponse>), response.GetType());
            var stripeErrorResponse = (RestHttpActionResult<PaymentProcessorErrorResponse>) response;
            var content = stripeErrorResponse.Content;
            Assert.AreEqual("type", content.Error.Type);
            Assert.AreEqual("message", content.Error.Message);
            Assert.AreEqual("code", content.Error.Code);
            Assert.AreEqual("decline", content.Error.DeclineCode);
            Assert.AreEqual("param", content.Error.Param);

            _donorService.VerifyAll();
            _paymentService.VerifyAll();
        }

        [Test]
        public void TestCreateDonorForUnauthenticatedUserStripeUpdateThrowsStripeException()
        {
            _fixture.Request.Headers.Authorization = null;
            var dto = new CreateDonorDTO
            {
                email_address = "me@here.com",
                stripe_token_id = "456",
                first_name = "",
                last_name = ""
            };

            var contactDonor = new MpContactDonor
            {
                DonorId = 123,
                ContactId = 789,
                Email = "me@here.com",
                ProcessorId = "102030",
                RegisteredUser = false,
            };

            var stripeException = new PaymentProcessorException(HttpStatusCode.PaymentRequired, "auxMessage", "type", "message", "code", "decline", "param");
            _donorService.Setup(mocked => mocked.GetContactDonorForEmail("me@here.com")).Returns(contactDonor);
            _donorService.Setup(
                (mocked => mocked.CreateOrUpdateContactDonor(contactDonor, string.Empty, string.Empty, string.Empty,  "me@here.com", "456", It.IsAny<DateTime>())))
                .Throws(stripeException);

            var response = _fixture.Post(dto);
            Assert.AreEqual(typeof(RestHttpActionResult<PaymentProcessorErrorResponse>), response.GetType());

            _donorService.VerifyAll();
            _paymentService.VerifyAll();
        }

        [Test]
        public void TestCreateDonorForAuthenticatedUserStripeUpdateThrowsStripeException()
        {
            var dto = new CreateDonorDTO
            {
                email_address = "me@here.com",
                stripe_token_id = "456"
            };

            var contactDonor = new MpContactDonor
            {
                DonorId = 123,
                ContactId = 789,
                Email = "me@here.com",
                ProcessorId = "102030",
                RegisteredUser = true,
            };

            var stripeException = new PaymentProcessorException(HttpStatusCode.PaymentRequired, "auxMessage", "type", "message", "code", "decline", "param");
            _authTokenExpiryService.Setup(a => a.IsAuthtokenCloseToExpiry(It.IsAny<HttpRequestHeaders>())).Returns(true);
            _donorService.Setup(mocked => mocked.GetContactDonorForAuthenticatedUser(It.IsAny<string>())).Returns(contactDonor);
            _donorService.Setup(
                (mocked => mocked.CreateOrUpdateContactDonor(contactDonor, string.Empty, string.Empty, string.Empty,  String.Empty, "456", It.IsAny<DateTime>())))
                .Throws(stripeException);

            var response = _fixture.Post(dto);
            Assert.AreEqual(typeof(RestHttpActionResult<PaymentProcessorErrorResponse>), response.GetType());

            _donorService.VerifyAll();
            _paymentService.VerifyAll();
        }

        //[Test]
        //public void TestCreateRecurringGift()
        //{
        //    const string stripeToken = "tok_123";
        //    var contactDonor = new MpContactDonor
        //    {
        //        Email = "you@here.com"
        //    };
        //    var contactDonorUpdated = new MpContactDonor
        //    {
        //        Email = "me@here.com",
        //        Details = new MpContactDetails
        //        {
        //            EmailAddress = "me@here.com",
        //            DisplayName = "Bart Simpson"
        //        }
        //    };
        //    var recurringGiftDto = new RecurringGiftDto
        //    {
        //        StripeTokenId = stripeToken
        //    };
        //    _authTokenExpiryService.Setup(a => a.IsAuthtokenCloseToExpiry(It.IsAny<HttpRequestHeaders>())).Returns(true);
        //    _donorService.Setup(mocked => mocked.GetContactDonorForAuthenticatedUser(_authType + " " + _authToken)).Returns(contactDonor);
        //    _donorService.Setup(mocked => mocked.CreateOrUpdateContactDonor(contactDonor, string.Empty, string.Empty, string.Empty, string.Empty, null, null)).Returns(contactDonorUpdated);
        //    _donorService.Setup(mocked => mocked.CreateRecurringGift(_authType + " " + _authToken, recurringGiftDto, contactDonorUpdated, "me@here.com", "Bart Simpson")).Returns(123);

        //    var response = _fixture.CreateRecurringGift(recurringGiftDto);
        //    _donorService.VerifyAll();
        //    Assert.IsNotNull(response);
        //    Assert.IsInstanceOf<OkNegotiatedContentResult<RecurringGiftDto>>(response);
        //    var dtoResponse = ((OkNegotiatedContentResult<RecurringGiftDto>) response).Content;
        //    Assert.IsNotNull(dtoResponse);
        //    Assert.AreSame(recurringGiftDto, dtoResponse);
        //    Assert.AreEqual(contactDonorUpdated.Email, recurringGiftDto.EmailAddress);
        //    Assert.AreEqual(123, recurringGiftDto.RecurringGiftId);
        //}

        //[Test]
        //public void TestCreateRecurringGiftStripeError()
        //{
        //    const string stripeToken = "tok_123";
        //    var contactDonor = new MpContactDonor
        //    {
        //        Email = "you@here.com"
        //    };
        //    var contactDonorUpdated = new MpContactDonor
        //    {
        //        Email = "me@here.com",
        //        Details = new MpContactDetails
        //        {
        //            EmailAddress = "me@here.com",
        //            DisplayName = "Bart Simpson"
        //        }
        //    };
        //    var recurringGiftDto = new RecurringGiftDto
        //    {
        //        StripeTokenId = stripeToken
        //    };
        //    var stripeException = new PaymentProcessorException(HttpStatusCode.Forbidden,
        //                                                        "aux message",
        //                                                        "error type",
        //                                                        "message",
        //                                                        "code",
        //                                                        "decline code",
        //                                                        "param",
        //                                                        new ContentBlock());
        //    _authTokenExpiryService.Setup(a => a.IsAuthtokenCloseToExpiry(It.IsAny<HttpRequestHeaders>())).Returns(true);
        //    _donorService.Setup(mocked => mocked.GetContactDonorForAuthenticatedUser(_authType + " " + _authToken)).Returns(contactDonor);
        //    _donorService.Setup(mocked => mocked.CreateOrUpdateContactDonor(contactDonor, string.Empty, string.Empty, string.Empty, string.Empty, null, null)).Returns(contactDonorUpdated);
        //    _donorService.Setup(mocked => mocked.CreateRecurringGift(_authType + " " + _authToken, recurringGiftDto, contactDonorUpdated, "me@here.com", "Bart Simpson")).Throws(stripeException);

        //    var response = _fixture.CreateRecurringGift(recurringGiftDto);
        //    _donorService.VerifyAll();
        //    Assert.IsNotNull(response);
        //    Assert.IsInstanceOf<RestHttpActionResult<PaymentProcessorErrorResponse>>(response);
        //    var err = (RestHttpActionResult<PaymentProcessorErrorResponse>) response;
        //    Assert.AreEqual(HttpStatusCode.Forbidden, err.StatusCode);
        //}

        //[Test]
        //public void TestCreateRecurringGiftMinistryPlatformException()
        //{
        //    _authTokenExpiryService.Setup(a => a.IsAuthtokenCloseToExpiry(It.IsAny<HttpRequestHeaders>())).Returns(true);
        //    _donorService.Setup(mocked => mocked.GetContactDonorForAuthenticatedUser(_authType + " " + _authToken)).Throws<ApplicationException>();

        //    try
        //    {
        //        _fixture.CreateRecurringGift(new RecurringGiftDto());
        //        Assert.Fail("expected exception was not thrown");
        //    }
        //    catch (HttpResponseException)
        //    {
        //        // expected
        //    }
        //    _donorService.VerifyAll();
        //}

        //[Test]
        //public void TestCancelRecurringGift()
        //{
        //    var authUserToken = _authType + " " + _authToken;
        //    const int recurringGiftId = 123;
        //    const bool sendEmail = true;
        //    _authTokenExpiryService.Setup(a => a.IsAuthtokenCloseToExpiry(It.IsAny<HttpRequestHeaders>())).Returns(true);
        //    _donorService.Setup(mocked => mocked.CancelRecurringGift(authUserToken, recurringGiftId, sendEmail));
        //    var response = _fixture.CancelRecurringGift(recurringGiftId);
        //    _donorService.VerifyAll();
        //    Assert.IsNotNull(response);
        //    Assert.IsInstanceOf<OkResult>(response);
        //}

        //[Test]
        //public void TestCancelRecurringGiftStripeError()
        //{
        //    var authUserToken = _authType + " " + _authToken;
        //    const int recurringGiftId = 123;
        //    const bool sendEmail = true;

        //    var stripeException = new PaymentProcessorException(HttpStatusCode.Forbidden,
        //                                                        "aux message",
        //                                                        "error type",
        //                                                        "message",
        //                                                        "code",
        //                                                        "decline code",
        //                                                        "param",
        //                                                        new ContentBlock());

        //    _authTokenExpiryService.Setup(a => a.IsAuthtokenCloseToExpiry(It.IsAny<HttpRequestHeaders>())).Returns(true);
        //    _donorService.Setup(mocked => mocked.CancelRecurringGift(authUserToken, recurringGiftId, sendEmail)).Throws(stripeException);

        //    var response = _fixture.CancelRecurringGift(recurringGiftId);
        //    _donorService.VerifyAll();
        //    Assert.IsNotNull(response);
        //    Assert.IsInstanceOf<RestHttpActionResult<PaymentProcessorErrorResponse>>(response);
        //    var err = (RestHttpActionResult<PaymentProcessorErrorResponse>)response;
        //    Assert.AreEqual(HttpStatusCode.Forbidden, err.StatusCode);
        //}

        //[Test]
        //public void TestCancelRecurringGiftMinistryPlatformException()
        //{
        //    var authUserToken = _authType + " " + _authToken;
        //    const int recurringGiftId = 123;
        //    const bool sendEmail = true;
        //    _authTokenExpiryService.Setup(a => a.IsAuthtokenCloseToExpiry(It.IsAny<HttpRequestHeaders>())).Returns(true);
        //    _donorService.Setup(mocked => mocked.CancelRecurringGift(authUserToken, recurringGiftId, sendEmail)).Throws<ApplicationException>();

        //    try
        //    {
        //        _fixture.CancelRecurringGift(recurringGiftId);
        //        Assert.Fail("expected exception was not thrown");
        //    }
        //    catch (HttpResponseException)
        //    {
        //        // expected
        //    }
        //    _donorService.VerifyAll();
        //}

        //[Test]
        //public void TestEditRecurringGift()
        //{
        //    var authorizedUserToken = _authType + " " + _authToken;
        //    var donor = new MpContactDonor();
        //    var editGift = new RecurringGiftDto();
        //    var newGift = new RecurringGiftDto();
        //    const int recurringGiftId = 123;

        //    _authTokenExpiryService.Setup(a => a.IsAuthtokenCloseToExpiry(It.IsAny<HttpRequestHeaders>())).Returns(true);
        //    _donorService.Setup(mocked => mocked.GetContactDonorForAuthenticatedUser(authorizedUserToken)).Returns(donor);
        //    _donorService.Setup(mocked => mocked.EditRecurringGift(authorizedUserToken, editGift, donor)).Returns(newGift);

        //    var response = _fixture.EditRecurringGift(recurringGiftId, editGift);
        //    _donorService.VerifyAll();

        //    Assert.AreEqual(recurringGiftId, editGift.RecurringGiftId);
        //    Assert.IsNotNull(response);
        //    Assert.IsInstanceOf<OkNegotiatedContentResult<RecurringGiftDto>>(response);
        //    var dtoResponse = ((OkNegotiatedContentResult<RecurringGiftDto>)response).Content;
        //    Assert.IsNotNull(dtoResponse);
        //    Assert.AreSame(newGift, dtoResponse);
        //}

        //[Test]
        //public void TestEditRecurringGiftStripeError()
        //{
        //    var authorizedUserToken = _authType + " " + _authToken;
        //    var donor = new MpContactDonor();
        //    var editGift = new RecurringGiftDto();
        //    const int recurringGiftId = 123;

        //    var stripeException = new PaymentProcessorException(HttpStatusCode.Forbidden,
        //                                                        "aux message",
        //                                                        "error type",
        //                                                        "message",
        //                                                        "code",
        //                                                        "decline code",
        //                                                        "param",
        //                                                        new ContentBlock());
        //    _authTokenExpiryService.Setup(a => a.IsAuthtokenCloseToExpiry(It.IsAny<HttpRequestHeaders>())).Returns(true);
        //    _donorService.Setup(mocked => mocked.GetContactDonorForAuthenticatedUser(authorizedUserToken)).Returns(donor);
        //    _donorService.Setup(mocked => mocked.EditRecurringGift(authorizedUserToken, editGift, donor)).Throws(stripeException);

        //    var response = _fixture.EditRecurringGift(recurringGiftId, editGift);
        //    _donorService.VerifyAll();
        //    Assert.AreEqual(recurringGiftId, editGift.RecurringGiftId);
        //    Assert.IsNotNull(response);
        //    Assert.IsInstanceOf<RestHttpActionResult<PaymentProcessorErrorResponse>>(response);
        //    var err = (RestHttpActionResult<PaymentProcessorErrorResponse>)response;
        //    Assert.AreEqual(HttpStatusCode.Forbidden, err.StatusCode);
        //}

        //[Test]
        //public void TestEditRecurringGiftMinistryPlatformException()
        //{
        //    var authorizedUserToken = _authType + " " + _authToken;
        //    var donor = new MpContactDonor();
        //    var editGift = new RecurringGiftDto();
        //    const int recurringGiftId = 123;

        //    _authTokenExpiryService.Setup(a => a.IsAuthtokenCloseToExpiry(It.IsAny<HttpRequestHeaders>())).Returns(true);
        //    _donorService.Setup(mocked => mocked.GetContactDonorForAuthenticatedUser(authorizedUserToken)).Returns(donor);
        //    _donorService.Setup(mocked => mocked.EditRecurringGift(authorizedUserToken, editGift, donor)).Throws<ApplicationException>();

        //    try
        //    {
        //        _fixture.EditRecurringGift(recurringGiftId, editGift);
        //        Assert.Fail("expected exception was not thrown");
        //    }
        //    catch (HttpResponseException)
        //    {
        //        // expected
        //    }
        //    _donorService.VerifyAll();

        //    Assert.AreEqual(recurringGiftId, editGift.RecurringGiftId);
        //}
    }
}

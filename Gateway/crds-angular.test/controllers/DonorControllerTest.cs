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

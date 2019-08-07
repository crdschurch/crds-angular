using crds_angular.Controllers.API;
using crds_angular.Models.Json;
using crds_angular.Services.Interfaces;
using Crossroads.Web.Common.Configuration;
using Crossroads.Web.Common.Security;
using Moq;
using NUnit.Framework;
using System;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Web.Http;
using System.Web.Http.Results;

namespace crds_angular.test.controllers
{
    [TestFixture]
    class AccountControllerTest
    {
        private AccountController accountController;

        private Mock<IAuthTokenExpiryService> _authTokenExpiryService;
        private Mock<IConfigurationWrapper> _configurationWrapper;
        private Mock<IAccountService> _accountService;
        private const string USERNAME = "testme";
        private const string PASSWORD = "changeme";
        private const string FIRSTNAME = "Test";
        private const string NEW_PASSWORD = "changemeagain";

        [SetUp]
        public void SetUp()
        {
            _configurationWrapper = new Mock<IConfigurationWrapper>();
            Environment.SetEnvironmentVariable("ApiUser", "mockApiUser");
            Environment.SetEnvironmentVariable("ApiPassword", "mockApiPassword");

            _authTokenExpiryService = new Mock<IAuthTokenExpiryService>();
            _accountService = new Mock<IAccountService>();
            _accountService.Setup(m => m.ChangePassword(It.IsAny<string>(), It.IsAny<string>())).Returns(true);

            accountController = new AccountController(_authTokenExpiryService.Object,
                                                      _accountService.Object,
                                                      new Mock<IUserImpersonationService>().Object,
                                                      new Mock<IAuthenticationRepository>().Object);
        }

        [Test]
        public void ShouldReturnUnauthorized()
        {
            accountController.Request = new HttpRequestMessage();
            IHttpActionResult result = accountController.UpdatePassword(new NewPassword { password = "whatever" });
            Assert.IsInstanceOf(typeof(UnauthorizedResult), result);
        }

        [Test]

        public void ShouldReturnOk()
        {
            _authTokenExpiryService.Setup(a => a.IsAuthtokenCloseToExpiry(It.IsAny<HttpRequestHeaders>())).Returns(true);
            // Set the cookie in the header
            var token = "Anything";
            HttpRequestMessage h = new HttpRequestMessage();
            h.Headers.Add("Authorization", token);
            accountController.Request = h;

            // Make the call...
            IHttpActionResult result = accountController.UpdatePassword(new NewPassword { password = NEW_PASSWORD });
            //OkNegotiatedContentResult<Object> o = (OkNegotiatedContentResult<Object>)result;
            Assert.IsInstanceOf(typeof(OkResult), result);

            IHttpActionResult result2 = accountController.UpdatePassword(new NewPassword { password = PASSWORD });
            Assert.IsInstanceOf(typeof(OkResult), result);

        }

    }
}

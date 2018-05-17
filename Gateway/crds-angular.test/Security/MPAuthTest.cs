using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NUnit.Framework;
using Moq;
using crds_angular.Security;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Web.Http.Controllers;
using System.Web.Http;
using System.Web.Http.Results;
using crds_angular.Exceptions;
using crds_angular.Services.Interfaces;
using Crossroads.Web.Common.Security;


namespace crds_angular.test.Security
{
    class MPAuthTest
    {
        private MPAuthTester fixture;

        private Mock<IAuthTokenExpiryService> _authTokenExpiryService;
        private Mock<Func<string, IHttpActionResult>> actionWhenAuthorized;
        private Mock<Func<IHttpActionResult>> actionWhenNotAuthorized;
        private Mock<IUserImpersonationService> _userImpersonationMock;
        private Mock<IAuthenticationRepository> _authenticationRepositoryMock;
        private OkResult okResult;

        private string authType;
        private string authToken;

        [SetUp]
        public void SetUp()
        {
            _userImpersonationMock = new Mock<IUserImpersonationService>();
            _authenticationRepositoryMock = new Mock<IAuthenticationRepository>();
            _authTokenExpiryService = new Mock<IAuthTokenExpiryService>();

            fixture = new MPAuthTester(_authTokenExpiryService.Object, 
                                       _userImpersonationMock.Object, 
                                       _authenticationRepositoryMock.Object);

            actionWhenAuthorized = new Mock<Func<string, IHttpActionResult>>(MockBehavior.Strict);
            actionWhenNotAuthorized = new Mock<Func<IHttpActionResult>>(MockBehavior.Strict);
            okResult = new OkResult(fixture);

            authType = "auth_type";
            authToken = "auth_token";
            fixture.Request = new HttpRequestMessage();
            fixture.Request.Headers.Authorization = new AuthenticationHeaderValue(authType, authToken);
            fixture.RequestContext = new HttpRequestContext();
        }

        [Test]
        public void testAuthorizedNotAuthorized()
        {
            fixture.Request = new HttpRequestMessage();
            fixture.Request.Headers.Authorization = null;

            var result = fixture.AuthTest(actionWhenAuthorized.Object);
            actionWhenAuthorized.VerifyAll();

            Assert.NotNull(result);
            Assert.IsInstanceOf(typeof(UnauthorizedResult), result);
        }

        [Test]
        public void testAuthorizedWhenAuthorized()
        {
            actionWhenAuthorized.Setup(mocked => mocked(authType + " " + authToken)).Returns(okResult);
            var result = fixture.AuthTest(actionWhenAuthorized.Object);
            _authTokenExpiryService.Setup(a => a.IsAuthtokenCloseToExpiry(It.IsAny<HttpRequestHeaders>())).Returns(true);
            actionWhenAuthorized.VerifyAll();

            Assert.NotNull(result);
            Assert.IsInstanceOf(typeof(OkResult), result);
            Assert.AreSame(okResult, result);
        }

        [Test]
        public void testOptionalAuthorizedNotAuthorized()
        {
            fixture.Request = new HttpRequestMessage();
            fixture.Request.Headers.Authorization = null;

            actionWhenNotAuthorized.Setup(mocked => mocked()).Returns(okResult);

            var result = fixture.AuthTest(actionWhenAuthorized.Object, actionWhenNotAuthorized.Object);
            actionWhenNotAuthorized.VerifyAll();
            actionWhenAuthorized.VerifyAll();

            Assert.NotNull(result);
            Assert.IsInstanceOf(typeof(OkResult), result);
            Assert.AreSame(okResult, result);
        }

        [Test]
        public void testOptionalAuthorizedWhenAuthorized()
        {
            actionWhenAuthorized.Setup(mocked => mocked(authType + " " + authToken)).Returns(okResult);
            _authTokenExpiryService.Setup(a => a.IsAuthtokenCloseToExpiry(It.IsAny<HttpRequestHeaders>())).Returns(true);
            var result = fixture.AuthTest(actionWhenAuthorized.Object, actionWhenNotAuthorized.Object);
            actionWhenAuthorized.VerifyAll();
            actionWhenNotAuthorized.VerifyAll();

            Assert.NotNull(result);
            Assert.IsInstanceOf(typeof(OkResult), result);
            Assert.AreSame(okResult, result);
        }

        [Test]
        public void testAuthorizedWhenImpersonateUserIsNotAuthorized()
        {
            fixture.Request = new HttpRequestMessage();
            fixture.Request.Headers.Authorization = null;
            fixture.Request.Headers.Add("ImpersonateUserId", "impersonator@allowed.com");
            
            _userImpersonationMock.Setup(mocked => mocked.WithImpersonation(authType + " " + authToken, "impersonator@notallowed.com", It.IsAny<Func<string, IHttpActionResult>>))
                .Throws<ImpersonationNotAllowedException>();

            var result = fixture.AuthTest(actionWhenAuthorized.Object);
            actionWhenAuthorized.VerifyAll();

            Assert.NotNull(result);
            Assert.IsInstanceOf(typeof(UnauthorizedResult), result);
        }


        [Test]
        public void testAuthorizedWhenImpersonateUserIsAuthorized()
        {
            string auth = authType + " " + authToken;
            fixture.Request.Headers.Add("ImpersonateUserId", "impersonator@allowed.com");
            actionWhenAuthorized.Setup(mocked => mocked(auth)).Returns(okResult);
            _authTokenExpiryService.Setup(a => a.IsAuthtokenCloseToExpiry(It.IsAny<HttpRequestHeaders>())).Returns(true);
            _userImpersonationMock.Setup(m => m.WithImpersonation(auth, "impersonator@allowed.com", It.IsAny<Func<IHttpActionResult>>())).Returns((string lambdaToken, string userId, Func<IHttpActionResult> predicate) =>
            {
                return predicate.Invoke();
            });

            var result = fixture.AuthTest(actionWhenAuthorized.Object);
            _userImpersonationMock.VerifyAll();
            actionWhenAuthorized.VerifyAll();

            Assert.NotNull(result);
            Assert.IsInstanceOf(typeof(OkResult), result);
            Assert.AreSame(okResult, result);
        }

        private class MPAuthTester : MPAuth
        {
            public MPAuthTester(IAuthTokenExpiryService authTokenExpiryService, 
                                IUserImpersonationService userImpersonationService, 
                                IAuthenticationRepository authenticationRepository) 
                : base(authTokenExpiryService, userImpersonationService, authenticationRepository)
            {
                
            }

            public IHttpActionResult AuthTest(Func<string, IHttpActionResult> doIt)
            {
                return(base.Authorized(doIt));
            }

            public IHttpActionResult AuthTest(Func<string, IHttpActionResult> actionWhenAuthorized, Func<IHttpActionResult> actionWhenNotAuthorized)
            {
                return (base.Authorized(actionWhenAuthorized, actionWhenNotAuthorized));
            }
        }
    }
}

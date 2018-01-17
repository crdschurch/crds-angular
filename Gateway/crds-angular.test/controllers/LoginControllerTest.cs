using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Reflection;
using System.Web.Http.Controllers;
using System.Web.Http.Results;
using crds_angular.Controllers.API;
using crds_angular.Models.Json;
using Moq;
using NUnit.Framework;
using ILoginService = crds_angular.Services.Interfaces.ILoginService;
using IPersonService = crds_angular.Services.Interfaces.IPersonService;
using MinistryPlatform.Translation.Repositories.Interfaces;
using crds_angular.Services.Interfaces;
using Crossroads.ClientApiKeys;
using Crossroads.Web.Common.Security;

namespace crds_angular.test.controllers
{
    [TestFixture]
    public class LoginControllerTest
    {
        private LoginController _fixture;

        private Mock<ILoginService> _loginServiceMock;
        private Mock<IPersonService> _personServiceMock;
        private Mock<IUserRepository> _userServiceMock;

        private Mock<IContactRepository> _contactRepository;
        private Mock<IAnalyticsService> _mockAnalyticService;


        private string _authType;
        private string _authToken;

        [SetUp]
        public void SetUp()
        {
            _loginServiceMock = new Mock<ILoginService>();
            _personServiceMock = new Mock<IPersonService>();
            _userServiceMock = new Mock<IUserRepository>();

            _contactRepository = new Mock<IContactRepository>();
            _mockAnalyticService = new Mock<IAnalyticsService>();

            _fixture = new LoginController(_loginServiceMock.Object, _personServiceMock.Object, _userServiceMock.Object, _mockAnalyticService.Object,  new Mock<IUserImpersonationService>().Object, new Mock<IAuthenticationRepository>().Object, _contactRepository.Object);


            _authType = "auth_type";
            _authToken = "auth_token";
            _fixture.Request = new HttpRequestMessage();
            _fixture.Request.Headers.Authorization = new AuthenticationHeaderValue(_authType, _authToken);
            _fixture.RequestContext = new HttpRequestContext();

            _loginServiceMock = new Mock<ILoginService>();
            _loginServiceMock.Setup(m => m.PasswordResetRequest(It.IsAny<string>(), It.IsAny<bool>())).Returns(true);
        }

        [Test]
        public void ShouldAcceptResetRequest()
        {
            var resetRequest = new PasswordResetRequest {Email = "test_email"};
            var result = _fixture.RequestPasswordReset(resetRequest);
            Assert.AreEqual(typeof(OkResult), result.GetType());
        }

        [Test]
        public void LoginPostShouldIgnoreClientApiKey()
        {
            var loginMethod = _fixture.GetType().GetMethods(BindingFlags.Instance | BindingFlags.Public | BindingFlags.DeclaredOnly).ToList().Find(m => m.Name.Equals("Post"));
            Assert.IsNotNull(loginMethod.GetCustomAttribute<IgnoreClientApiKeyAttribute>(), $"Login Post method should have [IgnoreClientApiKey] attribute, so the check scanner controller can use it");
        }

        [Test]
        public void AllMethodsExceptLoginPostShouldNotIgnoreClientApiKey()
        {
            var publicMethods =
                _fixture.GetType().GetMethods(BindingFlags.Instance | BindingFlags.Public | BindingFlags.DeclaredOnly).ToList().FindAll(m => !m.Name.Equals("Post"));
            publicMethods.ForEach(m =>
            {
                Assert.IsNull(m.GetCustomAttribute<IgnoreClientApiKeyAttribute>(), $"Method {m.Name} should not ignore client API key");
            });
        }
    }
}

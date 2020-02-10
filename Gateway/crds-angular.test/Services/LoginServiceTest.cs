
using crds_angular.Services;
using crds_angular.Services.Interfaces;
using log4net;
using Crossroads.Web.Common.Configuration;
using Crossroads.Web.Common.Security;
using MinistryPlatform.Translation.Models;
using MPInterfaces = MinistryPlatform.Translation.Repositories.Interfaces;
using Moq;
using NUnit.Framework;


namespace crds_angular.test.Services
{
    internal class LoginServiceTest
    {
        private ILoginService _loginService;

        private Mock<ILog> _logger;

        private Mock<IAuthenticationRepository> _authenticationRepository;
        private Mock<IConfigurationWrapper> _configurationWrapper;
        private Mock<MPInterfaces.IContactRepository> _contactService;
        private Mock<IEmailCommunication> _emailCommunication;
        private Mock<MPInterfaces.IUserRepository> _userRepository;
        

        [SetUp]
        public void SetUp()
        {
            _logger = new Mock<ILog>();
            _authenticationRepository = new Mock<IAuthenticationRepository>();
            _configurationWrapper = new Mock<IConfigurationWrapper>();
            _contactService = new Mock<MPInterfaces.IContactRepository>();
            _emailCommunication = new Mock<IEmailCommunication>();
            _userRepository = new Mock<MPInterfaces.IUserRepository>();
            
            _loginService = new LoginService(_authenticationRepository.Object, _configurationWrapper.Object, _contactService.Object, _emailCommunication.Object, _userRepository.Object);
        }

        [Test]
        public void ShouldHandleResetRequest()
        {
            string email = "someone@someone.com";

            _contactService.Setup(m => m.GetContactIdByEmail(It.IsAny<string>())).Returns(123456);
            _userRepository.Setup(m => m.GetUserIdByUsername(It.IsAny<string>())).Returns(123456);

            var result = _loginService.PasswordResetRequest(email, false);
            Assert.AreEqual(true, result);
        }

        [Test]
        public void TestIsValidPassword()
        {
            string token = "token_for_loggedin_user";
            string password = "secret";
            MpUser user = new MpUser()
            {
                UserId = "test@test.com"
            };
            AuthToken authData = new AuthToken()
            {
                AccessToken = "newtoken"
            };

            _userRepository.Setup(m => m.GetByAuthenticationToken(token)).Returns(user);
            _authenticationRepository.Setup(m => m.AuthenticateUser(user.UserId, password, false)).Returns(authData);

            var result = _loginService.IsValidPassword(token, password);
            Assert.AreEqual(true, result);
        }
    }
}

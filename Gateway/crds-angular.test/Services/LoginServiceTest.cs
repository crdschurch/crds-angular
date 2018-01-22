
using crds_angular.Services;
using crds_angular.Services.Interfaces;
using Crossroads.Utilities.Interfaces;
using log4net;
using Crossroads.Web.Common;
using Crossroads.Web.Common.Configuration;
using Crossroads.Web.Common.Security;
using MPInterfaces = MinistryPlatform.Translation.Repositories.Interfaces;
using Moq;
using NUnit.Framework;


namespace crds_angular.test.Services
{
    internal class LoginServiceTest
    {
        private ILoginService _loginService;

        private Mock<ILog> _logger;

        private Mock<IAuthenticationRepository> _authenticationService;
        private Mock<IConfigurationWrapper> _configurationWrapper;
        private Mock<MPInterfaces.IContactRepository> _contactService;
        private Mock<IEmailCommunication> _emailCommunication;
        private Mock<MPInterfaces.IUserRepository> _userService;
        

        [SetUp]
        public void SetUp()
        {
            _logger = new Mock<ILog>();
            _authenticationService = new Mock<IAuthenticationRepository>();
            _configurationWrapper = new Mock<IConfigurationWrapper>();
            _contactService = new Mock<MPInterfaces.IContactRepository>();
            _emailCommunication = new Mock<IEmailCommunication>();
            _userService = new Mock<MPInterfaces.IUserRepository>();
            
            _loginService = new LoginService(_authenticationService.Object, _configurationWrapper.Object, _contactService.Object, _emailCommunication.Object, _userService.Object);

            _contactService.Setup(m => m.GetContactIdByEmail(It.IsAny<string>())).Returns(123456);
            _userService.Setup(m => m.GetUserIdByUsername(It.IsAny<string>())).Returns(123456);
        }

        [Test]
        public void ShouldHandleResetRequest()
        {
            string email = "someone@someone.com";

            var result = _loginService.PasswordResetRequest(email, false);
            Assert.AreEqual(true, result);
        }
    }
}

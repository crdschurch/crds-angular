using System;
using System.Collections.Generic;
using Crossroads.Utilities.Interfaces;
using Crossroads.Web.Common;
using Crossroads.Web.Common.Configuration;
using Crossroads.Web.Common.MinistryPlatform;
using Crossroads.Web.Common.Security;
using MinistryPlatform.Translation.Repositories.GoCincinnati;
using MinistryPlatform.Translation.Repositories.Interfaces;
using Moq;
using NUnit.Framework;


namespace MinistryPlatform.Translation.Test.Services
{
    [TestFixture]
    class GroupConnectorServiceTest
    {
        private Mock<IMinistryPlatformService> _ministryPlatformService;
        private Mock<IMinistryPlatformRestRepository> _ministryPlatformRestRepository;
        private Mock<IAuthenticationRepository> _authService;
        private GroupConnectorRepository _fixture;
        private Mock<IConfigurationWrapper> _configuration;
        private Mock<IApiUserRepository> _apiUserService;

        [SetUp]
        public void SetUp()
        {
            _ministryPlatformService = new Mock<IMinistryPlatformService>();
            _ministryPlatformRestRepository = new Mock<IMinistryPlatformRestRepository>();
            _authService = new Mock<IAuthenticationRepository>();
            _configuration = new Mock<IConfigurationWrapper>();
            _apiUserService = new Mock<IApiUserRepository>();

        _configuration.Setup(mocked => mocked.GetConfigIntValue("GroupConnectorPageId")).Returns(467);
            _configuration.Setup(mocked => mocked.GetConfigValue("GroupConnectorRegistrationPageId")).Returns("GroupConnectorRegistrationPageId");
            _configuration.Setup(m => m.GetEnvironmentVarAsString("API_USER")).Returns("uid");
            _configuration.Setup(m => m.GetEnvironmentVarAsString("API_PASSWORD")).Returns("pwd");
            _authService.Setup(m => m.AuthenticateClient(It.IsAny<string>(), It.IsAny<string>())).Returns(new AuthToken
            {
                AccessToken = "ABC",
                ExpiresIn = 123
            });
            _fixture = new GroupConnectorRepository(_ministryPlatformService.Object, _ministryPlatformRestRepository.Object, _authService.Object, _configuration.Object, _apiUserService.Object);
            
        }

        [Test]
        public void ShouldCreateGroupConnector()
        {
            const int registrationId = 9999;
            const int expGroupConnectorId = 8888;

            _ministryPlatformService.Setup(
              mocked => mocked.CreateRecord(467, It.IsAny<Dictionary<string, object>>(), "ABC", true))
              .Returns(expGroupConnectorId);
            _ministryPlatformService.Setup(
             mocked => mocked.CreateSubRecord("GroupConnectorRegistrationPageId", expGroupConnectorId, It.IsAny<Dictionary<string, object>>(), "ABC", true))
             .Returns(123);

            var groupConnector = _fixture.CreateGroupConnector(registrationId, false);
            Assert.AreEqual(expGroupConnectorId, groupConnector);

            _ministryPlatformService.VerifyAll();
        }

        [Test]
        public void ShouldThrowApplicationExceptionWhenGroupConnectorCreationFails()
        {
            Exception ex = new Exception("GroupConnecotr creation failed");
            _ministryPlatformService.Setup(
                mocked => mocked.CreateRecord(467, It.IsAny<Dictionary<string, object>>(), "ABC", true))
                .Throws(ex);

            try
            {
                _fixture.CreateGroupConnector(9999, false);
                Assert.Fail("Expected exception was not thrown");
            }
            catch (Exception e)
            {
                Assert.IsInstanceOf(typeof(ApplicationException), e);
                Assert.AreSame(ex, e.InnerException);
            }
        }

        [Test]
        public void ShouldCreateGroupConnectorRegistration()
        {
            const int registrationId = 9999;
            const int groupConnectorId = 8888;

            _ministryPlatformService.Setup(
              mocked => mocked.CreateSubRecord("GroupConnectorRegistrationPageId", groupConnectorId, It.IsAny<Dictionary<string, object>>(), "ABC", true))
              .Returns(123);



            var groupConnectorRegistration = _fixture.CreateGroupConnectorRegistration(groupConnectorId, registrationId);
            Assert.AreEqual(123, groupConnectorRegistration);

            _ministryPlatformService.VerifyAll();
            
        }

        [Test]
        public void ShouldThrowApplicationExceptionWhenGroupConnectorRegistrationCreationFails()
        {
            Exception ex = new Exception("GroupConnecotrRegistration creation failed");
            _ministryPlatformService.Setup(
                mocked => mocked.CreateSubRecord("GroupConnectorRegistrationPageId",8888, It.IsAny<Dictionary<string, object>>(), "ABC", true))
                .Throws(ex);

            try
            {
                _fixture.CreateGroupConnectorRegistration(8888, 9999);
                Assert.Fail("Expected exception was not thrown");
            }
            catch (Exception e)
            {
                Assert.IsInstanceOf(typeof(ApplicationException), e);
                Assert.AreSame(ex, e.InnerException);
            }
        }

    }
}

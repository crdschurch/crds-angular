using System.Collections.Generic;
using Crossroads.Web.Common.MinistryPlatform;
using Crossroads.Web.Common.Configuration;
using Crossroads.Web.Common.Security;
using MinistryPlatform.Translation.Repositories;
using MinistryPlatform.Translation.Repositories.Interfaces;
using Moq;
using NUnit.Framework;

namespace MinistryPlatform.Translation.Test.Services
{
    public class DestinationServiceTest
    {
        private DestinationRepository _fixture;
        private Mock<IMinistryPlatformService> _ministryPlatformService;
        private Mock<IAuthenticationRepository> _authService;
        private Mock<IConfigurationWrapper> _configWrapper;
        private Mock<IApiUserRepository> _apiUserService;

        [SetUp]
        public void Setup()
        {
            _ministryPlatformService = new Mock<IMinistryPlatformService>();
            _authService = new Mock<IAuthenticationRepository>();
            _configWrapper = new Mock<IConfigurationWrapper>();
            _apiUserService = new Mock<IApiUserRepository>();
            _authService.Setup(m => m.AuthenticateClient(It.IsAny<string>(), It.IsAny<string>())).Returns(new AuthToken
            {
                AccessToken = "ABC",
                ExpiresIn = 123
            });

            _configWrapper.Setup(mocked => mocked.GetEnvironmentVarAsString("API_USER")).Returns("api-user");
            _configWrapper.Setup(mocked => mocked.GetEnvironmentVarAsString("API_PASSWORD")).Returns("api-password");
            _configWrapper.Setup(mocked => mocked.GetConfigIntValue("TripDestinationDocuments")).Returns(1234);

            _fixture = new DestinationRepository(_ministryPlatformService.Object, _authService.Object, _configWrapper.Object, _apiUserService.Object);
        }

        [Test]
        public void DocumentsForDestinationTest()
        {
            var destinationId = 0;
            var searchString = string.Format(",{0}", destinationId);

            var mockDocList = new List<Dictionary<string, object>>
            {
                new Dictionary<string, object>
                {
                    {"Description", "Doc 1 Description"},
                    {"Document", "Document 1"},
                    {"Document_ID", 1}
                },
                new Dictionary<string, object>
                {
                    {"Description", "Doc 2 Description"},
                    {"Document", "Document 2"},
                    {"Document_ID", 2}
                },
                new Dictionary<string, object>
                {
                    {"Description", "Doc 3 Description"},
                    {"Document", "Document 3"},
                    {"Document_ID", 3}
                }
            };

            _ministryPlatformService.Setup(m => m.GetPageViewRecords("TripDestinationDocuments", It.IsAny<string>(), searchString, "", 0)).Returns(mockDocList);

            var documents = _fixture.DocumentsForDestination(destinationId);
            _ministryPlatformService.VerifyAll();
            Assert.IsNotNull(documents);
            Assert.AreEqual(3, documents.Count);
            Assert.AreEqual(1, documents[0].DocumentId);
            Assert.AreEqual(2, documents[1].DocumentId);
            Assert.AreEqual(3, documents[2].DocumentId);
        }
    }
}
using System.Collections.Generic;
using Crossroads.Web.Common.Configuration;
using Crossroads.Web.Common.MinistryPlatform;
using Crossroads.Web.Common.Security;
using MinistryPlatform.Translation.Models.GoCincinnati;
using MinistryPlatform.Translation.Repositories.GoCincinnati;
using MinistryPlatform.Translation.Repositories.Interfaces;
using Moq;
using NUnit.Framework;

namespace MinistryPlatform.Translation.Test.Services
{
    [TestFixture]
    class GoLocalRegistrationTest
    {

        private readonly RegistrationRepository _fixture;

        private readonly Mock<IMinistryPlatformService> _ministryPlatformService;
        private readonly Mock<IMinistryPlatformRestRepository> _ministryPlatformRest;
        private readonly Mock<IAuthenticationRepository> _authenticationRepository;
        private readonly Mock<IConfigurationWrapper> _configurationWrapper;

        private const string token = "totallylegittoken";

        public GoLocalRegistrationTest()
        {
            _ministryPlatformService = new Mock<IMinistryPlatformService>();
            _ministryPlatformRest = new Mock<IMinistryPlatformRestRepository>();
            _authenticationRepository = new Mock<IAuthenticationRepository>();
            _configurationWrapper = new Mock<IConfigurationWrapper>();
            _fixture = new RegistrationRepository(_ministryPlatformService.Object, _ministryPlatformRest.Object, _authenticationRepository.Object, _configurationWrapper.Object);
        }

        [Test]
        public void ShouldGetRegistrantsForProject()
        {
            const int projectId = 1;

            _authenticationRepository.Setup(m => m.AuthenticateUser(It.IsAny<string>(), It.IsAny<string>(), false)).Returns(new AuthToken
            {
                AccessToken = "totallylegittoken",
                ExpiresIn = 123
            });
            _ministryPlatformRest.Setup(m => m.UsingAuthenticationToken(token)).Returns(_ministryPlatformRest.Object);
            _ministryPlatformRest.Setup(m => m.Search<MpProjectRegistration>(It.IsAny<string>(), It.IsAny<string>(), null, false)).Returns(MockRegistrations());
            
            var results = _fixture.GetRegistrantsForProject(projectId);
            Assert.IsInstanceOf(typeof(List<MpProjectRegistration>), results);
            Assert.AreEqual(2, results.Count);
            _ministryPlatformRest.VerifyAll();
        }

        private List<MpProjectRegistration> MockRegistrations()
        {
            return new List<MpProjectRegistration>
            {
                new MpProjectRegistration
                {
                    ProjectId = 1,
                    Nickname = "Bob",
                    LastName = "Boberson",
                    Phone = "123-456-7890",
                    EmailAddress = "bob@bob.com",
                    SpouseParticipating = true,
                    FamilyCount = 5
                },
                new MpProjectRegistration
                {
                    ProjectId = 1,
                    Nickname = "Anita",
                    LastName = "Mann",
                    Phone = "123-456-7890",
                    EmailAddress = "anitamann@aol.com",
                    SpouseParticipating = true,
                    FamilyCount = 5
                }
            };
        }
    }
}

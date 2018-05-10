using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Crossroads.Web.Common.Configuration;
using Crossroads.Web.Common.MinistryPlatform;
using Crossroads.Web.Common.Security;
using MinistryPlatform.Translation.Models;
using MinistryPlatform.Translation.Models.Lookups;
using MinistryPlatform.Translation.Repositories;
using MinistryPlatform.Translation.Repositories.Interfaces;
using Moq;
using NUnit.Framework;

namespace MinistryPlatform.Translation.Test.Services
{
    [TestFixture]
    class LocationRepositoryTest
    {
        private ILocationRepository _fixture;
        private Mock<IMinistryPlatformRestRepository> _ministryPlatformRestRepository;
        private Mock<IConfigurationWrapper> _configurationWrapper;
        private Mock<IAuthenticationRepository> _authenticationService;


        [SetUp]
        public void Setup()
        {
            _ministryPlatformRestRepository = new Mock<IMinistryPlatformRestRepository>();
            _authenticationService = new Mock<IAuthenticationRepository>();
            _configurationWrapper = new Mock<IConfigurationWrapper>();
            
            _fixture = new LocationRepository(_ministryPlatformRestRepository.Object,_authenticationService.Object,_configurationWrapper.Object);

            _authenticationService.Setup(m => m.AuthenticateUser(It.IsAny<string>(), It.IsAny<string>())).Returns(new AuthToken
            {
                AccessToken = "ABC",
                ExpiresIn = 123
            });
        }

        [Test]
        public void TestGetLocations()
        {
            var _location1 = new MpLocation()
            {
                LocationId = 1,
                AddressLine1 = "Address 1",
                City = "City 1",
                State = "State 1",
                Zip = "Zip 1",
                Longitude = 1,
                Latitude = 1,
                LocationName = "Location 1",
                ImageUrl = "Image 1"
            };

            string token = "ABC";
            
            _ministryPlatformRestRepository.Setup(m => m.UsingAuthenticationToken(token)).Returns(_ministryPlatformRestRepository.Object);
            _ministryPlatformRestRepository.Setup(m => m.Search<MpLocation>(null, It.IsAny<string>(), null, false)).Returns(new List<MpLocation>() {_location1});

            var result =_fixture.GetLocations();

            _ministryPlatformRestRepository.VerifyAll();
            Assert.IsInstanceOf<IEnumerable<MpLocation>>(result);
            Assert.AreEqual(result.FirstOrDefault(), _location1);

        }
    }
}

using System.Collections.Generic;
using System.Web.Http.Results;
using crds_angular.Controllers.API;
using crds_angular.Models.Crossroads;
using crds_angular.Services.Interfaces;
using Crossroads.Web.Common.Security;
using Moq;
using NUnit.Framework;

namespace crds_angular.test.controllers
{
    [TestFixture]
    public class LocationControllerTest
    {
        private LocationController _fixture;

        private Mock<IAuthTokenExpiryService> _authTokenExpiryService;
        private Mock<ILocationService> _locationServiceMock;

        private LocationDTO _location1;
        private LocationDTO _location2;

        [SetUp]
        public void Setup()
        {
            _authTokenExpiryService = new Mock<IAuthTokenExpiryService>();
            _locationServiceMock = new Mock<ILocationService>();
            _location1 = new LocationDTO()
            {
                LocationId = 1,
                LocationName = "Location 1",
                Address = new AddressDTO("Address 1", null, "City 1", "State 1", "Zip 1", 1, 1)
            };
            _location2 = new LocationDTO()
            {
                LocationId = 2,
                LocationName = "Location 2",
                Address = new AddressDTO("Address 2", null, "City 2", "State 2", "Zip 2", 2, 2)
            };
            _fixture = new LocationController(_authTokenExpiryService.Object, 
                                              _locationServiceMock.Object, 
                                              new Mock<IUserImpersonationService>().Object, 
                                              new Mock<IAuthenticationRepository>().Object);
        }

        [Test]
        public void TestGet()
        {
            var locations = new List<LocationDTO> {_location1, _location2};
            _locationServiceMock.Setup(mocked => mocked.GetAllCrossroadsLocations()).Returns(locations);

            var result = _fixture.Get();

            _locationServiceMock.VerifyAll();
            Assert.IsNotNull(result);
            Assert.IsInstanceOf<OkNegotiatedContentResult<List<LocationDTO>>>(result);
        }

        [Test]
        public void TestGetProximities()
        {
            const string origin = "here";
            var locationDistances = new List<LocationProximityDto>
            {
                new LocationProximityDto()
                {
                    Location = _location1,
                    Origin = origin,
                    Distance = 1
                }
            };

            _locationServiceMock.Setup(mocked => mocked.GetDistanceToCrossroadsLocations(origin)).Returns(locationDistances);

            var result = _fixture.GetProximities(origin);

            _locationServiceMock.VerifyAll();
            Assert.IsNotNull(result);
            Assert.IsInstanceOf<OkNegotiatedContentResult<List<LocationProximityDto>>>(result);
        }

    }
}

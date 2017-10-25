using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web.Http.Results;
using crds_angular.Controllers.API;
using crds_angular.Models.Crossroads;
using crds_angular.Models.Crossroads.GoVolunteer;
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

        private Mock<ILocationService> _locationServiceMock;

        private OrgLocation _location1;
        private OrgLocation _location2;

        [SetUp]
        public void Setup()
        {
            _locationServiceMock = new Mock<ILocationService>();
            _location1 = new OrgLocation()
            {
                LocationId = 1,
                Address = "Address 1",
                City = "City 1",
                LocationName = "Location 1",
                State = "State 1",
                Zip = "Zip 1"
            };
            _location2 = new OrgLocation()
            {
                LocationId = 2,
                Address = "Address 2",
                City = "City 2",
                LocationName = "Location 2",
                State = "State 2",
                Zip = "Zip 2"
            };
            _fixture = new LocationController(_locationServiceMock.Object, new Mock<IUserImpersonationService>().Object, new Mock<IAuthenticationRepository>().Object);
        }

        [Test]
        public void TestGet()
        {
            var locations = new List<OrgLocation> {_location1, _location2};
            _locationServiceMock.Setup(mocked => mocked.GetAllCrossroadsLocations()).Returns(locations);

            var result = _fixture.Get();

            _locationServiceMock.VerifyAll();
            Assert.IsNotNull(result);
            Assert.IsInstanceOf<OkNegotiatedContentResult<List<OrgLocation>>>(result);
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

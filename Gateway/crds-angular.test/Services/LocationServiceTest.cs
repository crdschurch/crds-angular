using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using crds_angular.Models.Crossroads;
using crds_angular.Models.Crossroads.GoVolunteer;
using crds_angular.Services;
using crds_angular.Services.Interfaces;
using Crossroads.Web.Common.Configuration;
using Moq;
using Newtonsoft.Json;
using NUnit.Framework;

namespace crds_angular.test.Services
{
    [TestFixture]
    public class LocationServiceTest
    {
        private LocationService _fixture;

        private Mock<IConfigurationWrapper> _configurationWrapper;
        private Mock<IOrganizationService> _organizationService;
        private Mock<IAddressProximityService> _proximityService;

        private OrgLocation _location1;
        private OrgLocation _location2;

        private string origin = "here";

        [SetUp]
        public void Setup()
        {
            _configurationWrapper = new Mock<IConfigurationWrapper>();
            _organizationService = new Mock<IOrganizationService>();
            _proximityService = new Mock<IAddressProximityService>();
            _fixture = new LocationService(_configurationWrapper.Object, _organizationService.Object, _proximityService.Object);
            _configurationWrapper.Setup(mocked => mocked.GetConfigIntValue("CrossroadsOrgId")).Returns(2);
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
        }

        [Test]
        public void TestGetAllCrossroadsLocations()
        {
            var locations = new List<OrgLocation> { _location1, _location2 };
            _organizationService.Setup(mocked => mocked.GetLocationsForOrganization(2)).Returns(locations);
            var result = _fixture.GetAllCrossroadsLocations();
            _organizationService.VerifyAll();
            Assert.AreSame(locations, result);
        }

        [Test]
        public void TestGetDistanceToCrossroadsLocations()
        {
            var distance1 = 10;
            var distance2 = 5;
            var locations = new List<OrgLocation> { _location1, _location2 };
            _organizationService.Setup(mocked => mocked.GetLocationsForOrganization(2)).Returns(locations);
            AddressDTO location1Address = new AddressDTO()
            {
                AddressLine1 = _location1.Address,
                City = _location1.City,
                State = _location1.State,
                PostalCode = _location1.Zip
            };
            AddressDTO location2Address = new AddressDTO()
            {
                AddressLine1 = _location2.Address,
                City = _location2.City,
                State = _location2.State,
                PostalCode = _location2.Zip
            };
            var addresses = new List<AddressDTO> {location1Address, location2Address};
            _proximityService.Setup(mocked => mocked.GetProximity(origin, It.IsAny<List<AddressDTO>>(), null)).Returns(new List<decimal?> { distance1, distance2 });

            LocationProximityDto locationProximityDto1 = new LocationProximityDto()
            {
                Origin = origin,
                Distance = distance1,
                Location = _location1
            };
            LocationProximityDto locationProximityDto2 = new LocationProximityDto()
            {
                Origin = origin,
                Distance = distance2,
                Location = _location2
            };

            var expected = new List<LocationProximityDto> { locationProximityDto2, locationProximityDto1 };
            var result = _fixture.GetDistanceToCrossroadsLocations(origin);
            _proximityService.VerifyAll();
            Assert.AreEqual(JsonConvert.SerializeObject(expected), JsonConvert.SerializeObject(result)); 
        }
    }
}

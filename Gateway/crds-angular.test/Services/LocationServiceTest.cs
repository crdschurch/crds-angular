using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using crds_angular.App_Start;
using crds_angular.Models;
using crds_angular.Models.Crossroads;
using crds_angular.Services;
using crds_angular.Services.Interfaces;
using Crossroads.Web.Common.Configuration;
using MinistryPlatform.Translation.Models;
using MinistryPlatform.Translation.Repositories.Interfaces;
using Moq;
using Newtonsoft.Json;
using NUnit.Framework;

namespace crds_angular.test.Services
{
    [TestFixture]
    public class LocationServiceTest
    {
        private LocationService _fixture;
        
        private Mock<ILocationRepository> _locationRepository;
        private Mock<IAddressProximityService> _proximityService;

        private MpLocation _location1;
        private MpLocation _location2;
        private AddressDTO _address1;
        private AddressDTO _address2;
        private LocationDTO _locationDto1;
        private LocationDTO _locationDto2;

        private string origin = "here";

        [SetUp]
        public void Setup()
        {
            _locationRepository = new Mock<ILocationRepository>();
            _proximityService = new Mock<IAddressProximityService>();
            _fixture = new LocationService(_locationRepository.Object, _proximityService.Object);
            _location1 = new MpLocation()
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
            _location2 = new MpLocation()
            {
                LocationId = 2,
                AddressLine1 = "Address 2",
                City = "City 2",
                State = "State 2",
                Zip = "Zip 2",
                Longitude = 2,
                Latitude = 2,
                LocationName = "Location 2",
                ImageUrl = "Image 2"
            };
            _address1 = new AddressDTO()
            {
                AddressLine1 = _location1.AddressLine1,
                City = _location1.City,
                State = _location1.State,
                PostalCode = _location1.Zip,
                Longitude = _location1.Longitude,
                Latitude = _location1.Latitude
            };
            _address2 = new AddressDTO()
            {
                AddressLine1 = _location2.AddressLine1,
                City = _location2.City,
                State = _location2.State,
                PostalCode = _location2.Zip,
                Longitude = _location2.Longitude,
                Latitude = _location2.Latitude
            };
            _locationDto1 = new LocationDTO()
            {
                Address = _address1,
                ImageUrl = _location1.ImageUrl,
                LocationId = _location1.LocationId,
                LocationName = _location1.LocationName
            };
            _locationDto2 = new LocationDTO()
            {
                Address = _address2,
                ImageUrl = _location2.ImageUrl,
                LocationId = _location2.LocationId,
                LocationName = _location2.LocationName
            };
            AutoMapperConfig.RegisterMappings();
        }

        [Test]
        public void TestGetAllCrossroadsLocations()
        {
            var locations = new List<MpLocation> { _location1, _location2 };
            _locationRepository.Setup(mocked => mocked.GetLocations(It.IsAny<string>())).Returns(locations);
            var result = _fixture.GetAllCrossroadsLocations();
            _locationRepository.VerifyAll();
            var expected = new List<LocationDTO>() { _locationDto1, _locationDto2};
            Assert.AreEqual(JsonConvert.SerializeObject(expected), JsonConvert.SerializeObject(result));
        }

        [Test]
        public void TestGetDistanceToCrossroadsLocations()
        {
            var distance1 = 10;
            var distance2 = 5;
            var locations = new List<MpLocation> { _location1, _location2 };
            _locationRepository.Setup(mocked => mocked.GetLocations(It.IsAny<string>())).Returns(locations);
            var addresses = new List<AddressDTO> {_address1, _address2};
            _proximityService.Setup(mocked => mocked.GetProximity(origin, It.IsAny<List<AddressDTO>>(), null)).Returns(new List<decimal?> { distance1, distance2 });

            LocationProximityDto locationProximityDto1 = new LocationProximityDto()
            {
                Origin = origin,
                Distance = distance1,
                Location = _locationDto1
            };
            LocationProximityDto locationProximityDto2 = new LocationProximityDto()
            {
                Origin = origin,
                Distance = distance2,
                Location = _locationDto2
            };

            var expected = new List<LocationProximityDto> { locationProximityDto2, locationProximityDto1 };
            var result = _fixture.GetDistanceToCrossroadsLocations(origin);
            _proximityService.VerifyAll();
            Assert.AreEqual(JsonConvert.SerializeObject(expected), JsonConvert.SerializeObject(result)); 
        }
    }
}

using System.Collections.Generic;
using System.Device.Location;
using crds_angular.App_Start;
using crds_angular.Models.Crossroads;
using crds_angular.Services;
using crds_angular.Services.Interfaces;
using MinistryPlatform.Translation.Models;
using Moq;
using NUnit.Framework;
using MPServices = MinistryPlatform.Translation.Repositories.Interfaces;

namespace crds_angular.test.Services
{
    public class AddressServiceTest
    {

        private AddressService _fixture;
        private Mock<MPServices.IAddressRepository> _mpAddressServiceMock;
        private Mock<IAddressGeocodingService> _addressGeocodingService;

        [SetUp]
        public void SetUp()
        {
            AutoMapperConfig.RegisterMappings();

            _mpAddressServiceMock = new Mock<MPServices.IAddressRepository>(MockBehavior.Strict);
            _addressGeocodingService = new Mock<IAddressGeocodingService>(MockBehavior.Strict);

            _fixture = new AddressService(_mpAddressServiceMock.Object, _addressGeocodingService.Object);            
        }

        [Test]
        public void Given_An_Address_That_Exists_It_Should_Return_The_Existing_Address()
        {
            var address = new AddressDTO()
            {
                AddressLine1 = "123 Sesame St",
                AddressLine2 = "",
                City = "South Side",
                State = "OH",               
                PostalCode = "12312"
            };

            var addressResults = new List<MpAddress>()
            {
                new MpAddress()
                {
                    Address_ID = 12345
                },
                new MpAddress()
                {
                    Address_ID = 232323
                }
            };
            var coords = new GeoCoordinate(39.15946159999999, -84.42336390000003);
            _mpAddressServiceMock.Setup(mocked => mocked.FindMatches(It.IsAny<MpAddress>())).Returns(addressResults);
            _mpAddressServiceMock.Setup(mocked => mocked.Update(It.IsAny<MpAddress>())).Returns(12345);
            _addressGeocodingService.Setup(mocked => mocked.GetGeoCoordinates(It.IsAny<AddressDTO>())).Returns(coords);

            _fixture.FindOrCreateAddress(address);

            _mpAddressServiceMock.Verify(x => x.FindMatches(It.IsAny<MpAddress>()), Times.Once);
            _mpAddressServiceMock.Verify(x => x.Create(It.IsAny<MpAddress>()), Times.Never);
            Assert.AreEqual(address.AddressID, 12345);
        }

        [Test]
        public void TestFindExistingAddressWithoutGeoCoordinates()
        {
            var address = new AddressDTO()
            {
                AddressLine1 = "123 Sesame St",
                AddressLine2 = "",
                City = "South Side",
                State = "OH",
                PostalCode = "12312"
            };

            var addressResults = new List<MpAddress>()
            {
                new MpAddress()
                {
                    Address_ID = 12345
                },
                new MpAddress()
                {
                    Address_ID = 232323
                }
            };

            var coords = new GeoCoordinate(39.15946159999999, -84.42336390000003);
            _mpAddressServiceMock.Setup(mocked => mocked.FindMatches(It.IsAny<MpAddress>())).Returns(addressResults);
            _addressGeocodingService.Setup(mocked => mocked.GetGeoCoordinates(It.IsAny<AddressDTO>())).Returns(coords);
            _mpAddressServiceMock.Setup(mocked => mocked.Update(It.IsAny<MpAddress>())).Returns(67890);

            _fixture.FindOrCreateAddress(address, true);

            _mpAddressServiceMock.Verify(x => x.FindMatches(It.IsAny<MpAddress>()), Times.Once);
            _mpAddressServiceMock.Verify(x => x.Update(It.Is<MpAddress>(a => a.Latitude == coords.Latitude && a.Longitude == coords.Longitude)), Times.Exactly(1));
            _addressGeocodingService.Verify(mocked => mocked.GetGeoCoordinates(It.IsAny<AddressDTO>()));
            Assert.AreEqual(address.AddressID, 67890);
        }

        [Test]
        public void TestFindExistingAddressWithGeoCoordinates()
        {
            var address = new AddressDTO()
            {
                AddressLine1 = "123 Sesame St",
                AddressLine2 = "",
                City = "South Side",
                State = "OH",
                PostalCode = "12312"
            };

            var addressResults = new List<MpAddress>()
            {
                new MpAddress()
                {
                    Address_ID = 12345,
                    Latitude = 39.15946159999999,
                    Longitude = -84.42336390000003
                },
                new MpAddress()
                {
                    Address_ID = 232323
                }
            };
            var coords = new GeoCoordinate(39.15946159999999, -84.42336390000003);

            _mpAddressServiceMock.Setup(mocked => mocked.FindMatches(It.IsAny<MpAddress>())).Returns(addressResults);
            _addressGeocodingService.Setup(mocked => mocked.GetGeoCoordinates(It.IsAny<AddressDTO>())).Returns(coords);
            _mpAddressServiceMock.Setup(mocked => mocked.Update(It.IsAny<MpAddress>())).Returns(12345);
            _addressGeocodingService.Setup(mocked => mocked.GetGeoCoordinates(It.IsAny<AddressDTO>())).Returns(coords);

            _fixture.FindOrCreateAddress(address, true);

            _mpAddressServiceMock.Verify(x => x.FindMatches(It.IsAny<MpAddress>()), Times.Once);
            _mpAddressServiceMock.Verify(x => x.Update(It.IsAny<MpAddress>()), Times.Once);
            _addressGeocodingService.Verify(mocked => mocked.GetGeoCoordinates(It.IsAny<AddressDTO>()), Times.Once());
            Assert.AreEqual(address.AddressID, 12345);
        }

        [Test]
        public void Given_An_Address_That_Does_Not_Exist_It_Should_Create_A_New_Address()
        {
            var address = new AddressDTO()
            {
                AddressLine1 = "123 Sesame St",
                AddressLine2 = "",
                City = "South Side",
                State = "OH",
                PostalCode = "12312"
            };
            var coords = new GeoCoordinate(39.15946159999999, -84.42336390000003);
            var addressResults = new List<MpAddress>();

            _mpAddressServiceMock.Setup(mocked => mocked.FindMatches(It.IsAny<MpAddress>())).Returns(addressResults);
            _mpAddressServiceMock.Setup(mocked => mocked.Create(It.IsAny<MpAddress>())).Returns(12345);
            _addressGeocodingService.Setup(mocked => mocked.GetGeoCoordinates(It.IsAny<AddressDTO>())).Returns(coords);

            _fixture.FindOrCreateAddress(address);

            _mpAddressServiceMock.Verify(x => x.FindMatches(It.IsAny<MpAddress>()), Times.Once);
            _mpAddressServiceMock.Verify(x => x.Create(It.IsAny<MpAddress>()), Times.Once);
            
            Assert.AreEqual(address.AddressID, 12345);
        }

        [Test]
        public void TestFindNewAddressSetGeoCoordinates()
        {
            var address = new AddressDTO()
            {
                AddressLine1 = "123 Sesame St",
                AddressLine2 = "",
                City = "South Side",
                State = "OH",
                PostalCode = "12312"
            };

            var addressResults = new List<MpAddress>();
            var coords = new GeoCoordinate(39.15946159999999, -84.42336390000003);

            _mpAddressServiceMock.Setup(mocked => mocked.FindMatches(It.IsAny<MpAddress>())).Returns(addressResults);
            _addressGeocodingService.Setup(mocked => mocked.GetGeoCoordinates(It.IsAny<AddressDTO>())).Returns(coords);
            _mpAddressServiceMock.Setup(mocked => mocked.Create(It.IsAny<MpAddress>())).Returns(12345);

            _fixture.FindOrCreateAddress(address, true);

            _mpAddressServiceMock.Verify(x => x.FindMatches(It.IsAny<MpAddress>()), Times.Once);
            _mpAddressServiceMock.Verify(x => x.Create(It.Is<MpAddress>(a => a.Latitude == coords.Latitude && a.Longitude == coords.Longitude)), Times.Once);
            _addressGeocodingService.Verify(mocked => mocked.GetGeoCoordinates(It.IsAny<AddressDTO>()));

            Assert.AreEqual(address.AddressID, 12345);
        }

        [Test]
        public void TestFindNewAddressDoNotSetGeoCoordinates()
        {
            var address = new AddressDTO()
            {
                AddressLine1 = "123 Sesame St",
                AddressLine2 = "",
                City = "South Side",
                State = "OH",
                PostalCode = "12312"
            };

            var addressResults = new List<MpAddress>();

            _mpAddressServiceMock.Setup(mocked => mocked.FindMatches(It.IsAny<MpAddress>())).Returns(addressResults);
            _mpAddressServiceMock.Setup(mocked => mocked.Create(It.IsAny<MpAddress>())).Returns(12345);

            _fixture.FindOrCreateAddress(address);

            _mpAddressServiceMock.Verify(x => x.FindMatches(It.IsAny<MpAddress>()), Times.Once);
            _mpAddressServiceMock.Verify(x => x.Create(It.IsAny<MpAddress>()), Times.Once);
            _addressGeocodingService.Verify(mocked => mocked.GetGeoCoordinates(It.IsAny<AddressDTO>()), Times.Never);

            Assert.AreEqual(address.AddressID, 12345);
        }

        [Test]
        public void TestCreateAddress()
        {
            var address = new AddressDTO()
            {
                AddressLine1 = "123 Sesame St",
                AddressLine2 = "",
                City = "South Side",
                State = "OH",
                PostalCode = "12312"
            };

            var coords = new GeoCoordinate(39.15946159999999, -84.42336390000003);
            _addressGeocodingService.Setup(mocked => mocked.GetGeoCoordinates(It.IsAny<AddressDTO>())).Returns(coords);
            _mpAddressServiceMock.Setup(m => m.Create(It.IsAny<MpAddress>())).Returns(8675309);

            _fixture.CreateAddress(address);

            _mpAddressServiceMock.Verify(x => x.Create(It.IsAny<MpAddress>()), Times.Once);
        }
    }
}

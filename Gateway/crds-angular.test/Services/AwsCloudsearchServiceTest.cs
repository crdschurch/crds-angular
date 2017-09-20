using crds_angular.App_Start;
using crds_angular.Models.Finder;
using crds_angular.Services;
using crds_angular.Services.Interfaces;
using Moq;
using NUnit.Framework;
using MinistryPlatform.Translation.Repositories.Interfaces;
using Crossroads.Web.Common.Configuration;

namespace crds_angular.test.Services
{
    [Category("IntegrationTests")]
    public class AwsCloudsearchServiceTest
    {
        private AwsCloudsearchService _fixture;
        private Mock<IAddressGeocodingService> _addressGeocodingService;
        private Mock<IFinderRepository> _mpFinderRepository;
        private Mock<IConfigurationWrapper> _mpConfigurationWrapper;
        
        [SetUp]
        public void SetUp()
        {
            _addressGeocodingService = new Mock<IAddressGeocodingService>(); ;
            _mpFinderRepository = new Mock<IFinderRepository>();
            _mpConfigurationWrapper = new Mock<IConfigurationWrapper>();

            _fixture = new AwsCloudsearchService(_mpFinderRepository.Object, _mpConfigurationWrapper.Object);

            //force AutoMapper to register
            AutoMapperConfig.RegisterMappings();
        }

        [Test]
        public void ShouldReturnBoundingBox()
        {
            var rc = _fixture.BuildBoundingBox(new MapBoundingBox(39.1234, -84.51, 27.66, -81.55));
            Assert.IsNotNull(rc);
            Assert.IsTrue( rc.UpperLeftCoordinates.Lat == 39.1234);
        }
    }
}

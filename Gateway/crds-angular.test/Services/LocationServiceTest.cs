using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Moq;
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

        [SetUp]
        public void Setup()
        {
            _configurationWrapper = new Mock<IConfigurationWrapper>();
            _organizationService = new Mock<IOrganizationService>();
            _proximityService = new Mock<IAddressProximityService>();

            _configurationWrapper.Setup(mocked => mocked.GetConfigIntValue("CrossroadsOrgId")).Returns(2);
        }

        [Test]
        public void TestGetAllCrossroadsLocations()
        {
            throw new NotImplementedException();
        }

        [Test]
        public void TestGetDistanceToCrossroadsLocations()
        {
            throw new NotImplementedException();
        }
    }
}

using System.Net.Http;
using System.Net.Http.Headers;
using System.Web.Http.Controllers;
using crds_angular.Controllers.API;
using crds_angular.Services.Interfaces;
using Crossroads.Web.Common.Configuration;
using Crossroads.Web.Common.Security;
using Moq;
using NUnit.Framework;

namespace crds_angular.test.controllers
{
    public class FinderControllerTest
    {
        private FinderController _fixture;

        private Mock<IAuthTokenExpiryService> _authTokenExpiryService;
        private Mock<IAddressService> _addressService;
        private Mock<IAddressGeocodingService> _addressGeocodingService;
        private Mock<IGroupToolService> _groupToolService;
        private Mock<IFinderService> _finderService;
        private Mock<IUserImpersonationService> _userImpersonationService;
        private Mock<IAuthenticationRepository> _authenticationRepository;
        private Mock<IAwsCloudsearchService> _awsCloudsearchService;
        private Mock<IAnalyticsService> _analyticsService;
        private Mock<IConfigurationWrapper> _configurationWrapper;
        private string _authToken;
        private string _authType;

        [SetUp]
        public void SetUp()
        {
            _authTokenExpiryService = new Mock<IAuthTokenExpiryService>();
            _finderService = new Mock<IFinderService>();
            _userImpersonationService = new Mock<IUserImpersonationService>();
            _authenticationRepository = new Mock<IAuthenticationRepository>();
            _awsCloudsearchService = new Mock<IAwsCloudsearchService>();
            _groupToolService = new Mock<IGroupToolService>();
            _analyticsService = new Mock<IAnalyticsService>();
            _configurationWrapper = new Mock<IConfigurationWrapper>();

            _authType = "authType";
            _authToken = "authToken";

            _fixture = new FinderController(_authTokenExpiryService.Object,
                                            _finderService.Object,
                                            _groupToolService.Object,
                                            _userImpersonationService.Object,
                                            _authenticationRepository.Object,
                                            _awsCloudsearchService.Object,
                                            _analyticsService.Object,
                                            _configurationWrapper.Object)
            {
                Request = new HttpRequestMessage(),
                RequestContext = new HttpRequestContext()
            };

            _fixture.Request.Headers.Authorization = new AuthenticationHeaderValue(_authType, _authToken);

        }

        [Test]
        public void TestObjectInstantiates()
        {
            Assert.IsNotNull(_fixture);
        }
    }
}


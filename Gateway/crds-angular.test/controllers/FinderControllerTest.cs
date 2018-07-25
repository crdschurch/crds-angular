using System;
using System.Collections.Generic;
using System.Device.Location;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Web.Http;
using System.Web.Http.Controllers;
using System.Web.Http.Results;
using crds_angular.Controllers.API;
using crds_angular.Exceptions;
using crds_angular.Models.AwsCloudsearch;
using crds_angular.Models.Crossroads;
using crds_angular.Models.Finder;
using crds_angular.Services.Analytics;
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

        [Test]
        public void TestGetMyPinsByContactIdWithResults()
        {
            var fakeQueryParams = new PinSearchQueryParams();
            fakeQueryParams.CenterGeoCoords = new GeoCoordinates(39.123, -84.456);
            fakeQueryParams.ContactId = 12345;
            fakeQueryParams.FinderType = "CONNECT";
            var geoCoordinate = new GeoCoordinate(39.123, -84.456);
            var listPinDto = GetListOfPinDto();
            var address = new AddressDTO("123 Main st","","Independence","KY","41051",32,-84);

            _authTokenExpiryService.Setup(a => a.IsAuthtokenCloseToExpiry(It.IsAny<HttpRequestHeaders>())).Returns(true);
            _finderService.Setup(m => m.GetGeoCoordsFromAddressOrLatLang(It.IsAny<string>(), It.IsAny<GeoCoordinates>())).Returns(geoCoordinate);
            _finderService.Setup(m => m.GetMyPins(It.IsAny<string>(), It.IsAny<GeoCoordinate>(), It.IsAny<int>(), It.IsAny<string>())).Returns(listPinDto);
            _finderService.Setup(m => m.RandomizeLatLong(It.IsAny<AddressDTO>())).Returns(address);

            var response = _fixture.GetMyPinsByContactId(fakeQueryParams);

            Assert.IsNotNull(response);
            Assert.IsInstanceOf<OkNegotiatedContentResult<PinSearchResultsDto>>(response);
        }

        [Test]
        public void InviteToGroupShouldCallAnalytics()
        {   var token = "good ABC";
            var groupId = 1;
            var fakeInvite = new User()
            {
                email = "email@email.com"
            };
            _fixture.SetupAuthorization("good", "ABC");
 
            _finderService.Setup(m => m.InviteToGroup(
                It.Is<string>(toke => toke.Equals(token)), 
                It.Is<int>(id => id.Equals(groupId)),
                It.Is<User>(user => user.email == fakeInvite.email),
                It.Is<string>(connectType => connectType.Equals("connect"))
            ));

            _authenticationRepository.Setup(m => m.GetContactId(It.IsAny<string>())).Returns(12345);
            _authTokenExpiryService.Setup(a => a.IsAuthtokenCloseToExpiry(It.IsAny<HttpRequestHeaders>())).Returns(true);
            _analyticsService.Setup(m => m.Track(
                                        It.Is<string>(contactId => contactId.Equals("12345")),
                                        It.Is<string>(eventName => eventName.Equals("HostInvitationSent")),
                                        It.Is<EventProperties>(props => props["InvitationToEmail"].Equals(fakeInvite.email))
                                    ));

            _fixture.InviteToGroup(groupId, "connect", fakeInvite);
            
            _analyticsService.VerifyAll();
            _finderService.VerifyAll();
        }

        [Test]
        public void RequestToBeAHostShouldCallAnalytics()
        {
            var token = "good ABC";
            _fixture.SetupAuthorization("good", "ABC");
            var fakeRequest = new HostRequestDto()
            {
                Address = new AddressDTO()
                {
                    City = "City!",
                    State = "OH",
                    PostalCode = "12345"
                },
                ContactId = 42
            };

            _finderService.Setup(m => m.RequestToBeHost(
                It.Is<string>(toke => toke.Equals(token)),
                It.Is<HostRequestDto>(dto => 
                        dto.Address.City.Equals(fakeRequest.Address.City)
                        && dto.Address.State.Equals(fakeRequest.Address.State)
                        && dto.Address.PostalCode.Equals(fakeRequest.Address.PostalCode)
                        && dto.ContactId.Equals(fakeRequest.ContactId))
                ));
            _authTokenExpiryService.Setup(a => a.IsAuthtokenCloseToExpiry(It.IsAny<HttpRequestHeaders>())).Returns(true);
            _analyticsService.Setup(m => m.Track(
                                        It.Is<string>(contactId => contactId.Equals(fakeRequest.ContactId.ToString())),
                                        It.Is<string>(eventName => eventName.Equals("RegisteredAsHost")),
                                        It.Is<EventProperties>(props =>
                                                              props["City"].Equals(fakeRequest.Address.City)
                                                              && props["State"].Equals(fakeRequest.Address.State)
                                                              && props["Zip"].Equals(fakeRequest.Address.PostalCode))                                                                
                                        ));
            _fixture.RequestToBeHost(fakeRequest);
            _finderService.VerifyAll();
            _analyticsService.VerifyAll();
        }

        [Test]
        public void TestGetMyPinsByContactIdReturnsNothing()
        {

            var fakeQueryParams = new PinSearchQueryParams();
            fakeQueryParams.CenterGeoCoords = new GeoCoordinates(39.123, -84.456);
            fakeQueryParams.ContactId = 12345;
            fakeQueryParams.FinderType = "CONNECT";
            var geoCoordinate = new GeoCoordinate(39.123, -84.456);

            _authTokenExpiryService.Setup(a => a.IsAuthtokenCloseToExpiry(It.IsAny<HttpRequestHeaders>())).Returns(true);
            _finderService.Setup(m => m.GetGeoCoordsFromAddressOrLatLang(It.IsAny<string>(), It.IsAny<GeoCoordinates>())).Returns(geoCoordinate);
            _finderService.Setup(m => m.GetMyPins(It.IsAny<string>(), It.IsAny<GeoCoordinate>(), It.IsAny<int>(), It.IsAny<string>())).Returns(new List<PinDto>());

            var response = _fixture.GetMyPinsByContactId(fakeQueryParams) as OkNegotiatedContentResult<PinSearchResultsDto>;
            Assert.That(response != null && response.Content.PinSearchResults.Count == 0);
        }

        [Test]
        public void AddToGroupShouldUseRoleId()
        {
            var token = "good ABC";
            _fixture.SetupAuthorization("good", "ABC");
            var fakePerson = new User()
            {
               email = "fake@person.com",
               firstName = "fake",
               lastName = "person",
               password = "pass"
            };
            var groupId = 1;
            var roleId = 2;
            _authTokenExpiryService.Setup(a => a.IsAuthtokenCloseToExpiry(It.IsAny<HttpRequestHeaders>())).Returns(true);
            _finderService.Setup(m => m.AddUserDirectlyToGroup(It.Is<string>(toke => toke.Equals(token)),It.Is<User>(u => u.Equals(fakePerson)), 1, 2));

            _fixture.AddToGroup(groupId, fakePerson, roleId);
            _finderService.VerifyAll();
        }

        [Test]
        [ExpectedException(typeof(HttpResponseException))]
        public void TestNotAuthorized()
        {
            _authTokenExpiryService.Setup(a => a.IsAuthtokenCloseToExpiry(It.IsAny<HttpRequestHeaders>())).Returns(true);
            _authenticationRepository.Setup(mocked => mocked.GetContactId("abc")).Returns(123456);
            _fixture.EditGatheringPin(GetListOfPinDto()[0]);
        }

        private static List<PinDto> GetListOfPinDto()
        {
            var list = new List<PinDto>();

            var addr1 = new AddressDTO
            {
                Latitude = 30.1,
                Longitude = -80.1
            };
            var pin1 = new PinDto
            {
                Contact_ID = 1,
                FirstName = "pinhead",
                LastName = "One",
                Address = addr1
            };

            var addr2 = new AddressDTO
            {
                Latitude = 30.2,
                Longitude = -80.2
            };
            var pin2 = new PinDto
            {
                Contact_ID = 2,
                FirstName = "pinhead",
                LastName = "Two",
                Address = addr2
            };
            list.Add(pin1);
            list.Add(pin2);
            return list;
        }

    }
}


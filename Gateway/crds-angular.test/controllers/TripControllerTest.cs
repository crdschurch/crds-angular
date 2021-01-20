using System;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Web.Http;
using System.Web.Http.Controllers;
using System.Web.Http.Results;
using crds_angular.Controllers.API;
using crds_angular.Models.Crossroads.Trip;
using crds_angular.Services.Interfaces;
using Crossroads.Web.Common.Security;
using MinistryPlatform.Translation.Repositories.Interfaces;
using Moq;
using NUnit.Framework;

namespace crds_angular.test.controllers
{
    [TestFixture]
    public class TripControllerTest
    {
        private Mock<IAuthTokenExpiryService> _authTokenExpiryService;
        private readonly Mock<ITripService> _tripService;
        private readonly Mock<IContactRepository> _contactRepository;
        private readonly TripController _fixture;
        private readonly string authToken;
        private readonly string authType;

        public TripControllerTest()
        {
            _authTokenExpiryService = new Mock<IAuthTokenExpiryService>();
            _tripService = new Mock<ITripService>();
            _fixture = new TripController(_authTokenExpiryService.Object, 
                                          _tripService.Object, 
                                          new Mock<IUserImpersonationService>().Object, 
                                          new Mock<IAuthenticationRepository>().Object, 
                                          new Mock<IContactRepository>().Object );
            authType = "auth_type";
            authToken = "auth_token";
            _fixture.Request = new HttpRequestMessage();
            _fixture.Request.Headers.Authorization = new AuthenticationHeaderValue(authType, authToken);
            _fixture.RequestContext = new HttpRequestContext();
        }

        [Ignore]//At this point we haven't figured out how to mock the new Auth service
        public void ShouldGetCampaign()
        {
            const int pledgeCampaignId = 23;
            var dto = GetTripCampaignDto(pledgeCampaignId);
            _authTokenExpiryService.Setup(a => a.IsAuthtokenCloseToExpiry(It.IsAny<HttpRequestHeaders>())).Returns(true);
            _tripService.Setup(m => m.GetTripCampaign(pledgeCampaignId)).Returns(dto);
            var response = _fixture.GetCampaigns(pledgeCampaignId);
            _tripService.VerifyAll();
            Assert.IsNotNull(response);
            Assert.IsInstanceOf<OkNegotiatedContentResult<TripCampaignDto>>(response);
            var r = (OkNegotiatedContentResult<TripCampaignDto>)response;
            Assert.IsNotNull(r.Content);
            Assert.AreSame(dto, r.Content);
        }

        [Ignore]//At this point we haven't figured out how to mock the new Auth service
        public void ShouldThrowExceptionWhenGettingCampaign()
        {
            const int pledgeCampaignId = 23;
            var dto = GetTripCampaignDto(pledgeCampaignId);
            _authTokenExpiryService.Setup(a => a.IsAuthtokenCloseToExpiry(It.IsAny<HttpRequestHeaders>())).Returns(true);
            _tripService.Setup(m => m.GetTripCampaign(pledgeCampaignId)).Throws<InvalidOperationException>();
            Assert.Throws<HttpResponseException>(() =>
            {
                var response = _fixture.GetCampaigns(pledgeCampaignId);
                _tripService.VerifyAll();
                Assert.IsNotNull(response);
            });            
        }


        private TripCampaignDto GetTripCampaignDto(int pledgeCampaignId, bool isFull = false)
        {
            return new TripCampaignDto()
            {
                FormId = 12,
                Id = pledgeCampaignId,
                IsFull = isFull,
                Name = "Name",
                Nickname = "Blah",
                RegistrationDeposit = "500",
                RegistrationEnd = new DateTime(),
                RegistrationStart = new DateTime(),
                YoungestAgeAllowed = 5
            };
        }
    }
}

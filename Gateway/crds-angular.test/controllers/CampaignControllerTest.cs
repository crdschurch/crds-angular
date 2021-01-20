using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Web.Http.Controllers;
using System.Web.Http.Results;
using crds_angular.Controllers.API;
using crds_angular.Models.Crossroads.Campaign;
using crds_angular.Services.Interfaces;
using MinistryPlatform.Translation.Exceptions;
using Moq;
using NUnit.Framework;

namespace crds_angular.test.controllers
{
    [TestFixture]
    public class CampaignControllerTest
    {
        private Mock<IAuthTokenExpiryService> _authTokenExpiryService;
        private Mock<ICampaignService> _campaignServiceMock;
        private CampaignController _fixture;
        private readonly int _pledgeCampaignId = 24680;

        [SetUp]
        public void SetUp()
        {
            _authTokenExpiryService = new Mock<IAuthTokenExpiryService>();
            _campaignServiceMock = new Mock<ICampaignService>();

            _fixture = new CampaignController(_authTokenExpiryService.Object, _campaignServiceMock.Object, null, null);

            _fixture.Request = new HttpRequestMessage();
            _fixture.Request.Headers.Authorization = new AuthenticationHeaderValue("auth_type", "auth_token");
            _fixture.RequestContext = new HttpRequestContext();
        }

        [Test]
        public void ShouldReturnCampaignSummary()
        {
            _campaignServiceMock.Setup(m => m.GetSummary(_pledgeCampaignId)).Returns(new List<PledgeCampaignSummaryDto>());

            var result = _fixture.GetSummary(_pledgeCampaignId);
            Assert.IsInstanceOf<OkNegotiatedContentResult<List<PledgeCampaignSummaryDto>>>(result);
            var contentResult = (OkNegotiatedContentResult<List<PledgeCampaignSummaryDto>>)result;
            Assert.IsNotNull(contentResult.Content);

            _campaignServiceMock.VerifyAll();
        }

        [Test]
        public void ShouldReturnNotFoundStatus()
        {
            _campaignServiceMock.Setup(m => m.GetSummary(_pledgeCampaignId))
                .Throws(new PledgeCampaignNotFoundException(_pledgeCampaignId)
            );

            var result = _fixture.GetSummary(_pledgeCampaignId);
            Assert.IsInstanceOf<ResponseMessageResult>(result);
            var messageResult = (ResponseMessageResult)result;
            Assert.AreEqual(HttpStatusCode.NotFound, messageResult.Response.StatusCode);

            _campaignServiceMock.VerifyAll();
        }
    }
}

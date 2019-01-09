using System.Net.Http.Headers;
using System.Web.Http.Results;
using crds_angular.Controllers.API;
using crds_angular.Models.Crossroads.Profile;
using crds_angular.Services.Interfaces;
using Crossroads.Web.Common.Security;
using Moq;
using NUnit.Framework;

namespace crds_angular.test.controllers
{
    [TestFixture]
    public class ParticipantControllerTest
    {
        private ParticipantController _fixture;

        private Mock<IAuthTokenExpiryService> _authTokenExpiryService;
        private Mock<IGroupService> _groupService;
        private const string AuthType = "Bearer";
        private const string AuthToken = "tok123";


        [SetUp]
        public void SetUp()
        {
            _authTokenExpiryService = new Mock<IAuthTokenExpiryService>();
            _groupService = new Mock<IGroupService>(MockBehavior.Strict);
            _fixture = new ParticipantController(_authTokenExpiryService.Object, 
                                                 _groupService.Object, 
                                                 new Mock<IUserImpersonationService>().Object, 
                                                 new Mock<IAuthenticationRepository>().Object);

            //_fixture.SetupAuthorization(AuthType, AuthToken);
        }

        [Test]
        public void TestGetParticipant()
        {
            var participant = new Participant();
            _authTokenExpiryService.Setup(a => a.IsAuthtokenCloseToExpiry(It.IsAny<HttpRequestHeaders>())).Returns(true); 
            _groupService.Setup(mocked => mocked.GetParticipantRecord(string.Format("{0} {1}", AuthType, AuthToken))).Returns(participant);

            var result = _fixture.GetParticipant();
            Assert.IsNotNull(result);
            Assert.IsInstanceOf<OkNegotiatedContentResult<Participant>>(result);
            var okResult = (OkNegotiatedContentResult<Participant>) result;
            Assert.IsNotNull(okResult.Content);
            Assert.AreSame(participant, okResult.Content);
        }
    }
}

using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http.Headers;
using System.Web.Http;
using System.Web.Http.Results;
using crds_angular.Controllers.API;
using crds_angular.Exceptions;
using crds_angular.Models.Crossroads;
using crds_angular.Models.Crossroads.Groups;
using crds_angular.Models.Finder;
using crds_angular.Models.Json;
using crds_angular.Services.Analytics;
using crds_angular.Services.Interfaces;
using Crossroads.Utilities.Interfaces;
using Crossroads.Web.Common;
using Crossroads.Web.Common.Configuration;
using Crossroads.Web.Common.Security;
using MinistryPlatform.Translation.Models;
using Moq;
using NUnit.Framework;

namespace crds_angular.test.controllers
{
    public class GroupToolControllerTest
    {
        private GroupToolController _fixture;

        private Mock<IAuthTokenExpiryService> _authTokenExpiryService;
        private Mock<IGroupToolService> _groupToolService;
        private Mock<IGroupService> _groupService;
        private Mock<IConfigurationWrapper> _configurationWrapper;
        private Mock<IAnalyticsService> _mockAnalyticsService;

        private const string AuthType = "abc";
        private const string AuthToken = "123";
        private readonly string _auth = string.Format("{0} {1}", AuthType, AuthToken);

        private const int _trialMemberRoleId = 67;
        private const int _memberRoleId = 16;

        [SetUp]
        public void SetUp()
        {
            _authTokenExpiryService = new Mock<IAuthTokenExpiryService>();
            _groupToolService = new Mock<IGroupToolService>(MockBehavior.Strict);
            _groupService = new Mock<IGroupService>(MockBehavior.Strict);
            _configurationWrapper = new Mock<IConfigurationWrapper>();
            _mockAnalyticsService = new Mock<IAnalyticsService>();
            _configurationWrapper.Setup(mocked => mocked.GetConfigIntValue("SmallGroupTypeId")).Returns(1);
            _fixture = new GroupToolController(_authTokenExpiryService.Object,
                                               _groupToolService.Object, 
                                               _configurationWrapper.Object, 
                                               new Mock<IUserImpersonationService>().Object, 
                                               new Mock<IAuthenticationRepository>().Object, 
                                               _mockAnalyticsService.Object, 
                                               _groupService.Object);
            _fixture.SetupAuthorization(AuthType, AuthToken);

        }

        [Test]
        public void ShouldEndGroupSuccessfully()
        {
            var groupId = 9876;
            var groupReasonEndedId = 1;
            string token = "abc 123";
         
            _authTokenExpiryService.Setup(a => a.IsAuthtokenCloseToExpiry(It.IsAny<HttpRequestHeaders>())).Returns(true);
            _groupToolService.Setup(mocked => mocked.EndGroup(It.IsAny<int>(), It.IsAny<int>())).Verifiable();
            _groupToolService.Setup(mocked => mocked.VerifyCurrentUserIsGroupLeader(token, groupId)).Returns(new MyGroup());

            IHttpActionResult result = _fixture.EndSmallGroup(groupId);

            _groupToolService.VerifyAll();
            Assert.IsNotNull(result);
            Assert.IsInstanceOf(typeof(OkResult), result);

        }

        [Test]
        public void ShouldNotEndGroup()
        {
            var groupId = 9876;
            var groupReasonEndedId = 1;
            string token = "1234frd32";
            Exception ex = new Exception();
            _authTokenExpiryService.Setup(a => a.IsAuthtokenCloseToExpiry(It.IsAny<HttpRequestHeaders>())).Returns(true);
            _groupToolService.Setup(mocked => mocked.EndGroup(It.IsAny<int>(), It.IsAny<int>())).Throws(ex);
            _groupToolService.Setup(mocked => mocked.VerifyCurrentUserIsGroupLeader(It.IsAny<string>(), It.IsAny<int>())).Returns(new MyGroup());
            IHttpActionResult result = _fixture.EndSmallGroup(groupId);

            _groupToolService.VerifyAll();
            Assert.IsNotNull(result);
            Assert.IsInstanceOf(typeof(BadRequestResult), result);
        }

        [Test]
        public void ShouldNotEndGroupNotALeader()
        {
            var groupId = 1234;
            var groupReasonEndedId = 1;
            string token = "abc 123";
            _authTokenExpiryService.Setup(a => a.IsAuthtokenCloseToExpiry(It.IsAny<HttpRequestHeaders>())).Returns(true);
            _groupToolService.Setup(mocked => mocked.VerifyCurrentUserIsGroupLeader(It.IsAny<string>(), It.IsAny<int>())).Throws(new NotGroupLeaderException("User is not a leader"));

            IHttpActionResult result = _fixture.EndSmallGroup(groupId);

            _groupToolService.VerifyAll();
            Assert.IsNotNull(result);
            Assert.IsInstanceOf(typeof(BadRequestResult), result);
        }
    }
}

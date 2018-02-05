using System;
using System.Collections.Generic;
using System.Net;
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
            _groupToolService = new Mock<IGroupToolService>(MockBehavior.Strict);
            _groupService = new Mock<IGroupService>(MockBehavior.Strict);
            _configurationWrapper = new Mock<IConfigurationWrapper>();
            _mockAnalyticsService = new Mock<IAnalyticsService>();
            _configurationWrapper.Setup(mocked => mocked.GetConfigIntValue("SmallGroupTypeId")).Returns(1);
            _fixture = new GroupToolController(_groupToolService.Object, _configurationWrapper.Object, new Mock<IUserImpersonationService>().Object, new Mock<IAuthenticationRepository>().Object, _mockAnalyticsService.Object, _groupService.Object);
            _fixture.SetupAuthorization(AuthType, AuthToken);

        }

        [Test]
        public void TestRemoveParticipantFromMyGroup()
        {
            _groupService.Setup(mocked => mocked.RemoveParticipantFromGroup(_auth, 2, 3));
            var groupInfo = new GroupParticipantRemovalDto();
            groupInfo.GroupId = 2;
            groupInfo.GroupParticipantId = 3;
            var result = _fixture.RemoveSelfFromGroup(groupInfo);
            _groupService.VerifyAll();

            Assert.IsNotNull(result);
            Assert.IsInstanceOf<OkResult>(result);
        }

        [Test]
        public void TestRemoveSelfParticipantFromGroup()
        {
            _groupToolService.Setup(mocked => mocked.RemoveParticipantFromMyGroup(_auth, 2, 3, "test"));
            var result = _fixture.RemoveParticipantFromMyGroup(2, 3, "test");
            _groupToolService.VerifyAll();

            Assert.IsNotNull(result);
            Assert.IsInstanceOf<OkResult>(result);
        }

        [Test]
        public void TestRemoveParticipantFromMyGroupWithGroupParticipantRemovalException()
        {
            var ex = new GroupParticipantRemovalException("message");
            _groupToolService.Setup(mocked => mocked.RemoveParticipantFromMyGroup(_auth, 2, 3, "test")).Throws(ex);
            try
            {
                _fixture.RemoveParticipantFromMyGroup(2, 3, "test");
                Assert.Fail("Expected exception was not thrown");
            }
            catch (HttpResponseException e)
            {
                Assert.AreEqual(ex.StatusCode, e.Response.StatusCode);
            }

            _groupToolService.VerifyAll();
        }

        [Test]
        public void TestRemoveParticipantFromMyGroupWithOtherException()
        {
            var ex = new Exception();
            _groupToolService.Setup(mocked => mocked.RemoveParticipantFromMyGroup(_auth, 2, 3, "test")).Throws(ex);
            try
            {
                _fixture.RemoveParticipantFromMyGroup(2, 3, "test");
                Assert.Fail("Expected exception was not thrown");
            }
            catch (HttpResponseException e)
            {
                Assert.AreEqual(HttpStatusCode.BadRequest, e.Response.StatusCode);
            }

            _groupToolService.VerifyAll();
        }

	    [Test]
        public void TestPostGroupMessage()
        {
            _groupToolService.Setup(mocked => mocked.SendAllGroupParticipantsEmail(_auth, 123, 1, "subject", "message"));
            var result = _fixture.PostGroupMessage(123, 1, new GroupMessageDTO
            {
                Subject = "subject",
                Body = "message"
            });
            _groupToolService.VerifyAll();

            Assert.IsNotNull(result);
            Assert.IsInstanceOf<OkResult>(result);
        }

        [Test]
        public void ShouldEndGroupSuccessfully()
        {
            var groupId = 9876;
            var groupReasonEndedId = 1;
            string token = "abc 123";
         
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

            _groupToolService.Setup(mocked => mocked.VerifyCurrentUserIsGroupLeader(It.IsAny<string>(), It.IsAny<int>())).Throws(new NotGroupLeaderException("User is not a leader"));

            IHttpActionResult result = _fixture.EndSmallGroup(groupId);

            _groupToolService.VerifyAll();
            Assert.IsNotNull(result);
            Assert.IsInstanceOf(typeof(BadRequestResult), result);
        }
    }
}

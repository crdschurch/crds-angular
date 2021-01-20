using System;
using System.Collections.Generic;
using Crossroads.Web.Common.Configuration;
using Crossroads.Web.Common.MinistryPlatform;
using MinistryPlatform.Translation.Models.Childcare;
using MinistryPlatform.Translation.Repositories;
using MinistryPlatform.Translation.Repositories.Interfaces;
using Moq;
using NUnit.Framework;

namespace MinistryPlatform.Translation.Test.Services
{
    [TestFixture]
    public class ChildcareRequestServiceTest
    {
        private Mock<IEventRepository> _eventService;
        private Mock<IMinistryPlatformService> _ministryPlatformService;
        private Mock<IApiUserRepository> _apiUserService;
        private Mock<IConfigurationWrapper> _configuration;
        private Mock<IGroupRepository> _groupService;
        private ChildcareRequestRepository _fixture;
        private Mock<IMinistryPlatformRestRepository> _ministryPlatformRestService;

        private int _childcareRequestPage = 36;
        private int _childcareRequestPending = 3;

        [SetUp]
        public void Setup()
        {
            _ministryPlatformRestService = new Mock<IMinistryPlatformRestRepository>();
            _eventService = new Mock<IEventRepository>();
            _ministryPlatformService = new Mock<IMinistryPlatformService>();
            _apiUserService = new Mock<IApiUserRepository>();
            _apiUserService.Setup(m => m.GetDefaultApiClientToken()).Returns("useme");
            _eventService = new Mock<IEventRepository>();
            _groupService = new Mock<IGroupRepository>();
            _configuration = new Mock<IConfigurationWrapper>();
            _configuration.Setup(mocked => mocked.GetConfigIntValue("ChildcareRequestPageId")).Returns(_childcareRequestPage);
            _configuration.Setup(mocked => mocked.GetConfigIntValue("ChildcareRequestPending")).Returns(_childcareRequestPending);
            _fixture = new ChildcareRequestRepository(_configuration.Object, _ministryPlatformService.Object, _apiUserService.Object, _eventService.Object, _groupService.Object, _ministryPlatformRestService.Object);
        }

        [Test]
        public void CreateRequest()
        {
            var request = new MpChildcareRequest
            {
                RequesterId = 1,
                LocationId = 2,
                MinistryId = 3,
                GroupId = 4,
                StartDate = DateTime.Today,
                EndDate = DateTime.Today.AddDays(7),
                Frequency = "Weekly",
                PreferredTime = "8:00AM to 9:00AM",
                Notes = "This is a test request"
            };

            var requestDict = new Dictionary<string, object>
            {
                {"Requester_ID", request.RequesterId},
                {"Congregation_ID", request.LocationId},
                {"Ministry_ID", request.MinistryId},
                {"Group_ID", request.GroupId},
                {"Start_Date", request.StartDate},
                {"End_Date", request.EndDate},
                {"Frequency", request.Frequency},
                {"Childcare_Session", request.PreferredTime},
                {"Notes", request.Notes},
                {"Request_Status_ID", _childcareRequestPending}
            };

            _ministryPlatformService.Setup(m => m.CreateRecord(_childcareRequestPage, requestDict, It.IsAny<string>(), false));
            _fixture.CreateChildcareRequest(request);
            _ministryPlatformService.VerifyAll();
        }

        [Test]
        public void UpdateRequest()
        {
            var request = new MpChildcareRequest
            {
                ChildcareRequestId = 999,
                RequesterId = 1,
                LocationId = 2,
                MinistryId = 3,
                GroupId = 4,
                StartDate = DateTime.Today,
                EndDate = DateTime.Today.AddDays(7),
                Frequency = "Weekly",
                PreferredTime = "8:00AM to 9:00AM",
                Notes = "This is a test request"
            };

            var requestDict = new Dictionary<string, object>
            {
                {"Childcare_Request_ID", request.ChildcareRequestId },
                {"Requester_ID", request.RequesterId},
                {"Congregation_ID", request.LocationId},
                {"Ministry_ID", request.MinistryId},
                {"Group_ID", request.GroupId},
                {"Start_Date", request.StartDate},
                {"End_Date", request.EndDate},
                {"Frequency", request.Frequency},
                {"Childcare_Session", request.PreferredTime},
                {"Notes", request.Notes},
                {"Request_Status_ID", _childcareRequestPending}
            };

            _ministryPlatformService.Setup(m => m.UpdateRecord(_childcareRequestPage, requestDict, It.IsAny<string>()));
            _fixture.UpdateChildcareRequest(request);
            _ministryPlatformService.VerifyAll();
        }
    }


}

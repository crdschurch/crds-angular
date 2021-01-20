using System;
using System.Collections.Generic;
using System.Web.Http;
using System.Web.Http.Results;
using crds_angular.Controllers.API;
using crds_angular.Models.Crossroads.Events;
using crds_angular.Services.Interfaces;
using Crossroads.Web.Common.MinistryPlatform;
using Crossroads.Web.Common.Security;
using MinistryPlatform.Translation.Repositories.Interfaces;
using Moq;
using NUnit.Framework;
using IEventService = crds_angular.Services.Interfaces.IEventService;

namespace crds_angular.test.controllers
{
    [TestFixture]
    internal class EventLocationControllerTest
    {
        private EventController controller;

        private Mock<IAuthTokenExpiryService> _authTokenExpiryService;
        private Mock<IMinistryPlatformService> _ministryPlatfromServiceMock;
        private Mock<IApiUserRepository> _apiUserService;
        private Mock<IEventService> _eventService;



        [SetUp]
        public void SetUp()
        {
            _authTokenExpiryService = new Mock<IAuthTokenExpiryService>();
            _ministryPlatfromServiceMock = new Mock<IMinistryPlatformService>();

            _apiUserService = new Mock<IApiUserRepository>();
            _apiUserService.Setup(m => m.GetDefaultApiClientToken()).Returns("something");

            _eventService = new Mock<IEventService>();

            controller = new EventController(_authTokenExpiryService.Object, 
                                             _ministryPlatfromServiceMock.Object, 
                                             _apiUserService.Object, 
                                             _eventService.Object, 
                                             new Mock<IUserImpersonationService>().Object, 
                                             new Mock<IAuthenticationRepository>().Object);
        }


        [Test]
        public void ShouldReturnTodaysEvents()
        {
            const string site = "Oakley";

            var todaysEvents = new List<Dictionary<string, object>>
            {
                new Dictionary<string, object>
                {
                    {"Event_Start_Date", new DateTime(2015, 3, 30, 9, 30, 0)},
                    {"Event_Title", "Mock Event"},
                    {"Room_Name", "Mock Room"},
                    {"Room_Number", 999}
                }
            };

            _ministryPlatfromServiceMock.Setup(
                m => m.GetRecordsDict("TodaysEventLocationRecords", It.IsAny<string>(), site, "5 asc"))
                .Returns(todaysEvents);

            // Make the call...
            IHttpActionResult result = controller.Get(site);

            _apiUserService.VerifyAll();
            _ministryPlatfromServiceMock.VerifyAll();

            Assert.IsInstanceOf(typeof (OkNegotiatedContentResult<List<Event>>), result);
        }
    }
}
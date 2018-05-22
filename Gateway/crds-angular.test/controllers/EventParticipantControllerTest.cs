using System;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Web.Http.Controllers;
using System.Web.Http.Results;
using crds_angular.Controllers.API;
using crds_angular.Exceptions.Models;
using crds_angular.Models.Crossroads.Events;
using crds_angular.Models.Json;
using crds_angular.Services.Interfaces;
using crds_angular.test.Helpers;
using Crossroads.Web.Common.Security;
using Moq;
using NUnit.Framework;

namespace crds_angular.test.controllers
{
    public class EventParticipantControllerTest
    {
        private Mock<IAuthTokenExpiryService> _authTokenExpiryService;
        private readonly Mock<IEventParticipantService> _eventParticipantService;
        private readonly EventParticipantController _fixture;
        private readonly string authToken;
        private readonly string authType;

        public EventParticipantControllerTest()
        {
            Factories.EventParticipantDTO();
            _authTokenExpiryService = new Mock<IAuthTokenExpiryService>();
            _eventParticipantService = new Mock<IEventParticipantService>();
            _fixture = new EventParticipantController(_authTokenExpiryService.Object, 
                                                      _eventParticipantService.Object, 
                                                      new Mock<IUserImpersonationService>().Object, 
                                                      new Mock<IAuthenticationRepository>().Object);
            authType = "auth_type";
            authToken = "auth_token";
            _fixture.Request = new HttpRequestMessage();
            _fixture.Request.Headers.Authorization = new AuthenticationHeaderValue(authType, authToken);
            _fixture.RequestContext = new HttpRequestContext();
        }

        [TearDown]
        public void Cleanup()
        {
            _eventParticipantService.VerifyAll();
        }

        [Test]
        public void ContactShouldBeInterestedInEvent()
        {
            const int contactId = 2345;
            const int eventId = 6532;                        
            var dto = FactoryGirl.NET.FactoryGirl.Build<EventParticipantDTO>();
            _authTokenExpiryService.Setup(a => a.IsAuthtokenCloseToExpiry(It.IsAny<HttpRequestHeaders>())).Returns(true);
            _eventParticipantService.Setup(m => m.GetEventParticipantByContactAndEvent(contactId, eventId)).Returns(dto);
            _eventParticipantService.Setup(m => m.IsParticipantInvalid(dto)).Returns(false);

            var res = _fixture.IsParticipantValid(contactId, eventId);
            Assert.IsInstanceOf<OkResult>(res);
        }

        [Test]
        public void ContactShouldBeExpiredFromEvent()
        {
            const int contactId = 2345;
            const int eventId = 6532;
            const int interested = 5;
            var endDate = DateTime.Now.AddMinutes(-1);
            var dto = FactoryGirl.NET.FactoryGirl.Build<EventParticipantDTO>();
            _authTokenExpiryService.Setup(a => a.IsAuthtokenCloseToExpiry(It.IsAny<HttpRequestHeaders>())).Returns(true);
            _eventParticipantService.Setup(m => m.GetEventParticipantByContactAndEvent(contactId, eventId)).Returns(dto);
            _eventParticipantService.Setup(m => m.IsParticipantInvalid(dto)).Returns(true);

            var res = _fixture.IsParticipantValid(contactId, eventId);
            Assert.IsInstanceOf<NotFoundResult>(res);
        }

        [Test]
        public void IsContactInterestedThrowsException()
        {
            const int contactId = 2345;
            const int eventId = 6532;
            var endDate = DateTime.Now.AddMinutes(-1);
            var dto = FactoryGirl.NET.FactoryGirl.Build<EventParticipantDTO>();
            _authTokenExpiryService.Setup(a => a.IsAuthtokenCloseToExpiry(It.IsAny<HttpRequestHeaders>())).Returns(true);
            _eventParticipantService.Setup(m => m.GetEventParticipantByContactAndEvent(contactId, eventId)).Returns(dto);
            _eventParticipantService.Setup(m => m.IsParticipantInvalid(dto)).Throws<Exception>();
            var response = _fixture.IsParticipantValid(contactId, eventId);
            Assert.IsNotNull(response);
            Assert.IsInstanceOf<RestHttpActionResult<ApiErrorDto>>(response);
            var r = (RestHttpActionResult<ApiErrorDto>)response;
            Assert.AreEqual(HttpStatusCode.InternalServerError, r.StatusCode);
            Assert.IsNotNull(r.Content);
            Assert.AreEqual("Error determining if participant is interested", r.Content.Message);
        }
    }
}
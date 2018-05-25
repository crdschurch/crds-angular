using System;
using crds_angular.Models.Crossroads.Events;
using crds_angular.Services;
using crds_angular.test.Helpers;
using Crossroads.Utilities.FunctionalHelpers;
using Crossroads.Utilities.Interfaces;
using Crossroads.Web.Common;
using Crossroads.Web.Common.Configuration;
using Crossroads.Web.Common.MinistryPlatform;
using MinistryPlatform.Translation.Models;
using MinistryPlatform.Translation.Repositories.Interfaces;
using Moq;
using NUnit.Framework;

namespace crds_angular.test.Services
{
    public class EventParticipantServiceTest
    {

        private readonly Mock<IEventParticipantRepository> _eventParticipantRepository;
        private readonly Mock<IConfigurationWrapper> _configurationWrapper;
        private readonly Mock<IApiUserRepository> _apiUserRepository;
        private readonly EventParticipantService _fixture;

        public EventParticipantServiceTest()
        {            
            Factories.EventParticipant();
            Factories.EventParticipantDTO();           
            _eventParticipantRepository = new Mock<IEventParticipantRepository>();
            _configurationWrapper = new Mock<IConfigurationWrapper>();
            _apiUserRepository = new Mock<IApiUserRepository>();
            _fixture = new EventParticipantService(_eventParticipantRepository.Object, _configurationWrapper.Object, _apiUserRepository.Object);
        }

        [Test]
        public void ShouldFindAnEventParticipantBasedOnEventIdAndContactId()
        {
            const int contactId = 234;
            const int eventId = 2345;
            const string token = "asl;gh";
            var evtPar = FactoryGirl.NET.FactoryGirl.Build<MpEventParticipant>();
            var eventParticpant = new Ok<MpEventParticipant>(evtPar);

            _apiUserRepository.Setup(m => m.GetDefaultApiClientToken()).Returns(token);
            _eventParticipantRepository.Setup(m => m.GetEventParticipantByContactAndEvent(contactId, eventId, token)).Returns(eventParticpant);

            var res = _fixture.GetEventParticipantByContactAndEvent(contactId, eventId);
            Assert.IsInstanceOf<EventParticipantDTO>(res);
            Assert.AreEqual(evtPar.EventParticipantId, res.EventParticipantId);
        }

        [Test]
        public void ShouldHandleNoEventParticpantFound()
        {
            const int contactId = 234;
            const int eventId = 2345;
            const string token = "asl;gh";
            var evtPar = FactoryGirl.NET.FactoryGirl.Build<MpEventParticipant>();
            var eventParticpant = new Err<MpEventParticipant>("No Event Participant Found with those parameters");

            _apiUserRepository.Setup(m => m.GetDefaultApiClientToken()).Returns(token);
            _eventParticipantRepository.Setup(m => m.GetEventParticipantByContactAndEvent(contactId, eventId, token)).Returns(eventParticpant);


            Assert.Throws<ApplicationException>(() =>
            {
                var res = _fixture.GetEventParticipantByContactAndEvent(contactId, eventId);
            });
        }

        [Test]
        public void EventParticipantShouldNotBeExpired()
        {
            const int interested = 5;
            const int cancelled = 1;
            const int registered = 3;
            const int confirmed = 2;

            var endDate = DateTime.Now.AddMinutes(1);
            var evtPar = FactoryGirl.NET.FactoryGirl.Build<EventParticipantDTO>(x =>
            {
                x.ParticipationStatus = interested;
                x.EndDate = endDate;
            });
            _configurationWrapper.Setup(m => m.GetConfigIntValue("Participant_Status_Interested")).Returns(interested);
            _configurationWrapper.Setup(m => m.GetConfigIntValue("Participant_Status_Cancelled")).Returns(cancelled);
            _configurationWrapper.Setup(m => m.GetConfigIntValue("Participant_Status_Registered")).Returns(registered);
            _configurationWrapper.Setup(m => m.GetConfigIntValue("Participant_Status_Confirmed")).Returns(confirmed);

            var res = _fixture.IsParticipantInvalid(evtPar);
            Assert.IsFalse(res);
        }

        [Test]
        public void EventParticipantShouldBeExpired()
        {
            const int interested = 5;
            const int cancelled = 1;
            const int registered = 3;
            const int confirmed = 2;

            var endDate = DateTime.Now.AddMinutes(-1);
            var evtPar = FactoryGirl.NET.FactoryGirl.Build<EventParticipantDTO>(x =>
            {
                x.ParticipationStatus = interested;
                x.EndDate = endDate;
            });
            _configurationWrapper.Setup(m => m.GetConfigIntValue("Participant_Status_Interested")).Returns(interested);
            _configurationWrapper.Setup(m => m.GetConfigIntValue("Participant_Status_Cancelled")).Returns(cancelled);
            _configurationWrapper.Setup(m => m.GetConfigIntValue("Participant_Status_Registered")).Returns(registered);
            _configurationWrapper.Setup(m => m.GetConfigIntValue("Participant_Status_Confirmed")).Returns(confirmed);

            var res = _fixture.IsParticipantInvalid(evtPar);
            Assert.IsTrue(res);
        }

        [Test]
        public void EventParticipantShouldNotBeInterested()
        {
            const int interested = 5;
            const int cancelled = 1;
            const int registered = 3;
            const int confirmed = 2;

            var evtPar = FactoryGirl.NET.FactoryGirl.Build<EventParticipantDTO>(x =>
            {
                x.ParticipationStatus = registered;
                x.EndDate = null;
            });
            _configurationWrapper.Setup(m => m.GetConfigIntValue("Participant_Status_Interested")).Returns(interested);
            _configurationWrapper.Setup(m => m.GetConfigIntValue("Participant_Status_Cancelled")).Returns(cancelled);
            _configurationWrapper.Setup(m => m.GetConfigIntValue("Participant_Status_Registered")).Returns(registered);
            _configurationWrapper.Setup(m => m.GetConfigIntValue("Participant_Status_Confirmed")).Returns(confirmed);

            var res = _fixture.IsParticipantInvalid(evtPar);
            Assert.IsFalse(res);
        }

        [Test]
        public void EventParticipantShouldNotBeInterestedBecauseCancelled()
        {
            const int interested = 5;
            const int cancelled = 1;
            const int registered = 3;
            const int confirmed = 2;

            var evtPar = FactoryGirl.NET.FactoryGirl.Build<EventParticipantDTO>(x =>
            {
                x.ParticipationStatus = cancelled;
                x.EndDate = null;
            });
            _configurationWrapper.Setup(m => m.GetConfigIntValue("Participant_Status_Interested")).Returns(interested);
            _configurationWrapper.Setup(m => m.GetConfigIntValue("Participant_Status_Cancelled")).Returns(cancelled);
            _configurationWrapper.Setup(m => m.GetConfigIntValue("Participant_Status_Registered")).Returns(registered);
            _configurationWrapper.Setup(m => m.GetConfigIntValue("Participant_Status_Confirmed")).Returns(confirmed);

            var res = _fixture.IsParticipantInvalid(evtPar);
            Assert.IsTrue(res);
        }


        [TearDown]
        public void Cleanup()
        {
            _apiUserRepository.VerifyAll();
            _eventParticipantRepository.VerifyAll();
            _configurationWrapper.VerifyAll();
        }

    }
}

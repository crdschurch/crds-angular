using System;
using System.Collections.Generic;
using System.Linq;
using crds_angular.App_Start;
using crds_angular.Models.Crossroads.Events;
using crds_angular.Services;
using Crossroads.Utilities.Interfaces;
using Crossroads.Web.Common.Configuration;
using Crossroads.Web.Common.MinistryPlatform;
using MinistryPlatform.Translation.Models;
using MinistryPlatform.Translation.Repositories.Interfaces;
using MpEvent = MinistryPlatform.Translation.Models.MpEvent;
using IEventRepository = MinistryPlatform.Translation.Repositories.Interfaces.IEventRepository;
using IGroupRepository = MinistryPlatform.Translation.Repositories.Interfaces.IGroupRepository;
using Moq;
using MvcContrib.TestHelper.Ui;
using NUnit.Framework;
using MinistryPlatform.Translation.Models.EventReservations;

namespace crds_angular.test.Services
{
    [TestFixture]
    public class EventServiceTest
    {
        private Mock<IContactRelationshipRepository> _contactRelationshipService;
        private Mock<IContactRepository> _contactService;
        private Mock<IContentBlockService> _contentBlockService;
        private Mock<IEventRepository> _eventService;
        private Mock<IParticipantRepository> _participantService;
        private Mock<IGroupParticipantRepository> _groupParticipantService;
        private Mock<IGroupRepository> _groupService;
        private Mock<ICommunicationRepository> _communicationService;
        private Mock<IConfigurationWrapper> _configurationWrapper;
        private Mock<IApiUserRepository> _apiUserService;
        private Mock<IRoomRepository> _roomService;
        private Mock<IEquipmentRepository> _equipmentService;
        private Mock<IEventParticipantRepository> _eventParticipantService;
        private readonly int childcareEventTypeID;

        private EventService _fixture;

        [SetUp]
        public void SetUp()
        {
            AutoMapperConfig.RegisterMappings();

            _contactRelationshipService = new Mock<IContactRelationshipRepository>(MockBehavior.Strict);
            _configurationWrapper = new Mock<IConfigurationWrapper>(MockBehavior.Strict);
            _apiUserService = new Mock<IApiUserRepository>(MockBehavior.Strict);
            _contentBlockService = new Mock<IContentBlockService>(MockBehavior.Strict);
            _contactService = new Mock<IContactRepository>(MockBehavior.Strict);
            _groupService = new Mock<IGroupRepository>(MockBehavior.Strict);
            _communicationService = new Mock<ICommunicationRepository>(MockBehavior.Strict);
            _configurationWrapper = new Mock<IConfigurationWrapper>(MockBehavior.Strict);
            _apiUserService = new Mock<IApiUserRepository>(MockBehavior.Strict);
            _groupParticipantService = new Mock<IGroupParticipantRepository>(MockBehavior.Strict);
            _participantService = new Mock<IParticipantRepository>(MockBehavior.Strict);
            _eventService = new Mock<IEventRepository>();
            _roomService = new Mock<IRoomRepository>();
            _equipmentService = new Mock<IEquipmentRepository>();
            _eventParticipantService = new Mock<IEventParticipantRepository>(MockBehavior.Strict);


            _configurationWrapper = new Mock<IConfigurationWrapper>();
            _configurationWrapper.Setup(mocked => mocked.GetConfigIntValue("EventsReadyForPrimaryContactReminder")).Returns(2205);
            _configurationWrapper.Setup(mocked => mocked.GetConfigIntValue("EventPrimaryContactReminderTemplateId")).Returns(14909);
            _configurationWrapper.Setup(mocked => mocked.GetConfigIntValue("ChildcareEventType")).Returns(98765);
            _configurationWrapper.Setup(mocked => mocked.GetConfigIntValue("ChildcareGroupType")).Returns(272727);

            _fixture = new EventService(_eventService.Object,
                                        _groupService.Object,
                                        _communicationService.Object,
                                        _contactService.Object,
                                        _contentBlockService.Object,
                                        _configurationWrapper.Object,
                                        _apiUserService.Object,
                                        _contactRelationshipService.Object,
                                        _groupParticipantService.Object,
                                        _participantService.Object,
                                        _roomService.Object,
                                        _equipmentService.Object,
                                        _eventParticipantService.Object);
        }

        [Test]
        public void ShouldSendPrimaryContactReminderEmails()
        {
            const string search = "";
            const string apiToken = "qwerty1234";
            var defaultContact = new MpMyContact()
            {
                Contact_ID = 321,
                Email_Address = "default@email.com"
            };

            var testEvent = new MpEvent()
            {
                EventId = 32,
                EventStartDate = new DateTime(),
                EventEndDate = new DateTime().AddHours(2),
                PrimaryContact = new MpContact()
                {
                    EmailAddress = "test@test.com",
                    ContactId = 4321
                }
            };

            var testEventList = new List<MpEvent>()
            {
                testEvent
            };

            _apiUserService.Setup(m => m.GetDefaultApiClientToken()).Returns(apiToken);
            _eventService.Setup(m => m.EventsByPageViewId(apiToken, 2205, search)).Returns(testEventList);
            var eventList = testEventList.Select(evt => new crds_angular.Models.Crossroads.Events.Event()
            {
                name = evt.EventTitle,
                EventId = evt.EventId,
                EndDate = evt.EventEndDate,
                StartDate = evt.EventStartDate,
                EventType = evt.EventType,
                location = evt.Congregation,
                PrimaryContactEmailAddress = evt.PrimaryContact.EmailAddress,
                PrimaryContactId = evt.PrimaryContact.ContactId
            });

            eventList.ForEach(evt =>
            {
                var mergeData = new Dictionary<string, object>
                {
                    {"Event_ID", evt.EventId},
                    {"Event_Title", evt.name},
                    {"Event_Start_Date", evt.StartDate.ToShortDateString()},
                    {"Event_Start_Time", evt.StartDate.ToShortTimeString()}
                };

                var contact = new MpContact() {ContactId = defaultContact.Contact_ID, EmailAddress = defaultContact.Email_Address};
                var fakeCommunication = new MinistryPlatform.Translation.Models.MpCommunication()
                {
                    AuthorUserId = defaultContact.Contact_ID,
                    DomainId = 1,
                    EmailBody = "Test event email stuff",
                    EmailSubject = "Test Event Reminder",
                    FromContact = contact,
                    MergeData = mergeData,
                    ReplyToContact = contact,
                    TemplateId = 14909,
                    ToContacts = new List<MpContact>() {contact}
                };

                var testContact = new MpMyContact()
                {
                    Contact_ID = 9876,
                    Email_Address = "ghj@cr.net"

                };

                _contactService.Setup(m => m.GetContactById(9876)).Returns(testContact);
                _communicationService.Setup(m => m.GetTemplateAsCommunication(14909,
                                                                              testContact.Contact_ID,
                                                                              testContact.Email_Address,
                                                                              evt.PrimaryContactId,
                                                                              evt.PrimaryContactEmailAddress,
                                                                              evt.PrimaryContactId,
                                                                              evt.PrimaryContactEmailAddress,
                                                                              mergeData)).Returns(fakeCommunication);
                _communicationService.Setup(m => m.SendMessage(fakeCommunication, false));
                _communicationService.Verify();

            });
            _fixture.EventsReadyForPrimaryContactReminder(apiToken);
            _eventService.Verify();

        }

        [Test]
        public void TestAddEvent()
        {
            _eventService.Setup(mocked => mocked.CreateEvent(It.IsAny<MpEventReservationDto>())).Returns(123);
            var id = _fixture.AddEvent(GetEventToolTestObject());
            _eventService.VerifyAll();
            Assert.AreEqual(123, id, "Returned incorrect id");
        }

        [Test]
        public void TestUpdateEvent()
        {
            _eventService.Setup(mocked => mocked.UpdateEvent(It.IsAny<MpEventReservationDto>()));
            _fixture.UpdateEvent(GetEventToolTestObject(), 123);
            _eventService.VerifyAll();
            _eventService.Verify(x=>x.UpdateEvent(It.IsAny<MpEventReservationDto>()), Times.Once);
        }

        [Test]
        public void TestUpdateEventWithCancelledTrue()
        {
            _eventService.Setup(mocked => mocked.UpdateEvent(It.IsAny<MpEventReservationDto>()));
            var dto = GetEventToolTestObject();
            dto.Cancelled = true;
            _fixture.UpdateEvent(dto, 123);
            _eventService.VerifyAll();
            _eventService.Verify(mock => mock.UpdateEvent(It.IsAny<MpEventReservationDto>()), Times.Once);
        }

        private EventToolDto GetEventToolTestObject()
        {
            return new EventToolDto()
            {
                CongregationId = 1,
                ContactId = 1234,
                Description = "This is a description",
                DonationBatchTool = false,
                StartDateTime = new DateTime(2016, 12, 16, 10, 0, 0),
                EndDateTime = new DateTime(2016, 12, 16, 11, 0, 0),
                EventTypeId = 78,
                MeetingInstructions = "These are instructions",
                MinutesSetup = 0,
                MinutesTeardown = 0,
                ProgramId = 102,
                ReminderDaysId = 2,
                SendReminder = false,
                Title = "Test Event",
                ParticipantsExpected = 8
            };
        }

        private EventToolDto GetEventToolTestObjectWithRooms()
        {
            return new EventToolDto()
            {
                CongregationId = 1,
                ContactId = 1234,
                Description = "This is a description",
                DonationBatchTool = false,
                StartDateTime = new DateTime(2016, 12, 16, 10, 0, 0),
                EndDateTime = new DateTime(2016, 12, 16, 11, 0, 0),
                EventTypeId = 78,
                MeetingInstructions = "These are instructions",
                MinutesSetup = 0,
                MinutesTeardown = 0,
                ProgramId = 102,
                ReminderDaysId = 2,
                SendReminder = false,
                Title = "Test Event",
                ParticipantsExpected = 8,
                Rooms = new List<EventRoomDto>()
                {
                    new EventRoomDto()
                    {
                        Name = "Room1",
                        LayoutId = 1,
                        RoomId = 1, 
                        Cancelled = false,
                        RoomReservationId = 1,
                        Equipment = new List<EventRoomEquipmentDto>()
                        {
                            new EventRoomEquipmentDto()
                            {
                                Cancelled = false,
                                EquipmentId = 1, 
                                EquipmentReservationId = 1,
                                QuantityRequested = 10
                            }, 
                            new EventRoomEquipmentDto()
                            {
                                Cancelled = false,
                                EquipmentId = 2,
                                EquipmentReservationId = 2,
                                QuantityRequested = 42
                            }
                        }
                    },
                    new EventRoomDto()
                    {
                        Name = "Room2",
                        LayoutId = 1,
                        RoomId = 2,
                        Cancelled = false,
                        RoomReservationId = 2
                    }
                }
            };
        }
    }

}
      

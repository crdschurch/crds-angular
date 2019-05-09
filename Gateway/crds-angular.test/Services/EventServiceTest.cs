using System;
using System.Collections.Generic;
using System.Linq;
using AutoMapper;
using crds_angular.App_Start;
using crds_angular.Models.Crossroads.Events;
using crds_angular.Models.Crossroads.Groups;
using crds_angular.Services;
using Crossroads.Utilities.Interfaces;
using Crossroads.Web.Common;
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
using NUnit.Framework.Constraints;
using Rhino.Mocks;

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
        public void TestGetEventRoomDetails()
        {
            var e = new MpEvent
            {
                EventTitle = "title",
                CongregationId = 12,
                EventEndDate = DateTime.Today.AddDays(2),
                EventStartDate = DateTime.Today.AddDays(1),
                ReminderDaysPriorId = 34
            };
            _eventService.Setup(mocked => mocked.GetEvent(123)).Returns(e);

            var r = new List<MpRoomReservationDto>
            {
                new MpRoomReservationDto
                {
                    Cancelled = false,
                    Hidden = false,
                    RoomLayoutId = 1,
                    Notes = "notes 1",
                    RoomId = 11,
                    EventRoomId = 111,
                    Capacity = 1111,
                    CheckinAllowed = false,
                    Name = "name 1",
                    Label = "label 1"
                },
                new MpRoomReservationDto
                {
                    Cancelled = true,
                    Hidden = true,
                    RoomLayoutId = 2,
                    Notes = "notes 2",
                    RoomId = 22,
                    EventRoomId = 222,
                    Capacity = 2222,
                    CheckinAllowed = true,
                    Name = "name 2",
                    Label = "label 2"
                }
            };
            _roomService.Setup(mocked => mocked.GetRoomReservations(123)).Returns(r);

            var p = new List<List<MpEventParticipant>>
            {
                new List<MpEventParticipant>
                {
                    new MpEventParticipant()
                },
                new List<MpEventParticipant>
                {
                    new MpEventParticipant(),
                    new MpEventParticipant()
                }
            };

            var g = new List<MpEventGroup>
            {
                new MpEventGroup()
                {
                    GroupId = 1
                }
            };
            _eventParticipantService.Setup(mocked => mocked.GetEventParticipants(123, 11)).Returns(p[0]);
            _eventParticipantService.Setup(mocked => mocked.GetEventParticipants(123, 22)).Returns(p[1]);
            _eventService.Setup(mocked => mocked.GetEventGroupsForEvent(123)).Returns(g);
            _groupService.Setup(mocked => mocked.getGroupDetails(g[0].GroupId)).Returns(new MpGroup());

        var response = _fixture.GetEventRoomDetails(123);
            _eventService.VerifyAll();
            _roomService.VerifyAll();
            _equipmentService.VerifyAll();
            _eventParticipantService.VerifyAll();

            Assert.NotNull(response);
            Assert.AreEqual(e.EventTitle, response.Title);
            Assert.AreEqual(e.CongregationId, response.CongregationId);
            Assert.AreEqual(e.EventStartDate, response.StartDateTime);
            Assert.AreEqual(e.EventEndDate, response.EndDateTime);
            Assert.AreEqual(e.ReminderDaysPriorId, response.ReminderDaysId);
            Assert.IsNotNull(response.Rooms);
            Assert.AreEqual(r.Count, response.Rooms.Count);
            for (var i = 0; i < r.Count; i++)
            {
                Assert.AreEqual(r[i].Cancelled, response.Rooms[i].Cancelled);
                Assert.AreEqual(r[i].Hidden, response.Rooms[i].Hidden);
                Assert.AreEqual(r[i].RoomLayoutId, response.Rooms[i].LayoutId);
                Assert.AreEqual(r[i].Notes, response.Rooms[i].Notes);
                Assert.AreEqual(r[i].RoomId, response.Rooms[i].RoomId);
                Assert.AreEqual(r[i].EventRoomId, response.Rooms[i].RoomReservationId);
                Assert.AreEqual(r[i].Capacity, response.Rooms[i].Capacity);
                Assert.AreEqual(r[i].CheckinAllowed, response.Rooms[i].CheckinAllowed);
                Assert.AreEqual(r[i].Name, response.Rooms[i].Name);
                Assert.AreEqual(r[i].Label, response.Rooms[i].Label);
                Assert.AreEqual(p[i].Count, response.Rooms[i].ParticipantsAssigned);
            }
        }

        [Test]
        public void TestGetEventReservation()
        {
            var e = new MpEvent
            {
                EventTitle = "title",
                CongregationId = 12,
                EventEndDate = DateTime.Today.AddDays(2),
                EventStartDate = DateTime.Today.AddDays(1),
                ReminderDaysPriorId = 34
            };
            _eventService.Setup(mocked => mocked.GetEvent(123)).Returns(e);

            var r = new List<MpRoomReservationDto>
            {
                new MpRoomReservationDto
                {
                    Cancelled = false,
                    Hidden = false,
                    RoomLayoutId = 1,
                    Notes = "notes 1",
                    RoomId = 11,
                    EventRoomId = 111,
                    Capacity = 1111,
                    CheckinAllowed = false,
                    Name = "name 1",
                    Label = "label 1"
                },
                new MpRoomReservationDto
                {
                    Cancelled = true,
                    Hidden = true,
                    RoomLayoutId = 2,
                    Notes = "notes 2",
                    RoomId = 22,
                    EventRoomId = 222,
                    Capacity = 2222,
                    CheckinAllowed = true,
                    Name = "name 2",
                    Label = "label 2"
                }
            };
            _roomService.Setup(mocked => mocked.GetRoomReservations(123)).Returns(r);

            var q = new List<List<MpEquipmentReservationDto>>()
            {
                new List<MpEquipmentReservationDto>
                {
                    new MpEquipmentReservationDto
                    {
                        Cancelled = false,
                        EquipmentId = 1,
                        EventRoomId = 11,
                        QuantityRequested = 111
                    },
                    new MpEquipmentReservationDto
                    {
                        Cancelled = true,
                        EquipmentId = 2,
                        EventRoomId = 22,
                        QuantityRequested = 222
                    }

                },
                new List<MpEquipmentReservationDto>
                {
                    new MpEquipmentReservationDto
                    {
                        Cancelled = false,
                        EquipmentId = 3,
                        EventRoomId = 33,
                        QuantityRequested = 333
                    },
                    new MpEquipmentReservationDto
                    {
                        Cancelled = true,
                        EquipmentId = 4,
                        EventRoomId = 44,
                        QuantityRequested = 444
                    }
                }
            };

            var g = new List<MpEventGroup>
            {
                new MpEventGroup()
                {
                    GroupId = 1
                }
            };
            _equipmentService.Setup(mocked => mocked.GetEquipmentReservations(123, r[0].RoomId)).Returns(q[0]);
            _equipmentService.Setup(mocked => mocked.GetEquipmentReservations(123, r[1].RoomId)).Returns(q[1]);
            _eventService.Setup(mocked => mocked.GetEventGroupsForEvent(123)).Returns(g);
            _groupService.Setup(mocked => mocked.getGroupDetails(g[0].GroupId)).Returns(new MpGroup());

            var response = _fixture.GetEventReservation(123);
            _eventService.VerifyAll();
            _roomService.VerifyAll();
            _equipmentService.VerifyAll();

            Assert.NotNull(response);
            Assert.AreEqual(e.EventTitle, response.Title);
            Assert.AreEqual(e.CongregationId, response.CongregationId);
            Assert.AreEqual(e.EventStartDate, response.StartDateTime);
            Assert.AreEqual(e.EventEndDate, response.EndDateTime);
            Assert.AreEqual(e.ReminderDaysPriorId, response.ReminderDaysId);
            Assert.IsNotNull(response.Rooms);
            Assert.AreEqual(r.Count, response.Rooms.Count);
            for (var i = 0; i < r.Count; i++)
            {
                Assert.AreEqual(r[i].Cancelled, response.Rooms[i].Cancelled);
                Assert.AreEqual(r[i].Hidden, response.Rooms[i].Hidden);
                Assert.AreEqual(r[i].RoomLayoutId, response.Rooms[i].LayoutId);
                Assert.AreEqual(r[i].Notes, response.Rooms[i].Notes);
                Assert.AreEqual(r[i].RoomId, response.Rooms[i].RoomId);
                Assert.AreEqual(r[i].EventRoomId, response.Rooms[i].RoomReservationId);
                Assert.AreEqual(r[i].Capacity, response.Rooms[i].Capacity);
                Assert.AreEqual(r[i].CheckinAllowed, response.Rooms[i].CheckinAllowed);
                Assert.AreEqual(r[i].Name, response.Rooms[i].Name);
                Assert.AreEqual(r[i].Label, response.Rooms[i].Label);
                Assert.IsNotNull(response.Rooms[i].Equipment);
                Assert.AreEqual(q[i].Count, response.Rooms[i].Equipment.Count);
                for (var j = 0; j < q[i].Count; j++)
                {
                    Assert.AreEqual(q[i][j].Cancelled, response.Rooms[i].Equipment[j].Cancelled);
                    Assert.AreEqual(q[i][j].EquipmentId, response.Rooms[i].Equipment[j].EquipmentId);
                    Assert.AreEqual(q[i][j].EventEquipmentId, response.Rooms[i].Equipment[j].EquipmentReservationId);
                    Assert.AreEqual(q[i][j].QuantityRequested, response.Rooms[i].Equipment[j].QuantityRequested);
                }
            }
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

        [Test]
        public void TestUpdateEventReservation()
        {
            var newReservation = GetEventToolTestObjectWithRooms();
            var oldEventData = Mapper.Map<MpEvent>(newReservation);

            var roomReservationReturn = new List<MpRoomReservationDto>()
            {
                new MpRoomReservationDto()
                {
                    Name = "Room1",
                    Cancelled = false,
                    RoomId = 1,
                    EventId = 1,
                    EventRoomId = 1,
                    RoomLayoutId = 1,
                },
                new MpRoomReservationDto()
                {
                    Name = "Room2",
                    Cancelled = false,
                    RoomId = 2,
                    EventId = 1,
                    EventRoomId = 2,
                    RoomLayoutId = 1
                }
            };

            var equipmentForRoom1 = new List<MpEquipmentReservationDto>()
            {
                new MpEquipmentReservationDto()
                {
                    Cancelled = false,
                    RoomId = 1, 
                    EventId = 1,
                    QuantityRequested = 10,
                    EquipmentId = 1,
                    EventEquipmentId = 1,
                    EventRoomId = 1
                }, 
                new MpEquipmentReservationDto()
                {
                    Cancelled = false,
                    RoomId = 1,
                    EventId = 1,
                    QuantityRequested = 42,
                    EquipmentId = 2,
                    EventEquipmentId = 2,
                    EventRoomId = 2
                }
            };

            _eventService.Setup(mock => mock.GetEvent(1)).Returns(oldEventData);
            _roomService.Setup(mockyMock => mockyMock.GetRoomReservations(1)).Returns(roomReservationReturn);
            _equipmentService.Setup(mock => mock.GetEquipmentReservations(1, 1)).Returns(equipmentForRoom1);
            _equipmentService.Setup(mock => mock.GetEquipmentReservations(1, 2)).Returns(new List<MpEquipmentReservationDto>());
            _eventService.Setup(mock => mock.UpdateEvent(It.IsAny<MpEventReservationDto>()));
            _eventService.Setup(mocked => mocked.GetEventGroupsForEvent(1)).Returns(new List<MpEventGroup>());
            _roomService.Setup(mock => mock.UpdateRoomReservation(It.IsAny<MpRoomReservationDto>()));
            _equipmentService.Setup(mock => mock.UpdateEquipmentReservation(It.IsAny<MpEquipmentReservationDto>()));

            var result = _fixture.UpdateEventReservation(newReservation, 1);
            Assert.IsTrue(result);
            _equipmentService.Verify(m => m.UpdateEquipmentReservation(It.IsAny<MpEquipmentReservationDto>()), Times.Exactly(2));
            _roomService.Verify(m => m.UpdateRoomReservation(It.IsAny<MpRoomReservationDto>()), Times.Exactly(2));
            _eventService.Verify(m => m.UpdateEvent(It.IsAny<MpEventReservationDto>()), Times.Once);
        }

        [Test]
        public void TestUpdateEventReservationDateButNotRooms()
        {
            var newReservation = GetEventToolTestObjectWithRooms();
            var oldEventData = Mapper.Map<MpEvent>(newReservation);

            oldEventData.EventStartDate = oldEventData.EventStartDate.AddDays(-1);

            var roomReservationReturn = new List<MpRoomReservationDto>()
            {
                new MpRoomReservationDto()
                {
                    Name = "Room1",
                    Cancelled = false,
                    RoomId = 1,
                    EventId = 1,
                    EventRoomId = 1,
                    RoomLayoutId = 1,
                },
                new MpRoomReservationDto()
                {
                    Name = "Room2",
                    Cancelled = false,
                    RoomId = 2,
                    EventId = 1,
                    EventRoomId = 2,
                    RoomLayoutId = 1
                }
            };

            var equipmentForRoom1 = new List<MpEquipmentReservationDto>()
            {
                new MpEquipmentReservationDto()
                {
                    Cancelled = false,
                    RoomId = 1,
                    EventId = 1,
                    QuantityRequested = 10,
                    EquipmentId = 1,
                    EventEquipmentId = 1,
                    EventRoomId = 1
                },
                new MpEquipmentReservationDto()
                {
                    Cancelled = false,
                    RoomId = 1,
                    EventId = 1,
                    QuantityRequested = 42,
                    EquipmentId = 2,
                    EventEquipmentId = 2,
                    EventRoomId = 2
                }
            };

            _eventService.Setup(mock => mock.GetEvent(1)).Returns(oldEventData);
            _roomService.Setup(mockyMock => mockyMock.GetRoomReservations(1)).Returns(roomReservationReturn);
            _eventService.Setup(mocked => mocked.GetEventGroupsForEvent(1)).Returns(new List<MpEventGroup>());
            _equipmentService.Setup(mock => mock.GetEquipmentReservations(1, 1)).Returns(equipmentForRoom1);
            _equipmentService.Setup(mock => mock.GetEquipmentReservations(1, 2)).Returns(new List<MpEquipmentReservationDto>());
            _eventService.Setup(mock => mock.UpdateEvent(It.IsAny<MpEventReservationDto>()));
            _roomService.Setup(mock => mock.UpdateRoomReservation(It.IsAny<MpRoomReservationDto>()));
            _equipmentService.Setup(mock => mock.UpdateEquipmentReservation(It.IsAny<MpEquipmentReservationDto>()));

            var result = _fixture.UpdateEventReservation(newReservation, 1);
            Assert.IsTrue(result);
            _equipmentService.Verify(m => m.UpdateEquipmentReservation(It.IsAny<MpEquipmentReservationDto>()), Times.Exactly(4));
            _roomService.Verify(m => m.UpdateRoomReservation(It.IsAny<MpRoomReservationDto>()), Times.Exactly(4));
            _eventService.Verify(m => m.UpdateEvent(It.IsAny<MpEventReservationDto>()), Times.Once);
        }

        [Test]
        public void UpdateEventReservationShouldCancelRooms()
        {
            var newReservation = GetEventToolTestObjectWithRooms();
            var oldEventData = Mapper.Map<MpEvent>(newReservation);

            var roomReservationReturn = new List<MpRoomReservationDto>()
            {
                new MpRoomReservationDto()
                {
                    Name = "Room3",
                    Cancelled = false,
                    RoomId = 3,
                    EventId = 1,
                    EventRoomId = 3,
                    RoomLayoutId = 1,
                },
                new MpRoomReservationDto()
                {
                    Name = "Room4",
                    Cancelled = false,
                    RoomId = 4,
                    EventId = 1,
                    EventRoomId = 4,
                    RoomLayoutId = 1
                }
            };

            var equipmentForRoom1 = new List<MpEquipmentReservationDto>()
            {
                new MpEquipmentReservationDto()
                {
                    Cancelled = false,
                    RoomId = 1,
                    EventId = 1,
                    QuantityRequested = 10,
                    EquipmentId = 1,
                    EventEquipmentId = 1,
                    EventRoomId = 1
                },
                new MpEquipmentReservationDto()
                {
                    Cancelled = false,
                    RoomId = 1,
                    EventId = 1,
                    QuantityRequested = 42,
                    EquipmentId = 2,
                    EventEquipmentId = 2,
                    EventRoomId = 2
                }
            };

            _eventService.Setup(mock => mock.GetEvent(1)).Returns(oldEventData);
            _roomService.Setup(mockyMock => mockyMock.GetRoomReservations(1)).Returns(roomReservationReturn);
            _eventService.Setup(mocked => mocked.GetEventGroupsForEvent(1)).Returns(new List<MpEventGroup>());
            _equipmentService.Setup(mock => mock.GetEquipmentReservations(1, 3)).Returns(equipmentForRoom1);
            _equipmentService.Setup(mock => mock.GetEquipmentReservations(1, 4)).Returns(new List<MpEquipmentReservationDto>());
            _eventService.Setup(mock => mock.UpdateEvent(It.IsAny<MpEventReservationDto>()));
            _roomService.Setup(mock => mock.UpdateRoomReservation(It.IsAny<MpRoomReservationDto>()));
            _equipmentService.Setup(mock => mock.UpdateEquipmentReservation(It.IsAny<MpEquipmentReservationDto>()));

            var result = _fixture.UpdateEventReservation(newReservation, 1);
            Assert.IsTrue(result);
            _equipmentService.Verify(m => m.UpdateEquipmentReservation(It.IsAny<MpEquipmentReservationDto>()), Times.Exactly(4));
            _roomService.Verify(m => m.UpdateRoomReservation(It.IsAny<MpRoomReservationDto>()), Times.Exactly(4));
            _eventService.Verify(m => m.UpdateEvent(It.IsAny<MpEventReservationDto>()), Times.Once);
        }

        [Test]
        public void UpdateEventReservationToChildCareEvent()
        {
            var newReservation = GetEventToolTestObjectWithRooms();           
            var oldEventData = Mapper.Map<MpEvent>(newReservation);
            newReservation.EventTypeId = 98765;
            newReservation.Group = new GroupDTO()
            {
                CongregationId = 1
            };

            var roomReservationReturn = new List<MpRoomReservationDto>()
            {
                new MpRoomReservationDto()
                {
                    Name = "Room1",
                    Cancelled = false,
                    RoomId = 1,
                    EventId = 1,
                    EventRoomId = 1,
                    RoomLayoutId = 1,
                },
                new MpRoomReservationDto()
                {
                    Name = "Room2",
                    Cancelled = false,
                    RoomId = 2,
                    EventId = 1,
                    EventRoomId = 2,
                    RoomLayoutId = 1
                }
            };

            var equipmentForRoom1 = new List<MpEquipmentReservationDto>()
            {
                new MpEquipmentReservationDto()
                {
                    Cancelled = false,
                    RoomId = 1,
                    EventId = 1,
                    QuantityRequested = 10,
                    EquipmentId = 1,
                    EventEquipmentId = 1,
                    EventRoomId = 1
                },
                new MpEquipmentReservationDto()
                {
                    Cancelled = false,
                    RoomId = 1,
                    EventId = 1,
                    QuantityRequested = 42,
                    EquipmentId = 2,
                    EventEquipmentId = 2,
                    EventRoomId = 2
                }
            };

            _eventService.Setup(mock => mock.GetEvent(1)).Returns(oldEventData);
            _roomService.Setup(mockyMock => mockyMock.GetRoomReservations(1)).Returns(roomReservationReturn);
            _eventService.Setup(mocked => mocked.GetEventGroupsForEvent(1)).Returns(new List<MpEventGroup>());
            _equipmentService.Setup(mock => mock.GetEquipmentReservations(1, 1)).Returns(equipmentForRoom1);
            _equipmentService.Setup(mock => mock.GetEquipmentReservations(1, 2)).Returns(new List<MpEquipmentReservationDto>());
            _eventService.Setup(mock => mock.UpdateEvent(It.IsAny<MpEventReservationDto>()));
            _roomService.Setup(mock => mock.UpdateRoomReservation(It.IsAny<MpRoomReservationDto>()));
            _equipmentService.Setup(mock => mock.UpdateEquipmentReservation(It.IsAny<MpEquipmentReservationDto>()));
            _groupService.Setup(mock => mock.CreateGroup(It.IsAny<MpGroup>())).Returns(42);
            _eventService.Setup(mock => mock.CreateEventGroup(It.IsAny<MpEventGroup>(), It.IsAny<string>())).Returns(2);

            var result = _fixture.UpdateEventReservation(newReservation, 1);
            Assert.IsTrue(result);
            _equipmentService.Verify(m => m.UpdateEquipmentReservation(It.IsAny<MpEquipmentReservationDto>()), Times.Exactly(2));
            _roomService.Verify(m => m.UpdateRoomReservation(It.IsAny<MpRoomReservationDto>()), Times.Exactly(2));
            _eventService.Verify(m => m.UpdateEvent(It.IsAny<MpEventReservationDto>()), Times.Once);
            _groupService.Verify(m => m.CreateGroup(It.IsAny<MpGroup>()), Times.Once());
            _eventService.Verify(m => m.CreateEventGroup(It.IsAny<MpEventGroup>(), It.IsAny<string>()), Times.Once);
        }

        [Test]
        public void UpdateEventReservationFromChildCareToNot()
        {
            var newReservation = GetEventToolTestObjectWithRooms();
            var oldEventData = Mapper.Map<MpEvent>(newReservation);
            oldEventData.EventType = "98765";
            newReservation.Group = new GroupDTO()
            {
                CongregationId = 1
            };

            var roomReservationReturn = new List<MpRoomReservationDto>()
            {
                new MpRoomReservationDto()
                {
                    Name = "Room1",
                    Cancelled = false,
                    RoomId = 1,
                    EventId = 1,
                    EventRoomId = 1,
                    RoomLayoutId = 1,
                },
                new MpRoomReservationDto()
                {
                    Name = "Room2",
                    Cancelled = false,
                    RoomId = 2,
                    EventId = 1,
                    EventRoomId = 2,
                    RoomLayoutId = 1
                }
            };

            var equipmentForRoom1 = new List<MpEquipmentReservationDto>()
            {
                new MpEquipmentReservationDto()
                {
                    Cancelled = false,
                    RoomId = 1,
                    EventId = 1,
                    QuantityRequested = 10,
                    EquipmentId = 1,
                    EventEquipmentId = 1,
                    EventRoomId = 1
                },
                new MpEquipmentReservationDto()
                {
                    Cancelled = false,
                    RoomId = 1,
                    EventId = 1,
                    QuantityRequested = 42,
                    EquipmentId = 2,
                    EventEquipmentId = 2,
                    EventRoomId = 2
                }
            };

            var mpEventGroupData = new List<MpEventGroup>()
            {
                new MpEventGroup()
                {
                    EventId = 1,
                    Closed = false,
                    RoomId = 1,
                    EventRoomId = 2,
                    DomainId = 1,
                    GroupId = 42,
                    GroupName = "_childCare",
                    GroupTypeId = 272727,
                    Created = true,
                    EventGroupId = 1
                }
            };

            var mpGroup = new MpGroup()
            {
                Name = "ChildCareGroup",
                GroupId = 42,
                ChildCareAvailable = true,
                CongregationId = 1,
                KidsWelcome = true,
                TargetSize = 42,
                GroupType = 272727
            };

            _eventService.Setup(mock => mock.GetEvent(1)).Returns(oldEventData);
            _roomService.Setup(mockyMock => mockyMock.GetRoomReservations(1)).Returns(roomReservationReturn);
            _eventService.Setup(mock => mock.GetEventGroupsForEvent(1)).Returns(mpEventGroupData);
            _equipmentService.Setup(mock => mock.GetEquipmentReservations(1, 1)).Returns(equipmentForRoom1);
            _equipmentService.Setup(mock => mock.GetEquipmentReservations(1, 2)).Returns(new List<MpEquipmentReservationDto>());
            _eventService.Setup(mock => mock.UpdateEvent(It.IsAny<MpEventReservationDto>()));
            _roomService.Setup(mock => mock.UpdateRoomReservation(It.IsAny<MpRoomReservationDto>()));
            _equipmentService.Setup(mock => mock.UpdateEquipmentReservation(It.IsAny<MpEquipmentReservationDto>()));
            _eventService.Setup(mock => mock.DeleteEventGroupsForEvent(1, 272727));
            _groupService.Setup(mock => mock.EndDateGroup(42, null, null));
            _groupService.Setup(mock => mock.getGroupDetails(42)).Returns(mpGroup);

            var result = _fixture.UpdateEventReservation(newReservation, 1);
            Assert.IsTrue(result);
            _equipmentService.Verify(m => m.UpdateEquipmentReservation(It.IsAny<MpEquipmentReservationDto>()), Times.Exactly(2));
            _roomService.Verify(m => m.UpdateRoomReservation(It.IsAny<MpRoomReservationDto>()), Times.Exactly(2));
            _eventService.Verify(m => m.UpdateEvent(It.IsAny<MpEventReservationDto>()), Times.Once);
            _eventService.Verify(m => m.DeleteEventGroupsForEvent(1, 272727), Times.Once);
            _groupService.Verify(m => m.EndDateGroup(42, null, null), Times.Once);
        }

        [Test]
        public void UpdateEventReservationStaysChildCare()
        {
            var newReservation = GetEventToolTestObjectWithRooms();
            newReservation.EventTypeId = 98765;
            var oldEventData = Mapper.Map<MpEvent>(newReservation);
            newReservation.Group = new GroupDTO()
            {
                CongregationId = 1
            };

            var roomReservationReturn = new List<MpRoomReservationDto>()
            {
                new MpRoomReservationDto()
                {
                    Name = "Room1",
                    Cancelled = false,
                    RoomId = 1,
                    EventId = 1,
                    EventRoomId = 1,
                    RoomLayoutId = 1,
                },
                new MpRoomReservationDto()
                {
                    Name = "Room2",
                    Cancelled = false,
                    RoomId = 2,
                    EventId = 1,
                    EventRoomId = 2,
                    RoomLayoutId = 1
                }
            };

            var equipmentForRoom1 = new List<MpEquipmentReservationDto>()
            {
                new MpEquipmentReservationDto()
                {
                    Cancelled = false,
                    RoomId = 1,
                    EventId = 1,
                    QuantityRequested = 10,
                    EquipmentId = 1,
                    EventEquipmentId = 1,
                    EventRoomId = 1
                },
                new MpEquipmentReservationDto()
                {
                    Cancelled = false,
                    RoomId = 1,
                    EventId = 1,
                    QuantityRequested = 42,
                    EquipmentId = 2,
                    EventEquipmentId = 2,
                    EventRoomId = 2
                }
            };

            var mpEventGroupData = new List<MpEventGroup>()
            {
                new MpEventGroup()
                {
                    EventId = 1,
                    Closed = false,
                    RoomId = 1,
                    EventRoomId = 2,
                    DomainId = 1,
                    GroupId = 42,
                    GroupName = "_childCare",
                    GroupTypeId = 272727,
                    Created = true,
                    EventGroupId = 1
                }
            };

            var mpGroup = new MpGroup()
            {
                Name = "ChildCareGroup",
                GroupId = 42,
                ChildCareAvailable = true,
                CongregationId = 1,
                KidsWelcome = true,
                TargetSize = 42
            };

            _eventService.Setup(mock => mock.GetEvent(1)).Returns(oldEventData);
            _eventService.Setup(mock => mock.GetEventGroupsForEvent(1)).Returns(mpEventGroupData);
            _roomService.Setup(mockyMock => mockyMock.GetRoomReservations(1)).Returns(roomReservationReturn);
            _equipmentService.Setup(mock => mock.GetEquipmentReservations(1, 1)).Returns(equipmentForRoom1);
            _equipmentService.Setup(mock => mock.GetEquipmentReservations(1, 2)).Returns(new List<MpEquipmentReservationDto>());
            _eventService.Setup(mock => mock.UpdateEvent(It.IsAny<MpEventReservationDto>()));
            _roomService.Setup(mock => mock.UpdateRoomReservation(It.IsAny<MpRoomReservationDto>()));
            _equipmentService.Setup(mock => mock.UpdateEquipmentReservation(It.IsAny<MpEquipmentReservationDto>()));
            _groupService.Setup(mock => mock.getGroupDetails(42)).Returns(mpGroup);
            _groupService.Setup(mock => mock.UpdateGroup(It.IsAny<MpGroup>())).Returns(42);

            var result = _fixture.UpdateEventReservation(newReservation, 1);
            Assert.IsTrue(result);
            _equipmentService.Verify(m => m.UpdateEquipmentReservation(It.IsAny<MpEquipmentReservationDto>()), Times.Exactly(2));
            _roomService.Verify(m => m.UpdateRoomReservation(It.IsAny<MpRoomReservationDto>()), Times.Exactly(2));
            _eventService.Verify(m => m.UpdateEvent(It.IsAny<MpEventReservationDto>()), Times.Once);
            _groupService.Verify(m => m.getGroupDetails(42), Times.Once);
            _groupService.Verify(m => m.UpdateGroup(It.IsAny<MpGroup>()), Times.Once);
            _eventService.Verify(m => m.GetEventGroupsForEvent(1), Times.Exactly(2));

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
      

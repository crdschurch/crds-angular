﻿using System;
using System.Collections.Generic;
using System.Linq;
using AutoMapper;
using crds_angular.App_Start;
using crds_angular.Models.Crossroads;
using crds_angular.Models.Crossroads.Attribute;
using crds_angular.Models.Crossroads.Events;
using crds_angular.Models.Crossroads.Groups;
using crds_angular.Services.Interfaces;
using crds_angular.test.Models.Crossroads.Events;
using Crossroads.Utilities.Interfaces;
using MinistryPlatform.Translation.Exceptions;
using MinistryPlatform.Translation.Models;
using Moq;
using NUnit.Framework;
using MpAttribute = MinistryPlatform.Translation.Models.MpAttribute;
using MpEvent = MinistryPlatform.Translation.Models.MpEvent;
using GroupService = crds_angular.Services.GroupService;
using MPServices = MinistryPlatform.Translation.Repositories.Interfaces;
using IGroupRepository = MinistryPlatform.Translation.Repositories.Interfaces.IGroupRepository;
using Participant = MinistryPlatform.Translation.Models.Participant;

namespace crds_angular.test.Services
{
    public class GroupServiceTest
    {
        private GroupService fixture;
        private Mock<MPServices.IAuthenticationRepository> authenticationService;
        private Mock<MPServices.IGroupRepository> groupService;
        private Mock<MPServices.IEventRepository> eventService;
        private Mock<MPServices.IContactRelationshipRepository> contactRelationshipService;
        private Mock<IServeService> serveService;
        private Mock<IGroupRepository> _groupService;
        private Mock<MPServices.IParticipantRepository> participantService;
        private Mock<MPServices.ICommunicationRepository> _communicationService;
        private Mock<MPServices.IContactRepository> _contactService;
        private Mock<IConfigurationWrapper> config;
        private Mock<IObjectAttributeService> _objectAttributeService;
        private Mock<MPServices.IApiUserRepository> _apiUserService;
        private Mock<MPServices.IAttributeRepository> _attributeService;

        private readonly List<ParticipantSignup> mockParticipantSignup = new List<ParticipantSignup>
        {
            new ParticipantSignup()
            {
                particpantId = 999,
                childCareNeeded = false,
                SendConfirmationEmail = true
            },
            new ParticipantSignup()
            {
                particpantId = 888,
                childCareNeeded = false,
                SendConfirmationEmail = true
            }
        };

        private const int GROUP_ROLE_DEFAULT_ID = 123;
        private const int JOURNEY_GROUP_ID = 19;

        [SetUp]
        public void SetUp()
        {
            Mapper.Initialize(cfg => cfg.AddProfile<EventProfile>());
            AutoMapperConfig.RegisterMappings();

            authenticationService = new Mock<MPServices.IAuthenticationRepository>();
            groupService = new Mock<IGroupRepository>();
            eventService = new Mock<MPServices.IEventRepository>(MockBehavior.Strict);
            contactRelationshipService = new Mock<MPServices.IContactRelationshipRepository>();
            serveService = new Mock<IServeService>();
            participantService = new Mock<MPServices.IParticipantRepository>();
            _groupService = new Mock<IGroupRepository>();
            _communicationService = new Mock<MPServices.ICommunicationRepository>();
            _contactService = new Mock<MPServices.IContactRepository>();

            _objectAttributeService = new Mock<IObjectAttributeService>();
            _apiUserService = new Mock<MPServices.IApiUserRepository>();
            _attributeService = new Mock<MPServices.IAttributeRepository>();

            config = new Mock<IConfigurationWrapper>();

            config.Setup(mocked => mocked.GetConfigIntValue("Group_Role_Default_ID")).Returns(GROUP_ROLE_DEFAULT_ID);
            config.Setup(mocked => mocked.GetConfigIntValue("JourneyGroupId")).Returns(JOURNEY_GROUP_ID);

            fixture = new GroupService(groupService.Object,
                                       config.Object,
                                       eventService.Object,
                                       contactRelationshipService.Object,
                                       serveService.Object,
                                       participantService.Object,
                                       _communicationService.Object,
                                       _contactService.Object,
                                       _objectAttributeService.Object,
                                       _apiUserService.Object,
                                       _attributeService.Object);
        }

        [Test]
        public void shouldThrowExceptionWhenAddingToCommunityGroupIfGetGroupDetailsFails()
        {
            Exception exception = new Exception("Oh no, Mr. Bill!");
            groupService.Setup(mocked => mocked.getGroupDetails(456)).Throws(exception);

            try
            {
                fixture.addParticipantsToGroup(456, mockParticipantSignup);
                Assert.Fail("Expected exception was not thrown");
            }
            catch (Exception e)
            {
                Assert.IsInstanceOf(typeof(ApplicationException), e);
                Assert.AreSame(exception, e.InnerException);
            }

            groupService.VerifyAll();
        }

        [Test]
        public void shouldThrowCommunityGroupIsFullExceptionWhenGroupFullIndicatorIsSet()
        {
            var g = new MpGroup
            {
                TargetSize = 3,
                Full = true,
                Participants = new List<MpGroupParticipant>
                {
                    new MpGroupParticipant()
                }
            };
            groupService.Setup(mocked => mocked.getGroupDetails(456)).Returns(g);

            try
            {
                fixture.addParticipantsToGroup(456, mockParticipantSignup);
                Assert.Fail("Expected exception was not thrown");
            }
            catch (Exception e)
            {
                Assert.IsInstanceOf(typeof(GroupFullException), e);
            }

            groupService.VerifyAll();
        }

        [Test]
        public void shouldThrowCommunityGroupIsFullExceptionWhenNotEnoughSpaceRemaining()
        {
            var g = new MpGroup
            {
                TargetSize = 2,
                Full = false,
                Participants = new List<MpGroupParticipant>
                {
                    new MpGroupParticipant()
                }
            };
            groupService.Setup(mocked => mocked.getGroupDetails(456)).Returns(g);

            try
            {
                fixture.addParticipantsToGroup(456, mockParticipantSignup);
                Assert.Fail("Expected exception was not thrown");
            }
            catch (Exception e)
            {
                Assert.IsInstanceOf(typeof(GroupFullException), e);
            }

            groupService.VerifyAll();
        }

        [Test]
        public void shouldAddParticipantsToCommunityGroupAndEvents()
        {
            var g = new MpGroup
            {
                TargetSize = 0,
                Full = false,
                Participants = new List<MpGroupParticipant>()
            };
            groupService.Setup(mocked => mocked.getGroupDetails(456)).Returns(g);

            groupService.Setup(mocked => mocked.addParticipantToGroup(999, 456, GROUP_ROLE_DEFAULT_ID, false, It.IsAny<DateTime>(), null, false)).Returns(999456);
            groupService.Setup(mocked => mocked.addParticipantToGroup(888, 456, GROUP_ROLE_DEFAULT_ID, false, It.IsAny<DateTime>(), null, false)).Returns(888456);
            groupService.Setup(mocked => mocked.SendCommunityGroupConfirmationEmail(It.IsAny<int>(), 456, true, false));

            var events = new List<MpEvent>
            {
                new MpEvent {EventId = 777},
                new MpEvent {EventId = 555},
                new MpEvent {EventId = 444}
            };
            groupService.Setup(mocked => mocked.getAllEventsForGroup(456)).Returns(events);

            eventService.Setup(mocked => mocked.RegisterParticipantForEvent(999, 777, 456, 999456)).Returns(999777);
            eventService.Setup(mocked => mocked.RegisterParticipantForEvent(999, 555, 456, 999456)).Returns(999555);
            eventService.Setup(mocked => mocked.RegisterParticipantForEvent(999, 444, 456, 999456)).Returns(999444);

            eventService.Setup(mocked => mocked.RegisterParticipantForEvent(888, 777, 456, 888456)).Returns(888777);
            eventService.Setup(mocked => mocked.RegisterParticipantForEvent(888, 555, 456, 888456)).Returns(888555);
            eventService.Setup(mocked => mocked.RegisterParticipantForEvent(888, 444, 456, 888456)).Returns(888444);

            fixture.addParticipantsToGroup(456, mockParticipantSignup);

            groupService.VerifyAll();
            eventService.VerifyAll();
        }

        [Test]
        public void testGetGroupDetails()
        {
            var g = new MpGroup
            {
                TargetSize = 0,
                Full = true,
                Participants = new List<MpGroupParticipant>(),
                GroupType = 90210,
                WaitList = true,
                WaitListGroupId = 10101,
                GroupId = 98765
            };

            var eventList = new List<MpEvent>()
            {
                EventHelpers.TranslationEvent()
            };

            groupService.Setup(mocked => mocked.getGroupDetails(456)).Returns(g);

            groupService.Setup(mocked => mocked.getAllEventsForGroup(456)).Returns(eventList);

            var relations = new List<MpGroupSignupRelationships>
            {
                new MpGroupSignupRelationships {RelationshipId = 111}
            };
            groupService.Setup(mocked => mocked.GetGroupSignupRelations(90210)).Returns(relations);

            var contactRelations = new List<MpContactRelationship>
            {
                new MpContactRelationship
                {
                    Contact_Id = 333,
                    Relationship_Id = 111,
                    Participant_Id = 222
                }
            };
            contactRelationshipService.Setup(mocked => mocked.GetMyCurrentRelationships(777, "auth token")).Returns(contactRelations);

            var participant = new Participant
            {
                ParticipantId = 555,
            };
            groupService.Setup(mocked => mocked.checkIfUserInGroup(555, It.IsAny<List<MpGroupParticipant>>())).Returns(false);
            groupService.Setup(mocked => mocked.checkIfUserInGroup(222, It.IsAny<List<MpGroupParticipant>>())).Returns(false);

            var attributes = new ObjectAllAttributesDTO();
            _objectAttributeService.Setup(mocked => mocked.GetObjectAttributes(It.IsAny<string>(), It.IsAny<int>(), It.IsAny<MpObjectAttributeConfiguration>())).Returns(attributes);

            var response = fixture.getGroupDetails(456, 777, participant, "auth token");

            groupService.VerifyAll();
            contactRelationshipService.VerifyAll();

            Assert.IsNotNull(response);
            Assert.IsTrue(response.GroupFullInd);
            Assert.AreEqual(g.GroupId, response.GroupId);
            Assert.AreEqual(2, response.SignUpFamilyMembers.Count);
            Assert.AreEqual(g.WaitListGroupId, response.WaitListGroupId);
            Assert.AreEqual(g.WaitList, response.WaitListInd);
        }

        [Test]
        public void GetGroupsForParticipant()
        {
            const string token = "1234frd32";
            const int participantId = 54;

            var attributes = new ObjectAllAttributesDTO();

            groupService.Setup(mocked => mocked.GetGroupsForParticipant(token, participantId)).Returns(MockGroup());
            _objectAttributeService.Setup(mocked => mocked.GetObjectAttributes(token, It.IsAny<int>(), It.IsAny<MpObjectAttributeConfiguration>(), It.IsAny<List<MpAttribute>>())).Returns(attributes);

            var grps = fixture.GetGroupsForParticipant(token, participantId);

            groupService.VerifyAll();
            Assert.IsNotNull(grps);

        }

        [Test]
        public void GetGroupsByTypeForParticipant()
        {
            const string token = "1234frd32";
            const int participantId = 54;
            const int groupTypeId = 19;

            var groups = new List<MpGroup>()
            {
                new MpGroup
                {
                    GroupId = 321,
                    CongregationId = 5,
                    Name = "Test Journey Group 2016",
                    GroupRoleId = 16,
                    GroupDescription = "The group will test some new code",
                    MinistryId = 8,
                    ContactId = 4321,
                    GroupType = 19,
                    StartDate = Convert.ToDateTime("2016-02-12"),
                    EndDate = Convert.ToDateTime("2018-02-12"),
                    MeetingDayId = 3,
                    MeetingTime = "10:00",
                    AvailableOnline = false,
                    Address = new MpAddress()
                    {
                        Address_Line_1 = "123 Sesame St",
                        Address_Line_2 = "",
                        City = "South Side",
                        State = "OH",
                        Postal_Code = "12312"
                    }
                }
            };

            var attributes = new ObjectAllAttributesDTO();

            groupService.Setup(mocked => mocked.GetGroupsByTypeForParticipant(token, participantId, groupTypeId)).Returns(groups);
            _objectAttributeService.Setup(mocked => mocked.GetObjectAttributes(token, It.IsAny<int>(), It.IsAny<MpObjectAttributeConfiguration>(), It.IsAny<List<MpAttribute>>()))
                .Returns(attributes);

            var grps = fixture.GetGroupsByTypeForParticipant(token, participantId, groupTypeId);

            groupService.VerifyAll();
            Assert.IsNotNull(grps);
        }

        [Test]
        public void TestCreateGroup()
        {
            var start = DateTime.Now;
            var end = DateTime.Now.AddYears(2);

            var newGroup = new MpGroup()
            {
                Name = "New Testing Group",
                GroupDescription = "The best group ever created for testing stuff and things",
                GroupId = 145,
                GroupType = 19,
                MinistryId = 8,
                CongregationId = 1,
                StartDate = start,
                EndDate = end,
                Full = false,
                AvailableOnline = true,
                RemainingCapacity = 8,
                WaitList = false,
                ChildCareAvailable = false,
                MeetingDayId = 2,
                MeetingTime = "18000",
                GroupRoleId = 16
            };

            var group = new GroupDTO()
            {
                GroupName = "New Testing Group",
                GroupId = 145,
                GroupDescription = "The best group ever created for testing stuff and things",
                GroupTypeId = 19,
                MinistryId = 8,
                CongregationId = 1,
                StartDate = start,
                EndDate = end,
                GroupFullInd = false,
                AvailableOnline = true,
                RemainingCapacity = 8,
                WaitListInd = false,
                MeetingDayId = 2,
                MeetingTime = "18000",
                GroupRoleId = 16
            };

            groupService.Setup(mocked => mocked.CreateGroup(newGroup)).Returns(14);
            var groupResp = fixture.CreateGroup(group);

            _groupService.VerifyAll();
            Assert.IsNotNull(groupResp);
        }

        [Test]
        public void WhenLookupParticipantIsCalledWithAllParticipantIdSpecified_ShouldNotLookupParticipant()
        {
            fixture.LookupParticipantIfEmpty("123", mockParticipantSignup);

            participantService.Verify(x => x.GetParticipantRecord(It.IsAny<string>()), Times.Never);
        }

        [Test]
        public void WhenLookupParticipantIsCalledWithoutParticipantIdSpecified_ShouldLookupParticipantAndSetParticipantId()
        {
            var token = "123";
            var participant = new Participant() {ParticipantId = 100};

            participantService.Setup(x => x.GetParticipantRecord(token)).Returns(participant);
            var participants = new List<ParticipantSignup>
            {
                new ParticipantSignup()
                {
                    childCareNeeded = false,
                    SendConfirmationEmail = true
                },
            };
            fixture.LookupParticipantIfEmpty(token, participants);

            participantService.Verify(x => x.GetParticipantRecord(It.IsAny<string>()), Times.Once);

            Assert.AreEqual(100, participants[0].particpantId);
        }

        [Test]
        public void SendJourneyEmailInviteNoGroupsFound()
        {
            var groupId = 98765;
            const string token = "doit";
            var participant = new Participant() {ParticipantId = 100};

            var communication = new EmailCommunicationDTO()
            {
                emailAddress = "BlackWidow@marvel.com",
                groupId = 98765
            };

            participantService.Setup(x => x.GetParticipantRecord(token)).Returns(participant);

            Assert.Throws<InvalidOperationException>(() => fixture.SendJourneyEmailInvite(communication, token));
            _communicationService.Verify(x => x.SendMessage(It.IsAny<MpCommunication>(), false), Times.Never);
        }

        [Test]
        public void SendJourneyEmailInviteNoGroupMembershipFound()
        {
            var groupId = 98765;
            const string token = "doit";
            var participant = new Participant() {ParticipantId = 100};
            var communication = new EmailCommunicationDTO()
            {
                emailAddress = "BlackWidow@marvel.com",
                groupId = 98765
            };

            var groups = new List<MpGroup>()
            {
                new MpGroup() {}
            };

            participantService.Setup(x => x.GetParticipantRecord(token)).Returns(participant);
            var membership = groups.Where(group => group.GroupId == groupId).ToList();
            Assert.AreEqual(membership.Count, 0);
            Assert.Throws<InvalidOperationException>(() => fixture.SendJourneyEmailInvite(communication, token));
            _communicationService.Verify(x => x.SendMessage(It.IsAny<MpCommunication>(), false), Times.Never);
        }

        [Test]
        public void SendJourneyEmailInviteGroupMembershipIsFound()
        {
            const string token = "doit";
            const int groupId = 98765;
            var participant = new Participant() {ParticipantId = 100};

            var groups = new List<MpGroup>()
            {
                new MpGroup()
                {
                    GroupId = 98765
                }
            };

            var communication = new EmailCommunicationDTO()
            {
                emailAddress = "BlackWidow@marvel.com",
                groupId = 98765
            };

            var template = new MpMessageTemplate()
            {
                Subject = "You Can Join My Group",
                Body = "This is a journey group."
            };
            var contact = new MpMyContact()
            {
                Contact_ID = 7689
            };

            var attributes = new ObjectAllAttributesDTO();

            participantService.Setup(x => x.GetParticipantRecord(token)).Returns(participant);
            groupService.Setup(x => x.GetGroupsByTypeForParticipant(token, participant.ParticipantId, JOURNEY_GROUP_ID)).Returns(groups);
            _communicationService.Setup(mocked => mocked.GetTemplate(It.IsAny<int>())).Returns(template);
            _contactService.Setup(mocked => mocked.GetContactById(It.IsAny<int>())).Returns(contact);
            _objectAttributeService.Setup(mocked => mocked.GetObjectAttributes(token, It.IsAny<int>(), It.IsAny<MpObjectAttributeConfiguration>(), It.IsAny<List<MpAttribute>>()))
                .Returns(attributes);
            _communicationService.Setup(m => m.SendMessage(It.IsAny<MpCommunication>(), false)).Verifiable();

            var membership = groups.Where(group => group.GroupId == groupId).ToList();
            fixture.SendJourneyEmailInvite(communication, token);
            Assert.AreEqual(membership.Count, 1);
            _communicationService.Verify(m => m.SendMessage(It.IsAny<MpCommunication>(), false), Times.Once);
        }

        [Test]
        public void CanReturnSmallGroupsForAUser()
        {
            const string token = "JUSTDOITFOLLOWYOURDREAMS";

            var newGroupList = new List<MpGroup>()
            {
                new MpGroup()
                {
                    Name = "Awesome Sweet Small Group",
                    GroupDescription = "This is not the greatest group in the world no this is just a tribute",
                    GroupId = 1337,
                    GroupType = 1,
                    MinistryId = 0,
                    CongregationId = 0,
                    StartDate = DateTime.Now,
                    EndDate = null,
                    Full = false,
                    AvailableOnline = true,
                    RemainingCapacity = 8,
                    WaitList = false,
                    ChildCareAvailable = false,
                    MeetingDayId = null,
                    MeetingDay = "Monday",
                    MeetingTime = "19:30",
                    GroupRoleId = 0,
                    Address = new MpAddress()
                    {
                        Address_ID = null,
                        Address_Line_1 = "123 Place Street",
                        Address_Line_2 = null,
                        City = "CITY",
                        State = "OH",
                        Postal_Code = "45219",
                        Foreign_Country = null,
                        County = null
                    },
                    Participants = new List<MpGroupParticipant>()
                    {
                        new MpGroupParticipant()
                        {
                            ParticipantId = 123456,
                            ContactId = 456812,
                            GroupParticipantId = 0,
                            NickName = "Phillip J",
                            LastName = "Fry",
                            GroupRoleId = 0,
                            GroupRoleTitle = null,
                            Email = null
                        },
                        new MpGroupParticipant()
                        {
                            ParticipantId = 654321,
                            ContactId = 77777,
                            GroupParticipantId = 0,
                            NickName = "Leelah",
                            LastName = "Multipass",
                            GroupRoleId = 0,
                            GroupRoleTitle = null,
                            Email = null
                        }
                    }
                }
            };

            groupService.Setup(x => x.GetSmallGroupsForAuthenticatedUser(token)).Returns(newGroupList);

            var groups = fixture.GetSmallGroupsForAuthenticatedUser(token);
            Assert.AreEqual(groups.Count, 1);
            Assert.AreEqual(groups[0].GroupName, "Awesome Sweet Small Group");
        }

        public void shouldThrowGroupIsFullExceptionWhenGroupFullIndicatorIsSet()
        {
            var g = new MpGroup
            {
                TargetSize = 3,
                Full = true,
                Participants = new List<MpGroupParticipant>
                {
                    new MpGroupParticipant()
                }
            };
            groupService.Setup(mocked => mocked.getGroupDetails(456)).Returns(g);

            try
            {
                fixture.addParticipantToGroupNoEvents(456, mockParticipantSignup.FirstOrDefault());
                Assert.Fail("Expected exception was not thrown");
            }
            catch (Exception e)
            {
                Assert.IsInstanceOf(typeof(GroupFullException), e);
            }

            groupService.VerifyAll();
        }

        [Test]
        public void shouldAddParticipantsToCommunityGroupButNotEvents()
        {
            var g = new MpGroup
            {
                TargetSize = 0,
                Full = false,
                Participants = new List<MpGroupParticipant>()
            };
            groupService.Setup(mocked => mocked.getGroupDetails(456)).Returns(g);

            groupService.Setup(mocked => mocked.addParticipantToGroup(999, 456, GROUP_ROLE_DEFAULT_ID, false, It.IsAny<DateTime>(), null, false)).Returns(999456);
            
            fixture.addParticipantToGroupNoEvents(456, mockParticipantSignup.FirstOrDefault());

            groupService.VerifyAll();
        }


        private List<MpGroup> MockGroup()
        {
            var groups = new List<MpGroup>()
            {
                new MpGroup
                {
                    GroupId = 321,
                    CongregationId = 5,
                    Name = "Test Journey Group 2016",
                    GroupRoleId = 16,
                    GroupDescription = "The group will test some new code",
                    MinistryId = 8,
                    ContactId = 4321,
                    GroupType = 19,
                    StartDate = Convert.ToDateTime("2016-02-12"),
                    EndDate = Convert.ToDateTime("2018-02-12"),
                    MeetingDayId = 3,
                    MeetingTime = "10:00",
                    AvailableOnline = false,
                    Address = new MpAddress()
                    {
                        Address_Line_1 = "123 Sesame St",
                        Address_Line_2 = "",
                        City = "South Side",
                        State = "OH",
                        Postal_Code = "12312"
                    }
                }
            };

            return groups;

        }
    }
}

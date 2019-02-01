using System;
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
using Crossroads.Web.Common.Configuration;
using Crossroads.Web.Common.MinistryPlatform;
using Crossroads.Web.Common.Security;
using MinistryPlatform.Translation.Exceptions;
using MinistryPlatform.Translation.Models;
using Moq;
using NUnit.Framework;
using MpAttribute = MinistryPlatform.Translation.Models.MpAttribute;
using MpEvent = MinistryPlatform.Translation.Models.MpEvent;
using GroupService = crds_angular.Services.GroupService;
using MPServices = MinistryPlatform.Translation.Repositories.Interfaces;
using IGroupRepository = MinistryPlatform.Translation.Repositories.Interfaces.IGroupRepository;
using Participant = MinistryPlatform.Translation.Models.MpParticipant;
using crds_angular.Util.Interfaces;
using Amazon.CloudSearchDomain.Model;

namespace crds_angular.test.Services
{
    public class GroupServiceTest
    {
        private GroupService fixture;
        private Mock<IAwsCloudsearchService> _awsCloudsearchService;
        private Mock<IAuthenticationRepository> authenticationService;
        private Mock<MPServices.IGroupRepository> groupRepository;
        private Mock<MPServices.IEventRepository> eventService;
        private Mock<MPServices.IContactRelationshipRepository> contactRelationshipService;
        private Mock<IServeService> serveService;
        private Mock<IGroupRepository> _groupRepository;
        private Mock<MPServices.IParticipantRepository> participantService;
        private Mock<MPServices.ICommunicationRepository> _communicationService;
        private Mock<MPServices.IContactRepository> _contactService;
        private Mock<IConfigurationWrapper> config;
        private Mock<IObjectAttributeService> _objectAttributeService;
        private Mock<IApiUserRepository> _apiUserService;
        private Mock<MPServices.IAttributeRepository> _attributeRepository;
        private Mock<IEmailCommunication> _emailCommunicationService;
        private Mock<MPServices.IUserRepository> _userRespository;
        private Mock<MPServices.IInvitationRepository> _invitationRepository;
        private Mock<IAttributeService> _attributeService;
        private Mock<IDateTime> _dateTimeWrapper;

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
        private const int GROUP_ROLE_LEADER = 22;
        private const int DOMAIN_ID = 66;
        private const int ONSITE_GROUP_ID = 8;
        private const int GroupCategoryAttributeTypeId = 10;
        private const int GroupTypeAttributeTypeId = 20;
        private const int GroupAgeRangeAttributeTypeId = 30;
        private const int DefaultAuthorUser = 5;
        private const int RemoveSelfFromGroupTemplateId = 2024;
        private const int _memberRoleId = 16;

        [SetUp]
        public void SetUp()
        {
            Mapper.Initialize(cfg => cfg.AddProfile<EventProfile>());
            AutoMapperConfig.RegisterMappings();

            _awsCloudsearchService = new Mock<IAwsCloudsearchService>();
            authenticationService = new Mock<IAuthenticationRepository>();
            groupRepository = new Mock<IGroupRepository>();
            eventService = new Mock<MPServices.IEventRepository>(MockBehavior.Strict);
            contactRelationshipService = new Mock<MPServices.IContactRelationshipRepository>();
            serveService = new Mock<IServeService>();
            participantService = new Mock<MPServices.IParticipantRepository>();
            _groupRepository = new Mock<IGroupRepository>();
            _communicationService = new Mock<MPServices.ICommunicationRepository>();
            _contactService = new Mock<MPServices.IContactRepository>();
            _emailCommunicationService = new Mock<IEmailCommunication>();
            _userRespository = new Mock<MPServices.IUserRepository>();
            _invitationRepository = new Mock<MPServices.IInvitationRepository>();

            _objectAttributeService = new Mock<IObjectAttributeService>();
            _apiUserService = new Mock<IApiUserRepository>();
            _attributeRepository = new Mock<MPServices.IAttributeRepository>();
            _attributeService = new Mock<IAttributeService>();

            _dateTimeWrapper = new Mock<IDateTime>();

            config = new Mock<IConfigurationWrapper>();

            config.Setup(mocked => mocked.GetConfigIntValue("GroupRoleLeader")).Returns(GROUP_ROLE_LEADER);
            config.Setup(mocked => mocked.GetConfigIntValue("OnsiteGroupTypeId")).Returns(ONSITE_GROUP_ID);
            config.Setup(mocked => mocked.GetConfigIntValue("Group_Role_Default_ID")).Returns(GROUP_ROLE_DEFAULT_ID);
            config.Setup(mocked => mocked.GetConfigIntValue("JourneyGroupTypeId")).Returns(JOURNEY_GROUP_ID);
            config.Setup(mocked => mocked.GetConfigIntValue("DomainId")).Returns(DOMAIN_ID);
            config.Setup(mocked => mocked.GetConfigIntValue("GroupCategoryAttributeTypeId")).Returns(GroupCategoryAttributeTypeId);
            config.Setup(mocked => mocked.GetConfigIntValue("GroupTypeAttributeTypeId")).Returns(GroupTypeAttributeTypeId);
            config.Setup(mocked => mocked.GetConfigIntValue("GroupAgeRangeAttributeTypeId")).Returns(GroupAgeRangeAttributeTypeId);
            config.Setup(mocked => mocked.GetConfigIntValue("DefaultAuthorUser")).Returns(DefaultAuthorUser);
            config.Setup(mocked => mocked.GetConfigIntValue("RemoveSelfFromGroupTemplateId")).Returns(RemoveSelfFromGroupTemplateId);

            fixture = new GroupService(_awsCloudsearchService.Object, 
                                        groupRepository.Object,
                                       config.Object,
                                       eventService.Object,
                                       contactRelationshipService.Object,
                                       serveService.Object,
                                       participantService.Object,
                                       _communicationService.Object,
                                       _contactService.Object,
                                       _objectAttributeService.Object,
                                       _apiUserService.Object,
                                       _attributeRepository.Object,
                                       _emailCommunicationService.Object,
                                       _userRespository.Object,
                                       _invitationRepository.Object,
                                       _attributeService.Object,
                                       _dateTimeWrapper.Object);
                                    
        }

        [Test]
        public void TestAddContactToGroup()
        {
            const int groupId = 123;
            const int contactId = 456;
            var participant = new Participant
            {
                ParticipantId = 789
            };
            const int groupParticipantId = 987;

            participantService.Setup(mocked => mocked.GetParticipant(contactId)).Returns(participant);
            groupRepository.Setup(mocked => mocked.AddParticipantToGroup(participant.ParticipantId, groupId, GROUP_ROLE_DEFAULT_ID, false, false, It.IsAny<DateTime>(), null, null))
                .Returns(groupParticipantId);

            fixture.addContactToGroup(groupId, contactId, GROUP_ROLE_DEFAULT_ID);
            participantService.VerifyAll();
            groupRepository.VerifyAll();
        }

        [Test]
        public void TestAddContactToGroupCantGetParticipant()
        {
            const int groupId = 123;
            const int contactId = 456;

            var ex = new Exception("DOH!!!!!");

            participantService.Setup(mocked => mocked.GetParticipant(contactId)).Throws(ex);

            try
            {
                fixture.addContactToGroup(groupId, contactId, _memberRoleId);
                Assert.Fail("expected exception was not thrown");
            }
            catch (ApplicationException e)
            {
                Assert.AreSame(ex, e.InnerException);
            }

            participantService.VerifyAll();
            groupRepository.VerifyAll();
        }

        [Test]
        public void TestAddContactToGroupCantAddContact()
        {
            const int groupId = 123;
            const int contactId = 456;
            var participant = new Participant
            {
                ParticipantId = 789
            };

            var ex = new ApplicationException("DOH!!!!!");

            participantService.Setup(mocked => mocked.GetParticipant(contactId)).Returns(participant);
            groupRepository.Setup(mocked => mocked.AddParticipantToGroup(participant.ParticipantId, groupId, GROUP_ROLE_DEFAULT_ID, false, false, It.IsAny<DateTime>(), null, null))
                .Throws(ex);

            try
            {
                fixture.addContactToGroup(groupId, contactId, GROUP_ROLE_DEFAULT_ID);
                Assert.Fail("expected exception was not thrown");
            }
            catch (ApplicationException e)
            {
                Assert.AreSame(ex, e);
            }
            participantService.VerifyAll();
            groupRepository.VerifyAll();
        }

        [Test]
        public void RemoveOnsiteParticipantsIfNotLeader()
        {
            string token = "123ABC";
            Participant loggedInParticipant = new Participant()
            {
                ParticipantId = 1
            };
            var groups = new List<GroupDTO>()
            {
                new GroupDTO()
                {
                    GroupId = 1,
                    Participants = new List<GroupParticipantDTO>()
                    {
                        new GroupParticipantDTO()
                        {
                            ParticipantId = loggedInParticipant.ParticipantId,
                            GroupRoleId = GROUP_ROLE_LEADER
                        },
                        new GroupParticipantDTO()
                        {
                            ParticipantId = 2,
                            GroupRoleId = 1
                        },
                        new GroupParticipantDTO()
                        {
                            ParticipantId = 2,
                            GroupRoleId = 1
                        }
                    },
                    GroupTypeId = ONSITE_GROUP_ID
                },
                new GroupDTO()
                {
                    GroupId = 2,
                    Participants = new List<GroupParticipantDTO>()
                    {
                        new GroupParticipantDTO()
                        {
                            ParticipantId = loggedInParticipant.ParticipantId,
                            GroupRoleId = 1
                        },
                        new GroupParticipantDTO()
                        {
                            ParticipantId = 2,
                            GroupRoleId = GROUP_ROLE_LEADER
                        },
                        new GroupParticipantDTO()
                        {
                            ParticipantId = 2,
                            GroupRoleId = 1
                        }
                    },
                    GroupTypeId = JOURNEY_GROUP_ID
                },
                new GroupDTO()
                {
                    GroupId = 3,
                    Participants = new List<GroupParticipantDTO>()
                    {
                        new GroupParticipantDTO()
                        {
                            ParticipantId = loggedInParticipant.ParticipantId,
                            GroupRoleId = 1
                        },
                        new GroupParticipantDTO()
                        {
                            ParticipantId = 2,
                            GroupRoleId = GROUP_ROLE_LEADER
                        },
                        new GroupParticipantDTO()
                        {
                            ParticipantId = 2,
                            GroupRoleId = 1
                        }
                    },
                    GroupTypeId = ONSITE_GROUP_ID
                }
            };

            participantService.Setup(mocked => mocked.GetParticipantRecord(It.IsAny<string>())).Returns(loggedInParticipant);

            var results = fixture.RemoveOnsiteParticipantsIfNotLeader(groups, token);

            //Is leader of onsite group, no participants should be removed
            Assert.AreEqual(results.Find(g => g.GroupId == 1).Participants.Count(), 3);
            //Is not leader of journey group, no participants should be removed
            Assert.AreEqual(results.Find(g => g.GroupId == 2).Participants.Count(), 3);
            //Is not leader of onsite group, all participants should be removed
            Assert.AreEqual(results.Find(g => g.GroupId == 3).Participants.Count(), 0);

            participantService.VerifyAll();
        }

        [Test]
        public void shouldThrowExceptionWhenAddingToCommunityGroupIfGetGroupDetailsFails()
        {
            Exception exception = new Exception("Oh no, Mr. Bill!");
            groupRepository.Setup(mocked => mocked.getGroupDetails(456)).Throws(exception);

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

            groupRepository.VerifyAll();
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
            groupRepository.Setup(mocked => mocked.getGroupDetails(456)).Returns(g);

            try
            {
                fixture.addParticipantsToGroup(456, mockParticipantSignup);
                Assert.Fail("Expected exception was not thrown");
            }
            catch (Exception e)
            {
                Assert.IsInstanceOf(typeof(GroupFullException), e);
            }

            groupRepository.VerifyAll();
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
            groupRepository.Setup(mocked => mocked.getGroupDetails(456)).Returns(g);

            try
            {
                fixture.addParticipantsToGroup(456, mockParticipantSignup);
                Assert.Fail("Expected exception was not thrown");
            }
            catch (Exception e)
            {
                Assert.IsInstanceOf(typeof(GroupFullException), e);
            }

            groupRepository.VerifyAll();
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
            groupRepository.Setup(mocked => mocked.getGroupDetails(456)).Returns(g);
            groupRepository.Setup(mocked => mocked.SendCommunityGroupConfirmationEmail(It.IsAny<int>(), 456, true, false));

            var events = new List<MpEvent>
            {
                new MpEvent {EventId = 777},
                new MpEvent {EventId = 555},
                new MpEvent {EventId = 444}
            };

            
            DateTime mockDateTime = new DateTime(2025, 4, 18, 8, 23, 56);
            _dateTimeWrapper.Setup(m => m.Today).Returns(mockDateTime.Date);

            groupRepository.Setup(mocked => mocked.getAllEventsForGroup(456, mockDateTime.Date, false)).Returns(events);
            groupRepository.Setup(mocked => mocked.GetParticipantGroupMemberId(456,999)).Returns(999456);
            groupRepository.Setup(mocked => mocked.GetParticipantGroupMemberId(456, 888)).Returns(-1);
            groupRepository.Setup(mocked => mocked.AddParticipantToGroup(888, 456, GROUP_ROLE_DEFAULT_ID, false, false, It.IsAny<DateTime>(), null, null)).Returns(888456);

            eventService.Setup(mocked => mocked.SafeRegisterParticipant(777, 999, 456, 999456)).Returns(999777);
            eventService.Setup(mocked => mocked.SafeRegisterParticipant(555, 999, 456, 999456)).Returns(999555);
            eventService.Setup(mocked => mocked.SafeRegisterParticipant(444, 999, 456, 999456)).Returns(999444);

            eventService.Setup(mocked => mocked.SafeRegisterParticipant(777, 888, 456, 888456)).Returns(888777);
            eventService.Setup(mocked => mocked.SafeRegisterParticipant(555, 888, 456, 888456)).Returns(888555);
            eventService.Setup(mocked => mocked.SafeRegisterParticipant(444, 888, 456, 888456)).Returns(888444);

            fixture.addParticipantsToGroup(456, mockParticipantSignup);

            groupRepository.VerifyAll();
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

            groupRepository.Setup(mocked => mocked.getGroupDetails(456)).Returns(g);

            groupRepository.Setup(mocked => mocked.getAllEventsForGroup(456, null, false)).Returns(eventList);

            var relations = new List<MpGroupSignupRelationships>
            {
                new MpGroupSignupRelationships {RelationshipId = 111}
            };
            groupRepository.Setup(mocked => mocked.GetGroupSignupRelations(90210)).Returns(relations);

            var contactRelations = new List<MpContactRelationship>
            {
                new MpContactRelationship
                {
                    Contact_Id = 333,
                    Relationship_Id = 111,
                    Participant_Id = 222
                }
            };
            contactRelationshipService.Setup(mocked => mocked.GetMyCurrentRelationships(It.IsAny<int>(), It.IsAny<string>())).Returns(contactRelations);

            var participant = new Participant
            {
                ParticipantId = 555,
            };
            groupRepository.Setup(mocked => mocked.checkIfUserInGroup(It.IsAny<int>(), It.IsAny<List<MpGroupParticipant>>())).Returns(false);
            
            var attributes = new ObjectAllAttributesDTO();
            _objectAttributeService.Setup(mocked => mocked.GetObjectAttributes( It.IsAny<int>(), It.IsAny<MpObjectAttributeConfiguration>())).Returns(attributes);

            var response = fixture.getGroupDetails(456, 777, participant);

            groupRepository.VerifyAll();
            contactRelationshipService.VerifyAll();

            Assert.IsNotNull(response);
            Assert.IsTrue(response.GroupFullInd);
            Assert.AreEqual(g.GroupId, response.GroupId);
            Assert.AreEqual(2, response.SignUpFamilyMembers.Count);
            Assert.AreEqual(g.WaitListGroupId, response.WaitListGroupId);
            Assert.AreEqual(g.WaitList, response.WaitListInd);
        }

        [Test]
        public void GetGroupDetailsByInvitationGuidTest()
        {
            var mpInvitationDto = new MpInvitation
            {
                SourceId = 123123,
                EmailAddress = "test@userdomain.com",
                GroupRoleId = 66,
                InvitationType = 1,
                RecipientName = "Test User",
                RequestDate = new DateTime(2004, 1, 13)
            };

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

            var objectAllAttribute = new ObjectAllAttributesDTO();
            objectAllAttribute.MultiSelect = new Dictionary<int, ObjectAttributeTypeDTO>();
            objectAllAttribute.SingleSelect = new Dictionary<int, ObjectSingleAttributeDTO>();

            _invitationRepository.Setup(mocked => mocked.GetOpenInvitation(It.IsAny<string>())).Returns(mpInvitationDto);
            groupRepository.Setup(mocked => mocked.GetSmallGroupDetailsById(123123)).Returns(g);
            _attributeRepository.Setup(mocked => mocked.GetAttributes(It.IsAny<int>())).Returns(new List<MpAttribute>());
            _objectAttributeService.Setup(
                mocked => mocked.GetObjectAttributes(It.IsAny<int>(), It.IsAny<MpObjectAttributeConfiguration>(), It.IsAny<List<MpAttribute>>()))
                .Returns(objectAllAttribute);

            var response = fixture.GetGroupDetailsByInvitationGuid("crazy long guid");

            groupRepository.VerifyAll();
            contactRelationshipService.VerifyAll();

            Assert.IsNotNull(response);
            Assert.AreEqual(g.GroupId, response.GroupId);
        }

        [Test]
        public void GetGroupsForParticipant()
        {
            const string token = "1234frd32";
            const int participantId = 54;

            var attributes = new ObjectAllAttributesDTO();

            groupRepository.Setup(mocked => mocked.GetGroupsForParticipant(It.IsAny<string>(), participantId)).Returns(MockGroup());
            _objectAttributeService.Setup(mocked => mocked.GetObjectAttributes( It.IsAny<int>(), It.IsAny<MpObjectAttributeConfiguration>(), It.IsAny<List<MpAttribute>>()))
                .Returns(attributes);

            var grps = fixture.GetGroupsForParticipant(participantId);

            groupRepository.VerifyAll();
            Assert.IsNotNull(grps);
        }

        [Test]
        public void GetGroupsByTypeOrIdNoParticipant()
        {
            const string token = "123";
            const int participantId = 42;
            int[] groupTypeIds = {1, 8};

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
                    GroupType = 1,
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

            var participants = new List<MpGroupParticipant>()
            {
                new MpGroupParticipant()
                {
                    GroupParticipantId = 3,
                    GroupName = "Test Journey Group 2016",
                    GroupId = 321,
                    GroupRoleId = 22
                }
            };
            var objectAllAttribute = new ObjectAllAttributesDTO();
            objectAllAttribute.MultiSelect = new Dictionary<int, ObjectAttributeTypeDTO>();
            objectAllAttribute.SingleSelect = new Dictionary<int, ObjectSingleAttributeDTO>();

            var allAttributes = new List<MpAttribute>()
            {
                new MpAttribute
                {
                    AttributeId = 1,
                    AttributeTypeId = 10,

                }
            };

            _attributeRepository.Setup(mocked => mocked.GetAttributes((int?) null)).Returns(allAttributes);


            _objectAttributeService.Setup(mocked => mocked.GetObjectAttributes( groups[0].GroupId, It.IsAny<MpObjectAttributeConfiguration>(), It.IsAny<List<MpAttribute>>()))
                .Returns(objectAllAttribute);

            participantService.Setup(mocked => mocked.GetParticipantRecord(token)).Returns(new Participant {ParticipantId = participantId});

            groupRepository.Setup(mocked => mocked.GetGroupsForParticipantByTypeOrID(42, (string)null, groupTypeIds, (int?)null)).Returns(groups);

            groupRepository.Setup(mocked => mocked.GetGroupParticipants(groups[0].GroupId, true)).Returns(participants);

            var result = fixture.GetGroupsByTypeOrId(token, null, groupTypeIds);

            participantService.VerifyAll();
            groupRepository.Verify();

            Assert.AreEqual(result.Count, 1);
            Assert.AreEqual(result[0].Participants.Count, 1);
            Assert.AreEqual(result[0].GroupId, 321);
        }

        [Test]
        public void GetGroupsByTypeOrIdWithParticipant()
        {
            const string token = "123";
            const int participantId = 42;
            int[] groupTypeIds = { 1, 8 };

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
                    GroupType = 1,
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

            var participants = new List<MpGroupParticipant>()
            {
                new MpGroupParticipant()
                {
                    GroupParticipantId = 3,
                    GroupName = "Test Journey Group 2016",
                    GroupId = 321,
                    GroupRoleId = 22
                }
            };
            var objectAllAttribute = new ObjectAllAttributesDTO();
            objectAllAttribute.MultiSelect = new Dictionary<int, ObjectAttributeTypeDTO>();
            objectAllAttribute.SingleSelect = new Dictionary<int, ObjectSingleAttributeDTO>();

            var allAttributes = new List<MpAttribute>()
            {
                new MpAttribute
                {
                    AttributeId = 1,
                    AttributeTypeId = 10,

                }
            };

            _attributeRepository.Setup(mocked => mocked.GetAttributes((int?)null)).Returns(allAttributes);

            _objectAttributeService.Setup(mocked => mocked.GetObjectAttributes(It.IsAny<int>(), It.IsAny<MpObjectAttributeConfiguration>(), It.IsAny<List<MpAttribute>>()))
                .Returns(objectAllAttribute);

            groupRepository.Setup(mocked => mocked.GetGroupsForParticipantByTypeOrID(42, (string)null, groupTypeIds, (int?)null)).Returns(groups);

            groupRepository.Setup(mocked => mocked.GetGroupParticipants(groups[0].GroupId, true)).Returns(participants);

            var result = fixture.GetGroupsByTypeOrId(token, participantId, groupTypeIds);

            participantService.VerifyAll();
            groupRepository.Verify();

            Assert.AreEqual(result.Count, 1);
            Assert.AreEqual(result[0].Participants.Count, 1);
            Assert.AreEqual(result[0].GroupId, 321);
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

            groupRepository.Setup(mocked => mocked.GetGroupsByTypeForParticipant(participantId, groupTypeId)).Returns(groups);
            _objectAttributeService.Setup(mocked => mocked.GetObjectAttributes(It.IsAny<int>(), It.IsAny<MpObjectAttributeConfiguration>(), It.IsAny<List<MpAttribute>>()))
                .Returns(attributes);

            var grps = fixture.GetGroupsByTypeForParticipant(participantId, groupTypeId);

            groupRepository.VerifyAll();
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

            groupRepository.Setup(mocked => mocked.CreateGroup(newGroup)).Returns(14);
            _awsCloudsearchService.Setup(mocked => mocked.UploadSingleGroupToAwsFromMp(It.IsAny<int>()));
            var groupResp = fixture.CreateGroup(group);

            _groupRepository.VerifyAll();
            Assert.IsNotNull(groupResp);
        }

        [Test]
        public void WhenLookupParticipantIsCalledWithAllParticipantIdSpecified_ShouldNotLookupParticipant()
        {
            fixture.LookupParticipantIfEmpty(123, mockParticipantSignup);
        }

        [Test]
        public void WhenLookupParticipantIsCalledWithoutParticipantIdSpecified_ShouldLookupParticipantAndSetParticipantId()
        {
            var leaderpartid = 123;
            var participant = new Participant() {ParticipantId = 100};

            var participants = new List<ParticipantSignup>
            {
                new ParticipantSignup()
                {
                    childCareNeeded = false,
                    SendConfirmationEmail = true
                },
            };
            fixture.LookupParticipantIfEmpty(leaderpartid, participants);
            Assert.AreEqual(123, participants[0].particpantId);
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
            _communicationService.Verify(x => x.SendMessage(It.IsAny<MinistryPlatform.Translation.Models.MpCommunication>(), false), Times.Never);
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
            _communicationService.Verify(x => x.SendMessage(It.IsAny<MinistryPlatform.Translation.Models.MpCommunication>(), false), Times.Never);
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
            groupRepository.Setup(x => x.GetGroupsByTypeForParticipant(participant.ParticipantId, JOURNEY_GROUP_ID)).Returns(groups);
            _communicationService.Setup(mocked => mocked.GetTemplate(It.IsAny<int>())).Returns(template);
            _contactService.Setup(mocked => mocked.GetContactById(It.IsAny<int>())).Returns(contact);
            _objectAttributeService.Setup(mocked => mocked.GetObjectAttributes(It.IsAny<int>(), It.IsAny<MpObjectAttributeConfiguration>(), It.IsAny<List<MpAttribute>>()))
                .Returns(attributes);
            _communicationService.Setup(m => m.SendMessage(It.IsAny<MinistryPlatform.Translation.Models.MpCommunication>(), false)).Verifiable();

            var membership = groups.Where(group => group.GroupId == groupId).ToList();
            fixture.SendJourneyEmailInvite(communication, token);
            Assert.AreEqual(membership.Count, 1);
            _communicationService.Verify(m => m.SendMessage(It.IsAny<MinistryPlatform.Translation.Models.MpCommunication>(), false), Times.Once);
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
            groupRepository.Setup(mocked => mocked.getGroupDetails(456)).Returns(g);

            try
            {
                fixture.addParticipantToGroupNoEvents(456, mockParticipantSignup.FirstOrDefault());
                Assert.Fail("Expected exception was not thrown");
            }
            catch (Exception e)
            {
                Assert.IsInstanceOf(typeof(GroupFullException), e);
            }

            groupRepository.VerifyAll();
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
            groupRepository.Setup(mocked => mocked.getGroupDetails(456)).Returns(g);

            groupRepository.Setup(mocked => mocked.AddParticipantToGroup(999, 456, GROUP_ROLE_DEFAULT_ID, false, false, It.IsAny<DateTime>(), null, null)).Returns(999456);

            fixture.addParticipantToGroupNoEvents(456, mockParticipantSignup.FirstOrDefault());

            groupRepository.VerifyAll();
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

        [Test]
        public void shouldUpdateGroup()
        {
            var start = DateTime.Now;
            var end = DateTime.Now.AddYears(2);

            var group = new GroupDTO()
            {
                GroupName = "New Testing Group",
                GroupDescription = "The best group ever created for testing stuff and things",
                GroupId = 145,
                GroupTypeId = 19,
                MinistryId = 8,
                CongregationId = 1,
                StartDate = start,
                EndDate = end,
                GroupFullInd = false,
                AvailableOnline = true,
                RemainingCapacity = 8,
                WaitListInd = false,
                ChildCareAvailable = false,
                MeetingDayId = 2,
                MeetingTime = "18000",
                GroupRoleId = 16
            };

            groupRepository.Setup(mocked => mocked.GetGroupParticipants(group.GroupId, true)).Returns(new List<MpGroupParticipant>()
            {
                new MpGroupParticipant()
                {
                    Email = "DukeNukem@compuserv.net",
                    ContactId = 12,
                    GroupParticipantId = 13,
                    GroupRoleId = 22,
                    GroupRoleTitle = "Member",
                    LastName = "Nukem",
                    NickName = "Duke",
                    ParticipantId = 11,
                    StartDate = start
                }
            });
            groupRepository.Setup(mocked => mocked.UpdateGroup(It.IsAny<MpGroup>())).Returns(14);
            this._objectAttributeService.Setup(mocked => mocked.GetObjectAttributes(It.IsAny<int>(), It.IsAny<MpObjectAttributeConfiguration>()))
                .Returns(new ObjectAllAttributesDTO());

            var groupResp = fixture.UpdateGroup(group);


            _groupRepository.VerifyAll();
            Assert.IsNotNull(groupResp);
        }

        [Test]
        public void shouldUpdateGroupThatHasMinors()
        {
            var start = DateTime.Now;
            var end = DateTime.Now.AddYears(2);

            var group = new GroupDTO()
            {
                GroupName = "New Testing Group",
                GroupDescription = "The best group ever created for testing stuff and things",
                GroupId = 145,
                GroupTypeId = 19,
                MinistryId = 8,
                CongregationId = 1,
                StartDate = start,
                EndDate = end,
                GroupFullInd = false,
                AvailableOnline = true,
                RemainingCapacity = 8,
                WaitListInd = false,
                ChildCareAvailable = false,
                MeetingDayId = 2,
                MeetingTime = "18000",
                GroupRoleId = 16,
                MinorAgeGroupsAdded = true
            };

            List<MpGroupParticipant> participants = new List<MpGroupParticipant>()
            {
                new MpGroupParticipant()
                {
                    Email = "DukeNukem@compuserv.net",
                    ContactId = 12,
                    GroupParticipantId = 13,
                    GroupRoleId = 22,
                    GroupRoleTitle = "Member",
                    LastName = "Nukem",
                    NickName = "Duke",
                    ParticipantId = 11,
                    StartDate = start
                }
            };

            groupRepository.Setup(mocked => mocked.GetGroupParticipants(group.GroupId, true)).Returns(participants);

            var template = new MpMessageTemplate
            {
                Body = "body",
                FromContactId = 12,
                FromEmailAddress = "me@here.com",
                ReplyToContactId = 34,
                ReplyToEmailAddress = "you@there.com",
                Subject = "subject"
            };

            const string toEmail = "studentministry@crossroads.net";

            _communicationService.Setup(mocked => mocked.GetTemplate(It.IsAny<int>())).Returns(template);

            _communicationService.Setup(
                mocked =>
                    mocked.SendMessage(
                        It.Is<MinistryPlatform.Translation.Models.MpCommunication>(
                            c =>
                                c.AuthorUserId == 5 && c.DomainId == DOMAIN_ID && c.EmailBody.Equals(template.Body) && c.EmailSubject.Equals(template.Subject) &&
                                c.FromContact.ContactId == template.FromContactId && c.FromContact.EmailAddress.Equals(template.FromEmailAddress) &&
                                c.ReplyToContact.ContactId == template.ReplyToContactId && c.ReplyToContact.EmailAddress.Equals(template.ReplyToEmailAddress) &&
                                c.ToContacts.Count == 1 && c.ToContacts[0].EmailAddress.Equals(toEmail) &&
                                c.MergeData["Leaders"].ToString()
                                .Equals($"<ul> <li>Name: {participants[0].NickName} {participants[0].LastName}  Email: {participants[0].Email} </li></ul>")),
                        false)).Returns(77);

            groupRepository.Setup(mocked => mocked.UpdateGroup(It.IsAny<MpGroup>())).Returns(14);
            this._objectAttributeService.Setup(mocked => mocked.GetObjectAttributes(It.IsAny<int>(), It.IsAny<MpObjectAttributeConfiguration>()))
                .Returns(new ObjectAllAttributesDTO());

            var groupResp = fixture.UpdateGroup(group);

            _groupRepository.VerifyAll();
            Assert.IsNotNull(groupResp);
        }

        [Test]
        public void shouldSendEmailIfCreatingMinorGroup()
        {
            var start = DateTime.Now;
            var end = DateTime.Now.AddYears(2);

            var group = new GroupDTO()
            {
                GroupName = "New Testing Group",
                GroupDescription = "The best group ever created for testing stuff and things",
                GroupId = 0,
                GroupTypeId = 19,
                MinistryId = 8,
                CongregationId = 1,
                StartDate = start,
                EndDate = end,
                GroupFullInd = false,
                AvailableOnline = true,
                RemainingCapacity = 8,
                WaitListInd = false,
                ChildCareAvailable = false,
                MeetingDayId = 2,
                MeetingTime = "18000",
                GroupRoleId = 16,
                MinorAgeGroupsAdded = true,
                Participants = new List<GroupParticipantDTO>()
                {
                    new GroupParticipantDTO()
                    {
                        ParticipantId = 1,
                        GroupRoleId = 22,
                        GroupParticipantId = 2,
                        Email = "groupymcgroupface@gmail.com",
                        StartDate = DateTime.Now,
                        ContactId = 1,
                        NickName = "GroupMan",
                        LastName = "Jones",
                        GroupRoleTitle = "Leader"
                    }
                }

            };

            var template = new MpMessageTemplate
            {
                Body = "body",
                FromContactId = 12,
                FromEmailAddress = "me@here.com",
                ReplyToContactId = 34,
                ReplyToEmailAddress = "you@there.com",
                Subject = "subject"
            };

            const string toEmail = "studentministry@crossroads.net";

            _communicationService.Setup(mocked => mocked.GetTemplate(It.IsAny<int>())).Returns(template);

            _communicationService.Setup(
                mocked =>
                    mocked.SendMessage(
                        It.Is<MinistryPlatform.Translation.Models.MpCommunication>(
                            c =>
                                c.AuthorUserId == 5 && c.DomainId == DOMAIN_ID && c.EmailBody.Equals(template.Body) && c.EmailSubject.Equals(template.Subject) &&
                                c.FromContact.ContactId == template.FromContactId && c.FromContact.EmailAddress.Equals(template.FromEmailAddress) &&
                                c.ReplyToContact.ContactId == template.ReplyToContactId && c.ReplyToContact.EmailAddress.Equals(template.ReplyToEmailAddress) &&
                                c.ToContacts.Count == 1 && c.ToContacts[0].EmailAddress.Equals(toEmail) &&
                                c.MergeData["Leaders"].ToString()
                                .Equals($"<ul> <li>Name: {group.Participants[0].NickName} {group.Participants[0].LastName}  Email: {group.Participants[0].Email} </li></ul>")),
                        false)).Returns(77);

            groupRepository.Setup(mocked => mocked.CreateGroup(It.IsAny<MpGroup>())).Returns(14);
            _awsCloudsearchService.Setup(mocked => mocked.UploadSingleGroupToAwsFromMp(It.IsAny<int>()));
            this._objectAttributeService.Setup(mocked => mocked.GetObjectAttributes(It.IsAny<int>(), It.IsAny<MpObjectAttributeConfiguration>()))
                .Returns(new ObjectAllAttributesDTO());

            var groupResp = fixture.CreateGroup(group);

            _groupRepository.VerifyAll();
            Assert.IsNotNull(groupResp);
        }

        [Test]
        public void ShouldCallRepositoryUpdateParticipant()
        {
            var participant = new GroupParticipantDTO()
            {
                GroupParticipantId = 1,
                GroupRoleId = 22,
                GroupRoleTitle = "Group Leader"
            };

            groupRepository.Setup(x => x.UpdateGroupParticipant(It.IsAny<List<MpGroupParticipant>>()));
            fixture.UpdateGroupParticipantRole(participant);
            groupRepository.Verify();

        }

        [Test]
        public void ShouldUpdateParticipant()
        {
            var participant1 = new MpGroupParticipant()
            {
                ParticipantId = 1,
                GroupParticipantId = 1,
                GroupRoleId = 22,
                GroupRoleTitle = "Group Leader"
            };
            var participant2 = new MpGroupParticipant()
            {
                ParticipantId = 2,
                GroupParticipantId = 2,
                GroupRoleId = 66,
                GroupRoleTitle = "Apprentice"
            };
            var participantList = new List<MpGroupParticipant> {participant1, participant2};

            var groupId = 1;
            var participantId = 2;
            var roleId = 3;
            
            groupRepository.Setup(x => x.GetGroupParticipants(groupId, true)).Returns(participantList);

            fixture.UpdateGroupParticipantRole(groupId,participantId,roleId);
            groupRepository.Verify(x => x.UpdateGroupParticipant(It.IsAny<List<MpGroupParticipant>>()), Times.Once);
        }

        [Test]
        public void ShouldSendEmailWhenLeaderAddedToGroupWithStudents()
        {
            var token = "123";
            var participant = new GroupParticipantDTO()
            {
                ParticipantId = 1,
                GroupParticipantId = 1,
                GroupRoleId = 22,
                GroupRoleTitle = "Group Leader"
            };

            var part = new List<MpGroupParticipant>();

            groupRepository.Setup(x => x.ParticipantGroupHasStudents(It.IsAny<string>(),
                                                                     participant.ParticipantId,
                                                                     participant.GroupParticipantId)).Returns(true);

            fixture.UpdateGroupParticipantRole(participant);
            groupRepository.VerifyAll(); // (x=>x.SendNewStudentMinistryGroupAlertEmail(part),Times.Once);

        }

        [Test]
        public void ShouldNotSendEmailWhenLeaderAddedToGroupWithOutStudents()
        {
            var token = "123";

            var participant = new GroupParticipantDTO()
            {
                ParticipantId = 1,
                GroupParticipantId = 1,
                GroupRoleId = 22,
                GroupRoleTitle = "Group Leader"
            };

            var group = new MpGroup()
            {
                Name = "group",
                GroupDescription = "Desc"
            };

            var part = new List<MpGroupParticipant>();

            groupRepository.Setup(x => x.ParticipantGroupHasStudents(token,
                                                                     participant.ParticipantId,
                                                                     participant.GroupParticipantId)).Returns(false);

            fixture.UpdateGroupParticipantRole(participant);
            groupRepository.Verify(x => x.SendNewStudentMinistryGroupAlertEmail(part, group), Times.Never);
        }

        [Test]
        public void ShouldSendEmailToParticipants()
        {
            var token = "123";
            var sender = new Participant()
            {
                ContactId = 123,
                EmailAddress = "testSender@test.com"
            };
            List<GroupParticipantDTO> participants = new List<GroupParticipantDTO>()
            {
                new GroupParticipantDTO()
                {
                    ParticipantId = 1,
                    GroupParticipantId = 1,
                    ContactId = 456,
                    Email = "1@1.com"

                },
                new GroupParticipantDTO()
                {
                    ParticipantId = 2,
                    GroupParticipantId = 2,
                    ContactId = 789,
                    Email = "2@2.com"
                },
                new GroupParticipantDTO()
                {
                    ParticipantId = 3,
                    GroupParticipantId = 3,
                    ContactId = 012,
                    Email = "3@3.com"
                },
                new GroupParticipantDTO()
                {
                    ParticipantId = 4,
                    GroupParticipantId = 4,
                    ContactId = 345,
                    Email = "4@4.com"
                }
            };

            string subject = "Test email subject from 'ShouldSendEmailToParticipants()' test";
            string body = "Test email body from 'ShouldSendEmailToParticipants()' test";
            int authorId = 5;
            var fromContact = new MpContact()
            {
                ContactId = 1519180,
                EmailAddress = "updates@crossroads.net"
            };
            var replyToContact = new MpContact()
            {
                ContactId = sender.ContactId,
                EmailAddress = sender.EmailAddress
            };

            var toContacts = participants.Select(p => new MpContact
            {
                ContactId = p.ContactId,
                EmailAddress = p.Email
            }).ToList();

            participantService.Setup(mocked => mocked.GetParticipantRecord(token)).Returns(sender);
            _communicationService.Setup(mocked => mocked.SendMessage(It.Is<MinistryPlatform.Translation.Models.MpCommunication>(
                c =>
                    c.AuthorUserId == authorId && c.DomainId == DOMAIN_ID && c.EmailBody.Equals(body) && c.EmailSubject.Equals(subject) &&
                    c.FromContact.ContactId == fromContact.ContactId && c.FromContact.EmailAddress.Equals(fromContact.EmailAddress) &&
                    c.ReplyToContact.ContactId == replyToContact.ContactId && c.ReplyToContact.EmailAddress.Equals(replyToContact.EmailAddress) &&
                    c.ToContacts.Count == 4 && c.ToContacts[0].EmailAddress.Equals(participants[0].Email))
                                                                     ,
                                                                     false)).Returns(33);

            fixture.SendParticipantsEmail(token, participants, subject, body);
            groupRepository.VerifyAll();
            _communicationService.VerifyAll();
        }

        [Test]
        public void GetGroupsForAuthenticatedUserReturnsCorrectParticipantsAndAttributesInGroups()
        {
            var testGroups = TestGroups();
            var participants = ParticipantsForTestGroups();
            var attributes = AttributesForTestGroups();
            var token = "123";
            var groupId = 1;

            groupRepository.Setup(x => x.GetMyGroupParticipationByType(token, It.IsAny<int[]>(), null)).Returns(testGroups);
            groupRepository.Setup(x => x.GetGroupParticipants(groupId, true)).Returns(participants);
            _attributeRepository.Setup(x => x.GetAttributes(It.IsAny<int>())).Returns(new List<MpAttribute>());
            _objectAttributeService.Setup(mocked => mocked.GetObjectAttributes(It.IsAny<int>(), It.IsAny<MpObjectAttributeConfiguration>(),It.IsAny<List<MpAttribute>>())).Returns(attributes);

            var groups = fixture.GetGroupsForAuthenticatedUser(token, It.IsAny<int[]>());
            groupRepository.VerifyAll();
            Assert.AreEqual(4, groups[0].Participants.Count, "Incorrect number of participants");


        }

        [Test]
        public void GetGroupByIdForAuthenticatedUserReturnsCorrectCorrectParticipantsandAttributesInGroup()
        {
            var testGroups = TestGroups();
            var participants = ParticipantsForTestGroups();
            var attributes = AttributesForTestGroups();
            var token = "123";
            var groupId = 1;

            groupRepository.Setup(x => x.GetMyGroupParticipationByType(token, null, It.IsAny<int>())).Returns(testGroups);
            groupRepository.Setup(x => x.GetGroupParticipants(groupId, true)).Returns(participants);
            _attributeRepository.Setup(x => x.GetAttributes(It.IsAny<int>())).Returns(new List<MpAttribute>());
            _objectAttributeService.Setup(mocked => mocked.GetObjectAttributes(It.IsAny<int>(), It.IsAny<MpObjectAttributeConfiguration>(), It.IsAny<List<MpAttribute>>())).Returns(attributes);

            var group = fixture.GetGroupByIdForAuthenticatedUser(token, groupId).FirstOrDefault();
            groupRepository.VerifyAll();
            Assert.AreEqual(4, group.Participants.Count, "Incorrect number of participants");

        }

        private List<MpGroup> TestGroups()
        {
            var groups = new List<MpGroup>()
            {
                new MpGroup
                {
                    GroupId = 1,
                    CongregationId = 8,
                    Name = "Test Group 1",
                    GroupRoleId = 16,
                    GroupDescription = "Test Group 1",
                    MinistryId = 8,
                    ContactId = 3,
                    GroupType = 1,
                    StartDate = Convert.ToDateTime("2016-02-12"),
                    MeetingDayId = 3,
                    MeetingTime = "10:00",
                    AvailableOnline = false,
                    Address = new MpAddress()
                    {
                        Address_Line_1 = "123 Main St",
                        Address_Line_2 = "",
                        City = "Somewhere",
                        State = "OH",
                        Postal_Code = "45454"
                    }
                },
                new MpGroup
                {
                    GroupId = 8,
                    CongregationId = 8,
                    Name = "Test Group 2",
                    GroupRoleId = 22,
                    GroupDescription = "Test Group 2",
                    MinistryId = 8,
                    ContactId = 2,
                    GroupType = 8,
                    StartDate = Convert.ToDateTime("2016-02-12"),
                    MeetingDayId = 3,
                    MeetingTime = "10:00",
                    AvailableOnline = false,
                    Address = new MpAddress()
                    {
                        Address_Line_1 = "124 Main St",
                        Address_Line_2 = "",
                        City = "Somewhere",
                        State = "OH",
                        Postal_Code = "45454"
                    }
                }
            };

            return groups;
        }

        private List<MpGroupParticipant> ParticipantsForTestGroups()
        {
            var participants = new List<MpGroupParticipant>()
            {
                new MpGroupParticipant()
                {
                    ParticipantId = 1,
                    GroupParticipantId = 1,
                    ContactId = 1
                },
                new MpGroupParticipant()
                {
                    ParticipantId = 2,
                    GroupParticipantId = 1,
                    ContactId = 2
                },
                new MpGroupParticipant()
                {
                    ParticipantId = 3,
                    GroupParticipantId = 1,
                    ContactId = 3
                },
                new MpGroupParticipant()
                {
                    ParticipantId = 4,
                    GroupParticipantId = 1,
                    ContactId = 4
                }
            };

            return participants;
        }

        private ObjectAllAttributesDTO AttributesForTestGroups()
        {
            var attributes = new ObjectAllAttributesDTO()
            {
                    MultiSelect = new Dictionary<int, ObjectAttributeTypeDTO>(),
                    SingleSelect = new Dictionary<int, ObjectSingleAttributeDTO>()

            };

            return attributes;
        }

    }
}

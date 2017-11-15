using System;
using System.Collections.Generic;
using System.Linq;
using Amazon.CloudSearchDomain.Model;
using AutoMapper;
using crds_angular.App_Start;
using crds_angular.Exceptions;
using crds_angular.Models.Crossroads;
using crds_angular.Models.Crossroads.Attribute;
using crds_angular.Models.Crossroads.Groups;
using crds_angular.Services;
using crds_angular.Services.Analytics;
using crds_angular.Services.Interfaces;
using Crossroads.Utilities.Interfaces;
using Crossroads.Utilities.Models;
using Crossroads.Web.Common.Configuration;
using MinistryPlatform.Translation.Models;
using MinistryPlatform.Translation.Models.DTO;
using Moq;
using NUnit.Framework;
using Rhino.Mocks;
using MPServices = MinistryPlatform.Translation.Repositories.Interfaces;

namespace crds_angular.test.Services
{
    public class GroupToolServiceTest
    {
        private GroupToolService _fixture;
        private Mock<IAwsCloudsearchService> _awsCloudsearchService;
        private Mock<MPServices.IGroupToolRepository> _groupToolRepository;
        private Mock<MPServices.ICommunicationRepository> _communicationRepository;
        private Mock<IGroupService> _groupService;
        private Mock<MPServices.IGroupRepository> _groupRepository;
        private Mock<MPServices.IParticipantRepository> _participantRepository;
        private Mock<IContentBlockService> _contentBlockService;
        private Mock<MPServices.IInvitationRepository> _invitationRepositor;
        private Mock<IAddressProximityService> _addressProximityService;
        private Mock<MPServices.IContactRepository> _contactRepository;
        private Mock<IAddressProximityService> _addressMatrixService;
        private Mock<IEmailCommunication> _emailCommunicationService;
        private Mock<IAttributeService> _attributeService;
        private Mock<IAddressService> _addressService;
        private Mock<MPServices.IFinderRepository> _finderRepository;
        private Mock<IAnalyticsService> _mockAnalyticService;


        private const int GroupRoleLeader = 22;
        private const int RemoveParticipantFromGroupEmailTemplateId = 654;
        private const int GroupEndedParticipantEmailTemplate = 88876;
        private const int DomainId = 321;
        private const string BaseUrl = "test.com";
        private const int DefaultEmailContactId = 876;
        private const int AddressMatrixSearchDepth = 2;
        private const int DefaultGroupContactId = 999;
        private const int RequestToJoinEmailTemplateId = 954;
        private const int RequestToJoinAnywhereEmailTemplateId = 956;
        private const int GroupRequestPendingReminderEmailTemplateId = 5150;
        private const int GatheringRequestPendingReminderEmailTemplateId = 6171;
        private const int GatheringHostDenyTemplate = 2010;
        private const int GatheringHostAcceptTemplate = 2009;
        private const int AttributeCategoryGroupCategory = 90;
        private readonly int smallGroupTypeId = 434;
        private readonly int onsiteGroupTypeId = 435;
        private readonly int anywhereGroupTypeId = 436;
        private const int _memberRoleId = 16;

        [SetUp]
        public void SetUp()
        {
            AutoMapperConfig.RegisterMappings();

            _awsCloudsearchService = new Mock<IAwsCloudsearchService>(MockBehavior.Strict);
            _communicationRepository = new Mock<MPServices.ICommunicationRepository>(MockBehavior.Strict);
            _groupToolRepository = new Mock<MPServices.IGroupToolRepository>(MockBehavior.Strict);
            _groupService = new Mock<IGroupService>(MockBehavior.Strict);
            _groupRepository = new Mock<MPServices.IGroupRepository>(MockBehavior.Strict);
            _participantRepository = new Mock<MPServices.IParticipantRepository>(MockBehavior.Strict);
            _contentBlockService = new Mock<IContentBlockService>(MockBehavior.Strict);
            _invitationRepositor = new Mock<MPServices.IInvitationRepository>(MockBehavior.Strict);
            _addressProximityService = new Mock<IAddressProximityService>(MockBehavior.Strict);
            _contactRepository = new Mock<MPServices.IContactRepository>();
            _addressMatrixService = new Mock<IAddressProximityService>(MockBehavior.Strict);
            _emailCommunicationService = new Mock<IEmailCommunication>(MockBehavior.Strict);
            _attributeService = new Mock<IAttributeService>(MockBehavior.Strict);
            _addressService = new Mock<IAddressService>(MockBehavior.Strict);
            _finderRepository = new Mock<MPServices.IFinderRepository>();
            _mockAnalyticService = new Mock<IAnalyticsService>();


            var configuration = new Mock<IConfigurationWrapper>();

            configuration.Setup(mocked => mocked.GetConfigIntValue("GroupRoleLeader")).Returns(GroupRoleLeader);
            configuration.Setup(mocked => mocked.GetConfigIntValue("GenericGroupForCMSMergeEmailTemplateId")).Returns(RemoveParticipantFromGroupEmailTemplateId);
            configuration.Setup(mocked => mocked.GetConfigIntValue("DomainId")).Returns(DomainId);
            configuration.Setup(mocked => mocked.GetConfigValue("BaseURL")).Returns(BaseUrl);
            configuration.Setup(mocked => mocked.GetConfigIntValue("DefaultContactEmailId")).Returns(DefaultEmailContactId);
            configuration.Setup(mocked => mocked.GetConfigIntValue("GroupEndedParticipantEmailTemplate")).Returns(GroupEndedParticipantEmailTemplate);
            configuration.Setup(mocked => mocked.GetConfigIntValue("AddressMatrixSearchDepth")).Returns(AddressMatrixSearchDepth);
            configuration.Setup(mocked => mocked.GetConfigIntValue("DefaultGroupContactEmailId")).Returns(DefaultGroupContactId);
            configuration.Setup(mocked => mocked.GetConfigIntValue("GroupRequestToJoinEmailTemplate")).Returns(RequestToJoinEmailTemplateId);
            configuration.Setup(mocked => mocked.GetConfigIntValue("AnywhereGroupRequestToJoinEmailTemplate")).Returns(RequestToJoinAnywhereEmailTemplateId);
            configuration.Setup(mocked => mocked.GetConfigIntValue("GroupRequestPendingReminderEmailTemplateId")).Returns(GroupRequestPendingReminderEmailTemplateId);
            configuration.Setup(mocked => mocked.GetConfigIntValue("GatheringRequestPendingReminderEmailTemplateId")).Returns(GatheringRequestPendingReminderEmailTemplateId);
            configuration.Setup(mocked => mocked.GetConfigIntValue("GroupCategoryAttributeTypeId")).Returns(AttributeCategoryGroupCategory);
            configuration.Setup(mocked => mocked.GetConfigIntValue("SmallGroupTypeId")).Returns(smallGroupTypeId);
            configuration.Setup(mocked => mocked.GetConfigIntValue("OnsiteGroupTypeId")).Returns(onsiteGroupTypeId);
            configuration.Setup(mocked => mocked.GetConfigIntValue("AnywhereGroupTypeId")).Returns(anywhereGroupTypeId);
            configuration.Setup(mocked => mocked.GetConfigIntValue("GatheringHostAcceptTemplate")).Returns(GatheringHostAcceptTemplate);
            configuration.Setup(mocked => mocked.GetConfigIntValue("GatheringHostDenyTemplate")).Returns(GatheringHostDenyTemplate);

            _fixture = new GroupToolService(_awsCloudsearchService.Object, 
                                            _groupToolRepository.Object,
                                            _groupRepository.Object,
                                            _groupService.Object,
                                            _participantRepository.Object,
                                            _communicationRepository.Object,
                                            _contentBlockService.Object,
                                            configuration.Object,
                                            _invitationRepositor.Object,
                                            _addressProximityService.Object,
                                            _contactRepository.Object,
                                            _addressMatrixService.Object,
                                            _emailCommunicationService.Object,
                                            _attributeService.Object,
                                            _addressService.Object,
                                            _mockAnalyticService.Object,
                                            _finderRepository.Object);
        }

        [ExpectedException(typeof(GroupNotFoundForParticipantException))]
        public void TestVerifyCurrentUserIsGroupLeaderGroupNotFound()
        {
            _groupRepository.Setup(mocked => mocked.GetAuthenticatedUserParticipationByGroupID("abc", 1)).Returns((MpGroupParticipant) null);
            _fixture.VerifyCurrentUserIsGroupLeader("abc", 2);
        }

        [Test]
        [ExpectedException(typeof(NotGroupLeaderException))]
        public void TestVerifyCurrentUserIsGroupLeaderNotGroupLeader()
        {
            var myGroupParticipant = new MpGroupParticipant()
            {
                ParticipantId = 123,
                GroupParticipantId = 321,
                GroupRoleId = 8
            };
            _groupRepository.Setup(mocked => mocked.GetAuthenticatedUserParticipationByGroupID("abc", 1)).Returns(myGroupParticipant);
            _fixture.VerifyCurrentUserIsGroupLeader("abc", 1);
        }

        [ExpectedException(typeof(GroupNotFoundForParticipantException))]
        public void TestGetMyGroupInfoGroupNotFound()
        {
            _groupService.Setup(mocked => mocked.GetGroupByIdForAuthenticatedUser("abc", 2)).Returns(new List<GroupDTO>());
            _fixture.GetMyGroupInfo("abc", 2);
        }

        [Test]
        [ExpectedException(typeof(NotGroupLeaderException))]
        public void TestGetMyGroupInfoNotGroupLeader()
        {
            const int myParticipantId = 952;
            var myParticipant = new MpParticipant
            {
                ParticipantId = myParticipantId
            };
            _participantRepository.Setup(mocked => mocked.GetParticipantRecord("abc")).Returns(myParticipant);

            var groups = new List<GroupDTO>
            {
                new GroupDTO
                {
                    Participants = new List<GroupParticipantDTO>
                    {
                        new GroupParticipantDTO
                        {
                            ParticipantId = myParticipantId,
                            GroupRoleId = GroupRoleLeader + 1
                        }
                    }
                }
            };
            _groupService.Setup(mocked => mocked.GetGroupByIdForAuthenticatedUser("abc", 2)).Returns(groups);
            _fixture.GetMyGroupInfo("abc", 2);
        }

        [Test]
        public void TestVerifyCurrentUserIsGroupLeader()
        {
            var myGroupParticipant = new MpGroupParticipant()
            {
                GroupParticipantId = 5432,
                ParticipantId = 4242,
                GroupRoleId = GroupRoleLeader
            };

            const int groupId = 2;

            _groupRepository.Setup(mocked => mocked.GetAuthenticatedUserParticipationByGroupID("abc", 2)).Returns(myGroupParticipant);

            var result = _fixture.VerifyCurrentUserIsGroupLeader("abc", groupId);
            _groupRepository.VerifyAll();

            Assert.IsNotNull(result);
            Assert.AreEqual(myGroupParticipant.ParticipantId, result.Me.ParticipantId);
            Assert.AreEqual(groupId, result.Group.GroupId);
        }

        [Test]
        public void TestGetMyGroupInfoIsGroupLeader()
        {
            const int myParticipantId = 952;
            var myParticipant = new MpParticipant
            {
                ParticipantId = myParticipantId
            };
            _participantRepository.Setup(mocked => mocked.GetParticipantRecord("abc")).Returns(myParticipant);

            var groups = new List<GroupDTO>
            {
                new GroupDTO
                {
                    Participants = new List<GroupParticipantDTO>
                    {
                        new GroupParticipantDTO
                        {
                            ParticipantId = myParticipantId,
                            GroupRoleId = GroupRoleLeader
                        }
                    }
                }
            };
            _groupService.Setup(mocked => mocked.GetGroupByIdForAuthenticatedUser("abc", 2)).Returns(groups);
            var result = _fixture.GetMyGroupInfo("abc", 2);
            _participantRepository.VerifyAll();
            _groupService.VerifyAll();

            Assert.IsNotNull(result);
            Assert.AreSame(myParticipant, result.Me);
            Assert.AreSame(groups[0], result.Group);
        }

        [Test]
        public void TestApproveDenyInquiryFromMyGroupApprove()
        {
            const int myParticipantId = 952;
            var myParticipant = new MpParticipant
            {
                ParticipantId = myParticipantId
            };

            _participantRepository.Setup(mocked => mocked.GetParticipantRecord("abc")).Returns(myParticipant);

            var groupParticipants = new List<GroupParticipantDTO>
            {
                new GroupParticipantDTO
                {
                    ParticipantId = myParticipantId,
                    GroupRoleId = GroupRoleLeader
                },
                new GroupParticipantDTO
                {
                    ParticipantId = 9090,
                    GroupParticipantId = 19090,
                    GroupRoleId = GroupRoleLeader
                }
            };

            var groups = new List<GroupDTO>
            {
                new GroupDTO
                {
                    Participants = groupParticipants,
                    Address = new AddressDTO() {City = "cityname", State = "CA" }
                }
            };

            var mygroup = new GroupDTO {GroupTypeId = 5,
                                        Address = new AddressDTO() { City = "cityname", State = "CA" },
                                        Participants = groupParticipants
            };
            
            var inquiry = new Inquiry
            {
                ContactId = 123,
                InquiryId = 456
            };
            const string message = "message";

            var approveParticipant = new MpParticipant
            {
                ParticipantId = 9090
            };
            _participantRepository.Setup(mocked => mocked.GetParticipant(123)).Returns(approveParticipant);

            _groupService.Setup(mocked => mocked.addContactToGroup(2, 123, _memberRoleId)).Returns(It.IsAny<int>());
            _groupRepository.Setup(mocked => mocked.UpdateGroupInquiry(2, 456, true)).Verifiable();

            var template = new MpMessageTemplate
            {
                Body = "body",
                FromContactId = 99,
                FromEmailAddress = "88",
                ReplyToContactId = 77,
                ReplyToEmailAddress = "66",
                Subject = "55"
            };

            _communicationRepository.Setup(mocked => mocked.GetTemplate(It.IsAny<int>())).Returns(template);

            _communicationRepository.Setup(
                mocked =>
                    mocked.SendMessage(
                        It.IsAny<MpCommunication>(),
                        false)).Returns(5);

            _communicationRepository.Setup(mocked => mocked.ParseTemplateBody(It.IsAny<string>(), It.IsAny<Dictionary<string, object>>())).Returns(string.Empty);

            _contentBlockService.SetupGet(mocked => mocked["groupToolApproveInquirySubjectTemplateText"]).Returns(new ContentBlock());
            _contentBlockService.SetupGet(mocked => mocked["groupToolApproveInquiryEmailTemplateText"]).Returns(new ContentBlock());
            _groupService.Setup(mocked => mocked.GetGroupDetails(It.IsAny<int>())).Returns(mygroup);

            var leader = new MpMyContact
            {
                Last_Name = "last",
                Nickname = "Nick"
            };
            _contactRepository.Setup(mocked => mocked.GetContactById(It.IsAny<int>())).Returns(leader);
           

            _fixture.ApproveDenyInquiryFromMyGroup("abc", 2, true, inquiry, message, _memberRoleId);

            _participantRepository.VerifyAll();
            _groupService.VerifyAll();
            _groupRepository.VerifyAll();
            _communicationRepository.VerifyAll();
            _mockAnalyticService.Verify(x => x.Track(It.IsAny<string>(), "AcceptedIntoGroup", It.IsAny<EventProperties>()), Times.Once);


        }

        [Test]
        public void TestApproveDenyInquiryFromMyGroupDeny()
        {
            const int myParticipantId = 952;
            var myParticipant = new MpParticipant
            {
                ParticipantId = myParticipantId
            };
            _participantRepository.Setup(mocked => mocked.GetParticipantRecord("abc")).Returns(myParticipant);

            var groups = new List<GroupDTO>
            {
                new GroupDTO
                {
                    Participants = new List<GroupParticipantDTO>
                    {
                        new GroupParticipantDTO
                        {
                            ParticipantId = myParticipantId,
                            GroupRoleId = GroupRoleLeader
                        },
                        new GroupParticipantDTO
                        {
                            ParticipantId = 9090,
                            GroupParticipantId = 19090,
                            GroupRoleId = GroupRoleLeader
                        }
                    },
                    Address = new AddressDTO() {City = "cityname", State = "CA" }
                }
            };
            var mygroup = new GroupDTO { GroupTypeId = 5, Address = new AddressDTO() { City = "cityname", State = "CA" } };

            var inquiry = new Inquiry
            {
                ContactId = 123,
                InquiryId = 456
            };
            const string message = "message";

            var approveParticipant = new MpParticipant
            {
                ParticipantId = 9090
            };
            _participantRepository.Setup(mocked => mocked.GetParticipant(123)).Returns(approveParticipant);

            _groupRepository.Setup(mocked => mocked.UpdateGroupInquiry(2, 456, false)).Verifiable();

            var template = new MpMessageTemplate
            {
                Body = "body",
                FromContactId = 99,
                FromEmailAddress = "88",
                ReplyToContactId = 77,
                ReplyToEmailAddress = "66",
                Subject = "55"
            };

            _communicationRepository.Setup(mocked => mocked.GetTemplate(It.IsAny<int>())).Returns(template);

            _communicationRepository.Setup(
                mocked =>
                    mocked.SendMessage(
                        It.IsAny<MpCommunication>(),
                        false)).Returns(5);

            _communicationRepository.Setup(mocked => mocked.ParseTemplateBody(It.IsAny<string>(), It.IsAny<Dictionary<string, object>>())).Returns(string.Empty);

            _contentBlockService.SetupGet(mocked => mocked["groupToolDenyInquirySubjectTemplateText"]).Returns(new ContentBlock());
            _contentBlockService.SetupGet(mocked => mocked["groupToolDenyInquiryEmailTemplateText"]).Returns(new ContentBlock());
            _groupService.Setup(mocked => mocked.GetGroupDetails(It.IsAny<int>())).Returns(mygroup);

            var leader = new MpMyContact
            {
                Last_Name = "last",
                Nickname = "Nick"
            };
            _contactRepository.Setup(mocked => mocked.GetContactById(It.IsAny<int>())).Returns(leader);

            _fixture.ApproveDenyInquiryFromMyGroup("abc", 2, false, inquiry, message, _memberRoleId);

            _participantRepository.VerifyAll();
            _groupService.VerifyAll();
            _groupRepository.VerifyAll();
            _communicationRepository.VerifyAll();

        }

        [Test]
        public void TestAcceptDenyGroupInvitationAccepting()
        {
            string token = "afdsak;fkjadfjkas;fpeiwjkja";
            int groupId = 23;
            string invitationGuid = "akdfjadfjajeoihqwpoi392053qiweur9";


            var participant = new MpParticipant
            {
                ParticipantId = 9090
            };

            List<MpGroupParticipant> groupParticipants = new List<MpGroupParticipant>();

            _participantRepository.Setup(mocked => mocked.GetParticipantRecord(It.IsAny<string>())).Returns(participant);
            _groupRepository.Setup(
                mocked => mocked.AddParticipantToGroup(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<int>(), It.IsAny<bool>(), false, It.IsAny<DateTime>(), null, null)).Returns(1);
            _groupRepository.Setup(mocked => mocked.GetGroupParticipants(It.IsAny<int>(), It.IsAny<bool>())).Returns(groupParticipants);
            _invitationRepositor.Setup(mocked => mocked.MarkInvitationAsUsed(It.IsAny<string>())).Verifiable();

            _fixture.AcceptDenyGroupInvitation(token, groupId, invitationGuid, true);
            _participantRepository.VerifyAll();
            _groupRepository.VerifyAll();
            _invitationRepositor.VerifyAll();
        }

        public void TestAcceptDenyGroupInvitationInGroup()
        {
            string token = "afdsak;fkjadfjkas;fpeiwjkja";
            int groupId = 23;
            string invitationGuid = "akdfjadfjajeoihqwpoi392053qiweur9";


            var participant = new MpParticipant
            {
                ParticipantId = 9090
            };

            var groupParticipant = new MpGroupParticipant
            {
                GroupParticipantId = 9090
            };

            List<MpGroupParticipant> groupParticipants = new List<MpGroupParticipant>();
            groupParticipants.Add(groupParticipant);

            _participantRepository.Setup(mocked => mocked.GetParticipantRecord(It.IsAny<string>())).Returns(participant);
            _groupRepository.Setup(
                mocked => mocked.AddParticipantToGroup(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<int>(), It.IsAny<bool>(), false, It.IsAny<DateTime>(), null, null)).Returns(1);
            _groupRepository.Setup(mocked => mocked.GetGroupParticipants(It.IsAny<int>(), It.IsAny<bool>())).Returns(groupParticipants);
            _invitationRepositor.Setup(mocked => mocked.MarkInvitationAsUsed(It.IsAny<string>())).Verifiable();

            var ex = new DuplicateGroupParticipantException("Cannot accept invite - already member of group");

            try
            {
                _fixture.AcceptDenyGroupInvitation(token, groupId, invitationGuid, true);
                Assert.Fail("expected exception was not thrown");
            }
            catch (DuplicateGroupParticipantException e)
            {
                Assert.AreSame(typeof(DuplicateGroupParticipantException), e.GetType());
                Assert.AreSame(ex, e.InnerException);
            }

            _participantRepository.VerifyAll();
            _groupRepository.VerifyAll();
            _invitationRepositor.VerifyAll();
        }

        [Test]
        public void TestAcceptDenyGroupInvitationDenying()
        {
            string token = "afdsak;fkjadfjkas;fpeiwjkja";
            int groupId = 23;
            string invitationGuid = "akdfjadfjajeoihqwpoi392053qiweur9";

            _invitationRepositor.Setup(mocked => mocked.MarkInvitationAsUsed(It.IsAny<string>())).Verifiable();

            _fixture.AcceptDenyGroupInvitation(token, groupId, invitationGuid, false);
            _invitationRepositor.VerifyAll();
        }

        [Test]
        public void TestGetGroupCategories()
        {
            var cats = new List<AttributeCategoryDTO>()
            {
                new AttributeCategoryDTO()
                {
                    CategoryId= 1,
                    AttributeCategory= "Journey",
                    Description= "The current Journey",
                    ExampleText= "Journey Group",
                    RequiresActiveAttribute= true
                },
                new AttributeCategoryDTO()
                {
                    CategoryId= 2,
                    AttributeCategory= "Interest",
                    Description= "desc",
                    ExampleText= "Ex. Boxing, XBox",
                    RequiresActiveAttribute= false
                },
                new AttributeCategoryDTO()
                {
                    CategoryId= 3,
                    AttributeCategory= "Neighborhoods",
                    Description= "desc",
                    ExampleText= "Ex. Boxing, XBox",
                    RequiresActiveAttribute= false
                },
                new AttributeCategoryDTO()
                {
                    CategoryId= 4,
                    AttributeCategory= "Spiritual growth",
                    Description= "desc",
                    ExampleText= "Ex. Boxing, XBox",
                    RequiresActiveAttribute= false
                },
                new AttributeCategoryDTO()
                {
                    CategoryId= 5,
                    AttributeCategory= "Life Stages",
                    Description= "desc",
                    ExampleText= "Ex. Boxing, XBox",
                    RequiresActiveAttribute= false
                },
                new AttributeCategoryDTO()
                {
                    CategoryId= 6,
                    AttributeCategory= "Healing",
                    Description= "desc",
                    ExampleText= "Ex. Boxing, XBox",
                    RequiresActiveAttribute= false
                }
            };

            var attribute = new AttributeDTO()
            {
                AttributeId = 1,
                Name = "I am _______",
            };

            _attributeService.Setup(mocked => mocked.GetAttributeCategory(It.Is<int>(id => id.Equals(AttributeCategoryGroupCategory)))).Returns(cats);
            _attributeService.Setup(mocked => mocked.GetOneAttributeByCategoryId(It.Is<int>(id => id.Equals(1)))).Returns(attribute);


            List<AttributeCategoryDTO> returnedCats = _fixture.GetGroupCategories();

            Assert.AreEqual(returnedCats.First(c => c.CategoryId == 1).Attribute.AttributeId, attribute.AttributeId);
            Assert.IsTrue(returnedCats.Count(c => c.Attribute == null) == 5);
            _attributeService.VerifyAll();
        }

        [Test]
        public void TestGetGroupCategoriesNoActiveAttribute()
        {
            var cats = new List<AttributeCategoryDTO>()
            {
                new AttributeCategoryDTO()
                {
                    CategoryId= 1,
                    AttributeCategory= "Journey",
                    Description= "The current Journey",
                    ExampleText= "Journey Group",
                    RequiresActiveAttribute= true
                },
                new AttributeCategoryDTO()
                {
                    CategoryId= 2,
                    AttributeCategory= "Interest",
                    Description= "desc",
                    ExampleText= "Ex. Boxing, XBox",
                    RequiresActiveAttribute= false
                },
                new AttributeCategoryDTO()
                {
                    CategoryId= 3,
                    AttributeCategory= "Neighborhoods",
                    Description= "desc",
                    ExampleText= "Ex. Boxing, XBox",
                    RequiresActiveAttribute= false
                },
                new AttributeCategoryDTO()
                {
                    CategoryId= 4,
                    AttributeCategory= "Spiritual growth",
                    Description= "desc",
                    ExampleText= "Ex. Boxing, XBox",
                    RequiresActiveAttribute= false
                },
                new AttributeCategoryDTO()
                {
                    CategoryId= 5,
                    AttributeCategory= "Life Stages",
                    Description= "desc",
                    ExampleText= "Ex. Boxing, XBox",
                    RequiresActiveAttribute= false
                },
                new AttributeCategoryDTO()
                {
                    CategoryId= 6,
                    AttributeCategory= "Healing",
                    Description= "desc",
                    ExampleText= "Ex. Boxing, XBox",
                    RequiresActiveAttribute= false
                }
            };

            AttributeDTO attribute = null;

            _attributeService.Setup(mocked => mocked.GetAttributeCategory(It.Is<int>(id => id.Equals(AttributeCategoryGroupCategory)))).Returns(cats);
            _attributeService.Setup(mocked => mocked.GetOneAttributeByCategoryId(It.Is<int>(id => id.Equals(1)))).Returns(attribute);


            List<AttributeCategoryDTO> returnedCats = _fixture.GetGroupCategories();
            
            Assert.IsNull(returnedCats.First(c => c.CategoryId == 2).Attribute);
            Assert.IsTrue(returnedCats.Count(c => c.Attribute == null) == 5);
            Assert.IsTrue(returnedCats.Count() == 5);
            _attributeService.VerifyAll();
        }


        [Test]
        [ExpectedException(typeof(GroupNotFoundForParticipantException))]
        public void TestRemoveParticipantFromMyGroupGroupNotFound()
        {
            _groupService.Setup(mocked => mocked.GetGroupByIdForAuthenticatedUser("abc", 2)).Returns(new List<GroupDTO>());
            _fixture.RemoveParticipantFromMyGroup("abc", 2, 3, "message");
        }

        [Test]
        [ExpectedException(typeof(NotGroupLeaderException))]
        public void TestRemoveParticipantNotGroupLeader()
        {
            const int myParticipantId = 952;
            var myParticipant = new MpParticipant
            {
                ParticipantId = myParticipantId
            };
            _participantRepository.Setup(mocked => mocked.GetParticipantRecord("abc")).Returns(myParticipant);

            var groups = new List<GroupDTO>
            {
                new GroupDTO
                {
                    Participants = new List<GroupParticipantDTO>
                    {
                        new GroupParticipantDTO
                        {
                            ParticipantId = myParticipantId,
                            GroupRoleId = GroupRoleLeader + 1
                        }
                    }
                }
            };
            _groupService.Setup(mocked => mocked.GetGroupByIdForAuthenticatedUser("abc", 2)).Returns(groups);
            _fixture.RemoveParticipantFromMyGroup("abc", 2, 3, "message");
        }

        [Test]
        public void TestRemoveParticipantEndDateFails()
        {
            const int groupId = 222;

            const int myParticipantId = 952;
            var myParticipant = new MpParticipant
            {
                ParticipantId = myParticipantId
            };
            _participantRepository.Setup(mocked => mocked.GetParticipantRecord("abc")).Returns(myParticipant);

            const int removeParticipantId = 3;
            var groups = new List<GroupDTO>
            {
                new GroupDTO
                {
                    Participants = new List<GroupParticipantDTO>
                    {
                        new GroupParticipantDTO
                        {
                            ParticipantId = myParticipantId,
                            GroupRoleId = GroupRoleLeader
                        },
                        new GroupParticipantDTO
                        {
                            ParticipantId = removeParticipantId
                        }
                    }
                }
            };
            _groupService.Setup(mocked => mocked.GetGroupByIdForAuthenticatedUser("abc", groupId)).Returns(groups);

            var ex = new Exception("can't end date participant");
            _groupService.Setup(mocked => mocked.endDateGroupParticipant(groupId, removeParticipantId)).Throws(ex);

            try
            {
                _fixture.RemoveParticipantFromMyGroup("abc", groupId, removeParticipantId, "message");
                Assert.Fail("expected exception was not thrown");
            }
            catch (GroupParticipantRemovalException e)
            {
                Assert.AreSame(typeof(GroupParticipantRemovalException), e.GetType());
                Assert.AreSame(ex, e.InnerException);
            }

            _communicationRepository.VerifyAll();
            _groupToolRepository.VerifyAll();
            _groupService.VerifyAll();
            _participantRepository.VerifyAll();
            _contentBlockService.VerifyAll();
        }

        [Test]
        public void TestRemoveParticipantOkEmailFails()
        {
            const int groupId = 222;

            const int myParticipantId = 952;
            var myParticipant = new MpParticipant
            {
                ParticipantId = myParticipantId
            };
            _participantRepository.Setup(mocked => mocked.GetParticipantRecord("abc")).Returns(myParticipant);

            const int removeParticipantId = 3;
            var groups = new List<GroupDTO>
            {
                new GroupDTO
                {
                    Participants = new List<GroupParticipantDTO>
                    {
                        new GroupParticipantDTO
                        {
                            ParticipantId = myParticipantId,
                            GroupRoleId = GroupRoleLeader
                        },
                        new GroupParticipantDTO
                        {
                            ParticipantId = removeParticipantId,
                            GroupParticipantId = removeParticipantId
                        }
                    }
                }
            };
            _groupService.Setup(mocked => mocked.GetGroupByIdForAuthenticatedUser("abc", groupId)).Returns(groups);
            _groupService.Setup(mocked => mocked.endDateGroupParticipant(groupId, removeParticipantId));

            var ex = new Exception("can't get template");
            _communicationRepository.Setup(mocked => mocked.GetTemplate(RemoveParticipantFromGroupEmailTemplateId)).Throws(ex);
            _fixture.RemoveParticipantFromMyGroup("abc", groupId, removeParticipantId, "message");
            _communicationRepository.VerifyAll();
            _groupToolRepository.VerifyAll();
            _groupService.VerifyAll();
            _participantRepository.VerifyAll();
            _contentBlockService.VerifyAll();
        }

        [Test]
        public void TestSendGroupParticipantEmailNoOptionalParameters()
        {
            const int groupId = 222;
            const int myParticipantId = 952;
            const int removeParticipantId = 3;
            const int removeGroupParticipantId = 13;
            const int templateId = 765;

            var group = new GroupDTO
            {
                GroupName = "group name",
                GroupDescription = "group description",
                Participants = new List<GroupParticipantDTO>
                {
                    new GroupParticipantDTO
                    {
                        ParticipantId = myParticipantId,
                        GroupRoleId = GroupRoleLeader
                    },
                    new GroupParticipantDTO
                    {
                        ParticipantId = removeParticipantId,
                        GroupParticipantId = removeGroupParticipantId,
                        NickName = "nickname",
                        ContactId = 90,
                        Email = "80"
                    }
                },
                Address = new AddressDTO() { City = "cityname", State = "CA" }
            };

            var participant = group.Participants.Find(p => p.GroupParticipantId == removeGroupParticipantId);
            MpParticipant toParticipant = new MpParticipant
            {
                ContactId = participant.ContactId,
                EmailAddress = participant.Email,
                PreferredName = participant.NickName,
                ParticipantId = participant.ParticipantId
            };

            var template = new MpMessageTemplate
            {
                Body = "body",
                FromContactId = 99,
                FromEmailAddress = "88",
                ReplyToContactId = 77,
                ReplyToEmailAddress = "66",
                Subject = "55"
            };
            _communicationRepository.Setup(mocked => mocked.GetTemplate(templateId)).Returns(template);

            _communicationRepository.Setup(mocked => mocked.ParseTemplateBody(string.Empty, It.IsAny<Dictionary<string, object>>())).Returns(string.Empty);

            _communicationRepository.Setup(
                mocked =>
                    mocked.SendMessage(
                        It.Is<MpCommunication>(
                            c =>
                                c.DomainId == DomainId && c.EmailBody.Equals(template.Body) && c.EmailSubject.Equals(template.Subject) &&
                                c.FromContact.ContactId == template.FromContactId && c.FromContact.EmailAddress.Equals(template.FromEmailAddress) &&
                                c.ReplyToContact.ContactId == template.ReplyToContactId && c.ReplyToContact.EmailAddress.Equals(template.ReplyToEmailAddress) &&
                                c.MergeData["NickName"].Equals("nickname") && c.MergeData["Email_Template_Text"].Equals(string.Empty) &&
                                c.MergeData["Email_Custom_Message"].Equals(string.Empty) && c.MergeData["Group_Name"].Equals(group.GroupName) &&
                                c.MergeData["Group_Description"].Equals(group.GroupDescription)),
                        false)).Returns(5);

            var leader = new MpMyContact
            {
                Last_Name = "last",
                Nickname = "Nick"
            };
            _contactRepository.Setup(mocked => mocked.GetContactById(It.IsAny<int>())).Returns(leader);

            _fixture.SendGroupParticipantEmail(groupId, group, templateId, toParticipant);
            _communicationRepository.VerifyAll();
            _contentBlockService.VerifyAll();
        }

        [Test]
        public void TestSendGroupParticipantEmailToGroupParticipantId()
        {
            const int groupId = 222;
            const int myParticipantId = 952;
            const int removeParticipantId = 3;
            const int removeGroupParticipantId = 13;
            const int templateId = 765;

            var group = new GroupDTO
            {
                GroupName = "group name",
                GroupDescription = "group description",
                Participants = new List<GroupParticipantDTO>
                {
                    new GroupParticipantDTO
                    {
                        ParticipantId = myParticipantId,
                        GroupRoleId = GroupRoleLeader
                    },
                    new GroupParticipantDTO
                    {
                        ParticipantId = removeParticipantId,
                        GroupParticipantId = removeGroupParticipantId,
                        NickName = "nickname",
                        ContactId = 90,
                        Email = "80"
                    }
                },
                Address = new AddressDTO() { City = "cityname", State = "CA" }
            };

            var participant = group.Participants.Find(p => p.GroupParticipantId == removeGroupParticipantId);
            MpParticipant toParticipant = new MpParticipant
            {
                ContactId = participant.ContactId,
                EmailAddress = participant.Email,
                PreferredName = participant.NickName,
                ParticipantId = participant.ParticipantId
            };

            var template = new MpMessageTemplate
            {
                Body = "body",
                FromContactId = 99,
                FromEmailAddress = "88",
                ReplyToContactId = 77,
                ReplyToEmailAddress = "66",
                Subject = "55"
            };
            _communicationRepository.Setup(mocked => mocked.GetTemplate(templateId)).Returns(template);

            const string contentBlockTitle = "title";
            var content = new ContentBlock
            {
                Content = "content"
            };
            _contentBlockService.Setup(mocked => mocked[contentBlockTitle]).Returns(content);

            var fromParticipant = new MpParticipant
            {
                ContactId = 456,
                EmailAddress = "email",
                DisplayName = "display name",
                PreferredName = "preferred name"
            };

            _communicationRepository.Setup(mocked => mocked.ParseTemplateBody(content.Content, It.IsAny<Dictionary<string, object>>())).Returns(content.Content);

            _communicationRepository.Setup(
                mocked =>
                    mocked.SendMessage(
                        It.Is<MpCommunication>(
                            c =>
                                c.DomainId == DomainId && c.EmailBody.Equals(template.Body) && c.EmailSubject.Equals(template.Subject) &&
                                c.FromContact.ContactId == template.FromContactId && c.FromContact.EmailAddress.Equals(template.FromEmailAddress) &&
                                c.ReplyToContact.ContactId == fromParticipant.ContactId && c.ReplyToContact.EmailAddress.Equals(fromParticipant.EmailAddress) &&
                                c.MergeData["NickName"].Equals("nickname") && c.MergeData["Email_Template_Text"].Equals(content.Content) &&
                                c.MergeData["Email_Custom_Message"].Equals("message") && c.MergeData["Group_Name"].Equals(group.GroupName) &&
                                c.MergeData["Group_Description"].Equals(group.GroupDescription) && c.MergeData["From_Display_Name"].Equals(fromParticipant.DisplayName) &&
                                c.MergeData["From_Preferred_Name"].Equals(fromParticipant.PreferredName)),
                        false)).Returns(5);

            var leader = new MpMyContact
            {
                Last_Name = "last",
                Nickname = "Nick"
            };
            _contactRepository.Setup(mocked => mocked.GetContactById(It.IsAny<int>())).Returns(leader);

            _fixture.SendGroupParticipantEmail(groupId, group, templateId, toParticipant, contentBlockTitle, contentBlockTitle, "message", fromParticipant);
            _communicationRepository.VerifyAll();
            _contentBlockService.VerifyAll();
        }

        [Test]
        public void TestSendGroupParticipantEmailToGroupParticipant()
        {
            const int groupId = 222;
            const int myParticipantId = 952;
            const int removeParticipantId = 3;
            const int removeGroupParticipantId = 13;
            const int templateId = 765;

            var group = new GroupDTO
            {
                GroupName = "group name",
                GroupDescription = "group description",
                Participants = new List<GroupParticipantDTO>
                {
                    new GroupParticipantDTO
                    {
                        ParticipantId = myParticipantId,
                        GroupRoleId = GroupRoleLeader
                    },
                    new GroupParticipantDTO
                    {
                        ParticipantId = removeParticipantId + 1,
                        GroupParticipantId = removeGroupParticipantId + 1,
                        NickName = "nickname",
                        ContactId = 91,
                        Email = "80"
                    }
                },
                Address = new AddressDTO() { City = "cityname", State = "CA" }
            };

            var toGroupParticipant = new MpParticipant
            {
                ParticipantId = removeParticipantId,
                PreferredName = "preferred",
                EmailAddress = "email"
            };

            var template = new MpMessageTemplate
            {
                Body = "body",
                FromContactId = 99,
                FromEmailAddress = "88",
                ReplyToContactId = 77,
                ReplyToEmailAddress = "66",
                Subject = "55"
            };
            _communicationRepository.Setup(mocked => mocked.GetTemplate(templateId)).Returns(template);

            const string subjectContentBlockTitle = "subject";
            var subjectContent = new ContentBlock
            {
                Content = "<p>subject content</p>"
            };
            _contentBlockService.Setup(mocked => mocked[subjectContentBlockTitle]).Returns(subjectContent);

            const string bodyContentBlockTitle = "body";
            var bodyContent = new ContentBlock
            {
                Content = "body content"
            };
            _contentBlockService.Setup(mocked => mocked[bodyContentBlockTitle]).Returns(bodyContent);

            _communicationRepository.Setup(mocked => mocked.ParseTemplateBody("subject content", It.IsAny<Dictionary<string, object>>())).Returns("subject content parsed");
            _communicationRepository.Setup(mocked => mocked.ParseTemplateBody(bodyContent.Content, It.IsAny<Dictionary<string, object>>())).Returns($"{bodyContent.Content} parsed");

            var fromParticipant = new MpParticipant
            {
                ContactId = 456,
                EmailAddress = "email",
                DisplayName = "display name",
                PreferredName = "preferred name"
            };

            _communicationRepository.Setup(
                mocked =>
                    mocked.SendMessage(
                        It.Is<MpCommunication>(
                            c =>
                                c.DomainId == DomainId && c.EmailBody.Equals(template.Body) && c.EmailSubject.Equals(template.Subject) &&
                                c.FromContact.ContactId == template.FromContactId && c.FromContact.EmailAddress.Equals(template.FromEmailAddress) &&
                                c.ReplyToContact.ContactId == fromParticipant.ContactId && c.ReplyToContact.EmailAddress.Equals(fromParticipant.EmailAddress) &&
                                c.MergeData["NickName"].Equals("preferred") && c.MergeData["Email_Template_Text"].Equals($"{bodyContent.Content} parsed") && c.MergeData["Subject_Template_Text"].Equals("subject content parsed") &&
                                c.MergeData["Email_Custom_Message"].Equals("message") && c.MergeData["Group_Name"].Equals(group.GroupName) &&
                                c.MergeData["Group_Description"].Equals(group.GroupDescription) && c.MergeData["From_Display_Name"].Equals(fromParticipant.DisplayName) &&
                                c.MergeData["From_Preferred_Name"].Equals(fromParticipant.PreferredName)),
                        false)).Returns(5);

            var leader = new MpMyContact
            {
                Last_Name = "last",
                Nickname = "Nick"
            };
            _contactRepository.Setup(mocked => mocked.GetContactById(It.IsAny<int>())).Returns(leader);

            _fixture.SendGroupParticipantEmail(groupId, group, templateId, toGroupParticipant, subjectContentBlockTitle, bodyContentBlockTitle, "message", fromParticipant);
            _communicationRepository.VerifyAll();
            _contentBlockService.VerifyAll();
        }

        [Test]
        public void CanGetInvitationsForGroups()
        {
            var sourceId = 1;
            var invitationTypeId = 1;
            var token = "dude";

            _groupRepository.Setup(m => m.GetAuthenticatedUserParticipationByGroupID("dude", sourceId))
                .Returns(new MpGroupParticipant() {GroupParticipantId = 32, ParticipantId = 4, GroupRoleId = GroupRoleLeader});
            _groupToolRepository.Setup(m => m.GetInvitations(It.IsAny<int>(), It.IsAny<int>())).Returns(getMpInvations());
            var invitations = _fixture.GetInvitations(sourceId, invitationTypeId, token);

            Assert.AreEqual(4, invitations.Count);
        }

        [Test]
        public void EndGroupCallsAws()
        {
            _awsCloudsearchService.Setup(aws => aws.DeleteGroupFromAws(It.IsAny<int>())).Returns(new UploadDocumentsResponse());
            _groupService.Setup(gs => gs.GetGroupParticipants(It.IsAny<int>(), It.IsAny<bool>())).Returns(new List<GroupParticipantDTO>());
            _groupService.Setup(gs => gs.EndDateGroup(It.IsAny<int>(), It.IsAny<int>()));
            _groupService.Setup(gs => gs.GetGroupDetails(It.IsAny<int>())).Returns(new GroupDTO());

            _fixture.EndGroup(123, 4);

            _awsCloudsearchService.Verify(aws => aws.DeleteGroupFromAws(It.IsAny<int>()), Times.Once);
        }

        private List<MpInvitation> getMpInvations()
        {
            var invitations = new List<MpInvitation>();
            invitations.Add(
                new MpInvitation
                {
                    EmailAddress = "dudley@doright.com",
                    GroupRoleId = 16, // 16 = Group member
                    InvitationType = 1, // 1 = Group invitation type
                    RecipientName = "Dudley Doright",
                    RequestDate = new DateTime(2016, 7, 6)
                }

                );

            invitations.Add(
                new MpInvitation
                {
                    EmailAddress = "jker@gmail.com",
                    GroupRoleId = 16, // 16 = Group member
                    InvitationType = 1, // 1 = Group invitation type
                    RecipientName = "Joe",
                    RequestDate = new DateTime(2016, 7, 5)
                }

                );

            invitations.Add(
                new MpInvitation
                {
                    EmailAddress = "doubleDown@joker.com",
                    GroupRoleId = 16, // 16 = Group member
                    InvitationType = 1, // 1 = Group invitation type
                    RecipientName = "Joe from Chicago",
                    RequestDate = new DateTime(2016, 7, 4)
                }

                );

            invitations.Add(
                new MpInvitation
                {
                    EmailAddress = "ratso@rizzo.com",
                    GroupRoleId = 22, // 16 = Group member
                    InvitationType = 1, // 1 = Group invitation type
                    RecipientName = "Ratso Rizzo",
                    RequestDate = new DateTime(2016, 7, 3)
                }

                );


            return invitations;
        }

        [Test]
        public void CanGetInquiriesForGroups()
        {
            var mpResults = new List<MpInquiry>();

            mpResults.Add(new MpInquiry
            {
                InquiryId = 178,
                GroupId = 199846,
                EmailAddress = "test@jk.com",
                PhoneNumber = "444-111-2111",
                FirstName = "Joe",
                LastName = "Smith",
                RequestDate = new DateTime(2004, 3, 12),
                Placed = true,
            });

            var dto = new List<Inquiry>();

            dto.Add(new Inquiry
            {
                InquiryId = 178,
                GroupId = 199846,
                EmailAddress = "test@jk.com",
                PhoneNumber = "444-111-2111",
                FirstName = "Joe",
                LastName = "Smith",
                RequestDate = new DateTime(2004, 3, 12),
                Placed = true,
            });


            var groupId = 1;
            var token = "dude";

            _groupRepository.Setup(mocked => mocked.GetAuthenticatedUserParticipationByGroupID("dude", groupId))
                .Returns(new MpGroupParticipant() {GroupParticipantId = 37362, GroupRoleId = GroupRoleLeader, ParticipantId = 23});
            _groupToolRepository.Setup(m => m.GetInquiries(It.IsAny<int>())).Returns(mpResults);

            var inquiries = _fixture.GetInquiries(groupId, token);

            Assert.AreEqual(1, inquiries.Count);
            Assert.AreEqual(dto[0].InquiryId, inquiries[0].InquiryId);
            Assert.AreEqual(dto[0].GroupId, inquiries[0].GroupId);
            Assert.AreEqual(dto[0].EmailAddress, inquiries[0].EmailAddress);
            Assert.AreEqual(dto[0].PhoneNumber, inquiries[0].PhoneNumber);
            Assert.AreEqual(dto[0].FirstName, inquiries[0].FirstName);
            Assert.AreEqual(dto[0].LastName, inquiries[0].LastName);
            Assert.AreEqual(dto[0].RequestDate, inquiries[0].RequestDate);
            Assert.AreEqual(dto[0].Placed, inquiries[0].Placed);
        }

        [Test]
        public void TestSendAllGroupParticipantsEmail()
        {
            string token = "123ABC";

            var groupParticipantDTO = new MpParticipant
            {
                ContactId = 123,
                EmailAddress = "test@test.com",
                ParticipantId = 456
            };

            var groups = new List<GroupDTO>();

            var group1 = new GroupDTO();
            group1.Participants = new List<GroupParticipantDTO>();

            GroupParticipantDTO groupParticipant = new GroupParticipantDTO
            {
                ContactId = 123,
                GroupRoleId = 22,
                ParticipantId = 456
            };

            group1.Participants.Add(groupParticipant);
            groups.Add(group1);

            _communicationRepository.Setup(m => m.SendMessage(It.IsAny<MpCommunication>(), false)).Returns(1);
            _participantRepository.Setup(m => m.GetParticipantRecord(token)).Returns(groupParticipantDTO);
            _groupService.Setup(m => m.GetGroupByIdForAuthenticatedUser(token, 1)).Returns(groups);

            _fixture.SendAllGroupParticipantsEmail(token, 1, 123, "aaa", "bbb");
            _communicationRepository.VerifyAll();
        }

        [Test]
        public void TestSendAllGroupParticipantsEmailOnlyDistinct()
        {
            string token = "123ABC";

            var groupParticipantDTO = new MpParticipant
            {
                ContactId = 123,
                EmailAddress = "leader@test.com",
                ParticipantId = 456
            };

            var groups = new List<GroupDTO>();

            var group1 = new GroupDTO();
            group1.Participants = new List<GroupParticipantDTO>();

            List<GroupParticipantDTO> groupParticipants = new List<GroupParticipantDTO>()
            {


                new GroupParticipantDTO()
                {
                    ContactId = 123,
                    Email = "leader@test.com",
                    ParticipantId = 456,
                    GroupRoleId = 22,
                },
                new GroupParticipantDTO()
                {
                    ContactId = 123,
                    Email = "leader@test.com",
                    ParticipantId = 456,
                    GroupRoleId = 765,
                },
                new GroupParticipantDTO()
                {
                    ContactId = 012,
                    Email = "different@email.com",
                    ParticipantId = 345,
                    GroupRoleId = 765,
                },
                new GroupParticipantDTO()
                {
                    ContactId = 555,
                    Email = "member@test.com",
                    ParticipantId = 777,
                    GroupRoleId = 765,
                }
            };

            // There are 3 Distinct email addresses 
            int expectedToContactsCount = 3;

            group1.Participants = groupParticipants;
            groups.Add(group1);

            _communicationRepository.Setup(m => m.SendMessage(It.Is<MpCommunication>(email => email.ToContacts.Count() == expectedToContactsCount), false)).Returns(1);
            _participantRepository.Setup(m => m.GetParticipantRecord(token)).Returns(groupParticipantDTO);
            _groupService.Setup(m => m.GetGroupByIdForAuthenticatedUser(token, 1)).Returns(groups);

            _fixture.SendAllGroupParticipantsEmail(token, 1, 123, "aaa", "bbb");
            _communicationRepository.VerifyAll();
        }

        [Test]
        public void TestSendAllGroupLeadersEmail()
        {
            string token = "123ABC";

            var groupParticipantDTO = new MpParticipant
            {
                ContactId = 123,
                EmailAddress = "test@test.com",
                ParticipantId = 456
            };

            var message = new GroupMessageDTO
            {
                Body = "hi my name is",
                Subject = "I need help"
            };

            var requestorContact = new MpMyContact
            {
                Contact_ID = 123,
                Email_Address = "test@test.com",
                Last_Name = "Smith",
                Nickname = "Test"
            };

            var group = new GroupDTO();
            group.GroupId = 1231;
            group.GroupName = "Test Group";
            group.Participants = new List<GroupParticipantDTO>
            {
                new GroupParticipantDTO
                {
                    ContactId = 123,
                    GroupRoleId = 22,
                    ParticipantId = 456
                }
            };

            _contactRepository.Setup(m => m.GetContactById(123)).Returns(requestorContact);
            _communicationRepository.Setup(m => m.SendMessage(It.IsAny<MpCommunication>(), false)).Returns(1);
            _participantRepository.Setup(m => m.GetParticipantRecord(It.IsAny<string>())).Returns(groupParticipantDTO);
            _groupService.Setup(m => m.GetGroupDetails(It.IsAny<int>())).Returns(group);

            _fixture.SendAllGroupLeadersEmail(token, 1, message);
            _mockAnalyticService.Verify(x => x.Track(It.IsAny<string>(), "GroupLeaderContacted", It.IsAny<EventProperties>()), Times.Once);
            _communicationRepository.VerifyAll();
        }

        [Test]
        public void TestSearchGroupsNoKeywordsNothingFound()
        {
            int[] groupTypeId = new int[] {1};
          
            var keywords = new[] {"kw1", "kw2"};
            _groupToolRepository.Setup(mocked => mocked.SearchGroups(groupTypeId, null, null)).Returns(new List<MpGroupSearchResultDto>());
            var results = _fixture.SearchGroups(groupTypeId);
            _groupToolRepository.VerifyAll();
            Assert.IsNull(results);
        }

        [Test]
        public void TestSearchGroups()
        {
            int[] groupTypeId = new int[] {1};
            const string keywordString = "kw1 kw2 it's wine&dine";
            var keywords = keywordString
                    .Replace("'", "''") // Replace single quote with two, since the MP Rest API doesn't do it
                    .Replace("&", "%26") // Replace & with the hex representation, to avoid looking like a stored proc parameter
                    .Split((char[])null, StringSplitOptions.RemoveEmptyEntries);

            var searchResults = new List<MpGroupSearchResultDto>
            {
                new MpGroupSearchResultDto
                {
                    GroupId = 123,
                    Name = "group 1",
                    GroupType = 1231
                },
                new MpGroupSearchResultDto
                {
                    GroupId = 456,
                    Name = "group 2",
                    GroupType = 4564
                }
            };
            _groupToolRepository.Setup(mocked => mocked.SearchGroups(groupTypeId, keywords, null)).Returns(searchResults);
            var results = _fixture.SearchGroups(groupTypeId, keywordString);
            _groupToolRepository.VerifyAll();
            Assert.IsNotNull(results);
            Assert.AreEqual(searchResults.Count, results.Count);
            foreach (var expected in searchResults)
            {
                Assert.IsNotNull(results.Find(g => g.GroupId == expected.GroupId && g.GroupName.Equals(expected.Name) && g.GroupTypeId == expected.GroupType));
            }
        }

        [Test]
        public void TestSearchGroupsMultipleTypes()
        {
            int[] groupTypeId = new int[] { 1, 2 };
            const string keywordString = "kw1 kw2 it's wine&dine";
            var keywords = keywordString
                    .Replace("'", "''") // Replace single quote with two, since the MP Rest API doesn't do it
                    .Replace("&", "%26") // Replace & with the hex representation, to avoid looking like a stored proc parameter
                    .Split((char[])null, StringSplitOptions.RemoveEmptyEntries);

            var searchResults = new List<MpGroupSearchResultDto>
            {
                new MpGroupSearchResultDto
                {
                    GroupId = 123,
                    Name = "group 1",
                    GroupType = 1231
                },
                new MpGroupSearchResultDto
                {
                    GroupId = 456,
                    Name = "group 2",
                    GroupType = 4564
                }
            };
            _groupToolRepository.Setup(mocked => mocked.SearchGroups(groupTypeId, keywords, null)).Returns(searchResults);
            var results = _fixture.SearchGroups(groupTypeId, keywordString);
            _groupToolRepository.VerifyAll();
            Assert.IsNotNull(results);
            Assert.AreEqual(searchResults.Count, results.Count);
            foreach (var expected in searchResults)
            {
                Assert.IsNotNull(results.Find(g => g.GroupId == expected.GroupId && g.GroupName.Equals(expected.Name) && g.GroupTypeId == expected.GroupType));
            }
        }

        [Test]
        public void TestSearchGroupsWithLocation()
        {
            int[] groupTypeId = new int[] {1};
            var searchResults = new List<MpGroupSearchResultDto>
            {
                new MpGroupSearchResultDto
                {
                    GroupId = 123,
                    Name = "group 1",
                    GroupType = 1231,
                    Address = new MpAddress
                    {
                        Address_Line_1 = "line1",
                        City = "city",
                        State = "state",
                        Postal_Code = "12301",
                        Latitude = 1,
                        Longitude = 2
                    }
                },
                new MpGroupSearchResultDto
                {
                    GroupId = 456,
                    Name = "group 2",
                    GroupType = 4564,
                    Address = new MpAddress()
                },
                new MpGroupSearchResultDto
                {
                    GroupId = 789,
                    Name = "group 3",
                    GroupType = 7897,
                    Address = new MpAddress
                    {
                        Address_Line_1 = "line1",
                        City = "city",
                        State = "state",
                        Postal_Code = "78901",
                        Latitude = 3,
                        Longitude = 4
                    }
                }
            };

            var groups = searchResults.Select(Mapper.Map<GroupDTO>).ToList();
            const string location = "loc loc loc";
            var geoResults = new List<decimal?> { null, 9, 3 };
            var distanceMatrixResults = new List<decimal?> { 2, 5 };
            _groupToolRepository.Setup(mocked => mocked.SearchGroups(groupTypeId, It.IsAny<string[]>(), null)).Returns(searchResults);
            _addressService.Setup(mocked => mocked.FindOrCreateAddress(It.IsAny<AddressDTO>(), true));
            _addressProximityService.Setup(mocked => mocked.GetProximity(location, It.IsAny<List<AddressDTO>>(), null)).Returns(geoResults);
            _addressMatrixService.Setup(mocked => mocked.GetProximity(location, It.IsAny<List<AddressDTO>>(), null)).Returns(distanceMatrixResults);

            var results = _fixture.SearchGroups(groupTypeId, null, location);
            _addressProximityService.VerifyAll();
            _addressMatrixService.VerifyAll();
            _groupToolRepository.VerifyAll();
            Assert.IsNotNull(results);
            Assert.AreEqual(searchResults.Count, results.Count);

            var expectedOrder = new List<MpGroupSearchResultDto>
            {
                searchResults[2],
                searchResults[1],
                searchResults[0]
            };

            var expectedDistances = new decimal?[]
            {
                2, 5, null
            };

            for (var i = 0; i < expectedOrder.Count; i++)
            {
                Assert.AreEqual(expectedOrder[i].GroupId, results[i].GroupId);
                Assert.AreEqual(expectedDistances[i], results[i].Proximity);
            }
        }

        [Test]
        public void TestSendGroupEndedEmailToParticipant()
        {
            string fromEmailAddress = "from@email.com";
            var participant = new GroupParticipantDTO()
            {
                NickName = "nickname",
                Email = "email@email.com",
                ContactId = 123,
            };
            var template = new MpMessageTemplate
            {
                Body = "body",
                FromContactId = 876,
                FromEmailAddress = "from@crossroads.net",
                ReplyToContactId = 77,
                ReplyToEmailAddress = "reply@crossroads.net",
                Subject = "55"
            };
            _communicationRepository.Setup(mocked => mocked.GetTemplate(It.IsAny<int>())).Returns(template);
            var to = new List<MpContact>();
            to.Add(new MpContact() {ContactId = participant.ContactId, EmailAddress = participant.Email});
            var url = @"https://" + BaseUrl + "/groups/search";
            _communicationRepository.Setup(
                mocked =>
                    mocked.SendMessage(
                        It.Is<MpCommunication>(
                            c =>
                                c.DomainId == DomainId
                                && c.EmailBody.Equals(template.Body)
                                && c.EmailSubject.Equals(template.Subject)
                                && c.FromContact.ContactId == template.FromContactId
                                && c.FromContact.EmailAddress.Equals(template.FromEmailAddress)
                                && c.ReplyToContact.ContactId == template.FromContactId
                                && c.ReplyToContact.EmailAddress.Equals(template.FromEmailAddress)
                                && c.AuthorUserId == 5
                                && c.ToContacts[0].ContactId == participant.ContactId
                                && c.ToContacts[0].EmailAddress == participant.Email
                                && c.TemplateId == GroupEndedParticipantEmailTemplate
                                && c.MergeData["Participant_Name"].Equals("nickname")
                                && c.MergeData["Group_Tool_Url"].Equals(url)
                        ),
                    false)
            ).Returns(5);

            var mergeData = new Dictionary<string, object>()
            {
                {"Participant_Name", "nickname"},
                {"Group_Tool_Url", url}
            };

            _fixture.SendSingleGroupParticipantEmail(participant,GroupEndedParticipantEmailTemplate, mergeData);
            _communicationRepository.VerifyAll();
        }

        [Test]
        public void TestCreateGroupInquiryValid()
        {
            var token = "123ABC";
            var groupId = 123;
            var syncedTime = System.DateTime.Now;
            var active = true;

            MpParticipant contactParticipant = new MpParticipant
            {
                ContactId = 1234567,
                EmailAddress = "test@test.com"
            };

            _participantRepository.Setup(mocked => mocked.GetParticipantRecord(token)).Returns(contactParticipant);

            MpMyContact mpMyContact = new MpMyContact
            {
                Contact_ID = 1234567,
                Last_Name = "Test",
                Nickname = "Test",
                Home_Phone = "555-555-5555"
            };

            _contactRepository.Setup(mocked => mocked.GetContactById(1234567)).Returns(mpMyContact);

            List<MpInquiry> mpInquiries = new List<MpInquiry>();

            _groupToolRepository.Setup(mocked => mocked.GetInquiries(groupId)).Returns(mpInquiries);
            
            List<MpGroupParticipant> participants = new List<MpGroupParticipant>();

            _groupRepository.Setup(mocked => mocked.GetGroupParticipants(groupId, active)).Returns(participants);
            _communicationRepository.Setup(mocked => mocked.GetTemplate(RequestToJoinEmailTemplateId)).Returns(new MpMessageTemplate()
                                                                                                               {
                                                                                                                   ReplyToEmailAddress = "replyto@crossroads.net",
                                                                                                                   ReplyToContactId = 5,
                                                                                                                   Body = "body",
                                                                                                                   FromContactId = 7,
                                                                                                                   Subject = "Subject",
                                                                                                                   FromEmailAddress = "from@crossroads.net"
                                                                                                               });

            _groupRepository.Setup(mocked => mocked.CreateGroupInquiry(It.IsAny<MpInquiry>()));
            _groupService.Setup(mocked => mocked.GetGroupDetails(123)).Returns(new GroupDTO()
                                                                               {
                                                                                   GroupId = 1,
                                                                                   GroupTypeId = smallGroupTypeId,
                                                                                   Participants = new List<GroupParticipantDTO>()
                                                                                   {
                                                                                       new GroupParticipantDTO
                                                                                       {
                                                                                           ContactId = 42,
                                                                                           NickName = "nickName",
                                                                                           GroupRoleId = GroupRoleLeader
                                                                                       }
                                                                                   }
                                                                               });

            _communicationRepository.Setup(mocked => mocked.SendMessage(It.IsAny<MpCommunication>(), false)).Returns(1);


            _fixture.SubmitInquiry(token, groupId,true);
            _mockAnalyticService.Verify(x => x.Track(It.IsAny<string>(), "RequestedToJoinGroup", It.IsAny<EventProperties>()), Times.Once);

            _groupRepository.VerifyAll();
            _groupToolRepository.VerifyAll();

        }

        [Test]
        public void TestCreateAnywhereGroupInquiryValid()
        {
            var token = "123ABC";
            var syncedTime = System.DateTime.Now;
            var active = true;
            var group = new GroupDTO()
            {
                GroupId = 123,
                GroupTypeId = anywhereGroupTypeId,
                GroupName = "Group Name",
                Address = new AddressDTO()
                {
                    City = "city!",
                    State = "OH",
                    PostalCode = "12345"
                },
                Participants = new List<GroupParticipantDTO>()
                {
                    new GroupParticipantDTO
                    {
                        ContactId = 42,
                        NickName = "nickName",
                        GroupRoleId = GroupRoleLeader
                    }
                }
            };

            MpParticipant contactParticipant = new MpParticipant
            {
                ContactId = 1234567,
                EmailAddress = "test@test.com"
            };

            _participantRepository.Setup(mocked => mocked.GetParticipantRecord(token)).Returns(contactParticipant);

            MpMyContact mpMyContact = new MpMyContact
            {
                Contact_ID = 1234567,
                Last_Name = "Test",
                Nickname = "Test",
                Home_Phone = "555-555-5555"
            };

            _contactRepository.Setup(mocked => mocked.GetContactById(1234567)).Returns(mpMyContact);

            List<MpInquiry> mpInquiries = new List<MpInquiry>();

            _groupToolRepository.Setup(mocked => mocked.GetInquiries(group.GroupId)).Returns(mpInquiries);

            List<MpGroupParticipant> participants = new List<MpGroupParticipant>();

            _groupRepository.Setup(mocked => mocked.GetGroupParticipants(group.GroupId, active)).Returns(participants);

            _communicationRepository.Setup(mocked => mocked.GetTemplate(RequestToJoinAnywhereEmailTemplateId)).Returns(new MpMessageTemplate()
            {
                ReplyToEmailAddress = "replyto@crossroads.net",
                ReplyToContactId = 5,
                Body = "body",
                FromContactId = 7,
                Subject = "Subject",
                FromEmailAddress = "from@crossroads.net"
            });


            _groupRepository.Setup(mocked => mocked.CreateGroupInquiry(It.IsAny<MpInquiry>()));
            _groupService.Setup(mocked => mocked.GetGroupDetails(123)).Returns(group);

            _communicationRepository.Setup(mocked => mocked.SendMessage(It.IsAny<MpCommunication>(), false)).Returns(1);

            _fixture.SubmitInquiry(token, group.GroupId, true);
            _mockAnalyticService.Verify(x => x.Track(It.IsAny<string>(), "RequestedToJoinGroup", It.Is<EventProperties>(props => 
                                    props["GroupName"].Equals(group.GroupName) 
                                    && props["GroupState"].Equals(group.Address.State)
                                    && props["GroupCity"].Equals(group.Address.City)
                                    && props["GroupZip"].Equals(group.Address.PostalCode))), Times.Once);

            _groupRepository.VerifyAll();
            _groupToolRepository.VerifyAll();

        }

        [Test]
        public void TestCreateGroupInquiryInvalid()
        {
            var token = "123ABC";
            var groupId = 123;
            var syncedTime = System.DateTime.Now;
            var active = true;

            MpParticipant contactParticipant = new MpParticipant
            {
                ContactId = 1234567
            };

            _participantRepository.Setup(mocked => mocked.GetParticipantRecord(token)).Returns(contactParticipant);

            MpMyContact mpMyContact = new MpMyContact
            {
                Contact_ID = 1234567,
                Last_Name = "Test",
                Nickname = "Test",
                Home_Phone = "555-555-5555"
            };

            _contactRepository.Setup(mocked => mocked.GetContactById(1234567)).Returns(mpMyContact);

            List<MpInquiry> mpInquiries = new List<MpInquiry>();

            var mpInquiry = new MpInquiry
            {
                ContactId = 1234567,
                EmailAddress = "test@test.com",
                FirstName = "Test",
                GroupId = 123,
                InquiryId = 123,
                LastName = "Test",
                PhoneNumber = "555-555-5555",
                Placed = null,
                RequestDate = syncedTime
            };

            mpInquiries.Add(mpInquiry);

            _groupToolRepository.Setup(mocked => mocked.GetInquiries(groupId)).Returns(mpInquiries);

            List<MpGroupParticipant> participants = new List<MpGroupParticipant>();

            var participant = new MpGroupParticipant
            {
                Congregation = "Oakley",
                ContactId = 1234567,
                Email = "test@test.com",
                GroupParticipantId = 7654321,
                GroupRoleId = 33,
                LastName = "TestLast",
                GroupRoleTitle = "Participant",
                NickName = "TestFirst",
                ParticipantId = 2222222,
                StartDate = syncedTime
            };

            participants.Add(participant);

            _groupRepository.Setup(mocked => mocked.GetGroupParticipants(groupId, active)).Returns(participants);
            

            try
            {
                _fixture.SubmitInquiry(token, groupId, true);
                Assert.Fail("expected exception was not thrown");
            }
            catch (ExistingRequestException e)
            {
                Assert.AreSame(typeof(ExistingRequestException), e.GetType());
            }

            _mockAnalyticService.Verify(x => x.Track(It.IsAny<string>(), "RequestedToJoinGroup", It.IsAny<EventProperties>()), Times.Never);

            _groupRepository.VerifyAll();
            _groupToolRepository.VerifyAll();
        }

        [Test]
        public void ShouldReturnJourneyName()
        {
            const string journeyName = "This is not the greatest journey in the world no, this is just a tribute";
            _groupToolRepository.Setup(m => m.GetCurrentJourney()).Returns(journeyName);

            var result = _fixture.GetCurrentJourney();
            _groupToolRepository.VerifyAll();
            Assert.AreEqual(result, journeyName);
        }

        [Test]
        public void ShouldReturnNullJourneyName()
        {
            _groupToolRepository.Setup(m => m.GetCurrentJourney()).Returns((string) null);
            var result = _fixture.GetCurrentJourney();
            _groupToolRepository.VerifyAll();
            Assert.IsNull(result);
        }

        [Test]
        public void TestSendSmallGroupPendingInquiryReminderEmails()
        {
            var inquiries = new List<MpInquiry>
            {
                new MpInquiry
                {
                    GroupId = 123,
                    FirstName = "first #1 123",
                    LastName = "last #1 123",
                    EmailAddress = "email #1 123"
                },
                new MpInquiry
                {
                    GroupId = 456,
                    FirstName = "first #1 456",
                    LastName = "last #1 456",
                    EmailAddress = "email #1 456"
                },
                new MpInquiry
                {
                    GroupId = 123,
                    FirstName = "first #2 123",
                    LastName = "last #2 123",
                    EmailAddress = "email #2 123"
                },
            };
            _groupToolRepository.Setup(mocked => mocked.GetInquiries(null)).Returns(inquiries);

            var group123 = new MpGroup
            {
                GroupId = 123,
                Name = "group 123",
                GroupDescription = "description 123",
                Participants = new List<MpGroupParticipant>
                {
                    new MpGroupParticipant
                    {
                        ContactId = 1231,
                        Email = "email 1231",
                        NickName = "nick 1231",
                        LastName = "last 1231",
                        GroupRoleId = GroupRoleLeader
                    },
                    new MpGroupParticipant
                    {
                        ContactId = 1232,
                        Email = "email 1232",
                        NickName = "nick 1232",
                        LastName = "last 1232",
                        GroupRoleId = GroupRoleLeader + 1
                    },
                    new MpGroupParticipant
                    {
                        ContactId = 1233,
                        Email = "email 1233",
                        NickName = "nick 1233",
                        LastName = "last 1233",
                        GroupRoleId = GroupRoleLeader
                    },
                }
            };
            _groupRepository.Setup(mocked => mocked.getGroupDetails(123)).Returns(group123);

            var group456 = new MpGroup
            {
                GroupId = 456,
                Name = "group 456",
                GroupDescription = "description 456",
                Participants = new List<MpGroupParticipant>
                {
                    new MpGroupParticipant
                    {
                        ContactId = 4561,
                        Email = "email 4561",
                        NickName = "nick 4561",
                        LastName = "last 4561",
                        GroupRoleId = GroupRoleLeader
                    },
                    new MpGroupParticipant
                    {
                        ContactId = 4562,
                        Email = "email 4562",
                        NickName = "nick 4562",
                        LastName = "last 4562",
                        GroupRoleId = GroupRoleLeader + 1
                    },
                    new MpGroupParticipant
                    {
                        ContactId = 4563,
                        Email = "email 4563",
                        NickName = "nick 4563",
                        LastName = "last 4563",
                        GroupRoleId = GroupRoleLeader
                    },
                }
            };
            _groupRepository.Setup(mocked => mocked.getGroupDetails(456)).Returns(group456);

            _emailCommunicationService.Setup(mocked => mocked.SendEmail(It.Is<EmailCommunicationDTO>(e =>
                                                                                                         e.groupId == 123 &&
                                                                                                         e.TemplateId == GroupRequestPendingReminderEmailTemplateId &&
                                                                                                         e.ToContactId == 1231
                                                                            ),
                                                                        null));
            _emailCommunicationService.Setup(mocked => mocked.SendEmail(It.Is<EmailCommunicationDTO>(e =>
                                                                                                         e.groupId == 123 &&
                                                                                                         e.TemplateId == GroupRequestPendingReminderEmailTemplateId &&
                                                                                                         e.ToContactId == 1233
                                                                            ),
                                                                        null));
            _emailCommunicationService.Setup(mocked => mocked.SendEmail(It.Is<EmailCommunicationDTO>(e =>
                                                                                                         e.groupId == 456 &&
                                                                                                         e.TemplateId == GroupRequestPendingReminderEmailTemplateId &&
                                                                                                         e.ToContactId == 4561
                                                                            ),
                                                                        null));
            _emailCommunicationService.Setup(mocked => mocked.SendEmail(It.Is<EmailCommunicationDTO>(e =>
                                                                                                         e.groupId == 456 &&
                                                                                                         e.TemplateId == GroupRequestPendingReminderEmailTemplateId &&
                                                                                                         e.ToContactId == 4563
                                                                            ),
                                                                        null));

            _fixture.SendSmallGroupPendingInquiryReminderEmails();
            _groupToolRepository.VerifyAll();
            _groupRepository.VerifyAll();
            _emailCommunicationService.VerifyAll();
        }

        [Test]
        public void TestSendSmallGroupPendingInquiryReminderEmailsForAnywhereGathering()
        {
            var inquiries = new List<MpInquiry>
            {
                new MpInquiry
                {
                    GroupId = 123,
                    FirstName = "first #1 123",
                    LastName = "last #1 123",
                    EmailAddress = "email #1 123"
                },
                new MpInquiry
                {
                    GroupId = 456,
                    FirstName = "first #1 456",
                    LastName = "last #1 456",
                    EmailAddress = "email #1 456"
                },
                new MpInquiry
                {
                    GroupId = 123,
                    FirstName = "first #2 123",
                    LastName = "last #2 123",
                    EmailAddress = "email #2 123"
                },
            };
            _groupToolRepository.Setup(mocked => mocked.GetInquiries(null)).Returns(inquiries);
           
            var group123 = new MpGroup
            {
                GroupId = 123,
                GroupType = anywhereGroupTypeId,
                Name = "group 123",
                GroupDescription = "description 123",
                Participants = new List<MpGroupParticipant>
                {
                    new MpGroupParticipant
                    {
                        ContactId = 1231,
                        Email = "email 1231",
                        NickName = "nick 1231",
                        LastName = "last 1231",
                        GroupRoleId = GroupRoleLeader
                    },
                    new MpGroupParticipant
                    {
                        ContactId = 1232,
                        Email = "email 1232",
                        NickName = "nick 1232",
                        LastName = "last 1232",
                        GroupRoleId = GroupRoleLeader + 1
                    },
                    new MpGroupParticipant
                    {
                        ContactId = 1233,
                        Email = "email 1233",
                        NickName = "nick 1233",
                        LastName = "last 1233",
                        GroupRoleId = GroupRoleLeader
                    },
                }
            };
            _groupRepository.Setup(mocked => mocked.getGroupDetails(123)).Returns(group123);

            var group456 = new MpGroup
            {
                GroupId = 456,
                GroupType = anywhereGroupTypeId,
                Name = "group 456",
                GroupDescription = "description 456",
                Participants = new List<MpGroupParticipant>
                {
                    new MpGroupParticipant
                    {
                        ContactId = 4561,
                        Email = "email 4561",
                        NickName = "nick 4561",
                        LastName = "last 4561",
                        GroupRoleId = GroupRoleLeader
                    },
                    new MpGroupParticipant
                    {
                        ContactId = 4562,
                        Email = "email 4562",
                        NickName = "nick 4562",
                        LastName = "last 4562",
                        GroupRoleId = GroupRoleLeader + 1
                    },
                    new MpGroupParticipant
                    {
                        ContactId = 4563,
                        Email = "email 4563",
                        NickName = "nick 4563",
                        LastName = "last 4563",
                        GroupRoleId = GroupRoleLeader
                    },
                }
            };
            _groupRepository.Setup(mocked => mocked.getGroupDetails(456)).Returns(group456);

            _emailCommunicationService.Setup(mocked => mocked.SendEmail(It.Is<EmailCommunicationDTO>(e =>
                                                                                                         e.groupId == 123 &&
                                                                                                         e.TemplateId == GatheringRequestPendingReminderEmailTemplateId &&
                                                                                                         e.ToContactId == 1231
                                                                            ),
                                                                        null));
            _emailCommunicationService.Setup(mocked => mocked.SendEmail(It.Is<EmailCommunicationDTO>(e =>
                                                                                                         e.groupId == 123 &&
                                                                                                         e.TemplateId == GatheringRequestPendingReminderEmailTemplateId &&
                                                                                                         e.ToContactId == 1233
                                                                            ),
                                                                        null));
            _emailCommunicationService.Setup(mocked => mocked.SendEmail(It.Is<EmailCommunicationDTO>(e =>
                                                                                                         e.groupId == 456 &&
                                                                                                         e.TemplateId == GatheringRequestPendingReminderEmailTemplateId &&
                                                                                                         e.ToContactId == 4561
                                                                            ),
                                                                        null));
            _emailCommunicationService.Setup(mocked => mocked.SendEmail(It.Is<EmailCommunicationDTO>(e =>
                                                                                                         e.groupId == 456 &&
                                                                                                         e.TemplateId == GatheringRequestPendingReminderEmailTemplateId &&
                                                                                                         e.ToContactId == 4563
                                                                            ),
                                                                        null));

            _fixture.SendSmallGroupPendingInquiryReminderEmails();
            _groupToolRepository.VerifyAll();
            _groupRepository.VerifyAll();
            _emailCommunicationService.VerifyAll();
        }
    }
}

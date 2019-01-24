using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Web.Http;
using System.Web.Http.Controllers;
using System.Web.Http.Results;
using crds_angular.Controllers.API;
using crds_angular.Models.Crossroads;
using crds_angular.Models.Crossroads.Groups;
using crds_angular.Models.Crossroads.Profile;
using crds_angular.Services.Interfaces;
using Crossroads.Web.Common.Security;
using MinistryPlatform.Translation.Exceptions;
using MinistryPlatform.Translation.Models;
using MinistryPlatform.Translation.Repositories.Interfaces;
using Moq;
using NUnit.Framework;
using MpEvent = MinistryPlatform.Translation.Models.MpEvent;

namespace crds_angular.test.controllers
{
    [TestFixture]
    public class GroupControllerTest
    {
        private GroupController _fixture;

        private Mock<IAuthTokenExpiryService> _authTokenExpiryService;
        private Mock<crds_angular.Services.Interfaces.IGroupService> _groupServiceMock;
        private Mock<IAuthenticationRepository> _authenticationServiceMock;
        private Mock<IContactRepository> _contactRepositoryMock;
        private Mock<IParticipantRepository> _participantServiceMock;
        private Mock<crds_angular.Services.Interfaces.IAddressService> _addressServiceMock;        
        private Mock<IGroupSearchService> _groupSearchServiceMock;
        private Mock<IGroupToolService> _groupToolServiceMock;
        private string _authType;
        private string _authToken;

        [SetUp]
        public void SetUp()
        {
            _authTokenExpiryService = new Mock<IAuthTokenExpiryService>();
            _groupServiceMock = new Mock<crds_angular.Services.Interfaces.IGroupService>();
            _authenticationServiceMock = new Mock<IAuthenticationRepository>();
            _contactRepositoryMock = new Mock<IContactRepository>();
            _participantServiceMock = new Mock<IParticipantRepository>();
            _addressServiceMock = new Mock<crds_angular.Services.Interfaces.IAddressService>();            
            _groupSearchServiceMock = new Mock<IGroupSearchService>();
            _groupToolServiceMock = new Mock<IGroupToolService>();

            _fixture = new GroupController(_authTokenExpiryService.Object,
                                          _groupServiceMock.Object, 
                                          _authenticationServiceMock.Object, 
                                          _contactRepositoryMock.Object, 
                                          _participantServiceMock.Object, 
                                          _addressServiceMock.Object, 
                                          _groupSearchServiceMock.Object, 
                                          _groupToolServiceMock.Object, 
                                          new Mock<IUserImpersonationService>().Object);

            _authType = "auth_type";
            _authToken = "auth_token";
            _fixture.Request = new HttpRequestMessage();
            _fixture.Request.Headers.Authorization = new AuthenticationHeaderValue(_authType, _authToken);
            _fixture.RequestContext = new HttpRequestContext();
        }

        [Test]
        public void TestGetGroupDetails()
        {
            int groupId = 333;
            int contactId = 777;

            MpGroup g = new MpGroup();
            g.GroupId = 333;
            g.GroupType = 8;
            g.GroupRole = "Member";
            g.Name = "Test Me";
            g.GroupId = 123456;
            g.TargetSize = 5;
            g.WaitList = true;
            g.WaitListGroupId = 888;
            g.RemainingCapacity = 10;

            MpParticipant participant = new MpParticipant();
            participant.ParticipantId = 90210;
            _participantServiceMock.Setup(
                mocked => mocked.GetParticipantRecord())
                .Returns(participant);

            _contactRepositoryMock.Setup(mocked => mocked.GetContactId(_fixture.Request.Headers.Authorization.ToString())).Returns(contactId);

            var relationRecord = new MpGroupSignupRelationships
            {
                RelationshipId = 1,
                RelationshipMinAge = 00,
                RelationshipMaxAge = 100
            };

            var groupDto = new GroupDTO
            {
            };
            _authTokenExpiryService.Setup(a => a.IsAuthtokenCloseToExpiry(It.IsAny<HttpRequestHeaders>())).Returns(true);
            _groupServiceMock.Setup(mocked => mocked.getGroupDetails(groupId, contactId, participant, _fixture.Request.Headers.Authorization.ToString())).Returns(groupDto);


            IHttpActionResult result = _fixture.Get(groupId);
            Assert.IsNotNull(result);
            Assert.IsInstanceOf(typeof (OkNegotiatedContentResult<GroupDTO>), result);
            _groupServiceMock.VerifyAll();

            var groupDtoResponse = ((OkNegotiatedContentResult<GroupDTO>) result).Content;

            Assert.NotNull(result);
            Assert.AreSame(groupDto, groupDtoResponse);
        }

        [Test]
        public void TestGetMyGroupsByType()
        {
            var groups = new List<GroupDTO>();
            _groupToolServiceMock.Setup(mocked => mocked.GetGroupToolGroups(It.IsAny<string>())).Returns(groups);
            var result = _fixture.GetMyGroups();
            Assert.IsNotNull(result);
            Assert.IsInstanceOf(typeof(OkNegotiatedContentResult<List<GroupDTO>>), result);
            _groupServiceMock.VerifyAll();

            var groupDtoResponse = ((OkNegotiatedContentResult<List<GroupDTO>>)result).Content;

            Assert.NotNull(result);
            Assert.AreSame(groups, groupDtoResponse);
        }

        [Test]
        public void GetGroupsByTypeForParticipantNoGroups()
        {
            const string token = "1234frd32";
            const int groupTypeId = 19;
          
            var participant = new Participant() 
            { 
                ParticipantId = 90210
            };

            var groups = new List<GroupDTO>();
            _authTokenExpiryService.Setup(a => a.IsAuthtokenCloseToExpiry(It.IsAny<HttpRequestHeaders>())).Returns(true);
            _groupServiceMock.Setup(
                mocked => mocked.GetParticipantRecord(_fixture.Request.Headers.Authorization.ToString()))
                .Returns(participant);

            _groupServiceMock.Setup(mocked => mocked.GetGroupsByTypeForParticipant(token, participant.ParticipantId, groupTypeId)).Returns(groups);
          
            IHttpActionResult result = _fixture.GetGroups(groupTypeId);
            Assert.IsNotNull(result);
            Assert.IsInstanceOf(typeof(OkNegotiatedContentResult<List<GroupDTO>>), result);
        }

        [Test]
        public void GetGroupsByTypeForParticipanGroupsFound()
        {
            const int groupTypeId = 19;

            var participant = new Participant()
            {
                ParticipantId = 90210
            };
            
            var groups = new List<GroupDTO>()
            {
                new GroupDTO()
                {
                    GroupName = "This will work"
                }
            };
            _authTokenExpiryService.Setup(a => a.IsAuthtokenCloseToExpiry(It.IsAny<HttpRequestHeaders>())).Returns(true);
           _groupServiceMock.Setup(
               mocked => mocked.GetParticipantRecord(_fixture.Request.Headers.Authorization.ToString()))
               .Returns(participant);

            _groupServiceMock.Setup(mocked => mocked.GetGroupsByTypeForParticipant(_fixture.Request.Headers.Authorization.ToString(), participant.ParticipantId, groupTypeId)).Returns(groups);

            IHttpActionResult result = _fixture.GetGroups(groupTypeId);
            Assert.IsNotNull(result);
            Assert.IsInstanceOf(typeof(OkNegotiatedContentResult<List<GroupDTO>>), result);
      }

        [Test]
        public void PostGroupSuccessfully()
        {
            var group = new GroupDTO()
            {
                GroupName = "This will work"
            };

            var returnGroup = new GroupDTO()
            {
                GroupName = "This will work"
            };
            _authTokenExpiryService.Setup(a => a.IsAuthtokenCloseToExpiry(It.IsAny<HttpRequestHeaders>())).Returns(true);
            _groupServiceMock.Setup(mocked => mocked.CreateGroup(group)).Returns(returnGroup);

            IHttpActionResult result = _fixture.PostGroup(group);
            Assert.IsNotNull(result);
            Assert.IsInstanceOf(typeof (CreatedNegotiatedContentResult<GroupDTO>), result);
        }

        [Test]
        public void PostGroupFailed()
        {
            Exception ex = new Exception();

            var group = new GroupDTO()
            {
                GroupName = "This will work"
            };
            _authTokenExpiryService.Setup(a => a.IsAuthtokenCloseToExpiry(It.IsAny<HttpRequestHeaders>())).Returns(true);
            _groupServiceMock.Setup(mocked => mocked.CreateGroup(group)).Throws(ex);

            IHttpActionResult result = _fixture.PostGroup(group);
            _groupServiceMock.VerifyAll();
            Assert.IsNotNull(result);
            Assert.IsInstanceOf(typeof (BadRequestResult), result);
        }


        [Test]
        public void PostGroupWithAddressSuccessfully()
        {
            var group = new GroupDTO()
            {
                GroupName = "This will work",
                Address = new AddressDTO()
                {
                    AddressLine1 = "123 Abc St.",
                    AddressLine2 = "Apt. 2",
                    City = "Cincinnati",
                    State = "OH",
                    County = "Hamilton",
                    ForeignCountry = "United States",
                    PostalCode = "45213"
                }
            };

            var returnGroup = new GroupDTO()
            {
                GroupName = "This will work"
            };
            _authTokenExpiryService.Setup(a => a.IsAuthtokenCloseToExpiry(It.IsAny<HttpRequestHeaders>())).Returns(true);
            _groupServiceMock.Setup(mocked => mocked.CreateGroup(group)).Returns(returnGroup);            

            IHttpActionResult result = _fixture.PostGroup(group);
            _addressServiceMock.Verify(x=> x.FindOrCreateAddress(group.Address, true), Times.Once);
            _groupServiceMock.VerifyAll();            
        }

        [Test]
        public void PostGroupWithNullAddressLine1_WillNotAddAddressToGroup_Successfully()
        {
            var group = new GroupDTO()
            {
                GroupName = "This will work",
                Address = new AddressDTO()
                {
                    AddressLine1 = null,
                    AddressLine2 = "Apt. 2",
                    City = "Cincinnati",
                    State = "OH",
                    County = "Hamilton",
                    ForeignCountry = "United States",
                    PostalCode = "45213"
                }
            };

            var returnGroup = new GroupDTO()
            {
                GroupName = "This will work"
            };
            _authTokenExpiryService.Setup(a => a.IsAuthtokenCloseToExpiry(It.IsAny<HttpRequestHeaders>())).Returns(true);
            _groupServiceMock.Setup(mocked => mocked.CreateGroup(group)).Returns(returnGroup);

            IHttpActionResult result = _fixture.PostGroup(group);
            _addressServiceMock.Verify(x => x.FindOrCreateAddress(group.Address, true), Times.Never);
            _groupServiceMock.VerifyAll();
        }

        [Test]
        public void PostGroupWithWithEmptyAddressLine1_WillNotAddAddressToGroup_Successfully()
        {
            var group = new GroupDTO()
            {
                GroupName = "This will work",
                Address = new AddressDTO()
                {
                    AddressLine1 = string.Empty,
                    AddressLine2 = "Apt. 2",
                    City = "Cincinnati",
                    State = "OH",
                    County = "Hamilton",
                    ForeignCountry = "United States",
                    PostalCode = "45213"
                }
            };

            var returnGroup = new GroupDTO()
            {
                GroupName = "This will work"
            };
            _authTokenExpiryService.Setup(a => a.IsAuthtokenCloseToExpiry(It.IsAny<HttpRequestHeaders>())).Returns(true);
            _groupServiceMock.Setup(mocked => mocked.CreateGroup(group)).Returns(returnGroup);

            IHttpActionResult result = _fixture.PostGroup(group);
            _addressServiceMock.Verify(x => x.FindOrCreateAddress(group.Address, true), Times.Never);
            _groupServiceMock.VerifyAll();
        }

        [Test]
        public void PostGroupWithoutAddressSuccessfully()
        {
            var group = new GroupDTO()
            {
                GroupName = "This will work",
                Address = null
            };

            var returnGroup = new GroupDTO()
            {
                GroupName = "This will work"
            };
            _authTokenExpiryService.Setup(a => a.IsAuthtokenCloseToExpiry(It.IsAny<HttpRequestHeaders>())).Returns(true);
            _groupServiceMock.Setup(mocked => mocked.CreateGroup(group)).Returns(returnGroup);

            IHttpActionResult result = _fixture.PostGroup(group);
            _addressServiceMock.Verify(x => x.FindOrCreateAddress(group.Address, true), Times.Never);            
            _groupServiceMock.VerifyAll();
        }

        [Test]
        public void TestGetGroupParticipantsFound()
        {
            const string token = "1234frd32";
            const int groupId = 170656;

            var participant = new List<GroupParticipantDTO>();
            _authTokenExpiryService.Setup(a => a.IsAuthtokenCloseToExpiry(It.IsAny<HttpRequestHeaders>())).Returns(true);
            _groupServiceMock.Setup(mocked => mocked.GetGroupParticipants(groupId, true)).Returns(participant);

            IHttpActionResult result = _fixture.GetGroupParticipants(groupId);
            Assert.IsNotNull(result);
            Assert.IsInstanceOf(typeof(OkNegotiatedContentResult<List<GroupParticipantDTO>>), result);
        }

        [Test]
        public void TestGetGroupParticipantsEmptyGroup()
        {
            const string token = "1234frd32";
            const int groupId = 1234;

            var participant = new List<GroupParticipantDTO>();
            _authTokenExpiryService.Setup(a => a.IsAuthtokenCloseToExpiry(It.IsAny<HttpRequestHeaders>())).Returns(true);
            _groupServiceMock.Setup(mocked => mocked.GetGroupParticipants(groupId, true)).Returns(participant);

            IHttpActionResult result = _fixture.GetGroupParticipants(groupId);

            Assert.IsNotNull(result);
            Assert.IsInstanceOf(typeof(OkNegotiatedContentResult<List<GroupParticipantDTO>>), result);
            Assert.AreEqual(0, participant.Count);
        }

        [Test]
        public void ShouldEditGroupSuccessfully()
        {
            var group = new GroupDTO()
            {
                GroupName = "This will work",
                Address = new AddressDTO
                {
                    AddressLine1 = "line 1",
                    City = "city",
                    State = "state",
                    PostalCode = "zip"
                }
            };

            var returnGroup = new GroupDTO()
            {
                GroupName = "This will work"
            };
            _authTokenExpiryService.Setup(a => a.IsAuthtokenCloseToExpiry(It.IsAny<HttpRequestHeaders>())).Returns(true);
            _groupServiceMock.Setup(mocked => mocked.UpdateGroup(group)).Returns(returnGroup);
            _addressServiceMock.Setup(mocked => mocked.FindOrCreateAddress(It.IsAny<AddressDTO>(), true));

            IHttpActionResult result = _fixture.EditGroup(group);
            _addressServiceMock.VerifyAll();
            Assert.IsNotNull(result);
            Assert.IsInstanceOf(typeof(CreatedNegotiatedContentResult<GroupDTO>), result);
        }

        [Test]
        public void ShouldNotEditGroup()
        {
            Exception ex = new Exception();

            var group = new GroupDTO()
            {
                GroupName = "This will work",
                Address = new AddressDTO
                {
                    AddressLine1 = "line 1",
                    City = "city",
                    State = "state",
                    PostalCode = "zip"
                }
            };
            _authTokenExpiryService.Setup(a => a.IsAuthtokenCloseToExpiry(It.IsAny<HttpRequestHeaders>())).Returns(true);
            _groupServiceMock.Setup(mocked => mocked.UpdateGroup(group)).Throws(ex);
            _addressServiceMock.Setup(mocked => mocked.FindOrCreateAddress(It.IsAny<AddressDTO>(), true));

            IHttpActionResult result = _fixture.EditGroup(group);
            _groupServiceMock.VerifyAll();
            _addressServiceMock.VerifyAll();
            Assert.IsNotNull(result);
            Assert.IsInstanceOf(typeof(BadRequestResult), result);
        }

        [Test]
        public void ShouldCallServiceUpdateParticipant()
        {
            var participant = new GroupParticipantDTO()
            {
                GroupParticipantId = 1,
                GroupRoleId = 22,
                GroupRoleTitle = "Group Leader"
            };
            _authTokenExpiryService.Setup(a => a.IsAuthtokenCloseToExpiry(It.IsAny<HttpRequestHeaders>())).Returns(true);
            _groupServiceMock.Setup(x => x.UpdateGroupParticipantRole(It.IsAny<GroupParticipantDTO>()));
            _fixture.UpdateParticipant(participant);
            _groupServiceMock.Verify();
        }
    }
}

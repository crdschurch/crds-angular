using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Web.Http;
using System.Web.Http.Controllers;
using System.Web.Http.Results;
using crds_angular.Controllers.API;
using crds_angular.Models;
using crds_angular.Models.Crossroads.Profile;
using crds_angular.Models.Crossroads.Serve;
using crds_angular.Services.Interfaces;
using Crossroads.Utilities.Interfaces;
using Crossroads.Web.Common;
using Crossroads.Web.Common.Configuration;
using Crossroads.Web.Common.Security;
using MinistryPlatform.Translation.Models;
using MinistryPlatform.Translation.Models.DTO;
using MinistryPlatform.Translation.Repositories.Interfaces;
using Moq;
using NUnit.Framework;
using IDonorService = crds_angular.Services.Interfaces.IDonorService;

namespace crds_angular.test.controllers
{
    [TestFixture]
    public class ProfileControllerTest
    {
        private ProfileController _fixture;

        private Mock<IAuthTokenExpiryService> _authTokenExpiryService;
        private Mock<crds_angular.Services.Interfaces.IPersonService> _personServiceMock;
        private Mock<IServeService> _serveServiceMock;
        private Mock<IAuthenticationRepository> _authenticationServiceMock;
        private Mock<crds_angular.Services.Interfaces.IDonorService> _donorService;
        private Mock<IUserImpersonationService> _impersonationService;
        private Mock<IAuthenticationRepository> _authenticationService;
        private Mock<IUserRepository> _userService;
        private Mock<IContactRelationshipRepository> _contactRelationshipService;
        private Mock<IConfigurationWrapper> _config;

        private string _authType;
        private string _authToken;

        private int myContactId = 123456;

        [SetUp]
        public void SetUp()
        {
            _authTokenExpiryService = new Mock<IAuthTokenExpiryService>();
            _personServiceMock = new Mock<crds_angular.Services.Interfaces.IPersonService>();
            _serveServiceMock = new Mock<IServeService>();
            _donorService = new Mock<IDonorService>();
            _impersonationService = new Mock<IUserImpersonationService>();
            _authenticationService = new Mock<IAuthenticationRepository>();
            _userService = new Mock<IUserRepository>();
            _contactRelationshipService = new Mock<IContactRelationshipRepository>();
            _config = new Mock<IConfigurationWrapper>();

            _config.Setup(mocked => mocked.GetConfigValue("AdminGetProfileRoles")).Returns("123,456");

            _fixture = new ProfileController(_authTokenExpiryService.Object, 
                                             _personServiceMock.Object, 
                                             _serveServiceMock.Object, 
                                             _impersonationService.Object, 
                                             _donorService.Object, 
                                             _authenticationService.Object, 
                                             _userService.Object, 
                                             _contactRelationshipService.Object, 
                                             _config.Object, 
                                             new Mock<IUserImpersonationService>().Object);
            _authenticationServiceMock = new Mock<IAuthenticationRepository>();

            _authType = "auth_type";
            _authToken = "auth_token";
            _fixture.Request = new HttpRequestMessage();
            _fixture.Request.Headers.Authorization = new AuthenticationHeaderValue(_authType, _authToken);
            _fixture.RequestContext = new HttpRequestContext();

        }

        [Test]
        public void TestAdminGetProfileUnauthorized()
        {
            var user = new MpUser
            {
                UserRecordId = 987
            };
            _authTokenExpiryService.Setup(a => a.IsAuthtokenCloseToExpiry(It.IsAny<HttpRequestHeaders>())).Returns(true);
            _authTokenExpiryService.Setup(a => a.IsAuthtokenCloseToExpiry(It.IsAny<HttpRequestHeaders>())).Returns(true);
            _userService.Setup(mocked => mocked.GetByAuthenticationToken(_authType + " " + _authToken)).Returns(user);
            _userService.Setup(mocked => mocked.GetUserRoles(987)).Returns((List<MpRoleDto>) null);

            var result = _fixture.AdminGetProfile(13579);
            Assert.IsNotNull(result);
            Assert.IsInstanceOf<UnauthorizedResult>(result);
            _userService.VerifyAll();
            _personServiceMock.VerifyAll();
        }

        [Test]
        public void TestAdminGetProfile()
        {
            var user = new MpUser
            {
                UserRecordId = 987
            };
            _authTokenExpiryService.Setup(a => a.IsAuthtokenCloseToExpiry(It.IsAny<HttpRequestHeaders>())).Returns(true);
            _userService.Setup(mocked => mocked.GetByAuthenticationToken(_authType + " " + _authToken)).Returns(user);
            _userService.Setup(mocked => mocked.GetUserRoles(987)).Returns(new List<MpRoleDto>
            {
                new MpRoleDto
                {
                    Id = 765
                },
                new MpRoleDto
                {
                    Id = 456
                }
            });

            var person = new Person();
            _personServiceMock.Setup(mocked => mocked.GetPerson(13579)).Returns(person);

            var result = _fixture.AdminGetProfile(13579);
            Assert.IsNotNull(result);
            Assert.IsInstanceOf<OkNegotiatedContentResult<Person>>(result);
            var r = (OkNegotiatedContentResult<Person>)result;
            Assert.IsNotNull(r.Content);
            Assert.AreSame(person, r.Content);
            _userService.VerifyAll();
            _personServiceMock.VerifyAll();
        }

        [Test]
        public void shouldGetProfileForFamilyMember()
        {
            
            Person me = new Person()
            {
                ContactId = myContactId
            };

            Person brady = new Person()
            {
                ContactId = 13579,
                NickName = "Brady"
            };

            var familyList = new List<FamilyMember>
            {
                new FamilyMember()
                {
                    Age = 52,
                    ContactId = myContactId,
                    Email = "test@mail.com",
                    LastName = "Maddox",
                    LoggedInUser = true,
                    ParticipantId = 123456,
                    PreferredName = "Tony",
                    RelationshipId = 1
                },
                new FamilyMember()
                {
                    Age = 12,
                    ContactId = 13579,
                    LastName = "Maddox",
                    PreferredName = "Brady"
                }
            };

            _serveServiceMock.Setup(x => x.GetImmediateFamilyParticipants(It.IsAny<int>())).Returns(familyList);
            _personServiceMock.Setup(x => x.GetLoggedInUserProfile(_authType + " " + _authToken)).Returns(me);
            _personServiceMock.Setup(x => x.GetPerson(13579)).Returns(brady);
            
            IHttpActionResult result = _fixture.GetProfile(13579);
            Assert.IsNotNull(result);
            Assert.IsInstanceOf<OkNegotiatedContentResult<Person>>(result);
            var r = (OkNegotiatedContentResult<Person>)result;
            Assert.IsNotNull(r.Content);
            Assert.AreEqual(r.Content.ContactId, brady.ContactId);

        }

        [Test]
        public void ShouldGetSpouse()
        {
            var heather = new FamilyMember
            {
                ContactId = 98765,
                PreferredName = "Heather",
                LastName = "Augustine",
                RelationshipId = 1,
                Age = 29,
                Email = "user@isp.tld"
            };


            var familyList = new List<MpContactRelationship>
            {
                new MpContactRelationship()
                {
                    Age = heather.Age,
                    Contact_Id = heather.ContactId,
                    Participant_Id = 346323,
                    Last_Name = heather.LastName,
                    Preferred_Name = heather.PreferredName,
                    Relationship_Id = heather.RelationshipId,
                    Email_Address = heather.Email,
                    HighSchoolGraduationYear = 0
                },
                new MpContactRelationship()
                {
                    Age = 12,
                    Contact_Id = 13579,
                    Last_Name = "Maddox",
                    Preferred_Name = "Brady",
                    Relationship_Id = 6
                }
            };

            _authTokenExpiryService.Setup(a => a.IsAuthtokenCloseToExpiry(It.IsAny<HttpRequestHeaders>())).Returns(true);
            _contactRelationshipService.Setup(x => x.GetMyImmediateFamilyRelationships(myContactId)).Returns(familyList);

            IHttpActionResult result = _fixture.GetMySpouse(myContactId);

            Assert.IsNotNull(result);
            Assert.IsInstanceOf<OkNegotiatedContentResult<FamilyMember>>(result);
            var r = (OkNegotiatedContentResult<FamilyMember>)result;
            Assert.AreEqual(r.Content.PreferredName, heather.PreferredName);
        }

        [Test]
        public void ShouldNotReturnSpouse()
        {
           var familyList = new List<MpContactRelationship>
            {
                new MpContactRelationship()
                {
                    Age = 12,
                    Contact_Id = 13579,
                    Last_Name = "Maddox",
                    Preferred_Name = "Brady",
                    Relationship_Id = 6
                }
            };

            _authTokenExpiryService.Setup(a => a.IsAuthtokenCloseToExpiry(It.IsAny<HttpRequestHeaders>())).Returns(true);
            _contactRelationshipService.Setup(x => x.GetMyImmediateFamilyRelationships(myContactId)).Returns(familyList);

            IHttpActionResult result = _fixture.GetMySpouse(myContactId);

            Assert.IsNotNull(result);
            Assert.IsInstanceOf<OkNegotiatedContentResult<FamilyMember>>(result);
            var r = (OkNegotiatedContentResult<FamilyMember>)result;
            Assert.IsNull(r.Content);
        }
        
    }
}
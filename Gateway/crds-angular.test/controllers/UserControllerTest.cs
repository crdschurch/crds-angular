using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Web.Http;
using System.Web.Http.Controllers;
using System.Web.Http.Results;
using crds_angular.Controllers.API;
using crds_angular.Exceptions;
using crds_angular.Models.Crossroads;
using crds_angular.Services.Analytics;
using crds_angular.Services.Interfaces;
using Crossroads.Web.Common.Security;
using MinistryPlatform.Translation.Models;
using MinistryPlatform.Translation.Models.DTO;
using MinistryPlatform.Translation.Repositories.Interfaces;
using Moq;
using Newtonsoft.Json;
using NUnit.Framework;

namespace crds_angular.test.controllers
{
    public class UserControllerTest
    {
        private UserController _fixture;

        private Mock<IAuthTokenExpiryService> _authTokenExpiryService;
        private Mock<IAccountService> _accountService;
        private Mock<IUserRepository> _userRepository;
        private Mock<IContactRepository> _contactRepository;
        private Mock<IAnalyticsService> _mockAnalyticService;

        private string _authType;
        private string _authToken;

        [SetUp]
        public void SetUp()
        {
            _authTokenExpiryService = new Mock<IAuthTokenExpiryService>();
            _accountService = new Mock<IAccountService>();
            _userRepository = new Mock<IUserRepository>();
            _contactRepository = new Mock<IContactRepository>();
            _mockAnalyticService = new Mock<IAnalyticsService>();

            _fixture = new UserController(_authTokenExpiryService.Object, 
                                          _accountService.Object, 
                                          _contactRepository.Object, 
                                          _userRepository.Object, 
                                          _mockAnalyticService.Object, 
                                          new Mock<IUserImpersonationService>().Object,
                                          new Mock<IAuthenticationRepository>().Object);

            _authType = "auth_type";
            _authToken = "auth_token";
            _fixture.Request = new HttpRequestMessage();
            _fixture.Request.Headers.Authorization = new AuthenticationHeaderValue(_authType, _authToken);
            _fixture.RequestContext = new HttpRequestContext();
        }

        [Test]
        public void ShouldRegisterNewUser()
        {
            var user = new User();
            _accountService.Setup(mocked => mocked.RegisterPerson(user, null)).Returns(user);

            var response = _fixture.Post(user, null);
            _accountService.VerifyAll();
            _mockAnalyticService.Verify(x => x.Track(It.IsAny<string>(), "SignedUp"), Times.Once);

            
            Assert.IsNotNull(response);
            Assert.IsInstanceOf<OkNegotiatedContentResult<User>>(response);
            var responseData = ((OkNegotiatedContentResult<User>) response).Content;
            Assert.IsNotNull(responseData);
            Assert.AreSame(user, responseData);
        }

        [Test]
        public void ShouldReturnBadResponseForDuplicateUser()
        {
            var user = new User();
            _accountService.Setup(mocked => mocked.RegisterPerson(user, null)).Throws(new DuplicateUserException("me@here.com"));

            try
            {
                _fixture.Post(user, null);
                Assert.Fail("Expected exception was not thrown");
            }
            catch (HttpResponseException e)
            {
                // Expected
            }
            _accountService.VerifyAll();
        }

        [Test]
        public void ShouldGetUserByName()
        {
            var username = "test";
            MpUser mpUser = new MpUser()
            {
                CanImpersonate = true,
                DisplayName = "Testy McTestface",
                Guid = "123123123123123",
                UserEmail = "mctestface@test.com",
                UserId = "test",
                UserRecordId = 1
            };
            MpMyContact mpMyContact = new MpMyContact()
            {
                Contact_ID = 2,
                First_Name = "Testy",
                Email_Address = mpUser.UserEmail,
                Age = 30,
                Mobile_Phone = "1234567890"
            };
            List<MpRoleDto> roles = new List<MpRoleDto>();
            LoginReturn dto = new LoginReturn()
            {
                userToken = _authType + " " + _authToken,
                userTokenExp = "",
                refreshToken = "",
                userId = mpMyContact.Contact_ID,
                username = mpMyContact.First_Name,
                userEmail = mpMyContact.Email_Address,
                roles = roles,
                age = mpMyContact.Age,
                userPhone = mpMyContact.Mobile_Phone,
                canImpersonate = mpUser.CanImpersonate
            };
            

            _userRepository.Setup(mocked => mocked.GetUserIdByUsername(username)).Returns(mpUser.UserRecordId);
            _userRepository.Setup(mocked => mocked.GetUserByRecordId(mpUser.UserRecordId)).Returns(mpUser);
            _userRepository.Setup(mocked => mocked.GetUserRoles(mpUser.UserRecordId)).Returns(roles);
            _contactRepository.Setup(mocked => mocked.GetContactByUserRecordId(mpUser.UserRecordId,null)).Returns(mpMyContact);

            var response = _fixture.Get(username);
              
            _userRepository.VerifyAll();
            _contactRepository.VerifyAll();
            Assert.IsNotNull(response);
            Assert.IsInstanceOf<OkNegotiatedContentResult<LoginReturn>>(response);
            var r = (OkNegotiatedContentResult<LoginReturn>)response;
            Assert.IsNotNull(r.Content);
            Assert.AreEqual(JsonConvert.SerializeObject(dto), JsonConvert.SerializeObject(r.Content));
        }
    }
}

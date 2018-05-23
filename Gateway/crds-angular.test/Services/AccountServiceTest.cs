using System.Collections.Generic;
using crds_angular.Exceptions;
using crds_angular.Models.Crossroads;
using crds_angular.Services;
using crds_angular.Services.Interfaces;
using Crossroads.Utilities.Interfaces;
using Crossroads.Web.Common;
using Crossroads.Web.Common.Configuration;
using Crossroads.Web.Common.MinistryPlatform;
using Crossroads.Web.Common.Security;
using MinistryPlatform.Translation.Repositories.Interfaces;
using Moq;
using NUnit.Framework;

namespace crds_angular.test.Services
{
    public class AccountServiceTests
    {
        private Mock<IAuthenticationRepository> _authenticationService;
        private Mock<IConfigurationWrapper> _configurationWrapper;
        private Mock<ILookupRepository> _lookupService;
        private Mock<ICommunicationRepository> _comunicationService;
        private Mock<ISubscriptionsService> _subscriptionService;
        private Mock<IMinistryPlatformService> _ministryPlatformService;
        private Mock<IApiUserRepository> _apiUserService;
        private Mock<IParticipantRepository> _participantService;
        private Mock<IContactRepository> _contactRepository;

        private AccountService _fixture;

        [SetUp]
        public void SetUp()
        {
            _authenticationService = new Mock<IAuthenticationRepository>();
            _configurationWrapper = new Mock<IConfigurationWrapper>();
            _lookupService = new Mock<ILookupRepository>();
            _comunicationService = new Mock<ICommunicationRepository>();
            _subscriptionService = new Mock<ISubscriptionsService>();
            _ministryPlatformService = new Mock<IMinistryPlatformService>();
            _apiUserService = new Mock<IApiUserRepository>();
            _participantService = new Mock<IParticipantRepository>();
            _contactRepository = new Mock<IContactRepository>();

            _fixture = new AccountService(_configurationWrapper.Object,
                                          _comunicationService.Object,
                                          _authenticationService.Object,
                                          _subscriptionService.Object,
                                          _ministryPlatformService.Object,
                                          _lookupService.Object,
                                          _apiUserService.Object,
                                          _participantService.Object,
                                          _contactRepository.Object);
        }

        [Test]
        [ExpectedException(typeof(DuplicateUserException))]
        public void ShouldNotRegisterDuplicatePerson()
        {
            var newUserData = new User
            {
                firstName = "Automated",
                lastName = "Test",
                email = "auto02@crossroads.net",
                password = "password"
            };

            _configurationWrapper.Setup(mocked => mocked.GetEnvironmentVarAsString("API_USER")).Returns("user");
            _configurationWrapper.Setup(mocked => mocked.GetEnvironmentVarAsString("API_PASSWORD")).Returns("password");
            _apiUserService.Setup(mocked => mocked.GetDefaultApiClientToken()).Returns("1234567890");
            
            _lookupService.Setup(mocked => mocked.EmailSearch(newUserData.email, "1234567890")).Returns(new Dictionary<string, object> { {"dp_RecordID", 123}});

            _fixture.RegisterPerson(newUserData, null);

            _ministryPlatformService.VerifyAll();
        }

        [Test]
        public void ShouldRegisterPerson()
        {
            var newUserData = new User
            {
                firstName = "Automated",
                lastName = "Test",
                email = "auto02@crossroads.net",
                password = "password"
            };

            _configurationWrapper.Setup(mocked => mocked.GetEnvironmentVarAsString("API_USER")).Returns("user");
            _configurationWrapper.Setup(mocked => mocked.GetEnvironmentVarAsString("API_PASSWORD")).Returns("password");
            
            _apiUserService.Setup(mocked => mocked.GetDefaultApiClientToken()).Returns("1234567890");
            _lookupService.Setup(mocked => mocked.EmailSearch(newUserData.email, "1234567890")).Returns(new Dictionary<string, object>());

            _configurationWrapper.Setup(mocked => mocked.GetConfigIntValue("Households")).Returns(123);
            _ministryPlatformService.Setup(mocked => mocked.CreateRecord(123, It.IsAny<Dictionary<string, object>>(), "1234567890", false)).Returns(321);

            _configurationWrapper.Setup(mocked => mocked.GetConfigIntValue("Contacts")).Returns(456);
            _ministryPlatformService.Setup(mocked => mocked.CreateRecord(456, It.IsAny<Dictionary<string, object>>(), "1234567890", false)).Returns(654);

            _configurationWrapper.Setup(mocked => mocked.GetConfigIntValue("Users")).Returns(789);
            _ministryPlatformService.Setup(mocked => mocked.CreateRecord(789, It.IsAny<Dictionary<string, object>>(), "1234567890", true)).Returns(987);

            _configurationWrapper.Setup(mocked => mocked.GetConfigIntValue("Users_Roles")).Returns(345);
            _ministryPlatformService.Setup(mocked => mocked.CreateSubRecord(345, 987, It.IsAny<Dictionary<string, object>>(), "1234567890", false)).Returns(543);

            _participantService.Setup(m => m.CreateParticipantRecord(654)).Returns(567);

            _subscriptionService.Setup(mocked => mocked.SetSubscriptions(It.IsAny<Dictionary<string, object>>(), 654, "1234567890")).Returns(999);

            _fixture.RegisterPerson(newUserData, null);

            _ministryPlatformService.VerifyAll();
            _participantService.VerifyAll();
            _subscriptionService.VerifyAll();
        }

        [Test]
        public void ShouldRegisterWithDefaultHouseholdSource()
        {
            var newUserData = new User
            {
                firstName = "Automated",
                lastName = "Test",
                email = "auto02@crossroads.net",
                password = "password"
            };

            var expectedHouseholdDict = new Dictionary<string, object>
            {
                {"Household_Name", newUserData.lastName},
                {"Congregation_ID", 321},
                {"Household_Source_ID", 654}
            };

            _configurationWrapper.Setup(mocked => mocked.GetEnvironmentVarAsString("API_USER")).Returns("user");
            _configurationWrapper.Setup(mocked => mocked.GetEnvironmentVarAsString("API_PASSWORD")).Returns("password");

            _apiUserService.Setup(mocked => mocked.GetDefaultApiClientToken()).Returns("1234567890");
            _lookupService.Setup(mocked => mocked.EmailSearch(newUserData.email, "1234567890")).Returns(new Dictionary<string, object>());

            _configurationWrapper.Setup(mocked => mocked.GetConfigIntValue("Congregation_Default_ID")).Returns(321);
            _configurationWrapper.Setup(mocked => mocked.GetConfigIntValue("Household_Default_Source_ID")).Returns(654);
            _configurationWrapper.Setup(mocked => mocked.GetConfigIntValue("Households")).Returns(123);
            _ministryPlatformService.Setup(mocked => mocked.CreateRecord(123, expectedHouseholdDict, "1234567890", false)).Returns(321);

            _configurationWrapper.Setup(mocked => mocked.GetConfigIntValue("Contacts")).Returns(456);
            _ministryPlatformService.Setup(mocked => mocked.CreateRecord(456, It.IsAny<Dictionary<string, object>>(), "1234567890", false)).Returns(654);

            _configurationWrapper.Setup(mocked => mocked.GetConfigIntValue("Users")).Returns(789);
            _ministryPlatformService.Setup(mocked => mocked.CreateRecord(789, It.IsAny<Dictionary<string, object>>(), "1234567890", true)).Returns(987);

            _configurationWrapper.Setup(mocked => mocked.GetConfigIntValue("Users_Roles")).Returns(345);
            _ministryPlatformService.Setup(mocked => mocked.CreateSubRecord(345, 987, It.IsAny<Dictionary<string, object>>(), "1234567890", false)).Returns(543);

            _fixture.RegisterPerson(newUserData, null);

            _ministryPlatformService.VerifyAll();
            _participantService.VerifyAll();
            _subscriptionService.VerifyAll();
        }

        [Test]
        public void ShouldRegisterWithSpecificHouseholdSource()
        {
            var newUserData = new User
            {
                firstName = "Automated",
                lastName = "Test",
                email = "auto02@crossroads.net",
                password = "password"
            };

            var expectedHouseholdDict = new Dictionary<string, object>
            {
                {"Household_Name", newUserData.lastName},
                {"Congregation_ID", 321},
                {"Household_Source_ID", 51}
            };

            _configurationWrapper.Setup(mocked => mocked.GetEnvironmentVarAsString("API_USER")).Returns("user");
            _configurationWrapper.Setup(mocked => mocked.GetEnvironmentVarAsString("API_PASSWORD")).Returns("password");

            _apiUserService.Setup(mocked => mocked.GetDefaultApiClientToken()).Returns("1234567890");
            _lookupService.Setup(mocked => mocked.EmailSearch(newUserData.email, "1234567890")).Returns(new Dictionary<string, object>());

            _configurationWrapper.Setup(mocked => mocked.GetConfigIntValue("Congregation_Default_ID")).Returns(321);
            _configurationWrapper.Setup(mocked => mocked.GetConfigIntValue("Household_Default_Source_ID")).Callback(() =>
                                                                                                                    {
                                                                                                                        Assert.Fail();
                                                                                                                    });
            _configurationWrapper.Setup(mocked => mocked.GetConfigIntValue("Households")).Returns(123);
            _ministryPlatformService.Setup(mocked => mocked.CreateRecord(123, It.IsAny<Dictionary<string, object>>(), "1234567890", false)).Returns(321);

            _configurationWrapper.Setup(mocked => mocked.GetConfigIntValue("Contacts")).Returns(456);
            _ministryPlatformService.Setup(mocked => mocked.CreateRecord(456, It.IsAny<Dictionary<string, object>>(), "1234567890", false)).Returns(654);

            _configurationWrapper.Setup(mocked => mocked.GetConfigIntValue("Users")).Returns(789);
            _ministryPlatformService.Setup(mocked => mocked.CreateRecord(789, It.IsAny<Dictionary<string, object>>(), "1234567890", true)).Returns(987);

            _configurationWrapper.Setup(mocked => mocked.GetConfigIntValue("Users_Roles")).Returns(345);
            _ministryPlatformService.Setup(mocked => mocked.CreateSubRecord(345, 987, It.IsAny<Dictionary<string, object>>(), "1234567890", false)).Returns(543);

            _fixture.RegisterPerson(newUserData, 51);

            _ministryPlatformService.VerifyAll();
            _participantService.VerifyAll();
            _subscriptionService.VerifyAll();
        }
    }
}
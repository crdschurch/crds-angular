using Crossroads.Web.Common.Configuration;
using Crossroads.Web.Common.MinistryPlatform;
using Crossroads.Web.Common.Security;
using MinistryPlatform.Translation.Repositories;
using MinistryPlatform.Translation.Repositories.Interfaces;
using Moq;
using NUnit.Framework;
using System;
using System.Collections.Generic;

namespace MinistryPlatform.Translation.Test.Services
{
    [TestFixture]
    public class ResponseServiceTest
    {
        [SetUp]
        public void SetUp()
        {
            _ministryPlatformService = new Mock<IMinistryPlatformService>();
            _authService = new Mock<IAuthenticationRepository>();
            _configWrapper = new Mock<IConfigurationWrapper>();
            _apiUserService = new Mock<IApiUserRepository>();

            _configWrapper.Setup(mocked => mocked.GetConfigIntValue("SignupToServeReminders")).Returns(2203);
            Environment.SetEnvironmentVariable("API_USER", "uid");
            Environment.SetEnvironmentVariable("API_PASSWORD", "pwd");
            _authService.Setup(m => m.AuthenticateClient(It.IsAny<string>(), It.IsAny<string>())).Returns(new AuthToken
            {
                AccessToken = "ABC",
                ExpiresIn = 123
            });

            _fixture = new ResponseRepository(_authService.Object, _configWrapper.Object, _ministryPlatformService.Object, _apiUserService.Object);
        }

        private ResponseRepository _fixture;
        private Mock<IMinistryPlatformService> _ministryPlatformService;
        private Mock<IAuthenticationRepository> _authService;
        private Mock<IConfigurationWrapper> _configWrapper;
        private Mock<IApiUserRepository> _apiUserService;

        private readonly int EventParticipantPageId = 281;
        private readonly int EventParticipantStatusDefaultID = 2;
        private readonly int EventsPageId = 308;
        private readonly string EventsWithEventTypeId = "EventsWithEventTypeId";

        [Test]
        public void TestGoodGetServeReminders()
        {
            // Arrange
            List<Dictionary<string, object>> responseValues = new List<Dictionary<string, object>>();

            var response = new Dictionary<string, object>
            {
                {"Event_Title", "event-title-100"},
                {"Event_Type", "event-type-100"},
                {"Event_Start_Date", new DateTime(2015, 3, 28, 8, 30, 0)},
                {"Event_End_Date", new DateTime(2015, 3, 28, 8, 30, 0)},
                {"Opportunity_Contact_ID", 1234567},
                {"Opportunity_Contact_Email_Address", "opportunity_test@test.com"},
                {"Opportunity_Title", "test opportunity title"},
                {"Shift_End", new TimeSpan(3, 0, 0)},
                {"Shift_Start", new TimeSpan(2, 0, 0)},
                {"Contact_ID", 2345678},
                {"Email_Address", "test@test.com"},
                {"Communication_ID", 123}
            };

            responseValues.Add(response);
            _ministryPlatformService.Setup(m => m.GetPageViewRecords(2203, It.IsAny<string>(), string.Empty, string.Empty, 0)).Returns(responseValues);

            //Act
            var reminders = _fixture.GetServeReminders("test token");

            ////Assert
            _ministryPlatformService.VerifyAll();
            Assert.AreEqual(reminders[0].Event_Start_Date, new DateTime(2015, 3, 28, 8, 30, 0));
            Assert.AreEqual(reminders[0].Event_End_Date, new DateTime(2015, 3, 28, 8, 30, 0));
            Assert.AreEqual(reminders[0].Event_Title, "event-title-100");
            Assert.AreEqual(reminders[0].Opportunity_Contact_Id, 1234567);
            Assert.AreEqual(reminders[0].Opportunity_Email_Address, "opportunity_test@test.com");
            Assert.AreEqual(reminders[0].Opportunity_Title, "test opportunity title");
            Assert.AreEqual(reminders[0].Shift_End, new TimeSpan(3, 0, 0));
            Assert.AreEqual(reminders[0].Shift_Start, new TimeSpan(2, 0, 0));
            Assert.AreEqual(reminders[0].Signedup_Contact_Id, 2345678);
            Assert.AreEqual(reminders[0].Signedup_Email_Address, "test@test.com");
        }

        [Test]
        public void TestNullGetServeReminders()
        {
            // Arrange
            List<Dictionary<string, object>> responseValues = new List<Dictionary<string, object>>();

            var response = new Dictionary<string, object>
            {
                {"Event_Title", "event-title-100"},
                {"Event_Type", "event-type-100"},
                {"Event_Start_Date", new DateTime(2015, 3, 28, 8, 30, 0)},
                {"Event_End_Date", new DateTime(2015, 3, 28, 8, 30, 0)},
                {"Opportunity_Contact_ID", 1234567},
                {"Opportunity_Contact_Email_Address", "opportunity_test@test.com"},
                {"Opportunity_Title", "test opportunity title"},
                {"Shift_End", null},
                {"Shift_Start", null},
                {"Contact_ID", 2345678},
                {"Email_Address", "test@test.com"},
                {"Communication_ID", 123}
            };

            responseValues.Add(response);
            _ministryPlatformService.Setup(m => m.GetPageViewRecords(2203, It.IsAny<string>(), string.Empty, string.Empty, 0)).Returns(responseValues);

            //Act
            var reminders = _fixture.GetServeReminders("test token");

            ////Assert
            _ministryPlatformService.VerifyAll();
            Assert.AreEqual(reminders[0].Event_Start_Date, new DateTime(2015, 3, 28, 8, 30, 0));
            Assert.AreEqual(reminders[0].Event_End_Date, new DateTime(2015, 3, 28, 8, 30, 0));
            Assert.AreEqual(reminders[0].Event_Title, "event-title-100");
            Assert.AreEqual(reminders[0].Opportunity_Contact_Id, 1234567);
            Assert.AreEqual(reminders[0].Opportunity_Email_Address, "opportunity_test@test.com");
            Assert.AreEqual(reminders[0].Opportunity_Title, "test opportunity title");
            Assert.AreEqual(reminders[0].Shift_End, null);
            Assert.AreEqual(reminders[0].Shift_Start, null);
            Assert.AreEqual(reminders[0].Signedup_Contact_Id, 2345678);
            Assert.AreEqual(reminders[0].Signedup_Email_Address, "test@test.com");
        }
    }
}
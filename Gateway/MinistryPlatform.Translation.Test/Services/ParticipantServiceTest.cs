using System;
using System.Collections.Generic;
using Crossroads.Utilities.Interfaces;
using Crossroads.Web.Common;
using Crossroads.Web.Common.Configuration;
using Crossroads.Web.Common.MinistryPlatform;
using Crossroads.Web.Common.Security;
using MinistryPlatform.Translation.Models;
using MinistryPlatform.Translation.Repositories;
using MinistryPlatform.Translation.Repositories.Interfaces;
using Moq;
using NUnit.Framework;

namespace MinistryPlatform.Translation.Test.Services
{
    [TestFixture]
    public class ParticipantServiceTest
    {
        private ParticipantRepository _fixture;
        private Mock<IMinistryPlatformService> _mpServiceMock;
        private Mock<IAuthenticationRepository> _authService;
        private Mock<IConfigurationWrapper> _configWrapper;
        private Mock<IMinistryPlatformRestRepository> _ministryPlatformRestMock;
		private Mock<IApiUserRepository> _apiUserRepositoryMock;

        [SetUp]
        public void SetUp()
        {
            _mpServiceMock = new Mock<IMinistryPlatformService>();
            _authService = new Mock<IAuthenticationRepository>();
            _configWrapper = new Mock<IConfigurationWrapper>();
            _ministryPlatformRestMock = new Mock<IMinistryPlatformRestRepository>();
			_apiUserRepositoryMock = new Mock<IApiUserRepository>();
            _configWrapper.Setup(m => m.GetEnvironmentVarAsString("API_USER")).Returns("uid");
            _configWrapper.Setup(m => m.GetEnvironmentVarAsString("API_PASSWORD")).Returns("pwd");
            _configWrapper.Setup(m => m.GetConfigIntValue("Participants")).Returns(355);
            _authService.Setup(m => m.AuthenticateClient(It.IsAny<string>(), It.IsAny<string>())).Returns(new AuthToken
            {
                AccessToken = "ABC",
                ExpiresIn = 123
            });

            _fixture = new ParticipantRepository(_mpServiceMock.Object, _ministryPlatformRestMock.Object, _authService.Object, _configWrapper.Object, _apiUserRepositoryMock.Object);
        }

        [Test]
        public void GetParticipantByParticipantId()
        {
            const int contactId = 99999;
            var searchString = $"Contact_ID_Table.[Contact_ID]={contactId}";
            var backToTheFuture = new DateTime(2015, 10, 21);
            var mockParticpant = new MpParticipant
            {
                ContactId = contactId,
                Age = 99,
                ParticipantId = 100,
                EmailAddress = "email-address",
                Nickname = "nick-name",
                DisplayName = "display-name",
                ApprovedSmallGroupLeader = true,
                AttendanceStart = backToTheFuture,
                GroupLeaderStatus = 4
            };

            _ministryPlatformRestMock.Setup(m => m.UsingAuthenticationToken(It.IsAny<string>())).Returns(_ministryPlatformRestMock.Object);
            _ministryPlatformRestMock.Setup(m => m.Search<MpParticipant>(searchString, It.IsAny<List<string>>(), null, false)).Returns(new List<MpParticipant> { mockParticpant });

            var participant = _fixture.GetParticipant(contactId);

            Assert.IsNotNull(participant);
            Assert.AreEqual("nick-name", participant.PreferredName);
            Assert.AreEqual("email-address", participant.EmailAddress);
            Assert.AreEqual(100, participant.ParticipantId);
            Assert.IsTrue(participant.ApprovedSmallGroupLeader);
            Assert.AreEqual(backToTheFuture, participant.AttendanceStart);
        }

        [Test]
        public void TestGetParticipantRecord()
        {
            const string token = "tok123";

            const string viewKey = "MyParticipantRecords";
            var mockDictionaryList = new List<Dictionary<string, object>>
            {
                new Dictionary<string, object>
                {
                    {"Contact_ID", 99999},
                    {"dp_RecordID", 100},
                    {"Email_Address", "email-address"},
                    {"Nickname", "nick-name"},
                    {"Display_Name", "display-name"},
                    {"Age", 99},
                    {"Group_Leader_Status_ID", 4 }
                }
            };

            _apiUserRepositoryMock.Setup(m => m.GetDefaultApiClientToken()).Returns(token);
            _configWrapper.Setup(m => m.GetConfigIntValue("GroupLeaderApproved")).Returns(4);
            _mpServiceMock.Setup(m => m.GetRecordsDict(viewKey, token, string.Empty, string.Empty)).Returns(mockDictionaryList);

            var participant = _fixture.GetParticipantRecord(token);

            _mpServiceMock.VerifyAll();

            Assert.IsNotNull(participant);
            Assert.AreEqual("nick-name", participant.PreferredName);
            Assert.AreEqual("email-address", participant.EmailAddress);
            Assert.AreEqual(100, participant.ParticipantId);
            Assert.IsTrue(participant.ApprovedSmallGroupLeader);
        }

        [Test]

        public void TestCreateParticipantRecord()
        {
            const int contactId = 9999;
            var date = DateTime.Now; 

            var mockDictionary = new Dictionary<string, object>
            {
                    {"Participant_Type_Default_ID", 2},
                    {"Participant_Start_Date", date},
                    {"Contact ID", 99999},
            };

            _mpServiceMock.Setup(
                mocked => mocked.CreateRecord(355, It.IsAny<Dictionary<string,object>>(), "ABC", false))
                .Returns(123);

            var participant = _fixture.CreateParticipantRecord(contactId);
            Assert.AreEqual(123, participant);           
        }

    }

}
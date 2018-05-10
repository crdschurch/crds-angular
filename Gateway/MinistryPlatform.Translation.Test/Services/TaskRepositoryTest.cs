using System;
using System.Collections.Generic;
using Crossroads.Utilities.Interfaces;
using Crossroads.Web.Common;
using Crossroads.Web.Common.Configuration;
using Crossroads.Web.Common.MinistryPlatform;
using Crossroads.Web.Common.Security;
using MinistryPlatform.Translation.Repositories;
using MinistryPlatform.Translation.Repositories.Interfaces;
using Moq;
using NUnit.Framework;

namespace MinistryPlatform.Translation.Test
{
    [TestFixture]
    public class TaskRepositoryTest
    {
        private TaskRepository _fixture;

        private Mock<IMinistryPlatformService> _ministryPlatformService;
        private Mock<IAuthenticationRepository> _authService;
        private Mock<IConfigurationWrapper> _configWrapper;
        private Mock<IMinistryPlatformRestRepository> _ministryPlatformRestRepository;
        private Mock<IApiUserRepository> _apiUserRepository;
        private readonly int _roomReservationPageID;

        [SetUp]
        public void SetUp()
        {
            _ministryPlatformService = new Mock<IMinistryPlatformService>(MockBehavior.Strict);
            _authService = new Mock<IAuthenticationRepository>(MockBehavior.Strict);
            _configWrapper = new Mock<IConfigurationWrapper>(MockBehavior.Strict);
            _ministryPlatformRestRepository = new Mock<IMinistryPlatformRestRepository>(MockBehavior.Strict);
            _apiUserRepository = new Mock<IApiUserRepository>();

            _configWrapper.Setup(mocked => mocked.GetConfigIntValue("TasksNeedingAutoStarted")).Returns(2212);
            _configWrapper.Setup(mocked => mocked.GetConfigIntValue("TasksPageId")).Returns(400);
            _configWrapper.Setup(mocked => mocked.GetConfigIntValue("RoomReservationPageId")).Returns(384);
            _configWrapper.Setup(mocked => mocked.GetEnvironmentVarAsString("API_USER")).Returns("api_user");
            _configWrapper.Setup(mocked => mocked.GetEnvironmentVarAsString("API_PASSWORD")).Returns("password");

            _authService.Setup(m => m.AuthenticateUser(It.IsAny<string>(), It.IsAny<string>())).Returns(new AuthToken
            {
                AccessToken = "ABC",
                ExpiresIn = 123
            });

            _fixture = new TaskRepository(_authService.Object, _configWrapper.Object, _ministryPlatformService.Object, _ministryPlatformRestRepository.Object, _apiUserRepository.Object);
        }

        [Test]
        public void ShouldGetMPTaskList()
        {
            // Arrange
            List<Dictionary<string, object>> responseValues = new List<Dictionary<string, object>>();

            var response = new Dictionary<string, object>
            {
                {"Task ID", 1234567},
                {"Title", "Task Title"},
                {"Author User ID", 2345678},
                {"Assigned User ID", 3456789},
                {"Start Date", new DateTime(2016, 3, 28, 8, 30, 0)},
                {"End Date", new DateTime(2016, 3, 28, 8, 30, 0)},
                {"Completed", false},
                {"Description", "Task Description"},
                {"Record ID", 1234567},
                {"Page ID", 302 },
                {"Process Submission ID", 3456789},
                {"Process Step ID", 4567890},
                {"Rejected", false},
                {"Escalated", false},
                {"Record Description", "Task Record Description"}
            };

            responseValues.Add(response);
            _ministryPlatformService.Setup(m => m.GetPageViewRecords(2212, It.IsAny<string>(), string.Empty, string.Empty, 0)).Returns(responseValues);

            // Act
            var reminders = _fixture.GetTasksToAutostart();

            ////Assert
            _ministryPlatformService.VerifyAll();
            Assert.AreEqual(reminders[0].Task_ID, 1234567);
            Assert.AreEqual(reminders[0].Title, "Task Title");
            Assert.AreEqual(reminders[0].Author_User_ID, 2345678);
            Assert.AreEqual(reminders[0].Assigned_User_ID, 3456789);
            Assert.AreEqual(reminders[0].StartDate, new DateTime(2016, 3, 28, 8, 30, 0));
            Assert.AreEqual(reminders[0].EndDate, new DateTime(2016, 3, 28, 8, 30, 0));
            Assert.AreEqual(reminders[0].Description, "Task Description");
            Assert.AreEqual(reminders[0].Record_ID, 1234567);
            Assert.AreEqual(reminders[0].Page_ID, 302);
            Assert.AreEqual(reminders[0].Process_Submission_ID, 3456789);
            Assert.AreEqual(reminders[0].Process_Step_ID, 4567890);
            Assert.AreEqual(reminders[0].Rejected, false);
            Assert.AreEqual(reminders[0].Escalated, false);
            Assert.AreEqual(reminders[0].Record_Description, "Task Record Description");
        }

        [Test]
        public void ShouldCompleteTask()
        {
            // Arrange
            var token = "12b3c4d5e6f7g8h";
            var taskId = 1234567;
            var rejected = false;
            var comments = "comments";

            _ministryPlatformService.Setup(m => m.CompleteTask(It.IsAny<string>(), It.IsAny<int>(), It.IsAny<bool>(), It.IsAny<string>()));

            // Act
            _fixture.CompleteTask(token, taskId, rejected, comments);

            // Assert
            _ministryPlatformService.VerifyAll();
        }

        [Test]
        public void ShouldHandleNullTaskListFields()
        {
            // Arrange
            List<Dictionary<string, object>> responseValues = new List<Dictionary<string, object>>();

            var response = new Dictionary<string, object>
            {
                {"Task ID", 1234567},
                {"Title", null},
                {"Author User ID", 2345678},
                {"Assigned User ID", 3456789},
                {"Start Date", null},
                {"End Date", null},
                {"Completed", false},
                {"Description", null},
                {"Record ID", 1234567},
                {"Page ID", 302 },
                {"Process Submission ID", 3456789},
                {"Process Step ID", 4567890},
                {"Rejected", false},
                {"Escalated", false},
                {"Record Description", "Task Record Description"}
            };

            responseValues.Add(response);
            _ministryPlatformService.Setup(m => m.GetPageViewRecords(2212, It.IsAny<string>(), string.Empty, string.Empty, 0)).Returns(responseValues);

            // Act
            var reminders = _fixture.GetTasksToAutostart();

            ////Assert
            _ministryPlatformService.VerifyAll();
            Assert.AreEqual(reminders[0].Task_ID, 1234567);
            Assert.AreEqual(reminders[0].Title, null);
            Assert.AreEqual(reminders[0].Author_User_ID, 2345678);
            Assert.AreEqual(reminders[0].Assigned_User_ID, 3456789);
            Assert.AreEqual(reminders[0].StartDate, null);
            Assert.AreEqual(reminders[0].EndDate, null);
            Assert.AreEqual(reminders[0].Description, null);
            Assert.AreEqual(reminders[0].Record_ID, 1234567);
            Assert.AreEqual(reminders[0].Page_ID, 302);
            Assert.AreEqual(reminders[0].Process_Submission_ID, 3456789);
            Assert.AreEqual(reminders[0].Process_Step_ID, 4567890);
            Assert.AreEqual(reminders[0].Rejected, false);
            Assert.AreEqual(reminders[0].Escalated, false);
            Assert.AreEqual(reminders[0].Record_Description, "Task Record Description");
        }

    }
}

﻿using System;
using System.Collections.Generic;
using System.Reactive.Linq;
using System.Threading;
using crds_angular.Models.Crossroads.GroupLeader;
using crds_angular.Models.Crossroads.Profile;
using crds_angular.Services;
using crds_angular.Services.Interfaces;
using Crossroads.Web.Common.Configuration;
using MinistryPlatform.Translation.Models;
using MinistryPlatform.Translation.Repositories.Interfaces;
using Moq;
using NUnit.Framework;

namespace crds_angular.test.Services
{
    [TestFixture]
    public class GroupLeaderServiceTest
    {
        private Mock<IUserRepository> _userRepo;
        private Mock<IPersonService> _personService;
        private Mock<IFormSubmissionRepository> _formService;
        private Mock<IParticipantRepository> _participantRepository;
        private Mock<IConfigurationWrapper> _configWrapper;
        private IGroupLeaderService _fixture;

        [SetUp]
        public void Setup()
        {
            _userRepo = new Mock<IUserRepository>();
            _personService = new Mock<IPersonService>();
            _formService = new Mock<IFormSubmissionRepository>();
            _participantRepository = new Mock<IParticipantRepository>();
            _configWrapper = new Mock<IConfigurationWrapper>();
            _fixture = new GroupLeaderService(_personService.Object, _userRepo.Object, _formService.Object, _participantRepository.Object, _configWrapper.Object);
        }

        [TearDown]
        public void Teardown()
        {
            _personService.VerifyAll();
            _participantRepository.VerifyAll();
            _userRepo.VerifyAll();
        }

        [Test]
        public void ShouldSaveProfileWithCorrectDisplayName()
        {
            var leaderDto = GroupLeaderMock();

            const string fakeToken = "letmein";
            const int fakeUserId = 98124;

            _userRepo.Setup(m => m.GetUserIdByUsername(leaderDto.OldEmail)).Returns(fakeUserId);
            _userRepo.Setup(m => m.UpdateUser(It.IsAny<Dictionary<string, object>>()));
            _personService.Setup(m => m.SetProfile(fakeToken, It.IsAny<Person>())).Callback((string token, Person person) =>
            {
                Assert.AreEqual(person.GetContact().Display_Name, $"{leaderDto.LastName}, {leaderDto.NickName}");
            });
            _fixture.SaveProfile(fakeToken, leaderDto).Wait();            
        }

        [Test]
        public void ShouldSaveProfileWithCorrectDisplayNameAndUserWithCorrectEmail()
        {
            var leaderDto = GroupLeaderMock();

            const string fakeToken = "letmein";
            const int fakeUserId = 98124;

            _userRepo.Setup(m => m.GetUserIdByUsername(leaderDto.OldEmail)).Returns(fakeUserId);
            _userRepo.Setup(m => m.UpdateUser(It.IsAny<Dictionary<string, object>>())).Callback((Dictionary<string, object> userData) =>
            {
                Thread.Sleep(5000);
                Assert.AreEqual(leaderDto.Email, userData["User_Name"]);
                Assert.AreEqual(leaderDto.Email, userData["User_Email"]);
            }); ;
            _personService.Setup(m => m.SetProfile(fakeToken, It.IsAny<Person>())).Callback((string token, Person person) =>
            {
                Assert.AreEqual(person.GetContact().Display_Name, $"{leaderDto.LastName}, {leaderDto.NickName}");
            });
            _fixture.SaveProfile(fakeToken, leaderDto).Wait();
        }

        [Test]
        public void ShouldUpdateUserWithCorrectEmail()
        {
            const string fakeToken = "letmein";
            const int fakeUserId = 98124;
            var leaderDto = GroupLeaderMock();
            _personService.Setup(m => m.SetProfile(fakeToken, It.IsAny<Person>()));
            _userRepo.Setup(m => m.GetUserIdByUsername(leaderDto.OldEmail)).Returns(fakeUserId);
            _userRepo.Setup(m => m.UpdateUser(It.IsAny<Dictionary<string, object>>())).Callback((Dictionary<string, object> userData) =>
            {
                Assert.AreEqual(leaderDto.Email, userData["User_Name"]);
                Assert.AreEqual(leaderDto.Email, userData["User_Email"]);
            });
            _fixture.SaveProfile(fakeToken, leaderDto).Wait();
        }

        [Test]
        public void ShouldRethrowExceptionWhenPersonServiceThrows()
        {
            const string fakeToken = "letmein";
            const int fakeUserId = 98124;
            var leaderDto = GroupLeaderMock();
            _personService.Setup(m => m.SetProfile(fakeToken, It.IsAny<Person>())).Throws(new Exception("no person to save"));
            _userRepo.Setup(m => m.GetUserIdByUsername(leaderDto.OldEmail)).Returns(fakeUserId);            

            Assert.Throws<Exception>(() =>
            {
                _fixture.SaveProfile(fakeToken, leaderDto).Wait();
            });
        }

        [Test]
        public void ShouldRethrowExceptionWhenUserServiceThrows()
        {
            const string fakeToken = "letmein";
            const int fakeUserId = 98124;
            var leaderDto = GroupLeaderMock();
            _personService.Setup(m => m.SetProfile(fakeToken, It.IsAny<Person>()));
            _userRepo.Setup(m => m.GetUserIdByUsername(leaderDto.OldEmail)).Returns(fakeUserId);
            _userRepo.Setup(m => m.UpdateUser(It.IsAny<Dictionary<string, object>>())).Throws(new Exception("no user to save"));

            Assert.Throws<Exception>(() =>
            {
                _fixture.SaveProfile(fakeToken, leaderDto).Wait();
            });
        }

        [Test]
        public void ShouldSaveReferenceData()
        {
            var fakeDto = GroupLeaderMock();

            const int groupLeaderFormConfig = 23;
            const int groupLeaderReference = 56;
            const int groupLeaderHuddle = 92;
            const int groupLeaderStudent = 126;

            _configWrapper.Setup(m => m.GetConfigIntValue("GroupLeaderFormId")).Returns(groupLeaderFormConfig);
            _configWrapper.Setup(m => m.GetConfigIntValue("GroupLeaderReferenceFieldId")).Returns(groupLeaderReference);
            _configWrapper.Setup(m => m.GetConfigIntValue("GroupLeaderHuddleFieldId")).Returns(groupLeaderHuddle);
            _configWrapper.Setup(m => m.GetConfigIntValue("GroupLeaderStudentFieldId")).Returns(groupLeaderStudent);

            _formService.Setup(m => m.SubmitFormResponse(It.IsAny<MpFormResponse>())).Returns((MpFormResponse form) =>
            {
                Assert.AreEqual(groupLeaderFormConfig, form.FormId);
                return 1;
            });
            var responseId = _fixture.SaveReferences(fakeDto).Wait();
            Assert.AreEqual(responseId, 1);
        }

        [Test]
        public void ShouldThrowExceptionWhenSaveReferenceDataFails()
        {
            var fakeDto = GroupLeaderMock();

            const int groupLeaderFormConfig = 23;
            const int groupLeaderReference = 56;
            const int groupLeaderHuddle = 92;
            const int groupLeaderStudent = 126;

            _configWrapper.Setup(m => m.GetConfigIntValue("GroupLeaderFormId")).Returns(groupLeaderFormConfig);
            _configWrapper.Setup(m => m.GetConfigIntValue("GroupLeaderReferenceFieldId")).Returns(groupLeaderReference);
            _configWrapper.Setup(m => m.GetConfigIntValue("GroupLeaderHuddleFieldId")).Returns(groupLeaderHuddle);
            _configWrapper.Setup(m => m.GetConfigIntValue("GroupLeaderStudentFieldId")).Returns(groupLeaderStudent);

            _formService.Setup(m => m.SubmitFormResponse(It.IsAny<MpFormResponse>())).Returns((MpFormResponse form) =>
            {
                Assert.AreEqual(groupLeaderFormConfig, form.FormId);
                return 0;
            });

            Assert.Throws<ApplicationException>(() => _fixture.SaveReferences(fakeDto).Wait());
        }

        [Test]
        public void ShouldSetStatusToInterested()
        {
            var fakeToken = "letmein";
            const int groupLeaderInterested = 2;
            var mockParticpant = ParticipantMock();

            _configWrapper.Setup(m => m.GetConfigIntValue("GroupLeaderInterested")).Returns(groupLeaderInterested);
            _participantRepository.Setup(m => m.GetParticipantRecord(fakeToken)).Returns(mockParticpant);
            mockParticpant.GroupLeaderStatus = groupLeaderInterested;
            _participantRepository.Setup(m => m.UpdateParticipant(mockParticpant));
            _fixture.SetInterested(fakeToken);
        }

        private static GroupLeaderProfileDTO GroupLeaderMock()
        {
            return new GroupLeaderProfileDTO()
            {
                ContactId = 12345,
                BirthDate = new DateTime(1980, 02, 21),
                Email = "silbermm@gmail.com",
                LastName = "Silbernagel",
                NickName = "Matt",
                Site = 1,            
                OldEmail = "matt.silbernagel@ingagepartners.com",
                HouseholdId = 81562,
                HuddleResponse = "No",
                LeadStudents = "Yes",
                ReferenceContactId = "89158"
            };
        }

        private static MpParticipant ParticipantMock()
        {
            return new MpParticipant
            {
                ContactId = 12345,
                ParticipantId = 67890,
                GroupLeaderStatus = 1,
                DisplayName = "Fakerson, Fakey"
            };
        }
    }
}

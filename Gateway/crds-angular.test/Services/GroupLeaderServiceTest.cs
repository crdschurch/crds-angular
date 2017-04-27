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
        private Mock<ICommunicationRepository> _communicationRepository;
        private Mock<IContactRepository> _contactMock;
        private IGroupLeaderService _fixture;

        [SetUp]
        public void Setup()
        {
            _userRepo = new Mock<IUserRepository>();
            _personService = new Mock<IPersonService>();
            _formService = new Mock<IFormSubmissionRepository>();
            _participantRepository = new Mock<IParticipantRepository>();
            _configWrapper = new Mock<IConfigurationWrapper>();
            _communicationRepository = new Mock<ICommunicationRepository>();
            _contactMock = new Mock<IContactRepository>();
            _fixture = new GroupLeaderService(_personService.Object, _userRepo.Object, _formService.Object, _participantRepository.Object, _configWrapper.Object, _communicationRepository.Object, _contactMock.Object);
        }

        [TearDown]
        public void Teardown()
        {
            _personService.VerifyAll();
            _participantRepository.VerifyAll();
            _userRepo.VerifyAll();
            _formService.VerifyAll();
            _contactMock.VerifyAll();
            _configWrapper.VerifyAll();
            _communicationRepository.VerifyAll();
            _contactMock.VerifyAll();
        }

        [Test]
        public void ShouldSaveProfileWithCorrectDisplayName()
        {
            var leaderDto = GroupLeaderMock();

            const string fakeToken = "letmein";
            const int fakeUserId = 98124;
            var fakePerson = PersonMock(leaderDto);

            _userRepo.Setup(m => m.GetUserIdByUsername(leaderDto.OldEmail)).Returns(fakeUserId);
            _userRepo.Setup(m => m.UpdateUser(It.IsAny<Dictionary<string, object>>()));
            _personService.Setup(m => m.GetLoggedInUserProfile(fakeToken)).Returns(fakePerson);
            _contactMock.Setup(m => m.UpdateContact(It.IsAny<int>(), It.IsAny<Dictionary<string, object>>())).Callback((int contactId, Dictionary<string, object> obj) =>
            {
                Assert.AreEqual(obj["Display_Name"], $"{leaderDto.LastName}, {leaderDto.NickName}");
            });
            _contactMock.Setup(m => m.UpdateHousehold(It.IsAny<MpHousehold>())).Returns(Observable.Start(() => new MpHousehold()));
            _fixture.SaveProfile(fakeToken, leaderDto).Wait();            
        }

        [Test]
        public void ShouldSaveProfileWithCorrectDisplayNameAndUserWithCorrectEmail()
        {
            var leaderDto = GroupLeaderMock();

            const string fakeToken = "letmein";
            const int fakeUserId = 98124;
            var fakePerson = PersonMock(leaderDto);

            _personService.Setup(m => m.GetLoggedInUserProfile(fakeToken)).Returns(fakePerson);
            _userRepo.Setup(m => m.GetUserIdByUsername(leaderDto.OldEmail)).Returns(fakeUserId);
            _userRepo.Setup(m => m.UpdateUser(It.IsAny<Dictionary<string, object>>())).Callback((Dictionary<string, object> userData) =>
            {
                Thread.Sleep(5000);
                Assert.AreEqual(leaderDto.Email, userData["User_Name"]);
                Assert.AreEqual(leaderDto.Email, userData["User_Email"]);
            });
            _contactMock.Setup(m => m.UpdateContact(It.IsAny<int>(), It.IsAny<Dictionary<string, object>>()));
            _contactMock.Setup(m => m.UpdateHousehold(It.IsAny<MpHousehold>())).Returns(Observable.Start(() => new MpHousehold()));           
            _fixture.SaveProfile(fakeToken, leaderDto).Wait();
        }

        [Test]
        public void ShouldUpdateUserWithCorrectEmail()
        {
            const string fakeToken = "letmein";
            const int fakeUserId = 98124;
            var leaderDto = GroupLeaderMock();
            var fakePerson = PersonMock(leaderDto);
            _personService.Setup(m => m.GetLoggedInUserProfile(fakeToken)).Returns(fakePerson);
            _userRepo.Setup(m => m.GetUserIdByUsername(leaderDto.OldEmail)).Returns(fakeUserId);
            _userRepo.Setup(m => m.UpdateUser(It.IsAny<Dictionary<string, object>>())).Callback((Dictionary<string, object> userData) =>
            {
                Assert.AreEqual(leaderDto.Email, userData["User_Name"]);
                Assert.AreEqual(leaderDto.Email, userData["User_Email"]);
            });
            _contactMock.Setup(m => m.UpdateContact(It.IsAny<int>(), It.IsAny<Dictionary<string, object>>()));
            _contactMock.Setup(m => m.UpdateHousehold(It.IsAny<MpHousehold>())).Returns(Observable.Start(() => new MpHousehold()));
            _fixture.SaveProfile(fakeToken, leaderDto).Wait();
        }

        [Test]
        public void ShouldRethrowExceptionWhenPersonServiceThrows()
        {
            const string fakeToken = "letmein";
            const int fakeUserId = 98124;
            var leaderDto = GroupLeaderMock();

            _personService.Setup(m => m.GetLoggedInUserProfile(fakeToken)).Throws(new Exception("no person to get"));            
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
            var fakePerson = PersonMock(leaderDto);
            _personService.Setup(m => m.GetLoggedInUserProfile(fakeToken)).Returns(fakePerson);
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
        public void ShouldThrowExceptionWhenSaveProfileFails()
        {
            const string fakeToken = "letmein";
            var leaderDto = new GroupLeaderProfileDTO();

            _userRepo.Setup(m => m.GetUserIdByUsername(It.IsAny<string>()));
            _personService.Setup(m => m.SetProfile(fakeToken, It.IsAny<Person>())).Throws<ApplicationException>();
            _userRepo.Setup(m => m.UpdateUser(It.IsAny<Dictionary<string, object>>()));

            Assert.Throws<ApplicationException>(() => _fixture.SaveProfile(fakeToken, leaderDto).Wait());
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

        [Test]
        public void ShouldSaveSpiritualGrowthAnswers()
        {
            const int fakeFormId = 5;
            const int fakeStoryFieldId = 1;
            const int fakeTaughtFieldId = 2;
            const int fakeResponseId = 10;
            const int fakeTemplateId = 12;
            
            var growthDto = SpiritualGrowthMock();

            _configWrapper.Setup(m => m.GetConfigIntValue("GroupLeaderFormId")).Returns(fakeFormId);
            _configWrapper.Setup(m => m.GetConfigIntValue("GroupLeaderFormStoryFieldId")).Returns(fakeStoryFieldId);
            _configWrapper.Setup(m => m.GetConfigIntValue("GroupLeaderFormTaughtFieldId")).Returns(fakeTaughtFieldId);
            _configWrapper.Setup(m => m.GetConfigIntValue("GroupLeaderConfirmationTemplate")).Returns(fakeTemplateId);

            _formService.Setup(m => m.SubmitFormResponse(It.IsAny<MpFormResponse>())).Returns((MpFormResponse form) =>
            {
                Assert.AreEqual(fakeFormId, form.FormId);
                return fakeResponseId;
            });

            _communicationRepository.Setup(m => m.GetTemplate(fakeTemplateId)).Returns(ConfirmationEmailMock());

            var responseId = _fixture.SaveSpiritualGrowth(growthDto).Wait();
            Assert.AreEqual(fakeResponseId, responseId);
        }

        [Test]
        public void ShouldThrowExceptionWhenSavingSpiritualGrowthFails()
        {
            const int fakeFormId = 5;
            const int fakeStoryFieldId = 1;
            const int fakeTaughtFieldId = 2;
            const int errorResponseId = 0;
            const int fakeTemplateId = 12;

            var growthDto = SpiritualGrowthMock();

            _configWrapper.Setup(m => m.GetConfigIntValue("GroupLeaderFormId")).Returns(fakeFormId);
            _configWrapper.Setup(m => m.GetConfigIntValue("GroupLeaderFormStoryFieldId")).Returns(fakeStoryFieldId);
            _configWrapper.Setup(m => m.GetConfigIntValue("GroupLeaderFormTaughtFieldId")).Returns(fakeTaughtFieldId);
            _configWrapper.Setup(m => m.GetConfigIntValue("GroupLeaderConfirmationTemplate")).Returns(fakeTemplateId);

            _formService.Setup(m => m.SubmitFormResponse(It.IsAny<MpFormResponse>())).Returns((MpFormResponse form) =>
            {
                Assert.AreEqual(fakeFormId, form.FormId);
                return errorResponseId;
            });

            _communicationRepository.Setup(m => m.GetTemplate(fakeTemplateId)).Returns(ConfirmationEmailMock());

            Assert.Throws<ApplicationException>(() => _fixture.SaveSpiritualGrowth(growthDto).Wait());
        }

        private MpMessageTemplate ConfirmationEmailMock()
        {
            return new MpMessageTemplate
            {
                FromContactId = 1234,
                FromEmailAddress = "donotreply@crossroads.net",
                ReplyToContactId = 1235,
                ReplyToEmailAddress = "seriouslydonotreply@crossroads.net",
                Subject = "This is a test email",
                Body = "Some testing content here."
            };
        }

        [Test]
        public void ShouldSetApplicantAsApplied()
        {
            const int groupLeaderAppliedId = 3;
            const string fakeToken = "letmein";
            var fakeParticipant = ParticipantMock();          
            _participantRepository.Setup(m => m.GetParticipantRecord(fakeToken)).Returns(fakeParticipant);
            _participantRepository.Setup(m => m.UpdateParticipant(It.IsAny<MpParticipant>())).Callback((MpParticipant p) =>
            {
                Assert.AreEqual(groupLeaderAppliedId, p.GroupLeaderStatus);
            });
            _configWrapper.Setup(m => m.GetConfigIntValue("GroupLeaderApplied")).Returns(groupLeaderAppliedId);

            var res = _fixture.SetApplied(fakeToken);

            res.Subscribe((n) =>
            {
                Assert.AreEqual(n, 1);
            },
            (err) =>
            {
                Assert.Fail();
            });
        }

        [Test]
        public void ShouldFailToSetApplicantAsAppliedIfUpdateFails()
        {
            const int groupLeaderAppliedId = 3;
            const string fakeToken = "letmein";
            var fakeParticipant = ParticipantMock();
            _participantRepository.Setup(m => m.GetParticipantRecord(fakeToken)).Returns(fakeParticipant);
            _participantRepository.Setup(m => m.UpdateParticipant(It.IsAny<MpParticipant>())).Callback((MpParticipant p) =>
            {
                Assert.AreEqual(groupLeaderAppliedId, p.GroupLeaderStatus);
            }).Throws<Exception>();
            _configWrapper.Setup(m => m.GetConfigIntValue("GroupLeaderApplied")).Returns(groupLeaderAppliedId);

            var res = _fixture.SetApplied(fakeToken);

            res.Subscribe((n) =>
            {
                Assert.Fail();
            },
            Assert.IsInstanceOf<Exception>);
        }

        [Test]
        public void ShouldFailToSetApplicantAsAppliedIfUpGetProfileFails()
        {           
            const string fakeToken = "letmein";            
            _participantRepository.Setup(m => m.GetParticipantRecord(fakeToken)).Throws<Exception>();                      
            var res = _fixture.SetApplied(fakeToken);
            res.Subscribe((n) =>
            {
                Assert.Fail();
            },
            Assert.IsInstanceOf<Exception>);
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
                FirstName = "Matty-boy",
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

        private static Person PersonMock(GroupLeaderProfileDTO leaderDto)
        {
            return new Person
            {
                FirstName = leaderDto.NickName,
                LastName = leaderDto.LastName,
                NickName = leaderDto.NickName,
                EmailAddress = leaderDto.Email
            };
        }

        private static SpiritualGrowthDTO SpiritualGrowthMock()
        {
            return new SpiritualGrowthDTO()
            {
                ContactId = 654321,
                EmailAddress = "hornerjn@gmail.com",
                Story = "my diary",
                Taught = "i lEarnDed hOw to ReAd"
            };
        }
    }
}

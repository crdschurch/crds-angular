using System;
using System.Collections.Generic;
using System.Reactive.Linq;
using System.Threading;
using crds_angular.Models.Crossroads.GroupLeader;
using crds_angular.Models.Crossroads.Profile;
using crds_angular.Services;
using crds_angular.Services.Analytics;
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
        private Mock<IAnalyticsService> _analyticsMock;
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
            _analyticsMock = new Mock<IAnalyticsService>();
            _fixture = new GroupLeaderService(_personService.Object, _userRepo.Object, _formService.Object, _participantRepository.Object, _configWrapper.Object, _communicationRepository.Object, _contactMock.Object, _analyticsMock.Object);
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
            _analyticsMock.VerifyAll();
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
            const int groupLeaderReferenceName = 57;
            const int groupLeaderHuddle = 92;
            const int groupLeaderStudent = 126;

            _configWrapper.Setup(m => m.GetConfigIntValue("GroupLeaderFormId")).Returns(groupLeaderFormConfig);
            _configWrapper.Setup(m => m.GetConfigIntValue("GroupLeaderReferenceFieldId")).Returns(groupLeaderReference);
            _configWrapper.Setup(m => m.GetConfigIntValue("GroupLeaderReferenceNameFieldId")).Returns(groupLeaderReferenceName);
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
        public void ShouldSaveNoReferenceData()
        {
            var fakeDto = NoReferenceProfileMock();

            const int groupLeaderFormConfig = 23;
            const int groupLeaderReference = 56;
            const int groupLeaderReferenceName = 57;
            const int groupLeaderHuddle = 92;
            const int groupLeaderStudent = 126;

            _configWrapper.Setup(m => m.GetConfigIntValue("GroupLeaderFormId")).Returns(groupLeaderFormConfig);
            _configWrapper.Setup(m => m.GetConfigIntValue("GroupLeaderReferenceFieldId")).Returns(groupLeaderReference);
            _configWrapper.Setup(m => m.GetConfigIntValue("GroupLeaderReferenceNameFieldId")).Returns(groupLeaderReferenceName);
            _configWrapper.Setup(m => m.GetConfigIntValue("GroupLeaderHuddleFieldId")).Returns(groupLeaderHuddle);
            _configWrapper.Setup(m => m.GetConfigIntValue("GroupLeaderStudentFieldId")).Returns(groupLeaderStudent);

            _formService.Setup(m => m.SubmitFormResponse(It.IsAny<MpFormResponse>()))
                .Returns((MpFormResponse form) =>
                {
                    // Make sure that referenceDisplayName is an empty string not null
                    Assert.AreEqual("", form.FormAnswers[1].Response);
                    return 1;
                });

            var responseId = _fixture.SaveReferences(fakeDto).Wait();
            Assert.AreEqual(1, responseId);
        }
	
	    [Test]
        public void ShouldThrowExceptionWhenSaveReferenceDataFails()
        {
            var fakeDto = GroupLeaderMock();

            const int groupLeaderFormConfig = 23;
            const int groupLeaderReference = 56;
            const int groupLeaderReferenceName = 57;
            const int groupLeaderHuddle = 92;
            const int groupLeaderStudent = 126;

            _configWrapper.Setup(m => m.GetConfigIntValue("GroupLeaderFormId")).Returns(groupLeaderFormConfig);
            _configWrapper.Setup(m => m.GetConfigIntValue("GroupLeaderReferenceFieldId")).Returns(groupLeaderReference);
            _configWrapper.Setup(m => m.GetConfigIntValue("GroupLeaderReferenceNameFieldId")).Returns(groupLeaderReferenceName);
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
            var fakePerson = PersonMock(leaderDto);
            _personService.Setup(m => m.GetLoggedInUserProfile(fakeToken)).Returns(fakePerson);
            _userRepo.Setup(m => m.GetUserIdByUsername(It.IsAny<string>()));
            _contactMock.Setup(m => m.UpdateContact(It.IsAny<int>(), It.IsAny<Dictionary<string, object>>())).Throws<ApplicationException>();            

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
            const int fakeOpenResponse1FieldId = 1;
            const int fakeOpenResponse2FieldId = 2;
            const int fakeOpenResponse3FieldId = 3;
            const int fakeOpenResponse4FieldId = 4;
            const int fakeResponseId = 10;
            const int fakeTemplateId = 12;
            
            var growthDto = SpiritualGrowthMock();

            _configWrapper.Setup(m => m.GetConfigIntValue("GroupLeaderFormId")).Returns(fakeFormId);
            _configWrapper.Setup(m => m.GetConfigIntValue("GroupLeaderFormOpenResponse1FieldId")).Returns(fakeOpenResponse1FieldId);
            _configWrapper.Setup(m => m.GetConfigIntValue("GroupLeaderFormOpenResponse2FieldId")).Returns(fakeOpenResponse2FieldId);
            _configWrapper.Setup(m => m.GetConfigIntValue("GroupLeaderFormOpenResponse3FieldId")).Returns(fakeOpenResponse3FieldId);
            _configWrapper.Setup(m => m.GetConfigIntValue("GroupLeaderFormOpenResponse4FieldId")).Returns(fakeOpenResponse4FieldId);
            _configWrapper.Setup(m => m.GetConfigIntValue("GroupLeaderConfirmationTemplate")).Returns(fakeTemplateId);

            _formService.Setup(m => m.SubmitFormResponse(It.IsAny<MpFormResponse>())).Returns((MpFormResponse form) =>
            {
                Assert.AreEqual(fakeFormId, form.FormId);
                return fakeResponseId;
            });

            _analyticsMock.Setup(m => m.Track(It.IsAny<string>(), It.IsAny<string>()));
            _communicationRepository.Setup(m => m.GetTemplate(fakeTemplateId)).Returns(ConfirmationEmailMock());

            var responseId = _fixture.SaveSpiritualGrowth(growthDto).Wait();
            Assert.AreEqual(fakeResponseId, responseId);
        }

        [Test]
        public void ShouldThrowExceptionWhenSavingSpiritualGrowthFails()
        {
            const int fakeFormId = 5;
            const int fakeOpenResponse1FieldId = 1;
            const int fakeOpenResponse2FieldId = 2;
            const int fakeOpenResponse3FieldId = 3;
            const int fakeOpenResponse4FieldId = 4;
            const int errorResponseId = 0;
            const int fakeTemplateId = 12;

            var growthDto = SpiritualGrowthMock();

            _configWrapper.Setup(m => m.GetConfigIntValue("GroupLeaderFormId")).Returns(fakeFormId);
            _configWrapper.Setup(m => m.GetConfigIntValue("GroupLeaderFormOpenResponse1FieldId")).Returns(fakeOpenResponse1FieldId);
            _configWrapper.Setup(m => m.GetConfigIntValue("GroupLeaderFormOpenResponse2FieldId")).Returns(fakeOpenResponse2FieldId);
            _configWrapper.Setup(m => m.GetConfigIntValue("GroupLeaderFormOpenResponse3FieldId")).Returns(fakeOpenResponse3FieldId);
            _configWrapper.Setup(m => m.GetConfigIntValue("GroupLeaderFormOpenResponse4FieldId")).Returns(fakeOpenResponse4FieldId);
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

        [Test]
        public void ShouldGetGroupLeaderStatus()
        {
            const string token = "letmein";
            var participant = ParticipantMock();

            _participantRepository.Setup(m => m.GetParticipantRecord(token)).Returns(participant);

            var response = _fixture.GetGroupLeaderStatus(token);
            response.Subscribe((n) =>
                               {
                                   Assert.AreEqual(participant.GroupLeaderStatus, response);
                               },
                               (err) =>
                               {
                                   Assert.Fail(err.ToString());
                               });
        }



        [Test]
        public void ShouldGetURLSegment()
        {
            const string appCode = "appCode";
            const string fieldName = "fieldName";
            const string urlSegment = "/group-leader";


            _configWrapper.Setup(m => m.GetConfigValue("GroupLeaderAppCode")).Returns(appCode);
            _configWrapper.Setup(m => m.GetConfigValue("GroupLeaderApplicationUrlSegment")).Returns(fieldName);
            _configWrapper.Setup(m => m.GetMpConfigValue(appCode, fieldName, true)).Returns(urlSegment);
            
            var response = _fixture.GetUrlSegment();

            response.Subscribe((n) =>
                               {
                                   Assert.AreEqual(urlSegment, response);
                               },
                               (err) =>
                               {
                                   Assert.Fail(err.ToString());
                               });
        }



        [Test]
        [ExpectedException(typeof(ApplicationException))]
        public void ShouldThrowExceptionWhenGettingStatusFails()
        {
            const string token = "letmein";

            _participantRepository.Setup(m => m.GetParticipantRecord(token)).Throws<Exception>();

            var response = _fixture.GetGroupLeaderStatus(token);
            response.Subscribe((n) =>
            {
                Assert.Fail("Didn't throw ApplicationException");
            });
        }

        [Test]
        public void ShouldGetReferenceData()
        {
            const int contactId = 123456;
            const int refContactId = 987654;
            var contact = ContactMock(contactId);
            var participant = ParticipantMock();
            var studentLeaderInterest = "true";

            _configWrapper.Setup(m => m.GetConfigIntValue("GroupLeaderFormId")).Returns(29);
            _configWrapper.Setup(m => m.GetConfigIntValue("GroupLeaderFormReferenceContact")).Returns(1519);
            _configWrapper.Setup(m => m.GetConfigIntValue("GroupLeaderStudentFieldId")).Returns(123);
            _participantRepository.Setup(m => m.GetParticipant(contactId)).Returns(participant);
            _contactMock.Setup(m => m.GetContactById(contactId)).Returns(contact);
            _formService.Setup(m => m.GetFormResponseAnswer(29, contactId, 123, null)).Returns(studentLeaderInterest);
            _formService.Setup(m => m.GetFormResponseAnswer(29, contactId, 1519, null)).Returns(refContactId.ToString());

            var res = _fixture.GetApplicationData(contactId);

            res.Subscribe((result) =>
            {
                Assert.AreEqual(participant, result["participant"]);
                Assert.AreEqual(contact, result["contact"]);
                Assert.AreEqual("987654", result["referenceContactId"]);
                Assert.AreEqual(studentLeaderInterest, result["studentLeaderRequest"]);
            },
            (err) =>
            {
                Assert.Fail(err.ToString());
            });
        }

        [Test]
        public void ShouldGetNoReferenceId()
        {
            const int contactId = 123456;
            var contact = ContactMock(contactId);
            var participant = ParticipantMock();

            _configWrapper.Setup(m => m.GetConfigIntValue("GroupLeaderFormId")).Returns(29);
            _configWrapper.Setup(m => m.GetConfigIntValue("GroupLeaderFormReferenceContact")).Returns(1519);
            _participantRepository.Setup(m => m.GetParticipant(contactId)).Returns(participant);
            _contactMock.Setup(m => m.GetContactById(contactId)).Returns(contact);
            _formService.Setup(m => m.GetFormResponseAnswer(29, contactId, 1519, null));

            var res = _fixture.GetApplicationData(contactId);

            res.Subscribe((result) =>
            {
                Assert.AreEqual(participant, result["participant"]);
                Assert.AreEqual(contact, result["contact"]);
                Assert.AreEqual("0", result["referenceContactId"]);
            },
            (err) =>
            {
                Assert.Fail(err.ToString());
            });
        }

        [Test]
        [ExpectedException(typeof(ApplicationException))]
        public void ShouldThrowApplicationExceptionOnGetReferenceData()
        {
            const int contactId = 123456;
            const int refContactId = 987654;
            var contact = ContactMock(contactId);

            _configWrapper.Setup(m => m.GetConfigIntValue("GroupLeaderFormId")).Returns(29);
            _configWrapper.Setup(m => m.GetConfigIntValue("GroupLeaderFormReferenceContact")).Returns(1519);
            _participantRepository.Setup(m => m.GetParticipant(contactId)).Throws<ApplicationException>();

            var res = _fixture.GetApplicationData(contactId);

            res.Subscribe((result) =>
            {
                Assert.Fail("No Exception Thrown");
            });
        }



        [Test]
        public void ShouldSendAnEmailToStudentMinistry()
        {
            const int messageId = 456;
            const int templateId = 2021;
            const int hsmsContactId = 987;
            const int contactId = 13524;
            const string emailAddr = "1@2.com";
            var participant = ParticipantMock();
            var contact = ContactMock(contactId);
            
            var referenceData = new Dictionary<string, object>
            {
                { "participant", participant },
                { "contact", contact }

            };

            _contactMock.Setup(m => m.GetContactEmail(hsmsContactId)).Returns(emailAddr);
            _configWrapper.Setup(m => m.GetConfigIntValue("GroupLeaderForStudentsEmailTemplate")).Returns(templateId);
            _configWrapper.Setup(m => m.GetConfigIntValue("StudentMinistryContactId")).Returns(hsmsContactId);

            var mergeData = new Dictionary<string, object>();
            var communication = SMCommunication(templateId, mergeData, emailAddr, hsmsContactId);

            _communicationRepository.Setup(m => m.GetTemplateAsCommunication(templateId, hsmsContactId, It.IsAny<string>(), It.IsAny<Dictionary<string, object>>())).Returns(communication);
            _communicationRepository.Setup(m => m.SendMessage(communication, false)).Returns(messageId);

            var result = _fixture.SendStudentMinistryRequestEmail(referenceData);

            result.Subscribe((n) =>
                             {
                                 Assert.AreEqual(messageId, result);
                             },
                             (err) =>
                             {
                                 Assert.Fail(err.ToString());
                             });
        }

        [Test]
        public void ShouldSendAnEmailToThePersonsReference()
        {
            const int contactId = 123456;
            const int referenceContactId = 9876545;
            const int messageId = 456;
            const int templateId = 2018;
            var participant = ParticipantMock();
            var contact = ContactMock(contactId);
            var referenceContact = ContactMock(referenceContactId);
            var referenceData = new Dictionary<string,object>
            {
                { "participant", participant },
                { "contact", contact },
                { "referenceContactId", referenceContactId.ToString() }
            };

            _contactMock.Setup(m => m.GetContactById(referenceContactId)).Returns(referenceContact);
            _configWrapper.Setup(m => m.GetConfigIntValue("GroupLeaderReferenceEmailTemplate")).Returns(templateId);
            _configWrapper.Setup(m => m.GetConfigValue("BaseMPUrl")).Returns("adminint");

            var mergeData = new Dictionary<string, object>();
            var communication = ReferenceCommunication(2018, mergeData, referenceContact);

            _communicationRepository.Setup(m => m.GetTemplateAsCommunication(2018, referenceContact.Contact_ID, It.IsAny<string>(), It.IsAny<Dictionary<string, object>>())).Returns(communication);
            _communicationRepository.Setup(m => m.SendMessage(communication, false)).Returns(messageId);

            var result = _fixture.SendReferenceEmail(referenceData);

            result.Subscribe((n) =>
            {
                Assert.AreEqual(messageId, result);
            },
            (err) =>
            {
                Assert.Fail(err.ToString());
            });
        }

        [Test]
        public void ShouldSetupMergeDataCorrectly()
        {
            const int contactId = 123456;
            const int referenceContactId = 9876545;
            const int messageId = 456;
            const int templateId = 2018;
            var participant = ParticipantMock();
            var contact = ContactMock(contactId);
            var referenceContact = ContactMock(referenceContactId);
            var referenceData = new Dictionary<string, object>
            {
                { "participant", participant },
                { "contact", contact },
                { "referenceContactId", referenceContactId.ToString() }
            };
     
            _contactMock.Setup(m => m.GetContactById(referenceContactId)).Returns(referenceContact);
            _configWrapper.Setup(m => m.GetConfigIntValue("GroupLeaderReferenceEmailTemplate")).Returns(templateId);
            _configWrapper.Setup(m => m.GetConfigValue("BaseMPUrl")).Returns("/");

            var mergeData = new Dictionary<string, object>
            {
                {"Recipient_First_Name", referenceContact.Nickname },
                {"First_Name" , contact.Nickname },
                {"Last_Name", contact.Last_Name },
                {"Participant_ID", participant.ParticipantId },
                {"Base_Url", "/"}
            };

            var communication = ReferenceCommunication(2018, mergeData, referenceContact);

            _communicationRepository.Setup(m => m.GetTemplateAsCommunication(2018, referenceContact.Contact_ID, It.IsAny<string>(), mergeData)).Returns(communication);
            _communicationRepository.Setup(m => m.SendMessage(communication, false)).Returns(messageId);

            var result = _fixture.SendReferenceEmail(referenceData);

            result.Subscribe((n) =>
            {
                Assert.AreEqual(messageId, result);
            },
            (err) =>
            {
                Assert.Fail(err.ToString());
            });
        }

        [Test]
        public void ShouldSendNoReferenceEmail()
        {
            const int templateId = 5;
            const int applicantContactId = 9987654;
            const int groupsContactId = 1123456;
            const string groupsEmail = "groups@groups.com";
            const int messageId = 7;
            var applicantContact = ContactMock(applicantContactId);
            var groupsContact = new MpMyContact
            {
                Contact_ID = groupsContactId,
                Email_Address = groupsEmail
            };
            var mergeData = new Dictionary<string, object>
            {
                { "First_Name", applicantContact.Nickname },
                { "Last_Name", applicantContact.Last_Name },
                { "Email_Address", applicantContact.Email_Address }
            };
            var communication = NoReferenceCommunication(templateId, mergeData, groupsContact);
            var referenceData = new Dictionary<string, object>
            {
                { "contact", applicantContact },
                { "participant", ParticipantMock() },
                { "referenceContactId", "0" }
            };

            _configWrapper.Setup(m => m.GetConfigIntValue("GroupLeaderNoReferenceEmailTemplate")).Returns(templateId);
            _configWrapper.Setup(m => m.GetConfigIntValue("DefaultGroupContactEmailId")).Returns(groupsContactId);
            _contactMock.Setup(m => m.GetContactEmail(groupsContactId)).Returns(groupsEmail);
            _communicationRepository.Setup(m => m.GetTemplateAsCommunication(templateId, groupsContactId, groupsEmail, mergeData)).Returns(communication);
            _communicationRepository.Setup(m => m.SendMessage(communication, false)).Returns(messageId);

            var response = _fixture.SendNoReferenceEmail(referenceData);

            response.Subscribe((n) =>
                               {
                                   Assert.AreEqual(messageId, response);
                               },
                               (err) =>
                               {
                                   Assert.Fail(err.ToString());
                               });
        }

        private static MpCommunication ReferenceCommunication(int templateId, Dictionary<string, object> mergeData, MpMyContact toContact)
        {
            var from = new MpContact {ContactId = 122222, EmailAddress = "groups@crossroads.net"};
            return new MpCommunication
            {
                AuthorUserId = 1,
                DomainId = 1,
                EmailBody = "<h1> hello </h1>",
                FromContact = from,
                ReplyToContact = from,
                TemplateId = templateId,
                EmailSubject = "whateva",
                MergeData = mergeData,
                ToContacts = new List<MpContact>() {new MpContact {EmailAddress = toContact.Email_Address, ContactId = toContact.Contact_ID} }
            };
        }

        private static MpCommunication SMCommunication(int templateId, Dictionary<string, object> mergeData, string emailAddr, int contactId)
        {
            var from = new MpContact { ContactId = 122222, EmailAddress = "groups@crossroads.net" };
            return new MpCommunication
            {
                AuthorUserId = 1,
                DomainId = 1,
                EmailBody = "<h1> hello </h1>",
                FromContact = from,
                ReplyToContact = from,
                TemplateId = templateId,
                EmailSubject = "whateva",
                MergeData = mergeData,
                ToContacts = new List<MpContact>() { new MpContact { EmailAddress = emailAddr, ContactId = contactId } }
            };
        }

        private static MpCommunication NoReferenceCommunication(int templateId, Dictionary<string, object> mergeData, MpMyContact toContact)
        {
            var from = new MpContact() {ContactId = 122222, EmailAddress = "groups@crossroads.net"};
            return new MpCommunication
            {
                AuthorUserId = 1,
                DomainId = 1,
                EmailBody = "<h1> hello </h1>",
                FromContact = from,
                ReplyToContact = from,
                TemplateId = templateId,
                EmailSubject = "Interview Needed",
                MergeData = mergeData,
                ToContacts = new List<MpContact> { new MpContact { EmailAddress = toContact.Email_Address, ContactId = toContact.Contact_ID} }
            };
        }

        private static MpMessageTemplate ReferenceTemplate()
        {
            return new MpMessageTemplate
            {
                Body = "<h1> hello </h1>",
                FromContactId = 122222,
                FromEmailAddress = "groups@crossroads.net",
                ReplyToContactId = 1222222,
                ReplyToEmailAddress = "groups@crossroads.net",
                Subject = "whateva"
            };
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
                ReferenceContactId = "89158",
                ReferenceDisplayName = "Horner, Jon"
            };
        }

        private static GroupLeaderProfileDTO NoReferenceProfileMock()
        {
            return new GroupLeaderProfileDTO
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
                ReferenceContactId = "0",
                ReferenceDisplayName = null
            };
        }

        private static MpMyContact ContactMock(int contactId)
        {
            return new MpMyContact
            {
                First_Name = "Blah",
                Last_Name = "Halb",
                Contact_ID = contactId,
                Nickname = "B",
                Email_Address = "sumemail@mock.com"
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
                OpenResponse1 = "open response 1",
                OpenResponse2 = "open response 2",
                OpenResponse3 = "open response 3",
                OpenResponse4 = "open response 4"
            };
        }
    }
}

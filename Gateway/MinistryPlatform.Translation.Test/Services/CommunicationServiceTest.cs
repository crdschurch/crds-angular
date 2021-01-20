using System;
using System.Collections.Generic;
using Crossroads.Web.Common.Configuration;
using Crossroads.Web.Common.MinistryPlatform;
using Crossroads.Web.Common.Security;
using MinistryPlatform.Translation.Models;
using MinistryPlatform.Translation.Repositories;
using MinistryPlatform.Translation.Repositories.Interfaces;
using Moq;
using NUnit.Framework;
using MpCommunication = MinistryPlatform.Translation.Models.MpCommunication;


namespace MinistryPlatform.Translation.Test.Services
{
    [TestFixture]
    public class CommunicationServiceTest
    {
        private CommunicationRepository _fixture;
        private Mock<IMinistryPlatformService> _ministryPlatformService;
        private Mock<IMinistryPlatformRestRepository> _ministryPlatformRestRepository;
        private Mock<IAuthenticationRepository> _authService;
        private Mock<IConfigurationWrapper> _configWrapper;
        private Mock<IApiUserRepository> _apiUserRepository;

        [SetUp]
        public void SetUp()
        {
            _ministryPlatformService = new Mock<IMinistryPlatformService>();
            _ministryPlatformRestRepository = new Mock<IMinistryPlatformRestRepository>();
            _authService = new Mock<IAuthenticationRepository>();
            _configWrapper = new Mock<IConfigurationWrapper>();
            _apiUserRepository = new Mock<IApiUserRepository>();

            _authService.Setup(m => m.AuthenticateClient(It.IsAny<string>(), It.IsAny<string>())).Returns(new AuthToken
            {
                AccessToken = "ABC",
                ExpiresIn = 123
            });
            _fixture = new CommunicationRepository(_ministryPlatformService.Object, _ministryPlatformRestRepository.Object, _authService.Object, _configWrapper.Object, _apiUserRepository.Object);
        }

        [Test]
        public void GetTemplateAsCommunication()
        {
            const int templateId = 3;
            const int fromContactId = 6;
            const string fromEmailAddress = "brady@minon.com";
            const int replyContactId = 5;
            const string replyEmailAddress = "bob@minon.com";
            const int toContactId = 4;
            const string toEmailAddress = "help@me.com";
            const string mockBody = "mock email body";
            const string mockSubject = "mock subject";

            _configWrapper.Setup(m => m.GetConfigIntValue("DefaultAuthorUser")).Returns(99);

            //template
            var templateDictionary = new Dictionary<string, object>
            {
                { "Body", mockBody },
                { "Subject", mockSubject },
                { "From_Contact", fromContactId },
                { "From_Contact_Text", "Crossroads; " + fromEmailAddress },
                { "Reply_to_Contact", replyContactId },
                { "Reply_to_Contact_Text", "Crossroads;\t" + replyEmailAddress }
            };
            _ministryPlatformService.Setup(m => m.GetRecordDict(341, templateId, It.IsAny<string>(), false)).Returns(templateDictionary);

            var communication = _fixture.GetTemplateAsCommunication(templateId, fromContactId, fromEmailAddress, replyContactId, replyEmailAddress, toContactId, toEmailAddress);

            _configWrapper.VerifyAll();
            _ministryPlatformService.VerifyAll();

            Assert.AreEqual(99, communication.AuthorUserId);
            Assert.AreEqual(mockBody, communication.EmailBody);
            Assert.AreEqual(mockSubject, communication.EmailSubject);
            Assert.AreEqual(fromContactId, communication.FromContact.ContactId);
            Assert.AreEqual(fromEmailAddress, communication.FromContact.EmailAddress);
            Assert.IsNull(communication.MergeData);
            Assert.AreEqual(replyContactId, communication.ReplyToContact.ContactId);
            Assert.AreEqual(replyEmailAddress, communication.ReplyToContact.EmailAddress);
            Assert.AreEqual(toContactId, communication.ToContacts[0].ContactId);
            Assert.AreEqual(toEmailAddress, communication.ToContacts[0].EmailAddress);
        }

        [Test]
        public void TestGetTemplate()
        {
            const int templateId = 3;
            const int fromContactId = 6;
            const string fromEmailAddress = "brady@minon.com";
            const int replyContactId = 5;
            const string replyEmailAddress = "bob@minon.com";
            const string mockBody = "mock email body";
            const string mockSubject = "mock subject";

            var templateDictionary = new Dictionary<string, object>
            {
                { "Body", mockBody },
                { "Subject", mockSubject },
                { "From_Contact", fromContactId },
                { "From_Contact_Text", "Crossroads; " + fromEmailAddress },
                { "Reply_to_Contact", replyContactId },
                { "Reply_to_Contact_Text", "Crossroads;\t" + replyEmailAddress }
            };

            _ministryPlatformService.Setup(m => m.GetRecordDict(341, templateId, It.IsAny<string>(), false)).Returns(templateDictionary);
            var template = _fixture.GetTemplate(templateId);
            _ministryPlatformService.VerifyAll();
            Assert.IsNotNull(template);
            Assert.AreEqual(fromContactId, template.FromContactId);
            Assert.AreEqual(fromEmailAddress, template.FromEmailAddress);
            Assert.AreEqual(replyContactId, template.ReplyToContactId);
            Assert.AreEqual(replyEmailAddress, template.ReplyToEmailAddress);
            Assert.AreEqual(mockBody, template.Body);
            Assert.AreEqual(mockSubject, template.Subject);

        }

        [Test]
        public void TestParseTemplateBody()
        {
            var mergeData = new Dictionary<string, object>
            {
                {"DavidsGame", "Global Thermonuclear War"},
                {"WoprsGame", "Chess"},
                {"WhenToPlayChess", string.Empty}
            };

            var parsed = _fixture.ParseTemplateBody("David: Would you like to play a game of [DavidsGame]? / WOPR: Not right now, wouldn't you like to play a game of [WoprsGame] instead? / David: No, maybe some other time, [WhenToPlayChess]",
                                       mergeData);

            Assert.AreEqual("David: Would you like to play a game of Global Thermonuclear War? / WOPR: Not right now, wouldn't you like to play a game of Chess instead? / David: No, maybe some other time, ", parsed);
        }

        [Test]
        public void TestParseTemplateBodyWithNullValueInMergeData()
        {
            var mergeData = new Dictionary<string, object>
            {
                {"Key1", "Value1"},
                {"Key2", null}
            };

            var parsed = _fixture.ParseTemplateBody("This is [Key1] and [Key2]", mergeData);
            Assert.AreEqual("This is Value1 and ", parsed);
        }

        [Test]
        public void TestSendMessageWithDate()
        {
            MpCommunication comm = new MpCommunication()
            {
                AuthorUserId = 1,
                DomainId = 2,
                EmailBody = "body",
                EmailSubject = "subject",
                FromContact = new MpContact(),
                MergeData = new Dictionary<string, object>(),
                ReplyToContact = new MpContact(),
                StartDate = new DateTime(2017, 1, 1),
                TemplateId = 123,
                ToContacts = new List<MpContact>()
            };
            var spiedDict = new Dictionary<string, object>();
            _ministryPlatformService.Setup(mock => mock.CreateRecord(It.IsAny<int>(), It.IsAny<Dictionary<string, object>>(), It.IsAny<string>(),false))
                .Callback<int, Dictionary<string, object>, string, bool>((id, dict, token, bl) => spiedDict = dict);
            _ministryPlatformService.Setup(mock => mock.CreateSubRecord(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<Dictionary<string, object>>(), It.IsAny<string>(), false))
                .Returns(1);
            _fixture.SendMessage(comm);
            Assert.AreEqual(comm.StartDate, spiedDict["Start_Date"]);

        }

        [Test]
        public void TestSendMessageWithoutDate()
        {
            MpCommunication comm = new MpCommunication()
            {
                AuthorUserId = 1,
                DomainId = 2,
                EmailBody = "body",
                EmailSubject = "subject",
                FromContact = new MpContact(),
                MergeData = new Dictionary<string, object>(),
                ReplyToContact = new MpContact(),
                TemplateId = 123,
                ToContacts = new List<MpContact>()
            };
            var spiedDict = new Dictionary<string, object>();
            _ministryPlatformService.Setup(mock => mock.CreateRecord(It.IsAny<int>(), It.IsAny<Dictionary<string, object>>(), It.IsAny<string>(), false))
                .Callback<int, Dictionary<string, object>, string, bool>((id, dict, token, bl) => spiedDict = dict);
            _ministryPlatformService.Setup(mock => mock.CreateSubRecord(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<Dictionary<string, object>>(), It.IsAny<string>(), false))
                .Returns(1);
            _fixture.SendMessage(comm);

            // Verify that the dictionary contains a Start_Date, that it is a DateTime, 
            // and that is reasonably close to DateTime.Now
            Assert.IsTrue(spiedDict.ContainsKey("Start_Date"));
            Assert.IsInstanceOf<DateTime>(spiedDict["Start_Date"]);
            Assert.IsTrue(DateTime.Now.Subtract((DateTime)spiedDict["Start_Date"]).TotalSeconds < 30);
        }
    }
}
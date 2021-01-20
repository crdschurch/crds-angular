using System;
using System.Collections.Generic;
using Crossroads.Web.Common.Configuration;
using Crossroads.Web.Common.MinistryPlatform;
using MinistryPlatform.Translation.Models;
using MinistryPlatform.Translation.Models.Childcare;
using MinistryPlatform.Translation.Repositories;
using MinistryPlatform.Translation.Repositories.Interfaces;
using Moq;
using NUnit.Framework;

namespace MinistryPlatform.Translation.Test.Services
{
    [TestFixture]
    public class ChildcareRepositoryTest
    {
        private Mock<IMinistryPlatformRestRepository> _ministryPlatformRest;
        private Mock<IConfigurationWrapper> _configurationWrapper;
        private Mock<IApiUserRepository> _apiUserRepository;
        private IChildcareRepository _fixture;

        [SetUp]
        public void Setup()
        {
            _ministryPlatformRest = new Mock<IMinistryPlatformRestRepository>();
            _configurationWrapper = new Mock<IConfigurationWrapper>();
            _apiUserRepository = new Mock<IApiUserRepository>();   
            _fixture = new ChildcareRepository(_configurationWrapper.Object, _ministryPlatformRest.Object,_apiUserRepository.Object);
        }

        [Test]
        public void childShouldBeRsvpd()
        {
            var token = "some silly string";
            var contactId = 12234;
            var groupId = 989423;

            var parms = new Dictionary<string,object>()
            {
                {"@ContactId", contactId},
                {"@EventGroupID", groupId }
            };

            var retVal = new List<List<MPRspvd>>()
            {
                new List<MPRspvd>()
                {
                    new MPRspvd() {Rsvpd = true}
                }
            };

            _ministryPlatformRest.Setup(m => m.UsingAuthenticationToken(token)).Returns(_ministryPlatformRest.Object);
            _ministryPlatformRest.Setup(m => m.GetFromStoredProc<MPRspvd>("api_crds_childrsvpd", parms)).Returns(retVal);
            var rsvpd = _fixture.IsChildRsvpd(contactId, groupId, token);

            _ministryPlatformRest.VerifyAll();
            Assert.AreEqual(true, rsvpd);
        }

        [Test]
        public void childShouldNotBeRsvpd()
        {
            var token = "some silly string";
            var contactId = 12234;
            var groupId = 989423;

            var parms = new Dictionary<string, object>()
            {
                {"@ContactId", contactId},
                {"@EventGroupID", groupId }
            };

            var retVal = new List<List<MPRspvd>>()
            {
                new List<MPRspvd>()
            };

            _ministryPlatformRest.Setup(m => m.UsingAuthenticationToken(token)).Returns(_ministryPlatformRest.Object);
            _ministryPlatformRest.Setup(m => m.GetFromStoredProc<MPRspvd>("api_crds_childrsvpd", parms)).Returns(retVal);
            var rsvpd = _fixture.IsChildRsvpd(contactId, groupId, token);

            _ministryPlatformRest.VerifyAll();
            Assert.AreEqual(false, rsvpd);
        }

        [Test]
        public void ShouldGetChildcareEmails()
        {
            const String token = "random long string";

            var retVal = new List<List<MpContact>>()
            {
                new List<MpContact>()
                {
                    new MpContact() {EmailAddress = "matt.silbernagel@ingagepartners.com"},
                    new MpContact() {EmailAddress = "silbermm@gmail.com"}
                }
            };

            _ministryPlatformRest.Setup(m => m.UsingAuthenticationToken(token)).Returns(_ministryPlatformRest.Object);
            _ministryPlatformRest.Setup(m => m.GetFromStoredProc<MpContact>("api_crds_ChildcareReminderEmails")).Returns(retVal);
            
            var resp = _fixture.GetChildcareReminderEmails(token);
            _ministryPlatformRest.VerifyAll();
            Assert.AreEqual(2, resp.Count);
        }

        [Test]
        public void ShouldGetEmptyListOfChildcareEmails()
        {
            const String token = "random long string";

            var retVal = new List<List<MpContact>>()
            {
                new List<MpContact>() { }         
            };

            _ministryPlatformRest.Setup(m => m.UsingAuthenticationToken(token)).Returns(_ministryPlatformRest.Object);
            _ministryPlatformRest.Setup(m => m.GetFromStoredProc<MpContact>("api_crds_ChildcareReminderEmails")).Returns(retVal);

            var resp = _fixture.GetChildcareReminderEmails(token);
            _ministryPlatformRest.VerifyAll();
            Assert.AreEqual(0, resp.Count);
        }

        [Test]
        public void ShouldThowExceptionWhenChildcareEmailsErrors()
        {
            const string token = "random long string";

            _ministryPlatformRest.Setup(m => m.UsingAuthenticationToken(token)).Returns(_ministryPlatformRest.Object);
            _ministryPlatformRest.Setup(m => m.GetFromStoredProc<MpContact>("api_crds_ChildcareReminderEmails")).Throws<Exception>();

            Assert.Throws<Exception>(() =>
            {
                _fixture.GetChildcareReminderEmails(token);
            });
            
        }
    }
}

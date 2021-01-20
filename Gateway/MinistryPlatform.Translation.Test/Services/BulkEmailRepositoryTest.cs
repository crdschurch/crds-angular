using System;
using System.Collections.Generic;
using System.Configuration;
using Crossroads.Web.Common.Configuration;
using Crossroads.Web.Common.Security;
using Crossroads.Web.Common.MinistryPlatform;
using MinistryPlatform.Translation.Models;
using MinistryPlatform.Translation.Repositories;
using MinistryPlatform.Translation.Repositories.Interfaces;
using Moq;
using NUnit.Framework;

namespace MinistryPlatform.Translation.Test.Services
{
    [TestFixture]
    public class BulkEmailRepositoryTest
    {
        private BulkEmailRepository _fixture;

        private Mock<IMinistryPlatformService> _ministryPlatformService;
        private Mock<IAuthenticationRepository> _authService;
        private Mock<IConfigurationWrapper> _configWrapper;
        private Mock<IApiUserRepository> _apiUserService;



        [SetUp]
        public void SetUp()
        {
            _ministryPlatformService = new Mock<IMinistryPlatformService>();
            _authService = new Mock<IAuthenticationRepository>();
            _apiUserService = new Mock<IApiUserRepository>();
            _configWrapper = new Mock<IConfigurationWrapper>();

            _fixture = new BulkEmailRepository(_authService.Object, _configWrapper.Object, _ministryPlatformService.Object, _apiUserService.Object);
        }

        [Test]
        public void ShouldHandleUnsubscribeSync()
        {
            var token = "a1b2c3d4e5f6g7h8";
            var unsubscribed = true;

            var contactPublications = GenerateContactPublications(1);

            MpBulkEmailSubscriberOpt bulkEmailSubscriberOpt = new MpBulkEmailSubscriberOpt
            {
                EmailAddress = "test@test.com",
                PublicationID = 1,
                Status = "unsubscribed",
                ThirdPartySystemID = "a1b2c3d4"
            };

            var segmentationPageId = Convert.ToInt32(ConfigurationManager.AppSettings["SegmentationBasePageViewId"]);

            _ministryPlatformService.Setup(mocked => mocked.GetPageViewRecords(segmentationPageId, It.IsAny<string>(), It.IsAny<string>(), "", 0)).Returns(contactPublications);

            var result = _fixture.SetSubscriberStatus(token, bulkEmailSubscriberOpt);

            Assert.AreEqual(result, unsubscribed);

        }

        [Test]
        public void ShouldHandleSubscribeSync()
        {
            var token = "a1b2c3d4e5f6g7h8";
            var unsubscribed = false;

            var contactPublications = GenerateContactPublications(1);

            MpBulkEmailSubscriberOpt bulkEmailSubscriberOpt = new MpBulkEmailSubscriberOpt
            {
                EmailAddress = "test@test.com",
                PublicationID = 1,
                Status = "subscribed",
                ThirdPartySystemID = "a1b2c3d4"
            };

            var segmentationPageId = Convert.ToInt32(ConfigurationManager.AppSettings["SegmentationBasePageViewId"]);

            _ministryPlatformService.Setup(mocked => mocked.GetPageViewRecords(segmentationPageId, It.IsAny<string>(), It.IsAny<string>(), "", 0)).Returns(contactPublications);

            var result = _fixture.SetSubscriberStatus(token, bulkEmailSubscriberOpt);

            Assert.AreEqual(result, unsubscribed);

        }

        [Test]
        public void ShouldHandlePendingSync()
        {
            var token = "a1b2c3d4e5f6g7h8";
            var unsubscribed = false;

            var contactPublications = GenerateContactPublications(1);

            MpBulkEmailSubscriberOpt bulkEmailSubscriberOpt = new MpBulkEmailSubscriberOpt
            {
                EmailAddress = "test@test.com",
                PublicationID = 1,
                Status = "pending",
                ThirdPartySystemID = "a1b2c3d4"
            };

            var segmentationPageId = Convert.ToInt32(ConfigurationManager.AppSettings["SegmentationBasePageViewId"]);

            _ministryPlatformService.Setup(mocked => mocked.GetPageViewRecords(segmentationPageId, It.IsAny<string>(), It.IsAny<string>(), "", 0)).Returns(contactPublications);

            var result = _fixture.SetSubscriberStatus(token, bulkEmailSubscriberOpt);

            Assert.AreEqual(result, unsubscribed);

        }

        private List<Dictionary<string, object>> GenerateContactPublications(int recordsToGenerate)
        {
            List<Dictionary<string, object>> contactPublications = new List<Dictionary<string, object>>();

            for (int i = 0; i < recordsToGenerate; i++)
            {
                Dictionary<string, object> contactPublication = new Dictionary<string, object>();
                contactPublication.Add("Contact_Publication_ID", i);

                contactPublications.Add(contactPublication);
            }

            return contactPublications;
        } 

    }
}
using System;
using System.Collections.Generic;
using Crossroads.Utilities.Interfaces;
using Crossroads.Web.Common;
using Crossroads.Web.Common.Configuration;
using Crossroads.Web.Common.MinistryPlatform;
using Crossroads.Web.Common.Security;
using MinistryPlatform.Translation.Models;
using MinistryPlatform.Translation.Models.DTO;
using MinistryPlatform.Translation.Repositories;
using MinistryPlatform.Translation.Repositories.Interfaces;
using Moq;
using NUnit.Framework;

namespace MinistryPlatform.Translation.Test.Services
{
    public class GroupToolRepositoryTest
    {
        private GroupToolRepository _fixture;
        private Mock<IMinistryPlatformService> _ministryPlatformService;
        private Mock<IMinistryPlatformRestRepository> _ministryPlatformRestRepository;
        private Mock<IApiUserRepository> _apiUserRepository;

        private const int InvitationPageId = 55;
        private const int SmallGroupTypeId = 1100;
        private const int GatheringTypeId = 3030;

        [SetUp]
        public void SetUp()
        {
            _ministryPlatformService = new Mock<IMinistryPlatformService>(MockBehavior.Strict);
            _ministryPlatformRestRepository = new Mock<IMinistryPlatformRestRepository>(MockBehavior.Strict);
            _apiUserRepository = new Mock<IApiUserRepository>(MockBehavior.Strict);

            var config = new Mock<IConfigurationWrapper>(MockBehavior.Strict);
            var auth = new Mock<IAuthenticationRepository>(MockBehavior.Strict);

            config.Setup(mocked => mocked.GetConfigIntValue("InvitationPageID")).Returns(InvitationPageId);
            config.Setup(mocked => mocked.GetConfigIntValue("SmallGroupTypeId")).Returns(SmallGroupTypeId);
            config.Setup(mocked => mocked.GetConfigIntValue("AnywhereGroupTypeId")).Returns(GatheringTypeId);

            config.Setup(mocked => mocked.GetEnvironmentVarAsString("API_USER")).Returns("api_user");
            config.Setup(mocked => mocked.GetEnvironmentVarAsString("API_PASSWORD")).Returns("password");

            auth.Setup(m => m.AuthenticateUser(It.IsAny<string>(), It.IsAny<string>())).Returns(new AuthToken
            {
                AccessToken = "ABC",
                ExpiresIn = 123
            });

            _fixture = new GroupToolRepository(_ministryPlatformService.Object, config.Object, auth.Object, _ministryPlatformRestRepository.Object, _apiUserRepository.Object);
        }

        [Test]
        public void GetInquiriesTest()
        {
            var dto = new List<MpInquiry>
            {
                new MpInquiry
                {
                    InquiryId = 178,
                    GroupId = 199846,
                    EmailAddress = "test@jk.com",
                    PhoneNumber = "444-111-2111",
                    FirstName = "Joe",
                    LastName = "Smith",
                    RequestDate = new DateTime(2004, 3, 12),
                    Placed = true,
                    ContactId = 1,
                }
            };

            const int groupId = 199846;
            const string token = "abc123";
            _apiUserRepository.Setup(mocked => mocked.GetDefaultApiUserToken()).Returns(token);
            _ministryPlatformRestRepository.Setup(m => m.UsingAuthenticationToken("abc123")).Returns(_ministryPlatformRestRepository.Object);
            _ministryPlatformRestRepository.Setup(
                m =>
                    m.Search<MpInquiry>($"Group_ID_Table.Group_Type_ID in ({SmallGroupTypeId}, {GatheringTypeId}) AND Placed is null AND Group_Inquiries.Group_ID = {groupId}",
                        "Group_Inquiries.*", (string)null, (bool)false)).Returns(dto);


            var result = _fixture.GetInquiries(groupId);
            _ministryPlatformRestRepository.VerifyAll();

            Assert.IsNotNull(result);
        }

        [Test]
        public void TestGetInquiriesForAllGroups()
        {
            const string token = "abc123";
            var inquiryResults = new List<MpInquiry>
            {
                new MpInquiry
                {
                    ContactId = 789,
                    EmailAddress = "me@here.com",
                    FirstName = "first",
                    LastName = "last",
                    GroupId = 456,
                    InquiryId = 123,
                    Placed = true,
                    PhoneNumber = "513-555-1212"
                }
            };
            _apiUserRepository.Setup(mocked => mocked.GetDefaultApiUserToken()).Returns(token);
            _ministryPlatformRestRepository.Setup(m => m.UsingAuthenticationToken("abc123")).Returns(_ministryPlatformRestRepository.Object);
            _ministryPlatformRestRepository.Setup(
                m =>
                    m.Search<MpInquiry>($"Group_ID_Table.Group_Type_ID in ({SmallGroupTypeId}, {GatheringTypeId}) AND Placed is null",
                        "Group_Inquiries.*", (string)null, (bool)false)).Returns(inquiryResults);
            var results = _fixture.GetInquiries();
            _apiUserRepository.VerifyAll();
            _ministryPlatformRestRepository.VerifyAll();
            Assert.IsNotNull(results);
        }

        [Test]
        public void GetCurrentyJourneyShouldReturnAttributeName()
        {
            var dto = new List<MpAttribute>
            {
                new MpAttribute
                {
                    Name = "This is not the best journey in the world"
                }
            };
            _apiUserRepository.Setup(m => m.GetDefaultApiUserToken()).Returns("TheBestToken");
            _ministryPlatformRestRepository.Setup(m => m.UsingAuthenticationToken("TheBestToken")).Returns(_ministryPlatformRestRepository.Object);
            _ministryPlatformRestRepository.Setup(
                m =>
                    m.Search<MpAttribute>(
                        "ATTRIBUTE_CATEGORY_ID_TABLE.ATTRIBUTE_CATEGORY_ID = 51 AND GETDATE() BETWEEN START_DATE AND ISNULL(END_DATE, GETDATE())",
                        "Attribute_Name",(string) null, (bool) false)).Returns(dto);

            var result = _fixture.GetCurrentJourney();

            _ministryPlatformRestRepository.VerifyAll();
            Assert.AreEqual(result, dto[0].Name);

        }

        [Test]
        public void GetCurrentJourneyShouldReturnNull()
        {
            _apiUserRepository.Setup(m => m.GetDefaultApiUserToken()).Returns("TheBestToken");
            _ministryPlatformRestRepository.Setup(m => m.UsingAuthenticationToken("TheBestToken")).Returns(_ministryPlatformRestRepository.Object);
            _ministryPlatformRestRepository.Setup(
                m =>
                    m.Search<MpAttribute>(
                        "ATTRIBUTE_CATEGORY_ID_TABLE.ATTRIBUTE_CATEGORY_ID = 51 AND GETDATE() BETWEEN START_DATE AND ISNULL(END_DATE, GETDATE())",
                        "Attribute_Name", (string)null, (bool)false)).Returns(new List<MpAttribute>());

            var result = _fixture.GetCurrentJourney();

            _ministryPlatformRestRepository.VerifyAll();
            Assert.IsNull(result);
        }
    }
}

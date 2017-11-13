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
        private const int GroupInquiriesSubPageId = 304;
        private const int GroupInquiriesNotPlacedPageView = 90210;

        [SetUp]
        public void SetUp()
        {
            _ministryPlatformService = new Mock<IMinistryPlatformService>(MockBehavior.Strict);
            _ministryPlatformRestRepository = new Mock<IMinistryPlatformRestRepository>(MockBehavior.Strict);
            _apiUserRepository = new Mock<IApiUserRepository>(MockBehavior.Strict);

            var config = new Mock<IConfigurationWrapper>(MockBehavior.Strict);
            var auth = new Mock<IAuthenticationRepository>(MockBehavior.Strict);

            config.Setup(mocked => mocked.GetConfigIntValue("InvitationPageID")).Returns(InvitationPageId);
            config.Setup(mocked => mocked.GetConfigIntValue("GroupInquiresSubPage")).Returns(GroupInquiriesSubPageId);
            config.Setup(mocked => mocked.GetConfigIntValue("GroupInquiriesNotPlacedPageView")).Returns(GroupInquiriesNotPlacedPageView);

            config.Setup(mocked => mocked.GetEnvironmentVarAsString("API_USER")).Returns("api_user");
            config.Setup(mocked => mocked.GetEnvironmentVarAsString("API_PASSWORD")).Returns("password");

            auth.Setup(m => m.Authenticate(It.IsAny<string>(), It.IsAny<string>())).Returns(new AuthToken
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

            var returned = new List<Dictionary<string, object>>
            {
                new Dictionary<string, object>
                {
                    {"dp_RecordID", 178},
                    {"Email", "test@jk.com"},
                    {"Phone", "444-111-2111"},
                    {"First_Name", "Joe"},
                    {"Last_Name", "Smith"},
                    {"Inquiry_Date", "3/12/2004"},
                    {"Placed", "true"},
                    {"Contact_ID", 1}
                }
            };

            const string token = "abc123";
            _apiUserRepository.Setup(mocked => mocked.GetToken()).Returns(token);

            _ministryPlatformService.Setup(mocked => mocked.GetSubPageRecords(GroupInquiriesSubPageId, groupId, It.IsAny<string>())).Returns(returned);

            var result = _fixture.GetInquiries(groupId);
            _ministryPlatformService.VerifyAll();

            Assert.IsNotNull(result);
            Assert.AreEqual(dto[0].InquiryId, result[0].InquiryId);
            Assert.AreEqual(dto[0].GroupId, result[0].GroupId);
            Assert.AreEqual(dto[0].EmailAddress, result[0].EmailAddress);
            Assert.AreEqual(dto[0].PhoneNumber, result[0].PhoneNumber);
            Assert.AreEqual(dto[0].FirstName, result[0].FirstName);
            Assert.AreEqual(dto[0].LastName, result[0].LastName);
            Assert.AreEqual(dto[0].RequestDate, result[0].RequestDate);
            Assert.AreEqual(dto[0].RequestDate, result[0].RequestDate);
            Assert.AreEqual(dto[0].ContactId, result[0].ContactId);
        }

        [Test]
        public void TestSearchGroupsNoKeywords()
        {
            int[] groupTypeId = new int[] { 567 };
            const string token = "abc123";
            _apiUserRepository.Setup(mocked => mocked.GetToken()).Returns(token);
            var searchResults = new List<List<MpGroupSearchResultDto>>
            {
                new List<MpGroupSearchResultDto>
                {
                    new MpGroupSearchResultDto()
                }
            };
            _ministryPlatformRestRepository.Setup(mocked => mocked.UsingAuthenticationToken(token)).Returns(_ministryPlatformRestRepository.Object);
            _ministryPlatformRestRepository.Setup(
                mocked =>
                    mocked.GetFromStoredProc<MpGroupSearchResultDto>(GroupToolRepository.SearchGroupsProcName,
                                                                     It.Is<Dictionary<string, object>>(parms => parms.Count == 1 && parms["@GroupTypeId"].Equals(String.Join(",", groupTypeId))))).Returns(searchResults);

            var results = _fixture.SearchGroups(groupTypeId);
            _apiUserRepository.VerifyAll();
            _ministryPlatformRestRepository.VerifyAll();
            Assert.AreSame(searchResults[0], results);
        }

        [Test]
        public void TestSearchGroups()
        {
            var keywords = new[] {"keyword1", "keyword2"};
            int[] groupTypeId = new int[] { 567 };
            const string token = "abc123";
            _apiUserRepository.Setup(mocked => mocked.GetToken()).Returns(token);
            var searchResults = new List<List<MpGroupSearchResultDto>>
            {
                new List<MpGroupSearchResultDto>
                {
                    new MpGroupSearchResultDto()
                }
            };
            _ministryPlatformRestRepository.Setup(mocked => mocked.UsingAuthenticationToken(token)).Returns(_ministryPlatformRestRepository.Object);
            _ministryPlatformRestRepository.Setup(
                mocked =>
                    mocked.GetFromStoredProc<MpGroupSearchResultDto>(GroupToolRepository.SearchGroupsProcName,
                                                                     It.Is<Dictionary<string, object>>(
                                                                         parms =>
                                                                             parms.Count == 2 && parms["@GroupTypeId"].Equals(String.Join(",", groupTypeId)) &&
                                                                             parms["@SearchString"].Equals(string.Join(",", keywords))))).Returns(searchResults);

            var results = _fixture.SearchGroups(groupTypeId, keywords);
            _apiUserRepository.VerifyAll();
            _ministryPlatformRestRepository.VerifyAll();
            Assert.AreSame(searchResults[0], results);
        }

        [Test]
        public void TestSearchGroupsMultipleTypes()
        {
            var keywords = new[] { "keyword1", "keyword2" };
            int[] groupTypeId = new int[] { 567, 123 };
            const string token = "abc123";
            _apiUserRepository.Setup(mocked => mocked.GetToken()).Returns(token);
            var searchResults = new List<List<MpGroupSearchResultDto>>
            {
                new List<MpGroupSearchResultDto>
                {
                    new MpGroupSearchResultDto()
                }
            };
            _ministryPlatformRestRepository.Setup(mocked => mocked.UsingAuthenticationToken(token)).Returns(_ministryPlatformRestRepository.Object);
            _ministryPlatformRestRepository.Setup(
                mocked =>
                    mocked.GetFromStoredProc<MpGroupSearchResultDto>(GroupToolRepository.SearchGroupsProcName,
                                                                     It.Is<Dictionary<string, object>>(
                                                                         parms =>
                                                                             parms.Count == 2 && parms["@GroupTypeId"].Equals(String.Join(",", groupTypeId)) &&
                                                                             parms["@SearchString"].Equals(string.Join(",", keywords))))).Returns(searchResults);

            var results = _fixture.SearchGroups(groupTypeId, keywords);
            _apiUserRepository.VerifyAll();
            _ministryPlatformRestRepository.VerifyAll();
            Assert.AreSame(searchResults[0], results);
        }

        [Test]
        public void TestGetInquiriesForAllGroups()
        {
            const string token = "abc123";
            _apiUserRepository.Setup(mocked => mocked.GetToken()).Returns(token);
            var inquiryResults = new List<Dictionary<string, object>>
            {
                new Dictionary<string, object>
                {
                    {"dp_RecordID", 123},
                    {"Group_ID", 456},
                    {"Email", "me@here.com"},
                    {"Phone", "513-555-1212"},
                    {"First_Name", "first"},
                    {"Last_Name", "last"},
                    {"Inquiry_Date", new DateTime(1973, 10, 15).Date},
                    {"Placed", true},
                    {"Contact_ID", 789}
                }
            };

            _ministryPlatformService.Setup(mocked => mocked.GetPageViewRecords(GroupInquiriesNotPlacedPageView, token, string.Empty, string.Empty, 0)).Returns(inquiryResults);
            var results = _fixture.GetInquiries();
            _apiUserRepository.VerifyAll();
            _ministryPlatformService.VerifyAll();
            Assert.IsNotNull(results);
            Assert.AreEqual(1, results.Count);
            Assert.AreEqual(inquiryResults[0]["dp_RecordID"], results[0].InquiryId);
            Assert.AreEqual(inquiryResults[0]["Group_ID"], results[0].GroupId);
            Assert.AreEqual(inquiryResults[0]["Email"], results[0].EmailAddress);
            Assert.AreEqual(inquiryResults[0]["Phone"], results[0].PhoneNumber);
            Assert.AreEqual(inquiryResults[0]["First_Name"], results[0].FirstName);
            Assert.AreEqual(inquiryResults[0]["Last_Name"], results[0].LastName);
            Assert.AreEqual(inquiryResults[0]["Inquiry_Date"], results[0].RequestDate);
            Assert.AreEqual(inquiryResults[0]["Placed"], results[0].Placed);
            Assert.AreEqual(inquiryResults[0]["Contact_ID"], results[0].ContactId);
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

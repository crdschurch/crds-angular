using System;
using System.Collections.Generic;
using crds_angular.Models.Crossroads.Campaign;
using Crossroads.Web.Common.MinistryPlatform;
using crds_angular.Services;
using crds_angular.Services.Interfaces;
using crds_angular.Util.Interfaces;
using MinistryPlatform.Translation.Models.DTO;
using MinistryPlatform.Translation.Repositories.Interfaces;
using Moq;
using NUnit.Framework;

namespace crds_angular.test.Services
{
    [TestFixture]
    public class CampaignServiceTest
    {
        private Mock<IApiUserRepository> _apiUserService;
        private Mock<ICampaignRepository> _campaignRepository;
        private Mock<IDateTime> _dateTimeWrapper;
        private ICampaignService _fixture;

        private int _pledgeCampaignId = 123456;

        [SetUp]
        public void Setup()
        {
            _apiUserService = new Mock<IApiUserRepository>();
            _campaignRepository = new Mock<ICampaignRepository>();
            _dateTimeWrapper = new Mock<IDateTime>();

            _fixture = new CampaignService(_apiUserService.Object, _campaignRepository.Object, _dateTimeWrapper.Object);

            const string apiToken = "qwerty1234";
            _apiUserService.Setup(m => m.GetDefaultApiClientToken()).Returns(apiToken);

            MpPledgeCampaignSummaryDto dto = new MpPledgeCampaignSummaryDto()
            {
                PledgeCampaignId = _pledgeCampaignId,
                NoCommitmentCount = 1100,
                NoCommitmentAmount = 1200000.00M,
                TotalGiven = 38000000.00M,
                TotalCommitted = 95000000.00M,
                StartDate = new DateTime(2015, 11, 15),
                EndDate = new DateTime(2018, 12, 31),
                NotStartedCount = 1800,
                BehindCount = 3200,
                OnPaceCount = 1400,
                AheadCount = 500,
                CompletedCount = 390,
                TotalCount = 1800 + 3200 + 1400 + 500 + 390
            };
            List<MpPledgeCampaignSummaryDto> list = new List<MpPledgeCampaignSummaryDto>();
            list.add(dto);
            _campaignRepository.Setup(m => m.GetPledgeCampaignSummary(apiToken, _pledgeCampaignId)).Returns(list);
        }

        [Test]
        public void ShouldGetSummaryForCampaignInProgess()
        {
            DateTime mockDateTime = new DateTime(2017, 4, 18, 14, 30, 25);
            _dateTimeWrapper.Setup(m => m.Now).Returns(mockDateTime);

            PledgeCampaignSummaryDto result = _fixture.GetSummary(_pledgeCampaignId)[0];

            Assert.AreEqual(_pledgeCampaignId, result.PledgeCampaignId);
            Assert.AreEqual(38000000.00M + 1200000.00M, result.TotalGiven);
            Assert.AreEqual(95000000.00M, result.TotalCommitted);
            Assert.AreEqual(521, result.CurrentDays);
            Assert.AreEqual(1143, result.TotalDays);
        }

        [Test]
        public void CurrentDaysShouldNotBeNegative()
        {
            DateTime mockDateTime = new DateTime(2010, 4, 18, 14, 30, 25);
            _dateTimeWrapper.Setup(m => m.Now).Returns(mockDateTime);

            PledgeCampaignSummaryDto result = _fixture.GetSummary(_pledgeCampaignId)[0];

            Assert.AreEqual(0, result.CurrentDays);
            Assert.AreEqual(1143, result.TotalDays);
        }

        [Test]
        public void CurrentDaysShouldNotExceedTotalDays()
        {
            DateTime mockDateTime = new DateTime(2025, 4, 18, 14, 30, 25);
            _dateTimeWrapper.Setup(m => m.Now).Returns(mockDateTime);

            PledgeCampaignSummaryDto result = _fixture.GetSummary(_pledgeCampaignId)[0];

            Assert.AreEqual(1143, result.CurrentDays);
            Assert.AreEqual(1143, result.TotalDays);
        }
    }
}

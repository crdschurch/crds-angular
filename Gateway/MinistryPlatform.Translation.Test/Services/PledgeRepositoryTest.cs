using System;
using System.Collections.Generic;
using Crossroads.Utilities.Interfaces;
using Crossroads.Web.Common;
using Crossroads.Web.Common.Configuration;
using Crossroads.Web.Common.MinistryPlatform;
using Crossroads.Web.Common.Security;
using MinistryPlatform.Translation.Models;
using MinistryPlatform.Translation.Repositories;
using MinistryPlatform.Translation.Repositories.Interfaces;
using Moq;
using NUnit.Framework;

namespace MinistryPlatform.Translation.Test.Services
{
    [TestFixture]
    public class PledgeRepositoryTest
    {
        private Mock<IMinistryPlatformService> _ministryPlatformService;
        private Mock<IMinistryPlatformRestRepository> _ministryPlatformRestRepository;
        private Mock<IAuthenticationRepository> _authService;
        private Mock<IConfigurationWrapper> _configWrapper;
        private IPledgeRepository _fixture;

        [SetUp]
        public void SetUp()
        {
            const int mockPledgesPageId = 9876;

            _ministryPlatformService = new Mock<IMinistryPlatformService>();
            _ministryPlatformRestRepository = new Mock<IMinistryPlatformRestRepository>();
            _authService = new Mock<IAuthenticationRepository>();
            _configWrapper = new Mock<IConfigurationWrapper>();

            _configWrapper.Setup(m => m.GetEnvironmentVarAsString("API_USER")).Returns("uid");
            _configWrapper.Setup(m => m.GetEnvironmentVarAsString("API_PASSWORD")).Returns("pwd");
            _authService.Setup(m => m.AuthenticateClient(It.IsAny<string>(), It.IsAny<string>())).Returns(new AuthToken
            {
                AccessToken = "ABC",
                ExpiresIn = 123
            });
            _configWrapper.Setup(m => m.GetConfigIntValue("Pledges")).Returns(mockPledgesPageId);
            _configWrapper.Setup(mocked => mocked.GetConfigIntValue("MyHouseholdPledges")).Returns(525);

            _fixture = new PledgeRepository(_ministryPlatformService.Object, _ministryPlatformRestRepository.Object, _authService.Object, _configWrapper.Object);
        }

        [Test]
        public void CreatePledgeRecordTest()
        {
            const int mockPageId = 9876;
            const int mockDonorId = 11111;
            const int mockCampaignId = 22222;
            const int mockPledgeAmount = 259;
            const int expectedPledgeId = 676767;

            _ministryPlatformService.Setup(mocked => mocked.CreateRecord(
                It.IsAny<int>(),
                It.IsAny<Dictionary<string, object>>(),
                It.IsAny<string>(),
                true)).Returns(expectedPledgeId);

            var response = _fixture.CreatePledge(mockDonorId, mockCampaignId, mockPledgeAmount);

            _ministryPlatformService.Verify(mocked => mocked.CreateRecord(mockPageId, It.IsAny<Dictionary<string, object>>(), It.IsAny<string>(), true));

            Assert.AreEqual(response, expectedPledgeId);
        }

        [Test]
        public void GetPledgesForAuthUserTest()
        {
            const int _myHouseholdPledges = 525;

            var records = new List<Dictionary<string, object>>
            {
                new Dictionary<string, object>
                {
                    {"Pledge_ID", 123},
                    {"Pledge_Campaign_ID", 678},
                    {"Pledge_Status", "Active"},
                    {"Donor_ID", 432},
                    {"Campaign_Name", "Winners Win"},
                    {"Total_Pledge", 1000.00M},
                    {"Donation_Total", 125.00M},
                    {"Display_Name", " Dr McSteamy"},
                    {"Start_Date", DateTime.Now.Date},
                    {"End_Date", DateTime.Now.AddYears(5).Date},
                    {"Pledge_Campaign_Type_ID", 987},
                    {"Campaign_Type", "campaign 1"}
                },
                new Dictionary<string, object>
                {
                    {"Pledge_ID", 321},
                    {"Pledge_Campaign_ID", 876},
                    {"Pledge_Status", "Active"},
                    {"Donor_ID", 111},
                    {"Campaign_Name", "Chartreuse Caboose"},
                    {"Total_Pledge", 3000.00M},
                    {"Donation_Total", 1215.00M},
                    {"Display_Name", "Ice Creamy"},
                    {"Start_Date", DateTime.Now.AddMonths(10).Date},
                    {"End_Date", DateTime.Now.AddYears(3).Date},
                    {"Pledge_Campaign_Type_ID", 654},
                    {"Campaign_Type", "campaign 2"}
                }
            };

            _ministryPlatformService.Setup(mocked => mocked.GetRecordsDict(_myHouseholdPledges, It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>())).Returns(records);
            var record = _fixture.GetPledgesForAuthUser("iamausertoken");

            _ministryPlatformService.VerifyAll();

            Assert.IsNotNull(record);
            Assert.AreEqual(records[0]["Pledge_ID"], record[0].PledgeId);
            Assert.AreEqual(records[0]["Pledge_Campaign_ID"], record[0].PledgeCampaignId);
            Assert.AreEqual(records[0]["Pledge_Status"], record[0].PledgeStatus);
            Assert.AreEqual(records[0]["Campaign_Name"], record[0].CampaignName);
            Assert.AreEqual(records[0]["Total_Pledge"], record[0].PledgeTotal);
            Assert.AreEqual(records[0]["Donation_Total"], record[0].PledgeDonations);
            Assert.AreEqual(records[0]["Start_Date"], record[0].CampaignStartDate);
            Assert.AreEqual(records[0]["End_Date"], record[0].CampaignEndDate);
            Assert.AreEqual(records[0]["Pledge_Campaign_Type_ID"], record[0].CampaignTypeId);
            Assert.AreEqual(records[0]["Campaign_Type"], record[0].CampaignTypeName);

            Assert.AreEqual(records[1]["Pledge_ID"], record[1].PledgeId);
            Assert.AreEqual(records[1]["Pledge_Campaign_ID"], record[1].PledgeCampaignId);
            Assert.AreEqual(records[1]["Pledge_Status"], record[1].PledgeStatus);
            Assert.AreEqual(records[1]["Campaign_Name"], record[1].CampaignName);
            Assert.AreEqual(records[1]["Total_Pledge"], record[1].PledgeTotal);
            Assert.AreEqual(records[1]["Donation_Total"], record[1].PledgeDonations);
            Assert.AreEqual(records[1]["Start_Date"], record[1].CampaignStartDate);
            Assert.AreEqual(records[1]["End_Date"], record[1].CampaignEndDate);
            Assert.AreEqual(records[1]["Pledge_Campaign_Type_ID"], record[1].CampaignTypeId);
            Assert.AreEqual(records[1]["Campaign_Type"], record[1].CampaignTypeName);
        }

        [Test]
        public void ShouldGetPledgeByCampaignAndContact()
        {
            const int mockCampaign = 1;
            const int mockContact = 10;

            var mockColumns = new List<string>
            {
                "Pledges.Pledge_ID",
                "Donor_ID_Table.Donor_ID",
                "Pledge_Campaign_ID_Table.Pledge_Campaign_ID",
                "Pledge_Campaign_ID_Table.Campaign_Name",
                "Pledge_Campaign_ID_Table_Pledge_Campaign_Type_ID_Table.Pledge_Campaign_Type_ID",
                "Pledge_Campaign_ID_Table_Pledge_Campaign_Type_ID_Table.Campaign_Type",
                "Pledge_Campaign_ID_Table.Start_Date",
                "Pledge_Campaign_ID_Table.End_Date",
                "Pledge_Status_ID_Table.Pledge_Status_ID",
                "Pledge_Status_ID_Table.Pledge_Status",
                "Pledges.Total_Pledge"
            };

            var mockPledge = new MpPledge
            {
                CampaignName = "Kernel Panic",
                DonorId = 1234,
                PledgeTotal = 1000000,
                CampaignEndDate = DateTime.Today.AddDays(30),
                CampaignStartDate = DateTime.Today,
                CampaignTypeId = 4,
                CampaignTypeName = "Band Tour Fund",
                PledgeCampaignId = 567,
                PledgeDonations = 15.00M,
                PledgeId = 42,
                PledgeStatus = "Active",
                PledgeStatusId = 1
            };
            var mockPledges = new List<MpPledge> {mockPledge};

            _ministryPlatformRestRepository.Setup(mocked => mocked.UsingAuthenticationToken("ABC")).Returns(_ministryPlatformRestRepository.Object);
            _ministryPlatformRestRepository.Setup(
                mocked =>
                    mocked.Search<MpPledge>(
                        "Donor_ID_Table_Contact_ID_Table.Contact_ID=" + mockContact + " AND Pledge_Campaign_ID_Table.Pledge_Campaign_ID=" + mockCampaign +
                        " AND Pledge_Status_ID_Table.Pledge_Status_ID=1",
                        mockColumns, It.IsAny<string>(), It.IsAny<bool>())).Returns(mockPledges);

            var record =_fixture.GetPledgeByCampaignAndContact(mockCampaign, mockContact);
            Assert.AreEqual(mockPledge.CampaignName, record.CampaignName);
            Assert.AreEqual(mockPledge.DonorId, record.DonorId);
            Assert.AreEqual(mockPledge.PledgeTotal, record.PledgeTotal);
            Assert.AreEqual(mockPledge.CampaignEndDate, record.CampaignEndDate);
            Assert.AreEqual(mockPledge.CampaignStartDate, record.CampaignStartDate);
            Assert.AreEqual(mockPledge.CampaignTypeId, record.CampaignTypeId);
            Assert.AreEqual(mockPledge.CampaignTypeName, record.CampaignTypeName);
            Assert.AreEqual(mockPledge.PledgeCampaignId, record.PledgeCampaignId);
            Assert.AreEqual(mockPledge.PledgeDonations, record.PledgeDonations);
            Assert.AreEqual(mockPledge.PledgeId, record.PledgeId);
            Assert.AreEqual(mockPledge.PledgeStatus, record.PledgeStatus);
            Assert.AreEqual(mockPledge.PledgeStatusId, record.PledgeStatusId);
        }

        [Test]
        public void ShouldGetPledgesByCampaign()
        {

            const int mockCampaign = 1;
            var mockColumns = new List<string>
            {
                "Pledges.Pledge_ID",
                "Donor_ID_Table.Donor_ID",
                "Pledge_Campaign_ID_Table.Pledge_Campaign_ID",
                "Pledge_Campaign_ID_Table.Campaign_Name",
                "Pledge_Campaign_ID_Table_Pledge_Campaign_Type_ID_Table.Pledge_Campaign_Type_ID",
                "Pledge_Campaign_ID_Table_Pledge_Campaign_Type_ID_Table.Campaign_Type",
                "Pledge_Campaign_ID_Table.Start_Date",
                "Pledge_Campaign_ID_Table.End_Date",
                "Pledge_Status_ID_Table.Pledge_Status_ID",
                "Pledge_Status_ID_Table.Pledge_Status",
                "Pledges.Total_Pledge"
            };

            var mockPledge = new MpPledge
            {
                CampaignName = "Kernel Panic",
                DonorId = 1234,
                PledgeTotal = 1000000,
                CampaignEndDate = DateTime.Today.AddDays(30),
                CampaignStartDate = DateTime.Today,
                CampaignTypeId = 4,
                CampaignTypeName = "Band Tour Fund",
                PledgeCampaignId = 567,
                PledgeDonations = 15.00M,
                PledgeId = 42,
                PledgeStatus = "Active",
                PledgeStatusId = 1
            };
            var mockPledges = new List<MpPledge> { mockPledge };

            _ministryPlatformRestRepository.Setup(mocked => mocked.UsingAuthenticationToken("ABC")).Returns(_ministryPlatformRestRepository.Object);
            _ministryPlatformRestRepository.Setup(
                mocked =>
                    mocked.Search<MpPledge>(
                        "Pledge_Campaign_ID_Table.Pledge_Campaign_ID=" + mockCampaign +
                        " AND Pledge_Status_ID_Table.Pledge_Status_ID=1",
                        mockColumns, It.IsAny<string>(), It.IsAny<bool>())).Returns(mockPledges);

            List<MpPledge> records = _fixture.GetPledgesByCampaign(mockCampaign, "ABC");
            _ministryPlatformRestRepository.VerifyAll();
            Assert.AreEqual(1, records.Count);
        }

        [Test]
        public void ShouldGetPledgesByCampaignEmpty()
        {

            const int mockCampaign = 1;
            var mockColumns = new List<string>
            {
                "Pledges.Pledge_ID",
                "Donor_ID_Table.Donor_ID",
                "Pledge_Campaign_ID_Table.Pledge_Campaign_ID",
                "Pledge_Campaign_ID_Table.Campaign_Name",
                "Pledge_Campaign_ID_Table_Pledge_Campaign_Type_ID_Table.Pledge_Campaign_Type_ID",
                "Pledge_Campaign_ID_Table_Pledge_Campaign_Type_ID_Table.Campaign_Type",
                "Pledge_Campaign_ID_Table.Start_Date",
                "Pledge_Campaign_ID_Table.End_Date",
                "Pledge_Status_ID_Table.Pledge_Status_ID",
                "Pledge_Status_ID_Table.Pledge_Status",
                "Pledges.Total_Pledge"
            };

            
            var mockPledges = new List<MpPledge>();

            _ministryPlatformRestRepository.Setup(mocked => mocked.UsingAuthenticationToken("ABC")).Returns(_ministryPlatformRestRepository.Object);
            _ministryPlatformRestRepository.Setup(
                mocked =>
                    mocked.Search<MpPledge>(
                        "Pledge_Campaign_ID_Table.Pledge_Campaign_ID=" + mockCampaign +
                        " AND Pledge_Status_ID_Table.Pledge_Status_ID=1",
                        mockColumns, It.IsAny<string>(), It.IsAny<bool>())).Returns(mockPledges);

            List<MpPledge> records = _fixture.GetPledgesByCampaign(mockCampaign, "ABC");
            _ministryPlatformRestRepository.VerifyAll();
            Assert.AreEqual(0, records.Count);
        }

        [Test]
        public void ShouldGetPledgesByCampaignAndHandleException()
        {

            const int mockCampaign = 1;
            var mockColumns = new List<string>
            {
                "Pledges.Pledge_ID",
                "Donor_ID_Table.Donor_ID",
                "Pledge_Campaign_ID_Table.Pledge_Campaign_ID",
                "Pledge_Campaign_ID_Table.Campaign_Name",
                "Pledge_Campaign_ID_Table_Pledge_Campaign_Type_ID_Table.Pledge_Campaign_Type_ID",
                "Pledge_Campaign_ID_Table_Pledge_Campaign_Type_ID_Table.Campaign_Type",
                "Pledge_Campaign_ID_Table.Start_Date",
                "Pledge_Campaign_ID_Table.End_Date",
                "Pledge_Status_ID_Table.Pledge_Status_ID",
                "Pledge_Status_ID_Table.Pledge_Status",
                "Pledges.Total_Pledge"
            };

            var mockPledge = new MpPledge
            {
                CampaignName = "Kernel Panic",
                DonorId = 1234,
                PledgeTotal = 1000000,
                CampaignEndDate = DateTime.Today.AddDays(30),
                CampaignStartDate = DateTime.Today,
                CampaignTypeId = 4,
                CampaignTypeName = "Band Tour Fund",
                PledgeCampaignId = 567,
                PledgeDonations = 15.00M,
                PledgeId = 42,
                PledgeStatus = "Active",
                PledgeStatusId = 1
            };
            var mockPledges = new List<MpPledge> { mockPledge };

            _ministryPlatformRestRepository.Setup(mocked => mocked.UsingAuthenticationToken("ABC")).Returns(_ministryPlatformRestRepository.Object);
            _ministryPlatformRestRepository.Setup(
                mocked =>
                    mocked.Search<MpPledge>(
                        "Pledge_Campaign_ID_Table.Pledge_Campaign_ID=" + mockCampaign +
                        " AND Pledge_Status_ID_Table.Pledge_Status_ID=1",
                        mockColumns, It.IsAny<string>(), It.IsAny<bool>())).Throws<Exception>();

            Assert.Throws<Exception>(() =>
            {
                List<MpPledge> records = _fixture.GetPledgesByCampaign(mockCampaign, "ABC");
                _ministryPlatformRestRepository.VerifyAll();
            });
           
        }

    }
}
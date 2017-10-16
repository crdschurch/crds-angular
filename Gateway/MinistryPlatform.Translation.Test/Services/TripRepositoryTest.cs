using System.Collections.Generic;
using System.Linq;
using Crossroads.Web.Common.Configuration;
using Crossroads.Web.Common.MinistryPlatform;
using MinistryPlatform.Translation.Models;
using MinistryPlatform.Translation.Repositories;
using MinistryPlatform.Translation.Repositories.Interfaces;
using Moq;
using NUnit.Framework;

namespace MinistryPlatform.Translation.Test.Services
{
    [TestFixture]
    public class TripRepositoryTest
    {
        private readonly Mock<IMinistryPlatformRestRepository> _ministryPlatformRest;
        private readonly Mock<IConfigurationWrapper> _configurationWrapper;
        private readonly ITripRepository _fixture;

        private const string token = "some garbage token";
        private const string storedProc = "api_crds_Add_As_TripParticipant";

        public TripRepositoryTest()
        {
            _ministryPlatformRest = new Mock<IMinistryPlatformRestRepository>();
            _configurationWrapper = new Mock<IConfigurationWrapper>();
            _ministryPlatformRest.Setup(m => m.UsingAuthenticationToken(token)).Returns(_ministryPlatformRest.Object);
            _fixture = new TripRepository(_ministryPlatformRest.Object, _configurationWrapper.Object);
        }

        [SetUp]
        public void Setup()
        {
            _configurationWrapper.Setup(m => m.GetConfigValue("TripParticipantStoredProc")).Returns(storedProc);
        }

        [Test]
        public void ShouldAddAsTripParticipant()
        {

            var lists = ValidMpPledgeList();

            var values = new Dictionary<string, object>
            {
                {"@PledgeCampaignID", 12345},
                {"@ContactId", 433334}
            };
            _ministryPlatformRest.Setup(m => m.GetFromStoredProc<MpPledge>(storedProc, values)).Returns(lists);
            var returnVal = _fixture.AddAsTripParticipant(433334, 12345, token);

            Assert.AreEqual(lists[0].FirstOrDefault(), returnVal.Value);
            Assert.AreEqual(true, returnVal.Status);
            _ministryPlatformRest.VerifyAll();
        }

        [Test]
        public void ShouldNotAddAsParticipant()
        {
            var values = new Dictionary<string, object>
            {
                {"@PledgeCampaignID", 12345},
                {"@ContactId", 433334}
            };

            var lists = new List<List<MpPledge>>();

            _ministryPlatformRest.Setup(m => m.GetFromStoredProc<MpPledge>(storedProc, values)).Returns(lists);
            var returnVal = _fixture.AddAsTripParticipant(433334, 12345, token);

            Assert.AreEqual(false, returnVal.Status);
            Assert.IsNotEmpty(returnVal.ErrorMessage);
            _ministryPlatformRest.VerifyAll();
        }

        [Test]
        public void ShouldGetTripDocuments()
        {
            var mockDocs = new List<MpEventParticipantDocument>
            {
                new MpEventParticipantDocument
                {
                    EventParticipantDocumentId = 1,
                    DocumentId = 10,
                    EventParticipantId = 1234,
                    Received = true
                },
                new MpEventParticipantDocument
                {
                    EventParticipantDocumentId = 2,
                    DocumentId = 12,
                    EventParticipantId = 1234,
                    Received = false
                }
            };
            var eventParticipant = 1234;
            var searchString = $"cr_EventParticipant_Documents.Event_Participant_ID = {eventParticipant}";
            var columns = "EventParticipant_Document_ID, cr_EventParticipant_Documents.Event_Participant_ID, Document_ID, Received, cr_EventParticipant_Documents.Notes, Event_Participant_ID_Table_Event_ID_Table.Event_Title";

            _ministryPlatformRest.Setup(m => m.Search<MpEventParticipantDocument>(searchString, columns, null, false)).Returns(mockDocs);

            var result = _fixture.GetTripDocuments(eventParticipant, token);
            Assert.AreEqual(2, result.Count);
            _ministryPlatformRest.VerifyAll();
        }

        [Test]
        public void ShouldUpdateTripDocuments()
        {
            var mockDoc = new MpEventParticipantDocument
                {
                    EventParticipantDocumentId = 1,
                    DocumentId = 10,
                    EventParticipantId = 1234,
                    Received = true
                };
            _ministryPlatformRest.Setup(m => m.Update(mockDoc, null as string));

            var result = _fixture.ReceiveTripDocument(mockDoc, token);
            _ministryPlatformRest.VerifyAll();
        }

        private static List<List<MpPledge>> ValidMpPledgeList()
        {
            var mpPledge = new MpPledge
            {
                CampaignName = "asdfasdf",
                DonorId = 1234,
                PledgeId = 9087
            };

            return new List<List<MpPledge>>
            {
                new List<MpPledge>
                {
                    mpPledge
                }
            };
        }

    }
}

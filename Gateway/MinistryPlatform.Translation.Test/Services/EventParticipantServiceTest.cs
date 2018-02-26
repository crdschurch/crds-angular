using System;
using System.Collections.Generic;
using Crossroads.Utilities;
using Crossroads.Utilities.FunctionalHelpers;
using Crossroads.Utilities.Interfaces;
using FsCheck;
using Crossroads.Web.Common;
using Crossroads.Web.Common.Configuration;
using Crossroads.Web.Common.MinistryPlatform;
using Crossroads.Web.Common.Security;
using MinistryPlatform.Translation.Models;
using MinistryPlatform.Translation.Repositories;
using MinistryPlatform.Translation.Repositories.Interfaces;
using MinistryPlatform.Translation.Test.Helpers;
using Moq;
using NUnit.Framework;

namespace MinistryPlatform.Translation.Test.Services
{
    public class EventParticipantServiceTest
    {
        private EventParticipantRepository _fixture;
        private Mock<IMinistryPlatformService> _ministryPlatformService;
        private Mock<IMinistryPlatformRestRepository> _ministryPlatformRest;
        private Mock<IAuthenticationRepository> _authService;
        private Mock<IConfigurationWrapper> _configWrapper;

        [SetUp]
        public void Setup()
        {            
            Factories.EventParticipant();
            _ministryPlatformService = new Mock<IMinistryPlatformService>();
            _ministryPlatformRest = new Mock<IMinistryPlatformRestRepository>();
            _authService = new Mock<IAuthenticationRepository>();
            _configWrapper = new Mock<IConfigurationWrapper>();

            _authService.Setup(m => m.Authenticate(It.IsAny<string>(), It.IsAny<string>())).Returns(new AuthToken
            {
                AccessToken = "ABC",
                ExpiresIn = 123
            });

            _configWrapper.Setup(mocked => mocked.GetEnvironmentVarAsString("API_USER")).Returns("api-user");
            _configWrapper.Setup(mocked => mocked.GetEnvironmentVarAsString("API_PASSWORD")).Returns("api-password");
            _configWrapper.Setup(mocked => mocked.GetConfigIntValue("TripDestinationDocuments")).Returns(1234);

            _fixture = new EventParticipantRepository(_ministryPlatformService.Object, _ministryPlatformRest.Object, _authService.Object, _configWrapper.Object);
        }

        [Test]
        public void AddDocumentsToTripParticipantTest()
        {
            const int eventParticipantId = 9;
            var docs = new List<MpTripDocuments>
            {
                new MpTripDocuments
                {
                    DocumentId = 1,
                    Description = "doc 1 desc",
                    Document = "doc 1"
                },
                new MpTripDocuments
                {
                    DocumentId = 2,
                    Description = "doc 2 desc",
                    Document = "doc 2"
                }
            };

            _ministryPlatformService.Setup(m => m.CreateSubRecord("EventParticipantDocuments", eventParticipantId, It.IsAny<Dictionary<string, object>>(), It.IsAny<string>(), true))
                .Returns(34567);
            var returnVal = _fixture.AddDocumentsToTripParticipant(docs, eventParticipantId);

            _ministryPlatformService.VerifyAll();
            Assert.IsNotNull(returnVal);
            Assert.AreEqual(true, returnVal);
        }

        [Test]
        public void TestGetEventParticipants()
        {
            var p = new List<Dictionary<string, object>>
            {
                new Dictionary<string, object>
                {
                    {"Event_Participant_ID", Gen.Sample(7, 1, Gen.OneOf(Arb.Generate<int>())).HeadOrDefault},
                    {"Participant_ID", Gen.Sample(7, 1, Gen.OneOf(Arb.Generate<int>())).HeadOrDefault},
                    {"Participation_Status_ID", Gen.Sample(7, 1, Gen.OneOf(Arb.Generate<int>())).HeadOrDefault},
                    {"Room_ID", Gen.Sample(7, 1, Gen.OneOf(Arb.Generate<int>())).HeadOrDefault}
                },
                new Dictionary<string, object>
                {
                    {"Event_Participant_ID", Gen.Sample(7, 1, Gen.OneOf(Arb.Generate<int>())).HeadOrDefault},
                    {"Participant_ID", Gen.Sample(7, 1, Gen.OneOf(Arb.Generate<int>())).HeadOrDefault},
                    {"Participation_Status_ID", Gen.Sample(7, 1, Gen.OneOf(Arb.Generate<int>())).HeadOrDefault},
                    {"Room_ID", Gen.Sample(7, 1, Gen.OneOf(Arb.Generate<int>())).HeadOrDefault}
                }

            };

            _ministryPlatformService.Setup(mocked => mocked.GetSubpageViewRecords("EventParticipantAssignedToRoomApiSubPageView", 123, "ABC", ",,,\"987\"", "", 0)).Returns(p);
            var result = _fixture.GetEventParticipants(123, 987);

            _ministryPlatformService.VerifyAll();
            Assert.NotNull(result);
            Assert.AreEqual(p.Count, result.Count);
            for (var i = 0; i < p.Count; i++)
            {
                Assert.AreEqual(p[i]["Event_Participant_ID"], result[i].EventParticipantId);
                Assert.AreEqual(p[i]["Participant_ID"], result[i].ParticipantId);
                Assert.AreEqual(p[i]["Participation_Status_ID"], result[i].ParticipantStatus);
                Assert.AreEqual(p[i]["Room_ID"], result[i].RoomId);
            }
        }

        [Test]
        public void ShouldBeAnEventParticipant()
        {
            const int eventId = 12345;
            const int contactId = 587878745;
            const string apiToken = "letmein";
            var signupDate = new DateTime(2016, 10, 06);
            var expected = new List<MpEventParticipant>
            {
                new MpEventParticipant()
                {
                    EventParticipantId = 1232456,
                    SetupDate = signupDate
                }   
            };

            _configWrapper.Setup(m => m.GetConfigIntValue("Event_Participant_Status_ID_Cancelled")).Returns(5);
            var filter = $"Event_ID_Table.[Event_ID] = {eventId} AND Participant_ID_Table_Contact_ID_Table.[Contact_ID] = {contactId} AND Participation_Status_ID_Table.[Participation_Status_ID] <> 5";

            _ministryPlatformRest.Setup(m => m.UsingAuthenticationToken(apiToken)).Returns(_ministryPlatformRest.Object);
            _ministryPlatformRest.Setup(m => m.Search<MpEventParticipant>(filter, "Event_Participants.[Event_Participant_ID],Event_Participants.[_Setup_Date] as [Setup_Date]", (string) null, false)).Returns(expected);

            var result = _fixture.EventParticipantSignupDate(contactId, eventId, apiToken);
            Assert.IsNotNull(result);
            Assert.AreEqual(signupDate, result);
        }

        [Test]
        public void ShouldNotBeAnEventParticpant()
        {
            const int eventId = 12345;
            const int contactId = 587878745;
            const string apiToken = "letmein";
            var expected = new List<MpEventParticipant>();

            _configWrapper.Setup(m => m.GetConfigIntValue("Event_Participant_Status_ID_Cancelled")).Returns(5);

            var filter = $"Event_ID_Table.[Event_ID] = {eventId} AND Participant_ID_Table_Contact_ID_Table.[Contact_ID] = {contactId} AND Participation_Status_ID_Table.[Participation_Status_ID] <> 5";

            _ministryPlatformRest.Setup(m => m.UsingAuthenticationToken(apiToken)).Returns(_ministryPlatformRest.Object);
            _ministryPlatformRest.Setup(m => m.Search<MpEventParticipant>(filter, "Event_Participants.[Event_Participant_ID],Event_Participants.[_Setup_Date] as [Setup_Date]", (string)null, false)).Returns(expected);

            var result = _fixture.EventParticipantSignupDate(contactId, eventId, apiToken);
            Assert.IsNull(result);
        }

        [Test]
        public void ShouldFindEventParticipant()
        {
            const string token = "LETMEIN";
            const int contactId = 89898;
            const int eventId = 9876;
            var columns = new List<string>
            {
                "Participant_ID_Table_Contact_ID_Table.[Contact_ID]",
                "Event_ID_Table.[Event_ID]",
                "Event_Participant_ID",
                "Event_ID_Table.Event_Title",
                "Participation_Status_ID",
                "End_Date"
            };
            var eventParticipants = FactoryGirl.NET.FactoryGirl.Build<MpEventParticipant>();

            _ministryPlatformRest.Setup(m => m.UsingAuthenticationToken(token)).Returns(_ministryPlatformRest.Object);
            _ministryPlatformRest.Setup(
                m => m.Search<MpEventParticipant>($"Event_ID_Table.[Event_ID] = {eventId} AND Participant_ID_Table_Contact_ID_Table.[Contact_ID] = {contactId}", columns, null, false))
                .Returns(new List<MpEventParticipant>() {eventParticipants});

            var result = _fixture.GetEventParticipantByContactAndEvent(contactId, eventId, token);

            Assert.IsInstanceOf<Ok<MpEventParticipant>>(result);
            Assert.NotNull(result.Value);
            Assert.AreEqual(eventParticipants.EndDate, result.Value.EndDate);
        }

        [Test]
        public void ShouldHandleNoEventParticpant()
        {
            const string token = "LETMEIN";
            const int contactId = 89898;
            const int eventId = 9876;
            var columns = new List<string>
            {
                "Participant_ID_Table_Contact_ID_Table.[Contact_ID]",
                "Event_ID_Table.[Event_ID]",
                "Event_Participant_ID",
                "Event_ID_Table.Event_Title",
                "Participation_Status_ID",
                "End_Date"
            };
            _ministryPlatformRest.Setup(m => m.UsingAuthenticationToken(token)).Returns(_ministryPlatformRest.Object);
            _ministryPlatformRest.Setup(
                m => m.Search<MpEventParticipant>($"Event_ID_Table.[Event_ID] = {eventId} AND Participant_ID_Table_Contact_ID_Table.[Contact_ID] = {contactId}", columns, null, false))
                .Returns(new List<MpEventParticipant>());

            var result = _fixture.GetEventParticipantByContactAndEvent(contactId, eventId, token);

            Assert.IsInstanceOf<Err<MpEventParticipant>>(result);
            Assert.IsFalse(result.Status);            
        }

        [Test]
        public void ShouldHandleExceptionFindingEventParticipant()
        {
            const string token = "LETMEIN";
            const int contactId = 89898;
            const int eventId = 9876;
            var columns = new List<string>
            {
                "Participant_ID_Table_Contact_ID_Table.[Contact_ID]",
                "Event_ID_Table.[Event_ID]",
                "Event_Participant_ID",
                "Event_ID_Table.Event_Title",
                "Participation_Status_ID",
                "End_Date"
            };

            _ministryPlatformRest.Setup(m => m.UsingAuthenticationToken(token)).Returns(_ministryPlatformRest.Object);
            _ministryPlatformRest.Setup(
                m =>
                    m.Search<MpEventParticipant>($"Event_ID_Table.[Event_ID] = {eventId} AND Participant_ID_Table_Contact_ID_Table.[Contact_ID] = {contactId}",
                                                 columns,
                                                 null,
                                                 false))
                .Throws<Exception>();

            var result = _fixture.GetEventParticipantByContactAndEvent(contactId, eventId, token);

            Assert.IsInstanceOf<Err<MpEventParticipant>>(result);
            Assert.IsFalse(result.Status);
        }

        [Test]
        public void ShouldGetParticipantsByGender()
        {
            const string token = "ABC";
            const int eventId = 9876;
            const int genderId = 3;
            var searchString = $"Event_ID = {eventId} AND Participant_ID_Table_Contact_ID_Table_Gender_ID_Table.Gender_ID = {genderId} AND (End_Date IS NULL OR End_Date >= GETDATE())";
            const string column = "Count(*)";

            _ministryPlatformRest.Setup(m => m.UsingAuthenticationToken(token)).Returns(_ministryPlatformRest.Object);
            _ministryPlatformRest.Setup(m => m.Search<int>("Event_Participants", searchString, column, null, false)).Returns(42);

            var result = _fixture.GetEventParticipantCountByGender(eventId, genderId);

            Assert.NotNull(result);
            Assert.AreEqual(42 , result);
        }
    }
}
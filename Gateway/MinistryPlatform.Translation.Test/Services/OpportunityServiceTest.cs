using System;
using System.Collections.Generic;
using System.ServiceModel;
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
    [TestFixture]
    public class OpportunityServiceTest
    {
        private const int _signedupToServeSubPageViewId = 79;
        private const int _opportunityPageId = 348;
        private const int _eventPageId = 308;
        private const int _groupsParticipantsSubPageId = 88;

        private Mock<IMinistryPlatformService> _ministryPlatformService;
        private Mock<IAuthenticationRepository> _authenticationService;
        private Mock<IConfigurationWrapper> _configWrapper;
        private Mock<IParticipantRepository> _participantService;
        private Mock<IApiUserRepository> _apiUserService;
        private Mock<IMinistryPlatformRestRepository> _ministryPlatformRest;

        private OpportunityRepository _fixture;

        [SetUp]
        public void SetUp()
        {
            var now = DateTime.Now;
            _ministryPlatformService = new Mock<IMinistryPlatformService>();
            _authenticationService = new Mock<IAuthenticationRepository>();
            _configWrapper = new Mock<IConfigurationWrapper>();
            _participantService = new Mock<IParticipantRepository>();
            _apiUserService = new Mock<IApiUserRepository>();
            _ministryPlatformRest = new Mock<IMinistryPlatformRestRepository>();

        _configWrapper.Setup(m => m.GetEnvironmentVarAsString("API_USER")).Returns("uid");
            _configWrapper.Setup(m => m.GetEnvironmentVarAsString("API_PASSWORD")).Returns("pwd");
            _authenticationService.Setup(m => m.AuthenticateClient(It.IsAny<string>(), It.IsAny<string>())).Returns(new AuthToken
            {
                AccessToken = "ABC",
                ExpiresIn = 123
            });

            _fixture = new OpportunityRepository(_ministryPlatformService.Object,
                                                  _authenticationService.Object,
                                                  _configWrapper.Object,
                                                  _participantService.Object,
                                                  _ministryPlatformRest.Object,
                                                  _apiUserService.Object);
        }

        [Test]
        public void CreateOpportunityResponse()
        {
            const int opportunityId = 113;
            const string comment = "Test Comment";

            const int mockParticipantId = 7777;
            _participantService.Setup(m => m.GetParticipant(It.IsAny<int>()))
                .Returns(new MpParticipant {ParticipantId = mockParticipantId});

            const string opportunityResponsePageKey = "OpportunityResponses";
            _ministryPlatformService.Setup(
                m =>
                    m.CreateRecord(opportunityResponsePageKey,
                                   It.IsAny<Dictionary<string, object>>(),
                                   It.IsAny<string>(),
                                   true))
                .Returns(3333);

            Assert.DoesNotThrow(() => _fixture.RespondToOpportunity(opportunityId, comment, 123));

            _participantService.Verify(m => m.GetParticipant(It.IsAny<int>()), Times.Once);
            _ministryPlatformService.VerifyAll();
        }

        [Test]
        public void DeleteOpportunityResponse()
        {
            const int opportunityId = 113;
            const int mockParticipantId = 7777;
            //_authenticationService.Setup(m => m.GetParticipantRecord(It.IsAny<string>()))
            //    .Returns(new Participant { ParticipantId = mockParticipantId });

            const int responseResultId = 1234567;
            const int responseId = 1234;

            List<Dictionary<string, object>> responses = new List<Dictionary<string, object>>();
            Dictionary<string, object> response = new Dictionary<string, object>
            {
                {"dp_RecordID", responseId},
                {"Opportunity_ID", opportunityId},
                {"Participant_ID", mockParticipantId},
                {"Response_Result_ID", responseResultId}
            };
            responses.Add(response);

            _ministryPlatformService.Setup(
                m => m.DeleteRecord(It.IsAny<int>(), responseId, null, It.IsAny<string>()))
                .Returns(1);

            _ministryPlatformService.Setup(
                m =>
                    m.GetPageViewRecords("ResponseByOpportunityAndEvent", It.IsAny<string>(), It.IsAny<string>(), "", 0))
                .Returns(responses);

            Assert.DoesNotThrow(() => _fixture.DeleteResponseToOpportunities(mockParticipantId, opportunityId, 1));

            _ministryPlatformService.VerifyAll();
        }

        [Test]
        public void ShouldNotCreateOpportunityResponse()
        {
            const int opportunityId = 10000000;
            const string comment = "Fail Test Comment";
            const int mockParticipantId = 7777;
            _participantService.Setup(m => m.GetParticipant(It.IsAny<int>()))
                .Returns(new MpParticipant {ParticipantId = mockParticipantId});

            const string opportunityResponsePageKey = "OpportunityResponses";
            var exceptionDetail =
                new ExceptionDetail(new Exception("The creator of this fault did not specify a Reason."));
            var faultException = new FaultException<ExceptionDetail>(exceptionDetail);

            _ministryPlatformService.Setup(
                m =>
                    m.CreateRecord(opportunityResponsePageKey,
                                   It.IsAny<Dictionary<string, object>>(),
                                   It.IsAny<string>(),
                                   true))
                .Throws(faultException);

            Assert.Throws<FaultException<ExceptionDetail>>(
                () => _fixture.RespondToOpportunity( opportunityId, comment, 123));

            _participantService.Verify(m => m.GetParticipant(It.IsAny<int>()), Times.Once);
            _ministryPlatformService.VerifyAll();
        }

        [Test]
        public void GetOpportuntityResponseForContact()
        {
            const int contactId = 100;
            const int opportunityId = 900;
            const int subPageViewId = 76;
            var searchString = ",,,," + contactId;
            var mockResponse = new List<Dictionary<string, object>>
            {
                new Dictionary<string, object>
                {
                    {"dp_RecordID", 7},
                    {"Opportunity ID", 900},
                    {"Participant ID", 5},
                    {"Response Result ID", 1},
                    {"Response Date", "01/01/2015"}
                }
            };

            _ministryPlatformService.Setup(
                m => m.GetSubpageViewRecords(subPageViewId, opportunityId, It.IsAny<string>(), searchString, It.IsAny<string>(), It.IsAny<int>()))
                .Returns(mockResponse);

            var response = _fixture.GetOpportunityResponse(contactId, opportunityId);

            _ministryPlatformService.VerifyAll();

            Assert.IsNotNull(response);
            Assert.IsInstanceOf<MpResponse>((object)response);
            Assert.AreEqual(mockResponse[0]["dp_RecordID"], response.Response_ID);
        }

        [Test]
        public void GetOpportunityResponseSignUpYesTest()
        {
            //ARRANGE
            const int opportunityId = 2;
            const int eventId = 3;
            var participant = new MpParticipant {ParticipantId = 5};

            // mock _ministryPlatformService.GetPageViewRecords
            const string viewKey = "ResponseByOpportunityAndEvent";
            var searchString = string.Format(",{0},{1},{2}", opportunityId, eventId, participant.ParticipantId);
            const string sortString = "";
            var mockResult = MockDictionaryGetOpportunityResponseSignUpYesTest();
            _ministryPlatformService.Setup(
                m => m.GetPageViewRecords(viewKey, It.IsAny<string>(), searchString, sortString, 0)).Returns(mockResult);

            //ACT
            var response = _fixture.GetOpportunityResponse(opportunityId, eventId, participant);

            //ASSERT
            _ministryPlatformService.VerifyAll();

            Assert.IsNotNull(response);
            Assert.AreEqual(2, response.Opportunity_ID);
            Assert.AreEqual(5, response.Participant_ID);
            Assert.AreEqual(1, response.Response_Result_ID);
        }

        private List<Dictionary<string, object>> MockDictionaryGetOpportunityResponseSignUpYesTest()
        {
            var results = new List<Dictionary<string, object>>
            {
                new Dictionary<string, object>
                {
                    {"dp_RecordID", 7},
                    {"Opportunity_ID", 2},
                    {"Participant_ID", 5},
                    {"Response_Result_ID", 1}
                }
            };
            return results;
        }

        [Test]
        public void GetOpportunityResponseSignUpNoTest()
        {
            //ARRANGE
            const int opportunityId = 77;
            const int eventId = 3;
            var participant = new MpParticipant {ParticipantId = 777};

            // mock _ministryPlatformService.GetPageViewRecords
            const string viewKey = "ResponseByOpportunityAndEvent";
            var searchString = string.Format(",{0},{1},{2}", opportunityId, eventId, participant.ParticipantId);
            const string sortString = "";
            var mockResult = MockDictionaryGetOpportunityResponseSignUpNoTest();
            _ministryPlatformService.Setup(
                m => m.GetPageViewRecords(viewKey, It.IsAny<string>(), searchString, sortString, 0)).Returns(mockResult);

            //ACT
            var response = _fixture.GetOpportunityResponse(opportunityId, eventId, participant);

            //ASSERT
            _ministryPlatformService.VerifyAll();

            Assert.IsNotNull(response);
            Assert.AreEqual(77, response.Opportunity_ID);
            Assert.AreEqual(777, response.Participant_ID);
            Assert.AreEqual(2, response.Response_Result_ID);
        }

        private List<Dictionary<string, object>> MockDictionaryGetOpportunityResponseSignUpNoTest()
        {
            var results = new List<Dictionary<string, object>>
            {
                new Dictionary<string, object>
                {
                    {"dp_RecordID", 7},
                    {"Opportunity_ID", 77},
                    {"Participant_ID", 777},
                    {"Response_Result_ID", 2}
                }
            };
            return results;
        }

        [Test]
        public void GetOpportunityResponseNoExistingRsvpTest()
        {
            //ARRANGE
            const int opportunityId = 8;
            const int eventId = 3;
            var participant = new MpParticipant {ParticipantId = 5};

            // mock _ministryPlatformService.GetPageViewRecords
            const string viewKey = "ResponseByOpportunityAndEvent";
            var searchString = string.Format(",{0},{1},{2}", opportunityId, eventId, participant.ParticipantId);
            const string sortString = "";
            var mockResult = MockDictionaryGetOpportunityResponseNoExistingRsvpTest();
            _ministryPlatformService.Setup(
                m => m.GetPageViewRecords(viewKey, It.IsAny<string>(), searchString, sortString, 0)).Returns(mockResult);

            //ACT
            var response = _fixture.GetOpportunityResponse(opportunityId, eventId, participant);

            //ASSERT
            _ministryPlatformService.VerifyAll();

            Assert.IsNotNull(response);
            Assert.AreEqual(0, response.Opportunity_ID);
            Assert.AreEqual(0, response.Participant_ID);
            Assert.AreEqual(null, response.Response_Result_ID);
        }

        private List<Dictionary<string, object>> MockDictionaryGetOpportunityResponseNoExistingRsvpTest()
        {
            var results = new List<Dictionary<string, object>>();
            return results;
        }

        [Test]
        public void RespondToOpportunityForMultipleParticipants()
        {
            const string pageKey = "OpportunityResponses";
            var p1 = new Dictionary<string, object>
            {
                {"Response_Date", It.IsAny<DateTime>()},
                {"Opportunity_ID", 1},
                {"Participant_ID", 100},
                {"Closed", false},
                {"Comments", It.IsAny<string>()}
            };
            var token = It.IsAny<string>();
            _ministryPlatformService.Setup(m => m.CreateRecord(pageKey, p1, token, true)).Returns(27);

            var dto = new MpRespondToOpportunityDto {OpportunityId = 1, Participants = new List<int> {100, 200, 300}};
            Assert.DoesNotThrow(() => _fixture.RespondToOpportunity(dto));

            _ministryPlatformService.Verify(
                m =>
                    m.CreateRecord(It.IsAny<string>(), It.IsAny<Dictionary<string, object>>(), It.IsAny<string>(), true),
                Times.Exactly(3));
        }

        [Test]
        public void RespondToOpportunityAsLoggedInUserTest()
        {
            const int opportunityId = 9;
            const string comments = "test-comments";
            const int mockParticipantId = 7777;
            const string pageKey = "OpportunityResponses";

            _participantService.Setup(m => m.GetParticipant(It.IsAny<int>()))
                .Returns(new MpParticipant {ParticipantId = mockParticipantId});

            _ministryPlatformService.Setup(
                m => m.CreateRecord(pageKey, It.IsAny<Dictionary<string, object>>(), It.IsAny<string>(), true))
                .Returns(3333);

            var responseId = _fixture.RespondToOpportunity(opportunityId, comments, 123);

            _participantService.Verify(m => m.GetParticipant(It.IsAny<int>()), Times.Once);
            _ministryPlatformService.VerifyAll();

            Assert.IsNotNull(responseId);
            Assert.AreEqual(3333, responseId);
        }

        [Test]
        public void RespondToOpportunityAsParticipantId()
        {
            const string pageKey = "OpportunityResponses";
            const int participantId = 4444;
            const int opportunityId = 555;
            const string comment = "";
            const int eventId = 3333;
            const bool response = true;

            _ministryPlatformService.Setup(
                m =>
                    m.GetPageViewRecords("ResponseByOpportunityAndEvent", It.IsAny<string>(), It.IsAny<string>(), "", 0))
                .Returns(new List<Dictionary<string, object>>());
            _ministryPlatformService.Setup(
                m => m.CreateRecord(pageKey, It.IsAny<Dictionary<string, object>>(), It.IsAny<string>(), true))
                .Returns(4444);

            var responseId = _fixture.RespondToOpportunity(participantId, opportunityId, comment, eventId, response);

            _ministryPlatformService.VerifyAll();

            Assert.IsNotNull(responseId);
            Assert.AreEqual(0, responseId);
        }

        [Test]
        public void ShouldGetPeopleSignedUpTest()
        {
            const int opportunityId = 139;
            const int eventId = 950107;

            var signedupToServeResults = new List<Dictionary<string, object>>
            {
                new Dictionary<string, object>
                {
                    {"Response Date", 01/01/2001},
                    {"Display Name", "Test Group"},
                    {"Contact ID", 1},
                    {"Event Id", 99},
                    {"Event Title", "event-title"}
                },
                new Dictionary<string, object>
                {
                    {"Response Date", 01/01/2002},
                    {"Display Name", "Test Group2"},
                    {"Contact ID", 2},
                    {"Event Id", 102},
                    {"Event Title", "event-title2"}
                },
                new Dictionary<string, object>
                {
                    {"Response Date", 01/01/2003},
                    {"Display Name", "Test Group3"},
                    {"Contact ID", 3},
                    {"Event Id", 103},
                    {"Event Title", "event-title3"}
                }
            };

            var search = ",,," + eventId;
            _ministryPlatformService.Setup(mock =>
                                               mock.GetSubpageViewRecords(_signedupToServeSubPageViewId,
                                                                          opportunityId,
                                                                          It.IsAny<string>(),
                                                                          search,
                                                                          "",
                                                                          0))
                .Returns(signedupToServeResults);

            var response = _fixture.GetOpportunitySignupCount(opportunityId, eventId, It.IsAny<string>());

            Assert.IsNotNull(response);
            Assert.AreEqual(3, response);
        }

        [Test]
        public void ShouldGetAllEventDates()
        {
            const int opportunityId = 145;
            var today = DateTime.Today;

            var expectedEventType = new Dictionary<string, object>
            {
                {"Event_Type_ID_Text", "KC Nursery Oakley Sunday 8:30"}
            };
            var expectedEvents = new List<Dictionary<string, object>>
            {
                new Dictionary<string, object>
                {
                    {"Event_Start_Date", today.AddDays(-7)}
                },
                new Dictionary<string, object>
                {
                    {"Event_Start_Date", today}
                },
                new Dictionary<string, object>
                {
                    {"Event_Start_Date", today.AddDays(7)}
                }
            };
            var expectedDates = new List<DateTime>
            {
                today,
                today.AddDays(7)
            };

            _ministryPlatformService.Setup(
                mock => mock.GetRecordDict(_opportunityPageId, opportunityId, It.IsAny<string>(), false))
                .Returns(expectedEventType);
            _ministryPlatformService.Setup(
                mock =>
                    mock.GetRecordsDict(_eventPageId,
                                        It.IsAny<string>(),
                                        ",,KC Nursery Oakley Sunday 8:30",
                                        It.IsAny<string>()))
                .Returns(expectedEvents);

            var dates = _fixture.GetAllOpportunityDates(opportunityId);
            Assert.IsNotNull(dates);
            Assert.AreEqual(2, dates.Count);
            Assert.AreEqual(expectedDates, dates);
        }

        [Test]
        public void ShouldGetLastEventDate()
        {
            const int opportunityId = 145;

            var expectedEventType = new Dictionary<string, object>
            {
                {"Event_Type_ID_Text", "KC Nursery Oakley Sunday 8:30"}
            };
            var eventDate = DateTime.Today.AddDays(10);
            var expectedDateAsString = eventDate.ToString("MM/dd/yy hh:mmtt");
            var expectedEvents = new List<Dictionary<string, object>>
            {
                new Dictionary<string, object>
                {
                    {"Event_Start_Date", expectedDateAsString}
                }
            };
            var expectedLastDate = eventDate;

            _ministryPlatformService.Setup(
                mock => mock.GetRecordDict(_opportunityPageId, opportunityId, It.IsAny<string>(), false))
                .Returns(expectedEventType);
            _ministryPlatformService.Setup(
                mock =>
                    mock.GetRecordsDict(_eventPageId,
                                        It.IsAny<string>(),
                                        ",,KC Nursery Oakley Sunday 8:30",
                                        It.IsAny<string>()))
                .Returns(expectedEvents);

            var lastDate = _fixture.GetLastOpportunityDate(opportunityId);
            Assert.IsNotNull(lastDate);
            Assert.AreEqual(expectedLastDate, lastDate);
        }

        [Test]
        public void ShouldThrowExceptionGettingLastEventDateWhenNoEvents()
        {
            const int opportunityId = 145;

            var expectedEventType = new Dictionary<string, object>
            {
                {"Event_Type_ID_Text", "KC Nursery Oakley Sunday 8:30"}
            };
            var expectedEvents = new List<Dictionary<string, object>>();

            _ministryPlatformService.Setup(
                mock => mock.GetRecordDict(_opportunityPageId, opportunityId, It.IsAny<string>(), false))
                .Returns(expectedEventType);
            _ministryPlatformService.Setup(
                mock =>
                    mock.GetRecordsDict(_eventPageId,
                                        It.IsAny<string>(),
                                        ",,KC Nursery Oakley Sunday 8:30",
                                        It.IsAny<string>()))
                .Returns(expectedEvents);

            Assert.Throws<Exception>(() => _fixture.GetLastOpportunityDate(opportunityId));
        }

        [Test]
        public void ShouldReturnGroupParticipantsForOpportunity()
        {
            const int opportunityId = 145;

            var expectedOpportunity = new Dictionary<string, object>
            {
                {"dp_RecordID", 100},
                {"Opportunity Title", "Opportunity 100"},
                {"Event Type", "Event Type 100"},
                {"Event Type ID", 100},
                {"Role_Title", "Role Title 100"},
                {"Maximum_Needed", 100},
                {"Minimum_Needed", 50},
                {"Add_to_Group", 255},
                {"Add_to_Group_Text", "Test Group"},
                {"Group_Role_ID", 1},
                {"Event_Type_ID", 385}
            };

            var expectedParticipants = new List<Dictionary<string, object>>
            {
                new Dictionary<string, object>
                {
                    {"Contact_ID", 123},
                    {"Group_Role_ID", 1},
                    {"Role_Title", "Boss"},
                    {"Last_Name", "Garfunkel"},
                    {"Nickname", "Art"},
                    {"dp_RecordID", 12}
                },
                new Dictionary<string, object>
                {
                    {"Contact_ID", 456},
                    {"Group_Role_ID", 1},
                    {"Role_Title", "Boss"},
                    {"Last_Name", "Simon"},
                    {"Nickname", "Paul"},
                    {"dp_RecordID", 17}
                }
            };

            _ministryPlatformService.Setup(
                mock => mock.GetRecordDict(_opportunityPageId, opportunityId, It.IsAny<string>(), false))
                .Returns(expectedOpportunity);
            _ministryPlatformService.Setup(
                mock =>
                    mock.GetSubpageViewRecords(_groupsParticipantsSubPageId,
                                               255,
                                               It.IsAny<string>(),
                                               It.IsAny<string>(),
                                               It.IsAny<string>(),
                                               It.IsAny<int>())).Returns(expectedParticipants);

            var output = _fixture.GetGroupParticipantsForOpportunity(opportunityId);

            _ministryPlatformService.VerifyAll();
            Assert.AreEqual(255, output.GroupId);
            Assert.AreEqual("Test Group", output.Name);
            Assert.AreEqual(2, output.Participants.Count);
        }

        [Test]
        public void ShouldChangeRSVPFromYesToNo()
        {
            const int opportunityId = 9;
            const int eventId = 9234;
            const string comments = "test-comments";
            const int mockParticipantId = 7777;
            const int pageId = 382;


            // mock _ministryPlatformService.GetPageViewRecords
            const string viewKey = "ResponseByOpportunityAndEvent";
            var searchString = string.Format(",{0},{1},{2}", opportunityId, eventId, mockParticipantId);
            const string sortString = "";
            var mockResult = MockDictionaryGetOpportunityResponseSignUpYesTest();
            _ministryPlatformService.Setup(
                m => m.GetPageViewRecords(viewKey, It.IsAny<string>(), searchString, sortString, 0)).Returns(mockResult);

            _ministryPlatformService.Setup(
                m => m.UpdateRecord(pageId, It.IsAny<Dictionary<string, object>>(), It.IsAny<string>()));

            var responseId = _fixture.RespondToOpportunity(mockParticipantId, opportunityId, comments, eventId, false);

            _ministryPlatformService.VerifyAll();

            Assert.IsNotNull(responseId);
            Assert.AreEqual(7, responseId);
        }
    }
}
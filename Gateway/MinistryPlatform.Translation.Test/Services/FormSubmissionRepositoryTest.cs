using System;
using System.Collections.Generic;
using System.Data;
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
    public class FormSubmissionRepositoryTest
    {
        private Mock<IMinistryPlatformService> _ministryPlatformService;
        private Mock<IMinistryPlatformRestRepository> _ministryPlatformRestRepository;
        private Mock<IAuthenticationRepository> _authenticationRepository;
        private Mock<IDbConnection> _dpConnection;
        private Mock<IConfigurationWrapper> _configurationWrapper;   

        private IFormSubmissionRepository _fixture;

        private readonly int formResponsePageId = 399;
        private readonly int formAnswerPageId = 908;
        private readonly int allFormFieldsView = 999;
        private readonly int goTripFamilySignup = 111;
        private readonly string apiToken = "letmein";

        [SetUp]
        public void Setup()
        {
            _ministryPlatformService = new Mock<IMinistryPlatformService>();
            _ministryPlatformRestRepository = new Mock<IMinistryPlatformRestRepository>();
            _dpConnection = new Mock<IDbConnection>();
            _authenticationRepository = new Mock<IAuthenticationRepository>();
            _configurationWrapper = new Mock<IConfigurationWrapper>();         

            _fixture = new FormSubmissionRepository(_ministryPlatformService.Object, _dpConnection.Object, _authenticationRepository.Object, _configurationWrapper.Object, _ministryPlatformRestRepository.Object);

            _configurationWrapper.Setup(m => m.GetConfigIntValue("FormResponsePageId")).Returns(formResponsePageId);
            _configurationWrapper.Setup(m => m.GetConfigIntValue("FormAnswerPageId")).Returns(formAnswerPageId);
            _configurationWrapper.Setup(m => m.GetConfigIntValue("AllFormFieldsView")).Returns(allFormFieldsView);
            _configurationWrapper.Setup(m => m.GetConfigIntValue("GoTripFamilySignup")).Returns(goTripFamilySignup);
            _configurationWrapper.Setup(m => m.GetEnvironmentVarAsString("API_USER")).Returns("apiuser");
            _configurationWrapper.Setup(m => m.GetEnvironmentVarAsString("API_PASSWORD")).Returns("password");
            _authenticationRepository.Setup(m => m.Authenticate("apiuser", "password")).Returns(new AuthToken {AccessToken = apiToken});
        }

        [Test]
        public void ShouldCreateAFormResponseWithAnEventId()
        {
            const int recordId = 43;
            var formResponse = new MpFormResponse
            {
                ContactId = 123456,
                EventId = 8976,
                FormAnswers = new List<MpFormAnswer>(),
                FormId = 3333
            };

            var record = new Dictionary<string, object>
            {
                {"Form_ID", formResponse.FormId},
                {"Response_Date", DateTime.Now},
                {"Contact_ID", formResponse.ContactId},
                {"Opportunity_ID",  formResponse.OpportunityId },
                {"Opportunity_Response", formResponse.OpportunityResponseId},
                {"Pledge_Campaign_ID", formResponse.PledgeCampaignId},
                {"Event_ID", formResponse.EventId}
            };

            var searchString = $"Contact_ID={formResponse.ContactId} AND Form_ID={formResponse.FormId} AND Event_ID={formResponse.EventId}";
            const string selectColumns = "Form_Response_ID";
            _ministryPlatformRestRepository.Setup(m => m.UsingAuthenticationToken(apiToken)).Returns(_ministryPlatformRestRepository.Object);
            _ministryPlatformRestRepository.Setup(m => m.Search<MpFormResponse>(It.IsAny<string>(), It.IsAny<string>(), null, true)).Returns(( string filter, string column, string orderby, bool distinct) =>
            {
                Assert.AreEqual(searchString, filter);
                Assert.AreEqual(selectColumns, column);
                return new List<MpFormResponse>();
            });
            _ministryPlatformService.Setup(m => m.CreateRecord(It.IsAny<int>(), It.IsAny<Dictionary<string, object>>(), apiToken, true))
                .Returns((int formRId, Dictionary<string, object> data, string token, bool quickAdd) =>
            {
                Assert.AreEqual(record["Event_ID"], data["Event_ID"]);
                return recordId;
            });

            var result = _fixture.SubmitFormResponse(formResponse);

            _ministryPlatformService.VerifyAll();
      
            _authenticationRepository.VerifyAll();
            _ministryPlatformRestRepository.VerifyAll();

            Assert.AreEqual(recordId, result);
        }

        [Test]
        public void ShouldFindAFormResponseByEventId()
        {
            const int recordId = 43;           
            var formResponse = new MpFormResponse
            {
                ContactId = 123456,
                EventId = 8976,
                FormAnswers = new List<MpFormAnswer>(),
                FormId = 3333
            };

            var record = new Dictionary<string, object>
            {
                {"Form_ID", formResponse.FormId},
                {"Response_Date", DateTime.Now},
                {"Contact_ID", formResponse.ContactId},
                {"Opportunity_ID",  formResponse.OpportunityId },
                {"Opportunity_Response", formResponse.OpportunityResponseId},
                {"Pledge_Campaign_ID", formResponse.PledgeCampaignId},
                {"Event_ID", formResponse.EventId}
            };

            var searchString = $"Contact_ID={formResponse.ContactId} AND Form_ID={formResponse.FormId} AND Event_ID={formResponse.EventId}";
            const string selectColumns = "Form_Response_ID";
            _ministryPlatformRestRepository.Setup(m => m.UsingAuthenticationToken(apiToken)).Returns(_ministryPlatformRestRepository.Object);
            _ministryPlatformRestRepository.Setup(m => m.Search<MpFormResponse>(It.IsAny<string>(), It.IsAny<string>(), null, true)).Returns((string filter, string column, string orderby, bool distinct) =>
            {
                Assert.AreEqual(searchString, filter);
                Assert.AreEqual(selectColumns, column);
                formResponse.FormResponseId = 43;
                return new List<MpFormResponse>
                {
                    formResponse
                };
            });
            _ministryPlatformService.Setup(m => m.UpdateRecord(It.IsAny<int>(), It.IsAny<Dictionary<string, object>>(), apiToken));

            var result = _fixture.SubmitFormResponse(formResponse);

            _ministryPlatformService.VerifyAll();

            _authenticationRepository.VerifyAll();
            _ministryPlatformRestRepository.VerifyAll();

            Assert.AreEqual(recordId, result);
        }

        [Test]
        public void ShouldFindAFormResponseByEventIdANDPledgeCampaignId()
        {
            const int recordId = 43;
            var formResponse = new MpFormResponse
            {
                ContactId = 123456,
                EventId = 8976,
                FormAnswers = new List<MpFormAnswer>(),
                FormId = 3333,
                PledgeCampaignId = 34567
            };

            var record = new Dictionary<string, object>
            {
                {"Form_ID", formResponse.FormId},
                {"Response_Date", DateTime.Now},
                {"Contact_ID", formResponse.ContactId},
                {"Opportunity_ID",  formResponse.OpportunityId },
                {"Opportunity_Response", formResponse.OpportunityResponseId},
                {"Pledge_Campaign_ID", formResponse.PledgeCampaignId},
                {"Event_ID", formResponse.EventId}
            };

            var searchString = $"Contact_ID={formResponse.ContactId} AND Form_ID={formResponse.FormId} AND Event_ID={formResponse.EventId} AND Pledge_Campaign_ID={formResponse.PledgeCampaignId}";
            const string selectColumns = "Form_Response_ID";
            _ministryPlatformRestRepository.Setup(m => m.UsingAuthenticationToken(apiToken)).Returns(_ministryPlatformRestRepository.Object);
            _ministryPlatformRestRepository.Setup(m => m.Search<MpFormResponse>(It.IsAny<string>(), It.IsAny<string>(), null, true)).Returns((string filter, string column, string orderby, bool distinct) =>
            {
                Assert.AreEqual(searchString, filter);
                Assert.AreEqual(selectColumns, column);
                formResponse.FormResponseId = 43;
                return new List<MpFormResponse>
                {
                    formResponse
                };
            });
            _ministryPlatformService.Setup(m => m.UpdateRecord(It.IsAny<int>(), It.IsAny<Dictionary<string, object>>(), apiToken));

            var result = _fixture.SubmitFormResponse(formResponse);

            _ministryPlatformService.VerifyAll();
            _authenticationRepository.VerifyAll();
            _ministryPlatformRestRepository.VerifyAll();

            Assert.AreEqual(recordId, result);
        }

        [Test]
        public void ShouldFindAFormResponseByPledgeCampaignId()
        {
            const int recordId = 43;
            var formResponse = new MpFormResponse
            {
                ContactId = 123456,
                FormAnswers = new List<MpFormAnswer>(),
                FormId = 3333,
                PledgeCampaignId = 34567
            };

            var record = new Dictionary<string, object>
            {
                {"Form_ID", formResponse.FormId},
                {"Response_Date", DateTime.Now},
                {"Contact_ID", formResponse.ContactId},
                {"Opportunity_ID",  formResponse.OpportunityId },
                {"Opportunity_Response", formResponse.OpportunityResponseId},
                {"Pledge_Campaign_ID", formResponse.PledgeCampaignId},
                {"Event_ID", formResponse.EventId}
            };

            var searchString = $"Contact_ID={formResponse.ContactId} AND Form_ID={formResponse.FormId} AND Pledge_Campaign_ID={formResponse.PledgeCampaignId}";
            const string selectColumns = "Form_Response_ID";
            _ministryPlatformRestRepository.Setup(m => m.UsingAuthenticationToken(apiToken)).Returns(_ministryPlatformRestRepository.Object);
            _ministryPlatformRestRepository.Setup(m => m.Search<MpFormResponse>(It.IsAny<string>(), It.IsAny<string>(), null, true)).Returns((string filter, string column, string orderby, bool distinct) =>
            {
                Assert.AreEqual(searchString, filter);
                Assert.AreEqual(selectColumns, column);
                formResponse.FormResponseId = 43;
                return new List<MpFormResponse>
                {
                    formResponse
                };
            });
            _ministryPlatformService.Setup(m => m.UpdateRecord(It.IsAny<int>(), It.IsAny<Dictionary<string, object>>(), apiToken));

            var result = _fixture.SubmitFormResponse(formResponse);

            _ministryPlatformService.VerifyAll();
            _authenticationRepository.VerifyAll();
            _ministryPlatformRestRepository.VerifyAll();

            Assert.AreEqual(recordId, result);
        }

        [Test]
        public void ShouldFindAFormResponseByContactIdOnly()
        {
            const int recordId = 43;
            var formResponse = new MpFormResponse
            {
                ContactId = 123456,
                FormAnswers = new List<MpFormAnswer>(),
                FormId = 3333
            };

            var record = new Dictionary<string, object>
            {
                {"Form_ID", formResponse.FormId},
                {"Response_Date", DateTime.Now},
                {"Contact_ID", formResponse.ContactId},
                {"Opportunity_ID",  formResponse.OpportunityId },
                {"Opportunity_Response", formResponse.OpportunityResponseId},
                {"Pledge_Campaign_ID", formResponse.PledgeCampaignId},
                {"Event_ID", formResponse.EventId}
            };

            var searchString = $"Contact_ID={formResponse.ContactId} AND Form_ID={formResponse.FormId}";
            const string selectColumns = "Form_Response_ID";
            _ministryPlatformRestRepository.Setup(m => m.UsingAuthenticationToken(apiToken)).Returns(_ministryPlatformRestRepository.Object);
            _ministryPlatformRestRepository.Setup(m => m.Search<MpFormResponse>(It.IsAny<string>(), It.IsAny<string>(), null, true)).Returns((string filter, string column, string orderby, bool distinct) =>
            {
                Assert.AreEqual(searchString, filter);
                Assert.AreEqual(selectColumns, column);
                formResponse.FormResponseId = 43;
                return new List<MpFormResponse>
                {
                    formResponse
                };
            });
            _ministryPlatformService.Setup(m => m.UpdateRecord(It.IsAny<int>(), It.IsAny<Dictionary<string, object>>(), apiToken));

            var result = _fixture.SubmitFormResponse(formResponse);

            _ministryPlatformService.VerifyAll();
            _authenticationRepository.VerifyAll();
            _ministryPlatformRestRepository.VerifyAll();

            Assert.AreEqual(recordId, result);
        }

        [Test]
        public void ShouldGetFormResponseWhenPassedEventId()
        {
            const int formId = 333;
            const int contactId = 889899;
            const int eventId = 9098;
            const int responseId = 8876;

            var searchString = $"Form_ID={formId} AND Contact_ID={contactId} AND Event_ID={eventId}";
            var answersSearch = $"Form_Response_ID={responseId}";

            _ministryPlatformRestRepository.Setup(m => m.UsingAuthenticationToken(apiToken)).Returns(_ministryPlatformRestRepository.Object);
            _ministryPlatformRestRepository.Setup(m => m.Search<MpFormResponse>(searchString, null as string, null as string, false)).Returns(new List<MpFormResponse> {new MpFormResponse {FormResponseId = responseId } });
            _ministryPlatformRestRepository.Setup(m => m.Search<MpFormAnswer>(answersSearch, null as string, null as string, false)).Returns(new List<MpFormAnswer>());

            _fixture.GetFormResponse(formId, contactId, eventId);
            _ministryPlatformRestRepository.VerifyAll();
        }

        [Test]
        public void ShouldGetFormResponseWhenNotPassedEventId()
        {
            const int formId = 333;
            const int contactId = 889899;
            const int responseId = 8876;

            var searchString = $"Form_ID={formId} AND Contact_ID={contactId}";
            var answersSearch = $"Form_Response_ID={responseId}";

            _ministryPlatformRestRepository.Setup(m => m.UsingAuthenticationToken(apiToken)).Returns(_ministryPlatformRestRepository.Object);
            _ministryPlatformRestRepository.Setup(m => m.Search<MpFormResponse>(searchString, null as string, null as string, false)).Returns(new List<MpFormResponse> { new MpFormResponse { FormResponseId = responseId } });
            _ministryPlatformRestRepository.Setup(m => m.Search<MpFormAnswer>(answersSearch, null as string, null as string, false)).Returns(new List<MpFormAnswer>());

            _fixture.GetFormResponse(formId, contactId, null);
            _ministryPlatformRestRepository.VerifyAll();
        }

        [Test]
        public void ShouldThrowAnExceptionWhenGetFormResponseIsNull()
        {
            const int formId = 333;
            const int contactId = 889899;
            const int responseId = 8876;

            var searchString = $"Form_ID={formId} AND Contact_ID={contactId}";
            var answersSearch = $"Form_Response_ID={responseId}";

            _ministryPlatformRestRepository.Setup(m => m.UsingAuthenticationToken(apiToken)).Returns(_ministryPlatformRestRepository.Object);
            _ministryPlatformRestRepository.Setup(m => m.Search<MpFormResponse>(searchString, null as string, null as string, false)).Returns(new List<MpFormResponse> { (MpFormResponse) null });           

            Assert.Throws<ApplicationException>(() => _fixture.GetFormResponse(formId, contactId, null));
            _ministryPlatformRestRepository.VerifyAll();
        }

    }

}
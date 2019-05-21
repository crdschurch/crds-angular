using System;
using System.Collections.Generic;
using System.Linq;
using crds_angular.Exceptions;
using crds_angular.Models.Crossroads.Serve;
using crds_angular.Models.Crossroads.Trip;
using crds_angular.Services;
using crds_angular.Services.Interfaces;
using Crossroads.Utilities.FunctionalHelpers;
using Crossroads.Web.Common.Configuration;
using Crossroads.Web.Common.MinistryPlatform;
using MinistryPlatform.Translation.Models;
using MpCommunication = MinistryPlatform.Translation.Models.MpCommunication;
using MinistryPlatform.Translation.Repositories.Interfaces;
using Moq;
using NUnit.Framework;
using IDonationRepository = MinistryPlatform.Translation.Repositories.Interfaces.IDonationRepository;
using IEventRepository = MinistryPlatform.Translation.Repositories.Interfaces.IEventRepository;
using IGroupRepository = MinistryPlatform.Translation.Repositories.Interfaces.IGroupRepository;
using ICampaignRepository = MinistryPlatform.Translation.Repositories.Interfaces.ICampaignRepository;
using Crossroads.Web.Auth.Models;

namespace crds_angular.test.Services
{
    [TestFixture]
    public class TripServiceTest
    {
        private Mock<IEventParticipantRepository> _eventParticipantService;
        private Mock<IDonationRepository> _donationService;
        private Mock<IGroupRepository> _groupService;
        private Mock<IFormSubmissionRepository> _formSubmissionService;
        private Mock<IEventRepository> _eventService;
        private Mock<IPledgeRepository> _pledgeService;
        private Mock<ICampaignRepository> _campaignService;
        private Mock<IPrivateInviteRepository> _privateInviteService;
        private Mock<ICommunicationRepository> _communicationService;
        private Mock<IContactRepository> _contactService;
        private Mock<IContactRelationshipRepository> _contactRelationshipService;
        private Mock<IConfigurationWrapper> _configurationWrapper;
        private Mock<IPersonService> _personService;
        private Mock<IServeService> _serveService;
        private Mock<IProgramRepository> _programRepository;
        private Mock<IApiUserRepository> _apiUserReposity;
        private Mock<ITripRepository> _tripRepository;
        private Mock<IDonorRepository> _mpDonorRepositoryMock;

        private TripService _fixture;

        [SetUp]
        public void SetUp()
        {
            _eventParticipantService = new Mock<IEventParticipantRepository>();
            _donationService = new Mock<IDonationRepository>();
            _groupService = new Mock<IGroupRepository>();
            _formSubmissionService = new Mock<IFormSubmissionRepository>();
            _eventService = new Mock<IEventRepository>();
            _pledgeService = new Mock<IPledgeRepository>();
            _campaignService = new Mock<ICampaignRepository>();
            _privateInviteService = new Mock<IPrivateInviteRepository>();
            _communicationService = new Mock<ICommunicationRepository>();
            _contactService = new Mock<IContactRepository>();
            _contactRelationshipService = new Mock<IContactRelationshipRepository>();
            _configurationWrapper = new Mock<IConfigurationWrapper>();
            _personService = new Mock<IPersonService>();
            _serveService = new Mock<IServeService>();
            _programRepository = new Mock<IProgramRepository>();
            _tripRepository = new Mock<ITripRepository>();
            _apiUserReposity = new Mock<IApiUserRepository>();
            _mpDonorRepositoryMock = new Mock<IDonorRepository>();

            _fixture = new TripService(_eventParticipantService.Object,
                                       _donationService.Object,
                                       _groupService.Object,
                                       _formSubmissionService.Object,
                                       _eventService.Object,
                                       _pledgeService.Object,
                                       _campaignService.Object,
                                       _privateInviteService.Object,
                                       _communicationService.Object,
                                       _contactService.Object,
                                       _contactRelationshipService.Object,
                                       _configurationWrapper.Object,
                                       _personService.Object,
                                       _serveService.Object,
                                       _programRepository.Object,
                                       _apiUserReposity.Object,
                                       _tripRepository.Object,
                                       _mpDonorRepositoryMock.Object);
        }

        [Test]
        public void Search()
        {
            var mockMpSearchResponse = MockMpSearchResponse();
            _eventParticipantService.Setup(m => m.TripParticipants(It.IsAny<string>())).Returns(mockMpSearchResponse);

            var mockPledge1 = new MpPledge
            {
                PledgeId = 1,
                DonorId = mockMpSearchResponse[0].DonorId,
                PledgeCampaignId = mockMpSearchResponse[0].CampaignId,
                PledgeStatusId = 1
            };
            var mockPledge2 = new MpPledge
            {
                PledgeId = 2,
                DonorId = mockMpSearchResponse[1].DonorId,
                PledgeCampaignId = mockMpSearchResponse[1].CampaignId,
                PledgeStatusId = 1
            };
            var mockPledge3 = new MpPledge
            {
                PledgeId = 3,
                DonorId = mockMpSearchResponse[2].DonorId,
                PledgeCampaignId = mockMpSearchResponse[2].CampaignId,
                PledgeStatusId = 1
            };
            _pledgeService.Setup(m => m.GetPledgeByCampaignAndDonor(mockMpSearchResponse[0].CampaignId, mockMpSearchResponse[0].DonorId)).Returns(mockPledge1);
            _pledgeService.Setup(m => m.GetPledgeByCampaignAndDonor(mockMpSearchResponse[1].CampaignId, mockMpSearchResponse[1].DonorId)).Returns(mockPledge2);
            _pledgeService.Setup(m => m.GetPledgeByCampaignAndDonor(mockMpSearchResponse[2].CampaignId, mockMpSearchResponse[2].DonorId)).Returns(mockPledge3);

            var searchResults = _fixture.Search(It.IsAny<string>());

            _eventParticipantService.VerifyAll();
            _pledgeService.VerifyAll();
            Assert.AreEqual(2, searchResults.Count);

            var p1 = searchResults.FirstOrDefault(s => s.ParticipantId == 9999);
            Assert.IsNotNull(p1);
            Assert.AreEqual(2, p1.Trips.Count);

            var p2 = searchResults.FirstOrDefault(s => s.ParticipantId == 5555);
            Assert.IsNotNull(p2);
            Assert.AreEqual(1, p2.Trips.Count);
        }

        [Test]
        public void ShouldGetMyTrips()
        {
            AuthDTO token = fakeAuthDTO(1234, "example@example.com");
            const string apiToken = "apiToken";
            var mockFamily = new List<FamilyMember> { new FamilyMember { ContactId = 12345 }, new FamilyMember { ContactId = 98765 } };
            _serveService.Setup(m => m.GetImmediateFamilyParticipants(token.UserInfo.Mp.ContactId)).Returns(mockFamily);

            _apiUserReposity.Setup(m => m.GetDefaultApiClientToken()).Returns(apiToken);
            _configurationWrapper.Setup(m => m.GetConfigIntValue(It.IsAny<string>())).Returns(10);
            _donationService.Setup(m => m.GetMyTripDistributions(It.IsAny<int>())).Returns(MockTripDonationsResponse());
            _eventParticipantService.Setup(m => m.TripParticipants(It.IsAny<string>())).Returns(mockTripParticipants());
            _pledgeService.Setup(m => m.GetPledgeByCampaignAndDonor(It.IsAny<int>(), It.IsAny<int>())).Returns(mockPledge());
            _tripRepository.Setup(m => m.GetTripDocuments(It.IsAny<int>(), apiToken)).Returns(new List<MpEventParticipantDocument>());
            var myTrips = _fixture.GetMyTrips(token);

            _serveService.VerifyAll();
            _donationService.VerifyAll();
            _eventParticipantService.VerifyAll();

            Assert.IsNotNull(myTrips);
            Assert.AreEqual(2, myTrips.MyTrips.Count);
            Assert.AreEqual(2, myTrips.MyTrips[0].TripGifts.Count);
        }

        [Test]
        public void FundraisingDaysLeftShouldNotBeNegative()
        {

            AuthDTO token = fakeAuthDTO(1234, "example@example.com");
            const string apiToken = "apiToken";
            var mockFamily = new List<FamilyMember> { new FamilyMember { ContactId = 12345 } };
            _serveService.Setup(m => m.GetImmediateFamilyParticipants(token.UserInfo.Mp.ContactId)).Returns(mockFamily);

            _apiUserReposity.Setup(m => m.GetDefaultApiClientToken()).Returns(apiToken);
            _configurationWrapper.Setup(m => m.GetConfigIntValue(It.IsAny<string>())).Returns(10);
            _donationService.Setup(m => m.GetMyTripDistributions(It.IsAny<int>())).Returns(MockFundingPastTripDonationsResponse());
            _eventParticipantService.Setup(m => m.TripParticipants(It.IsAny<string>())).Returns(mockTripParticipants());
            _pledgeService.Setup(m => m.GetPledgeByCampaignAndDonor(It.IsAny<int>(), It.IsAny<int>())).Returns(mockPledge());
            _tripRepository.Setup(m => m.GetTripDocuments(It.IsAny<int>(), apiToken)).Returns(new List<MpEventParticipantDocument>());
            var myTrips = _fixture.GetMyTrips(token);

            Assert.IsNotNull(myTrips);
            Assert.AreEqual(0, myTrips.MyTrips[0].FundraisingDaysLeft);
        }

        [Test]
        public void CreateTripParticipant()
        {
            const int contactId = 3123;
            const int pledgeCampaignId = 09786834;
            const string token = "asdfasdf";
            var mockPledge = this.mockPledge();

            var tripPledgeDto = new Result<MpPledge>(true, mockPledge);

            _apiUserReposity.Setup(m => m.GetDefaultApiClientToken()).Returns(token);
            _tripRepository.Setup(m => m.AddAsTripParticipant(contactId, pledgeCampaignId, token)).Returns(tripPledgeDto);
            _fixture.CreateTripParticipant(contactId, pledgeCampaignId);

            _apiUserReposity.VerifyAll();
           _tripRepository.VerifyAll();
        }

        [Test]
        public void ShouldSendTripFullConfirmation()
        {
            
            const int pledgeCampaignId = 09786834;
            const string token = "asdfasdf";
            const int templateId = 7878;

            var campaign = mockPledgeCampaign(pledgeCampaignId);

            var pledges = mockPledges(campaign);
            pledges.Add(
                new MpPledge()
                {
                    CampaignName = campaign.Name,
                    CampaignStartDate = campaign.StartDate,
                    CampaignEndDate = campaign.EndDate,
                    CampaignTypeId = 1,
                    CampaignTypeName = campaign.Type,
                    DonorId = 3,
                    PledgeCampaignId = campaign.Id,
                    PledgeDonations = 1,
                    PledgeId = 4,
                    PledgeStatus = "active",
                    PledgeStatusId = 1,
                    PledgeTotal = 100
                }
            );

            var mergeData = new Dictionary<string, object>
            {
                {"Pledge_Campaign", campaign.Name}
            };

            var communication = new MpCommunication()
            {
                TemplateId = templateId,
                AuthorUserId = 1,
                DomainId = 1,
                EmailBody = "<p> Ok random body of text </p>",
                EmailSubject = "more randomness",
                FromContact = new MpContact() {ContactId = 5, EmailAddress = "updates@crossroads.net"},
                MergeData = mergeData,
                ReplyToContact = new MpContact {ContactId = 5, EmailAddress = "updates@crossroads.net"},
                StartDate = DateTime.Now,
                ToContacts = new List<MpContact> {new MpContact {ContactId = 45, EmailAddress = "asdf@asdf.com"}}
            };

            var eventDetails = EventDetails(campaign.EventId);

            _apiUserReposity.Setup(m => m.GetDefaultApiClientToken()).Returns(token);
            _campaignService.Setup(m => m.GetPledgeCampaign(pledgeCampaignId, token)).Returns(campaign);
            _pledgeService.Setup(m => m.GetPledgesByCampaign(pledgeCampaignId, token)).Returns(pledges);        

            _configurationWrapper.Setup(m => m.GetConfigIntValue("TripIsFullTemplateId")).Returns(templateId);
            _configurationWrapper.Setup(m => m.GetConfigIntValue("TripIsFullFromContactId")).Returns(5);
            _configurationWrapper.Setup(m => m.GetConfigValue("TripIsFullFromEmailAddress")).Returns("updates@crossroads.net");

            _eventService.Setup(m => m.GetEvent(campaign.EventId)).Returns(eventDetails);

            _communicationService.Setup(
                m => m.GetTemplateAsCommunication(templateId,
                                                  communication.FromContact.ContactId,
                                                  communication.FromContact.EmailAddress,
                                                  communication.FromContact.ContactId,
                                                  communication.FromContact.EmailAddress,
                                                  eventDetails.PrimaryContact.ContactId,
                                                  eventDetails.PrimaryContact.EmailAddress,
                                                  mergeData)).Returns(communication);


            _communicationService.Setup(m => m.SendMessage(communication, false)).Returns(1);

            _fixture.SendTripIsFullMessage(pledgeCampaignId);

            _apiUserReposity.VerifyAll();
            _tripRepository.VerifyAll();
            _configurationWrapper.VerifyAll();
            _communicationService.VerifyAll();
            _eventService.VerifyAll();
        }

        [Test]
        public void ShouldNotSendTripFullConfirmation()
        {
            const int pledgeCampaignId = 09786834;
            const string token = "asdfasdf";
            
            var campaign = mockPledgeCampaign(pledgeCampaignId);
            var pledges = mockPledges(campaign);
            
            _apiUserReposity.Setup(m => m.GetDefaultApiClientToken()).Returns(token);
            _campaignService.Setup(m => m.GetPledgeCampaign(pledgeCampaignId, token)).Returns(campaign);
            _pledgeService.Setup(m => m.GetPledgesByCampaign(pledgeCampaignId, token)).Returns(pledges);
            
            _fixture.SendTripIsFullMessage(pledgeCampaignId);

            _apiUserReposity.VerifyAll();
            _tripRepository.VerifyAll();
            _communicationService.Verify(m => m.SendMessage(It.IsAny<MpCommunication>(), It.IsAny<bool>()), Times.Never);
        }

        [Test]
        public void ThrowExceptionWhenCreateParticipantFails()
        {
            const int contactId = 3123;
            const int pledgeCampaignId = 09786834;
            const string token = "asdfasdf";

            var pledgeRes = new Result<MpPledge>(false, "Trip is Full");

            _apiUserReposity.Setup(m => m.GetDefaultApiClientToken()).Returns(token);
            _tripRepository.Setup(m => m.AddAsTripParticipant(contactId, pledgeCampaignId, token)).Returns(pledgeRes);
            Assert.Throws<TripFullException>(() => _fixture.CreateTripParticipant(contactId, pledgeCampaignId));

            _apiUserReposity.VerifyAll();
            _tripRepository.VerifyAll();
        }

        [Test]
        public void ShouldNotHaveScholarship()
        {
            const int contactId = 1234;
            const int pledgeCampaignId = 9876;

            _campaignService.Setup(m => m.GetPledgeCampaign(pledgeCampaignId)).Returns(mockPledgeCampaign());
            _donationService.Setup(m => m.GetMyTripDistributions(contactId)).Returns(MockTripDonationsResponse());

            var result = _fixture.HasScholarship(contactId, pledgeCampaignId);
            Assert.IsFalse(result);
        }

        [Test]
        public void ShouldHaveScholarship()
        {
            const int contactId = 1234;
            const int pledgeCampaignId = 9876;

            _campaignService.Setup(m => m.GetPledgeCampaign(pledgeCampaignId)).Returns(mockPledgeCampaign());
            _donationService.Setup(m => m.GetMyTripDistributions(contactId)).Returns(MockTripScholarshipDonationsResponse());

            var result = _fixture.HasScholarship(contactId, pledgeCampaignId);
            Assert.IsTrue(result);
        }

        [Test]
        public void TripShouldNotBeFull()
        {
            const int pledgeCampaignId = 12345456;
            const string apiToken = "980osjklhdfdf";

            var campaign = mockPledgeCampaign();



            _apiUserReposity.Setup(m => m.GetDefaultApiClientToken()).Returns(apiToken);
            _campaignService.Setup(m => m.GetPledgeCampaign(pledgeCampaignId, apiToken)).Returns(campaign);
            _pledgeService.Setup(m => m.GetPledgesByCampaign(pledgeCampaignId, apiToken)).Returns(mockPledges(campaign));

            var tripCampaign = _fixture.GetTripCampaign(pledgeCampaignId);

            _apiUserReposity.VerifyAll();
            _campaignService.VerifyAll();
            _pledgeService.VerifyAll();

            Assert.IsFalse(tripCampaign.IsFull);
        }

        [Test]
        public void TripShouldBeFull()
        {
            const int pledgeCampaignId = 12345456;
            const string apiToken = "980osjklhdfdf";

            var campaign = mockPledgeCampaign();

            var pledges = mockPledges(campaign);
            pledges.Add(
                new MpPledge()
                {
                    CampaignName = campaign.Name,
                    CampaignStartDate = campaign.StartDate,
                    CampaignEndDate = campaign.EndDate,
                    CampaignTypeId = 1,
                    CampaignTypeName = campaign.Type,
                    DonorId = 3,
                    PledgeCampaignId = campaign.Id,
                    PledgeDonations = 1,
                    PledgeId = 4,
                    PledgeStatus = "active",
                    PledgeStatusId = 1,
                    PledgeTotal = 100
                }
            );

            _apiUserReposity.Setup(m => m.GetDefaultApiClientToken()).Returns(apiToken);
            _campaignService.Setup(m => m.GetPledgeCampaign(pledgeCampaignId, apiToken)).Returns(campaign);
            _pledgeService.Setup(m => m.GetPledgesByCampaign(pledgeCampaignId, apiToken)).Returns(pledges);

            var tripCampaign = _fixture.GetTripCampaign(pledgeCampaignId);

            _apiUserReposity.VerifyAll();
            _campaignService.VerifyAll();
            _pledgeService.VerifyAll();

            Assert.IsTrue(tripCampaign.IsFull);


        }

        [Test]
        public void IsTripsFullShouldThrowException()
        {
            const int pledgeCampaignId = 12345456;
            const string apiToken = "980osjklhdfdf";

            var campaign = mockPledgeCampaign();

            var pledges = mockPledges(campaign);
            pledges.Add(
                new MpPledge()
                {
                    CampaignName = campaign.Name,
                    CampaignStartDate = campaign.StartDate,
                    CampaignEndDate = campaign.EndDate,
                    CampaignTypeId = 1,
                    CampaignTypeName = campaign.Type,
                    DonorId = 3,
                    PledgeCampaignId = campaign.Id,
                    PledgeDonations = 1,
                    PledgeId = 4,
                    PledgeStatus = "active",
                    PledgeStatusId = 1,
                    PledgeTotal = 100
                }
            );

            _apiUserReposity.Setup(m => m.GetDefaultApiClientToken()).Returns(apiToken);
            _campaignService.Setup(m => m.GetPledgeCampaign(pledgeCampaignId, apiToken)).Returns(campaign);
            _pledgeService.Setup(m => m.GetPledgesByCampaign(pledgeCampaignId, apiToken)).Throws<Exception>();

            Assert.Throws<Exception>(() =>
            {
                _fixture.GetTripCampaign(pledgeCampaignId);
                _apiUserReposity.VerifyAll();
                _campaignService.VerifyAll();
                _pledgeService.VerifyAll();
            });
        }

        [Test]
        public void ShouldNotBeFullIfMaxParticipantsIsNull()
        {
            const int pledgeCampaignId = 12345456;
            const string apiToken = "980osjklhdfdf";

            var campaign = mockPledgeCampaign();
            campaign.MaximumRegistrants = null;

            var pledges = mockPledges(campaign);
            pledges.Add(
                new MpPledge()
                {
                    CampaignName = campaign.Name,
                    CampaignStartDate = campaign.StartDate,
                    CampaignEndDate = campaign.EndDate,
                    CampaignTypeId = 1,
                    CampaignTypeName = campaign.Type,
                    DonorId = 3,
                    PledgeCampaignId = campaign.Id,
                    PledgeDonations = 1,
                    PledgeId = 4,
                    PledgeStatus = "active",
                    PledgeStatusId = 1,
                    PledgeTotal = 100
                }
            );

            _apiUserReposity.Setup(m => m.GetDefaultApiClientToken()).Returns(apiToken);
            _campaignService.Setup(m => m.GetPledgeCampaign(pledgeCampaignId, apiToken)).Returns(campaign);

            var tripcampaign = _fixture.GetTripCampaign(pledgeCampaignId);
            Assert.IsFalse(tripcampaign.IsFull);
            _apiUserReposity.VerifyAll();
            _campaignService.VerifyAll();
            _pledgeService.Verify(m => m.GetPledgesByCampaign(pledgeCampaignId, apiToken), Times.Never);
        }

        [Test]
        public void ShouldGetSignedIPromise()
        {
            const int eventParticipantId = 1234;
            const string token = "faketoken";
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

            _apiUserReposity.Setup(m => m.GetDefaultApiClientToken()).Returns(token);
            _configurationWrapper.Setup(m => m.GetConfigIntValue("IPromiseDocumentId")).Returns(10);
            _tripRepository.Setup(m => m.GetTripDocuments(eventParticipantId, token)).Returns(mockDocs);

            var result = _fixture.GetIPromise(eventParticipantId);
            Assert.IsTrue(result);
        }

        [Test]
        public void ShouldGetNotSignedIPromise()
        {
            const int eventParticipantId = 1234;
            const string token = "faketoken";
            var mockDocs = new List<MpEventParticipantDocument>
            {
                new MpEventParticipantDocument
                {
                    EventParticipantDocumentId = 1,
                    DocumentId = 11,
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

            _apiUserReposity.Setup(m => m.GetDefaultApiClientToken()).Returns(token);
            _configurationWrapper.Setup(m => m.GetConfigIntValue("IPromiseDocumentId")).Returns(10);
            _tripRepository.Setup(m => m.GetTripDocuments(eventParticipantId, token)).Returns(mockDocs);

            var result = _fixture.GetIPromise(eventParticipantId);
            Assert.IsFalse(result);
        }

        [Test]
        public void ShouldGetNotSignedIPromiseWhenNoDocs()
        {
            const int eventParticipantId = 1234;
            const string token = "faketoken";
            var mockDocs = new List<MpEventParticipantDocument>();

            _apiUserReposity.Setup(m => m.GetDefaultApiClientToken()).Returns(token);
            _configurationWrapper.Setup(m => m.GetConfigIntValue("IPromiseDocumentId")).Returns(10);
            _tripRepository.Setup(m => m.GetTripDocuments(eventParticipantId, token)).Returns(mockDocs);

            var result = _fixture.GetIPromise(eventParticipantId);
            Assert.IsFalse(result);
        }

        private MpEvent EventDetails(int eventId = 8)
        {
            return new MpEvent
            {
                PrimaryContact = new MpContact {ContactId = 5, EmailAddress = "updates@crossroads.net"}
            };
        }

        [Test]
        public void ShouldSendTripEmail()
        {
            const int formResponseId = 12345;
            const int contactId = 1234;
            const int pledgeCampaignId = 9876;

            var mycontact = new MpMyContact
            {
                Contact_ID = 7,
                Email_Address = "from@from.com"
            };
            

            var to = new MpContact
            {
                ContactId = 8,
                EmailAddress = "to@to.com"
            };

            var tolist = new List<MpContact> {to};
            
            var mpc = new MpCommunication
            {
                AuthorUserId = 1,
                DomainId = 1,
                EmailBody = "body",
                EmailSubject = "subject",
                FromContact = to,
                MergeData = new Dictionary<string, object>(),
                ReplyToContact = to,
                StartDate = new DateTime(2011, 10, 11),
                TemplateId = 444,
                ToContacts = tolist
            };

            _campaignService.Setup(m => m.GetPledgeCampaign(pledgeCampaignId)).Returns(mockPledgeCampaign());
            _donationService.Setup(m => m.GetMyTripDistributions(contactId)).Returns(MockTripScholarshipDonationsResponse());
            _formSubmissionService.Setup(f => f.SubmitFormResponse(It.IsAny<MpFormResponse>())).Returns(formResponseId);
            _communicationService.Setup(
                s =>
                    s.GetTemplateAsCommunication(It.IsAny<int>(),
                                                 It.IsAny<int>(),
                                                 It.IsAny<string>(),
                                                 It.IsAny<int>(),
                                                 It.IsAny<string>(),
                                                 It.IsAny<int>(),
                                                 It.IsAny<string>(),
                                                 It.IsAny<Dictionary<string, object>>())).Returns(mpc);

            _communicationService.Setup(s => s.SendMessage(mpc,false)).Returns(9999);
            _contactService.Setup(s => s.GetContactById(It.IsAny<int>())).Returns(mycontact);

            var result = _fixture.SaveApplication(mockTripApplication(contactId,pledgeCampaignId));

            Assert.IsTrue(result == formResponseId);
          
            _configurationWrapper.Verify(v => v.GetConfigIntValue("TripApplicantSuccessTemplate"), Times.Exactly(1));
        }

        [Test]
        public void ShouldSendComboEmail()
        {
            const int formResponseId = 12345;
            const int contactId = 1234;
            const int pledgeCampaignId = 9876;

            var mycontact = new MpMyContact
            {
                Contact_ID = 7,
                Email_Address = "from@from.com"
            };


            var to = new MpContact
            {
                ContactId = 8,
                EmailAddress = "to@to.com"
            };

            var tolist = new List<MpContact> { to };

            var mpc = new MpCommunication
            {
                AuthorUserId = 1,
                DomainId = 1,
                EmailBody = "body",
                EmailSubject = "subject",
                FromContact = to,
                MergeData = new Dictionary<string, object>(),
                ReplyToContact = to,
                StartDate = new DateTime(2011, 10, 11),
                TemplateId = 444,
                ToContacts = tolist
            };

            var program = new MpProgram
            {
                Name = "prog name",
                AllowRecurringGiving = false,
                ProgramId = 333,
                ProgramType = 3
            };

            _campaignService.Setup(m => m.GetPledgeCampaign(pledgeCampaignId)).Returns(mockPledgeCampaign());
            _donationService.Setup(m => m.GetMyTripDistributions(contactId)).Returns(MockTripDonationsResponse());
            _formSubmissionService.Setup(f => f.SubmitFormResponse(It.IsAny<MpFormResponse>())).Returns(formResponseId);
            _communicationService.Setup(
                s =>
                    s.GetTemplateAsCommunication(It.IsAny<int>(),
                                                 It.IsAny<int>(),
                                                 It.IsAny<string>(),
                                                 It.IsAny<int>(),
                                                 It.IsAny<string>(),
                                                 It.IsAny<int>(),
                                                 It.IsAny<string>(),
                                                 It.IsAny<Dictionary<string, object>>())).Returns(mpc);

            _communicationService.Setup(s => s.SendMessage(mpc, false)).Returns(9999);
            _contactService.Setup(s => s.GetContactById(It.IsAny<int>())).Returns(mycontact);
            _programRepository.Setup(s => s.GetProgramById(It.IsAny<int>())).Returns(program);

            var result = _fixture.SaveApplication(mockTripApplication(contactId, pledgeCampaignId));

            Assert.IsTrue(result == formResponseId);

            _configurationWrapper.Verify(v => v.GetConfigIntValue("TripAppAndDonationComboMessageTemplateId"), Times.Exactly(1));
        }

        private TripApplicationDto mockTripApplication(int contactid, int pledgeid)
        {
            var depositInfo = new TripApplicationDto.ApplicationDepositInformation();
            depositInfo.DonationAmount = "300";
            depositInfo.DonationDate = "1/1/2011";
            depositInfo.PaymentMethod = "Bank";

            var pageTwo = new TripApplicationDto.ApplicationPageTwo();
            pageTwo.Allergies = "";            
            pageTwo.GuardianFirstName = "Bob";
            pageTwo.GuardianLastName = "Smith";
            pageTwo.Referral = "";
            pageTwo.ScrubSizeBottom = "S";
            pageTwo.ScrubSizeTop = "S";
            pageTwo.SpiritualLife = new string[] { "" };
            pageTwo.Why = "";

            var pageThree = new TripApplicationDto.ApplicationPageThree();
            pageThree.EmergencyContactEmail = "bob@bob.com";
            pageThree.EmergencyContactFirstName = "bob";
            pageThree.EmergencyContactLastName = "roberts";
            pageThree.EmergencyContactPrimaryPhone = "888-888-8888";
            pageThree.EmergencyContactSecondaryPhone = "555-555-5555";
            pageThree.Conditions = "";

            var pageFour = new TripApplicationDto.ApplicationPageFour();
            pageFour.GroupCommonName = "group name";
            pageFour.InterestedInGroupLeader = "No";
            pageFour.RoommateFirstChoice = "Pete Rose";
            pageFour.RoommateSecondChoice = "Bill Clinton";
            pageFour.SupportPersonEmail = "support@qwerty.com";
            pageFour.WhyGroupLeader = "";

            var pageFive = new TripApplicationDto.ApplicationPageFive();
            pageFive.NolaFirstChoiceExperience = "first choice";
            pageFive.NolaFirstChoiceWorkTeam = "first work team";
            pageFive.NolaSecondChoiceWorkTeam = "second work team";
            pageFive.SponsorChildFirstName = "Suzy";
            pageFive.SponsorChildLastName = "Sponserchild";
            pageFive.SponsorChildNumber = "99";
            pageFive.SponsorChildTown = "Townville";

            var pageSix = new TripApplicationDto.ApplicationPageSix();
            pageSix.DescribeExperienceAbroad = "None";
            pageSix.ExperienceAbroad = "None";
            pageSix.FrequentFlyers = new string[] { "" };
            pageSix.InternationalTravelExpericence = "NA";
            pageSix.PassportCountry = "";
            pageSix.PassportExpirationDate = "";
            pageSix.PassportFirstName = "";
            pageSix.PassportLastName = "";
            pageSix.PassportMiddleName = "";
            pageSix.PassportNumber = "";
            pageSix.PastAbuseHistory = "None";

            var dto = new TripApplicationDto();
            dto.ContactId = contactid;
            dto.PledgeCampaignId = pledgeid;
            dto.InviteGUID = "";
            dto.DepositInformation = depositInfo;
            dto.PageTwo = pageTwo;
            dto.PageThree = pageThree;
            dto.PageFour = pageFour;
            dto.PageFive = pageFive;
            dto.PageSix = pageSix;

            return dto;
        }

        private MpPledge mockPledge()
        {
            return new MpPledge
            {
                CampaignName = "",
                CampaignStartDate = DateTime.Now,
                CampaignEndDate = DateTime.Now,
                CampaignTypeId = 1,
                CampaignTypeName = "test",
                DonorId = 3,
                PledgeCampaignId = 1,
                PledgeDonations = 1,
                PledgeId = 1,
                PledgeStatus = "active",
                PledgeStatusId = 1,
                PledgeTotal = 100
            };
        }

        private MpPledgeCampaign mockPledgeCampaign(int campaignId = 4)
        {
            return new MpPledgeCampaign
            {
                Id = campaignId,
                EventId = 8,
                EndDate = new DateTime(2020, 08, 12),
                StartDate = new DateTime(2012, 09, 12),
                Goal = 5000.00,
                Name = "Go Midgar",
                Nickname = "Go Nica",
                ProgramId = 123,
                MaximumRegistrants = 4,                            
            };
        }

        private TripCampaignDto mockTripCampaignDto(int campaignId = 4)
        {
            return new TripCampaignDto
            {
                Id = campaignId,
                Name = "Name",
                FormId = 1,
                Nickname = "Nickname",
                YoungestAgeAllowed = 17,
                RegistrationEnd = DateTime.Now,
                RegistrationStart = DateTime.Now,
                RegistrationDeposit = "300",
                IsFull = false
            };
        }

        private List<MpPledge> mockPledges(MpPledgeCampaign campaign)
        {
            return new List<MpPledge>
            {
                new MpPledge()
                {
                    CampaignName = campaign.Name,
                    CampaignStartDate = campaign.StartDate,
                    CampaignEndDate = campaign.EndDate,
                    CampaignTypeId = 1,
                    CampaignTypeName = campaign.Type,
                    DonorId = 3,
                    PledgeCampaignId = campaign.Id,
                    PledgeDonations = 1,
                    PledgeId = 1,
                    PledgeStatus = "active",
                    PledgeStatusId = 1,
                    PledgeTotal = 100
                },
                new MpPledge()
                {
                    CampaignName = campaign.Name,
                    CampaignStartDate = campaign.StartDate,
                    CampaignEndDate = campaign.EndDate,
                    CampaignTypeId = 1,
                    CampaignTypeName = campaign.Type,
                    DonorId = 3,
                    PledgeCampaignId = campaign.Id,
                    PledgeDonations = 1,
                    PledgeId = 2,
                    PledgeStatus = "active",
                    PledgeStatusId = 1,
                    PledgeTotal = 100
                },
                new MpPledge()
                {
                    CampaignName = campaign.Name,
                    CampaignStartDate = campaign.StartDate,
                    CampaignEndDate = campaign.EndDate,
                    CampaignTypeId = 1,
                    CampaignTypeName = campaign.Type,
                    DonorId = 3,
                    PledgeCampaignId = campaign.Id,
                    PledgeDonations = 1,
                    PledgeId = 3,
                    PledgeStatus = "active",
                    PledgeStatusId = 1,
                    PledgeTotal = 100
                }
            };
        }

        private List<MpTripDistribution> MockFundingPastTripDonationsResponse()
        {
            return new List<MpTripDistribution>
            {
                new MpTripDistribution
                {
                    ContactId = 1234,
                    EventTypeId = 6,
                    EventId = 8,
                    EventTitle = "GO Someplace",
                    EventStartDate = DateTime.Today,
                    EventEndDate = DateTime.Today,
                    TotalPledge = 1000,
                    CampaignStartDate = DateTime.Today.AddDays(-15),
                    CampaignEndDate = DateTime.Today.AddDays(-10),
                    DonorNickname = "John",
                    DonorFirstName = "John",
                    DonorLastName = "Donor",
                    DonorEmail = "crdsusertest+johndonor@gmail.com",
                    DonationDate = DateTime.Today,
                    DonationAmount = 350
                }
            };
        }

        private List<MpTripParticipant> mockTripParticipants()
        {
            return new List<MpTripParticipant>
            {
                new MpTripParticipant()
                {
                    EmailAddress = "myEmail@Address.com",
                    EventStartDate = new DateTime(2015, 10, 08),
                    EventEndDate = new DateTime(2015, 10, 23),
                    EventId = 8,
                    EventParticipantId = 21,
                    EventTitle = "Go Someplace",
                    EventType = "MissionTrip",
                    Lastname = "Name",
                    Nickname = "Funny",
                    ParticipantId = 213,
                    ProgramId = 2,
                    ProgramName = "Go Someplace",
                    CampaignId = 1,
                    DonorId = 3
                }
            };
        }

        private List<MpTripDistribution> MockTripDonationsResponse()
        {
            return new List<MpTripDistribution>
            {
                new MpTripDistribution
                {
                    ContactId = 1234,
                    EventTypeId = 6,
                    EventId = 8,
                    EventTitle = "GO Someplace",
                    EventStartDate = DateTime.Today,
                    EventEndDate = DateTime.Today,
                    TotalPledge = 1000,
                    CampaignStartDate = DateTime.Today,
                    CampaignEndDate = DateTime.Today,
                    DonorNickname = "John",
                    DonorFirstName = "John",
                    DonorLastName = "Donor",
                    DonorEmail = "crdsusertest+johndonor@gmail.com",
                    DonationDate = DateTime.Today,
                    DonationAmount = 350
                },
                new MpTripDistribution
                {
                    ContactId = 1234,
                    EventTypeId = 6,
                    EventId = 8,
                    EventTitle = "Go Someplace",
                    EventStartDate = DateTime.Today,
                    EventEndDate = DateTime.Today,
                    TotalPledge = 1000,
                    CampaignStartDate = DateTime.Today,
                    CampaignEndDate = DateTime.Today,
                    DonorNickname = "John",
                    DonorFirstName = "John",
                    DonorLastName = "Donor",
                    DonorEmail = "crdsusertest+johndonor@gmail.com",
                    DonationDate = DateTime.Today,
                    DonationAmount = 200
                }
            };
        }

        private List<MpTripDistribution> MockTripScholarshipDonationsResponse()
        {
            return new List<MpTripDistribution>
            {
                new MpTripDistribution
                {
                    ContactId = 1234,
                    EventTypeId = 6,
                    EventId = 8,
                    EventTitle = "GO Someplace",
                    EventStartDate = DateTime.Today,
                    EventEndDate = DateTime.Today,
                    TotalPledge = 1000,
                    CampaignStartDate = DateTime.Today,
                    CampaignEndDate = DateTime.Today,
                    DonorNickname = "Crossroads",
                    DonorFirstName = "Crossroads",
                    DonorLastName = "Crossroads",
                    DonorEmail = "crossroads@crossroads.net",
                    DonationDate = DateTime.Today,
                    DonationAmount = 1000
                }
            };
        }

        private AuthDTO fakeAuthDTO(int contactId, string emailAddress)
        {
            AuthDTO token = new AuthDTO();
            token.UserInfo = new UserInfo();
            token.UserInfo.Mp = new MpUserInfo();
            token.UserInfo.Mp.ContactId = contactId;
            token.UserInfo.Mp.Email = emailAddress;

            return token;
        }

        private static List<MpTripParticipant> MockMpSearchResponse()
        {
            return new List<MpTripParticipant>
            {
                new MpTripParticipant
                {
                    EmailAddress = "test@aol.com",
                    EventEndDate = new DateTime(2015, 7, 1),
                    EventId = 1,
                    EventParticipantId = 7777,
                    EventStartDate = new DateTime(2015, 7, 1),
                    EventTitle = "Test Trip 1",
                    EventType = "Go Trip",
                    Lastname = "Subject",
                    Nickname = "Test",
                    ParticipantId = 9999,
                    CampaignId = 1,
                    DonorId = 1
                },
                new MpTripParticipant
                {
                    EmailAddress = "test@aol.com",
                    EventEndDate = new DateTime(2015, 8, 1),
                    EventId = 2,
                    EventParticipantId = 888,
                    EventStartDate = new DateTime(2015, 8, 1),
                    EventTitle = "Test Trip 2",
                    EventType = "Go Trip",
                    Lastname = "Subject",
                    Nickname = "Test",
                    ParticipantId = 9999,
                    CampaignId = 2,
                    DonorId = 2
                },
                new MpTripParticipant
                {
                    EmailAddress = "spec@aol.com",
                    EventEndDate = new DateTime(2015, 7, 1),
                    EventId = 1,
                    EventParticipantId = 4444,
                    EventStartDate = new DateTime(2015, 7, 1),
                    EventTitle = "Test Trip 1",
                    EventType = "Go Trip",
                    Lastname = "Dummy",
                    Nickname = "Crash",
                    ParticipantId = 5555,
                    CampaignId = 3,
                    DonorId = 3
                }
            };
        }
    }
}
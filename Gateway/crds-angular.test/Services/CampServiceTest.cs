using System;
using System.Collections.Generic;
using System.Linq;
using crds_angular.Models.Crossroads.Attribute;
using crds_angular.Models.Crossroads.Camp;
using crds_angular.Models.Crossroads.Payment;
using crds_angular.Services;
using crds_angular.Services.Interfaces;
using crds_angular.test.Helpers;
using Crossroads.Utilities.FunctionalHelpers;
using Crossroads.Web.Common.Configuration;
using Crossroads.Web.Common.MinistryPlatform;
using MinistryPlatform.Translation.Models;
using MinistryPlatform.Translation.Repositories.Interfaces;
using Moq;
using NUnit.Framework;
using MinistryPlatform.Translation.Models.MinistryPlatform.Translation.Models;
using MinistryPlatform.Translation.Models.Payments;
using MinistryPlatform.Translation.Models.Product;
using Crossroads.Web.Common.Security;
using Crossroads.Web.Auth.Models;

namespace crds_angular.test.Services
{
    public class CampServiceTest
    {
        private ICampService _fixture;
        private Mock<ICampRepository> _campService;
        private Mock<IFormSubmissionRepository> _formSubmissionRepository;
        private Mock<IConfigurationWrapper> _configurationWrapper;
        private Mock<IParticipantRepository> _participantRepository;
        private Mock<IEventRepository> _eventRepository;
        private Mock<IApiUserRepository> _apiUserRepository;
        private Mock<IGroupService> _groupService;
        private Mock<IContactRepository> _contactService;
        private Mock<ICongregationRepository> _congregationRepository;
        private Mock<IGroupRepository> _groupRepository;
        private Mock<IEventParticipantRepository> _eventParticipantRepository;
        private Mock<IMedicalInformationRepository> _medicalInformationRepository;
        private Mock<IProductRepository> _productRepository;
        private Mock<IInvoiceRepository> _invoiceRepository;
        private Mock<ICommunicationRepository> _communicationRepository;
        private Mock<IPaymentRepository> _paymentRepository;
        private Mock<IObjectAttributeService> _objectAttributeService;
        private Mock<ICampRules> _campRules;
        private Mock<IPaymentService> _paymentService;

        [SetUp]
        public void SetUp()
        {
            Factories.CampReservationDTO();
            Factories.MpGroupParticipant();
            Factories.MpCongregation();
            Factories.CampProductDTO();
            Factories.MpParticipant();
            Factories.MpEvent();
            Factories.MpProduct();
            Factories.MpProductOptionPrice();

            _contactService = new Mock<IContactRepository>();
            _campService = new Mock<ICampRepository>();
            _formSubmissionRepository = new Mock<IFormSubmissionRepository>();
            _configurationWrapper = new Mock<IConfigurationWrapper>();
            _participantRepository = new Mock<IParticipantRepository>();
            _eventRepository = new Mock<IEventRepository>();
            _apiUserRepository = new Mock<IApiUserRepository>();
            _groupService = new Mock<IGroupService>();
            _contactService = new Mock<IContactRepository>();
            _congregationRepository = new Mock<ICongregationRepository>();
            _groupRepository = new Mock<IGroupRepository>();
            _eventParticipantRepository = new Mock<IEventParticipantRepository>();
            _medicalInformationRepository = new Mock<IMedicalInformationRepository>();
            _productRepository = new Mock<IProductRepository>();
            _invoiceRepository = new Mock<IInvoiceRepository>();
            _communicationRepository = new Mock<ICommunicationRepository>();
            _paymentRepository = new Mock<IPaymentRepository>();
            _objectAttributeService = new Mock<IObjectAttributeService>();
            _campRules = new Mock<ICampRules>();
            _paymentService = new Mock<IPaymentService>();

            _fixture = new CampService(_campService.Object, 
                                       _formSubmissionRepository.Object, 
                                       _configurationWrapper.Object, 
                                       _participantRepository.Object, 
                                       _eventRepository.Object, 
                                       _apiUserRepository.Object, 
                                       _contactService.Object,
                                       _congregationRepository.Object,
                                       _groupRepository.Object,
                                       _eventParticipantRepository.Object,
                                       _medicalInformationRepository.Object,
                                       _productRepository.Object,
                                       _invoiceRepository.Object,
                                       _communicationRepository.Object,
                                       _paymentRepository.Object,
                                       _objectAttributeService.Object,
                                       _campRules.Object,
                                       _paymentService.Object);
        }

        [Test]
        public void shouldGetCampFamilyMembers()
        {
            var token = new AuthDTO();
            var apiToken = "apiToken";
            var myContactId = 2187211;
            var signedUpDate = DateTime.Now;
            var eventId = 5433;
            var myContact = getFakeContact(myContactId);
            var eventParticipant = new MpEventParticipant
            {
            };

            _contactService.Setup(m => m.GetMyProfile(token)).Returns(myContact);
            _contactService.Setup(m => m.GetHouseholdFamilyMembers(myContact.Household_ID)).Returns(getFakeHouseholdMembers(myContact));
            _contactService.Setup(m => m.GetOtherHouseholdMembers(myContact.Household_ID)).Returns(new List<MpHouseholdMember>());
            _apiUserRepository.Setup(m => m.GetDefaultApiClientToken()).Returns(apiToken);
            _groupRepository.Setup(m => m.IsMemberOfEventGroup(123, eventId, apiToken)).Returns(true);
            _eventParticipantRepository.Setup(m => m.GetEventParticipantEligibility(It.IsAny<int>(), It.IsAny<int>())).Returns(eventParticipant);

            var result = _fixture.GetEligibleFamilyMembers(eventId, token);
            Assert.AreEqual(result.Count, 1);
            _contactService.VerifyAll();
            _groupRepository.VerifyAll();
            _eventParticipantRepository.VerifyAll();
        }


        [Test]
        public void shouldReturnGradeGroupsSortedByGroupName()
        {
            const int eventId = 123;
            var camp = new MpCamp
            {
                EventId = 1,
                EventTitle = "Fake Event",
                EventType = 2,
                StartDate = new DateTime(2017, 1, 1),
                EndDate = new DateTime(2018, 1, 1),
                OnlineProductId = 3,
                RegistrationStartDate = new DateTime(2017, 1, 1),
                RegistrationEndDate = new DateTime(2018, 1, 1),
                ProgramId = 4,
                PrimaryContactEmail = "bob@bob.com"
            };

            var eventGroup1 = new MpEventGroup
            {
                GroupId = 1,
                GroupName = "7th Grade"
            };
            var eventGroup2 = new MpEventGroup
            {
                GroupId = 2,
                GroupName = "5th Grade"
            };
            var eventGroup3 = new MpEventGroup
            {
                GroupId = 3,
                GroupName = "6th Grade"
            };
            var groups = new List<MpEventGroup> {eventGroup1, eventGroup2, eventGroup3};
            camp.CampGradesList = groups;


            _campService.Setup(m => m.GetCampEventDetails(eventId)).Returns(camp);

            var rc = _fixture.GetCampEventDetails(eventId);
            Assert.That(rc.EligibleGradesList[0].GroupName == "5th Grade");
            Assert.That(rc.EligibleGradesList[1].GroupName == "6th Grade");
            Assert.That(rc.EligibleGradesList[2].GroupName == "7th Grade");
        }

        [Test]
        public void shouldCreateNewContactAndCampReservation()
        {
            const int groupId = 123;
            var campReservation = FactoryGirl.NET.FactoryGirl.Build<CampReservationDTO>((res) =>
            {
                res.ContactId = 0;
                res.CurrentGrade = groupId;
            });
            var group = FactoryGirl.NET.FactoryGirl.Build<MpGroupParticipant>((res) => res.GroupId = groupId);
            AuthDTO token = new AuthDTO();
            const string apiToken = "apiToken";
            const int eventParticipantId = 6;
            const int newEventparticipantId = 0;
            const int eventId = 4;
            const int timeout = 20;
            const int formId = 8;
            const int newContactId = 3;

            var congregation = FactoryGirl.NET.FactoryGirl.Build<MpCongregation>();

            var household = new MpMyContact
            {
                Household_ID = 2345
            };

            var contact = new List<MpRecordID>
            {
                new MpRecordID
                {
                    RecordId = newContactId
                }
            };

            var camp = new MpEvent
            {
                EventId = eventId,
                MinutesUntilTimeout = timeout
            };

            var participant = new MpParticipant
            {
                ParticipantId = 2
            };

            var eventParticipant = new MpEventParticipant
            {
                EventParticipantId = newEventparticipantId
            };

            _contactService.Setup(m => m.GetMyProfile(token)).Returns(household);
            _contactService.Setup(m => m.CreateContact(It.IsAny<MpContact>())).Returns(contact);
            _participantRepository.Setup(m => m.GetParticipant(newContactId)).Returns(participant);
            _eventRepository.Setup(m => m.GetEvent(eventId)).Returns(camp);
            _objectAttributeService.Setup(m =>
                                              m.SaveObjectAttributes(newContactId,
                                                                     It.IsAny<Dictionary<int, ObjectAttributeTypeDTO>>(),
                                                                     It.IsAny<Dictionary<int, ObjectSingleAttributeDTO>>(),
                                                                     It.IsAny<MpObjectAttributeConfiguration>()));
            _apiUserRepository.Setup(m => m.GetDefaultApiClientToken()).Returns(apiToken);

            _campRules.Setup(m => m.VerifyCampRules(eventId, campReservation.Gender)).Returns(true);

            _groupRepository.Setup(g => g.GetGradeGroupForContact(newContactId, apiToken)).Returns(new Result<MpGroupParticipant>(true, group));
             
            _configurationWrapper.Setup(m => m.GetConfigIntValue("SummerCampFormID")).Returns(formId);            
            _configurationWrapper.Setup(m => m.GetConfigIntValue("SummerCampForm.SchoolAttendingNextYear")).Returns(12);
            _configurationWrapper.Setup(m => m.GetConfigIntValue("SummerCampForm.PreferredRoommate")).Returns(14);
            _configurationWrapper.Setup(m => m.GetConfigIntValue("SummerCampForm.CamperCongregation")).Returns(9);
            _eventParticipantRepository.Setup(m => m.GetEventParticipantEligibility(eventId, participant.ParticipantId)).Returns(eventParticipant);
            _congregationRepository.Setup(m => m.GetCongregationById(It.IsAny<int>())).Returns(congregation);
            _congregationRepository.Setup(m => m.GetCongregationByName(It.IsAny<string>(), It.IsAny<string>())).Returns(new Ok<MpCongregation>(congregation));

            _eventRepository.Setup(m => m.RegisterInterestedParticipantWithEndDate(
                participant.ParticipantId,
                eventId, 
                It.Is<DateTime>(d => d <= DateTime.Now.AddMinutes(timeout) && d > DateTime.Now.AddMinutes(timeout).AddSeconds(-1))
                ))
                .Returns(eventParticipantId);

            _fixture.SaveCampReservation(campReservation, eventId, token);

            _eventRepository.VerifyAll();
            _participantRepository.VerifyAll();
            _contactService.VerifyAll();
            _configurationWrapper.VerifyAll();
            _formSubmissionRepository.VerifyAll();

        }

        [Test]
        public void ShouldThrowExceptionIfRulesDonotPass()
        {
            AuthDTO token = new AuthDTO();
            const string apiToken = "apiToken";
            const int groupId = 123;
            const int eventId = 4;
            const int contactId = 231;

            var campReservation = FactoryGirl.NET.FactoryGirl.Build<CampReservationDTO>((res) =>
            {
                res.ContactId = contactId;
                res.CurrentGrade = groupId;
            });
            var group = FactoryGirl.NET.FactoryGirl.Build<MpGroupParticipant>((res) => res.GroupId = groupId);


            var household = new MpMyContact
            {
                Household_ID = 2345
            };

            var participant = new MpParticipant
            {
                ParticipantId = 2
            };

            var congregation = new MpCongregation
            {
                Name = "mASON"
            };

            _contactService.Setup(m => m.GetMyProfile(token)).Returns(household);
            _congregationRepository.Setup(m => m.GetCongregationById(campReservation.CrossroadsSite)).Returns(congregation);
            _contactService.Setup(m => m.UpdateContact(contactId, It.IsAny<Dictionary<string, object>>()));
            _participantRepository.Setup(m => m.GetParticipant(contactId)).Returns(participant);

            _objectAttributeService.Setup(m =>
                                              m.SaveObjectAttributes(contactId,
                                                                     It.IsAny<Dictionary<int, ObjectAttributeTypeDTO>>(),
                                                                     It.IsAny<Dictionary<int, ObjectSingleAttributeDTO>>(),
                                                                     It.IsAny<MpObjectAttributeConfiguration>()));
            _apiUserRepository.Setup(m => m.GetDefaultApiClientToken()).Returns(apiToken);
            _groupRepository.Setup(g => g.GetGradeGroupForContact(contactId, apiToken)).Returns(new Result<MpGroupParticipant>(true, group));


            _campRules.Setup(m => m.VerifyCampRules(eventId, campReservation.Gender)).Returns(false);
            Assert.Throws<ApplicationException>(() =>
            {
                _fixture.SaveCampReservation(campReservation, eventId, token);
                _eventRepository.VerifyAll();
                _participantRepository.VerifyAll();
                _configurationWrapper.VerifyAll();
                _formSubmissionRepository.VerifyAll();
            });
            
        }

        [Test]
        public void shouldUpdateContactAndCreateCampReservation()
        {

            const int groupId = 123;
            AuthDTO token = new AuthDTO();
            const string apiToken = "apiToken";
            const int eventParticipantId = 6;
            const int eventId = 4;
            const int formId = 8;
            const int contactId = 231;

            var campReservation = FactoryGirl.NET.FactoryGirl.Build<CampReservationDTO>((res) =>
            {
                res.ContactId = contactId;
                res.CurrentGrade = groupId;
            });
            var group = FactoryGirl.NET.FactoryGirl.Build<MpGroupParticipant>((res) => res.GroupId = groupId);
           

            var household = new MpMyContact
            {
                Household_ID = 2345
            };

            var participant = new MpParticipant
            {
                ParticipantId = 2
            };

            var eventParticipant = new MpEventParticipant
            {
                EventParticipantId = eventParticipantId
            };

            var congregation = new MpCongregation
            {
                Name = "mASON"
            };

            _contactService.Setup(m => m.GetMyProfile(token)).Returns(household);
            _congregationRepository.Setup(m => m.GetCongregationById(campReservation.CrossroadsSite)).Returns(congregation);
            _contactService.Setup(m => m.UpdateContact(contactId, It.IsAny<Dictionary<string, object>>()));
            _participantRepository.Setup(m => m.GetParticipant(contactId)).Returns(participant);

            _objectAttributeService.Setup(m =>
                                              m.SaveObjectAttributes(contactId,
                                                                     It.IsAny<Dictionary<int, ObjectAttributeTypeDTO>>(),
                                                                     It.IsAny<Dictionary<int, ObjectSingleAttributeDTO>>(),
                                                                     It.IsAny<MpObjectAttributeConfiguration>()));
            _apiUserRepository.Setup(m => m.GetDefaultApiClientToken()).Returns(apiToken);
            _groupRepository.Setup(g => g.GetGradeGroupForContact(contactId, apiToken)).Returns(new Result<MpGroupParticipant>(true, group));
            _campRules.Setup(m => m.VerifyCampRules(eventId, campReservation.Gender)).Returns(true);
            _configurationWrapper.Setup(m => m.GetConfigIntValue("SummerCampFormID")).Returns(formId);
            _configurationWrapper.Setup(m => m.GetConfigIntValue("SummerCampForm.SchoolAttendingNextYear")).Returns(12);
            _configurationWrapper.Setup(m => m.GetConfigIntValue("SummerCampForm.PreferredRoommate")).Returns(14);            
            _eventParticipantRepository.Setup(m => m.GetEventParticipantEligibility(eventId, It.IsAny<int>())).Returns(eventParticipant);
            _fixture.SaveCampReservation(campReservation, eventId, token);
            _eventRepository.VerifyAll();
            _participantRepository.VerifyAll();
            _configurationWrapper.VerifyAll();
            _formSubmissionRepository.VerifyAll();
            
        }

        [Test]
        public void shouldGetCampFamilyMembersNotSignedUp()
        {
            AuthDTO token = new AuthDTO();
            const string apiToken = "apiToken";
            var myContactId = 2187211;
            var eventId = 5433;
            var myContact = getFakeContact(myContactId);
            var eventParticipant = new MpEventParticipant();

            _contactService.Setup(m => m.GetMyProfile(token)).Returns(myContact);
            _contactService.Setup(m => m.GetHouseholdFamilyMembers(myContact.Household_ID)).Returns(getFakeHouseholdMembers(myContact));
            _contactService.Setup(m => m.GetOtherHouseholdMembers(myContact.Household_ID)).Returns(new List<MpHouseholdMember>());
            _apiUserRepository.Setup(m => m.GetDefaultApiClientToken()).Returns(apiToken);
            _groupRepository.Setup(m => m.IsMemberOfEventGroup(123, eventId, apiToken)).Returns(true);
            _eventParticipantRepository.Setup(m => m.GetEventParticipantEligibility(eventId, It.IsAny<int>())).Returns(eventParticipant);

            var result = _fixture.GetEligibleFamilyMembers(eventId, token);
            Assert.AreEqual(result.Count, 1);
            Assert.IsNull(result.FirstOrDefault().SignedUpDate);
            _contactService.VerifyAll();
            _groupRepository.VerifyAll();
            _eventParticipantRepository.VerifyAll();
        }

        [Test]
        public void shouldGetCampFamilyNotHeadNotSignedUp()
        {
            AuthDTO token = new AuthDTO();
            const string apiToken = "apiToken";
            const int myContactId = 2187211;
            const int eventId = 5433;
            var myContact = getFakeContact(myContactId);
            var eventParticipant = new MpEventParticipant();

            _contactService.Setup(m => m.GetMyProfile(token)).Returns(myContact);
            _contactService.Setup(m => m.GetHouseholdFamilyMembers(myContact.Household_ID)).Returns(getFakeHouseholdMembers(myContact, false, "Adult Child"));            
            _apiUserRepository.Setup(m => m.GetDefaultApiClientToken()).Returns(apiToken);
            _groupRepository.Setup(m => m.IsMemberOfEventGroup(myContact.Contact_ID, eventId, apiToken)).Returns(true);
            _eventParticipantRepository.Setup(m => m.GetEventParticipantEligibility(eventId, It.IsAny<int>())).Returns(eventParticipant);

            var result = _fixture.GetEligibleFamilyMembers(eventId, token);
            Assert.AreEqual(result.Count, 1);
            Assert.IsNull(result.FirstOrDefault().SignedUpDate);
            Assert.IsTrue(result.FirstOrDefault().IsEligible);
            _contactService.VerifyAll();
            _groupRepository.VerifyAll();
            _eventParticipantRepository.VerifyAll();
        }

        [Test]
        public void shouldGetCampFamilyHouseholdPositionNotSet()
        {
            AuthDTO token = new AuthDTO();
            const string apiToken = "apiToken";
            const int myContactId = 2187211;
            const int eventId = 5433;
            var myContact = getFakeContact(myContactId);
            var eventParticipant = new MpEventParticipant();

            _contactService.Setup(m => m.GetMyProfile(token)).Returns(myContact);
            _contactService.Setup(m => m.GetHouseholdFamilyMembers(myContact.Household_ID)).Returns(getFakeHouseholdMembers(myContact, false));
            _apiUserRepository.Setup(m => m.GetDefaultApiClientToken()).Returns(apiToken);
            _groupRepository.Setup(m => m.IsMemberOfEventGroup(myContact.Contact_ID, eventId, apiToken)).Returns(true);
            _eventParticipantRepository.Setup(m => m.GetEventParticipantEligibility(eventId, It.IsAny<int>())).Returns(eventParticipant);

            var result = _fixture.GetEligibleFamilyMembers(eventId, token);
            Assert.AreEqual(result.Count, 1);
            Assert.IsNull(result.FirstOrDefault().SignedUpDate);
            Assert.IsTrue(result.FirstOrDefault().IsEligible);
            _contactService.VerifyAll();
            _groupRepository.VerifyAll();
            _eventParticipantRepository.VerifyAll();
        }

        [Test]
        public void shouldGetCampFamilyNotHeadSignedUp()
        {
            AuthDTO token = new AuthDTO();
            const string apiToken = "apiToken";
            const int myContactId = 2187211;
            const int eventId = 5433;
            var myContact = getFakeContact(myContactId);

            var signedUpOn = DateTime.Now;
            var eventParticipant = new MpEventParticipant
            {
                SetupDate = signedUpOn
            };

            _contactService.Setup(m => m.GetMyProfile(token)).Returns(myContact);
            _contactService.Setup(m => m.GetHouseholdFamilyMembers(myContact.Household_ID)).Returns(getFakeHouseholdMembers(myContact, false, "Adult Child"));
            _apiUserRepository.Setup(m => m.GetDefaultApiClientToken()).Returns(apiToken);
            _eventParticipantRepository.Setup(m => m.GetEventParticipantEligibility(eventId, It.IsAny<int>())).Returns(eventParticipant);

            var result = _fixture.GetEligibleFamilyMembers(eventId, token);
            Assert.AreEqual(result.Count, 1);
            Assert.IsNotNull(result.First().SignedUpDate);
            Assert.AreEqual(signedUpOn, result.First().SignedUpDate);
            Assert.IsTrue(result.First().IsEligible);
            _contactService.VerifyAll();
            _groupRepository.VerifyAll();
            _eventParticipantRepository.VerifyAll();
        }

        [Test]
        public void shouldGetCampFamilyNotHeadPendingParticipant()
        {
            AuthDTO token = new AuthDTO();
            const string apiToken = "apiToken";
            const int myContactId = 2187211;
            const int eventId = 5433;
            const int interestedStatus = 456;
            var myContact = getFakeContact(myContactId);

            var signedUpOn = DateTime.Now;
            var eventParticipant = new MpEventParticipant
            {
                ParticipantStatus = interestedStatus,
                EndDate = DateTime.Now.AddDays(1)
            };

            _configurationWrapper.Setup(m => m.GetConfigIntValue("Participant_Status_Cancelled"));
            _configurationWrapper.Setup(m => m.GetConfigIntValue("Participant_Status_Interested")).Returns(interestedStatus);
            _contactService.Setup(m => m.GetMyProfile(token)).Returns(myContact);
            _contactService.Setup(m => m.GetHouseholdFamilyMembers(myContact.Household_ID)).Returns(getFakeHouseholdMembers(myContact, false, "Adult Child"));
            _apiUserRepository.Setup(m => m.GetDefaultApiClientToken()).Returns(apiToken);
            _eventParticipantRepository.Setup(m => m.GetEventParticipantEligibility(eventId, It.IsAny<int>())).Returns(eventParticipant);

            var result = _fixture.GetEligibleFamilyMembers(eventId, token);
            Assert.AreEqual(result.Count, 1);
            Assert.IsTrue(result.First().IsPending);
            Assert.IsTrue(result.First().IsEligible);
            _configurationWrapper.VerifyAll();
            _contactService.VerifyAll();
            _groupRepository.VerifyAll();
            _eventParticipantRepository.VerifyAll();
        }

        [Test]
        public void shouldSaveEmergencyContact()
        {
            var contactId = 12345;
            AuthDTO token = new AuthDTO();
            const string apiToken = "apiToken";
            var formId = 10;
            var eventId = 6789;
            var participant = new MpParticipant
            {
                ParticipantId = 2
            };
            var myContact = new MpMyContact
            {
                Contact_ID = 999999,
                Household_ID = 77777
            };

            var myHousehold = new List<MpHouseholdMember>
            {
                new MpHouseholdMember
                {
                    ContactId = contactId
                }
            };

            var otherHousehold = new List<MpHouseholdMember>
            {
                new MpHouseholdMember
                {
                    ContactId = 5555555
                }
            };

            _contactService.Setup(m => m.GetMyProfile(token)).Returns(myContact);
            _contactService.Setup(m => m.GetHouseholdFamilyMembers(myContact.Household_ID)).Returns(myHousehold);
            _contactService.Setup(m => m.GetOtherHouseholdMembers(myContact.Household_ID)).Returns(otherHousehold);
            _participantRepository.Setup(m => m.GetParticipant(contactId)).Returns(participant);
            _eventRepository.Setup(m => m.SafeRegisterParticipant(eventId, participant.ParticipantId, 0, 0));
            _configurationWrapper.Setup(m => m.GetConfigIntValue("SummerCampFormID")).Returns(formId);
            _configurationWrapper.Setup(m => m.GetConfigIntValue("SummerCampForm.EmergencyContactFirstName")).Returns(10);
            _configurationWrapper.Setup(m => m.GetConfigIntValue("SummerCampForm.EmergencyContactLastName")).Returns(12);
            _configurationWrapper.Setup(m => m.GetConfigIntValue("SummerCampForm.EmergencyContactMobilePhone")).Returns(14);
            _configurationWrapper.Setup(m => m.GetConfigIntValue("SummerCampForm.EmergencyContactEmail")).Returns(12);
            _configurationWrapper.Setup(m => m.GetConfigIntValue("SummerCampForm.EmergencyContactRelationship")).Returns(14);

            _fixture.SaveCamperEmergencyContactInfo(MockCampEmergencyContactDTO(), eventId, contactId, token);
            _configurationWrapper.VerifyAll();
            _participantRepository.VerifyAll();
            _eventRepository.VerifyAll();
        }

        [Test]
        public void shouldGetEmptyListOfFamilyMembers()
        {
            AuthDTO token = new AuthDTO();
            const string apiToken = "apiToken";
            var myContactId = 2187211;
            var eventId = 1234;
            var myContact = getFakeContact(myContactId);

            // Get a family list with only me in it i.e. no family
            var family = getFakeHouseholdMembers(myContact).Where(member => member.ContactId == myContactId).ToList();

            _contactService.Setup(m => m.GetMyProfile(token)).Returns(myContact);
            _contactService.Setup(m => m.GetHouseholdFamilyMembers(myContact.Household_ID)).Returns(family);
            _contactService.Setup(m => m.GetOtherHouseholdMembers(myContact.Household_ID)).Returns(new List<MpHouseholdMember>());
            _apiUserRepository.Setup(m => m.GetDefaultApiClientToken()).Returns(apiToken);            

            var result = _fixture.GetEligibleFamilyMembers(eventId, token);
            Assert.AreEqual(result.Count, 0);
            _contactService.VerifyAll();
        }

        [Test]
        public void ShouldGetMyCampInfo()
        {
            AuthDTO token = new AuthDTO();
            const string apiToken = "apiToken";
            const int myContactId = 2187211;
            var myContact = getFakeContact(myContactId);
            const string campType = "Camp";
            var product = new MpProduct
            {
                ProductId = 111,
                BasePrice = 1000,
                DepositPrice = 200,
                ProductName = "Hipster Beard Wax"
            };

            var paymentDetail = new PaymentDetailDTO
            {
                PaymentAmount = 1000,
                RecipientEmail = "x@x.com",
                TotalToPay = 1000
            };

            var mpInvoiceResult = new Result<MpInvoiceDetail>(true, new MpInvoiceDetail() { InvoiceId = 1234 });
            var camps = new List<MpEvent>
            {
                new MpEvent
                {
                    EventId = 123,
                    EventEndDate = DateTime.Today.AddDays(5),
                    EventStartDate = new DateTime(2017, 5, 15),
                    EventTitle = "Summer Camp",
                    PrimaryContact = new MpContact
                    {
                        ContactId =  90876,
                        EmailAddress = "studentministry@crossroads.com"
                    }
                }
            };

            IEnumerable<MpParticipant> campers = new List<MpParticipant>
            {
                new MpParticipant
                {
                    ContactId = 123,
                    ParticipantId = 77777,
                    EmailAddress = "x@x",
                    DisplayName = "Jony",
                    Nickname = "campell",
                    GroupName = ""
                }
            };

            // Get a family list with only me in it i.e. no family
            var family = getFakeHouseholdMembers(myContact).ToList();

            _apiUserRepository.Setup(m => m.GetDefaultApiClientToken()).Returns(apiToken);
            _configurationWrapper.Setup(m => m.GetConfigValue("CampEventTypeName")).Returns(campType);
            _contactService.Setup(m => m.GetMyProfile(token)).Returns(myContact);
            _contactService.Setup(m => m.GetHouseholdFamilyMembers(myContact.Household_ID)).Returns(family);
            _contactService.Setup(m => m.GetOtherHouseholdMembers(myContact.Household_ID)).Returns(new List<MpHouseholdMember>());

            _eventRepository.Setup(m => m.GetEvents(campType, apiToken)).Returns(camps);
            _eventRepository.Setup(m => m.EventParticipants(apiToken, camps.First().EventId)).Returns(campers);
            _eventRepository.Setup(m => m.GetEvent(camps.First().EventId)).Returns(camps.First());

            _productRepository.Setup(m => m.GetProductForEvent(camps.First().EventId)).Returns(product);
            _invoiceRepository.Setup(m => m.GetInvoiceDetailsForProductAndCamper(product.ProductId, family[0].ContactId, camps.First().EventId))
                .Returns(mpInvoiceResult);
            _paymentService.Setup(m => m.GetPaymentDetails(0, mpInvoiceResult.Value.InvoiceId, token, false)).Returns(paymentDetail);

            var result = _fixture.GetMyCampInfo(token);
            Assert.AreEqual(result.Count, 1);
            _eventRepository.VerifyAll();
            _contactService.VerifyAll();
        }

        [Test]
        public void ShouldSaveMedicalInfo()
        {
            AuthDTO token = new AuthDTO();
            const string apiToken = "apiToken";
            const int contactId = 123;
            var medicalInfo = new MedicalInfoDTO
            {
                InsuranceCompany = "Ins. Co. Name",
                PhysicianName = "bobby",
                PhysicianPhone = "123-4567",
                PolicyHolder = "your mom",
                MedicationsAdministered = new List<string> { "benedryl", "tylenol" },
                Allergies = new List<Allergy>(),
                Medications = new List<Medication>()
            };          

            var updateToDictionary = new Dictionary<String, Object>
                {
                    {"Contact_ID", contactId},
                    {"Medicalinformation_ID", 12676}
                };

            var myContact = new MpMyContact
            {
                Contact_ID = 999999,
                Household_ID = 77777
            };

            var myHousehold = new List<MpHouseholdMember>
            {
                new MpHouseholdMember
                {
                    ContactId = contactId
                }
            };

            var otherHousehold = new List<MpHouseholdMember>
            {
                new MpHouseholdMember
                {
                    ContactId = 5555555
                }
            };

            _contactService.Setup(m => m.GetMyProfile(token)).Returns(myContact);
            _contactService.Setup(m => m.GetHouseholdFamilyMembers(myContact.Household_ID)).Returns(myHousehold);
            _contactService.Setup(m => m.GetOtherHouseholdMembers(myContact.Household_ID)).Returns(otherHousehold);

            _medicalInformationRepository.Setup(m => m.SaveMedicalInfo(It.IsAny<MpMedicalInformation>(), It.IsAny<int>())).Returns((MpMedicalInformation min, int cid) => new MpMedicalInformation
            {
                ContactId = 123,
                MedicalInformationId = 12676,
                InsuranceCompany = min.InsuranceCompany,
                PhysicianName = min.PhysicianName,
                PhysicianPhone = min.PhysicianPhone,
                PolicyHolder = min.PolicyHolder,
                MedicationsAdministered = "benedryl,tylenol"
            });
            _contactService.Setup(m => m.UpdateContact(contactId, updateToDictionary));
            _medicalInformationRepository.Setup(m => m.UpdateOrCreateMedAllergy(It.IsAny<List<MpMedicalAllergy>>(), It.IsAny<List<MpMedicalAllergy>>()));

            Assert.DoesNotThrow(() => _fixture.SaveCamperMedicalInfo(medicalInfo, contactId, token));
        }

        [Test]
        public void ShouldSaveMedicalInfoWithNullValuesInMedsAdministered()
        {
            AuthDTO token = new AuthDTO();
            const string apiToken = "apiToken";
            const int contactId = 123;
            var medicalInfo = new MedicalInfoDTO
            {
                InsuranceCompany = "Ins. Co. Name",
                PhysicianName = "bobby",
                PhysicianPhone = "123-4567",
                PolicyHolder = "your mom",
                MedicationsAdministered = new List<string> {null, "benedryl", "tylenol", null},
                Allergies = new List<Allergy>(),
                Medications = new List<Medication>()
            };

            var updateToDictionary = new Dictionary<String, Object>
            {
                {"Contact_ID", contactId},
                {"Medicalinformation_ID", 12676}
            };

            var myContact = new MpMyContact
            {
                Contact_ID = 999999,
                Household_ID = 77777
            };

            var myHousehold = new List<MpHouseholdMember>
            {
                new MpHouseholdMember
                {
                    ContactId = contactId
                }
            };

            var otherHousehold = new List<MpHouseholdMember>
            {
                new MpHouseholdMember
                {
                    ContactId = 5555555
                }
            };

            _contactService.Setup(m => m.GetMyProfile(token)).Returns(myContact);
            _contactService.Setup(m => m.GetHouseholdFamilyMembers(myContact.Household_ID)).Returns(myHousehold);
            _contactService.Setup(m => m.GetOtherHouseholdMembers(myContact.Household_ID)).Returns(otherHousehold);

            _medicalInformationRepository.Setup(m => m.SaveMedicalInfo(It.IsAny<MpMedicalInformation>(), It.IsAny<int>())).Returns((MpMedicalInformation min, int cid) =>
            {
                Assert.AreEqual("benedryl,tylenol", min.MedicationsAdministered);
                return new MpMedicalInformation
                {
                    ContactId = 123,
                    MedicalInformationId = 12676,
                    InsuranceCompany = min.InsuranceCompany,
                    PhysicianName = min.PhysicianName,
                    PhysicianPhone = min.PhysicianPhone,
                    PolicyHolder = min.PolicyHolder,
                    MedicationsAdministered = min.MedicationsAdministered
                };
            });        

        _contactService.Setup(m => m.UpdateContact(contactId, updateToDictionary));
            _medicalInformationRepository.Setup(m => m.UpdateOrCreateMedAllergy(It.IsAny<List<MpMedicalAllergy>>(), It.IsAny<List<MpMedicalAllergy>>()));

            Assert.DoesNotThrow(() => _fixture.SaveCamperMedicalInfo(medicalInfo, contactId, token));
        }

        [Test]
        public void ShouldSaveMedicalInfoWithAllNullValuesInMedsAdministered()
        {
            AuthDTO token = new AuthDTO();
            const string apiToken = "apiToken";
            const int contactId = 123;
            var medicalInfo = new MedicalInfoDTO
            {
                InsuranceCompany = "Ins. Co. Name",
                PhysicianName = "bobby",
                PhysicianPhone = "123-4567",
                PolicyHolder = "your mom",
                MedicationsAdministered = new List<string> { null, null, null, null },
                Allergies = new List<Allergy>(),
                Medications = new List<Medication>()
            };

            var updateToDictionary = new Dictionary<String, Object>
            {
                {"Contact_ID", contactId},
                {"Medicalinformation_ID", 12676}
            };

            var myContact = new MpMyContact
            {
                Contact_ID = 999999,
                Household_ID = 77777
            };

            var myHousehold = new List<MpHouseholdMember>
            {
                new MpHouseholdMember
                {
                    ContactId = contactId
                }
            };

            var otherHousehold = new List<MpHouseholdMember>
            {
                new MpHouseholdMember
                {
                    ContactId = 5555555
                }
            };

            _contactService.Setup(m => m.GetMyProfile(token)).Returns(myContact);
            _contactService.Setup(m => m.GetHouseholdFamilyMembers(myContact.Household_ID)).Returns(myHousehold);
            _contactService.Setup(m => m.GetOtherHouseholdMembers(myContact.Household_ID)).Returns(otherHousehold);

            _medicalInformationRepository.Setup(m => m.SaveMedicalInfo(It.IsAny<MpMedicalInformation>(), It.IsAny<int>())).Returns((MpMedicalInformation min, int cid) =>
            {
                Assert.AreEqual("", min.MedicationsAdministered);
                return new MpMedicalInformation
                {
                    ContactId = 123,
                    MedicalInformationId = 12676,
                    InsuranceCompany = min.InsuranceCompany,
                    PhysicianName = min.PhysicianName,
                    PhysicianPhone = min.PhysicianPhone,
                    PolicyHolder = min.PolicyHolder,
                    MedicationsAdministered = min.MedicationsAdministered
                };
            });

            _contactService.Setup(m => m.UpdateContact(contactId, updateToDictionary));
            _medicalInformationRepository.Setup(m => m.UpdateOrCreateMedAllergy(It.IsAny<List<MpMedicalAllergy>>(), It.IsAny<List<MpMedicalAllergy>>()));

            Assert.DoesNotThrow(() => _fixture.SaveCamperMedicalInfo(medicalInfo, contactId, token));
        }

        [Test]
        public void shouldGetCamperMedicalInfo()
        {
            const int eventId = 1289;
            const int contactId = 123; // this is set in the getFakeHouseholdMembers() method
            const int loggedInContactId = 6767;
            AuthDTO token = new AuthDTO();
            const string apiToken = "apiToken";

            var me = getFakeContact(loggedInContactId);
            var family = getFakeHouseholdMembers(me);
            var mpMedInfo = new MpMedicalInformation
            {
                ContactId = contactId,
                InsuranceCompany = "FAke Insurance",
                MedicalInformationId = 12999,
                MedicationsAdministered = "tylenol,benedryl",
                PhysicianName = "Dr. Tobias Funke",
                PhysicianPhone = "1-800-ido-care",
                PolicyHolder = "Lindsey",               
            };

            _contactService.Setup(m => m.GetMyProfile(token)).Returns(me);
            _contactService.Setup(m => m.GetHouseholdFamilyMembers(me.Household_ID)).Returns(family);
            _contactService.Setup(m => m.GetOtherHouseholdMembers(me.Household_ID)).Returns(new List<MpHouseholdMember>());
            _medicalInformationRepository.Setup(m => m.GetMedicalInformation(contactId)).Returns(mpMedInfo);
            _medicalInformationRepository.Setup(m => m.GetMedicalAllergyInfo(contactId)).Returns(new List<MpMedical>());
            _medicalInformationRepository.Setup(m => m.GetMedications(contactId)).Returns(new List<MpMedication>());

            var res = _fixture.GetCampMedicalInfo(eventId, contactId, token);
            Assert.AreEqual(123, res.ContactId);
            Assert.AreEqual(2, res.MedicationsAdministered.Count);
            Assert.AreEqual(new List<string> {"tylenol", "benedryl"}, res.MedicationsAdministered);

            _contactService.VerifyAll();
            _medicalInformationRepository.VerifyAll();
        }

        [Test]
        public void shouldGetCampMedicalInfoWhenThereAreNoMedications()
        {
            const int eventId = 1289;
            const int contactId = 123; // this is set in the getFakeHouseholdMembers() method
            const int loggedInContactId = 6767;
            AuthDTO token = new AuthDTO();
            const string apiToken = "apiToken";

            var me = getFakeContact(loggedInContactId);
            var family = getFakeHouseholdMembers(me);
            var mpMedInfo = new MpMedicalInformation
            {
                ContactId = contactId,
                InsuranceCompany = "Fake Insurance",
                MedicalInformationId = 12999,
                MedicationsAdministered = null,
                PhysicianName = "Dr. Tobias Funke",
                PhysicianPhone = "1-800-ido-care",
                PolicyHolder = "Lindsey",
            };

            _contactService.Setup(m => m.GetMyProfile(token)).Returns(me);
            _contactService.Setup(m => m.GetHouseholdFamilyMembers(me.Household_ID)).Returns(family);
            _contactService.Setup(m => m.GetOtherHouseholdMembers(me.Household_ID)).Returns(new List<MpHouseholdMember>());
            _medicalInformationRepository.Setup(m => m.GetMedicalInformation(contactId)).Returns(mpMedInfo);
            _medicalInformationRepository.Setup(m => m.GetMedicalAllergyInfo(contactId)).Returns(new List<MpMedical>());
            _medicalInformationRepository.Setup(m => m.GetMedications(contactId)).Returns(new List<MpMedication>());

            var res = _fixture.GetCampMedicalInfo(eventId, contactId, token);
            Assert.AreEqual(123, res.ContactId);
            Assert.IsEmpty(res.MedicationsAdministered);

            _contactService.VerifyAll();
            _medicalInformationRepository.VerifyAll();
        }

        [Test]
        public void ShouldGetCamperInfo()
        {
            const int eventId = 123;
            AuthDTO token = new AuthDTO();
            const string apiToken = "apiToken";
            const int contactId = 2187211;
            var myContact = getFakeContact(contactId);
            const int childContactId = 123456789;

            const int summerCampFormId = 28;
            const int congregationField = 353;
            const int preferredRoomateField = 1098;
            const int schoolAttendingField = 987;

            var congregation = FactoryGirl.NET.FactoryGirl.Build<MpCongregation>(m =>
            {
                m.Name = "Oakley";
                m.CongregationId = 1;
            });

            var participant = new Result<MpGroupParticipant>(true, new MpGroupParticipant() {GroupName = "6th Grade", GroupId = 34});
            var attributesDto = new ObjectAllAttributesDTO();

            var loggedInContact = new MpMyContact
            {
                Contact_ID = 56789,
                Household_ID = 10000
            };

            var householdfamily = new List<MpHouseholdMember>
            {
                new MpHouseholdMember
                {
                    ContactId = contactId
                }

            };
            var otherHouseholdfamily = new List<MpHouseholdMember>
            {
                new MpHouseholdMember
                {
                    ContactId = childContactId
                }

            };

            _configurationWrapper.Setup(m => m.GetConfigIntValue("SummerCampFormID")).Returns(summerCampFormId);
            _configurationWrapper.Setup(m => m.GetConfigIntValue("SummerCampForm.SchoolAttendingNextYear")).Returns(schoolAttendingField);
            _configurationWrapper.Setup(m => m.GetConfigIntValue("SummerCampForm.PreferredRoommate")).Returns(preferredRoomateField);
            _configurationWrapper.Setup(m => m.GetConfigIntValue("SummerCampForm.CamperCongregation")).Returns(congregationField);

            _formSubmissionRepository.Setup(m => m.GetFormResponseAnswer(summerCampFormId, contactId, schoolAttendingField, eventId)).Returns("Fairview");
            _formSubmissionRepository.Setup(m => m.GetFormResponseAnswer(summerCampFormId, contactId, preferredRoomateField, eventId)).Returns("");
            _formSubmissionRepository.Setup(m => m.GetFormResponseAnswer(summerCampFormId, contactId, congregationField, eventId)).Returns("Oakley");

            // get congregation Id by name
            _congregationRepository.Setup(m => m.GetCongregationByName("Oakley", apiToken)).Returns(new Ok<MpCongregation>(congregation));

            _contactService.Setup(m => m.GetMyProfile(token)).Returns(loggedInContact);
            _contactService.Setup(m => m.GetHouseholdFamilyMembers(loggedInContact.Household_ID)).Returns(householdfamily);
            _contactService.Setup(m => m.GetOtherHouseholdMembers(loggedInContact.Household_ID)).Returns(otherHouseholdfamily);
            _contactService.Setup(m => m.GetContactById(contactId)).Returns(myContact);
            _apiUserRepository.Setup(m => m.GetDefaultApiClientToken()).Returns(apiToken);
            _groupRepository.Setup(m => m.GetGradeGroupForContact(contactId, apiToken)).Returns(participant);
            _objectAttributeService.Setup(m => m.GetObjectAttributes(contactId, It.IsAny<MpObjectAttributeConfiguration>())).Returns(attributesDto);

            var result = _fixture.GetCamperInfo(token, eventId, contactId);
            Assert.AreEqual(result.ContactId, 2187211);
            Assert.AreEqual(result.CurrentGrade, 34);
            Assert.AreEqual(1, result.CrossroadsSite);
            _contactService.VerifyAll();
            _groupRepository.VerifyAll();
            _formSubmissionRepository.VerifyAll();
            _configurationWrapper.VerifyAll();
        }

        [Test]
        public void shouldGetCamperInfoNoGrade()
        {
            const int eventId = 123;
            AuthDTO token = new AuthDTO();
            const string apiToken = "apiToken";
            const int contactId = 2187211;
            var myContact = getFakeContact(contactId);
            const int childContactId = 123456789;

            var participant = new Result<MpGroupParticipant>(false, "some error message");
            var attributesDto = new ObjectAllAttributesDTO();

            var loggedInContact = new MpMyContact
            {
                Contact_ID = 56789,
                Household_ID = 10000
            };

            var householdfamily = new List<MpHouseholdMember>
            {
                new MpHouseholdMember
                {
                    ContactId = contactId
                }

            };
            var otherHouseholdfamily = new List<MpHouseholdMember>
            {
                new MpHouseholdMember
                {
                    ContactId = childContactId
                }

            };
            _contactService.Setup(m => m.GetMyProfile(token)).Returns(loggedInContact);
            _contactService.Setup(m => m.GetHouseholdFamilyMembers(loggedInContact.Household_ID)).Returns(householdfamily);
            _contactService.Setup(m => m.GetOtherHouseholdMembers(loggedInContact.Household_ID)).Returns(otherHouseholdfamily);
            _contactService.Setup(m => m.GetContactById(contactId)).Returns(myContact);
            _apiUserRepository.Setup(m => m.GetDefaultApiClientToken()).Returns(apiToken);
            _groupRepository.Setup(m => m.GetGradeGroupForContact(contactId, apiToken)).Returns(participant);
            _objectAttributeService.Setup(m => m.GetObjectAttributes(contactId, It.IsAny<MpObjectAttributeConfiguration>())).Returns(attributesDto);

            var result = _fixture.GetCamperInfo(token, eventId, contactId);
            _contactService.VerifyAll();
            _groupRepository.VerifyAll();
            Assert.AreEqual(result.ContactId, 2187211);
            Assert.AreEqual(0, result.CurrentGrade);
            
        }

        [Test]
        public void ShouldGetProductInfo()
        {
            const int eventId = 1234;
            AuthDTO token = new AuthDTO();
            const string apiToken = "apiToken";
            var product = new MpProduct
            {
                ProductId = 111,
                BasePrice = 1000,
                DepositPrice = 200,
                ProductName = "Hipster Beard Wax"
            };

            var mpevent = new MpEvent
            {
                EventId = 999,
                EventTitle = "Hipster Beard Training",
                EventType = "event-type-100",
                EventStartDate = new DateTime(2017, 3, 28, 8, 30, 0),
                EventEndDate = new DateTime(2017, 4, 28, 8, 30, 0),
                PrimaryContact = new MpContact { ContactId = 12345, EmailAddress = "thedude@beautifulbeards.com" },
                ParentEventId = 6543219,
                CongregationId = 2,
                ReminderDaysPriorId = 2,
                RegistrationStartDate = new DateTime(2017, 1, 1, 8, 30, 0),
                RegistrationEndDate = new DateTime(2017, 3, 15, 8, 30, 0),
                Cancelled = false
            };

            var mpprodoption1 = new MpProductOptionPrice
            {
                ProductOptionPriceId = 1,
                OptionTitle = "Option 1",
                OptionPrice = 20,
                DaysOutToHide = 90
            };

            var mpprodoption2 = new MpProductOptionPrice
            {
                ProductOptionPriceId = 1,
                OptionTitle = "Option 1",
                OptionPrice = 20,
                DaysOutToHide = 90
            };

            var paymentDetail = new PaymentDetailDTO
            {
                PaymentAmount = 1000,
                RecipientEmail = "x@x.com",
                TotalToPay = 1000
            };

            var mpInvoiceResult = new Result<MpInvoiceDetail>(true, new MpInvoiceDetail() { InvoiceId = 1234 });
            var mpoptionlist = new List<MpProductOptionPrice>() {mpprodoption1, mpprodoption2};
            const int contactid = 12345;

            _eventRepository.Setup(m => m.GetEvent(eventId)).Returns(mpevent);
            _productRepository.Setup(m => m.GetProductForEvent(eventId)).Returns(product);
            _productRepository.Setup(m => m.GetProductOptionPricesForProduct(product.ProductId)).Returns(mpoptionlist);
            _formSubmissionRepository.Setup(m => m.GetFormResponseAnswer(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<int>(), It.IsAny<int>())).Returns("true");
            _invoiceRepository.Setup(m => m.GetInvoiceDetailsForProductAndCamper(product.ProductId, contactid, eventId))
                .Returns(mpInvoiceResult);
            _paymentService.Setup(m => m.GetPaymentDetails(0, mpInvoiceResult.Value.InvoiceId, token, false)).Returns(paymentDetail);

            var result = _fixture.GetCampProductDetails(eventId,contactid, token);
            Assert.IsTrue(result.Options.Count == 2);
            Assert.IsTrue(result.ProductId == 111);
            Assert.IsTrue(result.PaymentDetail.TotalToPay == 1000);
        }      

      [Test]
      public void ShouldGetProductInfoWhenThereIsNoInvoiceYet()
      {
        const int eventId = 1234;
        AuthDTO token = new AuthDTO();
        const string apiToken = "apiToken";
        var product = new MpProduct
        {
          ProductId = 111,
          BasePrice = 1000,
          DepositPrice = 200,
          ProductName = "Hipster Beard Wax"
        };

        var mpevent = new MpEvent
        {
          EventId = 999,
          EventTitle = "Hipster Beard Training",
          EventType = "event-type-100",
          EventStartDate = new DateTime(2017, 3, 28, 8, 30, 0),
          EventEndDate = new DateTime(2017, 4, 28, 8, 30, 0),
          PrimaryContact = new MpContact { ContactId = 12345, EmailAddress = "thedude@beautifulbeards.com" },
          ParentEventId = 6543219,
          CongregationId = 2,
          ReminderDaysPriorId = 2,
          RegistrationStartDate = new DateTime(2017, 1, 1, 8, 30, 0),
          RegistrationEndDate = new DateTime(2017, 3, 15, 8, 30, 0),
          Cancelled = false
        };

        var mpprodoption1 = new MpProductOptionPrice
        {
          ProductOptionPriceId = 1,
          OptionTitle = "Option 1",
          OptionPrice = 20,
          DaysOutToHide = 90
        };

        var mpprodoption2 = new MpProductOptionPrice
        {
          ProductOptionPriceId = 1,
          OptionTitle = "Option 1",
          OptionPrice = 20,
          DaysOutToHide = 90
        };

        var mpInvoiceResult = new Result<MpInvoiceDetail>(false, "No invoice");
        var mpoptionlist = new List<MpProductOptionPrice>() { mpprodoption1, mpprodoption2 };
        const int contactid = 12345;

        _eventRepository.Setup(m => m.GetEvent(eventId)).Returns(mpevent);
        _productRepository.Setup(m => m.GetProductForEvent(eventId)).Returns(product);
        _productRepository.Setup(m => m.GetProductOptionPricesForProduct(product.ProductId)).Returns(mpoptionlist);
        _formSubmissionRepository.Setup(m => m.GetFormResponseAnswer(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<int>(), It.IsAny<int>())).Returns("true");
        _invoiceRepository.Setup(m => m.GetInvoiceDetailsForProductAndCamper(product.ProductId, contactid, eventId))
          .Returns(mpInvoiceResult);
        
        var result = _fixture.GetCampProductDetails(eventId, contactid, token);
        Assert.IsTrue(result.Options.Count == 2);
        Assert.IsTrue(result.ProductId == 111);
        Assert.IsNull(result.PaymentDetail);
    }

    [Test]
        public void shouldSendConfirmationEmail()
        {
            const int eventId = 1234554;
            AuthDTO token = new AuthDTO();
            const string apiToken = "apiToken";
            const int paymentId = 98789;
            const int invoiceId = 8767;
            const int contactId = 67676;
            const int templateId = 12345;
            const string baseUrl = "localhost:3000";

            var startDate = DateTime.Now;
            var endDate = new DateTime(2017, 8, 20); 

            var mpEvent = fakeEvent(eventId, startDate, endDate, contactId);
            var mpTemplate = fakeTemplate();
            var mpPayment = fakePayments(contactId, paymentId);

            _configurationWrapper.Setup(m => m.GetConfigValue("BaseUrl")).Returns(baseUrl);
            _configurationWrapper.Setup(m => m.GetConfigIntValue("CampConfirmationEmailTemplate")).Returns(templateId);

            // get the event and the message Id
            _eventRepository.Setup(m => m.GetEvent(eventId)).Returns(mpEvent);
            _paymentRepository.Setup(m => m.GetPaymentsForInvoice(invoiceId)).Returns(mpPayment);
            _communicationRepository.Setup(m => m.GetTemplateAsCommunication(templateId, contactId, "some@email2.com", contactId, "some@email2.com", contactId, "Ok@email.com", It.IsAny<Dictionary<string,object>>()));
            _contactService.Setup(m => m.GetContactById(mpPayment.First().ContactId)).Returns(new MpMyContact() { Contact_ID = contactId, Email_Address = "some@email2.com", First_Name = "Natt", Last_Name = "last"});
            _contactService.Setup(m => m.GetContactById(mpPayment.First().ContactId)).Returns(new MpMyContact() {Contact_ID = contactId, Email_Address = "some@email.com"});
          
            _communicationRepository.Setup(m => m.SendMessage(It.IsAny<MinistryPlatform.Translation.Models.MpCommunication>(), false)).Returns(1);

            var resp = _fixture.SendCampConfirmationEmail(eventId, invoiceId, paymentId, token);
           
            _paymentRepository.VerifyAll();
            _contactService.VerifyAll();
            _configurationWrapper.VerifyAll();
            _eventRepository.VerifyAll();

            Assert.IsTrue(resp);
        }

        [Test]
        public void ShouldSaveInvoiceAndChooseBestOptionPrice()
        {
            const int loggedInContactId = 456;
            AuthDTO token = new AuthDTO();
            const string apiToken = "apiToken";
            const int eventParticipantId = 7878;
            const int summerCampFinancialAssistanceField = 99;
            const int summerCampFormId = 9;
            const int productId = 90;
            const decimal productBasePrice = 450M;

            var eventStartDate = DateTime.Now.AddDays(190);
            var eventEndDate = DateTime.Now.AddDays(191);            

            var myContact = getFakeContact(loggedInContactId);
            var myFamily = getFakeHouseholdMembers(myContact);
            var campDto = FactoryGirl.NET.FactoryGirl.Build<CampProductDTO>(m => m.ContactId = myFamily.FirstOrDefault().ContactId);
            var participant = FactoryGirl.NET.FactoryGirl.Build<MpParticipant>(m =>
            {
                m.ContactId = campDto.ContactId;
                m.Age = myFamily.First().Age;
            });
            var mpEvent = FactoryGirl.NET.FactoryGirl.Build<MpEvent>(m =>
            {
                m.EventId = campDto.EventId;
                m.EventStartDate = eventStartDate;
                m.EventEndDate = eventEndDate;
            });
            var mpProduct = FactoryGirl.NET.FactoryGirl.Build<MpProduct>(m => { m.ProductId = productId; m.BasePrice = productBasePrice; });
            var mpProductOptions = new List<MpProductOptionPrice>
            {
                FactoryGirl.NET.FactoryGirl.Build<MpProductOptionPrice>(m =>
                {
                    m.ProductOptionPriceId = 67;
                    m.OptionPrice = -65;
                    m.OptionTitle = "Extra Early";
                    m.DaysOutToHide = 131;
                }),
                FactoryGirl.NET.FactoryGirl.Build<MpProductOptionPrice>(m =>
                {
                    m.ProductOptionPriceId = 69;
                    m.OptionPrice = -25;
                    m.OptionTitle = "Early";
                    m.DaysOutToHide = 101;
                })
            };
            var invoiceDetails = new Result<MpInvoiceDetail>(false, "no invoice found");

            _contactService.Setup(m => m.GetMyProfile(token)).Returns(myContact);
            _contactService.Setup(m => m.GetHouseholdFamilyMembers(myContact.Household_ID)).Returns(myFamily);
            _contactService.Setup(m => m.GetOtherHouseholdMembers(myContact.Household_ID)).Returns(new List<MpHouseholdMember>());

            _participantRepository.Setup(m => m.GetParticipant(campDto.ContactId)).Returns(participant);
            _eventRepository.Setup(m => m.GetEventParticipantRecordId(campDto.EventId, participant.ParticipantId)).Returns(eventParticipantId);

            _configurationWrapper.Setup(m => m.GetConfigIntValue("SummerCampForm.FinancialAssistance")).Returns(summerCampFinancialAssistanceField);
            _configurationWrapper.Setup(m => m.GetConfigIntValue("SummerCampFormID")).Returns(summerCampFormId);

            _formSubmissionRepository.Setup(m => m.SubmitFormResponse(It.IsAny<MpFormResponse>()));
            _invoiceRepository.Setup(m => m.GetInvoiceDetailsForProductAndCamper(productId, campDto.ContactId, campDto.EventId)).Returns(invoiceDetails); 

            _eventRepository.Setup(m => m.GetEvent(campDto.EventId)).Returns(mpEvent);
            _productRepository.Setup(m => m.GetProductForEvent(campDto.EventId)).Returns(mpProduct);
            _productRepository.Setup(m => m.GetProductOptionPricesForProduct(productId)).Returns(mpProductOptions);

            _invoiceRepository.Setup(m => m.CreateInvoiceAndDetail(productId, 67, loggedInContactId, campDto.ContactId, eventParticipantId)).Returns(true);

            _fixture.SaveInvoice(campDto, token);

            _contactService.VerifyAll();
            _participantRepository.VerifyAll();
            _eventRepository.VerifyAll();
            _configurationWrapper.VerifyAll();
            _formSubmissionRepository.VerifyAll();
            _invoiceRepository.VerifyAll();
            _productRepository.VerifyAll();
        }

        [Test]
        public void ShouldSaveInvoiceAndFilterOutOldOptionPrices()
        {
            const int loggedInContactId = 456;
            AuthDTO token = new AuthDTO();
            const string apiToken = "apiToken";
            const int eventParticipantId = 7878;
            const int summerCampFinancialAssistanceField = 99;
            const int summerCampFormId = 9;
            const int productId = 90;
            const decimal productBasePrice = 450M;

            var eventStartDate = DateTime.Now.AddDays(190);
            var eventEndDate = DateTime.Now.AddDays(191);

            var myContact = getFakeContact(loggedInContactId);
            var myFamily = getFakeHouseholdMembers(myContact);
            var campDto = FactoryGirl.NET.FactoryGirl.Build<CampProductDTO>(m => m.ContactId = myFamily.FirstOrDefault().ContactId);
            var participant = FactoryGirl.NET.FactoryGirl.Build<MpParticipant>(m =>
            {
                m.ContactId = campDto.ContactId;
                m.Age = myFamily.First().Age;
            });
            var mpEvent = FactoryGirl.NET.FactoryGirl.Build<MpEvent>(m =>
            {
                m.EventId = campDto.EventId;
                m.EventStartDate = eventStartDate;
                m.EventEndDate = eventEndDate;
            });
            var mpProduct = FactoryGirl.NET.FactoryGirl.Build<MpProduct>(m => { m.ProductId = productId; m.BasePrice = productBasePrice; });
            var mpProductOptions = new List<MpProductOptionPrice>
            {
                FactoryGirl.NET.FactoryGirl.Build<MpProductOptionPrice>(m =>
                {
                    m.ProductOptionPriceId = 67;
                    m.OptionPrice = -65;
                    m.OptionTitle = "Extra Early";
                    m.DaysOutToHide = 190;
                }),
                FactoryGirl.NET.FactoryGirl.Build<MpProductOptionPrice>(m =>
                {
                    m.ProductOptionPriceId = 69;
                    m.OptionPrice = -25;
                    m.OptionTitle = "Early";
                    m.DaysOutToHide = 101;
                })
            };
            var invoiceDetails = new Result<MpInvoiceDetail>(false, "no invoice found");

            _contactService.Setup(m => m.GetMyProfile(token)).Returns(myContact);
            _contactService.Setup(m => m.GetHouseholdFamilyMembers(myContact.Household_ID)).Returns(myFamily);
            _contactService.Setup(m => m.GetOtherHouseholdMembers(myContact.Household_ID)).Returns(new List<MpHouseholdMember>());

            _participantRepository.Setup(m => m.GetParticipant(campDto.ContactId)).Returns(participant);
            _eventRepository.Setup(m => m.GetEventParticipantRecordId(campDto.EventId, participant.ParticipantId)).Returns(eventParticipantId);

            _configurationWrapper.Setup(m => m.GetConfigIntValue("SummerCampForm.FinancialAssistance")).Returns(summerCampFinancialAssistanceField);
            _configurationWrapper.Setup(m => m.GetConfigIntValue("SummerCampFormID")).Returns(summerCampFormId);

            _formSubmissionRepository.Setup(m => m.SubmitFormResponse(It.IsAny<MpFormResponse>()));
            _invoiceRepository.Setup(m => m.GetInvoiceDetailsForProductAndCamper(productId, campDto.ContactId, campDto.EventId)).Returns(invoiceDetails);

            _eventRepository.Setup(m => m.GetEvent(campDto.EventId)).Returns(mpEvent);
            _productRepository.Setup(m => m.GetProductForEvent(campDto.EventId)).Returns(mpProduct);
            _productRepository.Setup(m => m.GetProductOptionPricesForProduct(productId)).Returns(mpProductOptions);

            _invoiceRepository.Setup(m => m.CreateInvoiceAndDetail(productId, 69, loggedInContactId, campDto.ContactId, eventParticipantId)).Returns(true);

            _fixture.SaveInvoice(campDto, token);

            _contactService.VerifyAll();
            _participantRepository.VerifyAll();
            _eventRepository.VerifyAll();
            _configurationWrapper.VerifyAll();
            _formSubmissionRepository.VerifyAll();
            _invoiceRepository.VerifyAll();
            _productRepository.VerifyAll();
        }


        [Test]
        public void ShouldUpdateAnInvoiceIfOneExistsAlready()
        {
            const int loggedInContactId = 456;
            AuthDTO token = new AuthDTO();
            const string apiToken = "apiToken";
            const int eventParticipantId = 7878;
            const int summerCampFinancialAssistanceField = 99;
            const int summerCampFormId = 9;
            const int productId = 123;
            const int invoiceId = 8765;
            const decimal productBasePrice = 450M;
            var eventStartDate = DateTime.Now.AddDays(190);
            var eventEndDate = DateTime.Now.AddDays(191);

            var myContact = getFakeContact(loggedInContactId);
            var myFamily = getFakeHouseholdMembers(myContact);
            var campDto = FactoryGirl.NET.FactoryGirl.Build<CampProductDTO>(m => m.ContactId = myFamily.FirstOrDefault().ContactId);
            var participant = FactoryGirl.NET.FactoryGirl.Build<MpParticipant>(m =>
            {
                m.ContactId = campDto.ContactId;
                m.Age = myFamily.First().Age;
            });
            var mpEvent = FactoryGirl.NET.FactoryGirl.Build<MpEvent>(m =>
            {
              m.EventId = campDto.EventId;
              m.EventStartDate = eventStartDate;
              m.EventEndDate = eventEndDate;
            });
            var mpProduct = FactoryGirl.NET.FactoryGirl.Build<MpProduct>(m => { m.ProductId = productId; m.BasePrice = productBasePrice; });
            var mpProductOptions = new List<MpProductOptionPrice>
            {
              FactoryGirl.NET.FactoryGirl.Build<MpProductOptionPrice>(m =>
              {
                m.ProductOptionPriceId = 67;
                m.OptionPrice = -65;
                m.OptionTitle = "Extra Early";
                m.DaysOutToHide = 190;
              }),
              FactoryGirl.NET.FactoryGirl.Build<MpProductOptionPrice>(m =>
              {
                m.ProductOptionPriceId = 69;
                m.OptionPrice = -25;
                m.OptionTitle = "Early";
                m.DaysOutToHide = 101;
              })
            };
         
            var invoiceDetails = new Result<MpInvoiceDetail>(true, new MpInvoiceDetail() { InvoiceId = invoiceId });

            _contactService.Setup(m => m.GetMyProfile(token)).Returns(myContact);
            _contactService.Setup(m => m.GetHouseholdFamilyMembers(myContact.Household_ID)).Returns(myFamily);
            _contactService.Setup(m => m.GetOtherHouseholdMembers(myContact.Household_ID)).Returns(new List<MpHouseholdMember>());

            _participantRepository.Setup(m => m.GetParticipant(campDto.ContactId)).Returns(participant);
            _eventRepository.Setup(m => m.GetEventParticipantRecordId(campDto.EventId, participant.ParticipantId)).Returns(eventParticipantId);

            _configurationWrapper.Setup(m => m.GetConfigIntValue("SummerCampForm.FinancialAssistance")).Returns(summerCampFinancialAssistanceField);
            _configurationWrapper.Setup(m => m.GetConfigIntValue("SummerCampFormID")).Returns(summerCampFormId);

            _formSubmissionRepository.Setup(m => m.SubmitFormResponse(It.IsAny<MpFormResponse>()));
            _eventRepository.Setup(m => m.GetEvent(campDto.EventId)).Returns(mpEvent);
            _productRepository.Setup(m => m.GetProductForEvent(campDto.EventId)).Returns(mpProduct);
            _productRepository.Setup(m => m.GetProductOptionPricesForProduct(productId)).Returns(mpProductOptions);
            _invoiceRepository.Setup(m => m.GetInvoiceDetailsForProductAndCamper(productId, campDto.ContactId, campDto.EventId)).Returns(invoiceDetails);
            _invoiceRepository.Setup(m => m.UpdateInvoiceAndDetail(invoiceDetails.Value.InvoiceId, mpProduct, 69, loggedInContactId, campDto.ContactId, eventParticipantId));

            _fixture.SaveInvoice(campDto, token);

            _contactService.VerifyAll();
            _participantRepository.VerifyAll();
            _eventRepository.VerifyAll();
            _configurationWrapper.VerifyAll();
            _formSubmissionRepository.VerifyAll();
            _invoiceRepository.VerifyAll();
            _productRepository.VerifyAll();
        }


        private static List<MpPayment> fakePayments(int contactId, int paymentId)
        {
            return new List<MpPayment>
            {
                new MpPayment()
                {
                    ContactId = contactId,
                    PaymentTotal = 900,
                    PaymentId = paymentId
                },
                new MpPayment()
                {
                    ContactId = contactId,
                    PaymentTotal = 100,
                    PaymentId = 8989
                }
            };
        }

        private static MpEvent fakeEvent(int eventId, DateTime startDate, DateTime endDate, int contactId)
        {
            return new MpEvent()
            {
                EventId = eventId,
                EventTitle = "Awesome Camp",
                EventStartDate = startDate,
                EventEndDate = endDate,
                RegistrationURL = "https://blah.com/some/url",
                PrimaryContactId = contactId,
                PrimaryContact = new MpContact()
                {
                    ContactId = contactId,
                    EmailAddress = "some@email2.com"
                }
            };
        }

        private static MpMessageTemplate fakeTemplate()
        {
            return new MpMessageTemplate()
            {
                Body = "Ok Body",
                FromContactId = 1234,
                FromEmailAddress = "some@email.com",
                ReplyToContactId = 1234,
                ReplyToEmailAddress = "some@email.com",
                Subject = "RE: Your Brains"
            };

        }

        private List<MpHouseholdMember> getFakeHouseholdMembers(MpMyContact me, bool isHead = true, string positionIfNotHead = null)
        {
            return new List<MpHouseholdMember>
            {
                new MpHouseholdMember
                {
                    Age = 10,
                    ContactId = 123,
                    DateOfBirth = new DateTime(2006, 04, 03),
                    HouseholdPosition = "Minor Child",
                    FirstName = "Miles",
                    LastName = "Sil",
                    Nickname = "Miles"
                },
                new MpHouseholdMember
                {
                    Age = me.Age,
                    ContactId = me.Contact_ID,
                    FirstName = me.First_Name,
                    LastName = me.Last_Name,
                    HouseholdPosition = isHead ? "Head of Household" : positionIfNotHead,
                    Nickname = "matt"
                }
            };
        }        

        private MpMyContact getFakeContact(int contactId)
        {
            return new MpMyContact()
            {
                First_Name = "Jon",
                Last_Name = "Baker",
                Middle_Name = "",
                Current_School = "mASON",
                Gender_ID = 1,
                Address_ID = 12,
                Address_Line_1 = "adsfasdf",
                Age = 36,
                Contact_ID = contactId,
                City = "Cincinnati",
                Date_Of_Birth = DateTime.Now.ToLongDateString(),
                Congregation_ID = 1,
                Household_ID = 23               
            };
        }
        //private CampReservationDTO MockCampReservationDTO()
        //{
        //    return new CampReservationDTO
        //    {
        //        ContactId = 0,
        //        FirstName = "Jon",
        //        LastName = "Horner",
        //        MiddleName = "",
        //        BirthDate = new DateTime(2006, 04, 03) + "",
        //        Gender = 1,
        //        PreferredName = "Jon",
        //        SchoolAttending = "Mason",
        //        CurrentGrade = 3,
        //        SchoolAttendingNext = "Mason",
        //        CrossroadsSite = 3,
        //        RoomMate = ""
        //    };
        //}
        private List<CampEmergencyContactDTO> MockCampEmergencyContactDTO()
        {
            return new List<CampEmergencyContactDTO>
            {
                new CampEmergencyContactDTO
                {
                    FirstName = "Jon",
                    LastName = "Horner",
                    Email = "lknair@gmail.com",
                    MobileNumber = "123456789",
                    Relationship = "friend",
                    PrimaryEmergencyContact = true
                },
                 new CampEmergencyContactDTO
                {
                    FirstName = "Bob",
                    LastName = "Horner",
                    Email = "lknair@gmail.com",
                    MobileNumber = "123456780",
                    Relationship = "friend",
                    PrimaryEmergencyContact = false
                }
            };
        }
        private CampReservationDTO MockCampReservationDTOwithContactId()
        {
            return new CampReservationDTO
            {
                ContactId = 12345,
                FirstName = "Jon",
                LastName = "Horner",
                MiddleName = "",
                BirthDate = new DateTime(2006, 04, 03) + "",
                Gender = 1,
                PreferredName = "Jon",
                SchoolAttending = "Mason",
                CurrentGrade = 3,
                SchoolAttendingNext = "Mason",
                CrossroadsSite = 3,
                RoomMate = ""
            };
        }
    }
}

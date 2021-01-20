using System;
using System.Collections.Generic;
using System.Linq;
using crds_angular.App_Start;
using crds_angular.Models.Crossroads.Profile;
using crds_angular.Models.Crossroads.Serve;
using crds_angular.Services;
using crds_angular.Services.Interfaces;
using Crossroads.Utilities.Extensions;
using Crossroads.Web.Common.Configuration;
using Crossroads.Web.Common.MinistryPlatform;
using Crossroads.Web.Common.Security;
using MinistryPlatform.Translation.Models;
using MinistryPlatform.Translation.Repositories.Interfaces;
using Moq;
using NUnit.Framework;
using IGroupRepository = MinistryPlatform.Translation.Repositories.Interfaces.IGroupRepository;
using IEventRepository = MinistryPlatform.Translation.Repositories.Interfaces.IEventRepository;
using Participant = MinistryPlatform.Translation.Models.MpParticipant;

namespace crds_angular.test.Services
{
    [TestFixture]
    public class ServeServiceTest
    {
        private Mock<IContactRelationshipRepository> _contactRelationshipService;
        private Mock<IContactRepository> _contactService;
        private Mock<IOpportunityRepository> _opportunityService;
        private Mock<IAuthenticationRepository> _authenticationService;
        private Mock<IPersonService> _personService;
        private Mock<IServeService> _serveService;
        private Mock<IEventRepository> _eventService;
        private Mock<IParticipantRepository> _participantService;
        private Mock<IGroupParticipantRepository> _groupParticipantService;
        private Mock<IGroupRepository> _groupService;
        private Mock<ICommunicationRepository> _communicationService;
        private Mock<IConfigurationWrapper> _configurationWrapper;
        private Mock<IApiUserRepository> _apiUserService;
        private Mock<IResponseRepository> _responseService;

        private ServeService _fixture;

        private MpMessageTemplate mockRsvpChangedTemplate = new MpMessageTemplate
        {
            Body =
                "This message is to confirm that you have changed your rsvp from [Previous_Opportunity_Name] to [Opportunity_Name]." +
                "If you have any questions, contact [Group_Contact] by replying to this email." +
                "int.crossroads.net/serve-signup",
            Subject = "Serving RSVP Confirmation"
        };

        private MpMessageTemplate mockRsvpNoTemplate = new MpMessageTemplate
        {
            Body =
                "Thank you for notifying us that you cannot serve with [Group_Name] from [Start_Date] to [End_Date]." +
                "If you have any questions, contact [Group_Contact] by replying to this email." +
                "int.crossroads.net/serve-signup",
            Subject = "Serving RSVP Confirmation"
        };

        private MpMessageTemplate mockRsvpYesTemplate = new MpMessageTemplate
        {
            Body = "Thank you for signing up to serve with [Opportunity_Name] from [Start_Date] to [End_Date]!" +
                   "On the day you are serving, please report to [Room] at [Shift_Start] and plan on staying until [Shift_End]." +
                   "If you have any questions, contact [Group_Contact] by replying to this email." +
                   "int.crossroads.net/serve-signup",
            Subject = "Serving RSVP Confirmation"
        };

        private readonly int rsvpYesId = 11366;
        private readonly int rsvpNoId = 11299;
        private readonly int rsvpChangeId = 11366;

        private MpOpportunity fakeOpportunity = new MpOpportunity();
        private MpMyContact fakeGroupContact = new MpMyContact();
        private MpMyContact fakeMyContact = new MpMyContact();

        [SetUp]
        public void SetUp()
        {
            _contactRelationshipService = new Mock<IContactRelationshipRepository>();
            _contactService = new Mock<IContactRepository>();
            _opportunityService = new Mock<IOpportunityRepository>();
            _authenticationService = new Mock<IAuthenticationRepository>();
            _personService = new Mock<crds_angular.Services.Interfaces.IPersonService>();
            _eventService = new Mock<IEventRepository>();
            _serveService = new Mock<IServeService>();
            _participantService = new Mock<IParticipantRepository>();
            _groupParticipantService = new Mock<IGroupParticipantRepository>();
            _groupService = new Mock<IGroupRepository>();
            _communicationService = new Mock<ICommunicationRepository>();
            _configurationWrapper = new Mock<IConfigurationWrapper>();
            _apiUserService = new Mock<IApiUserRepository>();
            _responseService = new Mock<IResponseRepository>();

            fakeOpportunity.EventTypeId = 3;
            fakeOpportunity.GroupContactId = 23;
            fakeOpportunity.GroupContactName = "Harold";
            fakeOpportunity.GroupName = "Mighty Ducks";
            fakeOpportunity.OpportunityId = 12;
            fakeOpportunity.OpportunityName = "Goalie";

            fakeGroupContact.Contact_ID = 23;
            fakeGroupContact.Email_Address = "fakeEmail@fake.com";
            fakeGroupContact.Nickname = "fakeNick";
            fakeGroupContact.Last_Name = "Name";

            fakeMyContact.Contact_ID = 8;
            fakeMyContact.Email_Address = "fakeUser@fake.com";

            _contactService.Setup(m => m.GetContactById(1)).Returns(fakeGroupContact);
            _contactService.Setup(m => m.GetContactById(fakeOpportunity.GroupContactId)).Returns(fakeGroupContact);
            _contactService.Setup(m => m.GetContactById(8)).Returns(fakeMyContact);

            _communicationService.Setup(m => m.GetTemplate(rsvpYesId)).Returns(mockRsvpYesTemplate);
            _communicationService.Setup(m => m.GetTemplate(rsvpNoId)).Returns(mockRsvpNoTemplate);
            _communicationService.Setup(m => m.GetTemplate(rsvpChangeId)).Returns(mockRsvpChangedTemplate);
            _configurationWrapper.Setup(m => m.GetConfigIntValue("RSVPYesId")).Returns(1);


            _contactService.Setup(mocked => mocked.GetContactId(It.IsAny<string>())).Returns(123456);
            var myContact = new MpMyContact
            {
                Contact_ID = 123456,
                Email_Address = "contact@email.com",
                Last_Name = "last-name",
                Nickname = "nickname",
                First_Name = "first-name",
                Middle_Name = "middle-name",
                Maiden_Name = "maiden-name",
                Mobile_Phone = "mobile-phone",
                Mobile_Carrier = 999,
                Date_Of_Birth = "date-of-birth",
                Marital_Status_ID = 5,
                Gender_ID = 2,
                Employer_Name = "employer-name",
                Address_Line_1 = "address-line-1",
                Address_Line_2 = "address-line-2",
                City = "city",
                State = "state",
                Postal_Code = "postal-code",                
                Foreign_Country = "foreign-country",
                Home_Phone = "home-phone",
                Congregation_ID = 8,
                Household_ID = 7,
                Address_ID = 6
            };
            _contactService.Setup(mocked => mocked.GetMyProfile(It.IsAny<string>())).Returns(myContact);

            var person = new Person();
            person.ContactId = myContact.Contact_ID;
            person.EmailAddress = myContact.Email_Address;
            person.LastName = myContact.Last_Name;
            person.NickName = myContact.Nickname;

            _personService.Setup(m => m.GetLoggedInUserProfile(It.IsAny<string>())).Returns(person);

            _fixture = new ServeService(_contactService.Object, _contactRelationshipService.Object,
                _opportunityService.Object, _eventService.Object,
                _participantService.Object, _groupParticipantService.Object, _groupService.Object,
                _communicationService.Object, _configurationWrapper.Object, _apiUserService.Object, _responseService.Object);

            //force AutoMapper to register
            AutoMapperConfig.RegisterMappings();
        }

        [Test]
        public void ShouldSendReminderEmails()
        {
            const string apiToken = "1234";
            const int defaultEmailTemplate = 14567;

            var now = DateTime.Now;

            var fakeServeReminder = new ServeReminder()
            {
                OpportunityTitle = "Ok Title",
                EventEndDate = now,
                EventStartDate = now,
                EventTitle = "Whatever",
                OpportunityContactId = fakeGroupContact.Contact_ID,
                OpportunityEmailAddress = fakeGroupContact.Email_Address,
                ShiftEnd = new TimeSpan(0, 7, 0, 0),
                ShiftStart = new TimeSpan(0, 9, 0, 0),
                SignedupContactId = fakeMyContact.Contact_ID,
                SignedupEmailAddress = fakeMyContact.Email_Address
            };

            var fakePageView = new MPServeReminder()
            {
                Opportunity_Title = fakeServeReminder.OpportunityTitle,               
                Opportunity_Contact_Id = fakeServeReminder.OpportunityContactId,
                Opportunity_Email_Address = fakeServeReminder.OpportunityEmailAddress,
                Event_End_Date = now,
                Event_Start_Date = now,
                Event_Title = fakeServeReminder.EventTitle,
                Signedup_Contact_Id = fakeMyContact.Contact_ID,
                Signedup_Email_Address = fakeMyContact.Email_Address,
                Template_Id = null,
                Shift_Start = fakeServeReminder.ShiftStart,
                Shift_End = fakeServeReminder.ShiftEnd
            };

            var fakeList = new List<MPServeReminder> ()
            {
                fakePageView
            };

            const int defaultContactEmailId = 1519180;
            

            var token = _apiUserService.Setup(m => m.GetDefaultApiClientToken()).Returns(apiToken);
            _responseService.Setup(m => m.GetServeReminders(apiToken)).Returns(fakeList);
            _contactService.Setup(m => m.GetContactById(defaultContactEmailId)).Returns(fakeGroupContact);

            fakeList.ForEach(f =>
            {
                var mergeData = new Dictionary<string, object>(){
                    {"Opportunity_Title", fakeServeReminder.OpportunityTitle},
                    {"Nickname", fakeMyContact.Nickname},
                    {"Event_Start_Date", fakeServeReminder.EventStartDate.ToShortDateString()},
                    {"Event_End_Date", fakeServeReminder.EventEndDate.ToShortDateString()},
                    {"Shift_Start", fakeServeReminder.ShiftStart},
                    {"Shift_End", fakeServeReminder.ShiftEnd}
                 };

                var contact = new MpContact() {ContactId = fakeGroupContact.Contact_ID, EmailAddress = fakeGroupContact.Email_Address};
                var toContact = new MpContact() {ContactId = fakeMyContact.Contact_ID, EmailAddress = fakeMyContact.Email_Address};
                var fakeCommunication = new MinistryPlatform.Translation.Models.MpCommunication()
                {
                    AuthorUserId = fakeGroupContact.Contact_ID,
                    DomainId = 1,
                    EmailBody = "Ok Email Body",
                    EmailSubject = "Whatever",
                    FromContact = contact,
                    MergeData = mergeData,
                    ReplyToContact = contact,
                    TemplateId = defaultEmailTemplate,
                    ToContacts = new List<MpContact>() {toContact}
                };

                _contactService.Setup(m => m.GetContactById(fakeServeReminder.SignedupContactId)).Returns(fakeMyContact);
                _communicationService.Setup(m => m.GetTemplateAsCommunication(defaultEmailTemplate,
                                                                              fakeGroupContact.Contact_ID,
                                                                              fakeGroupContact.Email_Address,
                                                                              fakeServeReminder.OpportunityContactId,
                                                                              fakeServeReminder.OpportunityEmailAddress,
                                                                              fakeMyContact.Contact_ID,
                                                                              fakeMyContact.Email_Address,
                                                                              mergeData)).Returns(fakeCommunication);
                _communicationService.Setup(m => m.SendMessage(fakeCommunication, false));
                _communicationService.Verify();

            });
            _responseService.Verify();
        }

        [Test]
        public void GetMyFamiliesServingEventsTest()
        {
            var contactId = 123456;

            _contactRelationshipService.Setup(m => m.GetMyImmediateFamilyRelationships(contactId))
                .Returns(MockContactRelationships());

            _participantService.Setup(m => m.GetParticipant(It.IsAny<int>()))
                .Returns(new Participant {ParticipantId = 1});

            _groupParticipantService.Setup(g => g.GetServingParticipants(It.IsAny<List<int>>(), It.IsAny<long>(), It.IsAny<long>(), contactId)).Returns(MockGroupServingParticipants());
            _groupParticipantService.Setup(g => g.GetListOfOpportunitiesByEventAndGroup(It.IsAny<int>(), It.IsAny<int>())).Returns(new List<MpSU2SOpportunity>());
            _groupParticipantService.Setup(g => g.GetRsvpMembers(It.IsAny<int>(), It.IsAny<int>())).Returns(new List<MpRsvpMember>());

            var servingDays = _fixture.GetServingDays(It.IsAny<string>(), contactId, It.IsAny<long>(), It.IsAny<long>());
            _contactRelationshipService.VerifyAll();
            _groupParticipantService.Verify();
            _serveService.VerifyAll();
            _participantService.VerifyAll();

            Assert.IsNotNull(servingDays);
            Assert.AreEqual(2, servingDays.Count);
            var servingDay = servingDays[0];
            Assert.AreEqual(2, servingDay.ServeTimes.Count);

            var servingTime = servingDay.ServeTimes[0];
            Assert.AreEqual(1, servingTime.ServingTeams.Count);

            servingTime = servingDay.ServeTimes[1];
            Assert.AreEqual(1, servingTime.ServingTeams.Count);
        }

        private static List<MpGroupServingParticipant> MockGroupServingParticipants()
        {
            var startDate = DateTime.Today;
            var servingParticipants = new List<MpGroupServingParticipant>
            {
                new MpGroupServingParticipant
                {
                    ContactId = 2,
                    DeadlinePassedMessage = 1,
                    DomainId = 1,
                    EventId = 3,
                    EventStartDateTime = startDate,
                    EventTitle = "Serving Event",
                    EventType = "Event Type",
                    EventTypeId = 4,
                    GroupId = 5,
                    GroupName = "Group",
                    GroupPrimaryContactEmail = "group@leader.com",
                    GroupRoleId = 6,
                    OpportunityId = 7,
                    OpportunityMaximumNeeded = 10,
                    OpportunityMinimumNeeded = 5,
                    OpportunityRoleTitle = "Member",
                    OpportunityShiftEnd = TimeSpan.Parse("8:30"),
                    OpportunityShiftStart = TimeSpan.Parse("10:30"),
                    OpportunitySignUpDeadline = 7,
                    OpportunityTitle = "Serving",
                    ParticipantEmail = "partici@pants.com",
                    ParticipantId = 8,
                    ParticipantLastName = "McServer",
                    ParticipantNickname = "Servy",
                    Rsvp = true
                },
                new MpGroupServingParticipant
                {
                    ContactId = 2,
                    DeadlinePassedMessage = 1,
                    DomainId = 1,
                    EventId = 3,
                    EventStartDateTime = startDate.AddHours(4),
                    EventTitle = "Serving Event",
                    EventType = "Event Type",
                    EventTypeId = 4,
                    GroupId = 5,
                    GroupName = "Group",
                    GroupPrimaryContactEmail = "group@leader.com",
                    GroupRoleId = 6,
                    OpportunityId = 7,
                    OpportunityMaximumNeeded = 10,
                    OpportunityMinimumNeeded = 5,
                    OpportunityRoleTitle = "Member",
                    OpportunityShiftEnd = TimeSpan.Parse("8:30"),
                    OpportunityShiftStart = TimeSpan.Parse("10:30"),
                    OpportunitySignUpDeadline = 7,
                    OpportunityTitle = "Serving",
                    ParticipantEmail = "partici@pants.com",
                    ParticipantId = 8,
                    ParticipantLastName = "McServer",
                    ParticipantNickname = "Servy",
                    Rsvp = true
                },
                new MpGroupServingParticipant
                {
                    ContactId = 2,
                    DeadlinePassedMessage = 1,
                    DomainId = 1,
                    EventId = 3,
                    EventStartDateTime = startDate.AddDays(1),
                    EventTitle = "Serving Event",
                    EventType = "Event Type",
                    EventTypeId = 4,
                    GroupId = 5,
                    GroupName = "Group",
                    GroupPrimaryContactEmail = "group@leader.com",
                    GroupRoleId = 6,
                    OpportunityId = 7,
                    OpportunityMaximumNeeded = 10,
                    OpportunityMinimumNeeded = 5,
                    OpportunityRoleTitle = "Member",
                    OpportunityShiftEnd = TimeSpan.Parse("8:30"),
                    OpportunityShiftStart = TimeSpan.Parse("10:30"),
                    OpportunitySignUpDeadline = 7,
                    OpportunityTitle = "Serving",
                    ParticipantEmail = "partici@pants.com",
                    ParticipantId = 8,
                    ParticipantLastName = "McServer",
                    ParticipantNickname = "Servy",
                    Rsvp = true
                },
                new MpGroupServingParticipant
                {
                    ContactId = 2,
                    DeadlinePassedMessage = 1,
                    DomainId = 1,
                    EventId = 3,
                    EventStartDateTime = startDate.AddDays(1).AddHours(4),
                    EventTitle = "Serving Event",
                    EventType = "Event Type",
                    EventTypeId = 4,
                    GroupId = 5,
                    GroupName = "Group",
                    GroupPrimaryContactEmail = "group@leader.com",
                    GroupRoleId = 6,
                    OpportunityId = 7,
                    OpportunityMaximumNeeded = 10,
                    OpportunityMinimumNeeded = 5,
                    OpportunityRoleTitle = "Member",
                    OpportunityShiftEnd = TimeSpan.Parse("8:30"),
                    OpportunityShiftStart = TimeSpan.Parse("10:30"),
                    OpportunitySignUpDeadline = 7,
                    OpportunityTitle = "Serving",
                    ParticipantEmail = "partici@pants.com",
                    ParticipantId = 8,
                    ParticipantLastName = "McServer",
                    ParticipantNickname = "Servy",
                    Rsvp = true
                }
            };
            return servingParticipants;
        }

        private static List<MpContactRelationship> MockContactRelationships()
        {
            var mockRelationships = new List<MpContactRelationship>();
            var mockRelationship1 = new MpContactRelationship();
            mockRelationship1.Contact_Id = 1111111;
            mockRelationship1.Participant_Id = 1;
            var mockRelationship2 = new MpContactRelationship();
            mockRelationship2.Contact_Id = 123456;
            mockRelationship2.Participant_Id = 2;
            mockRelationships.Add(mockRelationship1);
            mockRelationships.Add(mockRelationship2);
            return mockRelationships;
        }

        [Test, TestCaseSource("OpportunityCapacityCases")]
        public void OpportunityCapacityHasMinHasMax(int? min, int? max, List<MinistryPlatform.Translation.Models.MpResponse> mockResponses,
            Capacity expectedCapacity)
        {
            const int opportunityId = 9999;
            const int eventId = 1000;

            var opportunity = new MpOpportunity();
            opportunity.MaximumNeeded = max;
            opportunity.MinimumNeeded = min;
            opportunity.OpportunityId = opportunityId;
            opportunity.Responses = mockResponses;

            _opportunityService.Setup(m => m.GetOpportunityResponses(opportunityId, eventId))
                .Returns(opportunity.Responses);

            var capacity = _fixture.OpportunityCapacity(opportunityId, eventId, min, max);

            Assert.IsNotNull(capacity);
            Assert.AreEqual(capacity.Available, expectedCapacity.Available);
            Assert.AreEqual(capacity.BadgeType, expectedCapacity.BadgeType);
            Assert.AreEqual(capacity.Display, expectedCapacity.Display);
            Assert.AreEqual(capacity.Maximum, expectedCapacity.Maximum);
            Assert.AreEqual(capacity.Message, expectedCapacity.Message);
            Assert.AreEqual(capacity.Minimum, expectedCapacity.Minimum);
            Assert.AreEqual(capacity.Taken, expectedCapacity.Taken);
        }

        private static readonly object[] OpportunityCapacityCases =
        {
            new object[]
            {
                10, 20, new List<MinistryPlatform.Translation.Models.MpResponse>(),
                new Capacity
                {
                    Available = 10,
                    BadgeType = "label-info",
                    Display = true,
                    Maximum = 20,
                    Message = "10 Needed",
                    Minimum = 10,
                    Taken = 0
                }
            },
            new object[]
            {
                10, null, new List<MinistryPlatform.Translation.Models.MpResponse>(),
                new Capacity
                {
                    Available = 10,
                    BadgeType = "label-info",
                    Display = true,
                    Maximum = null,
                    Message = "10 Needed",
                    Minimum = 10,
                    Taken = 0
                }
            },
            new object[]
            {
                null, 20, new List<MinistryPlatform.Translation.Models.MpResponse>(),
                new Capacity
                {
                    Display = false,
                    Maximum = 20,
                    Minimum = null
                }
            },
            new object[]
            {
                10, 20, MockFifteenResponses(),
                new Capacity
                {
                    Display = false,
                    Maximum = 20,
                    Minimum = 10,
                }
            },
            new object[]
            {
                10, 20, MockTwentyResponses(),
                new Capacity
                {
                    Available = 0,
                    BadgeType = "label-default",
                    Display = true,
                    Maximum = 20,
                    Message = "Full",
                    Minimum = 10,
                    Taken = 20
                }
            }, 
            new object[]
            {
                null, null, MockFifteenResponses(),
                new Capacity()
                {
                    Display = false,
                    Maximum = null,
                    Minimum = null
                }
            }
        };

        [Test]
        public void OpportunityCapacityMinAndMaxNull()
        {
            const int opportunityId = 9999;
            const int eventId = 1000;

            var opportunity = new MpOpportunity();
            opportunity.MaximumNeeded = null;
            opportunity.MinimumNeeded = null;
            opportunity.OpportunityId = opportunityId;
            opportunity.Responses = new List<MinistryPlatform.Translation.Models.MpResponse>();

            _opportunityService.Setup(m => m.GetOpportunityResponses(opportunityId, eventId))
                .Returns(opportunity.Responses);

            var capacity = _fixture.OpportunityCapacity(opportunityId, eventId, opportunity.MinimumNeeded,
                opportunity.MaximumNeeded);

            Assert.IsNotNull(capacity);
            Assert.AreEqual(capacity.Display, false);
        }

        [Test]
        public void ChangeResponseFromNoToYes()
        {
            int contactId = fakeMyContact.Contact_ID;
            int opportunityId = fakeOpportunity.OpportunityId;
            int eventTypeId = fakeOpportunity.EventTypeId;
            const bool signUp = true;
            const bool alternateWeeks = false;
            var oppIds = new List<int>() {1, 2, 3, 4, 5};

            SetUpRSVPMocks(contactId, eventTypeId, opportunityId, signUp, SetupMockEvents());

            // The current Opportunity
            _opportunityService.Setup(m => m.GetOpportunityById(opportunityId, It.IsAny<string>()))
                .Returns(fakeOpportunity);
            _opportunityService.Setup(m => m.DeleteResponseToOpportunities(47, 1, 1)).Returns(1);

            // The previous Opportunity
            _opportunityService.Setup(m => m.GetOpportunityById(1, It.IsAny<string>())).Returns(new MpOpportunity()
            {
                OpportunityId = 1,
                OpportunityName = "Previous Opportunity"
            });

            _configurationWrapper.Setup(m => m.GetConfigIntValue("DefaultContactEmailId")).Returns(1234);
            _contactService.Setup(m => m.GetContactById(1234)).Returns(new MpMyContact()
            {
                Email_Address = "gmail@google.com",
                Contact_ID = 1234567890
            });
            
            SaveRsvpDto dto = new SaveRsvpDto
            {
                ContactId = contactId,
                OpportunityId = opportunityId,
                OpportunityIds = oppIds,
                EventTypeId = eventTypeId,
                AlternateWeeks = alternateWeeks,
                StartDateUnix = new DateTime(2015, 1, 1).ToUnixTime(),
                EndDateUnix = new DateTime(2016, 1, 1).ToUnixTime(),
                SignUp = signUp
            };

            _fixture.SaveServeRsvp("1234567", dto);

            _participantService.VerifyAll();
            _eventService.Verify(
                m =>
                    m.GetEventsByTypeForRange(eventTypeId, It.IsAny<DateTime>(), It.IsAny<DateTime>(),
                        It.IsAny<string>(), It.IsAny<bool>()), Times.Exactly(1));
            _eventService.Verify(m => m.RegisterParticipantForEvent(47, It.IsAny<int>(), 0, 0), Times.Exactly(5));
            _opportunityService.Verify(
                (m => m.RespondToOpportunity(47, opportunityId, It.IsAny<string>(), It.IsAny<int>(), signUp)),
                Times.Exactly(5));

            _communicationService.Verify(m => m.GetTemplate(rsvpChangeId));

            var comm = new MinistryPlatform.Translation.Models.MpCommunication
            {
                AuthorUserId = 5,
                DomainId = 1,
                EmailBody = mockRsvpChangedTemplate.Body,
                EmailSubject = mockRsvpChangedTemplate.Subject,
                FromContact = new MpContact {ContactId = fakeGroupContact.Contact_ID, EmailAddress = fakeGroupContact.Email_Address},
                ReplyToContact = new MpContact { ContactId = fakeGroupContact.Contact_ID, EmailAddress = fakeGroupContact.Email_Address },
                ToContacts = new List<MpContact> {new MpContact{ContactId = fakeGroupContact.Contact_ID, EmailAddress = fakeMyContact.Email_Address}}
            };

            var mergeData = new Dictionary<string, object>
            {
                {"Opportunity_Name", It.IsAny<string>()},
                {"Start_Date", It.IsAny<string>()},
                {"End_Date", It.IsAny<string>()},
                {"Shift_Start", It.IsAny<string>()},
                {"Shift_End", It.IsAny<string>()},
                {"Room", It.IsAny<string>()},
                {"Group_Contact", It.IsAny<string>()},
                {"Group_Name", It.IsAny<string>()},
                {"Volunteer_Name", It.IsAny<string>()},
                {"Previous_Opportunity_Name", It.IsAny<string>()}
            };

            _communicationService.Setup(m => m.SendMessage(It.IsAny<MinistryPlatform.Translation.Models.MpCommunication>(), false));
            _communicationService.Verify(m => m.SendMessage(It.IsAny<MinistryPlatform.Translation.Models.MpCommunication>(),false));
        }

        [Test]
        public void RespondToServeOpportunityYesEveryWeek()
        {
            const int contactId = 8;
            const int opportunityId = 12;
            const int eventTypeId = 3;
            const bool signUp = true;
            const bool alternateWeeks = false;
            var oppIds = new List<int>() {1, 2, 3, 4, 5};

            SetUpRSVPMocks(contactId, eventTypeId, opportunityId, signUp, SetupMockEvents());

            _configurationWrapper.Setup(m => m.GetConfigIntValue("DefaultContactEmailId")).Returns(1234);
            _contactService.Setup(m => m.GetContactById(1234)).Returns(new MpMyContact()
            {
                Email_Address = "gmail@google.com",
                Contact_ID = 1234567890
            });

            // The current Opportunity
            _opportunityService.Setup(m => m.GetOpportunityById(opportunityId, It.IsAny<string>()))
                .Returns(fakeOpportunity);

            SaveRsvpDto dto = new SaveRsvpDto
            {
                ContactId = contactId,
                OpportunityId = opportunityId,
                OpportunityIds = oppIds,
                EventTypeId = eventTypeId,
                AlternateWeeks = alternateWeeks,
                StartDateUnix = new DateTime(2015, 1, 1).ToUnixTime(),
                EndDateUnix = new DateTime(2016, 1, 1).ToUnixTime(),
                SignUp = signUp
            };

            _fixture.SaveServeRsvp("1234567", dto);

            _participantService.VerifyAll();
            _eventService.Verify(
                m =>
                    m.GetEventsByTypeForRange(eventTypeId, It.IsAny<DateTime>(), It.IsAny<DateTime>(),
                        It.IsAny<string>(), It.IsAny<bool>()), Times.Exactly(1));
            _eventService.Verify(m => m.RegisterParticipantForEvent(47, It.IsAny<int>(), 0, 0), Times.Exactly(5));
            _opportunityService.Verify(
                (m => m.RespondToOpportunity(47, opportunityId, It.IsAny<string>(), It.IsAny<int>(), signUp)),
                Times.Exactly(5));

            MpOpportunity o = new MpOpportunity();
            o.OpportunityName = "Whatever";
            o.OpportunityId = opportunityId;
            o.ShiftStart = new TimeSpan();
            o.ShiftEnd = new TimeSpan();
            o.Room = "123";

            _opportunityService.Setup(m => m.GetOpportunityById(opportunityId, It.IsAny<string>())).Returns(o);
        }

        [Test]
        public void ShouldGetAListOfPotientialVolunteers()
        {
            var saturday = new DateTime(2016, 1, 2);
            var sunday = new DateTime(2016, 1, 3);
            var eventId = 09876;
            var groupId = 00000;
            
            var groupParticipants = SetupGroupParticipants();
                        
            var evt = new crds_angular.Models.Crossroads.Events.Event()
            {
                EventId = eventId,
                StartDate = saturday,
                location = "anywhere",
                EndDate = saturday,
                name = "Something",
                EventType = "Serve Signup",
                PrimaryContactEmailAddress = "something@gmail.com",
                PrimaryContactId = 11111                
            };

            var otherResponses = new List<MinistryPlatform.Translation.Models.Opportunities.MpResponse>
            {
                new MinistryPlatform.Translation.Models.Opportunities.MpResponse()
                {
                    Contact_ID = groupParticipants.First().ContactId,
                    Event_ID = eventId,
                    Group_ID = groupId,
                    Participant_ID = groupParticipants.First().ParticipantId,
                    Response_Date = DateTime.Now,
                    Response_Result_ID = 2
                }
            };

            // no responses for Saturday...
            _opportunityService.Setup(m => m.GetContactsOpportunityResponseByGroupAndEvent(groupId, eventId)).Returns(new List<MinistryPlatform.Translation.Models.Opportunities.MpResponse>());

            // there is no response for the first participant for Saturday
            _opportunityService.Setup(m => m.SearchResponseByGroupAndEvent(
                string.Format(",,{0},,,,{1}", groupParticipants.First().ParticipantId, saturday.ToMinistryPlatformSearchFormat())
            )).Returns(new List<MinistryPlatform.Translation.Models.Opportunities.MpResponse>());

            // there is a response for the first participant for Sunday...
            _opportunityService.Setup(m => m.SearchResponseByGroupAndEvent(
                String.Format(",,{0},,,,{1}", groupParticipants.First().ParticipantId, sunday.ToMinistryPlatformSearchFormat())
            )).Returns(otherResponses);

            // there is not a response for the second participant for Saturday
            _opportunityService.Setup(m => m.SearchResponseByGroupAndEvent(
                string.Format(",,{0},,,,{1}", groupParticipants[1].ParticipantId, saturday.ToMinistryPlatformSearchFormat())
            )).Returns(new List<MinistryPlatform.Translation.Models.Opportunities.MpResponse>());

            // there is not a response for the second participant for Sunday
            _opportunityService.Setup(m => m.SearchResponseByGroupAndEvent(
                string.Format(",,{0},,,,{1}", groupParticipants[1].ParticipantId, sunday.ToMinistryPlatformSearchFormat())
            )).Returns(new List<MinistryPlatform.Translation.Models.Opportunities.MpResponse>());
            
            var potentialVolunteers = _fixture.PotentialVolunteers(groupId, evt, groupParticipants);
            Assert.AreEqual(potentialVolunteers.Count, 1);
        }

        [Test]
        public void ShouldGetAnEmptyListOfPotientialVolunteers()
        {
            var saturday = new DateTime(2016, 1, 2);
            var sunday = new DateTime(2016, 1, 3);
            var eventId = 09876;
            var groupId = 00000;

            var groupParticipants = SetupGroupParticipants();

            var evt = new crds_angular.Models.Crossroads.Events.Event()
            {
                EventId = eventId,
                StartDate = saturday,
                location = "anywhere",
                EndDate = saturday,
                name = "Something",
                EventType = "Serve Signup",
                PrimaryContactEmailAddress = "something@gmail.com",
                PrimaryContactId = 11111
            };

            var responses = new List<MinistryPlatform.Translation.Models.Opportunities.MpResponse>
            {
                new MinistryPlatform.Translation.Models.Opportunities.MpResponse()
                {
                    Contact_ID = groupParticipants[0].ContactId,
                    Event_ID = eventId,
                    Group_ID = groupId,
                    Participant_ID = groupParticipants.First().ParticipantId,
                    Response_Date = DateTime.Now,
                    Response_Result_ID = 2
                },
                new MinistryPlatform.Translation.Models.Opportunities.MpResponse()
                {
                    Contact_ID = groupParticipants[1].ContactId,
                    Event_ID = eventId,
                    Group_ID = groupId,
                    Participant_ID = groupParticipants.First().ParticipantId,
                    Response_Date = DateTime.Now,
                    Response_Result_ID = 2
                }
            };

            var otherResponses = new List<MinistryPlatform.Translation.Models.Opportunities.MpResponse>
            {
                new MinistryPlatform.Translation.Models.Opportunities.MpResponse()
                {
                    Contact_ID = groupParticipants.First().ContactId,
                    Event_ID = eventId,
                    Group_ID = groupId,
                    Participant_ID = groupParticipants.First().ParticipantId,
                    Response_Date = DateTime.Now,
                    Response_Result_ID = 2
                }
            };

            // no responses for Saturday...
            _opportunityService.Setup(m => m.GetContactsOpportunityResponseByGroupAndEvent(groupId, eventId)).Returns(responses);

            // there is a response for the first participant for Sunday...
            _opportunityService.Setup(m => m.SearchResponseByGroupAndEvent(
                String.Format(",,{0},,,,{1}", groupParticipants.First().ParticipantId, sunday.ToMinistryPlatformSearchFormat())
            )).Returns(otherResponses);

            // there is not a response for the second participant for Sunday
            _opportunityService.Setup(m => m.SearchResponseByGroupAndEvent(
                string.Format(",,{0},,,,{1}", groupParticipants[1].ParticipantId, sunday.ToMinistryPlatformSearchFormat())
            )).Returns(new List<MinistryPlatform.Translation.Models.Opportunities.MpResponse>());

            var potentialVolunteers = _fixture.PotentialVolunteers(groupId, evt, groupParticipants);
            Assert.AreEqual(potentialVolunteers.Count, 0);
        }

        [Test, TestCaseSource("AllMockEvents")]
        public void RespondToServeOpportunityYesForEveryOtherWeek(List<MpEvent> mockEvents)
        {
            const int contactId = 8;
            const int opportunityId = 12;
            const int eventTypeId = 3;
            const bool signUp = true;
            const bool alternateWeeks = true;
            var expectedEventIds = new List<int> {1, 3, 5};
            var oppIds = new List<int>() {1, 2, 3, 4, 5};

            SetUpRSVPMocks(contactId, eventTypeId, opportunityId, signUp, mockEvents);

            // The current Opportunity
            _opportunityService.Setup(m => m.GetOpportunityById(opportunityId, It.IsAny<string>()))
                .Returns(fakeOpportunity);

            _opportunityService.Setup(m => m.GetOpportunityById(1, It.IsAny<string>())).Returns(new MpOpportunity()
            {
                OpportunityId = 1,
                OpportunityName = "Previous Opportunity",
                GroupContactId = fakeOpportunity.GroupContactId
            });

            _configurationWrapper.Setup(m => m.GetConfigIntValue("DefaultContactEmailId")).Returns(1234);
            _contactService.Setup(m => m.GetContactById(1234)).Returns(new MpMyContact()
            {
                Email_Address = "gmail@google.com",
                Contact_ID = 1234567890
            });

            SaveRsvpDto dto = new SaveRsvpDto
            {
                ContactId = contactId,
                OpportunityId = opportunityId,
                OpportunityIds = oppIds,
                EventTypeId = eventTypeId,
                AlternateWeeks = alternateWeeks,
                StartDateUnix = new DateTime(2015, 1, 1).ToUnixTime(),
                EndDateUnix = new DateTime(2016, 1, 1).ToUnixTime(),
                SignUp = signUp
            };

            _fixture.SaveServeRsvp(It.IsAny<string>(), dto);

            // The current Opportunity
            _opportunityService.Setup(m => m.GetOpportunityById(opportunityId, It.IsAny<string>()))
                .Returns(fakeOpportunity);

            _participantService.VerifyAll();
            _eventService.Verify(
                m =>
                    m.GetEventsByTypeForRange(eventTypeId, It.IsAny<DateTime>(), It.IsAny<DateTime>(),
                        It.IsAny<string>(), It.IsAny<bool>()), Times.Exactly(1));

            _eventService.Verify(m => m.RegisterParticipantForEvent(47, It.IsIn<int>(expectedEventIds), 0, 0),
                Times.Exactly(3));

            _opportunityService.Verify(
                (m =>
                    m.RespondToOpportunity(47, opportunityId, It.IsAny<string>(), It.IsIn<int>(expectedEventIds), signUp)),
                Times.Exactly(3));
        }

        [Test]
        public void getRsvpMembersShouldReturnFilledOutTeam()
        {
            ServingTeam team = new ServingTeam
            {
                EventId = 4510561,
                GroupId = 23
            };

            _groupParticipantService.Setup(m => m.GetRsvpMembers(team.GroupId, team.EventId)).Returns(getRSVPMembersList());

            _groupParticipantService.Setup(m => m.GetListOfOpportunitiesByEventAndGroup(team.GroupId, team.EventId)).Returns(getServeOpportunities());

            var result = _fixture.GetServingTeamRsvps(team);
            _groupParticipantService.VerifyAll();

            Assert.AreEqual(result.Opportunities.Count, 2);

            foreach(var opp in result.Opportunities)
            {
                Assert.AreEqual(opp.RsvpMembers.Count, 2);
            }
        }

        private static readonly object[] AllMockEvents =
        {
            new[] {SetupMockEvents()},
            new[] {SetupWeekMissingInMySeriesMockEvents()},
            new[] {SetupWeekMissingNotInMySeriesMockEvents()},
            new[] {SetupWeekMutipleMissingInMySeriesMockEvents()},
            new[] {SetupWeekNotInSequentialOrderMockEvents()}
        };

        private static List<MpEvent> SetupMockEvents()
        {
            return new List<MpEvent>
            {
                new MpEvent
                {
                    EventId = 1,
                    EventStartDate = new DateTime(2015, 1, 1)
                },
                new MpEvent
                {
                    EventId = 2,
                    EventStartDate = new DateTime(2015, 1, 8)
                },
                new MpEvent
                {
                    EventId = 3,
                    EventStartDate = new DateTime(2015, 1, 15)
                },
                new MpEvent
                {
                    EventId = 4,
                    EventStartDate = new DateTime(2015, 1, 22)
                },
                new MpEvent
                {
                    EventId = 5,
                    EventStartDate = new DateTime(2015, 1, 29)
                }
            };
        }

        private static List<MpGroupParticipant> SetupGroupParticipants()
        {
            return new List<MpGroupParticipant>
            {
                new MpGroupParticipant()
                {
                    ContactId = 1234,
                    GroupRoleId = 1,
                    GroupRoleTitle = "Member",
                    LastName = "Silbernagel",
                    NickName = "Matt",
                    ParticipantId = 4321
                },
                new MpGroupParticipant()
                {
                    ContactId = 2345,
                    GroupRoleId = 1,
                    GroupRoleTitle = "Member",
                    LastName = "Maddox",
                    NickName = "Martha",
                    ParticipantId = 5432
                }
            };
        }

        private static List<MpEvent> SetupWeekMissingInMySeriesMockEvents()
        {
            return new List<MpEvent>
            {
                new MpEvent
                {
                    EventId = 1,
                    EventStartDate = new DateTime(2015, 1, 1)
                },
                new MpEvent
                {
                    EventId = 2,
                    EventStartDate = new DateTime(2015, 1, 8)
                },
                new MpEvent
                {
                    EventId = 4,
                    EventStartDate = new DateTime(2015, 1, 22)
                },
                new MpEvent
                {
                    EventId = 3,
                    EventStartDate = new DateTime(2015, 1, 29)
                },
                new MpEvent
                {
                    EventId = 6,
                    EventStartDate = new DateTime(2015, 2, 5)
                },
                new MpEvent
                {
                    EventId = 5,
                    EventStartDate = new DateTime(2015, 2, 12)
                }
            };
        }

        private static List<MpEvent> SetupWeekMissingNotInMySeriesMockEvents()
        {
            return new List<MpEvent>
            {
                new MpEvent
                {
                    EventId = 1,
                    EventStartDate = new DateTime(2015, 1, 1)
                },
                new MpEvent
                {
                    EventId = 3,
                    EventStartDate = new DateTime(2015, 1, 15)
                },
                new MpEvent
                {
                    EventId = 4,
                    EventStartDate = new DateTime(2015, 1, 22)
                },
                new MpEvent
                {
                    EventId = 5,
                    EventStartDate = new DateTime(2015, 1, 29)
                }
            };
        }

        private static List<MpEvent> SetupWeekMutipleMissingInMySeriesMockEvents()
        {
            return new List<MpEvent>
            {
                new MpEvent
                {
                    EventId = 1,
                    EventStartDate = new DateTime(2015, 1, 1)
                },
                new MpEvent
                {
                    EventId = 3,
                    EventStartDate = new DateTime(2015, 1, 15)
                },
                new MpEvent
                {
                    EventId = 4,
                    EventStartDate = new DateTime(2015, 1, 22)
                },
                new MpEvent
                {
                    EventId = 6,
                    EventStartDate = new DateTime(2015, 2, 5)
                },
                new MpEvent
                {
                    EventId = 5,
                    EventStartDate = new DateTime(2015, 2, 12)
                }
            };
        }

        private static List<MpEvent> SetupWeekNotInSequentialOrderMockEvents()
        {
            return new List<MpEvent>
            {
                new MpEvent
                {
                    EventId = 2,
                    EventStartDate = new DateTime(2015, 1, 8)
                },
                new MpEvent
                {
                    EventId = 5,
                    EventStartDate = new DateTime(2015, 1, 29)
                },
                new MpEvent
                {
                    EventId = 4,
                    EventStartDate = new DateTime(2015, 1, 22)
                },
                new MpEvent
                {
                    EventId = 1,
                    EventStartDate = new DateTime(2015, 1, 1)
                },
                new MpEvent
                {
                    EventId = 3,
                    EventStartDate = new DateTime(2015, 1, 15)
                }
            };
        }

        private void SetUpRSVPMocks(int contactId, int eventTypeId, int opportunityId, bool signUp,
            List<MpEvent> mockEvents)
        {
            var mockParticipant = new Participant
            {
                ParticipantId = 47
            };
            
            //mock it up
            _participantService.Setup(m => m.GetParticipant(contactId)).Returns(mockParticipant);
            _eventService.Setup(
                m =>
                    m.GetEventsByTypeForRange(eventTypeId, It.IsAny<DateTime>(), It.IsAny<DateTime>(),
                        It.IsAny<string>(), It.IsAny<bool>())).Returns(mockEvents);

            foreach (var mockEvent in mockEvents)
            {
                _eventService.Setup(m => m.RegisterParticipantForEvent(mockParticipant.ParticipantId, mockEvent.EventId, 0, 0));
                _opportunityService.Setup(
                    m =>
                        m.RespondToOpportunity(mockParticipant.ParticipantId, opportunityId, It.IsAny<string>(),
                            mockEvent.EventId, signUp));
            }
        }
        
        private static List<MinistryPlatform.Translation.Models.MpResponse> MockTwentyResponses()
        {
            var responses = new List<MinistryPlatform.Translation.Models.MpResponse>();
            for (var i = 0; i < 20; i++)
            {
                responses.Add(new MinistryPlatform.Translation.Models.MpResponse { Event_ID = 1000, Response_Result_ID = 1});
            }
            return responses;
        }

        private static List<MinistryPlatform.Translation.Models.MpResponse> MockFifteenResponses()
        {
            var responses = new List<MinistryPlatform.Translation.Models.MpResponse>();
            for (var i = 0; i < 15; i++)
            {
                responses.Add(new MinistryPlatform.Translation.Models.MpResponse { Event_ID = 1000, Response_Result_ID = 1});
            }
            return responses;
        }

        private static List<MpSU2SOpportunity> getServeOpportunities()
        {
            var opps = new List<MpSU2SOpportunity>();
            opps.Add(new MpSU2SOpportunity()
            {
                Group_Role_Id = 16,
                OpportunityId = 2218712,
                OpportunityTitle = "(t) Kindergarten K213 Sun 8:30",
                RsvpMembers = new List<MpRsvpMember>()             
            });

            opps.Add(new MpSU2SOpportunity()
            {
                Group_Role_Id = 22,
                OpportunityId = 2218735,
                OpportunityTitle = "Kindergarten Leader",
                RsvpMembers = new List<MpRsvpMember>()
            });

            return opps;
        }

        private static List<MpRsvpMember> getRSVPMembersList()
        {
            var rsvpMembers = new List<MpRsvpMember>()
            {
                
                    new MpRsvpMember()
                    {
                        EventId = 4510561,
                        GroupRoleId = 16,
                        LastName = "Strick",
                        Name = "JC",
                        Opportunity = 2218712,
                        ParticipantId = 7572172,
                        ResponseResultId = 1,
                        Age = 18
                    },

                    new MpRsvpMember()
                    {
                        EventId = 4510561,
                        GroupRoleId = 16,
                        LastName = "Nukem",
                        Name = "Duke",
                        Opportunity = 2218712,
                        ParticipantId = 7572183,
                        ResponseResultId = 2,
                        Age = null
                    },
                    new MpRsvpMember()
                    {
                        EventId = 4510561,
                        GroupRoleId = 22,
                        LastName = "Trujillo",
                        Name = "Liz",
                        Opportunity = 2218735,
                        ParticipantId = 7547422,
                        ResponseResultId = 1,
                        Age = 21
                    },
                    new MpRsvpMember()
                    {
                        EventId = 4510561,
                        GroupRoleId = 22,
                        LastName = "Strife",
                        Name = "Cloud",
                        Opportunity = 2218735,
                        ParticipantId = 7547423,
                        ResponseResultId = 2,
                        Age = 21
                    }
            };

            return rsvpMembers;
        }

    }
}

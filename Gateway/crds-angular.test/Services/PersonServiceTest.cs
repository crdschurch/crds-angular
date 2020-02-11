using System;
using System.Collections.Generic;
using System.Device.Location;
using crds_angular.App_Start;
using crds_angular.Models.Crossroads;
using crds_angular.Models.Crossroads.Attribute;
using crds_angular.Models.Crossroads.Profile;
using crds_angular.Services;
using crds_angular.Services.Analytics;
using crds_angular.Services.Interfaces;
using Crossroads.Web.Common.MinistryPlatform;
using Crossroads.Web.Common.Security;
using MinistryPlatform.Translation.Models;
using MPInterfaces = MinistryPlatform.Translation.Repositories.Interfaces;
using Moq;
using NUnit.Framework;
using Crossroads.Web.Common.Configuration;
using Crossroads.Service.Identity.Tests.Repositories;
using System.Net.Http;
using System.Threading;

namespace crds_angular.test.Services
{
    public class PersonServiceTest
    {
        private Mock<IObjectAttributeService> _objectAttributeService;
        private Mock<MPInterfaces.IContactRepository> _contactService;
        private Mock<IAuthenticationRepository> _authenticationService;
        private Mock<IApiUserRepository> _apiUserService;
        private Mock<MPInterfaces.IParticipantRepository> _participantService;
        private Mock<MPInterfaces.IUserRepository> _userService;
        private Mock<IAddressService> _addressService;
        private Mock<IAnalyticsService> _analyticsService;
        private Mock<IConfigurationWrapper> _configurationWrapper;
        private FakeHttpClientFactory _httpClientFactory;
        private MockRepository mockRepository;
        private IsolatedPersonService _fixture;
        private MpMyContact _myContact;
        private List<MpHouseholdMember> _householdMembers;

        private readonly DateTime startDate = new DateTime(2015, 2, 21);

        [TearDown]
        public void Teardown()
        {
            _objectAttributeService.VerifyAll();
            _contactService.VerifyAll();
            _authenticationService.VerifyAll();
            _participantService.VerifyAll();
            _userService.VerifyAll();
            _apiUserService.VerifyAll();
        }

        [SetUp]
        public void SetUp()
        {
            MockRepository mockRepository = new MockRepository(MockBehavior.Loose);
            _objectAttributeService = new Mock<IObjectAttributeService>();
            _contactService = new Mock<MPInterfaces.IContactRepository>();
            _authenticationService = new Mock<IAuthenticationRepository>();
            _participantService = new Mock<MPInterfaces.IParticipantRepository>();
            _userService = new Mock<MPInterfaces.IUserRepository>();
            _apiUserService = new Mock<IApiUserRepository>();            
            _addressService = new Mock<IAddressService>();
            _analyticsService = new Mock<IAnalyticsService>();
            _configurationWrapper = new Mock<IConfigurationWrapper>();
            _httpClientFactory = new FakeHttpClientFactory(mockRepository);
            List<HttpRequestMessage> sentRequestMessages = new List<HttpRequestMessage>();
            _httpClientFactory.SetupSendAsync().ReturnsAsync(new HttpResponseMessage
            {
                StatusCode = System.Net.HttpStatusCode.OK
            }).Callback((HttpRequestMessage msg, CancellationToken ct) => sentRequestMessages.Add(msg));
            _myContact = new MpMyContact
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
                Date_Of_Birth = "12/20/1983",
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
                Household_Name = "hh name",
                Address_ID = 6,
                Attendance_Start_Date = startDate
            };
            _householdMembers = new List<MpHouseholdMember>();

            _fixture = new IsolatedPersonService(_contactService.Object, _objectAttributeService.Object, _apiUserService.Object, _participantService.Object, _userService.Object, _authenticationService.Object, _addressService.Object, _analyticsService.Object, _configurationWrapper.Object, _httpClientFactory.httpClient);

            //force AutoMapper to register
            AutoMapperConfig.RegisterMappings();
        }

        [Test]
        public void ShouldNotUpdateCongregationIfCongregationIdIsNull()
        {
            var person = MockPerson(_myContact);
            var participant = MockParticipant(_myContact);
            const string token = "whateves";

            person.CongregationId = null;
            _contactService.Setup(m => m.UpdateContact(person.ContactId,
                                                       It.IsAny<Dictionary<string, object>>(),
                                                       It.IsAny<Dictionary<string, object>>(),
                                                       It.IsAny<Dictionary<string, object>>())).Callback((int contactId, Dictionary<string, object> profileDictionary, Dictionary<string, object> houseDictionary, Dictionary<string, object> addressDictionary) =>
                                                       {

                                                       });
            _objectAttributeService.Setup(
                m =>
                    m.SaveObjectAttributes(It.IsAny<int>(),
                                           It.IsAny<Dictionary<int, ObjectAttributeTypeDTO>>(),
                                           It.IsAny<Dictionary<int, ObjectSingleAttributeDTO>>(),
                                           It.IsAny<MpObjectAttributeConfiguration>()));

            _participantService.Setup(m => m.GetParticipant(person.ContactId)).Returns(participant);
            _addressService.Setup(m => m.GetGeoLocationCascading(It.IsAny<AddressDTO>())).Returns(new GeoCoordinate(5, 6));
            _analyticsService.Setup(m => m.Track(It.IsAny<String>(), It.IsAny<String>(), It.IsAny<EventProperties>()));
            _fixture.SetProfile(person, token);

        }

        [Test]
        public void TestGetProfileForContactId()
        {
            const int contactId = 123456;

            _contactService.Setup(mocked => mocked.GetContactById(contactId)).Returns(_myContact);
            _contactService.Setup(mocked => mocked.GetHouseholdFamilyMembers(7)).Returns(_householdMembers);
            _apiUserService.Setup(m => m.GetDefaultApiClientToken()).Returns("something");
            var allAttributesDto = new ObjectAllAttributesDTO();
            _objectAttributeService.Setup(mocked => mocked.GetObjectAttributes( It.IsAny<int>(), It.IsAny<MpObjectAttributeConfiguration>())).Returns(allAttributesDto);
            var person = _fixture.GetPerson(contactId);
            _contactService.VerifyAll();

            Assert.IsNotNull(person);

            Assert.AreEqual(123456, person.ContactId);
            Assert.AreEqual("contact@email.com", person.EmailAddress);
            Assert.AreEqual("nickname", person.NickName);
            Assert.AreEqual("first-name", person.FirstName);
            Assert.AreEqual("middle-name", person.MiddleName);
            Assert.AreEqual("last-name", person.LastName);
            Assert.AreEqual("maiden-name", person.MaidenName);
            Assert.AreEqual("mobile-phone", person.MobilePhone);
            Assert.AreEqual(999, person.MobileCarrierId);
            Assert.AreEqual("12/20/1983", person.DateOfBirth);
            Assert.AreEqual(5, person.MaritalStatusId);
            Assert.AreEqual(2, person.GenderId);
            Assert.AreEqual("employer-name", person.EmployerName);
            Assert.AreEqual("address-line-1", person.AddressLine1);
            Assert.AreEqual("address-line-2", person.AddressLine2);
            Assert.AreEqual("city", person.City);
            Assert.AreEqual("state", person.State);
            Assert.AreEqual("postal-code", person.PostalCode);
            Assert.AreEqual(startDate, person.AttendanceStartDate);
            Assert.AreEqual("foreign-country", person.ForeignCountry);
            Assert.AreEqual("home-phone", person.HomePhone);
            Assert.AreEqual(8, person.CongregationId);
            Assert.AreEqual(7, person.HouseholdId);
            Assert.AreEqual("hh name", person.HouseholdName);
            Assert.AreEqual(6, person.AddressId);
            Assert.AreSame(_householdMembers, person.HouseholdMembers);
        }

        [Test]
        public void GetLoggedInUserProfileTest()
        {
            const string token = "some-string";

            _contactService.Setup(mocked => mocked.GetMyProfile(token)).Returns(_myContact);
            _contactService.Setup(mocked => mocked.GetHouseholdFamilyMembers(7)).Returns(_householdMembers);           
            var person = _fixture.GetLoggedInUserProfile(token);

            Assert.IsNotNull(person);

            Assert.AreEqual(123456, person.ContactId);
            Assert.AreEqual("contact@email.com", person.EmailAddress);
            Assert.AreEqual("nickname", person.NickName);
            Assert.AreEqual("first-name", person.FirstName);
            Assert.AreEqual("middle-name", person.MiddleName);
            Assert.AreEqual("last-name", person.LastName);
            Assert.AreEqual("maiden-name", person.MaidenName);
            Assert.AreEqual("mobile-phone", person.MobilePhone);
            Assert.AreEqual(999, person.MobileCarrierId);
            Assert.AreEqual("12/20/1983", person.DateOfBirth);
            Assert.AreEqual(5, person.MaritalStatusId);
            Assert.AreEqual(2, person.GenderId);
            Assert.AreEqual("employer-name", person.EmployerName);
            Assert.AreEqual("address-line-1", person.AddressLine1);
            Assert.AreEqual("address-line-2", person.AddressLine2);
            Assert.AreEqual("city", person.City);
            Assert.AreEqual("state", person.State);
            Assert.AreEqual("postal-code", person.PostalCode);
            Assert.AreEqual(startDate, person.AttendanceStartDate);
            Assert.AreEqual("foreign-country", person.ForeignCountry);
            Assert.AreEqual("home-phone", person.HomePhone);
            Assert.AreEqual(8, person.CongregationId);
            Assert.AreEqual(7, person.HouseholdId);
            Assert.AreEqual("hh name", person.HouseholdName);
            Assert.AreEqual(6, person.AddressId);
            Assert.AreSame(_householdMembers, person.HouseholdMembers);
        }

        private MpParticipant MockParticipant(MpMyContact contact)
        {
            return new MpParticipant
            {
                ContactId = contact.Contact_ID,
                ParticipantId = 129876,
                EmailAddress = contact.Email_Address,
                DisplayName = contact.Display_Name,
                Nickname = contact.Nickname,
                Age = contact.Age,
                PreferredName = contact.Nickname
            };
        }

        private Person MockPerson(MpMyContact contact)
        {
            return new Person
            {
                AddressId = contact.Address_ID,
                AddressLine1 = contact.Address_Line_1,
                AddressLine2 = contact.Address_Line_2,
                City = contact.City,
                State = contact.State,
                PostalCode = contact.Postal_Code,
                Age = contact.Age,
                CongregationId = contact.Congregation_ID,
                ContactId = contact.Contact_ID,
                DateOfBirth = contact.Date_Of_Birth,
                EmailAddress = contact.Email_Address,
                FirstName = contact.First_Name,
                LastName = contact.Last_Name,
                NickName = contact.Nickname
            };
        }
    }
}

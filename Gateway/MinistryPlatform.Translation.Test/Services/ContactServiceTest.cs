using System;
using System.Collections.Generic;
using System.Linq;
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
    public class ContactServiceTest
    {
        private Mock<IMinistryPlatformService> _ministryPlatformService;
        private Mock<IAuthenticationRepository> _authService;
        private ContactRepository _fixture;
        private Mock<IConfigurationWrapper> _configuration;
        private Mock<IMinistryPlatformRestRepository> _ministryPlatformRest;
        private Mock<IApiUserRepository> _apiUserService;

        [SetUp]
        public void SetUp()
        {
            _ministryPlatformService = new Mock<IMinistryPlatformService>();
            _authService = new Mock<IAuthenticationRepository>();
            _configuration = new Mock<IConfigurationWrapper>();
            _ministryPlatformRest = new Mock<IMinistryPlatformRestRepository>();
            _apiUserService = new Mock<IApiUserRepository>();
            _configuration.Setup(mocked => mocked.GetConfigIntValue("Contacts")).Returns(292);
            _configuration.Setup(mocked => mocked.GetConfigIntValue("Households")).Returns(327);
            _configuration.Setup(mocked => mocked.GetConfigIntValue("SecurityRolesSubPageId")).Returns(363);
            _configuration.Setup(mocked => mocked.GetConfigIntValue("Congregation_Default_ID")).Returns(5);
            _configuration.Setup(mocked => mocked.GetConfigIntValue("Household_Default_Source_ID")).Returns(30);
            _configuration.Setup(mocked => mocked.GetConfigIntValue("Household_Position_Default_ID")).Returns(1);
            _configuration.Setup(mocked => mocked.GetConfigIntValue("StaffContactAttribute")).Returns(7088);
            _configuration.Setup(mocked => mocked.GetConfigIntValue("EventToolContactAttribute")).Returns(9048);
            _configuration.Setup(mocked => mocked.GetConfigIntValue("Addresses")).Returns(271);
            _configuration.Setup(m => m.GetEnvironmentVarAsString("API_USER")).Returns("uid");
            _configuration.Setup(m => m.GetEnvironmentVarAsString("API_PASSWORD")).Returns("pwd");
            _ministryPlatformRest.Setup(m => m.UsingAuthenticationToken("ABC")).Returns(_ministryPlatformRest.Object);

            _authService.Setup(m => m.AuthenticateClient(It.IsAny<string>(), It.IsAny<string>())).Returns(new AuthToken
            {
                AccessToken = "ABC",
                ExpiresIn = 123
            });
            _fixture = new ContactRepository(_ministryPlatformService.Object, _authService.Object, _configuration.Object, _ministryPlatformRest.Object, _apiUserService.Object);
        }

        [Test]
        public void GetMyProfile()
        {
            var dictionaryList = new List<Dictionary<string, object>>
            {
                new Dictionary<string, object>
                {
                    {"Address_ID", 100},
                    {"Address_Line_1", "address-line-1"},
                    {"Address_Line_2", "address-line-2"},
                    {"Congregation_ID", 5},
                    {"Household_ID", 4},
                    {"Household_Name", "hh name"},
                    {"City", "Cincinnati"},
                    {"State", "OH"},
                    {"Postal_Code", "45208"},
                    {"County", "Hamilton"},
                    {"Anniversary_Date", new DateTime(2013, 8, 5)},
                    {"Contact_ID", 3},
                    {"Date_of_Birth", new DateTime(2007, 5, 29)},
                    {"Email_Address", "email-address@email.com"},
                    {"Employer_Name", "Crossroads"},
                    {"First_Name", "first-name"},
                    {"Display_Name", "Displayname"},
                    {"Current_School", "CurrentSchool"},
                    {"Foreign_Country", "USA"},
                    {"Gender_ID", 2},
                    {"Home_Phone", "513-555-1234"},
                    {"Last_Name", "last-name"},
                    {"Maiden_Name", "maiden-name"},
                    {"Marital_Status_ID", 3},
                    {"Middle_Name", "middle-name"},
                    {"Mobile_Carrier_ID", 2},
                    {"Mobile_Phone", "513-555-9876"},
                    {"Nickname", "nickname"},
                    {"Age", 30},
                    {"Passport_Number", "12345"},
                    {"Passport_Firstname", "first-name"},
                    {"Passport_Lastname", "last-name"},
                    {"Passport_Country", "USA"},
                    {"Passport_Middlename", "middle-name"},
                    {"Passport_Expiration", "02/21/2020"}
                }
            };

            _ministryPlatformService.Setup(m => m.GetRecordsDict("MyProfile", It.IsAny<string>(), "", ""))
                .Returns(dictionaryList);

            var myProfile = _fixture.GetMyProfile(It.IsAny<string>());

            _ministryPlatformService.VerifyAll();

            Assert.IsNotNull(myProfile);
            Assert.AreEqual(3, myProfile.Contact_ID);
            Assert.AreEqual(100, myProfile.Address_ID);
            Assert.AreEqual("hh name", myProfile.Household_Name);
        }

        [Test]
        public void GetMyProfileWithOptionalNullableFields()
        {
            var dictionaryList = new List<Dictionary<string, object>>
            {
                new Dictionary<string, object>
                {
                    {"Address_ID", null},
                    {"Address_Line_1", "address-line-1"},
                    {"Address_Line_2", "address-line-2"},
                    {"Congregation_ID", null},
                    {"Household_ID", 4},
                    {"Household_Name", "hh name"},
                    {"City", "Cincinnati"},
                    {"State", "OH"},
                    {"Postal_Code", "45208"},
                    {"County", "Hamilton"},
                    {"Anniversary_Date", new DateTime(2013, 8, 5)},
                    {"Contact_ID", 3},
                    {"Date_of_Birth", new DateTime(2007, 5, 29)},
                    {"Email_Address", "email-address@email.com"},
                    {"Employer_Name", "Crossroads"},
                    {"First_Name", "first-name"},
                    {"Display_Name", "Displayname"},
                    {"Current_School", "Currentschool"},
                    {"Foreign_Country", "USA"},
                    {"Gender_ID", null},
                    {"Home_Phone", "513-555-1234"},
                    {"Last_Name", "last-name"},
                    {"Maiden_Name", "maiden-name"},
                    {"Marital_Status_ID", null},
                    {"Middle_Name", "middle-name"},
                    {"Mobile_Carrier_ID", null},
                    {"Mobile_Phone", "513-555-9876"},
                    {"Nickname", "nickname"},
                    {"Age", 30},
                    {"Passport_Number", "12345"},
                    {"Passport_Firstname", "first-name"},
                    {"Passport_Lastname", "last-name"},
                    {"Passport_Country", "USA"},
                    {"Passport_Middlename", "middle-name"},
                    {"Passport_Expiration", "02/21/2020"}
                }
            };

            _ministryPlatformService.Setup(m => m.GetRecordsDict("MyProfile", It.IsAny<string>(), "", ""))
                .Returns(dictionaryList);

            var myProfile = _fixture.GetMyProfile(It.IsAny<string>());

            _ministryPlatformService.VerifyAll();

            Assert.IsNotNull(myProfile);
            Assert.IsNull(myProfile.Address_ID);
            Assert.IsNull(myProfile.Congregation_ID);
            Assert.IsNull(myProfile.Gender_ID);
            Assert.IsNull(myProfile.Marital_Status_ID);
            Assert.IsNull(myProfile.Mobile_Carrier);
            Assert.AreEqual("hh name", myProfile.Household_Name);
        }

        [Test]
        public void ShouldCreateContactForGuestGiver()
        {
            _ministryPlatformService.Setup(
                mocked => mocked.CreateRecord(292, It.IsAny<Dictionary<string, object>>(), It.IsAny<string>(), false))
                .Returns(123);

            var contactId = _fixture.CreateContactForGuestGiver("me@here.com", "display name", "", "");

            _ministryPlatformService.Verify(mocked => mocked.CreateRecord(292,
                                                                          It.Is<Dictionary<string, object>>(d =>
                                                                                                                d["Email_Address"].Equals("me@here.com")
                                                                                                                && d["Company"].Equals(false)
                                                                                                                && d["Display_Name"].Equals("display name")
                                                                                                                && d["Nickname"].Equals("display name")
                                                                                                                && d["Household_Position_ID"].Equals(1)
                                                                              ),
                                                                          It.IsAny<string>(),
                                                                          false));

            Assert.AreEqual(123, contactId);
        }

        [Test]
        public void ShouldCreateContactForGuestGiverGivenFirstnameandLastName()
        {
            _ministryPlatformService.Setup(
                mocked => mocked.CreateRecord(292, It.IsAny<Dictionary<string, object>>(), It.IsAny<string>(), false))
                .Returns(123);

            var contactId = _fixture.CreateContactForGuestGiver("me@here.com", "display name", "firstName", "lastName");

            _ministryPlatformService.Verify(mocked => mocked.CreateRecord(292,
                                                                          It.Is<Dictionary<string, object>>(d =>
                                                                                                                d["Email_Address"].Equals("me@here.com")
                                                                                                                && d["First_Name"].Equals("firstName")
                                                                                                                && d["Last_Name"].Equals("lastName")
                                                                                                                && d["Company"].Equals(false)
                                                                                                                && d["Display_Name"].Equals("display name")
                                                                                                                && d["Nickname"].Equals("display name")
                                                                                                                && d["Household_Position_ID"].Equals(1)
                                                                              ),
                                                                          It.IsAny<string>(),
                                                                          false));

            Assert.AreEqual(123, contactId);
        }

        [Test]
        public void ShouldCreateSimpleContact()
        {
            const string firstname = "Mary";
            const string lastname = "richard";
            const string email = "mary.richard@gmail.com";
            var dob = new DateTime().ToString();
            const string mobile = "5554441111";


            _ministryPlatformService.Setup(
                mocked => mocked.CreateRecord(292, It.IsAny<Dictionary<string, object>>(), "ABC", false))
                .Returns(123);

            var contactId = _fixture.CreateSimpleContact(firstname, lastname, email, dob, mobile);

            Assert.IsInstanceOf<MpContact>(contactId);
            Assert.AreEqual(123, contactId.ContactId);

            _ministryPlatformService.Verify(mocked => mocked.CreateRecord(292,
                                                                          It.Is<Dictionary<string, object>>(d =>
                                                                                                                d["Company"].Equals(false)
                                                                                                                && d["Last_Name"].Equals(lastname)
                                                                                                                && d["First_Name"].Equals(firstname)
                                                                                                                && d["Display_Name"].Equals(lastname + ", " + firstname)
                                                                                                                && d["Nickname"].Equals(firstname)
                                                                              ),
                                                                          It.IsAny<string>(),
                                                                          false));
        }

        [Test]
        public void ShouldGetContactFromParticipant()
        {
            const int participantId = 2375529;
            const int contactId = 2562386;
            var mockContact = new Dictionary<string, object>
            {
                {"Contact_ID", contactId}
            };

            _ministryPlatformService.Setup(mocked => mocked.GetRecordDict(It.IsAny<int>(), participantId, It.IsAny<string>(), It.IsAny<bool>())).Returns(mockContact);

            var result = _fixture.GetContactIdByParticipantId(participantId);
            Assert.AreEqual(contactId, result);
        }

        [Test]
        public void ShouldThrowApplicationExceptionWhenGuestGiverCreationFails()
        {
            var ex = new Exception("Danger, Will Robinson!");
            _ministryPlatformService.Setup(
                mocked => mocked.CreateRecord(292, It.IsAny<Dictionary<string, object>>(), It.IsAny<string>(), false))
                .Throws(ex);

            try
            {
                _fixture.CreateContactForGuestGiver("me@here.com", "display", string.Empty, string.Empty);
                Assert.Fail("Expected exception was not thrown");
            }
            catch (Exception e)
            {
                Assert.IsInstanceOf(typeof(ApplicationException), e);
                Assert.AreSame(ex, e.InnerException);
            }
        }

        [Test]
        public void ShouldThrowApplicationExceptionWhenSimpleContactCreationFails()
        {
            var ex = new Exception("Simple contact creation failed");
            _ministryPlatformService.Setup(
                mocked => mocked.CreateRecord(292, It.IsAny<Dictionary<string, object>>(), It.IsAny<string>(), false))
                .Throws(ex);

            try
            {
                _fixture.CreateSimpleContact("mary", "richard", "mary.richard@gmail.com", "08/09/1981", "5554441111");
                Assert.Fail("Expected exception was not thrown");
            }
            catch (Exception e)
            {
                Assert.IsInstanceOf(typeof(ApplicationException), e);
                Assert.AreSame(ex, e.InnerException);
            }
        }

        [Test]
        public void ShouldGetFamilyWithAges()
        {
            var householdid = 1234567;

            var familyMembersReturned = new List<Dictionary<string, object>>();

            var childOne = new Dictionary<string, object>
            {
                {"Contact_ID", 1},
                {"First_Name", "Bobby"},
                {"Nickname", "Rob"},
                {"Last_Name", "DropTables"},
                {"Date_of_Birth", "2012-01-01"},
                {"__Age", 4},
                {"Household_Position", "Child"}
            };

            var childTwo = new Dictionary<string, object>
            {
                {"Contact_ID", 2},
                {"First_Name", "Jane"},
                {"Nickname", "Jane"},
                {"Last_Name", "DropTables"},
                {"Date_of_Birth", "2014-01-01"},
                {"__Age", 2},
                {"Household_Position", "Child"}
            };


            familyMembersReturned.Add(childOne);
            familyMembersReturned.Add(childTwo);

            var rc =
                _ministryPlatformService.Setup(mocked => mocked.GetSubpageViewRecords("HouseholdMembers", 1234567, It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), 0))
                    .Returns(familyMembersReturned);

            var family = _fixture.GetHouseholdFamilyMembers(householdid);

            Assert.AreEqual(family.Count, 2);
            Assert.AreEqual(family[0].Age, 4);
        }

        [Test]
        public void ShouldGetPrimaryContacts()
        {
            var returnData = new List<Dictionary<string, object>>
            {
                new Dictionary<string, object>
                {
                    {"Contact_ID", 1},
                    {"Display_Name", "Nukem, Duke"},
                    {"Email_Address", "Duke.Nukem@compuserv.net"},
                    {"Extra_Data", 3}
                },
                new Dictionary<string, object>
                {
                    {"Contact_ID", 2},
                    {"Display_Name", "Croft, Lara"},
                    {"Email_Address", "Lara.Croft@gmail.com"}
                }
            };

            const string columns = "Contact_ID_Table.Contact_ID, Contact_ID_Table.Display_Name, Contact_ID_Table.Email_Address, Contact_ID_Table.First_Name, Contact_ID_Table.Last_Name";
            string filter = $"Attribute_ID IN (7088,9048) AND Start_Date <= GETDATE() AND (End_Date IS NULL OR End_Date > GETDATE())";

            _ministryPlatformRest.Setup(m => m.Search<MpContactAttribute, Dictionary<string, object>>(filter, columns, "Display_Name", true)).Returns(returnData);

            var result = _fixture.PrimaryContacts();

            _ministryPlatformRest.VerifyAll();

            Assert.AreEqual(result, returnData);
        }

        [Test]
        public void ShouldGetContactByUserRecordId()
        {
            string tableName = "Contacts";
            const int userRecordId = 1234567;
            Dictionary<string, object> expectedFilter = new Dictionary<string, object>()
            {
                {"User_Account", userRecordId}
            };

            MpMyContact contact = new MpMyContact()
            {
                Contact_ID = 2,
                First_Name = "Testy",
                Email_Address = "testy@mctestface.com",
                Age = 30,
                Mobile_Phone = "1234567890"
            };

            List<MpMyContact> mockResults = new List<MpMyContact>
            {
                contact
            };

            _ministryPlatformRest.Setup(m => m.UsingAuthenticationToken(It.IsAny<string>())).Returns(_ministryPlatformRest.Object);

            _ministryPlatformRest.Setup(m => m.Get<MpMyContact>(tableName, expectedFilter))
                .Returns(mockResults);

            var result = _fixture.GetContactByUserRecordId(userRecordId);

            Assert.AreEqual(contact, result);
        }

        [Test]
        public void TestGetContactByUserRecordIdNoContact()
        {
            string tableName = "Contacts";
            const int userRecordId = 1234567;
            Dictionary<string, object> expectedFilter = new Dictionary<string, object>()
            {
                {"User_Account", userRecordId}
            };

            List<MpMyContact> mockResults = new List<MpMyContact>();

            _ministryPlatformRest.Setup(m => m.UsingAuthenticationToken(It.IsAny<string>())).Returns(_ministryPlatformRest.Object);

            _ministryPlatformRest.Setup(m => m.Get<MpMyContact>(tableName, expectedFilter))
                .Returns(mockResults);

            var result = _fixture.GetContactByUserRecordId(userRecordId);

            Assert.IsNull(result);
        }

        [Test]
        public void ShouldGetOtherHouseholdMembers()
        {
            const int householdId = 999;
            var householdContacts = GetContactHouseholds(householdId);
            _ministryPlatformRest.Setup(m => m.Search<MpContactHousehold>(It.IsAny<string>(), It.IsAny<List<string>>(), null, false)).Returns(householdContacts);

            var householdMembers = _fixture.GetOtherHouseholdMembers(householdId);
            Assert.AreEqual(1, householdMembers.Count);
            Assert.AreEqual("Minor Child", householdMembers.First().HouseholdPosition);            
        }     

        public static List<MpContactHousehold> GetContactHouseholds(int householdId)
        {
            return new List<MpContactHousehold>
            {
                new MpContactHousehold() {ContactId = 123445, HouseholdId = householdId, HouseholdPositionId = 2, Age = 10, DateOfBirth = null, FirstName = "Ellie", LastName = "Canterbury", HouseholdPosition = "Minor Child"},
                new MpContactHousehold() {ContactId = 54321, HouseholdId = householdId, HouseholdPositionId = 1, Age = 59, DateOfBirth = null, FirstName = "Ella", LastName = "Robey", HouseholdPosition = "Adult", EndDate = DateTime.Now.AddDays(-3)}
            };
        }
    }
}
using System;
using System.Collections.Generic;
using Crossroads.Web.Common.Configuration;
using Crossroads.Web.Common.MinistryPlatform;
using Crossroads.Web.Common.Security;
using MinistryPlatform.Translation.Models;
using MinistryPlatform.Translation.Models.Finder;
using MinistryPlatform.Translation.Repositories;
using Moq;
using NUnit.Framework;

namespace MinistryPlatform.Translation.Test.Services
{
    [TestFixture]
    public class FinderRepositoryTest
    {
        private Mock<IMinistryPlatformRestRepository> _ministryPlatformRestRepository;
        private Mock<IAuthenticationRepository> _authenticationService;  
        private Mock<IConfigurationWrapper> _config;
        private Mock<IApiUserRepository> _apiUserRepo;

        private List<string> _groupColumns = new List<string>
            {
                "Groups.Group_ID",
                "Groups.Group_Name",
                "Groups.Description",
                "Groups.Start_Date",
                "Groups.End_Date",
                "Offsite_Meeting_Address_Table.*",
                "Groups.Available_Online",
                "Groups.Primary_Contact",
                "Groups.Congregation_ID",
                "Groups.Ministry_ID",
                "Groups.Kids_Welcome"
            };

        private FinderRepository _fixture;


        [SetUp]
        public void SetUp()
        {
            _ministryPlatformRestRepository = new Mock<IMinistryPlatformRestRepository>();
            _authenticationService = new Mock<IAuthenticationRepository>();
            _config = new Mock<IConfigurationWrapper>();
            _apiUserRepo = new Mock<IApiUserRepository>();

            _ministryPlatformRestRepository.Setup(m => m.UsingAuthenticationToken("abc")).Returns(_ministryPlatformRestRepository.Object);

            _fixture = new FinderRepository(_config.Object, _ministryPlatformRestRepository.Object, _apiUserRepo.Object, _authenticationService.Object);
        }

        [Test]
        public void getFinderPinDetails()
        {
            var participantId = 123;

            var response = new FinderPinDto
            {
                Address = null,
                LastName = "Kerstanoff",
                Contact_ID = 12,
                EmailAddress = "JoeKer@gmail.com",
                FirstName = "Joe",
                Household_ID = 2,
                Participant_ID = 123
            };
            var addressResponse = new MpAddress {Address_ID = 1, Address_Line_1 = "123 street", City = "City!", Postal_Code = "12345"};
            _apiUserRepo.Setup(m => m.GetDefaultApiClientToken()).Returns("abc");
            _ministryPlatformRestRepository.Setup(
                mocked =>
                    mocked.Search<FinderPinDto>(
                        It.Is<string>(
                            m =>
                                m.Equals(
                                    $"Participant_Record = {participantId}")),
                        It.Is<string>(
                            m =>
                                m.Equals(
                                    "Email_Address, Nickname as FirstName, Last_Name as LastName, Participant_Record_Table.*, Household_ID")),
                        It.IsAny<string>(),
                        It.IsAny<bool>())).Returns(new List<FinderPinDto> { response });

            _ministryPlatformRestRepository.Setup(
                mocked =>
                    mocked.Search<MpContact, MpAddress>(
                        It.Is<string>(
                            m =>
                                m.Equals(
                                    $"Participant_Record = {participantId}")),
                        It.Is<string>(
                            m =>
                                m.Equals(
                                    "Household_ID_Table_Address_ID_Table.*")),
                        It.IsAny<string>(),
                        It.IsAny<bool>())).Returns(new List<MpAddress> { addressResponse });

            var value = _fixture.GetPinDetails(123);
            _ministryPlatformRestRepository.VerifyAll();

            Assert.AreEqual(value.LastName, response.LastName);
            Assert.AreEqual(value.Address.Address_ID, response.Address.Address_ID);

        }

        [Test]
        public void ShouldEnablePin()
        {
            _apiUserRepo.Setup(m => m.GetDefaultApiClientToken()).Returns("abc");
            _ministryPlatformRestRepository.Setup(
                mocked =>
                        mocked.Put("Participants", It.IsAny<List<Dictionary<string, object>>>())
            );

            _fixture.EnablePin(123);
            _ministryPlatformRestRepository.VerifyAll();

        }

        [Test]
        public void ShouldUpdateGathering()
        {
            _apiUserRepo.Setup(m => m.GetDefaultApiClientToken()).Returns("abc");
            

            var gathering = this.GetAGatheringDto();

            _ministryPlatformRestRepository.Setup(mocked => mocked.Update<FinderGatheringDto>(gathering, _groupColumns)).Returns(gathering);

            _fixture.UpdateGathering(gathering);
            _ministryPlatformRestRepository.VerifyAll();
        }

        private FinderGatheringDto GetAGatheringDto(int designator = 1)
        {
            return new FinderGatheringDto()
            {
                Address = this.GetAnAddress(),
                AvailableOnline = true,
                ChildCareAvailable = false,
                CongregationId = designator,
                Congregation = "Congregation",
                EndDate = null,
                Full = false,
                GroupDescription = "This is not the greatest group in the world",
                GroupId = designator,
                GroupRoleId = designator,
                PrimaryContact = designator,
                GroupType = 30,
                Name = "Test Gathering",
                MinistryId = designator,
                StartDate = DateTime.Now
            };
        }

        private MpAddress GetAnAddress(int designator = 1)
        {
            return new MpAddress()
            {
                Address_ID = designator,
                Address_Line_1 = $"{designator} Test Street",
                Address_Line_2 = null,
                City = "City!",
                County = "county",
                Foreign_Country = "USA",
                Latitude = designator,
                Longitude = designator,
                Postal_Code = "12345",
                State = "OH"
            };
        }

        [Test]
        public void ShouldDisablePin()
        {
            _apiUserRepo.Setup(m => m.GetDefaultApiClientToken()).Returns("abc");
            _ministryPlatformRestRepository.Setup(
                mocked =>
                        mocked.Put("Participants", It.IsAny<List<Dictionary<string, object>>>())
            );

            _fixture.DisablePin(123);
            _ministryPlatformRestRepository.VerifyAll();

        }
    }
}

﻿using System.Collections.Generic;
using System.Device.Location;
using System.Linq;
using crds_angular.App_Start;
using crds_angular.Models.Crossroads;
using crds_angular.Models.Finder;
using crds_angular.Services;
using crds_angular.Services.Interfaces;
using MinistryPlatform.Translation.Models;
using MinistryPlatform.Translation.Models.Finder;
using Moq;
using NUnit.Framework;
using MinistryPlatform.Translation.Repositories.Interfaces;
using AutoMapper;
using crds_angular.Models.Crossroads.Groups;
using Crossroads.Web.Common.Configuration;
using Crossroads.Web.Common.MinistryPlatform;
using Rhino.Mocks;
using Amazon.CloudSearchDomain.Model;
using crds_angular.Models.AwsCloudsearch;

namespace crds_angular.test.Services
{
    [TestFixture]
    public class FinderServiceTest
    {
        private FinderService _fixture;
        private Mock<IAddressGeocodingService> _addressGeocodingService;
        private Mock<IFinderRepository> _mpFinderRepository;
        private Mock<IContactRepository> _mpContactRepository;
        private Mock<IAddressService> _addressService;
        private Mock<IParticipantRepository> _mpParticipantRepository;
        private Mock<IConfigurationWrapper> _mpConfigurationWrapper;
        private Mock<IGroupToolService> _mpGroupToolService;
        private Mock<IGroupService> _groupService;
        private Mock<IApiUserRepository> _apiUserRepository;
        private Mock<IAddressProximityService> _addressProximityService;
        private Mock<IInvitationService> _invitationService;
        private Mock<IAwsCloudsearchService> _awsCloudsearchService;

        private int _memberRoleId = 16;
        private int _anywhereGatheringInvitationTypeId = 3;

        [SetUp]
        public void SetUp()
        {
            _addressGeocodingService = new Mock<IAddressGeocodingService>();
            _mpFinderRepository = new Mock<IFinderRepository>();
            _mpContactRepository = new Mock<IContactRepository>();
            _addressService = new Mock<IAddressService>();
            _mpParticipantRepository = new Mock<IParticipantRepository>();
            _mpGroupToolService = new Mock<IGroupToolService>();
            _mpConfigurationWrapper = new Mock<IConfigurationWrapper>();
            _addressProximityService = new Mock<IAddressProximityService>();
            _apiUserRepository = new Mock<IApiUserRepository>();
            _groupService = new Mock<IGroupService>();
            _invitationService = new Mock<IInvitationService>();
            _awsCloudsearchService = new Mock<IAwsCloudsearchService>();

            _mpConfigurationWrapper.Setup(mocked => mocked.GetConfigIntValue("GroupRoleLeader")).Returns(22);
            _mpConfigurationWrapper.Setup(mocked => mocked.GetConfigIntValue("ApprovedHostStatus")).Returns(3);
            _mpConfigurationWrapper.Setup(mocked => mocked.GetConfigIntValue("AnywhereGroupTypeId")).Returns(30);
            _mpConfigurationWrapper.Setup(mocked => mocked.GetConfigIntValue("Group_Role_Default_ID")).Returns(_memberRoleId);
            _mpConfigurationWrapper.Setup(mocked => mocked.GetConfigIntValue("AnywhereGatheringInvitationType")).Returns(_anywhereGatheringInvitationTypeId);

            _fixture = new FinderService(_addressGeocodingService.Object,
                                         _mpFinderRepository.Object,
                                         _mpContactRepository.Object,
                                         _addressService.Object,
                                         _mpParticipantRepository.Object,
                                         _groupService.Object,
                                         _mpGroupToolService.Object,
                                         _apiUserRepository.Object,
                                         _mpConfigurationWrapper.Object,
                                         _invitationService.Object,
                                         _awsCloudsearchService.Object);

            //force AutoMapper to register
            AutoMapperConfig.RegisterMappings();
        }

        [Test]
        public void ShouldGetPinDetails()
        {
            _apiUserRepository.Setup(ar => ar.GetToken()).Returns("abc123");
            _mpFinderRepository.Setup(m => m.GetPinDetails(123))
                .Returns(new FinderPinDto
                {
                    LastName = "Ker",
                    FirstName = "Joe",
                    Address = new MpAddress {Address_ID = 12, Postal_Code = "1234", Address_Line_1 = "123 street", City = "City", State = "OH"},
                    Participant_ID = 123,
                    EmailAddress = "joeker@gmail.com",
                    Contact_ID = 22,
                    Household_ID = 13,
                    Host_Status_ID = 3
                });

            _groupService.Setup(gs => gs.GetGroupsByTypeOrId("abc123", 123, new int[] {30}, (int?) null))
                .Returns(new List<GroupDTO>
                {
                    new GroupDTO
                    {
                        GroupId = 4444444,
                        Participants = new List<GroupParticipantDTO>
                        {
                            new GroupParticipantDTO
                            {
                                GroupRoleId = 22,
                                ParticipantId = 222

                            }
                        }
                    },
                    new GroupDTO
                    {
                        GroupId = 8675309,
                        Participants = new List<GroupParticipantDTO>
                        {
                            new GroupParticipantDTO
                            {
                                GroupRoleId = 22,
                                ParticipantId = 123

                            }
                        }
                    }
                });

            var result = _fixture.GetPinDetails(123);

            _mpFinderRepository.VerifyAll();

            Assert.AreEqual(result.LastName, "Ker");
            Assert.AreEqual(result.Address.AddressID, 12);
            Assert.AreEqual(result.Gathering.GroupId, 8675309);
        }

        [Test]
        public void ShouldEnablePin()
        {
            _mpFinderRepository.Setup(m => m.EnablePin(123));
            _fixture.EnablePin(123);
            _mpFinderRepository.VerifyAll();
        }

        [Test]
        public void ShouldGetGeoCoordinatesFromLatLang()
        {
            const string address = "123 Main Street, Walton, KY";

            var mockCoords = new GeoCoordinate()
            {
                Latitude = 39.2844738,
                Longitude = -84.319614
            };

            _addressGeocodingService.Setup(mocked => mocked.GetGeoCoordinates(address)).Returns(mockCoords);

            GeoCoordinate geoCoords = _fixture.GetGeoCoordsFromAddressOrLatLang(address, "39.2844738", "-84.319614");
            Assert.AreEqual(mockCoords, geoCoords);
        }

        [Test]
        public void ShouldGetGeoCoordinatesFromAddress()
        {
            const string address = "123 Main Street, Walton, KY";

            var mockCoords = new GeoCoordinate()
            {
                Latitude = 39.2844738,
                Longitude = -84.319614
            };

            _addressGeocodingService.Setup(mocked => mocked.GetGeoCoordinates(address)).Returns(mockCoords);

            GeoCoordinate geoCoords = _fixture.GetGeoCoordsFromAddressOrLatLang(address, "0", "0");
            Assert.AreEqual(mockCoords, geoCoords);
        }

        [Test]
        public void ShouldReturnAListOfPinsWhenSearching()
        {
            const string address = "123 Main Street, Walton, KY";
            var originCoords = new GeoCoordinate()
            {
                Latitude = 39.2844738,
                Longitude = -84.319614
            };

            var searchresults = new SearchResponse();
            searchresults.Hits = new Hits();
            searchresults.Hits.Found = 1;
            searchresults.Hits.Start = 0;
            searchresults.Hits.Hit = new List<Hit>();
            var hit = new Hit();
            var fields = new Dictionary<string, List<string>>();
            fields.Add("city", new List<string>() { "Union" });
            fields.Add("zip", new List<string>() { "41091" });
            fields.Add("firstname", new List<string>() { "Robert" });
            fields.Add("lastname", new List<string>() { "Smith" });
            fields.Add("latlong", new List<string>() { "38.94526,-84.661275" });
            hit.Fields = fields;
            searchresults.Hits.Hit.Add(hit);

            _awsCloudsearchService.Setup(mocked => mocked.SearchConnectAwsCloudsearch("matchall", "_all_fields",null,null)).Returns(searchresults);

            _mpConfigurationWrapper.Setup(mocked => mocked.GetConfigIntValue("AnywhereGroupTypeId")).Returns(30);
            _mpGroupToolService.Setup(m => m.SearchGroups(It.IsAny<int[]>(), null, It.IsAny<string>(), null, originCoords)).Returns(new List<GroupDTO>());
            _mpFinderRepository.Setup(mocked => mocked.GetPinsInRadius(originCoords)).Returns(new List<SpPinDto>());
            _addressGeocodingService.Setup(mocked => mocked.GetGeoCoordinates(address)).Returns(originCoords);
            _addressProximityService.Setup(mocked => mocked.GetProximity(address, new List<AddressDTO>(), originCoords)).Returns(new List<decimal?>());
            _addressProximityService.Setup(mocked => mocked.GetProximity(address, new List<string>(), originCoords)).Returns(new List<decimal?>());

            var boundingBox = new AwsBoundingBox
            {
                UpperLeftCoordinates = new GeoCoordinates(61.21, -149.9),
                BottomRightCoordinates = new GeoCoordinates(21.52, -77.78)
            };

            List<PinDto> pins = _fixture.GetPinsInBoundingBox(originCoords, address, boundingBox);

            Assert.IsInstanceOf<List<PinDto>>(pins);
        }

        public void ShouldRandomizeThePosition()
        {
            const double originalLatitude = 59.6378639;
            const double originalLongitude = -151.5068732;

            var address = new AddressDTO
            {
                AddressID = 222,
                AddressLine1 = "1393 Bay Avenue",
                City = "Homer",
                State = "AK",
                PostalCode = "99603",
                Latitude = originalLatitude,
                Longitude = originalLongitude
            };

            var result = _fixture.RandomizeLatLong(address);
            Assert.AreNotEqual(result.Longitude, originalLongitude);
            Assert.AreNotEqual(result.Latitude, originalLatitude);
        }

        [Test]
        public void ShouldUpdateHouseholdAddress()
        {
            var pin = new PinDto
            {
                Address = new AddressDTO
                {
                    AddressID = 741,
                    AddressLine1 = "123 Main Street",
                    City = "Cincinnati",
                    State = "OH",
                    PostalCode = "45249"
                },
                Contact_ID = 123,
                Participant_ID = 456,
                Household_ID = 789,
                EmailAddress = "",
                FirstName = "",
                LastName = "",
                Gathering = null,
                Host_Status_ID = 0
            };

            var address = Mapper.Map<MpAddress>(pin.Address);
            var addressDictionary = new Dictionary<string, object>
            {
                {"AddressID", pin.Address.AddressID},
                {"AddressLine1", pin.Address.AddressID},
                {"City", pin.Address.AddressID},
                {"State/Region", pin.Address.AddressID},
                {"PostCode", pin.Address.AddressID}
            };
            var householdDictionary = new Dictionary<string, object> {{"Household_ID", pin.Household_ID}};

            _addressService.Setup(m => m.SetGeoCoordinates(pin.Address));
            _mpContactRepository.Setup(m => m.UpdateHouseholdAddress((int) pin.Household_ID, householdDictionary, addressDictionary));
            _fixture.UpdateHouseholdAddress(pin);
            _mpFinderRepository.VerifyAll();
        }

        [Test]
        public void ShouldInviteToGathering()
        {
            string token = "abc";
            int gatheringId = 12345;
            User person = new User()
            {
                firstName = "doug",
                lastName = "shannon",
                email = "a@b.com",
            };

            Invitation expectedInvitation = new Invitation()
            {
                RecipientName = person.firstName,
                EmailAddress = person.email,
                SourceId = gatheringId,
                GroupRoleId = _memberRoleId,
                InvitationType = _anywhereGatheringInvitationTypeId
            };

            _invitationService.Setup(i => i.ValidateInvitation(It.Is<Invitation>(
                                                                   (inv) => inv.RecipientName == expectedInvitation.RecipientName
                                                                            && inv.EmailAddress == expectedInvitation.EmailAddress
                                                                            && inv.SourceId == expectedInvitation.SourceId
                                                                            && inv.GroupRoleId == expectedInvitation.GroupRoleId
                                                                            && inv.InvitationType == expectedInvitation.InvitationType),
                                                               It.Is<string>((s) => s == token)));

            _invitationService.Setup(i => i.CreateInvitation(It.Is<Invitation>(
                                                                   (inv) => inv.RecipientName == expectedInvitation.RecipientName
                                                                            && inv.EmailAddress == expectedInvitation.EmailAddress
                                                                            && inv.SourceId == expectedInvitation.SourceId
                                                                            && inv.GroupRoleId == expectedInvitation.GroupRoleId
                                                                            && inv.InvitationType == expectedInvitation.InvitationType),
                                                               It.Is<string>((s) => s == token)));
            _fixture.InviteToGathering(token, gatheringId, person);
            _invitationService.VerifyAll();
        }
    }
}

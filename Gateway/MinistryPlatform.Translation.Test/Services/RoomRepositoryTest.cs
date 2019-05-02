using System;
using System.Collections.Generic;
using Crossroads.Utilities.Interfaces;
using FsCheck;
using Crossroads.Web.Common;
using Crossroads.Web.Common.Configuration;
using Crossroads.Web.Common.MinistryPlatform;
using Crossroads.Web.Common.Security;
using MinistryPlatform.Translation.Models;
using MinistryPlatform.Translation.PlatformService;
using MinistryPlatform.Translation.Repositories;
using Moq;
using NUnit.Framework;
using MinistryPlatform.Translation.Repositories.Interfaces;
using MinistryPlatform.Translation.Test.Helpers;

namespace MinistryPlatform.Translation.Test.Services
{
    public class RoomRepositoryTest
    {
        private Mock<IMinistryPlatformService> _ministryPlatformService;
        private Mock<IMinistryPlatformRestRepository> _ministryPlatformRestRepository;

        private Mock<IConfigurationWrapper> _config;
        private Mock<IAuthenticationRepository> _authenticationService;
        private Mock<IApiUserRepository> _apiUserService;

        private RoomRepository _fixture;
        public const string GetRoomsProcName = "api_crds_GetReservedAndAvailableRoomsByLocation";

        [SetUp]
        public void SetUp()
        {
            _ministryPlatformRestRepository = new Mock<IMinistryPlatformRestRepository>();
            _ministryPlatformService = new Mock<IMinistryPlatformService>();
            _config = new Mock<IConfigurationWrapper>();
            _authenticationService = new Mock<IAuthenticationRepository>();
            _apiUserService = new Mock<IApiUserRepository>();

            _ministryPlatformRestRepository.Setup(m => m.UsingAuthenticationToken("abc")).Returns(_ministryPlatformRestRepository.Object);

            _authenticationService.Setup(m => m.AuthenticateClient(It.IsAny<string>(), It.IsAny<string>())).Returns(new AuthToken
            {
                AccessToken = "abc",
                ExpiresIn = 123
            });

            _fixture = new RoomRepository(_ministryPlatformService.Object, _ministryPlatformRestRepository.Object, _authenticationService.Object, _config.Object, _apiUserService.Object);
        }

        [Test]
        public void TestGetRoomReservations()
        {
            var l = new List<Dictionary<string, object>>
            {
                new Dictionary<string, object>
                {
                    {"Cancelled", false},
                    {"Event_Room_ID", 1},
                    {"Hidden", false},
                    {"Notes", "Notes 1"},
                    {"Room_ID", 11},
                    {"Room_Layout_ID", 111},
                    {"Capacity", 1111},
                    {"Label", "Label 1"},
                    {"Room_Name", "Name 1"},
                    {"Allow_Checkin", false},
                    {"Volunteers", 1},
                    {"_Approved", 1 },
                    {"Rejected", 0 }
                },
                new Dictionary<string, object>
                {
                    {"Cancelled", true},
                    {"Event_Room_ID", 2},
                    {"Hidden", true},
                    {"Notes", "Notes 2"},
                    {"Room_ID", 22},
                    {"Room_Layout_ID", 222},
                    {"Capacity", 2222},
                    {"Label", "Label 2"},
                    {"Room_Name", "Name 2"},
                    {"Allow_Checkin", true},
                    {"Volunteers", 10},
                    {"_Approved", 1 },
                    {"Rejected", 0 }
                },
                new Dictionary<string, object>
                {
                    {"Cancelled", true},
                    {"Event_Room_ID", 3},
                    {"Hidden", true},
                    {"Notes", "Notes 3"},
                    {"Room_ID", 33},
                    {"Room_Layout_ID", 333},
                    {"Capacity", 3333},
                    {"Label", "Label 3"},
                    {"Room_Name", "Name 3"},
                    {"Allow_Checkin", null},
                    {"Volunteers", 11},
                    {"_Approved", 1 },
                    {"Rejected", 0 }
                }
            };

            _ministryPlatformService.Setup(mocked => mocked.GetPageViewRecords("GetRoomReservations", It.IsAny<string>(), ",\"123\"", "", 0)).Returns(l);

            var reservations = _fixture.GetRoomReservations(123);
            _ministryPlatformService.VerifyAll();
            _authenticationService.VerifyAll();
            _config.VerifyAll();

            Assert.IsNotNull(reservations);
            Assert.AreEqual(l.Count, reservations.Count);

            for (var i = 0; i < l.Count; i++)
            {
                Assert.AreEqual(l[i]["Cancelled"], reservations[i].Cancelled);
                Assert.AreEqual(l[i]["Event_Room_ID"], reservations[i].EventRoomId);
                Assert.AreEqual(l[i]["Hidden"], reservations[i].Hidden);
                Assert.AreEqual(l[i]["Notes"], reservations[i].Notes);
                Assert.AreEqual(l[i]["Room_ID"], reservations[i].RoomId);
                Assert.AreEqual(l[i]["Room_Layout_ID"], reservations[i].RoomLayoutId);
                Assert.AreEqual(l[i]["Capacity"], reservations[i].Capacity);
                Assert.AreEqual(l[i]["Label"], reservations[i].Label);
                Assert.AreEqual(l[i]["Room_Name"], reservations[i].Name);

                // this is to handle a null value for allow checkin values, which is getting set to false if null
                var allowCheckinValue = l[i]["Allow_Checkin"];

                if (allowCheckinValue != null)
                {
                    Assert.AreEqual(l[i]["Allow_Checkin"], reservations[i].CheckinAllowed);
                }
                else
                {
                    Assert.AreEqual(false, reservations[i].CheckinAllowed);
                }

                Assert.AreEqual(l[i]["Volunteers"], reservations[i].Volunteers);
            }
        }

        [Test]
        public void ShouldDeleteEventRoomsForEvent()
        {
            Prop.ForAll<string, int, int>((token, selectionId, eventId) =>
            {
                var searchString = string.Format(",\"{0}\"", eventId);
                var eventRooms = GetMockedEventRooms(3);

                _ministryPlatformService.Setup(m => m.GetPageViewRecords(It.IsAny<string>(), It.IsAny<string>(), searchString, "", 0)).Returns(eventRooms);
                _apiUserService.Setup(mocked => mocked.GetDefaultApiClientToken()).Returns(token);

                var eventRoomIds = Conversions.BuildIntArrayFromKeyValue(eventRooms, "Event_Room_ID").ToArray();

                _ministryPlatformRestRepository.Setup(m => m.UsingAuthenticationToken(token)).Returns(_ministryPlatformRestRepository.Object);

                _fixture.DeleteEventRoomsForEvent(eventId);
                _ministryPlatformService.VerifyAll();
            }).QuickCheckThrowOnFailure();
        }

        [Test]
        public void ThouShaltGetRoomsByLocation()
        {
            var date = DateTime.Now;
            var rooms = GetMockedRoomsByLocation();
            var returnList = new List<List<MpRoom>>();
            returnList.Add(rooms);
            var parms = new Dictionary<string, object>
            {
                {"@StartDate", date},
                {"@EndDate", string.Join(",", date)},
                {"@LocationId", string.Join(",", 1)}
            };

            _ministryPlatformRestRepository.Setup(m => m.GetFromStoredProc<MpRoom>(GetRoomsProcName, parms)).Returns(returnList);

            var result = _fixture.GetRoomsByLocationId(1, date, date);
            Assert.AreEqual(result.Count, rooms.Count);
            Assert.AreEqual(result[0].RoomName, rooms[0].RoomName);

        }

        [Test]
        public void RoomsByLocationCanBeNull()
        {
            var date = DateTime.Now;
            var parms = new Dictionary<string, object>
            {
                {"@StartDate", date},
                {"@EndDate", string.Join(",", date)},
                {"@LocationId", string.Join(",", 1)}
            };

            _ministryPlatformRestRepository.Setup(m => m.GetFromStoredProc<MpRoom>(GetRoomsProcName, parms)).Returns((List<List<MpRoom>>) null);

            var result = _fixture.GetRoomsByLocationId(1, date, date);
            Assert.IsNull(result);
        }

        private List<MpRoom> GetMockedRoomsByLocation()
        {
            return new List<MpRoom>()
            {
                new MpRoom()
                {
                    RoomName = "Roomy McRoomFace",
                    RoomId = 1,
                    Description = "This is the name that won...",
                    BanquetCapacity = 20,
                    TheaterCapacity = 50,
                    LocationId = 1,
                    BuildingId = 1,
                    DisplayName = "Kerstanoff, Joe",
                    RoomStatus = true
                }, new MpRoom()
                {
                    RoomName = "Tribute to the best room in the world",
                    RoomId = 2,
                    Description = "This is just a tribute",
                    BanquetCapacity = 42,
                    TheaterCapacity = 42,
                    LocationId = 1,
                    BuildingId = 1,
                    DisplayName = null,
                    RoomStatus = null
                }, new MpRoom()
                {
                    RoomName = "Pending Room",
                    RoomId = 3,
                    Description = "This room has a pending reservation",
                    BanquetCapacity = 1,
                    TheaterCapacity = 0,
                    LocationId = 1,
                    BuildingId = 1,
                    DisplayName = "Nukem, Duke",
                    RoomStatus = false
                }
            };
        }

        private List<Dictionary<string, object>> GetMockedEventRooms(int recordsToGenerate)
        {
            var recordsList = new List<Dictionary<string, object>>();

            for (var i = 0; i < recordsToGenerate; i++)
            {
                recordsList.Add(new Dictionary<string, object>
                {
                    {"Cancelled", Gen.Sample(1, 1, Gen.OneOf(Arb.Generate<bool>())).HeadOrDefault},
                    {"Event_Room_ID", Gen.Sample(7, 1, Gen.OneOf(Arb.Generate<int>())).HeadOrDefault},
                    {"Hidden", Gen.Sample(1, 1, Gen.OneOf(Arb.Generate<bool>())).HeadOrDefault},
                    {"Notes", Gen.Sample(75, 1, Gen.OneOf(Arb.Generate<string>())).HeadOrDefault},
                    {"Room_ID", Gen.Sample(7, 1, Gen.OneOf(Arb.Generate<int>())).HeadOrDefault},
                    {"Room_Layout_ID", Gen.Sample(7, 1, Gen.OneOf(Arb.Generate<int>())).HeadOrDefault},
                    {"Capacity", Gen.Sample(3, 1, Gen.OneOf(Arb.Generate<int>())).HeadOrDefault},
                    {"Label", Gen.Sample(75, 1, Gen.OneOf(Arb.Generate<string>())).HeadOrDefault},
                    {"Room_Name", Gen.Sample(75, 1, Gen.OneOf(Arb.Generate<string>())).HeadOrDefault},
                    {"Allow_Checkin", Gen.Sample(1, 1, Gen.OneOf(Arb.Generate<bool>())).HeadOrDefault},
                    {"Volunteers", Gen.Sample(1, 1, Gen.OneOf(Arb.Generate<int>())).HeadOrDefault},
                    {"_Approved", Gen.Sample(1, 1, Gen.OneOf(Arb.Generate<bool>())).HeadOrDefault},
                    {"Rejected", Gen.Sample(1, 1, Gen.OneOf(Arb.Generate<bool>())).HeadOrDefault},
                });
            }

            return recordsList;
        }
    }
}

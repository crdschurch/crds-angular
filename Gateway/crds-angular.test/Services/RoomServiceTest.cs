using System;
using System.Collections.Generic;
using crds_angular.Services;
using MinistryPlatform.Translation.Models;
using MinistryPlatform.Translation.Repositories.Interfaces;
using Moq;
using NUnit.Framework;

namespace crds_angular.test.Services
{
    public class RoomServiceTest
    {
        private Mock<IRoomRepository> _roomRepository;
        private Mock<IEquipmentRepository> _equipmentRepository;
        private Mock<IEventRepository> _eventRepository;

        private RoomService _fixture;

        [SetUp]
        public void Setup()
        {
            _roomRepository = new Mock<IRoomRepository>();
            _equipmentRepository = new Mock<IEquipmentRepository>();
            _eventRepository = new Mock<IEventRepository>();

            _fixture = new RoomService(_roomRepository.Object, _equipmentRepository.Object, _eventRepository.Object);
        }

        [Test]
        public void GetRoomsReturnsNull()
        {
            var date = DateTime.Now;
            _roomRepository.Setup(m => m.GetRoomsByLocationId(1, date, date)).Returns((List<MpRoom>) null);

            var result = _fixture.GetRoomsByLocationId(1, date, date);

            Assert.IsNull(result);
        }

        [Test]
        public void GetRoomsShouldReturnRooms()
        {
            var date = DateTime.Now;
            var rooms = GetARooms();

            _roomRepository.Setup(m => m.GetRoomsByLocationId(1, date, date)).Returns(rooms);

            var result = _fixture.GetRoomsByLocationId(1, date, date);

            Assert.AreEqual(rooms.Count, result.Count);
            Assert.AreEqual(rooms[2].RoomName, result[0].Name, "Because rooms are ordered by name");
        }

        [Test]
        public void GetLayoutsShouldReturnLayouts()
        {
            var returnObject = new List<MinistryPlatform.Translation.Models.RoomLayout>()
            {
                new MinistryPlatform.Translation.Models.RoomLayout
                {
                    LayoutId = 1,
                    LayoutName = "Super Awesome Layout"
                },
                new MinistryPlatform.Translation.Models.RoomLayout
                {
                    LayoutId = 2,
                    LayoutName = "Super Lame Layout"
                }

            };

            _roomRepository.Setup(m => m.GetRoomLayouts()).Returns(returnObject);

            var result = _fixture.GetRoomLayouts();
            Assert.AreEqual(result.Count, returnObject.Count);
            Assert.AreEqual(result[0].LayoutName, returnObject[0].LayoutName);
        }


        private List<MpRoom> GetARooms()
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

    }
}

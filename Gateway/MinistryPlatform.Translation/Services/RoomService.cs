﻿using System;
using System.Collections.Generic;
using System.Linq;
using Crossroads.Utilities.Interfaces;
using log4net;
using MinistryPlatform.Translation.Extensions;
using MinistryPlatform.Translation.Models;
using MinistryPlatform.Translation.Models.EventReservations;
using MinistryPlatform.Translation.PlatformService;
using MinistryPlatform.Translation.Services.Interfaces;

namespace MinistryPlatform.Translation.Services
{
    public class RoomService : BaseService, IRoomService
    {
        private readonly IMinistryPlatformService _ministryPlatformService;
        private readonly ILog _logger = LogManager.GetLogger(typeof (RoomService));

        public RoomService(IMinistryPlatformService ministryPlatformService, IAuthenticationService authenticationService, IConfigurationWrapper configuration)
            : base(authenticationService, configuration)
        {
            _ministryPlatformService = ministryPlatformService;
        }

        public List<RoomReservationDto> GetRoomReservations(int eventId)
        {
            var token = ApiLogin();
            var search = string.Format(",\"{0}\"", eventId);
            var records = _ministryPlatformService.GetPageViewRecords("GetRoomReservations", token, search);

            return records.Select(record => new RoomReservationDto
            {
                Cancelled = record.ToBool("Cancelled"),
                EventRoomId = record.ToInt("Event_Room_ID"),
                Hidden = record.ToBool("Hidden"),
                Notes = record.ToString("Notes"),
                RoomId = record.ToInt("Room_ID"),
                RoomLayoutId = record.ToInt("Room_Layout_ID"),
                Capacity = record.ToNullableInt("Capacity") ?? 0,
                Label = record.ToString("Label"),
                Name = record.ToString("Room_Name"),
                CheckinAllowed = record.ToNullableBool("Allow_Checkin") ?? false
            }).ToList();
        }

        public int CreateRoomReservation(RoomReservationDto roomReservation, string token)
        {
            var roomReservationPageId = _configurationWrapper.GetConfigIntValue("RoomReservationPageId");

            var reservationDictionary = new Dictionary<string, object>();
            reservationDictionary.Add("Event_ID", roomReservation.EventId);
            reservationDictionary.Add("Room_ID", roomReservation.RoomId);

            if (roomReservation.RoomLayoutId != 0)
            {
                reservationDictionary.Add("Room_Layout_ID", roomReservation.RoomLayoutId);
            }

            reservationDictionary.Add("Notes", roomReservation.Notes);
            reservationDictionary.Add("Hidden", roomReservation.Hidden);
            reservationDictionary.Add("Cancelled", roomReservation.Cancelled);
            reservationDictionary.Add("Capacity", roomReservation.Capacity);
            reservationDictionary.Add("Label", roomReservation.Label);
            reservationDictionary.Add("Allow_Checkin", roomReservation.CheckinAllowed);

            try
            {
                return (_ministryPlatformService.CreateRecord(roomReservationPageId, reservationDictionary, token, true));
            }
            catch (Exception e)
            {
                var msg = string.Format("Error creating Room Reservation, roomReservation: {0}", roomReservation);
                _logger.Error(msg, e);
                throw (new ApplicationException(msg, e));
            }
        }

        public void UpdateRoomReservation(RoomReservationDto roomReservation, string token)
        {
            var roomReservationPageId = _configurationWrapper.GetConfigIntValue("RoomReservationPageId");
            var reservationDictionary = new Dictionary<string, object>
            {
                {"Event_ID", roomReservation.EventId},
                {"Event_Room_ID", roomReservation.EventRoomId},
                {"Room_ID", roomReservation.RoomId},
                {"Room_Layout_ID", roomReservation.RoomLayoutId},
                {"Notes", roomReservation.Notes},
                {"Hidden", roomReservation.Hidden},
                {"Cancelled", roomReservation.Cancelled},
                {"Capacity", roomReservation.Capacity},
                {"Label", roomReservation.Label},
                {"Allow_Checkin", roomReservation.CheckinAllowed}
            };

            try
            {
                _ministryPlatformService.UpdateRecord(roomReservationPageId, reservationDictionary, token);
            }
            catch (Exception e)
            {
                var msg = string.Format("Error updating Room Reservation, roomReservation: {0}", roomReservation);
                _logger.Error(msg, e);
                throw (new ApplicationException(msg, e));
            }
        }

        public void DeleteRoomReservation(RoomReservationDto roomReservation, string token)
        {
            // TODO: Move this to a classwide variable to support testing, dry it up, etc
            var roomReservationPageId = _configurationWrapper.GetConfigIntValue("RoomReservationPageId");
            _ministryPlatformService.DeleteRecord(roomReservationPageId, roomReservation.EventRoomId, null, token);
        }

        public List<Room> GetRoomsByLocationId(int locationId)
        {
            var t = ApiLogin();
            var search = string.Format(",,,,{0}", locationId);
            var records = _ministryPlatformService.GetPageViewRecords("RoomsByLocationId", t, search);

            return records.Select(record => new Room
            {
                BuildingId = record.ToInt("Building_ID"),
                LocationId = record.ToInt("Location_ID"),
                RoomId = record.ToInt("Room_ID"),
                RoomName = record.ToString("Room_Name"),
                RoomNumber = record.ToString("Room_Number"),
                BanquetCapacity = record.ToInt("Banquet_Capacity"),
                Description = record.ToString("Description"),
                TheaterCapacity = record.ToInt("Theater_Capacity")
            }).ToList();
        }

        public List<RoomLayout> GetRoomLayouts()
        {
            var t = ApiLogin();
            var records = _ministryPlatformService.GetPageViewRecords("RoomLayoutsById", t);

            return records.Select(record => new RoomLayout
            {
                LayoutId = record.ToInt("Room_Layout_ID"),
                LayoutName = record.ToString("Layout_Name")
            }).ToList();
        }

        public void DeleteEventRoomsForEvent(int eventId, string token)
        {
            // get event room ids
            var discardedEventRoomIds = GetRoomReservations(eventId).Select(r => r.EventRoomId).ToArray();

            // MP will throw an error if there are no elements to delete, so we need to exit the function before then
            if (discardedEventRoomIds.Length == 0)
            {
                return;
            }

            // create selection for event groups
            SelectionDescription eventRoomSelDesc = new SelectionDescription();
            eventRoomSelDesc.DisplayName = "DiscardedEventRooms " + DateTime.Now;
            eventRoomSelDesc.Kind = SelectionKind.Normal;
            eventRoomSelDesc.PageId = _configurationWrapper.GetConfigIntValue("RoomReservationPageId");
            var eventRoomSelId = _ministryPlatformService.CreateSelection(eventRoomSelDesc, token);


            // add events to selection
            _ministryPlatformService.AddToSelection(eventRoomSelId, discardedEventRoomIds, token);

            // delete the selection records
            _ministryPlatformService.DeleteSelectionRecords(eventRoomSelId, token);

            // delete the selection
            _ministryPlatformService.DeleteSelection(eventRoomSelId, token);
        }
    }
}
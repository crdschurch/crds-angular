using System;
using System.Collections.Generic;
using System.Linq;
using log4net;
using Crossroads.Web.Common.Configuration;
using Crossroads.Web.Common.MinistryPlatform;
using Crossroads.Web.Common.Security;
using MinistryPlatform.Translation.Extensions;
using MinistryPlatform.Translation.Models;
using MinistryPlatform.Translation.Models.EventReservations;
using MinistryPlatform.Translation.PlatformService;
using MinistryPlatform.Translation.Repositories.Interfaces;

namespace MinistryPlatform.Translation.Repositories
{
    public class RoomRepository : BaseRepository, IRoomRepository
    {
        private readonly string GetRoomsProcName = "api_crds_GetReservedAndAvailableRoomsByLocation";

        private readonly IMinistryPlatformService _ministryPlatformService;
        private readonly IMinistryPlatformRestRepository _ministryPlatformRestRepository;

        private readonly ILog _logger = LogManager.GetLogger(typeof(RoomRepository));

        public RoomRepository(IMinistryPlatformService ministryPlatformService,
                              IMinistryPlatformRestRepository ministryPlatformRestRepository,
                              IAuthenticationRepository authenticationService,
                              IConfigurationWrapper configuration,
                              IApiUserRepository apiUserRepository)
            : base(authenticationService, configuration, apiUserRepository)
        {
            _ministryPlatformService = ministryPlatformService;
            _ministryPlatformRestRepository = ministryPlatformRestRepository;
        }

        public List<MpRoomReservationDto> GetRoomReservations(int eventId)
        {
            var token = ApiLogin();
            var search = string.Format(",\"{0}\"", eventId);
            var records = _ministryPlatformService.GetPageViewRecords("GetRoomReservations", token, search);

            return records.Select(record => new MpRoomReservationDto
            {
                Cancelled = record.ToBool("Cancelled"),
                EventRoomId = record.ToInt("Event_Room_ID"),
                Hidden = record.ToBool("Hidden"),
                Notes = record.ToString("Notes"),
                RoomId = record.ToInt("Room_ID"),
                RoomLayoutId = record.ToNullableInt("Room_Layout_ID") ?? 0,
                Capacity = record.ToNullableInt("Capacity") ?? 0,
                Label = record.ToString("Label"),
                Name = record.ToString("Room_Name"),
                CheckinAllowed = record.ToNullableBool("Allow_Checkin") ?? false,
                Volunteers = record.ToInt("Volunteers"),
                Approved = record.ToNullableBool("_Approved") ?? false,
                Rejected = record.ToNullableBool("Rejected") ?? false
            }).ToList();
        }

        public int CreateRoomReservation(MpRoomReservationDto roomReservation)
        {
            string token = ApiLogin();
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
            reservationDictionary.Add("Volunteers", roomReservation.Volunteers);

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

        public void UpdateRoomReservation(MpRoomReservationDto roomReservation)
        {
            string token = ApiLogin();
            var roomReservationPageId = _configurationWrapper.GetConfigIntValue("RoomReservationPageId");
            var reservationDictionary = new Dictionary<string, object>
            {
                {"Event_ID", roomReservation.EventId},
                {"Event_Room_ID", roomReservation.EventRoomId},
                {"Room_ID", roomReservation.RoomId},
                { "Notes", roomReservation.Notes},
                {"Hidden", roomReservation.Hidden},
                {"Cancelled", roomReservation.Cancelled},
                {"Capacity", roomReservation.Capacity},
                {"Label", roomReservation.Label},
                {"Allow_Checkin", roomReservation.CheckinAllowed},
                {"Volunteers", roomReservation.Volunteers}
            };

            if (roomReservation.RoomLayoutId != 0)
            {
                reservationDictionary.Add("Room_Layout_ID", roomReservation.RoomLayoutId);
            }

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

        public void DeleteRoomReservation(MpRoomReservationDto roomReservation)
        {
            string token = ApiLogin();
            // TODO: Move this to a classwide variable to support testing, dry it up, etc
            var roomReservationPageId = _configurationWrapper.GetConfigIntValue("RoomReservationPageId");
            _ministryPlatformService.DeleteRecord(roomReservationPageId, roomReservation.EventRoomId, null, token);
        }

        public List<MpRoom> GetRoomsByLocationId(int locationId, DateTime startDate, DateTime endDate)
        {
            var token = ApiLogin();
            var parms = new Dictionary<string, object>
            {
                {"@StartDate", startDate},
                {"@EndDate", string.Join(",", endDate)},
                {"@LocationId", string.Join(",", locationId)}
            };

            var records = _ministryPlatformRestRepository.UsingAuthenticationToken(token).GetFromStoredProc<MpRoom>(GetRoomsProcName, parms);

            return records?.FirstOrDefault();
        }

        public List<RoomLayout> GetRoomLayouts()
        {
            var filter = "Room_ID IS NULL";
            var columns = "Room_Layout_ID,Layout_Name";
            var orderBy = "Room_Layout_ID";

            var token = ApiLogin();
            var records = _ministryPlatformRestRepository
                .UsingAuthenticationToken(token)
                .SearchTable<Dictionary<string, object>>("Room_Layouts", filter, columns, orderBy);

            return records.Select(record => new RoomLayout
            {
                LayoutId = record.ToInt("Room_Layout_ID"),
                LayoutName = record.ToString("Layout_Name")
            }).ToList();
        }

        public void DeleteEventRoomsForEvent(int eventId)
        {
            // get event room ids
            var discardedEventRoomIds = GetRoomReservations(eventId).Select(r => r.EventRoomId).ToArray();

            // MP will throw an error if there are no elements to delete, so we need to exit the function before then
            if (discardedEventRoomIds.Length == 0)
            {
                return;
            }

            _ministryPlatformRestRepository.UsingAuthenticationToken(ApiLogin())
                .Delete<MpEventRooms>(discardedEventRoomIds);
        }
    }
}
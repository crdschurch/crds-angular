using System;
using System.Collections.Generic;
using System.Linq;
using Crossroads.Utilities.Interfaces;
using log4net;
using Crossroads.Web.Common;
using Crossroads.Web.Common.Configuration;
using Crossroads.Web.Common.Security;
using MinistryPlatform.Translation.Extensions;
using MinistryPlatform.Translation.Models;
using MinistryPlatform.Translation.Models.EventReservations;
using MinistryPlatform.Translation.Repositories.Interfaces;
using Crossroads.Web.Common.MinistryPlatform;

namespace MinistryPlatform.Translation.Repositories
{
    public class EquipmentRepository : BaseRepository, IEquipmentRepository
    {
        private readonly IMinistryPlatformService _ministryPlatformService;
        private readonly ILog _logger = LogManager.GetLogger(typeof (RoomRepository));

        public EquipmentRepository(
            IMinistryPlatformService ministryPlatformService,
            IAuthenticationRepository authenticationService,
            IConfigurationWrapper configuration,
            IApiUserRepository apiUserRepository)
            : base(authenticationService, configuration, apiUserRepository)
        {
            _ministryPlatformService = ministryPlatformService;
        }

        public List<MpEquipmentReservationDto> GetEquipmentReservations(int eventId, int roomId)
        {
            var token = ApiLogin();
            var search = string.Format(",{0},{1}", eventId, roomId);
            var records = _ministryPlatformService.GetPageViewRecords("GetEquipmentReservations", token, search);

            return records.Select(record => new MpEquipmentReservationDto
            {
                Cancelled = record.ToBool("Cancelled"),
                EquipmentId = record.ToInt("Equipment_ID"),
                EventEquipmentId = record.ToInt("Event_Equipment_ID"),
                EventId = record.ToInt("Event_ID"),
                Notes = record.ToString("Notes"),
                QuantityRequested = record.ToInt("Quantity_Requested"),
                RoomId = record.ToInt("Room_ID")
            }).ToList();
        }

        public int CreateEquipmentReservation(MpEquipmentReservationDto equipmentReservation)
        {
            string token = ApiLogin();
            var equipmentReservationPageId = _configurationWrapper.GetConfigIntValue("EquipmentReservationPageId");
            var equipmentDictionary = new Dictionary<string, object>
            {
                {"Event_ID", equipmentReservation.EventId},
                {"Room_ID", equipmentReservation.RoomId},
                {"Equipment_ID", equipmentReservation.EquipmentId},
                {"Notes", equipmentReservation.Notes},
                {"Quantity_Requested", equipmentReservation.QuantityRequested},
                {"Cancelled", equipmentReservation.Cancelled}
            };

            try
            {
                return (_ministryPlatformService.CreateRecord(equipmentReservationPageId, equipmentDictionary, token, true));
            }
            catch (Exception e)
            {
                var msg = string.Format("Error creating Equipment Reservation, equipmentReservation: {0}", equipmentReservation);
                _logger.Error(msg, e);
                throw (new ApplicationException(msg, e));
            }
        }

        public void UpdateEquipmentReservation(MpEquipmentReservationDto equipmentReservation)
        {
            string token = ApiLogin();
            var equipmentReservationPageId = _configurationWrapper.GetConfigIntValue("EquipmentReservationPageId");
            var equipmentDictionary = new Dictionary<string, object>
            {
                {"Event_Equipment_ID", equipmentReservation.EventEquipmentId},
                {"Event_ID", equipmentReservation.EventId},
                {"Room_ID", equipmentReservation.RoomId},
                {"Equipment_ID", equipmentReservation.EquipmentId},
                {"Notes", equipmentReservation.Notes},
                {"Quantity_Requested", equipmentReservation.QuantityRequested},
                {"Approved", equipmentReservation.Approved},
                {"Cancelled", equipmentReservation.Cancelled}
            };

            try
            {
                _ministryPlatformService.UpdateRecord(equipmentReservationPageId, equipmentDictionary, token);
            }
            catch (Exception e)
            {
                var msg = string.Format("Error updating Equipment Reservation, equipmentReservation: {0}", equipmentReservation);
                _logger.Error(msg, e);
                throw (new ApplicationException(msg, e));
            }
        }

        public List<Equipment> GetEquipmentByLocationId(int locationId)
        {
            var t = ApiLogin();
            var search = string.Format(",,,,{0}", locationId);
            var records = _ministryPlatformService.GetPageViewRecords("EquipmentByLocationId", t, search);

            return records.Select(record => new Equipment
            {
                EquipmentId = record.ToInt("Equipment_ID"),
                EquipmentName = record.ToString("Equipment_Name"),
                QuantityOnHand = record.ToInt("Quantity_On_Hand")
            }).ToList();
        }
    }
}
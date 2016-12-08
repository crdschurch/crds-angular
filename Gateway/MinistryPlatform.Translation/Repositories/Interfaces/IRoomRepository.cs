using System;
using System.Collections.Generic;
using MinistryPlatform.Translation.Models;
using MinistryPlatform.Translation.Models.EventReservations;

namespace MinistryPlatform.Translation.Repositories.Interfaces
{
    public interface IRoomRepository
    {
        int CreateRoomReservation(MpRoomReservationDto roomReservation, string token);
        List<MpRoom> GetRoomsByLocationId(int locationId, DateTime startDate, DateTime endDate); List<RoomLayout> GetRoomLayouts();
        List<MpRoomReservationDto> GetRoomReservations(int eventId);
        void UpdateRoomReservation(MpRoomReservationDto roomReservation, string token);
        void DeleteRoomReservation(MpRoomReservationDto roomReservation, string token);
        void DeleteEventRoomsForEvent(int eventId, string token);
    }
}
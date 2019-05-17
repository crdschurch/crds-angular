using System;
using System.Collections.Generic;
using MinistryPlatform.Translation.Models;
using MinistryPlatform.Translation.Models.EventReservations;

namespace MinistryPlatform.Translation.Repositories.Interfaces
{
    public interface IRoomRepository
    {
        int CreateRoomReservation(MpRoomReservationDto roomReservation);
        List<MpRoom> GetRoomsByLocationId(int locationId, DateTime startDate, DateTime endDate);
        List<RoomLayout> GetRoomLayouts();
        List<MpRoomReservationDto> GetRoomReservations(int eventId);
        void UpdateRoomReservation(MpRoomReservationDto roomReservation);
        void DeleteRoomReservation(MpRoomReservationDto roomReservation);
        void DeleteEventRoomsForEvent(int eventId);
    }
}
using System;
using System.Collections.Generic;
using crds_angular.Models.Crossroads;
using crds_angular.Models.Crossroads.Events;

namespace crds_angular.Services.Interfaces
{
    public interface ICongregationService
    {
        Congregation GetCongregationById(int id);
        List<Room> GetRooms(int congregationId, DateTime startDate, DateTime endDate);
        List<RoomEquipment> GetEquipment(int congregationId);
        List<Congregation> GetAllCrossroadsCongregations();
    }
}
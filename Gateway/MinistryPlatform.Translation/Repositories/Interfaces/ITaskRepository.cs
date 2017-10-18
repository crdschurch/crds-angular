using System.Collections.Generic;
using MinistryPlatform.Translation.Models;
using MinistryPlatform.Translation.Models.EventReservations;


namespace MinistryPlatform.Translation.Repositories.Interfaces
{
    public interface ITaskRepository
    {
        List<MPTask> GetTasksToAutostart();

        void CompleteTask(string token, int taskId, bool rejected, string comments);
        void DeleteTasksForRoomReservations(List<int> roomReserverationIDs);
        List<MpRoomReservationRejectionDto> GetRejectedRoomReservations();
    }
}

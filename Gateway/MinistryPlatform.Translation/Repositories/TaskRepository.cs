using System;
using System.Collections.Generic;
using System.Linq;
using Crossroads.Utilities.Interfaces;
using Crossroads.Web.Common;
using Crossroads.Web.Common.Configuration;
using Crossroads.Web.Common.MinistryPlatform;
using Crossroads.Web.Common.Security;
using MinistryPlatform.Translation.Extensions;
using MinistryPlatform.Translation.Models;
using MinistryPlatform.Translation.Models.EventReservations;
using MinistryPlatform.Translation.Repositories.Interfaces;

namespace MinistryPlatform.Translation.Repositories
{
    public class TaskRepository : BaseRepository, ITaskRepository
    {
        private readonly IMinistryPlatformService _ministryPlatformService;
        private readonly IMinistryPlatformRestRepository _ministryPlatformRestRepository;
        private readonly int _autoStartedTaskPageViewId;
        private readonly int _roomReservationPageID;
        public const string GetRejectedRoomReservationsStoredProc = "cr_GetRejectedRoomReservations";


        public TaskRepository(IAuthenticationRepository authenticationService, 
                                IConfigurationWrapper configurationWrapper, 
                                IMinistryPlatformService ministryPlatformService, 
                                IMinistryPlatformRestRepository ministryPlatformRestRepository,
            IApiUserRepository apiUserRepository)
            : base(authenticationService, configurationWrapper, apiUserRepository)
        {
            _ministryPlatformService = ministryPlatformService;
            _ministryPlatformRestRepository = ministryPlatformRestRepository;
            _autoStartedTaskPageViewId = _configurationWrapper.GetConfigIntValue("TasksNeedingAutoStarted");
            _roomReservationPageID = _configurationWrapper.GetConfigIntValue("RoomReservationPageId");
        }

        public List<MPTask> GetTasksToAutostart()
        {
            var records = _ministryPlatformService.GetPageViewRecords(_autoStartedTaskPageViewId, ApiLogin());

            var tasks = records.Select(record => new MPTask
            {
                Task_ID = record.ToInt("Task ID"),
                Title = record.ToString("Title"),
                Author_User_ID = record.ToInt("Author User ID"),
                Assigned_User_ID = record.ToInt("Assigned User ID"),
                StartDate = record.ToNullableDate("Start Date"),
                EndDate = record.ToNullableDate("End Date"),
                Completed = record.ToBool("Completed"),
                Description = record.ToString("Description"),
                Record_ID = record.ToNullableInt("Record ID"),
                Page_ID = record.ToNullableInt("Page ID"),
                Process_Submission_ID = record.ToNullableInt("Process Submission ID"),
                Process_Step_ID = record.ToNullableInt("Process Step ID"),
                Rejected = record.ToBool("Rejected"),
                Escalated = record.ToBool("Escalated"),
                Record_Description = record.ToString("Record Description")
            }).ToList();

            return tasks;
        }

        public void CompleteTask(string token, int taskId, bool rejected, string comments)
        {
            // note that this call needs to user impersonation
            _ministryPlatformService.CompleteTask(token, taskId, rejected, comments);
        }

        //This is not covered in a unit test, and it has not been tested as we are not currently using it
        //but we will pick it back up next sprint
        public void DeleteTasksForRoomReservations(List<int> roomReserverationIDs)
        {
            var apiToken = ApiLogin();
            foreach (int roomReserverationID in roomReserverationIDs)
            {
                var task = _ministryPlatformRestRepository.UsingAuthenticationToken(apiToken).FindTask(_roomReservationPageID, roomReserverationID);
                if (task != null)
                    _ministryPlatformRestRepository.UsingAuthenticationToken(apiToken).DeleteTask(task.Task_ID, true, "Room Edited, Cancelled By User");
            }
        }

        public List<MpRoomReservationRejectionDto> GetRejectedRoomReservations()
        {
            var apiToken = ApiLogin();
            var results = _ministryPlatformRestRepository.UsingAuthenticationToken(apiToken).GetFromStoredProc<MpRoomReservationRejectionDto>(GetRejectedRoomReservationsStoredProc);
            return results?.FirstOrDefault();
        }

    }
}

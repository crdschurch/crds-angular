using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using crds_angular.Models.Crossroads;
using crds_angular.Services.Interfaces;
using Crossroads.Utilities.Interfaces;
using log4net;
using Crossroads.Web.Common;
using Crossroads.Web.Common.Configuration;
using Crossroads.Web.Common.MinistryPlatform;
using MPInterfaces = MinistryPlatform.Translation.Repositories.Interfaces;

namespace crds_angular.Services
{
    public class TaskService : ITaskService
    {
        private readonly ILog _logger = LogManager.GetLogger(typeof (TaskService));

        private readonly MPInterfaces.ITaskRepository _taskRepository;
        private readonly IApiUserRepository _apiUserService;
        private readonly IConfigurationWrapper _configWrapper;
        private readonly IUserImpersonationService _impersonationService;
        private readonly MPInterfaces.IUserRepository _userService;
        private readonly int _roomReservationRejectedEmailTemplateId;
        private readonly IEmailCommunication _emailCommunicationService;


        public TaskService(MPInterfaces.ITaskRepository taskRepository,
                           IApiUserRepository apiUserService,
                           IConfigurationWrapper configWrapper,
                           IUserImpersonationService impersonationService,
                           MPInterfaces.IUserRepository userService,
                           IEmailCommunication emailCommunicationService
                           )
        {
            _taskRepository = taskRepository;
            _apiUserService = apiUserService;
            _configWrapper = configWrapper;
            _impersonationService = impersonationService;
            _userService = userService;
            _emailCommunicationService = emailCommunicationService;
            _roomReservationRejectedEmailTemplateId = _configWrapper.GetConfigIntValue("RoomReservationRejectedEmailTemplateId");
        }

        public void AutoCompleteTasks()
        {
            try
            {
                var apiUserToken = _apiUserService.GetDefaultApiUserToken();
                var tasksToComplete = _taskRepository.GetTasksToAutostart();

                _logger.InfoFormat("Number of tasks to autocomplete: {0} ", tasksToComplete.Count);

                foreach (var task in tasksToComplete)
                {
                    _logger.InfoFormat("Inside of tasks to complete Loop");

                    var user = _userService.GetUserByRecordId(task.Assigned_User_ID);

                    _logger.InfoFormat("User Record ID for task to complete: {0}", user.UserRecordId);
                    _logger.InfoFormat("Task ID for task to complete: {0}", task.Task_ID);

                    try
                    {
                        _impersonationService.WithImpersonation(apiUserToken,
                                                                user.UserEmail,
                                                                () =>
                                                                {
                                                                    _taskRepository.CompleteTask(apiUserToken, task.Task_ID, task.Rejected, "Auto Completed");
                                                                    return true;
                                                                });

                    }
                    catch (Exception ex)
                    {
                        _logger.ErrorFormat("Auto complete task failed for Task {0} Detail: {1}", task.Task_ID, ex);
                    }
                }
            }
            catch (Exception outerException)
            {
                _logger.ErrorFormat("Could not process tasks for autocomplete, Detail: {0}", outerException);
            }
        }

        public void RoomReservationRejectionNotification()
        {
            try
            {
                var apiUserToken = _apiUserService.GetDefaultApiUserToken();
                var rejectedReservations = _taskRepository.GetRejectedRoomReservations();

                _logger.InfoFormat("Number of rejected room reservation notifications to send: {0} ", rejectedReservations.Count);

                foreach (var rejection in rejectedReservations)
                {
                    var mergeData = new Dictionary<string, object>
                    {
                        {"Room_Name", rejection.Room_Name},
                        {"Event_Start_Date", rejection.Event_Start_Date},
                        {"Event_Title", rejection.Event_Title},
                        {"Task_Rejection_Reason", rejection.Task_Rejection_Reason},
                    };

                    var email = new EmailCommunicationDTO
                    {
                        TemplateId = _roomReservationRejectedEmailTemplateId,
                        ToContactId = rejection.Requestor_Contact_ID,
                        MergeData = mergeData
                    };
                    _emailCommunicationService.SendEmail(email, apiUserToken);
                }

            }
            catch (Exception e)
            {
                _logger.Error("Exception retrieving pending inquiries", e);
                throw;
            }
        }
    }
}
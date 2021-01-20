using System;
using System.Linq;
using log4net;
using Crossroads.Web.Common.Configuration;
using Crossroads.Web.Common.MinistryPlatform;
using MinistryPlatform.Translation.Models;
using MinistryPlatform.Translation.Repositories.Interfaces;

namespace MinistryPlatform.Translation.Repositories
{
    public class CampRepository : ICampRepository
    {             
        private readonly IConfigurationWrapper _configurationWrapper;
        private readonly IMinistryPlatformRestRepository _ministryPlatformRest;
        private readonly IApiUserRepository _apiUserRepository;
        private readonly ILog _logger = LogManager.GetLogger(typeof(CampRepository));

        public CampRepository(IConfigurationWrapper configurationWrapper,
                              IMinistryPlatformRestRepository ministryPlatformRest,
                              IApiUserRepository apiUserRepository)
                              
        {            
            _configurationWrapper = configurationWrapper;
            _ministryPlatformRest = ministryPlatformRest;
            _apiUserRepository = apiUserRepository;
        }

        public MpCamp GetCampEventDetails(int eventId)
        {
            var apiToken = _apiUserRepository.GetDefaultApiClientToken();
            var campType = _configurationWrapper.GetConfigIntValue("CampEventType");
            var gradeGroupId = _configurationWrapper.GetConfigIntValue("AgeorGradeGroupType");
            var campGrades = _ministryPlatformRest.UsingAuthenticationToken(apiToken).Search<MpEventGroup>($"Event_ID_Table.[Event_ID] = {eventId} AND Group_ID_Table_Group_Type_ID_Table.[Group_Type_ID] = {gradeGroupId}", "Group_ID_Table.Group_ID, Group_ID_Table.Group_Name").ToList();
            const string columnList = "Events.[Event_ID],Events.[Event_Title],Events.[Event_Start_Date],Events.[Event_End_Date],Event_Type_ID_Table.[Event_Type_ID], Program_ID_Table.[Program_ID],Online_Registration_Product_Table.[Product_ID],Events.[Registration_Start],Events.[Registration_End],Primary_Contact_Table.[Email_Address]";
            var campData = _ministryPlatformRest.UsingAuthenticationToken(apiToken).Search<MpCamp>($"Event_ID = {eventId}", columnList).ToList();
            campData = campData.Where((camp) => camp.EventType == campType).ToList();
            var campEvent = campData.FirstOrDefault();
            if (campEvent == null) throw new Exception("No Camp found");
            campEvent.CampGradesList = campGrades;
            return campEvent;
        }
    }
}

using System.Collections.Generic;
using System.Linq;
using Crossroads.Web.Common.Configuration;
using Crossroads.Web.Common.MinistryPlatform;
using MinistryPlatform.Translation.Models;
using MinistryPlatform.Translation.Models.Childcare;
using MinistryPlatform.Translation.Repositories.Interfaces;

namespace MinistryPlatform.Translation.Repositories
{
    public class ChildcareRepository : IChildcareRepository
    {
        private readonly IMinistryPlatformRestRepository _ministryPlatformRest;
        private readonly IConfigurationWrapper _configurationWrapper;
        private readonly IApiUserRepository _apiUserRepository;

        public ChildcareRepository(IConfigurationWrapper configurationWrapper, IMinistryPlatformRestRepository ministryPlatformRest, IApiUserRepository apiUserRepository)
        {
            _configurationWrapper = configurationWrapper;
            _ministryPlatformRest = ministryPlatformRest;
            _apiUserRepository = apiUserRepository;
        }

        public List<MpChildcareDashboard> GetChildcareDashboard(int contactId)
        {
            var apiToken = _apiUserRepository.GetDefaultApiClientToken();
            var parms = new Dictionary<string, object> {{"Contact_ID", contactId}, {"Domain_ID", 1} };
            var dashboardData = _ministryPlatformRest.UsingAuthenticationToken(apiToken).GetFromStoredProc<MpChildcareDashboard>(_configurationWrapper.GetConfigValue("ChildcareDashboardStoredProc"), parms);
            var childcareDashboard = dashboardData.FirstOrDefault() ?? new List<MpChildcareDashboard>();
            return childcareDashboard;
        }

        public bool IsChildRsvpd(int contactId, int groupId, string token)
        {
            var parms = new Dictionary<string,object>()
            {
                {"@ContactId", contactId },
                {"@EventGroupID", groupId }
            };
            var storedProcReturn = _ministryPlatformRest.UsingAuthenticationToken(token).GetFromStoredProc<MPRspvd>("api_crds_childrsvpd", parms);
            var rsvpList = storedProcReturn.FirstOrDefault();
            if (rsvpList.Count > 0)
            {
                return rsvpList.FirstOrDefault().Rsvpd;
            }
            return false;            
        }

        public List<MpContact> GetChildcareReminderEmails(string token)
        {
            var storedProcReturn = _ministryPlatformRest.UsingAuthenticationToken(token).GetFromStoredProc<MpContact>("api_crds_ChildcareReminderEmails");
            var emailList = storedProcReturn.FirstOrDefault();
            return emailList;
        }

        public List<MpChildcareCancelledNotification> GetChildcareCancellations()
        {
            var apiToken = _apiUserRepository.GetDefaultApiClientToken();
            var parms = new Dictionary<string, object>
            {
                {"@ChildcareGroupType", _configurationWrapper.GetConfigIntValue("ChildcareGroupType") }
            };
            var notificationData = _ministryPlatformRest.UsingAuthenticationToken(apiToken).GetFromStoredProc<MpChildcareCancelledNotification>("api_crds_CancelledChildcareNotification", parms);

            return notificationData.FirstOrDefault() ?? new List<MpChildcareCancelledNotification>();
        }
    }
}

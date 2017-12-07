using System;
using System.Collections.Generic;
using System.Linq;
using Crossroads.Utilities.Interfaces;
using Crossroads.Web.Common;
using Crossroads.Web.Common.Configuration;
using Crossroads.Web.Common.MinistryPlatform;
using Crossroads.Web.Common.Security;
using MinistryPlatform.Translation.Extensions;
using MinistryPlatform.Translation.Models.Lookups;
using MinistryPlatform.Translation.Repositories.Interfaces;

namespace MinistryPlatform.Translation.Repositories
{
    public class LookupRepository : BaseRepository, ILookupRepository
    {
        private readonly IMinistryPlatformService _ministryPlatformServiceImpl;
        private readonly IMinistryPlatformRestRepository _ministryPlatformRest;

        public LookupRepository(IAuthenticationRepository authenticationService,
                                IConfigurationWrapper configurationWrapper,
                                IMinistryPlatformService ministryPlatformServiceImpl,
                                IMinistryPlatformRestRepository ministryPlatformRest)
            : base(authenticationService, configurationWrapper)
        {
            _ministryPlatformServiceImpl = ministryPlatformServiceImpl;
            _ministryPlatformRest = ministryPlatformRest;
        }

        public Dictionary<string, object> EmailSearch(string email, string token)
        {
            return _ministryPlatformServiceImpl.GetLookupRecord(AppSettings("Emails"), email, token);
        }

        public List<Dictionary<string, object>> EventTypes(string token)
        {
            token = ApiLogonIfNotAuthenticated(token);
            return Enumerable.OrderBy(_ministryPlatformServiceImpl.GetRecordsDict(AppSettings("EventTypesLookup"), token), x => x["dp_RecordName"].ToString()).ToList();
        }

        public List<Dictionary<string, object>> EventTypesForEventTool(string token)
        {
            const string columns = "Event_Type_ID AS dp_RecordID,Event_Type AS dp_RecordName,Allow_Multiday_Event";
            const string filter = "Show_On_Event_Tool=1";
            const string orderBy = "Event_Type";

            token = ApiLogonIfNotAuthenticated(token);
            var records = _ministryPlatformRest.UsingAuthenticationToken(token)
                .SearchTable<Dictionary<string, object>>("Event_Types", filter, columns, orderBy);

            return records;
        }

        public List<Dictionary<string, object>> Genders(string token = "")
        {
            token = ApiLogonIfNotAuthenticated(token);
            return _ministryPlatformServiceImpl.GetLookupRecords(AppSettings("Genders"), token);
        }

        public List<Dictionary<string, object>> MaritalStatus(string token)
        {
            token = ApiLogonIfNotAuthenticated(token);
            return _ministryPlatformServiceImpl.GetLookupRecords(AppSettings("MaritalStatus"), token);
        }

        public List<Dictionary<string, object>> ServiceProviders(string token)
        {
            token = ApiLogonIfNotAuthenticated(token);
            return _ministryPlatformServiceImpl.GetLookupRecords(AppSettings("ServiceProviders"), token);
        }

        public List<Dictionary<string, object>> States(string token)
        {
            token = ApiLogonIfNotAuthenticated(token);
            return _ministryPlatformServiceImpl.GetLookupRecords(AppSettings("States"), token);
        }

        public List<Dictionary<string, object>> Countries(string token)
        {
            token = ApiLogonIfNotAuthenticated(token);
            return _ministryPlatformServiceImpl.GetLookupRecords(AppSettings("Countries"), token);
        }

        public List<Dictionary<string, object>> CrossroadsLocations(string token = "")
        {
            token = ApiLogonIfNotAuthenticated(token);
            return _ministryPlatformServiceImpl.GetLookupRecords(AppSettings("CrossroadsLocations"), token);
        }

        public List<Dictionary<string, object>> ReminderDays(string token)
        {
            token = ApiLogonIfNotAuthenticated(token);
            return _ministryPlatformServiceImpl.GetLookupRecords(AppSettings("ReminderDaysLookup"), token);
        }

        public List<Dictionary<string, object>> WorkTeams(string token)
        {
            token = ApiLogonIfNotAuthenticated(token);
            return _ministryPlatformServiceImpl.GetLookupRecords(_configurationWrapper.GetConfigIntValue("WorkTeams"), token);
        }

        public List<Dictionary<string, object>> GroupReasonEnded(string token)
        {
            token = ApiLogonIfNotAuthenticated(token);
            return _ministryPlatformServiceImpl.GetRecordsDict(AppSettings("GroupEndedLookup"), token);
        }

        public IEnumerable<T> GetList<T>(string token)
        {
            if (typeof (T) == typeof (MpWorkTeams))
            {
                return (IEnumerable<T>) 
                    WorkTeams(token).Select(wt => new MpWorkTeams(wt.ToInt("dp_RecordID"), wt.ToString("dp_RecordName")));
            }
            if (typeof (T) == typeof (MpOtherOrganization))
            {                
                return (IEnumerable<T>)
                    _ministryPlatformServiceImpl.GetLookupRecords(_configurationWrapper.GetConfigIntValue("OtherOrgs"), token)
                    .Select(other => new MpOtherOrganization(other.ToInt("dp_RecordID"), other.ToString("dp_RecordName")));
            }

            return null;
        }

        public T GetObject<T>(string token)
        {
            throw new NotImplementedException();
        }

        public List<Dictionary<string, object>> MeetingDays(string token)
        {
            token = ApiLogonIfNotAuthenticated(token);
            return _ministryPlatformServiceImpl.GetLookupRecords(AppSettings("MeetingDay"), token);
        }

        public List<Dictionary<string, object>> Ministries(string token)
        {
            token = ApiLogonIfNotAuthenticated(token);
            return _ministryPlatformServiceImpl.GetLookupRecords(AppSettings("Ministries"), token);
        }

        public List<Dictionary<string, object>> ChildcareLocations(string token)
        {
            token = ApiLogonIfNotAuthenticated(token);
            return _ministryPlatformServiceImpl.GetPageViewRecords(AppSettings("CongregationsWithChildcarePageView"), token, "", "");
        }

        public List<Dictionary<string, object>> GroupsByCongregationAndMinistry(string token, string congregationid, string ministryid)
        {
            var searchString = string.Format("\"{0}\",\"{1}\",", congregationid, ministryid);

            var groups =  _ministryPlatformServiceImpl.GetPageViewRecords(AppSettings("GroupsByCongregationAndMinistry"), token, searchString);
            return groups;
        }
        public List<Dictionary<string, object>> ChildcareTimesByCongregation(string token, string congregationid)
        {
            var searchString = string.Format("\"{0}\",", congregationid);

            var times = _ministryPlatformServiceImpl.GetPageViewRecords(AppSettings("ChildcareTimesByCongregation"), token, searchString);
            return times;
        }

        private string ApiLogonIfNotAuthenticated(string token)
        {
            if (string.IsNullOrEmpty(token))
            {
                token = base.ApiLogin();
            }
            return token;
        }
    }
}
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
                                IMinistryPlatformRestRepository ministryPlatformRest,
                                IApiUserRepository apiUserRepository)
            : base(authenticationService, configurationWrapper, apiUserRepository)
        {
            _ministryPlatformServiceImpl = ministryPlatformServiceImpl;
            _ministryPlatformRest = ministryPlatformRest;
        }

        public Dictionary<string, object> EmailSearch(string email)
        {
            return _ministryPlatformServiceImpl.GetLookupRecord(AppSettings("Emails"), email, base.ApiLogin());
        }

        public List<Dictionary<string, object>> EventTypes()
        {
            return Enumerable.OrderBy(_ministryPlatformServiceImpl.GetRecordsDict(AppSettings("EventTypesLookup"), base.ApiLogin()), x => x["dp_RecordName"].ToString()).ToList();
        }

        public List<Dictionary<string, object>> EventTypesForEventTool()
        {
            const string columns = "Event_Type_ID AS dp_RecordID,Event_Type AS dp_RecordName,Allow_Multiday_Event";
            const string filter = "Show_On_Event_Tool=1";
            const string orderBy = "Event_Type";
            
            var records = _ministryPlatformRest.UsingAuthenticationToken(base.ApiLogin())
                .SearchTable<Dictionary<string, object>>("Event_Types", filter, columns, orderBy);

            return records;
        }

        public List<Dictionary<string, object>> Genders()
        {
            return _ministryPlatformServiceImpl.GetLookupRecords(AppSettings("Genders"), base.ApiLogin());
        }

        public List<Dictionary<string, object>> MaritalStatus()
        {
            return _ministryPlatformServiceImpl.GetLookupRecords(AppSettings("MaritalStatus"), base.ApiLogin());
        }

        public List<Dictionary<string, object>> ServiceProviders()
        {
            return _ministryPlatformServiceImpl.GetLookupRecords(AppSettings("ServiceProviders"), base.ApiLogin());
        }

        public List<Dictionary<string, object>> States()
        {
            return _ministryPlatformServiceImpl.GetLookupRecords(AppSettings("States"), base.ApiLogin());
        }

        public List<Dictionary<string, object>> Countries()
        {
            return _ministryPlatformServiceImpl.GetLookupRecords(AppSettings("Countries"), base.ApiLogin());
        }

        public List<Dictionary<string, object>> CrossroadsLocations()
        {
            return _ministryPlatformServiceImpl.GetLookupRecords(AppSettings("CrossroadsLocations"), base.ApiLogin());
        }

        public List<Dictionary<string, object>> ReminderDays()
        {
            return _ministryPlatformServiceImpl.GetLookupRecords(AppSettings("ReminderDaysLookup"), base.ApiLogin());
        }

        public List<Dictionary<string, object>> WorkTeams()
        {
            return _ministryPlatformServiceImpl.GetLookupRecords(_configurationWrapper.GetConfigIntValue("WorkTeams"), base.ApiLogin());
        }

        public List<Dictionary<string, object>> GroupReasonEnded()
        {
            return _ministryPlatformServiceImpl.GetRecordsDict(AppSettings("GroupEndedLookup"), base.ApiLogin());
        }

        public IEnumerable<T> GetList<T>()
        {
            if (typeof (T) == typeof (MpWorkTeams))
            {
                return (IEnumerable<T>) 
                    WorkTeams().Select(wt => new MpWorkTeams(wt.ToInt("dp_RecordID"), wt.ToString("dp_RecordName")));
            }
            if (typeof (T) == typeof (MpOtherOrganization))
            {                
                return (IEnumerable<T>)
                    _ministryPlatformServiceImpl.GetLookupRecords(_configurationWrapper.GetConfigIntValue("OtherOrgs"), base.ApiLogin())
                    .Select(other => new MpOtherOrganization(other.ToInt("dp_RecordID"), other.ToString("dp_RecordName")));
            }

            return null;
        }

        public List<Dictionary<string, object>> MeetingDays()
        {
            return _ministryPlatformServiceImpl.GetLookupRecords(AppSettings("MeetingDay"), base.ApiLogin());
        }

        public List<Dictionary<string, object>> MeetingFrequencies()
        {
            return _ministryPlatformServiceImpl.GetLookupRecords(AppSettings("MeetingFrequency"), base.ApiLogin());
        }

        public List<Dictionary<string, object>> Ministries()
        {
            return _ministryPlatformServiceImpl.GetLookupRecords(AppSettings("Ministries"), base.ApiLogin());
        }

        public List<Dictionary<string, object>> ChildcareLocations()
        {
            return _ministryPlatformServiceImpl.GetPageViewRecords(AppSettings("CongregationsWithChildcarePageView"), base.ApiLogin(), "", "");
        }

        public List<Dictionary<string, object>> GroupsByCongregationAndMinistry(string congregationid, string ministryid)
        {
            var searchString = string.Format("\"{0}\",\"{1}\",", congregationid, ministryid);

            var groups =  _ministryPlatformServiceImpl.GetPageViewRecords(AppSettings("GroupsByCongregationAndMinistry"), base.ApiLogin(), searchString);
            return groups;
        }
        public List<Dictionary<string, object>> ChildcareTimesByCongregation(string congregationid)
        {
            var searchString = string.Format("\"{0}\",", congregationid);

            var times = _ministryPlatformServiceImpl.GetPageViewRecords(AppSettings("ChildcareTimesByCongregation"), base.ApiLogin(), searchString);
            return times;
        }

    }
}
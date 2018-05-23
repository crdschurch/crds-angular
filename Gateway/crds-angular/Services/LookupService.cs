using System;
using crds_angular.Services.Interfaces;
using Crossroads.Web.Common.MinistryPlatform;
using MinistryPlatform.Translation.Repositories.Interfaces;

namespace crds_angular.Services {
    public class LookupService : ILookupService {

        private readonly ILookupRepository _lookupRepository;
        private readonly IApiUserRepository _apiUserRepository;
        
        public LookupService(ILookupRepository lookupRepository, IApiUserRepository apiUserRepository)
        {
            _lookupRepository = lookupRepository;
            _apiUserRepository = apiUserRepository;
        }

        public string GetMeetingDayFromId(int? meetingDayId)
        {
            if (meetingDayId == null)
            {
                return null;
            }

            string dayString = null;
            var days = _lookupRepository.MeetingDays(_apiUserRepository.GetDefaultApiClientToken());

            foreach (var day in days)
            {
                var dayid = Convert.ToInt32(day["dp_RecordID"]);
                if (dayid == meetingDayId)
                {
                    dayString = day["dp_RecordName"].ToString();
                }
            }
            return dayString;
        }
    }
}
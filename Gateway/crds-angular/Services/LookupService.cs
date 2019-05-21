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

        public string GetMeetingFrequencyFromId(int? meetingFrequencyId)
        {
            if (meetingFrequencyId == null)
            {
                return null;
            }

            string freqString = null;
            var freqs = _lookupRepository.MeetingFrequencies(_apiUserRepository.GetDefaultApiClientToken());

            foreach (var freq in freqs)
            {
                var freqid = Convert.ToInt32(freq["dp_RecordID"]);
                if (freqid == meetingFrequencyId)
                {
                    freqString = freq["dp_RecordName"].ToString();
                }
            }
            return freqString;
        }
    }
}
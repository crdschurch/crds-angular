using System;
using crds_angular.Models.Crossroads.Events;
using crds_angular.Services.Interfaces;
using Crossroads.Web.Common.Configuration;
using Crossroads.Web.Common.MinistryPlatform;
using MinistryPlatform.Translation.Repositories.Interfaces;

namespace crds_angular.Services
{
    public class EventParticipantService : IEventParticipantService
    {

        private readonly IEventParticipantRepository _eventParticipantRepository;
        private readonly IConfigurationWrapper _configurationWrapper;
        private readonly IApiUserRepository _apiUserRepository;

        public EventParticipantService(IEventParticipantRepository eventParticipantRepository, IConfigurationWrapper configurationWrapper, IApiUserRepository apiUserRepository)
        {
            _eventParticipantRepository = eventParticipantRepository;
            _configurationWrapper = configurationWrapper;
            _apiUserRepository = apiUserRepository;
        }

        public EventParticipantDTO GetEventParticipantByContactAndEvent(int contactId, int eventId)
        {
            var token = _apiUserRepository.GetDefaultApiClientToken();
            var res = _eventParticipantRepository.GetEventParticipantByContactAndEvent(contactId, eventId, token);
            if (res.Status)
            {
                return new EventParticipantDTO
                {
                    EventParticipantId = res.Value.EventParticipantId,
                    EventId = res.Value.EventId,
                    ParticipantId = res.Value.ParticipantId,
                    ParticipationStatus = res.Value.ParticipantStatus,
                    EndDate = res.Value.EndDate
                };
            }
            throw new ApplicationException(res.ErrorMessage);            
        }

        public bool IsParticipantInvalid(EventParticipantDTO participant)
        {
            var interestedStatus = _configurationWrapper.GetConfigIntValue("Participant_Status_Interested");
            var cancelledStatus = _configurationWrapper.GetConfigIntValue("Participant_Status_Cancelled");
            var registeredStatus = _configurationWrapper.GetConfigIntValue("Participant_Status_Registered");
            var confirmedStatus = _configurationWrapper.GetConfigIntValue("Participant_Status_Confirmed");
            if (participant.ParticipationStatus == cancelledStatus)
            {
                return true;
            }
            if (participant.ParticipationStatus == registeredStatus || participant.ParticipationStatus == confirmedStatus)
            {
                return false;
            }

            if (participant.ParticipationStatus == interestedStatus && participant.EndDate > DateTime.Now)
            {
                return false;
            };
            return true;
        }
    }
}
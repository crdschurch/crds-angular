using System;
using System.Collections.Generic;
using System.Linq;
using Crossroads.Utilities.FunctionalHelpers;
using log4net;
using Crossroads.Web.Common.Configuration;
using Crossroads.Web.Common.MinistryPlatform;
using MinistryPlatform.Translation.Models;
using MinistryPlatform.Translation.Repositories.Interfaces;

namespace MinistryPlatform.Translation.Repositories
{
    public class TripRepository : ITripRepository
    {
        private readonly ILog _logger = LogManager.GetLogger(typeof(TripRepository));

        private readonly IMinistryPlatformRestRepository _ministryPlatformRestRepository;
        private readonly IConfigurationWrapper _configurationWrapper;

        public TripRepository(IMinistryPlatformRestRepository ministryPlatformRestRepository, IConfigurationWrapper configurationWrapper)
        {
            _ministryPlatformRestRepository = ministryPlatformRestRepository;
            _configurationWrapper = configurationWrapper;
        }

        public Result<MpPledge> AddAsTripParticipant(int ContactId, int PledgeCampaignID, string token)
        {
            var storedProc = _configurationWrapper.GetConfigValue("TripParticipantStoredProc");
            try
            {
                var fields = new Dictionary<string, object>
                {
                    {"@PledgeCampaignID", PledgeCampaignID},
                    {"@ContactId", ContactId}
                };
                var result = _ministryPlatformRestRepository.UsingAuthenticationToken(token).GetFromStoredProc<MpPledge>(storedProc, fields);
                if (result.Count > 0 && result[0].Count > 0)
                {
                    return new Result<MpPledge>(true, result[0].FirstOrDefault());
                }
                _logger.Debug($"Adding a trip particpant returned no results. The trip is already full.");
                return new Result<MpPledge>(false, "Trip is already full");
            }
            catch (Exception e)
            {
                _logger.Error($"Failed to call stored procedure #{storedProc}");
                _logger.Error(e.Message);
                throw;
            }            
        }

        public List<MpEventParticipantDocument> GetTripDocuments(int eventParticipantId, string token)
        {
            try
            {
                var searchString = $"cr_EventParticipant_Documents.Event_Participant_ID = {eventParticipantId}";
                var columns = "EventParticipant_Document_ID, cr_EventParticipant_Documents.Event_Participant_ID, Document_ID, Received, cr_EventParticipant_Documents.Notes, Event_Participant_ID_Table_Event_ID_Table.Event_Title";
                return _ministryPlatformRestRepository.UsingAuthenticationToken(token).Search<MpEventParticipantDocument>(searchString, columns);
            }
            catch (Exception e)
            {
                _logger.Error($"Failed to get documents for Event Participant #{eventParticipantId}");
                _logger.Error(e.Message);
                throw;
            }
        }

        public bool ReceiveTripDocument(MpEventParticipantDocument tripDoc, string token)
        {
            try
            {
                _ministryPlatformRestRepository.UsingAuthenticationToken(token).Update(tripDoc);
                return true;
            }
            catch (Exception e)
            {
                _logger.Error($"Failed to save trip document for {tripDoc.EventParticipantId}");
                _logger.Error(e.Message);
                throw;
            }
        }
    }
}

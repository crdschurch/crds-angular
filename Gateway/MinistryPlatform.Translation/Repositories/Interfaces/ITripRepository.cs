using System.Collections.Generic;
using Crossroads.Utilities.FunctionalHelpers;
using MinistryPlatform.Translation.Models;

namespace MinistryPlatform.Translation.Repositories.Interfaces
{
    public interface ITripRepository
    {
        Result<MpPledge> AddAsTripParticipant(int ContactId, int PledgeCampaignID, string token);
        List<MpEventParticipantDocument> GetTripDocuments(int eventParticipantId, string token);
        bool ReceiveTripDocument(MpEventParticipantDocument tripDoc, string token);
    }
}
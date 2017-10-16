using System.Collections.Generic;
using crds_angular.Models.Crossroads.Trip;

namespace crds_angular.Services.Interfaces
{
    public interface ITripService
    {
        TripFormResponseDto GetFormResponses(int selectionId, int selectionCount, int recordId);
        List<TripGroupDto> GetGroupsByEventId(int eventId);
        MyTripsDto GetMyTrips(string token);
        List<TripParticipantDto> Search(string search);
        TripCampaignDto GetTripCampaign(int pledgeCampaignId);
        List<FamilyMemberTripDto> GetFamilyMembers(int pledgeId, string token);       
        int GeneratePrivateInvite(PrivateInviteDto dto, string token);
        bool ValidatePrivateInvite(int pledgeCampaignId, string guid, string token);
        int SaveApplication(TripApplicationDto dto);
        TripParticipantPledgeDto CreateTripParticipant(int contactId, int pledgeCampaignId);
        TripParticipantPledgeDto GetCampaignPledgeInfo(int contactId, int pledgeCampaignId);
        bool HasScholarship(int contactId, int campaignId);
        void SendTripIsFullMessage(int campaignId);
        bool GetIPromise(int eventParticipantId);
        TripDocument GetIPromiseDocument(int tripEventId);
        void ReceiveIPromiseDocument(TripDocument iPromiseDoc);
    }
}
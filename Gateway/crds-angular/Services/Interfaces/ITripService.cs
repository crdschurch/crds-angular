using System.Collections.Generic;
using crds_angular.Models.Crossroads.Trip;
using Crossroads.Web.Auth.Models;

namespace crds_angular.Services.Interfaces
{
    public interface ITripService
    {
        TripFormResponseDto GetFormResponses(int selectionId, int selectionCount, int recordId);
        List<TripGroupDto> GetGroupsByEventId(int eventId);
        MyTripsDto GetMyTrips(AuthDTO token);
        List<TripParticipantDto> Search(string search);
        TripCampaignDto GetTripCampaign(int pledgeCampaignId);
        List<FamilyMemberTripDto> GetFamilyMembers(int pledgeId, AuthDTO token);       
        int GeneratePrivateInvite(PrivateInviteDto dto);
        bool ValidatePrivateInvite(int pledgeCampaignId, string guid, AuthDTO token);
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
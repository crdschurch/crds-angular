using System;
using System.Collections.Generic;
using MinistryPlatform.Translation.Models;
using MinistryPlatform.Translation.Models.DTO;
using MpResponse = MinistryPlatform.Translation.Models.MpResponse;

namespace MinistryPlatform.Translation.Repositories.Interfaces
{
    public interface IOpportunityRepository
    {

        MpOpportunity GetOpportunityById(int opportunityId, string token);
        int GetOpportunitySignupCount(int opportunityId, int eventId, string token);
        List<DateTime> GetAllOpportunityDates(int id);
        MpGroup GetGroupParticipantsForOpportunity(int id);
        DateTime GetLastOpportunityDate(int opportunityId);
        int DeleteResponseToOpportunities(int participantId, int opportunityId, int eventId);
        int RespondToOpportunity( int opportunityId, string comments);
        MpResponse GetMyOpportunityResponses(int contactId, int opportunityId);
        MpResponse GetOpportunityResponse(int contactId, int opportunityId);
        MpResponse GetOpportunityResponse(int opportunityId, int eventId, MpParticipant participant);
        List<Models.Opportunities.MpResponse> SearchResponseByGroupAndEvent(String searchString);
        List<Models.Opportunities.MpResponse> GetContactsOpportunityResponseByGroupAndEvent(int groupId, int eventId);
        List<MpResponse> GetOpportunityResponses(int opportunityId, int eventId);
        void RespondToOpportunity(MpRespondToOpportunityDto opportunityResponse);
        int RespondToOpportunity(int participantId, int opportunityId, string comments, int eventId, bool response);
    }
}

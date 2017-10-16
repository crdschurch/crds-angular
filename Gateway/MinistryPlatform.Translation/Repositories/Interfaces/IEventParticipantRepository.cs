using System;
using System.Collections.Generic;
using Crossroads.Utilities.FunctionalHelpers;
using MinistryPlatform.Translation.Models;

namespace MinistryPlatform.Translation.Repositories.Interfaces
{
    public interface IEventParticipantRepository
    {
        bool AddDocumentsToTripParticipant(List<MpTripDocuments> documents, int eventParticipantId);
        List<MpTripParticipant> TripParticipants(string search);
        List<MpEventParticipant> GetChildCareParticipants(int daysBeforeEvent);
        List<MpEventParticipant> GetEventParticipants(int eventId, int? roomId = null);
        IObservable<MpEventParticipant> GetEventParticpant(int eventParticipantId);
        int GetEventParticipantByContactId(int eventId, int contactId);
        MpEventParticipant GetEventParticipantEligibility(int eventId, int contactId);
        DateTime? EventParticipantSignupDate(int contactId , int eventId , string apiToken);
        Result<MpEventParticipant> GetEventParticipantByContactAndEvent(int contactId, int eventId, string token);
        int GetEventParticipantCountByGender(int eventId, int genderId);
        IObservable<MpEventParticipant> GetEventParticpantByEventParticipantWaiver(int invitationSourceId);
    }
}

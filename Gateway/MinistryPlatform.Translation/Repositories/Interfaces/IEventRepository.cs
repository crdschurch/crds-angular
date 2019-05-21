using System;
using System.Collections.Generic;
using Crossroads.Utilities.FunctionalHelpers;
using MinistryPlatform.Translation.Models;
using MinistryPlatform.Translation.Models.EventReservations;
using Participant = MinistryPlatform.Translation.Models.MpParticipant;

namespace MinistryPlatform.Translation.Repositories.Interfaces
{
    public interface IEventRepository
    {
        int CreateEvent(MpEventReservationDto eventReservationReservation);
        void UpdateEvent(MpEventReservationDto eventReservationReservation);
        int SafeRegisterParticipant(int eventId, int participantId, int groupId = 0, int groupParticipantId = 0);
        int RegisterParticipantForEvent(int participantId, int eventId, int groupId = 0, int groupParticipantId = 0);
        int RegisterInterestedParticipantWithEndDate(int participantId, int eventId, DateTime? endDate);
        void SetParticipantAsRegistered(int eventId, int participantId);
        void UpdateParticipantEndDate(int eventParticipantId, DateTime? endDate);
        int UnregisterParticipantForEvent(int participantId, int eventId);
        List<MpEvent> GetEvents(string eventType, string token);
        List<MpEvent> GetEventsByTypeForRange(int eventTypeId, DateTime startDate, DateTime endDate, string token, bool includeCancelledEvents = true);
        List<MpGroup> GetGroupsForEvent(int eventId);
        int GetEventParticipantRecordId(int eventId, int participantId);
        bool EventHasParticipant(int eventId, int participantId);
        MpEvent GetEvent(int eventId);
        List<MpEvent> GetEventsByParentEventId(int parentEventId);
        IEnumerable<MpEvent> EventsByPageId(string token, int pageViewId);
        IEnumerable<MpEvent> EventsByPageViewId(string token, int pageViewId, string search);
        IEnumerable<Participant> EventParticipants(string token, int eventId);
        void SetReminderFlag(int eventId, string token);
        List<MpEventGroup> GetEventGroupsForEvent(int eventId);
        List<MpEventGroup> GetEventGroupsForGroup(int groupId, string token);
        //void CopyEventGroup(EventGroup eventGroup);
        void DeleteEventGroup(MpEventGroup eventGroup);
        int CreateEventGroup(MpEventGroup eventGroup, string token = "");
        void UpdateEventGroup(MpEventGroup eventGroup, string token);

        List<MpEvent> GetEventsBySite(string site, DateTime startDate, DateTime endDate);
        List<MpEvent> GetEventTemplatesBySite(string site);
        void DeleteEventGroupsForEvent(int eventId, int? groupTypeID = null);

        List<MpEventWaivers> GetWaivers(int eventId, int contactId);
        List<MpEventWaivers> GetWaivers(int eventId);
        void SetWaivers(List<MpWaiverResponse> waiverResponses);

        bool IsEventSeries(int eventId);
        Result<int> GetProductEmailTemplate(int eventId);

    }
}
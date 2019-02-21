using System;
using System.Collections.Generic;
using crds_angular.Models.Crossroads.Events;
using MinistryPlatform.Translation.Models;
using MpEvent = MinistryPlatform.Translation.Models.MpEvent;
using Crossroads.Web.Auth.Models;

namespace crds_angular.Services.Interfaces
{
    public interface IEventService
    {
        bool CreateEventReservation(EventToolDto eventTool, string token);
        EventToolDto GetEventReservation(int eventId);
        EventToolDto GetEventRoomDetails(int eventId);
        MpEvent GetEvent(int eventId);
        void RegisterForEvent(EventRsvpDto eventDto, string token);
        IList<Models.Crossroads.Events.Event> EventsReadyForPrimaryContactReminder(string token);
        IList<Models.Crossroads.Events.Event> EventsReadyForReminder(string token);
        IList<MpParticipant> EventParticpants(int eventId, string token);
        void SendReminderEmails();
        void SendPrimaryContactReminderEmails();
        List<MpParticipant> MyChildrenParticipants(int contactId, IList<MpParticipant> children, string token);
        MpEvent GetMyChildcareEvent(int parentEventId, AuthDTO token);
        MpEvent GetChildcareEvent(int parentEventId);
        bool UpdateEventReservation(EventToolDto eventReservation, int eventId, string token);
        EventRoomDto UpdateEventRoom(EventRoomDto eventRoom, int eventId, string token);

        bool CopyEventSetup(int eventTemplateId, int eventId, string token);
        List<MpEvent> GetEventsBySite(string site, string token, DateTime startDate, DateTime endDate);
        List<MpEvent> GetEventTemplatesBySite(string site, string token);
        int AddEventGroup(int eventId, int groupId, string token);
    }
}
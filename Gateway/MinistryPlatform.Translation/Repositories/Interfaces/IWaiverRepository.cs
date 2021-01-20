using System;
using MinistryPlatform.Translation.Models;

namespace MinistryPlatform.Translation.Repositories.Interfaces
{
    public interface IWaiverRepository
    {
        IObservable<MpWaivers> GetWaiver(int waiverId);
        IObservable<MpEventWaivers> GetEventWaivers(int eventId);

        IObservable<MpEventParticipantWaiver> CreateEventParticipantWaiver(int waiverId, int eventParticipantId, int contactId);
        IObservable<MpEventParticipantWaiver> GetEventParticipantWaiversByContact(int eventId, int contactId);
        IObservable<MpEventParticipantWaiver> AcceptEventParticpantWaiver(int eventParticipantWaiverId);
    }
}
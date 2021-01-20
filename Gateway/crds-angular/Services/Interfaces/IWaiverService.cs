using System;
using crds_angular.Models.Crossroads.Waivers;

namespace crds_angular.Services.Interfaces
{
    public interface IWaiverService
    {

        IObservable<WaiverDTO> GetWaiver(int waiverId);
        IObservable<WaiverDTO> EventWaivers(int eventId, int contactId);
        IObservable<int> SendAcceptanceEmail(ContactInvitation contactInvitation);
        IObservable<ContactInvitation> CreateWaiverInvitation(int waiverId, int eventParticipantId, int contactId);
        IObservable<WaiverDTO> AcceptWaiver(string guid);
    }
}
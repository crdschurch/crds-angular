using System;
using System.Collections.Generic;
using System.Reactive;
using crds_angular.Models.Crossroads.Waivers;
using MinistryPlatform.Translation.Models;

namespace crds_angular.Services.Interfaces
{
    public interface IWaiverService
    {

        IObservable<WaiverDTO> GetWaiver(int waiverId);
        IObservable<WaiverDTO> EventWaivers(int eventId, string token);
        IObservable<int> SendAcceptanceEmail(ContactInvitation contactInvitation);
        IObservable<ContactInvitation> CreateWaiverInvitation(int waiverId, int eventParticipantId, string token);
        IObservable<WaiverDTO> AcceptWaiver(string guid);
    }
}
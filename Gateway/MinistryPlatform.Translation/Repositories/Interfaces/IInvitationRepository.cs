using System;
using MinistryPlatform.Translation.Models;

namespace MinistryPlatform.Translation.Repositories.Interfaces
{
    public interface IInvitationRepository
    {
        IObservable<MpInvitation> CreateInvitationAsync(MpInvitation invite);
        MpInvitation CreateInvitation(MpInvitation dto);
        IObservable<MpInvitation> GetOpenInvitationAsync(string invitationGuid);
        MpInvitation GetOpenInvitation(string invitationGuid);
        void MarkInvitationAsUsed(string invitationGuid);
    }
}
using System;
using System.Collections.Generic;
using MinistryPlatform.Translation.Models;

namespace MinistryPlatform.Translation.Repositories.Interfaces
{
    public interface IInvitationRepository
    {
        MpInvitation CreateInvitation(MpInvitation dto);
        MpInvitation GetOpenInvitation(string invitationGuid);
        void MarkInvitationAsUsed(string invitationGuid);
    }
}
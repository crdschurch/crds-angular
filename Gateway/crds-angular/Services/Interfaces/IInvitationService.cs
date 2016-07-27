﻿using crds_angular.Models.Crossroads;
using System.Collections.Generic;
using MinistryPlatform.Translation.Models;

namespace crds_angular.Services.Interfaces
{
    public interface IInvitationService
    {
        Invitation CreateInvitation(Invitation dto);
        void ValidateInvitation(Invitation dto, string token);
    }
}

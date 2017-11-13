using System;
using System.Collections.Generic;
using MinistryPlatform.Translation.Models;

namespace MinistryPlatform.Translation.Repositories.Interfaces
{
    public interface IOrganizationRepository
    {
        MPOrganization GetOrganization(String name, string token);
        List<MPOrganization> GetOrganizations(string token);
        List<MpOrgLocation> GetLocationsForOrganization(int organizationId, string token);
    }
}

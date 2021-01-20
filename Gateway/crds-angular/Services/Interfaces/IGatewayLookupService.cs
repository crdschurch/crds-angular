using System.Collections.Generic;
using crds_angular.Models.Crossroads.Lookups;

namespace crds_angular.Services.Interfaces
{
    public interface IGatewayLookupService
    {
        List<OtherOrganization> GetOtherOrgs(string token = null);
    }
}

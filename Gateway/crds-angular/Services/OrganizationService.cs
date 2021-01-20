using System;
using System.Collections.Generic;
using System.Linq;
using MPInterfaces = MinistryPlatform.Translation.Repositories.Interfaces;
using crds_angular.Models.Crossroads.GoVolunteer;
using crds_angular.Services.Interfaces;
using Crossroads.Web.Common.MinistryPlatform;

namespace crds_angular.Services
{
    public class OrganizationService : IOrganizationService
    {
        private readonly MPInterfaces.IOrganizationRepository _mpOrganizationService;
        private readonly IApiUserRepository _mpApiUserService;

        public OrganizationService(MPInterfaces.IOrganizationRepository organizationService, IApiUserRepository apiUserService)
        {
            _mpOrganizationService = organizationService;
            _mpApiUserService = apiUserService;            
        }

        public Organization GetOrganizationByName(string name)
        {
            var apiUserToken = _mpApiUserService.GetDefaultApiClientToken();
            var org = new Organization();
            var mpOrg = _mpOrganizationService.GetOrganization(name, apiUserToken);
            if (mpOrg != null)
            {
                return org.FromMpOrganization(mpOrg);
            }
            return null;
        }

        public List<Organization> GetOrganizations()
        {
            var apiUserToken = _mpApiUserService.GetDefaultApiClientToken();
            var mpOrgs = _mpOrganizationService.GetOrganizations(apiUserToken);
            return mpOrgs.Select(o =>
            {
                var org = new Organization();
                return org.FromMpOrganization(o);
            }).Where(l => l.EndDate == null || l.EndDate >= DateTime.Today).ToList();
        }

        public List<OrgLocation> GetLocationsForOrganization(int orgId)
        {
            var apiUserToken = _mpApiUserService.GetDefaultApiClientToken();
            var locs = _mpOrganizationService.GetLocationsForOrganization(orgId, apiUserToken);
            return locs.Select(l =>
            {
                var loc = new OrgLocation();
                return loc.FromMpLocation(l);
            }).ToList();
        }
    }
}
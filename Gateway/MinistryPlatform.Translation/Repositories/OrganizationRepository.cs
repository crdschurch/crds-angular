using System.Collections.Generic;
using System.Linq;
using Crossroads.Web.Common.Configuration;
using Crossroads.Web.Common.Security;
using MinistryPlatform.Translation.Extensions;
using MinistryPlatform.Translation.Models;
using MinistryPlatform.Translation.Repositories.Interfaces;
using Crossroads.Web.Common.MinistryPlatform;

namespace MinistryPlatform.Translation.Repositories
{
    public class OrganizationRepository : BaseRepository,IOrganizationRepository
    {
        private readonly IMinistryPlatformService _ministryPlatformService;

        public OrganizationRepository(
            IAuthenticationRepository authenticationService,
            IConfigurationWrapper configurationWrapper,
            IMinistryPlatformService ministryPlatformService,
            IApiUserRepository apiUserRepository)
            : base(authenticationService, configurationWrapper, apiUserRepository)
        {
            _ministryPlatformService = ministryPlatformService;
        }

        public MPOrganization GetOrganization(string name, string token)
        {
            var search = string.Format("{0},", name);
            var result = _ministryPlatformService.GetRecordsDict(_configurationWrapper.GetConfigIntValue("OrganizationsPage"), token, search);            
            return MapOrganizations(result).SingleOrDefault();
        }

        public List<MPOrganization> GetOrganizations(string token)
        {
            var result = _ministryPlatformService.GetRecordsDict(_configurationWrapper.GetConfigIntValue("OrganizationsPage"), token);
            return MapOrganizations(result);
        }

        public List<MpOrgLocation> GetLocationsForOrganization(int orgId, string token)
        {
            var result = _ministryPlatformService.GetSubpageViewRecords(_configurationWrapper.GetConfigIntValue("LocationsForOrg"), orgId, token);

            return result.Select(record => new MpOrgLocation
            {
                LocationId = record.ToInt("Location ID"),
                LocationName = record.ToString("Location Name"),
                LocationTypeId = record.ToInt("Location Type ID"),
                OrganizationId = record.ToInt("Organization ID"),
                OrganizationName = record.ToString("Name"),
                Address = record.ToString("Address Line 1"),
                City = record.ToString("City"),
                State = record.ToString("State/Region"),
                Zip = record.ToString("Postal Code"),
                ImageUrl = record.ToString("Image URL")
            }).ToList();
        }

        private static List<MPOrganization> MapOrganizations(IEnumerable<Dictionary<string, object>> records )
        {
            return records.Select(record => new MPOrganization
            {
                ContactId = record.ToInt("Primary_Contact"),
                EndDate = record.ToNullableDate("End_Date"),
                StartDate = record.ToDate("Start_Date"),
                Name = record.ToString("Name"),
                OpenSignup = record.ToBool("Open_Signup"),
                OrganizationId = record.ToInt("dp_RecordID"),
                ImageUrl = record.ToString("Image_URL")
            }).ToList();
        }
    }
}

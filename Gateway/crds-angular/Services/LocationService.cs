using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using crds_angular.Models.Crossroads;
using crds_angular.Models.Crossroads.GoVolunteer;
using crds_angular.Services.Interfaces;
using Crossroads.Web.Common.Configuration;

namespace crds_angular.Services
{
    public class LocationService : ILocationService
    {
        private readonly IConfigurationWrapper _configurationWrapper;
        private readonly IOrganizationService _organizationService;
        private readonly IAddressProximityService _proximityService;

        public LocationService(IConfigurationWrapper configurationWrapper,
                               IOrganizationService organizationService,
                               IAddressProximityService proximityService)
        {
            _configurationWrapper = configurationWrapper;
            _organizationService = organizationService;
            _proximityService = proximityService;
        }

        public List<OrgLocation> GetAllCrossroadsLocations()
        {
            return _organizationService.GetLocationsForOrganization(_configurationWrapper.GetConfigIntValue("CrossroadsOrgId"));
        }

        public List<LocationProximityDto> GetDistanceToCrossroadsLocations(string origin)
        {
            var locations = GetAllCrossroadsLocations();
            List<AddressDTO> addresses = locations.Select(location => new AddressDTO()
            {
                AddressLine1 = location.Address,
                City = location.City,
                State = location.State,
                PostalCode = location.Zip
            }).ToList();
            List<decimal?> distances = _proximityService.GetProximity(origin, addresses);
            List<LocationProximityDto> locationProximityList = new List<LocationProximityDto>();
            for (int i = 0; i < addresses.Count; i++)
            {
                locationProximityList.Add(new LocationProximityDto()
                {
                    Origin = origin,
                    Location = locations[i],
                    Distance = distances?[i]
                });
            }
            return locationProximityList;
        }
    }
}
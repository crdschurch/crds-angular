using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using AutoMapper;
using crds_angular.Models;
using crds_angular.Models.Crossroads;
using crds_angular.Models.Crossroads.GoVolunteer;
using crds_angular.Services.Interfaces;
using Crossroads.Web.Common.Configuration;
using MinistryPlatform.Translation.Models;
using MinistryPlatform.Translation.Repositories.Interfaces;

namespace crds_angular.Services
{
    public class LocationService : ILocationService
    {
        private readonly ILocationRepository _locationRepository;
        private readonly IAddressProximityService _proximityService;

        public LocationService(ILocationRepository locationRepository,
                               IAddressProximityService proximityService)
        {
            _locationRepository = locationRepository;
            _proximityService = proximityService;
        }

        public List<LocationDTO> GetAllCrossroadsLocations()
        {
            var locations = _locationRepository.GetLocations("Available_Online=1");
            List<LocationDTO> locationDtos = locations.Select(Mapper.Map<MpLocation, LocationDTO>).ToList();
            return locationDtos;
        }

        public List<LocationProximityDto> GetDistanceToCrossroadsLocations(string origin)
        {
            var locations = GetAllCrossroadsLocations();
            List<AddressDTO> addresses = locations.Select(location => location.Address).ToList();
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
            return locationProximityList.OrderBy(o => o.Distance).ToList();
        }
    }
}
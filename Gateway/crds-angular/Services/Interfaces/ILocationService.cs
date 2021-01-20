using System.Collections.Generic;
using crds_angular.Models.Crossroads;

namespace crds_angular.Services.Interfaces
{
    public interface ILocationService
    {
        List<LocationDTO> GetAllCrossroadsLocations();
        List<LocationProximityDto> GetDistanceToCrossroadsLocations(string origin);
    }
}

using System.Device.Location;
using crds_angular.Models.Crossroads;
using crds_angular.Models.Finder;

namespace crds_angular.Services.Interfaces
{
    public interface IAddressService
    {
        void FindOrCreateAddress(AddressDTO address, bool updateGeoCoordinates = false);
        int CreateAddress(AddressDTO address);
        void SetGeoCoordinates(AddressDTO address);
        void SetGroupPinGeoCoordinates(PinDto pin);
        GeoCoordinate GetGeoLocationCascading(AddressDTO address);
        void SetGeoCoordinates(int addressid);
    }
}
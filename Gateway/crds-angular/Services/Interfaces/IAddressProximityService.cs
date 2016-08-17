﻿using System.Collections.Generic;
using crds_angular.Models.Crossroads;
using System.Device.Location;

namespace crds_angular.Services.Interfaces
{
    public interface IAddressProximityService
    {
        GeoCoordinate GetGeoCoordinates(string address);
        GeoCoordinate GetGeoCoordinates(AddressDTO address);
        AddressDTO ValidateAddress(string address);
        AddressDTO ValidateAddress(AddressDTO address);
        List<decimal?> GetProximity(string originAddress, List<AddressDTO> destinationAddresses);
    }
}
﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using crds_angular.Models.Crossroads;
using crds_angular.Services.Interfaces;
using MinistryPlatform.Translation.Models;

namespace crds_angular.Services
{
    public class AddressService : IAddressService
    {
        private readonly MinistryPlatform.Translation.Repositories.Interfaces.IAddressRepository _mpAddressService;

        public AddressService(MinistryPlatform.Translation.Repositories.Interfaces.IAddressRepository mpAddressService)
        {
            _mpAddressService = mpAddressService;
        }

        public void FindOrCreateAddress(AddressDTO address)
        {
            var mpAddress = AutoMapper.Mapper.Map<MpAddress>(address);
            var found = FindExistingAddress(address, mpAddress);
            if (found)
            {
                return;
            } 

            address.AddressID = CreateAddress(mpAddress);
        }

        private int CreateAddress(MpAddress address)
        {
            return _mpAddressService.Create(address);
        }

        private bool FindExistingAddress(AddressDTO address, MpAddress mpAddress)
        {
            var result = _mpAddressService.FindMatches(mpAddress);
            if (result.Count > 0)
            {
                var addressId = result.First(x => x.Address_ID.HasValue).Address_ID;
                if (addressId != null)
                {
                    address.AddressID = addressId.Value;
                    return true;
                }
            }

            return false;
        }
    }
}
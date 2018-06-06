using System;
using System.Linq;
using System.Collections.Generic;
using Crossroads.Utilities.Interfaces;
using Crossroads.Web.Common;
using Crossroads.Web.Common.Configuration;
using Crossroads.Web.Common.MinistryPlatform;
using MinistryPlatform.Translation.Extensions;
using MinistryPlatform.Translation.Models;
using MinistryPlatform.Translation.Repositories.Interfaces;
using MinistryPlatform.Translation.Helpers;

namespace MinistryPlatform.Translation.Repositories
{
    public class AddressRepository : IAddressRepository
    {
        private readonly IMinistryPlatformService _ministryPlatformService;
        private readonly IMinistryPlatformRestRepository _ministryPlatformRestRepository;
        private readonly IApiUserRepository _apiUserService;
        private readonly int _addressPageId;
        private readonly int _addressApiPageViewId;

        public AddressRepository(IConfigurationWrapper configurationWrapper, IMinistryPlatformService ministryPlatformService, IApiUserRepository apiUserService, IMinistryPlatformRestRepository mpRestRepository)
        {
            _ministryPlatformService = ministryPlatformService;
            _apiUserService = apiUserService;
            _addressPageId = configurationWrapper.GetConfigIntValue("Addresses");
            _addressApiPageViewId = configurationWrapper.GetConfigIntValue("AddressesApiPageView");
            _ministryPlatformRestRepository = mpRestRepository;
        }

        public int Create(MpAddress address)
        {
            var apiToken = _apiUserService.GetDefaultApiClientToken();

            var values = MapAddressDictionary(address);

            var addressId = _ministryPlatformService.CreateRecord(_addressPageId, values, apiToken);

            return addressId;
        }

        public int Update(MpAddress address)
        {
            var apiToken = _apiUserService.GetDefaultApiClientToken();

            var updatedAddress = _ministryPlatformRestRepository.UsingAuthenticationToken(apiToken).Update(address);    

            return updatedAddress.Address_ID.Value;
        }

        private static Dictionary<string, object> MapAddressDictionary(MpAddress address)
        {
            var values = new Dictionary<string, object>()
            {
                {"Address_Line_1", address.Address_Line_1},
                {"Address_Line_2", address.Address_Line_2},
                {"City", address.City},
                {"State/Region", address.State},
                {"Postal_Code", address.Postal_Code},
                {"Foreign_Country", address.Foreign_Country},
                {"County", address.County},
                {"Longitude", address.Longitude },
                {"Latitude", address.Latitude }
            };

            return values;
        }

        public List<MpAddress> FindMatches(MpAddress address)
        {
            var apiToken = _apiUserService.GetDefaultApiClientToken();
            var search = string.Format("{0}, {1}, {2}, {3}, {4}, {5}",
                                       AddQuotesIfNotEmpty(address.Address_Line_1),
                                       AddQuotesIfNotEmpty(address.Address_Line_2),
                                       AddQuotesIfNotEmpty(address.City),
                                       AddQuotesIfNotEmpty(address.State),
                                       AddQuotesIfNotEmpty(address.Postal_Code),
                                       AddQuotesIfNotEmpty(address.Foreign_Country));

            var records = _ministryPlatformService.GetPageViewRecords(_addressApiPageViewId, apiToken, search);

            object longitudeObj;
            object latitudeObj;
            double latitude;
            double longitude;
            var addresses = records.Select(record => new MpAddress()
            {
                Address_ID = record.ToInt("dp_RecordID"),
                Address_Line_1 = record.ToString("Address_Line_1"),
                Address_Line_2 = record.ToString("Address_Line_2"),
                City = record.ToString("City"),
                State = record.ToString("State/Region"),
                Postal_Code = record.ToString("Postal_Code"),
                Foreign_Country = record.ToString("Foreign_Country"),
                Latitude = record.TryGetValue("Latitude", out latitudeObj) && latitudeObj != null && double.TryParse(latitudeObj.ToString(), out latitude) ? latitude : (double?)null,
                Longitude = record.TryGetValue("Longitude", out longitudeObj) && longitudeObj != null && double.TryParse(longitudeObj.ToString(), out longitude) ? longitude : (double?)null,
            }).ToList();

            return addresses;
        }

        private string AddQuotesIfNotEmpty(string input)
        {
            if (String.IsNullOrEmpty(input))
            {
                return input;
            }

            return string.Format("\"{0}\"", input);
        }

        public MpAddress GetAddressById(string token, int id)
        {
            var record = _ministryPlatformService.GetRecordDict(_addressPageId, id, token);

            var address = new MpAddress()
            {
                Address_ID = record.ToInt("Address_ID"),
                Address_Line_1 = record.ToString("Address_Line_1"),
                Address_Line_2 = record.ToString("Address_Line_2"),
                City = record.ToString("City"),
                State = record.ToString("State/Region"),
                Postal_Code = record.ToString("Postal_Code")
            };

            return address;
        }
}
}
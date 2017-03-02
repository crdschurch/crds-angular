﻿using System;
using System.Net;
using System.Collections.Generic;
using System.IO;
using AutoMapper;
using crds_angular.Models.Finder;
using crds_angular.Models.Crossroads;
using crds_angular.Services.Interfaces;
using MinistryPlatform.Translation.Repositories.Interfaces;
using log4net;
using MinistryPlatform.Translation.Models;
using Newtonsoft.Json;
using Crossroads.Web.Common.Configuration;
using Crossroads.Web.Common.MinistryPlatform;
using System.Linq;

namespace crds_angular.Services
{

    public class RemoteAddress
    {
        public string Ip { get; set; }
        public string region_code { get; set; }
        public string city { get; set; }
        public string zip_code { get; set; }
        public double latitude { get; set; }
        public double longitude { get; set; }
    }

    public class FinderService : MinistryPlatformBaseService, IFinderService
    {
        private readonly IContactRepository _contactRepository;
        private readonly ILog _logger = LogManager.GetLogger(typeof(AddressService));
        private readonly IFinderRepository _finderRepository;
        private readonly IParticipantRepository _participantRepository;
        private readonly IAddressService _addressService;
        private readonly IGroupService _groupService;
        private readonly IApiUserRepository _apiUserRepository;
        private readonly int _approvedHost;
        private readonly int _anywhereGroupType;


        public FinderService(IFinderRepository finderRepository,
                            IContactRepository contactRepository, 
                            IAddressService addressService, 
                            IParticipantRepository participantRepository, 
                            IGroupService groupService,
                            IApiUserRepository apiUserRepository,
                            IConfigurationWrapper configurationWrapper
                            )
        {
            _finderRepository = finderRepository;
            _contactRepository = contactRepository;
            _addressService = addressService;
            _participantRepository = participantRepository;
            _groupService = groupService;
            _apiUserRepository = apiUserRepository;
            _approvedHost = configurationWrapper.GetConfigIntValue("ApprovedHostStatus");
            _anywhereGroupType = configurationWrapper.GetConfigIntValue("AnywhereGroupTypeId");
        }


        public PinDto GetPinDetails(int participantId)
        {
            var token = _apiUserRepository.GetToken();
            //first get pin details
            var pinDetails = Mapper.Map<PinDto>(_finderRepository.GetPinDetails(participantId));

            //get group details
            if (pinDetails.Host_Status_ID == _approvedHost)
            {
                pinDetails.Gathering = _groupService.GetGroupsByTypeOrId(token, participantId, new int[] {_anywhereGroupType}).FirstOrDefault();
            }
            
            //make sure we have a lat/long
            if (pinDetails.Address.Latitude == null || pinDetails.Address.Longitude == null)
            {
                _addressService.SetGeoCoordinates(pinDetails.Address);
            }
            // randomize the location
            pinDetails.Address = RandomizeLatLong(pinDetails.Address);
            //TODO get group details
            return pinDetails;
        }

        public void EnablePin(int participantId)
        {
            _finderRepository.EnablePin(participantId);
        }

        public void UpdateHouseholdAddress(PinDto pin)
        {
            if (pin.isFormDirty || (!pin.isFormDirty && !pin.Address.HasGeoCoordinates()))
            {
                _addressService.SetGeoCoordinates(pin.Address);
            }          

            var householdDictionary = new Dictionary<string, object> { { "Household_ID", pin.Household_ID } };
            var address = Mapper.Map<MpAddress>(pin.Address);
            var addressDictionary = getDictionary(address);
            addressDictionary.Add("State/Region", addressDictionary["State"]);
            _contactRepository.UpdateHouseholdAddress(pin.Contact_ID, householdDictionary, addressDictionary);
        }



        public AddressDTO GetAddressForIp(string ip)
        {
            var address = new AddressDTO();
            var request = WebRequest.Create("http://freegeoip.net/json/" + ip);
            using (var response = request.GetResponse())
            using (var stream = new StreamReader(response.GetResponseStream()))
            {
                var responseString = stream.ReadToEnd();
                var s = JsonConvert.DeserializeObject<RemoteAddress>(responseString);
                address.City = s.city;
                address.State = s.region_code;
                address.PostalCode = s.zip_code;
                address.Latitude = s.latitude;
                address.Longitude = s.longitude;
            }
            return address;
        }

        public int GetParticipantIdFromContact(int contactId)
        {
            var participant = _participantRepository.GetParticipant(contactId);
            return participant.ParticipantId;
        }

        public AddressDTO RandomizeLatLong(AddressDTO address)
        {
            if (!address.HasGeoCoordinates()) return address;
            var random = new Random(DateTime.Now.Millisecond);
            var distance = random.Next(40, 350); // up to a quarter mile
            var angle = random.Next(0, 359);
            const int earthRadius = 6371000; // in meters

            var distanceNorth = Math.Sin(angle) * angle;
            var distanceEast = Math.Cos(angle) * distance;

            double newLat = (double)(address.Latitude + (distanceNorth / earthRadius) * 180 / Math.PI);
            double newLong = (double)(address.Longitude + (distanceEast / (earthRadius * Math.Cos(newLat * 180 / Math.PI))) * 180 / Math.PI);
            address.Latitude = newLat;
            address.Longitude = newLong;

            return address;
        }
    }
}
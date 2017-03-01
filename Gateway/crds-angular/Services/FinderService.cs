﻿using System.Net;
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
using System.Device.Location;
using MinistryPlatform.Translation.Models.Finder;
using System.Device.Location;
using Crossroads.Web.Common.Configuration;

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
        private readonly IAddressGeocodingService _addressGeocodingService;
        private readonly IContactRepository _contactRepository;
        private readonly ILog _logger = LogManager.GetLogger(typeof(AddressService));
        private readonly IFinderRepository _finderRepository;
        private readonly IParticipantRepository _participantRepository;
        private readonly IAddressService _addressService;
        private readonly IGroupToolService _groupToolService;
        private readonly IConfigurationWrapper _configurationWrapper;

        public FinderService(IAddressGeocodingService addressGeocodingService, 
                             IFinderRepository finderRepository, 
                             IContactRepository contactRepository, 
                             IAddressService addressService, 
                             IParticipantRepository participantRepository,
                             IGroupToolService groupToolService,
                             IConfigurationWrapper configurationWrapper)
        {
            _addressGeocodingService = addressGeocodingService;
            _finderRepository = finderRepository;
            _contactRepository = contactRepository;
            _addressService = addressService;
            _participantRepository = participantRepository;
            _groupToolService = groupToolService;
            _configurationWrapper = configurationWrapper;
        }


        public PinDto GetPinDetails(int participantId)
        {
            //first get pin details
            var pinDetails = Mapper.Map<PinDto>(_finderRepository.GetPinDetails(participantId));

            //make sure we have a lat/long
            if (pinDetails.Address.Latitude == null || pinDetails.Address.Longitude == null)
            {
                _addressService.SetGeoCoordinates(pinDetails.Address);
            }

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
            _contactRepository.UpdateHouseholdAddress((int)pin.Contact_ID, householdDictionary, addressDictionary);
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

        public List<PinDto> GetPinsInRadius(GeoCoordinate originCoords, string address)
        {
            _finderRepository.GetPinsInRadius(originCoords); //get participants on map within radius 
            //get buildings on map within radius
            //get groups on map within radius 
            var groupPins = GetGroupPinsinRadius(originCoords,address);

            var pins = new List<PinDto>();

            List<SpPinDto> participantPinsFromSp = _finderRepository.GetPinsInRadius(originCoords);

            List<PinDto> participantPins = new List<PinDto>();

            foreach (SpPinDto piFromSP in participantPinsFromSp)
            {
                PinDto pin = Mapper.Map<PinDto>(piFromSP);
                participantPins.Add(pin); 
            }

            pins.AddRange(participantPins);

            return pins; 

        }

        private List<PinDto> GetGroupPinsinRadius(GeoCoordinate originCoords, string address)
        {
            // ignoring originCoords at this time
            var pins = new List<PinDto>();

            // get group for anywhere gathering
            var anywhereGroupTypeId = _configurationWrapper.GetConfigIntValue("AnywhereGatheringGroupTypeId");
            var groups = _groupToolService.SearchGroups(new int[]  { anywhereGroupTypeId},null,address,null);

            foreach (var group in groups)
            {
                pins.Add(Mapper.Map<PinDto>(group));
            }//
            // set pin type
            
            return pins;
        }
    }
}
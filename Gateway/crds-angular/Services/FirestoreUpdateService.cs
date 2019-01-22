﻿using AutoMapper;
using System;
using System.Threading.Tasks;
using crds_angular.Services.Interfaces;
using Crossroads.Web.Common.Configuration;
using Google.Cloud.Firestore;
using Google.Cloud.Storage.V1;
using log4net;
using crds_angular.Models.Crossroads;
using MinistryPlatform.Translation.Models.Finder;
using MinistryPlatform.Translation.Repositories.Interfaces;
using MinistryPlatform.Translation.Models;
using crds_angular.Models.Map;
using Crossroads.Web.Common.MinistryPlatform;
using NGeoHash.Portable;
using System.Collections.Generic;
using System.Linq;
using crds_angular.Models.Crossroads.Attribute;

namespace crds_angular.Services
{
    public class FirestoreUpdateService : IFirestoreUpdateService
    {
        private readonly ILog _logger = LogManager.GetLogger(typeof(FinderService));
        private readonly IImageService _imageService;
        private readonly IConfigurationWrapper _configurationWrapper;
        private readonly IFinderRepository _finderRepository;
        private readonly IApiUserRepository _apiUserRepository;
        private readonly IContactRepository _contactRepository;
        private readonly IAddressRepository _addressRepository;
        private readonly IGroupService _groupService;
        private readonly IAddressGeocodingService _addressGeocodingService;
        private readonly ICongregationRepository _congregationRepository;
        private readonly ILocationService _locationRepository;

        private readonly string _googleStorageBucketId;
        private readonly string _firestoreProjectId;

        private readonly Random _random = new Random(DateTime.Now.Millisecond);

        private const int PIN_PERSON = 1;
        private const int PIN_GROUP = 2;
        private const int PIN_SITE = 3;

        public FirestoreUpdateService(IImageService imageService, 
                                      IConfigurationWrapper configurationWrapper, 
                                      IFinderRepository finderRepository,
                                      IApiUserRepository apiUserRepository,
                                      IContactRepository contactRepository,
                                      IAddressRepository addressRepository,
                                      IGroupService groupService,
                                      IAddressGeocodingService addressGeocodingService,
                                      ICongregationRepository congregationRepository,
                                      ILocationService locationRepository)
        {
            // dependencies
            _imageService = imageService;
            _configurationWrapper = configurationWrapper;
            _finderRepository = finderRepository;
            _apiUserRepository = apiUserRepository;
            _contactRepository = contactRepository;
            _addressRepository = addressRepository;
            _groupService = groupService;
            _addressGeocodingService = addressGeocodingService;
            _congregationRepository = congregationRepository;
            _locationRepository = locationRepository;
            //constants
            _googleStorageBucketId = configurationWrapper.GetConfigValue("GoogleStorageBucketId");
            _firestoreProjectId = configurationWrapper.GetConfigValue("FirestoreMapProjectId");
        }

        public string SendProfilePhotoToFirestore(int participantId)
        {
            string urlForPhoto = "";
            try
            {
                // get the photo from someplace
                var memStream = _imageService.GetParticipantImage(participantId);
                if (memStream != null)
                {
                    var client = StorageClient.Create();
                    
                    var bucketName = _googleStorageBucketId;
                    var bucket = client.GetBucket(bucketName);
                    _logger.Info($"FIRESTORE: SendProfilePhotoToFirestore - bucketName = {bucketName}");
                    _logger.Info($"FIRESTORE: SendProfilePhotoToFirestore - participantId = {participantId}");
                    var photoUpload = client.UploadObject(bucketName, $"{participantId}.png", "image/png", memStream);
                    urlForPhoto = photoUpload.MediaLink;
                    _logger.Info($"FIRESTORE: SendProfilePhotoToFirestore - timecreated = {photoUpload.TimeCreated}");
                    _logger.Info($"FIRESTORE: SendProfilePhotoToFirestore - name = {photoUpload.Name}");
                    _logger.Info($"FIRESTORE: SendProfilePhotoToFirestore - urlForPhoto = {urlForPhoto}");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                _logger.Info($"FIRESTORE: SendProfilePhotoToFirestore - {ex.Message}");
            }
            return urlForPhoto;
        }

        public void DeleteProfilePhotoFromFirestore(int participantId)
        {
            try
            {
                var client = StorageClient.Create();
                var bucketName = _googleStorageBucketId;
                _logger.Info($"FIRESTORE: DeleteProfilePhotoFromFirestore - bucketName = {bucketName}");
                _logger.Info($"FIRESTORE: DeleteProfilePhotoFromFirestore - participantId = {participantId}");
                client.DeleteObject(bucketName, $"{participantId}.png");
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                _logger.Info($"FIRESTORE: DeleteProfilePhotoFromFirestore - {ex.Message}");
            }
        }

        /// <summary>
        /// This is the main point of entry for the addition of map pins to firestore
        /// </summary>
        /// <returns></returns>
        public async Task ProcessMapAuditRecords()
        {
            try
            {
                var recordList = _finderRepository.GetMapAuditRecords();
                while (recordList.Count > 0)
                {
                    foreach (MpMapAudit mapAuditRecord in recordList)
                    {
                        switch (Convert.ToInt32(mapAuditRecord.pinType))
                        {
                            case PIN_PERSON:
                                var personpinupdatedsuccessfully = await PersonPinToFirestoreAsync(mapAuditRecord.ParticipantId, mapAuditRecord.showOnMap, mapAuditRecord.pinType);
                                SetRecordProcessedFlag(mapAuditRecord, personpinupdatedsuccessfully);
                                break;
                            case PIN_GROUP:
                                var grouppinupdatedsuccessfully = await GroupPinToFirestoreAsync(mapAuditRecord.ParticipantId, mapAuditRecord.showOnMap, mapAuditRecord.pinType);
                                SetRecordProcessedFlag(mapAuditRecord, grouppinupdatedsuccessfully);
                                break;
                            case PIN_SITE:
                                var sitepinupdatedsuccessfully = await SitePinToFirestoreAsync(mapAuditRecord.ParticipantId, mapAuditRecord.showOnMap, mapAuditRecord.pinType);
                                SetRecordProcessedFlag(mapAuditRecord, sitepinupdatedsuccessfully);
                                break;
                        }
                    }
                    recordList = _finderRepository.GetMapAuditRecords();
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
            }
        }

        private void SetRecordProcessedFlag(MpMapAudit mapAuditRecord, bool success)
        {
            if (success)
            {
                mapAuditRecord.processed = true;
                mapAuditRecord.dateProcessed = DateTime.Now;
                _finderRepository.MarkMapAuditRecordAsProcessed(mapAuditRecord);
            }
        }

        ////////////////////////////////////////////////////////////////////////////////////////////////////////////////
        /// SITE PIN
        ////////////////////////////////////////////////////////////////////////////////////////////////////////////////
        private async Task<bool> SitePinToFirestoreAsync(int congregationid, bool showOnMap, string pinType)
        {
            // showonmap = true then add to firestore
            // showonmap = false then delete from firestore
            Console.WriteLine($"congregationid = {congregationid}, showonmap = {showOnMap}, pintype = {pinType}");
            if (showOnMap)
            {
                await DeleteSitePinFromFirestoreAsync(congregationid, pinType);
                return await AddSitePinToFirestoreAsync(congregationid, pinType);
            }
            else
            {
                return await DeleteSitePinFromFirestoreAsync(congregationid, pinType);
            }
        }

        private async Task<bool> DeleteSitePinFromFirestoreAsync(int congregationid, string pinType)
        {
            return await DeletePinFromFirestoreAsync(congregationid, pinType);
        }

        private async Task<bool> AddSitePinToFirestoreAsync(int congregationid, string pinType)
        {
            var apiToken = _apiUserRepository.GetDefaultApiClientToken();
            var address = new AddressDTO();
            try
            {
                var congregation = _congregationRepository.GetCongregationById(congregationid);
                var location = _locationRepository.GetAllCrossroadsLocations().Where(s => s.LocationId == congregation.LocationId).First();
                // get the address including lat/lon
                if (location.Address != null )
                {
                    address = location.Address;
                }
                else
                {
                    // no address for this contact/participant
                    return true;
                }

                var geohash = GeoHash.Encode(address.Latitude != null ? (double)address.Latitude : 0, address.Longitude != null ? (double)address.Longitude : 0);

                // create the pin object
                MapPin pin = new MapPin(congregation.Name, congregation.Name, address.Latitude != null ? (double)address.Latitude : 0, address.Longitude != null ? (double)address.Longitude : 0, Convert.ToInt32(pinType), congregationid.ToString(), geohash, "", null);

                FirestoreDb db = FirestoreDb.Create(_firestoreProjectId);
                CollectionReference collection = db.Collection("Pins");
                DocumentReference document = await collection.AddAsync(pin);
                Console.WriteLine(document.Id);
            }
            catch (Exception e)
            {
                Console.WriteLine("Problem getting MP Data for PinSync");
                Console.WriteLine(e.Message);
                return false;
            }
            return true;
        }

        ////////////////////////////////////////////////////////////////////////////////////////////////////////////////
        /// GROUP PIN
        ////////////////////////////////////////////////////////////////////////////////////////////////////////////////
        private async Task<bool> GroupPinToFirestoreAsync(int groupid, bool showOnMap, string pinType)
        {
            // showonmap = true then add to firestore
            // showonmap = false then delete from firestore
            Console.WriteLine($"groupid = {groupid}, showonmap = {showOnMap}, pintype = {pinType}");
            if (showOnMap)
            {
                await DeleteGroupPinFromFirestoreAsync(groupid, pinType);
                return await AddGroupPinToFirestoreAsync(groupid, pinType);
            }
            else
            {
                return await DeleteGroupPinFromFirestoreAsync(groupid, pinType);
            }
        }

        private async Task<bool> DeleteGroupPinFromFirestoreAsync(int groupid, string pinType)
        {
            return await DeletePinFromFirestoreAsync(groupid, pinType);
        }

        private async Task<bool> AddGroupPinToFirestoreAsync(int groupid, string pinType)
        {
            var apiToken = _apiUserRepository.GetDefaultApiClientToken();
            var address = new AddressDTO();
            try
            {
                //var group = _groupRepository.getGroupDetails(groupid);
                var group = _groupService.GetGroupDetailsWithAttributes(groupid);              
                var s = group.SingleAttributes;
                var t = group.AttributeTypes;

                // get the address including lat/lon	
                if (group.Address.AddressID == null)
                {
                    // Something with no address should not go on a map	
                    return true;
                }

                var addrFromDB = _addressRepository.GetAddressById(apiToken, (int)group.Address.AddressID);
                // if there is no lat/lon lets give one last attempt at geocoding
                if (address.Latitude == null || address.Longitude == null || address.Latitude == 0 || address.Longitude == 0)
                {
                    var geo = _addressGeocodingService.GetGeoCoordinates(Mapper.Map<AddressDTO>(addrFromDB));
                    if (geo.Latitude != 0 && geo.Longitude != 0)
                    {
                        addrFromDB.Latitude = geo.Latitude;
                        addrFromDB.Longitude = geo.Longitude;
                        _addressRepository.Update(addrFromDB);
                    }
                }

                _logger.Info($"FIRESTORE: AddGroupPinToFirestoreAsync - addrFromDB.Address_ID = {addrFromDB.Address_ID}");
                _logger.Info($"FIRESTORE: AddGroupPinToFirestoreAsync - addrFromDB.Address_Line_1 = {addrFromDB.Address_Line_1}");
                _logger.Info($"FIRESTORE: AddGroupPinToFirestoreAsync - addrFromDB.City = {addrFromDB.City}");
                _logger.Info($"FIRESTORE: AddGroupPinToFirestoreAsync - addrFromDB.State = {addrFromDB.State}");
                _logger.Info($"FIRESTORE: AddGroupPinToFirestoreAsync - addrFromDB.Latitude = {addrFromDB.Latitude}");
                _logger.Info($"FIRESTORE: AddGroupPinToFirestoreAsync - addrFromDB.Longitude = {addrFromDB.Longitude}");

                address = this.RandomizeLatLong(Mapper.Map<AddressDTO>(addrFromDB));
                var geohash = GeoHash.Encode(address.Latitude != null ? (double)address.Latitude : 0, address.Longitude != null ? (double)address.Longitude : 0);
                _logger.Info("FIRESTORE: After Map");
                _logger.Info($"FIRESTORE: AddGroupPinToFirestoreAsync - address.Address_ID = {address.AddressID}");
                _logger.Info($"FIRESTORE: AddGroupPinToFirestoreAsync - address.Address_Line_1 = {address.AddressLine1}");
                _logger.Info($"FIRESTORE: AddGroupPinToFirestoreAsync - address.City = {address.City}");
                _logger.Info($"FIRESTORE: AddGroupPinToFirestoreAsync - address.State = {address.State}");
                _logger.Info($"FIRESTORE: AddGroupPinToFirestoreAsync - address.Latitude = {address.Latitude}");
                _logger.Info($"FIRESTORE: AddGroupPinToFirestoreAsync - address.Longitude = {address.Longitude}");

                // if we are at 0,0 we should fail.	
                if (address.Latitude == null ||
                   address.Longitude == null ||
                   (address.Latitude == 0 && address.Longitude == 0))
                {
                    return true;
                }

                var dict = new Dictionary<string, string[]>();
                
                // get grouptype
                ObjectSingleAttributeDTO grouptype;
                if (s.TryGetValue(73, out grouptype))
                {
                    // grouptype is now equal to the value
                    var x = grouptype.Value;
                    dict.Add("GroupType", new string[] { x.Name });
                }

                // get age groups
                ObjectAttributeTypeDTO agegroup;
                if(t.TryGetValue(91, out agegroup))
                {
                    // roll through the age group. add selected to the dictionary
                    var ageGroups = new List<string>();
                    foreach( var a in agegroup.Attributes)
                    {
                        if (a.Selected)
                        {
                            ageGroups.Add(a.Name);
                        }
                    }

                    // add to the dict
                    if(ageGroups.Count > 0)
                    {
                        dict.Add("AgeGroups",  ageGroups.ToArray() );
                    }
                }

                // get group categories
                ObjectAttributeTypeDTO groupcategory;
                if(t.TryGetValue(90, out groupcategory))
                {
                    // roll through the age group. add selected to the dictionary
                    var categories = new List<string>();
                    foreach (var a in groupcategory.Attributes)
                    {
                        if (a.Selected)
                        {
                            categories.Add(a.Category);
                        }
                    }

                    // add to the dict
                    if (categories.Count > 0)
                    {
                        dict.Add("Categories", categories.ToArray());
                    }
                }

                // create the pin object
                MapPin pin = new MapPin(group.GroupDescription, group.GroupName, address.Latitude != null ? (double)address.Latitude : 0, address.Longitude != null ? (double)address.Longitude : 0, Convert.ToInt32(pinType), 
                    groupid.ToString(), geohash, "", dict);

                FirestoreDb db = FirestoreDb.Create(_firestoreProjectId);
                CollectionReference collection = db.Collection("Pins");
                DocumentReference document = await collection.AddAsync(pin);
                Console.WriteLine(document.Id);
            }
            catch (Exception e)
            {
                Console.WriteLine("Problem getting MP Data for PinSync");
                Console.WriteLine(e.Message);
                return false;
            }
            return true;
        }

        ////////////////////////////////////////////////////////////////////////////////////////////////////////////////
        /// PERSON PIN
        ////////////////////////////////////////////////////////////////////////////////////////////////////////////////
        public async Task<bool> PersonPinToFirestoreAsync(int participantid, bool showOnMap, string pinType)
        {
            // showonmap = true then add to firestore
            // showonmap = false then delete from firestore
            Console.WriteLine($"participantid = {participantid}, showonmap = {showOnMap}, pintype = {pinType}");
            if (showOnMap)
            {
                await DeletePersonPinFromFirestoreAsync(participantid, pinType);
                return await AddPersonPinToFirestoreAsync(participantid, pinType);
            }
            else
            {
                return await DeletePersonPinFromFirestoreAsync(participantid, pinType);
            }
        }

        private async Task<bool> DeletePersonPinFromFirestoreAsync(int participantid, string pinType)
        {
            DeleteProfilePhotoFromFirestore(participantid);
            return await DeletePinFromFirestoreAsync(participantid, pinType);
        }

        

        private async Task<bool> AddPersonPinToFirestoreAsync(int participantid, string pinType)
        {
            var apiToken = _apiUserRepository.GetDefaultApiClientToken();
            var address = new AddressDTO();
            try
            {
                int contactid = _contactRepository.GetContactIdByParticipantId(participantid);
                MpMyContact contact = _contactRepository.GetContactById(contactid);
                // get the address including lat/lon
                if (contact.Address_ID != null)
                {
                    address = this.RandomizeLatLong(Mapper.Map<AddressDTO>(_addressRepository.GetAddressById(apiToken, (int)contact.Address_ID)));
                }
                else
                {
                    // no address for this contact/participant
                    return true;
                }

                var geohash = GeoHash.Encode(address.Latitude != null ? (double)address.Latitude : 0, address.Longitude != null ? (double)address.Longitude : 0);

                var url = SendProfilePhotoToFirestore(participantid);

                // create the pin object
                MapPin pin = new MapPin("", contact.Nickname + " " + contact.Last_Name.ToCharArray()[0], address.Latitude != null ? (double)address.Latitude : 0, address.Longitude != null ? (double)address.Longitude : 0, Convert.ToInt32(pinType), participantid.ToString(), geohash, url, null);

                FirestoreDb db = FirestoreDb.Create(_firestoreProjectId);
                CollectionReference collection = db.Collection("Pins");
                DocumentReference document = await collection.AddAsync(pin);
                Console.WriteLine(document.Id);
            }
            catch (Exception e)
            {
                Console.WriteLine("Problem getting MP Data for PinSync");
                Console.WriteLine(e.Message);
                return false;
            }
            return true;
        }

        //////////////////////////////////////////////
        /// Common
        //////////////////////////////////////////////
        private async Task<bool> DeletePinFromFirestoreAsync(int internalid, string pinType)
        {
            FirestoreDb db = FirestoreDb.Create(_firestoreProjectId);
            CollectionReference collection = db.Collection("Pins");

            // find the firestore document
            Query query = collection.WhereEqualTo("internalId", internalid.ToString());

            QuerySnapshot querySnapshot = await query.GetSnapshotAsync();

            Console.WriteLine(querySnapshot.Count.ToString());

            foreach (DocumentSnapshot queryResult in querySnapshot.Documents)
            {
                int outvalue;
                var rc1 = queryResult.ContainsField("pinType");
                var rc = queryResult.TryGetValue<int>("pinType", out outvalue);
                var pintypequeryresult = queryResult.GetValue<int>("pinType");
                if (pintypequeryresult == Convert.ToInt32(pinType))
                {
                    WriteResult result = await collection.Document(queryResult.Id).DeleteAsync();
                    // mark as processed

                    Console.WriteLine(result.ToString());
                }
            }
            return true;
        }

        private AddressDTO RandomizeLatLong(AddressDTO address)
        {
            if (!address.HasGeoCoordinates()) return address;
            var distance = _random.Next(75, 300); // up to a quarter mile
            var angle = _random.Next(0, 359);
            const int earthRadius = 6371000; // in meters

            var distanceNorth = Math.Sin(angle) * distance;
            var distanceEast = Math.Cos(angle) * distance;

            var newLat = (double)(address.Latitude + (distanceNorth / earthRadius) * 180 / Math.PI);
            var newLong = (double)(address.Longitude + (distanceEast / (earthRadius * Math.Cos(newLat * 180 / Math.PI))) * 180 / Math.PI);
            address.Latitude = newLat;
            address.Longitude = newLong;

            return address;
        }
    }
}
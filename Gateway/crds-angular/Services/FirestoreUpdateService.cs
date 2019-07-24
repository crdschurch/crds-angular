using AutoMapper;
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
using crds_angular.Models.Finder;

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
        private readonly ILookupService _lookupService;

        private readonly string _googleStorageBucketId;
        private readonly string _firestoreProjectId;

        private readonly Random _random = new Random(DateTime.Now.Millisecond);

        

        public FirestoreUpdateService(IImageService imageService, 
                                      IConfigurationWrapper configurationWrapper, 
                                      IFinderRepository finderRepository,
                                      IApiUserRepository apiUserRepository,
                                      IContactRepository contactRepository,
                                      IAddressRepository addressRepository,
                                      IGroupService groupService,
                                      IAddressGeocodingService addressGeocodingService,
                                      ICongregationRepository congregationRepository,
                                      ILocationService locationRepository,
                                      ILookupService lookupService)
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
            _lookupService = lookupService;
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
            var updateStatus = false;
            try
            {
                var recordList = _finderRepository.GetMapAuditRecords();
                
                while (recordList.Count > 0)
                {
                    foreach (MpMapAudit mapAuditRecord in recordList)
                    {
                        // if we have multiple records with the same type and id in our collection then only process it once
                        // this condition can occur when multiple db triggers add to the cr_mapAudit table
                        if( recordList.FindIndex(f => f.pinType == mapAuditRecord.pinType && f.ParticipantId == mapAuditRecord.ParticipantId && f.processed == true) != -1)
                        {
                            updateStatus = true;
                        }
                       else 
                        {
                            switch (Convert.ToInt32(mapAuditRecord.pinType))
                            {
                                case PinTypeConstants.PIN_PERSON:
                                    updateStatus = await PersonPinToFirestoreAsync(mapAuditRecord.ParticipantId, mapAuditRecord.showOnMap, mapAuditRecord.pinType);
                                    break;
                                case PinTypeConstants.PIN_GROUP:
                                    updateStatus = await GroupPinToFirestoreAsync(mapAuditRecord.ParticipantId, mapAuditRecord.showOnMap, mapAuditRecord.pinType);
                                    break;
                                case PinTypeConstants.PIN_SITE:
                                    updateStatus = await SitePinToFirestoreAsync(mapAuditRecord.ParticipantId, mapAuditRecord.showOnMap, mapAuditRecord.pinType);
                                    break;
                                case PinTypeConstants.PIN_ONLINEGROUP:
                                    updateStatus = await OnlineGroupPinToFirestoreAsync(mapAuditRecord.ParticipantId, mapAuditRecord.showOnMap, mapAuditRecord.pinType);
                                    break;
                            }  
                        }
                        SetRecordProcessedStatusFlag(mapAuditRecord, updateStatus);
                    }
                    recordList = _finderRepository.GetMapAuditRecords();
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
            }
        }

        private void SetRecordProcessedStatusFlag(MpMapAudit mapAuditRecord, bool success)
        {
            mapAuditRecord.processStatus = success ? "SUCCESS" : "FAILURE";
            mapAuditRecord.processed = true;
            mapAuditRecord.dateProcessed = DateTime.Now;
            _finderRepository.MarkMapAuditRecordAsProcessed(mapAuditRecord);
            
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
                MapPin pin = new MapPin("", congregation.Name, address.Latitude != null ? (double)address.Latitude : 0, address.Longitude != null ? (double)address.Longitude : 0, 
                    Convert.ToInt32(pinType), congregationid.ToString(), geohash, "", null, BuildStaticText1(pinType,congregationid), BuildStaticText2(pinType,congregationid), true);

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
                    return false;
                }

                var addrFromDB = _addressRepository.GetAddressById(apiToken, (int)group.Address.AddressID);
                // if we have no location we will not add to firestore.	
                if (addrFromDB.Latitude == null || addrFromDB.Longitude == null || addrFromDB.Latitude == 0 || addrFromDB.Longitude == 0)
                {
                    return false;
                }
                                
                var address = this.RandomizeLatLong(Mapper.Map<AddressDTO>(addrFromDB));
                var geohash = GeoHash.Encode(address.Latitude != null ? (double)address.Latitude : 0, address.Longitude != null ? (double)address.Longitude : 0);
                
                var participantId = _groupService.GetPrimaryContactParticipantId(groupid);
                var url = SendProfilePhotoToFirestore(participantId);
                Console.WriteLine($"Small Group image url: {url}");

                // create the pin object
                MapPin pin = new MapPin(RemoveHtmlTags(group.GroupDescription), group.GroupName, address.Latitude != null ? (double)address.Latitude : 0, address.Longitude != null ? (double)address.Longitude : 0, Convert.ToInt32(pinType), 
                    groupid.ToString(), geohash, url, BuildGroupAttributeDictionary(s, t), BuildStaticText1(pinType, groupid), BuildStaticText2(pinType, groupid), (bool)group.AvailableOnline);

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
                MapPin pin = new MapPin("", contact.Nickname + " " + contact.Last_Name.ToCharArray()[0], address.Latitude != null ? (double)address.Latitude : 0, address.Longitude != null ? (double)address.Longitude : 0, 
                    Convert.ToInt32(pinType), participantid.ToString(), geohash, url, null, BuildStaticText1(pinType, participantid), BuildStaticText2(pinType, participantid), true);

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
        /// ONLINEGROUP PIN
        ////////////////////////////////////////////////////////////////////////////////////////////////////////////////
        private async Task<bool> OnlineGroupPinToFirestoreAsync(int groupid, bool showOnMap, string pinType)
        {
            // showonmap = true then add to firestore
            // showonmap = false then delete from firestore
            Console.WriteLine($"groupid = {groupid}, showonmap = {showOnMap}, pintype = {pinType}");
            if (showOnMap)
            {
                await DeleteOnlineGroupPinFromFirestoreAsync(groupid, pinType);
                return await AddOnlineGroupPinToFirestoreAsync(groupid, pinType);
            }
            else
            {
                return await DeleteOnlineGroupPinFromFirestoreAsync(groupid, pinType);
            }
        }

        private async Task<bool> DeleteOnlineGroupPinFromFirestoreAsync(int groupid, string pinType)
        {
            return await DeletePinFromFirestoreAsync(groupid, pinType);
        }

        private async Task<bool> AddOnlineGroupPinToFirestoreAsync(int groupid, string pinType)
        {
            var apiToken = _apiUserRepository.GetDefaultApiClientToken();
            var address = new AddressDTO();
            try
            {
                //var group = _groupRepository.getGroupDetails(groupid);
                var group = _groupService.GetGroupDetailsWithAttributes(groupid);
                var s = group.SingleAttributes;
                var t = group.AttributeTypes;
                
                var participantId = _groupService.GetPrimaryContactParticipantId(groupid);
                var url = SendProfilePhotoToFirestore(participantId);
                Console.WriteLine($"Online Group image url: {url}");

                // create the pin object
                MapPin pin = new MapPin(RemoveHtmlTags(group.GroupDescription), group.GroupName, Convert.ToInt32(pinType), groupid.ToString(), url,
                                        BuildGroupAttributeDictionary(s,t), BuildStaticText1(pinType, groupid), BuildStaticText2(pinType, groupid), (bool)group.AvailableOnline);

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
        private Dictionary<string, string[]> BuildGroupAttributeDictionary(Dictionary<int,ObjectSingleAttributeDTO> s, Dictionary<int, ObjectAttributeTypeDTO> t)
        {
            var dict = new Dictionary<string, string[]>();

            try
            {
                // get grouptype
                int grouptypeCategoryAttributeID = 73;
                ObjectSingleAttributeDTO grouptype;
                if (s.TryGetValue(grouptypeCategoryAttributeID, out grouptype) && grouptype.Value != null)
                {
                    // grouptype is now equal to the value
                    var x = grouptype.Value;
                    dict.Add("GroupType", new string[] { x.Name });
                }

                // get age groups
                int agegroupAttributeID = 91;
                ObjectAttributeTypeDTO agegroup;
                if (t.TryGetValue(agegroupAttributeID, out agegroup))
                {
                    // roll through the age group. add selected to the dictionary
                    var ageGroups = new List<string>();
                    foreach (var a in agegroup.Attributes)
                    {
                        if (a.Selected)
                        {
                            ageGroups.Add(a.Name);
                        }
                    }

                    // add to the dict
                    if (ageGroups.Count > 0)
                    {
                        dict.Add("AgeGroups", ageGroups.ToArray());
                    }
                }

                // get group categories
                int groupCategoryAttributeID = 90;
                ObjectAttributeTypeDTO groupcategory;
                if (t.TryGetValue(groupCategoryAttributeID, out groupcategory))
                {
                    // roll through the group categories. add selected to the dictionary
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

                // get group subcategories
                int groupSubcategoryAttributeID = 92;
                ObjectAttributeTypeDTO groupSubcategory;
                if (t.TryGetValue(groupSubcategoryAttributeID, out groupSubcategory))
                {
                    // roll through the group categories. add selected to the dictionary
                    var categories = new List<string>();
                    foreach (var a in groupSubcategory.Attributes)
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
            }
            catch(Exception e)
            {
                Console.WriteLine("Problem processing metadata in BuildGroupAttributeDictionary");
                Console.WriteLine(e.Message);
            }

            return dict;
        }

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

        private string BuildStaticText1(string pinType, int id)
        {
            string staticText1 = "";
            switch (Convert.ToInt32(pinType))
            {
                case PinTypeConstants.PIN_PERSON:
                    staticText1 = "";
                    break;
                case PinTypeConstants.PIN_GROUP:
                    staticText1 = BuildGroupTimeString(id);
                    break;
                case PinTypeConstants.PIN_SITE:
                    staticText1 = "";
                    break;
                case PinTypeConstants.PIN_ONLINEGROUP:
                    staticText1 = BuildGroupTimeString(id);
                    break;
            }
            return staticText1;
        }

        private string BuildStaticText2(string pinType, int id)
        {
            string staticText2 = "";
            switch (Convert.ToInt32(pinType))
            {
                case PinTypeConstants.PIN_PERSON:
                    staticText2 = BuildPersonAddressString(id);
                    break;
                case PinTypeConstants.PIN_GROUP:
                    staticText2 = BuildGroupAttrString(id);
                    break;
                case PinTypeConstants.PIN_SITE:
                    staticText2 = BuildLocationAddressString(id);
                    break;
                case PinTypeConstants.PIN_ONLINEGROUP:
                    staticText2 = BuildGroupAttrString(id);
                    break;
            }
            return staticText2;
        }

        private string BuildLocationAddressString(int congregationid)
        {
            var locationString = "";
            var congregation = _congregationRepository.GetCongregationById(congregationid);
            var location = _locationRepository.GetAllCrossroadsLocations().Where(x => x.LocationId == congregation.LocationId).First();
            if(location != null)
            {
                locationString = $"{location.Address.AddressLine1}\r\n{location.Address.City}, {location.Address.State} {location.Address.PostalCode}";
            }
            return locationString;
        }

        private string BuildPersonAddressString(int participantid)
        {
            var contact = _contactRepository.GetContactByParticipantId(participantid);
            return $"{contact.City}, {contact.State} {contact.Postal_Code}";
        }

        private string BuildGroupTimeString(int groupid)
        {
            var group = _groupService.GetGroupDetailsWithAttributes(groupid);
            if(group.MeetingFrequencyID == null || group.MeetingDayId == null || group.MeetingTime == null)
            {
                return "Flexible Meeting Time";
            }
            return $"{_lookupService.GetMeetingFrequencyFromId(group.MeetingFrequencyID)} on {_lookupService.GetMeetingDayFromId(group.MeetingDayId)} @ {DateTime.Parse(group.MeetingTime).ToString("hh:mm tt")}";
        }

        private string BuildGroupAttrString(int groupid)
        {
            var group = _groupService.GetGroupDetailsWithAttributes(groupid);
            var s = group.SingleAttributes;
            var t = group.AttributeTypes;
            string attrString="";

            // get grouptype
            ObjectSingleAttributeDTO grouptype;
            if (s.TryGetValue(73, out grouptype) && grouptype.Value != null)
            {
                attrString = $"{grouptype.Value.Name} | ";
            }

            // get age groups
            ObjectAttributeTypeDTO agegroup;
            if (t.TryGetValue(91, out agegroup))
            {
                var grouptext = "Ages";
                // roll through the age group. add selected to the dictionary
                var ageGroups = new List<string>();
                foreach (var a in agegroup.Attributes)
                {
                    if (a.Selected)
                    {
                        grouptext = $"{grouptext} {a.Name},";
                    }
                }
                attrString = $"{attrString} {grouptext.TrimEnd(',')} | ";
            }

            // get group categories
            ObjectAttributeTypeDTO groupcategory;
            if (t.TryGetValue(90, out groupcategory))
            {
                var categoryText = "";
                // roll through the age group. add selected to the dictionary
                var categories = new List<string>();
                foreach (var a in groupcategory.Attributes)
                {
                    if (a.Selected)
                    {
                        categoryText = $"{categoryText} {a.Category}";
                    }
                }
                attrString = $"{attrString} {categoryText}";
            }

            return attrString;
        }

        private string RemoveHtmlTags(string input)
        {
            System.Text.RegularExpressions.Regex rx = new System.Text.RegularExpressions.Regex("<[^>]*>");
            return rx.Replace(input, "");
        }
    }
}
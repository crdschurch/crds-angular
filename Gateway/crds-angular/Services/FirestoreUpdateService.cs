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

        private readonly string _googleStorageBucketId;
        private readonly string _firestoreProjectId;

        private readonly Random _random = new Random(DateTime.Now.Millisecond);

        

        public FirestoreUpdateService(IImageService imageService, 
                                      IConfigurationWrapper configurationWrapper, 
                                      IFinderRepository finderRepository,
                                      IApiUserRepository apiUserRepository,
                                      IContactRepository contactRepository,
                                      IAddressRepository addressRepository)
        {
            // dependencies
            _imageService = imageService;
            _configurationWrapper = configurationWrapper;
            _finderRepository = finderRepository;
            _apiUserRepository = apiUserRepository;
            _contactRepository = contactRepository;
            _addressRepository = addressRepository;
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
                    var photoUpload = client.UploadObject(bucketName, $"{participantId}.png", "image/png", memStream);
                    urlForPhoto = photoUpload.MediaLink;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
            return urlForPhoto;
        }

        public void DeleteProfilePhotoFromFirestore(int participantId)
        {
            var client = StorageClient.Create();
            var bucketName = _googleStorageBucketId;

            try
            {
                client.DeleteObject(bucketName, $"{participantId}.png");
            }
            catch (Exception ex)
            {
                //likely here because file does not exist in bucket
                Console.WriteLine(ex.Message);
            }
        }

        public async Task ProcessMapAuditRecords()
        {
            try
            {
                var recordList = _finderRepository.GetMapAuditRecords();
                foreach (MpMapAudit mapAuditRecord in recordList)
                {
                    switch (Convert.ToInt32(mapAuditRecord.pinType))
                    {
                        case 1:
                            var pinupdatedsuccessfully = await PersonPinToFirestoreAsync(mapAuditRecord.ParticipantId, mapAuditRecord.showOnMap, mapAuditRecord.pinType);
                            if (pinupdatedsuccessfully)
                            {
                                mapAuditRecord.processed = true;
                                mapAuditRecord.dateProcessed = DateTime.Now;
                                _finderRepository.MarkMapAuditRecordAsProcessed(mapAuditRecord);
                            }
                            break;
                    }

                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
            }
        }


        private async Task<bool> PersonPinToFirestoreAsync(int participantid, bool showOnMap, string pinType)
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

            FirestoreDb db = FirestoreDb.Create(_firestoreProjectId);
            CollectionReference collection = db.Collection("Pins");

            // find the firestore document
            Query query = collection.WhereEqualTo("internalId", participantid.ToString());

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
                MapPin pin = new MapPin("", contact.Nickname + " " + contact.Last_Name.ToCharArray()[0], address.Latitude != null ? (double)address.Latitude : 0, address.Longitude != null ? (double)address.Longitude : 0, Convert.ToInt32(pinType), participantid.ToString(), geohash, url);

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
using System.Collections.Generic;
using System.Device.Location;
using System.IO;
using System.Linq;
using System.Text;
using crds_angular.Services.Interfaces;
using Crossroads.Web.Common.Configuration;
using Amazon.CloudSearchDomain;
using Amazon.CloudSearchDomain.Model;
using AutoMapper;
using crds_angular.Models.AwsCloudsearch;
using crds_angular.Models.Finder;
using MinistryPlatform.Translation.Models.Finder;
using MinistryPlatform.Translation.Repositories.Interfaces;
using Newtonsoft.Json;

namespace crds_angular.Services
{
    public class AwsCloudsearchService : MinistryPlatformBaseService, IAwsCloudsearchService
    {
        private readonly IFinderRepository _finderRepository;
        protected string AmazonSearchUrl;
        protected string AwsAccessKeyId;
        protected string AwsSecretAccessKey;

        public AwsCloudsearchService(IFinderRepository finderRepository,
                                     IConfigurationWrapper configurationWrapper)
        {
            _finderRepository = finderRepository;

            AmazonSearchUrl = configurationWrapper.GetEnvironmentVarAsString("CRDS_AWS_CONNECT_ENDPOINT");
            AwsAccessKeyId = configurationWrapper.GetEnvironmentVarAsString("CRDS_AWS_CONNECT_ACCESSKEYID");
            AwsSecretAccessKey = configurationWrapper.GetEnvironmentVarAsString("CRDS_AWS_CONNECT_SECRETACCESSKEY");
        }

        public UploadDocumentsResponse UploadAllConnectRecordsToAwsCloudsearch()
        {
            var pinList = GetDataForCloudsearch();
            return SendAwsDocs(pinList);
        }

        public void UploadSingleGroupToAwsFromMp(int groupId)
        {
            MpConnectAws groupFromMp = _finderRepository.GetSingleGroupRecordFromMpInAwsPinFormat(groupId);
            if (groupFromMp != null) // new unauthorized gatherings will not get sent
            {
                AwsConnectDto groupInAwsUploadFormat = Mapper.Map<AwsConnectDto>(groupFromMp);

                AwsCloudsearchDto awsDto = CreateCloudSearchUploadDto(groupInAwsUploadFormat);
                List<AwsCloudsearchDto> awsDtoList = new List<AwsCloudsearchDto>() { awsDto };

                SendAwsDocs(awsDtoList);
            }
        }

        public UploadDocumentsResponse DeleteAllConnectRecordsInAwsCloudsearch()
        {
            var results = SearchConnectAwsCloudsearch("matchall", "_no_fields");
            var deletelist = results.Hits.Hit.Select(hit => new AwsCloudsearchDto
            {
                id = hit.Id,
                type = "delete"
            }).ToList();

            return SendAwsDocs(deletelist);
        }

        private UploadDocumentsResponse SendAwsDocs(List<AwsCloudsearchDto> awsDocs)
        {
            var cloudSearch = new AmazonCloudSearchDomainClient(AwsAccessKeyId, AwsSecretAccessKey, AmazonSearchUrl);

            // serialize
            var json = JsonConvert.SerializeObject(awsDocs, new JsonSerializerSettings { NullValueHandling = NullValueHandling.Ignore });
            var ms = new MemoryStream(Encoding.UTF8.GetBytes(json));
            var upload = new UploadDocumentsRequest()
            {
                ContentType = ContentType.ApplicationJson,
                Documents = ms
            };

            return (cloudSearch.UploadDocuments(upload));
        }

        public UploadDocumentsResponse DeleteGroupFromAws(int groupId)
        {
            var results = SearchConnectAwsCloudsearch($"groupid: {groupId}", "_no_fields");
            var deletelist = results.Hits.Hit.Select(hit => new AwsCloudsearchDto
            {
                id = hit.Id,
                type = "delete"
            }).ToList();

            if (deletelist.Count == 0) return null;

            return SendAwsDocs(deletelist);
        }

        public void UpdateGroupInAws(int groupId)
        {
            var results = SearchConnectAwsCloudsearch($"groupid: {groupId}", "_no_fields");

            switch (results.Hits.Hit.Count)
            {
                case 0:
                    // not found, so lets add it to aws
                    UploadSingleGroupToAwsFromMp(groupId);
                    return;
                case 1:
                    // found the exact match, let's update
                    var idToUpdate = results.Hits.Hit.FirstOrDefault()?.Id;
                    MpConnectAws groupFromAws = _finderRepository.GetSingleGroupRecordFromMpInAwsPinFormat(groupId);
                    AwsConnectDto groupInAwsUploadFormat = Mapper.Map<AwsConnectDto>(groupFromAws);

                    AwsCloudsearchDto awsDto = CreateCloudSearchUploadDto(groupInAwsUploadFormat);
                    awsDto.id = idToUpdate;
                    List<AwsCloudsearchDto> awsDtoList = new List<AwsCloudsearchDto> { awsDto };
                    SendAwsDocs((awsDtoList));
                    return;
                default:
                    // we found multiple matches. This seems to be an issue. Let's delete all mathing groups and just add the one we want
                    DeleteGroupFromAws(groupId);
                    UploadSingleGroupToAwsFromMp(groupId);
                    return;
            }
        }

        private AwsCloudsearchDto CreateCloudSearchUploadDto(AwsConnectDto groupDto)
        {
            AwsCloudsearchDto awsDto = new AwsCloudsearchDto()
            {
                type = "add",
                id = groupDto.AddressId + "-" + groupDto.PinType + "-" + groupDto.ParticipantId + "-" + groupDto.GroupId,
                fields = groupDto
            };

            return awsDto;
        }

        private List<AwsCloudsearchDto> GetDataForCloudsearch()
        {
            var pins = _finderRepository.GetAllPinsForAws().Select(Mapper.Map<AwsConnectDto>).ToList();
            var pinlist = new List<AwsCloudsearchDto>();
            foreach (var pin in pins)
            {
                var awsRecord = new AwsCloudsearchDto
                {
                    type = "add",
                    id = pin.AddressId + "-" + pin.PinType + "-" + pin.ParticipantId + "-" + pin.GroupId,
                    fields = pin
                };
                pinlist.Add(awsRecord);
            }
            return pinlist;
        }

        public AwsBoundingBox BuildBoundingBox(MapBoundingBox mapBox)
        {
            var awsMapBoundingBox = new AwsBoundingBox
            {
                UpperLeftCoordinates = new GeoCoordinates(mapBox.UpperLeftLat, mapBox.UpperLeftLng),
                BottomRightCoordinates = new GeoCoordinates(mapBox.BottomRightLat, mapBox.BottomRightLng)
            };

            return awsMapBoundingBox;
        }

        public SearchResponse SearchConnectAwsCloudsearch(string querystring, string returnFields, int returnSize = 10000, GeoCoordinate originCoords = null, AwsBoundingBox boundingBox = null)
        {
            var cloudSearch = new AmazonCloudSearchDomainClient(AwsAccessKeyId, AwsSecretAccessKey, AmazonSearchUrl);
            var searchRequest = new SearchRequest
            {
                Query = querystring,
                QueryParser = QueryParser.Structured,
                Size = returnSize,
                Return = returnFields + ",_score"
            };

            if (boundingBox != null)
            {
                searchRequest.FilterQuery = $"latlong:['{boundingBox.UpperLeftCoordinates.Lat},{boundingBox.UpperLeftCoordinates.Lng}','{boundingBox.BottomRightCoordinates.Lat},{boundingBox.BottomRightCoordinates.Lng}']";
            }

            if (originCoords != null)
            {
                searchRequest.Expr = $"{{'distance':'haversin({originCoords.Latitude},{originCoords.Longitude},latlong.latitude,latlong.longitude)'}}"; // use to sort by proximity
                searchRequest.Sort = "distance asc";
                searchRequest.Return += ",distance";
            }

            var response = cloudSearch.Search(searchRequest);
            return (response);
        }

        public SearchResponse SearchByGroupId(string groupId)
        {
            var cloudSearch = new AmazonCloudSearchDomainClient(AwsAccessKeyId, AwsSecretAccessKey, AmazonSearchUrl);
            var searchRequest = new SearchRequest
            {
                Query = "groupid: " + groupId,
                QueryParser = QueryParser.Structured,
                QueryOptions = "{'fields': ['groupid']}",
                Size = 1,
                Return = "_all_fields"
            };

            var response = cloudSearch.Search(searchRequest);
            return (response);
        }

        public void UploadNewPinToAws(PinDto pin)
        {
            var cloudSearch = new AmazonCloudSearchDomainClient(AwsAccessKeyId, AwsSecretAccessKey, AmazonSearchUrl);
            var upload = GetObjectToUploadToAws(pin);
            cloudSearch.UploadDocuments(upload);
        }

        private string GenerateAwsPinId(PinDto pin)
        {
            if (pin.PinType == PinType.GATHERING)
            {
                return GenerateAwsPinString(pin, pin.Gathering.Address.AddressID.ToString(), pin.Gathering.GroupId.ToString());
            }
            else
            {
                return GenerateAwsPinString(pin, pin.Address.AddressID.ToString());
            }
        }

        private string GenerateAwsPinString(PinDto pin, string addressId, string groupId = "")
        {
            return addressId + "-" + (int)pin.PinType + '-' + pin.Participant_ID + "-" + groupId;
        }

        public UploadDocumentsRequest GetObjectToUploadToAws(PinDto pin)
        {
            AwsConnectDto awsPinObject = Mapper.Map<AwsConnectDto>(pin);

            if (pin.PinType == PinType.GATHERING)
            {
                awsPinObject.AddressId = pin.Gathering.Address.AddressID;
                awsPinObject.Latitude = pin.Gathering.Address.Latitude;
                awsPinObject.Longitude = pin.Gathering.Address.Longitude;
                awsPinObject.City = pin.Gathering.Address.City;
                awsPinObject.LatLong = (pin.Gathering.Address.Latitude == null || pin.Gathering.Address.Longitude == null)
                    ? "0 , 0" : $"{pin.Gathering.Address.Latitude} , {pin.Gathering.Address.Longitude}";
                awsPinObject.State = pin.Gathering.Address.State;
                awsPinObject.Zip = pin.Gathering.Address.PostalCode;
                awsPinObject.GroupStartDate = pin.Gathering.StartDate;
                awsPinObject.GroupId = pin.Gathering.GroupId;
                awsPinObject.GroupTypeId = pin.Gathering.GroupTypeId;
                awsPinObject.GroupDescription = pin.Gathering.GroupDescription;
                awsPinObject.GroupName = pin.Gathering.GroupName;
                awsPinObject.GroupAvailableOnline = (pin.Gathering.AvailableOnline.HasValue && pin.Gathering.AvailableOnline.Value == true) ? 1 : 0;
            }

            AwsCloudsearchDto awsPostPinObject = new AwsCloudsearchDto("add", GenerateAwsPinId(pin), awsPinObject);

            var pinlist = new List<AwsCloudsearchDto> { awsPostPinObject };

            string jsonAwsObject = JsonConvert.SerializeObject(pinlist, new JsonSerializerSettings { NullValueHandling = NullValueHandling.Ignore });

            MemoryStream jsonAwsPinDtoStream = new MemoryStream(Encoding.UTF8.GetBytes(jsonAwsObject));

            UploadDocumentsRequest upload = new UploadDocumentsRequest()
            {
                ContentType = ContentType.ApplicationJson,
                Documents = jsonAwsPinDtoStream
            };

            return upload;
        }

    }
}
using System.Device.Location;
using Amazon.CloudSearchDomain.Model;
using crds_angular.Models.AwsCloudsearch;
using crds_angular.Models.Finder;

namespace crds_angular.Services.Interfaces
{
    public interface IAwsCloudsearchService
    {
        void UploadNewPinToAws(PinDto pin);
        void UploadSingleGroupToAwsFromMp(int groupId);
        UploadDocumentsResponse DeleteGroupFromAws(int groupId);
        UploadDocumentsResponse UploadAllConnectRecordsToAwsCloudsearch();
        UploadDocumentsResponse DeleteAllConnectRecordsInAwsCloudsearch();
        SearchResponse SearchConnectAwsCloudsearch(string querystring, string returnFields, int returnSize = 10000, GeoCoordinate originCoords = null, AwsBoundingBox boundingBox = null);
        SearchResponse SearchByGroupId(string groupId);
        AwsBoundingBox BuildBoundingBox(MapBoundingBox mapBoundingBox);
        void UpdateGroupInAws(int groupId);
    }
}
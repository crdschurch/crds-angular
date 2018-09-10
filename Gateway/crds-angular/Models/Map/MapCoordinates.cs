using Google.Cloud.Firestore;

namespace crds_angular.Models.Map
{
    [FirestoreData]
    public class MapCoordinates
    {
        [FirestoreProperty]
        public string geohash { get; set; }
        [FirestoreProperty]
        public GeoPoint geopoint { get; set; }
    }
}
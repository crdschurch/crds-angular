using Google.Cloud.Firestore;

namespace crds_angular.Models.Map
{
    [FirestoreData]
    public class MapLocation
    {
        [FirestoreProperty]
        public MapAddress address { get; set; }

        [FirestoreProperty]
        public MapCoordinates point { get; set; }
    }
}
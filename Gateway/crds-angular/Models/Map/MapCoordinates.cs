using Google.Cloud.Firestore;

namespace crds_angular.Models.Map
{
    [FirestoreData]
    public class MapCoordinates
    {
        [FirestoreProperty]
        public double latitude { get; set; }

        [FirestoreProperty]
        public double longitude { get; set; }
    }
}
using Google.Cloud.Firestore;

namespace crds_angular.Models.Map
{
    [FirestoreData]
    public class MapAddress
    {
        [FirestoreProperty]
        public string addressLine1 { get; set; }

        [FirestoreProperty]
        public string addressLine2 { get; set; }

        [FirestoreProperty]
        public string city { get; set; }

        [FirestoreProperty]
        public string state { get; set; }

        [FirestoreProperty]
        public string zip { get; set; }
    }
}
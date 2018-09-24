using Google.Cloud.Firestore;

namespace crds_angular.Models.Map
{
    [FirestoreData]
    public class MapPin
    {
        [FirestoreProperty]
        public string description { get; set; }

        [FirestoreProperty]
        public string name { get; set; }

        [FirestoreProperty]
        public int pinType { get; set; }

        [FirestoreProperty]
        public string internalId { get; set; }

        [FirestoreProperty]
        public MapCoordinates point { get; set; }

        public MapPin() { }

        public MapPin(string desc, string pinname, double latitude, double longitude, int pintype, string internalid, string geohash)
        {
            this.description = desc;
            this.name = pinname;
            this.pinType = pintype;
            this.internalId = internalid;

            var coord = new MapCoordinates
            {
                geopoint = new GeoPoint(latitude, longitude),
                geohash = geohash
            };
           
            this.point = coord;
        }
    }
}
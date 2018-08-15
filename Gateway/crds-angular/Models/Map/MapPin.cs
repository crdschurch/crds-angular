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
        public MapLocation location { get; set; }

        [FirestoreProperty]
        public string pinType { get; set; }

        [FirestoreProperty]
        public string internalId { get; set; }

        public MapPin() { }

        public MapPin(string desc, string pinname, string address1, string address2, string city, string state, string zip, double latitude, double longitude, string pintype, string internalid)
        {
            this.description = desc;
            this.name = pinname;
            this.pinType = pintype;
            this.internalId = internalid;

            var coord = new MapCoordinates
            {
                latitude = latitude,
                longitude = longitude
            };

            var address = new MapAddress
            {
                addressLine1 = address1,
                addressLine2 = address2,
                city = city,
                state = state,
                zip = zip
            };

            location = new MapLocation
            {
                address = address,
                coordinates = coord
            };
        }
    }
}
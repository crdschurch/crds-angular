using Google.Cloud.Firestore;
using System.Collections.Generic;

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

        [FirestoreProperty]
        public string imageUrl { get; set; }

        [FirestoreProperty]
        public Dictionary<string, string[]> meta { get; set; }

        public MapPin() { }

        // map pins that should have an address
        public MapPin(string desc, string pinname, double latitude, double longitude, int pintype, string internalid, string geohash, string imageurl, Dictionary<string, string[]> filtermetadata)
        {
            this.description = desc;
            this.name = pinname;
            this.pinType = pintype;
            this.internalId = internalid;
            this.imageUrl = imageurl;
            this.meta = filtermetadata;

            var coord = new MapCoordinates
            {
                geopoint = new GeoPoint(latitude, longitude),
                geohash = geohash
            };

            if (latitude == 0 || longitude == 0)
            {
                this.point = null;
            }
            else
            {
                this.point = coord;
            }
        }

        //map pins with no address
        public MapPin(string desc, string pinname, int pintype, string internalid, string imageurl, Dictionary<string, string[]> filtermetadata)
        {
            this.description = desc;
            this.name = pinname;
            this.pinType = pintype;
            this.internalId = internalid;
            this.imageUrl = imageurl;
            this.meta = filtermetadata;
        }
    }
}
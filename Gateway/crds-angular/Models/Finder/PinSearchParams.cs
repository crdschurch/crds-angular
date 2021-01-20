namespace crds_angular.Models.Finder
{
    public class PinSearchQueryParams
    {
        public string UserLocationSearchString { get; set; }

        public string UserKeywordSearchString { get; set; }

        public string UserFilterString { get; set; }

        public bool IsAddressSearch { get; set; }

        public bool IsMyStuff { get; set;  }

        public string FinderType { get; set; }

        public int ContactId { get; set; }

        public GeoCoordinates CenterGeoCoords { get; set; }

        public MapBoundingBox BoundingBox { get; set; }
    }
}

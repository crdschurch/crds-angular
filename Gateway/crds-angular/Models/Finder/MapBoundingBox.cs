namespace crds_angular.Models.Finder
{
    public class MapBoundingBox
    {
        public double? UpperLeftLat { get; set; }
        public double? UpperLeftLng { get; set; }
        public double? BottomRightLat { get; set; }
        public double? BottomRightLng { get; set; }

        public MapBoundingBox(double? upperLeftLat, double? upperLeftLng, double? bottomRightLat, double? bottomRightLng)
        {
            this.UpperLeftLat = upperLeftLat;
            this.UpperLeftLng = upperLeftLng;
            this.BottomRightLat = bottomRightLat;
            this.BottomRightLng = bottomRightLng;
        }
    }
}
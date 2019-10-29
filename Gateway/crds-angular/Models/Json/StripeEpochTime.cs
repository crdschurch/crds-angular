using System;


namespace crds_angular.Models.Json
{
    public static class StripeEpochTime
    {
        private static DateTime _epochStartDateTime = new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc);

        public static DateTime ConvertEpochToDateTime(long seconds)
        {
            var updatedTime = _epochStartDateTime.AddSeconds(seconds);
            return updatedTime;

            // this was setting it back by four hours
            //return TimeZoneInfo.ConvertTimeFromUtc(updatedTime, TimeZoneInfo.FindSystemTimeZoneById("Eastern Standard Time"));
        }

        public static long ConvertDateTimeToEpoch(this DateTime datetime)
        {
            if (datetime < _epochStartDateTime) return 0;

            return Convert.ToInt64(datetime.Subtract(_epochStartDateTime).TotalSeconds);
        }
    }
}
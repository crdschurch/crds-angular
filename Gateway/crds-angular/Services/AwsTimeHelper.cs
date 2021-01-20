using System;

namespace crds_angular.Services
{
    public class AwsTimeHelper
    {
        /*
		 * Convert 12-hour time to UTC DateTime string for AWS
		 * E.g. input: "5:12AM", output "0001-01-01T05:12:00.000Z"
		 * The method only sets hours and minutes, everything else is default
         * It is NOT possible to just set a time field in AWS, so we work around that
		 */
        public string ConvertTimeToUtcString(string time)
        {
            DateTime date = new DateTime();
            if (time == null) return null;
            string hourString = null;
            string minuteString = null;

            time = AddLeadingZeroIfMissing(time);

            bool isAmTime = isAM(time);

            hourString = isAmTime ? getAmTimeHours(time) : GetPmTimeHours(time);
            hourString = Convert24To00(hourString); 
            minuteString = time.Substring(3, 2);

            string dateAsUtcString;

            if (IsTimeStringAlreadyInMilitaryTime(time))
            {
                dateAsUtcString = BuildAwsFriendlyDateTimeFromMilitaryTime(time);
            }
            else
            {
                dateAsUtcString = date.ToUniversalTime().ToString("yyyy'-'MM'-'dd'T'" + hourString + "':'" + minuteString + "':'ss'.'fff'Z'");
            }

            return dateAsUtcString;
        }

        public string BuildAwsFriendlyDateTimeFromMilitaryTime(string time)
        {
            DateTime date = new DateTime();
            string[] timeComponents = time.Split(':');
            string hours = timeComponents[0];
            string minutes = timeComponents[1];
            string dateAsUtcString = date.ToUniversalTime().ToString("yyyy'-'MM'-'dd'T'" + hours + "':'" + minutes + "':'ss'.'fff'Z'");
            return dateAsUtcString;
        }

        public bool IsTimeStringAlreadyInMilitaryTime(string time)
        {
            bool doesContainPmOrAmFlag = time.Contains("AM") || time.Contains("PM");
            return !doesContainPmOrAmFlag;
        }

        //The UTC standard specifies a leading zero, e.g. "05:55PM" instead of "5:55PM" - add it if missing
        public string AddLeadingZeroIfMissing(string time)
        {
            if (time.ToCharArray().Length <= 6)
            {
                time = "0" + time;
            }

            return time;
        }

        //Check if the time has an "AM" indicator 
        private bool isAM(string time)
        {
            string lastTwoChars = time.Substring(time.Length - 2, 2);
            bool isAM = lastTwoChars == "AM";
            return isAM;
        }

        //Add 12 hours to PM time hours to get accurate military time
        public string GetPmTimeHours(string time)
        {
            string hourStringBeforeConversion = time.Substring(0, 2);
            string hourStringInMilitaryTime = hourStringBeforeConversion;
            if (hourStringBeforeConversion != "12")
            {
                hourStringInMilitaryTime = (Int32.Parse(hourStringBeforeConversion) + 12).ToString();
            }
            return hourStringInMilitaryTime;
        }

        // add 12 hours to 12AM to get 00:00 hours
        public string getAmTimeHours(string time)
        {
            string hourStringBeforeConversion = time.Substring(0, 2);
            string hourStringInMilitaryTime = hourStringBeforeConversion;
            if (hourStringBeforeConversion == "12")
            {
                hourStringInMilitaryTime = (Int32.Parse(hourStringBeforeConversion) + 12).ToString();
            }
            return hourStringInMilitaryTime;
        }

        //Convert hour value of "24" to "00" - 24 is not an acceptable hour figure, clock resets after 23:59
        public string Convert24To00(string timeHourPortion)
        {
            string midnightHourMilitaryTime = "00";
            string hourStringBeforeConversion = timeHourPortion;
            string adjustedHour = (Int32.Parse(hourStringBeforeConversion)) == 24 ? midnightHourMilitaryTime : hourStringBeforeConversion;
            return adjustedHour;
        }

    }
}
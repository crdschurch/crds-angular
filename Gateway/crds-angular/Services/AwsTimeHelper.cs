﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace crds_angular.Services
{
    public class AwsTimeHelper
    {
        /*
		 * Convert 12-hour time to UTC DateTime string for AWS
		 * E.g. input: "5:12AM", output "0001-01-01T05:12:00.000Z"
		 * The method only sets hours and minutes, everything else is default
		 */
        public string ConvertTimeToUtcString(string time)
        {
            DateTime date = new DateTime();
            if (time == null) return null;
            string hourString = null;
            string minuteString = null;

            time = AddLeadingZeroIfMissing(time);

            bool isAmTime = isAM(time);

            hourString = isAmTime ? time.Substring(0, 2) : GetPmTimeHours(time);
            minuteString = time.Substring(3, 2);

            string dateAsUtcString = date.ToUniversalTime().ToString("yyyy'-'MM'-'dd'T'" + hourString + "':'" + minuteString + "':'ss'.'fff'Z'");

            return dateAsUtcString;
        }

        //The UTC standard specifies a leading zero, e.g. "05:55PM" instead of "5:55PM" - add it if missing
        private string AddLeadingZeroIfMissing(string time)
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
        private string GetPmTimeHours(string time)
        {
            string hourStringBeforeConversion = time.Substring(0, 2);
            string hourStringInMilitaryTime = (Int32.Parse(hourStringBeforeConversion) + 12).ToString();
            return hourStringInMilitaryTime;
        }
    }
}